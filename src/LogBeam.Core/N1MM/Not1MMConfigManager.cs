// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace LogBeam.Core.N1MM;

/// <summary>
/// Detecção e configuração automática do envio de contactos do Not1MM (programa de concursos
/// nativo do Linux). O Not1MM envia os QSOs no formato do N1MM+ ("contactinfo", com app=NOT1MM),
/// por isso chegam ao mesmo receptor e à mesma porta do N1MM+.
/// Preferências em $XDG_CONFIG_HOME/not1mm/not1mm.json (por omissão ~/.config/not1mm/not1mm.json),
/// criadas pelo Not1MM no primeiro arranque. Só mudam três chaves: send_n1mm_packets e
/// send_n1mm_contact (true) e n1mm_contactport ("127.0.0.1:porta"); as restantes ficam como estavam.
/// </summary>
public class Not1MMConfigManager
{
    public const string FileName = "not1mm.json";

    private const string KeyPackets = "send_n1mm_packets";
    private const string KeyContact = "send_n1mm_contact";
    private const string KeyPort    = "n1mm_contactport";

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder       = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ILogger<Not1MMConfigManager> _logger;

    public Not1MMConfigManager(ILogger<Not1MMConfigManager> logger, string? configPath = null)
    {
        _logger    = logger;
        ConfigPath = configPath ?? DefaultPath();
    }

    /// <summary>Ficheiro de preferências do Not1MM.</summary>
    public string ConfigPath { get; }

    public static string DefaultPath()
    {
        var config = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrWhiteSpace(config))
            config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        return Path.Combine(config, "not1mm", FileName);
    }

    /// <summary>
    /// Estado do envio: configurado quando os pacotes e os contactos estão ligados e o destino é este
    /// computador (127.0.0.1 ou localhost) na porta esperada. Sem ficheiro (o Not1MM nunca foi aberto)
    /// ou ilegível: <see cref="N1MMBroadcastStatus.IniNotFound"/>.
    /// </summary>
    public N1MMBroadcastStatus ReadStatus(int expectedPort = 12060)
    {
        if (!File.Exists(ConfigPath))
        {
            _logger.LogInformation("Not1MM: preferências não encontradas em {Path}", ConfigPath);
            return N1MMBroadcastStatus.IniNotFound;
        }

        try
        {
            if (JsonNode.Parse(File.ReadAllText(ConfigPath)) is not JsonObject root)
                return N1MMBroadcastStatus.IniNotFound;

            if (!IsTrue(root[KeyPackets]) || !IsTrue(root[KeyContact]))
                return N1MMBroadcastStatus.NotConfigured;

            return N1MMConfigManager.IsLocalTarget(Text(root[KeyPort]), expectedPort)
                ? N1MMBroadcastStatus.ConfiguredCorrectly
                : N1MMBroadcastStatus.ConfiguredDifferentPort;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Not1MM: falha ao ler {Path}", ConfigPath);
            return N1MMBroadcastStatus.IniNotFound;
        }
    }

    /// <summary>
    /// Liga o envio dos contactos para 127.0.0.1:porta. Faz cópia de segurança antes de alterar;
    /// se já estiver configurado, não toca no ficheiro.
    /// </summary>
    public N1MMApplyResult ApplyConfiguration(int port = 12060)
    {
        if (!File.Exists(ConfigPath))
        {
            _logger.LogWarning("Not1MM: preferências não encontradas em {Path}", ConfigPath);
            return N1MMApplyResult.Failed;
        }

        if (ReadStatus(port) == N1MMBroadcastStatus.ConfiguredCorrectly)
        {
            _logger.LogInformation("Not1MM: envio de contactos já configurado para a porta {Port}; ficheiro não alterado.", port);
            return N1MMApplyResult.AlreadyConfigured;
        }

        var backupPath = $"{ConfigPath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
        try
        {
            if (JsonNode.Parse(File.ReadAllText(ConfigPath)) is not JsonObject root)
            {
                _logger.LogWarning("Not1MM: {Path} não tem um objecto JSON", ConfigPath);
                return N1MMApplyResult.Failed;
            }

            File.Copy(ConfigPath, backupPath, overwrite: false);
            _logger.LogInformation("Not1MM: cópia de segurança criada em {Backup}", backupPath);

            root[KeyPackets] = true;
            root[KeyContact] = true;
            root[KeyPort]    = $"127.0.0.1:{port}";
            File.WriteAllText(ConfigPath, root.ToJsonString(WriteOptions));

            _logger.LogInformation("Not1MM: envio de contactos configurado para 127.0.0.1:{Port}", port);
            return N1MMApplyResult.Applied;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Not1MM: falha ao configurar {Path}", ConfigPath);

            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, ConfigPath, overwrite: true);
                _logger.LogInformation("Not1MM: ficheiro reposto a partir de {Backup}", backupPath);
            }
            return N1MMApplyResult.Failed;
        }
    }

    /// <summary>
    /// O Not1MM está aberto? Procura um processo cuja linha de comando tenha um argumento chamado
    /// "not1mm" (o comando do pipx ou o do sistema). Só no Linux.
    /// </summary>
    public bool IsNot1MMRunning() =>
        !OperatingSystem.IsWindows() && Directory.EnumerateDirectories("/proc").Any(IsNot1MMProcess);

    private static bool IsNot1MMProcess(string procDir)
    {
        try
        {
            return File.ReadAllText(Path.Combine(procDir, "cmdline"))
                .Split('\0', StringSplitOptions.RemoveEmptyEntries)
                .Any(arg => Path.GetFileName(arg) == "not1mm");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
    }

    private static bool IsTrue(JsonNode? node) =>
        node is JsonValue v && v.TryGetValue<bool>(out var b) && b;

    private static string Text(JsonNode? node) =>
        node is JsonValue v && v.TryGetValue<string>(out var s) ? s : string.Empty;
}
