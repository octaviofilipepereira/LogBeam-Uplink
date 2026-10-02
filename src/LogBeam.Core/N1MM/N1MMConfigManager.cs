// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text;
using Microsoft.Extensions.Logging;

namespace LogBeam.Core.N1MM;

public enum N1MMBroadcastStatus
{
    IniNotFound,
    NotConfigured,
    ConfiguredCorrectly,
    ConfiguredDifferentPort
}

public enum N1MMApplyResult
{
    AlreadyConfigured,
    Applied,
    Failed
}

public record BackupInfo(string Path, DateTime CreatedAt);

/// <summary>
/// Detecção e configuração automática do envio de contactos do N1MM+.
/// Ficheiro "N1MM Logger.ini", secção [ExternalBroadcast]: IsBroadcastContact (True/False) e
/// BroadcastContactAddr (um ou mais destinos IP:porta, separados por espaços).
/// O ficheiro é alterado linha a linha: só mudam as linhas dessas duas chaves e o resto fica
/// igual byte a byte (lido e escrito em Latin-1, que preserva qualquer byte). Os destinos que
/// já existam (ex. outro programa a receber do N1MM+) são mantidos.
/// </summary>
public class N1MMConfigManager
{
    public const string IniFileName = "N1MM Logger.ini";

    private const string Section    = "ExternalBroadcast";
    private const string KeyEnabled = "IsBroadcastContact";
    private const string KeyAddr    = "BroadcastContactAddr";

    private static readonly Encoding FileEncoding = Encoding.Latin1;

    private readonly ILogger<N1MMConfigManager> _logger;

    public N1MMConfigManager(ILogger<N1MMConfigManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Auto-detects the N1MM+ config folder from the user's Documents directory.
    /// </summary>
    public string? FindN1MMFolder()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var candidates = new[]
        {
            Path.Combine(docs, "N1MM Logger+"),
            Path.Combine(docs, "N1MM Logger"),
        };
        return candidates.FirstOrDefault(Directory.Exists);
    }

    public string GetIniPath(string n1mmFolder) => Path.Combine(n1mmFolder, IniFileName);

    /// <summary>
    /// Estado do envio de contactos: configurado quando IsBroadcastContact=True e um dos destinos
    /// é este computador (127.0.0.1 ou localhost) na porta esperada.
    /// </summary>
    public N1MMBroadcastStatus ReadStatus(string n1mmFolder, int expectedPort = 12060)
    {
        var iniPath = GetIniPath(n1mmFolder);
        if (!File.Exists(iniPath))
        {
            _logger.LogWarning("N1MM+: ficheiro de configuração não encontrado em {Path}", iniPath);
            return N1MMBroadcastStatus.IniNotFound;
        }

        try
        {
            var lines   = ReadLines(iniPath, out _);
            var enabled = GetValue(lines, KeyEnabled);
            var targets = SplitTargets(GetValue(lines, KeyAddr));

            if (!string.Equals(enabled?.Trim(), "True", StringComparison.OrdinalIgnoreCase) || targets.Count == 0)
                return N1MMBroadcastStatus.NotConfigured;

            return targets.Any(t => IsLocalTarget(t, expectedPort))
                ? N1MMBroadcastStatus.ConfiguredCorrectly
                : N1MMBroadcastStatus.ConfiguredDifferentPort;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "N1MM+: falha ao ler {Path}", iniPath);
            return N1MMBroadcastStatus.IniNotFound;
        }
    }

    /// <summary>
    /// Activa o envio de contactos e acrescenta ip:porta aos destinos. Faz cópia de segurança
    /// antes de alterar; se já estiver configurado, não toca no ficheiro.
    /// </summary>
    public N1MMApplyResult ApplyConfiguration(string n1mmFolder, string ip = "127.0.0.1", int port = 12060)
    {
        var iniPath = GetIniPath(n1mmFolder);
        if (!File.Exists(iniPath))
        {
            _logger.LogWarning("N1MM+: ficheiro de configuração não encontrado em {Path}", iniPath);
            return N1MMApplyResult.Failed;
        }

        if (ReadStatus(n1mmFolder, port) == N1MMBroadcastStatus.ConfiguredCorrectly)
        {
            _logger.LogInformation("N1MM+: envio de contactos já configurado para a porta {Port}; ficheiro não alterado.", port);
            return N1MMApplyResult.AlreadyConfigured;
        }

        var backupPath = $"{iniPath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
        try
        {
            File.Copy(iniPath, backupPath, overwrite: false);
            _logger.LogInformation("N1MM+: cópia de segurança criada em {Backup}", backupPath);

            var lines   = ReadLines(iniPath, out var newline);
            var target  = $"{ip}:{port}";
            var targets = SplitTargets(GetValue(lines, KeyAddr));
            if (!targets.Contains(target, StringComparer.OrdinalIgnoreCase))
                targets.Add(target);

            SetValue(lines, KeyEnabled, "True");
            SetValue(lines, KeyAddr, string.Join(" ", targets));
            File.WriteAllText(iniPath, string.Join(newline, lines), FileEncoding);

            _logger.LogInformation("N1MM+: envio de contactos configurado para {Targets}", string.Join(" ", targets));
            return N1MMApplyResult.Applied;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "N1MM+: falha ao configurar {Path}", iniPath);

            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, iniPath, overwrite: true);
                _logger.LogInformation("N1MM+: ficheiro reposto a partir de {Backup}", backupPath);
            }
            return N1MMApplyResult.Failed;
        }
    }

    /// <summary>
    /// Restores a previously created backup.
    /// </summary>
    public bool RestoreBackup(string backupPath, string n1mmFolder)
    {
        var iniPath = GetIniPath(n1mmFolder);
        try
        {
            File.Copy(backupPath, iniPath, overwrite: true);
            _logger.LogInformation("Restored N1MM ini from {Backup}", backupPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup {Backup}", backupPath);
            return false;
        }
    }

    /// <summary>
    /// Lists all available .ini backups sorted by most recent first.
    /// </summary>
    public IEnumerable<BackupInfo> GetAvailableBackups(string n1mmFolder) =>
        Directory
            .GetFiles(n1mmFolder, IniFileName + ".backup*")
            .Select(f => new BackupInfo(f, File.GetCreationTime(f)))
            .OrderByDescending(b => b.CreatedAt);

    /// <summary>
    /// Checks if the N1MM+ process is currently running (executável N1MMLogger.net.exe).
    /// </summary>
    public bool IsN1MMRunning() =>
        System.Diagnostics.Process
            .GetProcessesByName("N1MMLogger.net")
            .Concat(System.Diagnostics.Process.GetProcessesByName("N1MMLogger"))
            .Any();

    // ─── Leitura e escrita linha a linha ────────────────────────────────────

    private static List<string> ReadLines(string path, out string newline)
    {
        var text = File.ReadAllText(path, FileEncoding);
        newline  = text.Contains("\r\n") ? "\r\n" : "\n";
        return text.Split(newline).ToList();
    }

    /// <summary>Índices [início, fim) das linhas da secção, sem o cabeçalho; null se não existir.</summary>
    private static (int Start, int End)? FindSection(List<string> lines)
    {
        var header = lines.FindIndex(l => l.Trim().Equals($"[{Section}]", StringComparison.OrdinalIgnoreCase));
        if (header < 0) return null;

        var end = lines.FindIndex(header + 1, l => l.TrimStart().StartsWith('['));
        return (header + 1, end < 0 ? lines.Count : end);
    }

    private static int FindKey(List<string> lines, (int Start, int End) section, string key)
    {
        for (var i = section.Start; i < section.End; i++)
        {
            var eq = lines[i].IndexOf('=');
            if (eq > 0 && lines[i][..eq].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static string? GetValue(List<string> lines, string key)
    {
        var section = FindSection(lines);
        if (section is null) return null;

        var i = FindKey(lines, section.Value, key);
        return i < 0 ? null : lines[i][(lines[i].IndexOf('=') + 1)..];
    }

    private static void SetValue(List<string> lines, string key, string value)
    {
        var section = FindSection(lines);
        if (section is null)
        {
            // Secção inexistente: acrescentada no fim, antes da linha vazia final (se houver).
            var at = lines.Count > 0 && lines[^1].Length == 0 ? lines.Count - 1 : lines.Count;
            lines.InsertRange(at, new[] { $"[{Section}]", $"{key}={value}" });
            return;
        }

        var i = FindKey(lines, section.Value, key);
        if (i >= 0)
        {
            lines[i] = $"{lines[i][..lines[i].IndexOf('=')]}={value}";
            return;
        }

        // Chave inexistente: inserida depois da última linha não vazia da secção.
        var insertAt = section.Value.End;
        while (insertAt > section.Value.Start && lines[insertAt - 1].Trim().Length == 0) insertAt--;
        lines.Insert(insertAt, $"{key}={value}");
    }

    private static List<string> SplitTargets(string? value) =>
        (value ?? string.Empty)
            .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    /// <summary>Destino que chega a um receptor neste computador: 127.0.0.1 ou localhost, na porta indicada.</summary>
    private static bool IsLocalTarget(string target, int port)
    {
        var colon = target.LastIndexOf(':');
        if (colon <= 0 || !int.TryParse(target[(colon + 1)..], out var p) || p != port) return false;

        var host = target[..colon];
        return host == "127.0.0.1" || host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
    }
}
