// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json;
using System.Text.Json.Serialization;
using LogBeam.UI.Models;

namespace LogBeam.UI.Settings;

/// <summary>
/// Gere ui.json (preferências exclusivas da UI, ex: idioma).
/// As configurações partilhadas com o Service (settings.json) são geridas por
/// LogBeam.Service.Settings.SettingsManager.
/// </summary>
public class UiPrefsManager
{
    private readonly string _prefsPath;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UiPrefsManager()
    {
        _prefsPath = Path.Combine(AppContext.BaseDirectory, "ui.json");
    }

    public UiPreferences LoadPrefs()
    {
        try
        {
            if (!File.Exists(_prefsPath)) return new UiPreferences();
            return JsonSerializer.Deserialize<UiPreferences>(File.ReadAllText(_prefsPath), JsonOpts) ?? new UiPreferences();
        }
        catch { return new UiPreferences(); }
    }

    public void SavePrefs(UiPreferences prefs)
    {
        Directory.CreateDirectory(AppContext.BaseDirectory);
        File.WriteAllText(_prefsPath, JsonSerializer.Serialize(prefs, JsonOpts));
    }
}
