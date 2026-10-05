// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Microsoft.Win32;

namespace LogBeam.UI;

/// <summary>
/// "Iniciar com o Windows": valor na chave Run do utilizador (HKCU, sem administrador), com
/// <see cref="TrayArgument"/> para a aplicação arrancar só na área de notificação. O desinstalador apaga o valor.
/// </summary>
internal static class WindowsStartup
{
    public const string TrayArgument = "--tray";
    public const string ValueName    = "LogBeam Uplink";

    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <param name="runKey">Chave a usar (os testes usam uma chave própria).</param>
    public static bool IsEnabled(string runKey = RunKey)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(runKey);
            return key?.GetValue(ValueName) is string s && s.Length > 0;
        }
        catch { return false; }
    }

    public static void Set(bool enabled, string exePath, string runKey = RunKey)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(runKey, writable: true);
            if (enabled)
                key.SetValue(ValueName, Command(exePath), RegistryValueKind.String);
            else if (key.GetValue(ValueName) is not null)
                key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Não foi possível alterar o arranque com o Windows");
        }
    }

    public static string Command(string exePath) => $"\"{exePath}\" {TrayArgument}";
}
