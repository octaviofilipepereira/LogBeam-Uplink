// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json;
using System.Text.Json.Serialization;
using LogBeam.Core.Security;
using LogBeam.Service.Models;
using Microsoft.Extensions.Logging;

namespace LogBeam.Service.Settings;

/// <summary>
/// Gerencia carregamento e armazenamento de configurações da aplicação
/// </summary>
public class SettingsManager
{
    private readonly string _configDir;
    private readonly string _configPath;
    private readonly ILogger<SettingsManager>? _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SettingsManager(ILogger<SettingsManager>? logger = null)
    {
        _logger = logger;

        _configDir  = AppContext.BaseDirectory;
        _configPath = Path.Combine(_configDir, "settings.json");
    }

    /// <summary>
    /// Carrega as configurações do ficheiro JSON
    /// Se o ficheiro não existir, retorna configurações com defaults
    /// </summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                _logger?.LogInformation("Arquivo de configuração não encontrado em {Path}. Usando defaults.", _configPath);
                return new AppSettings();
            }

            var json = File.ReadAllText(_configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();

            // Migração de settings.json antigos: a secção "qrz" passou a "hamQth"
            if (string.IsNullOrEmpty(settings.HamQth.Username))
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("qrz", out var oldQrz))
                    {
                        if (oldQrz.TryGetProperty("username", out var u))
                            settings.HamQth.Username = u.GetString() ?? string.Empty;
                        if (oldQrz.TryGetProperty("passwordEncrypted", out var p))
                            settings.HamQth.PasswordEncrypted = p.GetString() ?? string.Empty;
                        if (oldQrz.TryGetProperty("cacheTtlMinutes", out var c))
                            settings.HamQth.CacheTtlMinutes = c.GetInt32();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Falha ao migrar secção 'qrz' antiga do settings.json");
                }
            }

            // Migração de settings.json antigos: InstanceId/ApiKey soltos em "api" passaram a "api.profiles"
            if (settings.Api.Profiles.Count == 0)
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("api", out var oldApi) &&
                        oldApi.TryGetProperty("instanceId", out var iid) &&
                        oldApi.TryGetProperty("apiKey", out var key) &&
                        !string.IsNullOrEmpty(iid.GetString()))
                    {
                        settings.Api.Profiles.Add(new ApiProfile
                        {
                            Name       = "Principal",
                            InstanceId = iid.GetString() ?? string.Empty,
                            ApiKey     = key.GetString() ?? string.Empty,
                            Enabled    = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Falha ao migrar InstanceId/ApiKey antigos do settings.json");
                }
            }

            // Decifrar campos encrypted
            if (!string.IsNullOrEmpty(settings.HamQth.PasswordEncrypted))
            {
                try
                {
                    settings.HamQth.PasswordEncrypted = CredentialProtector.Unprotect(settings.HamQth.PasswordEncrypted);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Falha ao decifrar password HamQTH. Será solicitado no wizard.");
                    settings.HamQth.PasswordEncrypted = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(settings.ClubLog.PasswordEncrypted))
            {
                try   { settings.ClubLog.PasswordEncrypted = CredentialProtector.Unprotect(settings.ClubLog.PasswordEncrypted); }
                catch { settings.ClubLog.PasswordEncrypted = string.Empty; }
            }

            // Decifrar API Keys dos perfis. Ao contrário das passwords acima,
            // NÃO se limpa o valor se a decifra falhar/vier vazia — API Keys de
            // instalações anteriores a esta versão estão em texto simples, e
            // apagá-las quebraria o envio de QSOs até o utilizador as reintroduzir.
            // Assume-se texto simples nesse caso e a próxima Save() cifra-as.
            foreach (var profile in settings.Api.Profiles)
            {
                if (string.IsNullOrEmpty(profile.ApiKey)) continue;
                try
                {
                    var decrypted = CredentialProtector.Unprotect(profile.ApiKey);
                    if (!string.IsNullOrEmpty(decrypted)) profile.ApiKey = decrypted;
                }
                catch { /* mantém o valor original — provavelmente já em texto simples */ }
            }

            _logger?.LogInformation("Configurações carregadas de {Path}", _configPath);
            return settings;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao carregar configurações de {Path}", _configPath);
            return new AppSettings();
        }
    }

    /// <summary>
    /// Guarda as configurações no ficheiro JSON
    /// Cifra automaticamente campos sensíveis antes de guardar
    /// </summary>
    public void Save(AppSettings settings)
    {
        try
        {
            // Criar directório se não existir
            Directory.CreateDirectory(_configDir);

            // Fazer cópia para cifrar
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            var settingsCopy = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)!;

            // Cifrar campos sensíveis
            if (!string.IsNullOrEmpty(settingsCopy.HamQth.PasswordEncrypted))
            {
                try
                {
                    settingsCopy.HamQth.PasswordEncrypted = CredentialProtector.Protect(settingsCopy.HamQth.PasswordEncrypted);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Falha ao cifrar password HamQTH. Será guardado em texto claro.");
                }
            }
            if (!string.IsNullOrEmpty(settingsCopy.ClubLog.PasswordEncrypted))
            {
                try   { settingsCopy.ClubLog.PasswordEncrypted = CredentialProtector.Protect(settingsCopy.ClubLog.PasswordEncrypted); }
                catch { }
            }

            // Cifrar API Keys dos perfis (uma API Key dá escrita no logbook —
            // merece a mesma protecção DPAPI que as outras passwords).
            foreach (var profile in settingsCopy.Api.Profiles)
            {
                if (string.IsNullOrEmpty(profile.ApiKey)) continue;
                try
                {
                    profile.ApiKey = CredentialProtector.Protect(profile.ApiKey);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Falha ao cifrar API Key do perfil {Profile}. Será guardada em texto claro.", profile.Name);
                }
            }

            // Guardar ficheiro
            var encryptedJson = JsonSerializer.Serialize(settingsCopy, JsonOptions);
            File.WriteAllText(_configPath, encryptedJson);

            _logger?.LogInformation("Configurações guardadas em {Path}", _configPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao guardar configurações em {Path}", _configPath);
            throw;
        }
    }

    /// <summary>
    /// Verifica se é o primeiro arranque da aplicação
    /// </summary>
    public bool IsFirstRun() => !File.Exists(_configPath);

    /// <summary>
    /// Retorna o directório de configurações
    /// </summary>
    public string GetConfigDirectory() => _configDir;

    /// <summary>
    /// Retorna o caminho do ficheiro de configurações
    /// </summary>
    public string GetConfigPath() => _configPath;
}
