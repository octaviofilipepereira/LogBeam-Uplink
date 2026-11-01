// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Serilog;

namespace LogBeam.Linux;

/// <summary>
/// "Iniciar com o sistema": entrada em ~/.config/autostart, com <see cref="TrayArgument"/> para a
/// aplicação arrancar só na área de notificação.
/// </summary>
internal static class LinuxStartup
{
    public const string TrayArgument = "--tray";

    private static string EntryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "autostart", "logbeam-uplink.desktop");

    public static bool IsEnabled() => File.Exists(EntryPath);

    public static void Set(bool enabled)
    {
        try
        {
            if (!enabled)
            {
                File.Delete(EntryPath);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(EntryPath)!);
            File.WriteAllText(EntryPath,
                "[Desktop Entry]\n" +
                "Type=Application\n" +
                "Name=LogBeam Uplink\n" +
                $"Exec=logbeam-uplink {TrayArgument}\n" +
                "Icon=logbeam-uplink\n" +
                "Terminal=false\n");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Warning(ex, "Não foi possível alterar o arranque com o sistema");
        }
    }
}
