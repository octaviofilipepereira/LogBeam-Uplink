// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;

namespace LogBeam.Core.N1MM;

public enum N1MMBroadcastStatus
{
    IniNotFound,
    NotConfigured,
    ConfiguredCorrectly,
    ConfiguredDifferentPort
}

public record BackupInfo(string Path, DateTime CreatedAt);

/// <summary>
/// Manages detection and automatic configuration of N1MM+ broadcast settings.
/// </summary>
public class N1MMConfigManager
{
    private const string Section = "ExternalBroadcast";
    private const string KeyAddr  = "BroadcastContactsToIPAddr";
    private const string KeyState = "BroadcastContactsState";
    private const string KeyTypes = "BroadcastContactsTypes";

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

    public string GetIniPath(string n1mmFolder) =>
        Path.Combine(n1mmFolder, "N1MMLoggerPlus.ini");

    /// <summary>
    /// Reads the current broadcast configuration status.
    /// </summary>
    public N1MMBroadcastStatus ReadStatus(string n1mmFolder, int expectedPort = 12060)
    {
        var iniPath = GetIniPath(n1mmFolder);
        if (!File.Exists(iniPath)) return N1MMBroadcastStatus.IniNotFound;

        try
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(iniPath);

            var state = data[Section][KeyState];
            var addr  = data[Section][KeyAddr];
            var types = data[Section][KeyTypes];

            if (string.IsNullOrEmpty(state) || 
                !state.Equals("True", StringComparison.OrdinalIgnoreCase))
                return N1MMBroadcastStatus.NotConfigured;

            if (string.IsNullOrEmpty(types) ||
                !types.Contains("contactinfo", StringComparison.OrdinalIgnoreCase))
                return N1MMBroadcastStatus.NotConfigured;

            if (!string.IsNullOrEmpty(addr) && addr.Contains($":{expectedPort}"))
                return N1MMBroadcastStatus.ConfiguredCorrectly;

            return N1MMBroadcastStatus.ConfiguredDifferentPort;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read N1MM ini: {Path}", iniPath);
            return N1MMBroadcastStatus.IniNotFound;
        }
    }

    /// <summary>
    /// Applies broadcast configuration to N1MM+ .ini file with automatic backup.
    /// </summary>
    public bool ApplyConfiguration(
        string n1mmFolder,
        string ip   = "127.0.0.1",
        int    port = 12060)
    {
        var iniPath    = GetIniPath(n1mmFolder);
        var backupPath = iniPath + $".backup_{DateTime.Now:yyyyMMdd_HHmmss}";

        if (!File.Exists(iniPath))
        {
            _logger.LogWarning("N1MM ini not found at {Path}", iniPath);
            return false;
        }

        try
        {
            // Backup first
            File.Copy(iniPath, backupPath, overwrite: false);
            _logger.LogInformation("N1MM ini backup created: {Backup}", backupPath);

            var parser = new FileIniDataParser();
            var data   = parser.ReadFile(iniPath);

            data[Section][KeyAddr]  = $"{ip}:{port}";
            data[Section][KeyState] = "True";
            data[Section][KeyTypes] = "contactinfo";

            parser.WriteFile(iniPath, data);

            _logger.LogInformation(
                "N1MM broadcast configured: {Ip}:{Port}", ip, port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure N1MM ini.");

            // Rollback
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, iniPath, overwrite: true);
                _logger.LogInformation("Rollback applied from {Backup}", backupPath);
            }
            return false;
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
            .GetFiles(n1mmFolder, "N1MMLoggerPlus.ini.backup*")
            .Select(f => new BackupInfo(f, File.GetCreationTime(f)))
            .OrderByDescending(b => b.CreatedAt);

    /// <summary>
    /// Checks if the N1MM+ process is currently running.
    /// </summary>
    public bool IsN1MMRunning() =>
        System.Diagnostics.Process
            .GetProcessesByName("N1MM+")
            .Concat(System.Diagnostics.Process.GetProcessesByName("N1MMLogger"))
            .Any();
}
