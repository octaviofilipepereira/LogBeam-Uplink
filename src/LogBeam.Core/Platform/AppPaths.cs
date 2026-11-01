// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Platform;

/// <summary>
/// Pasta dos dados da aplicação (definições, fila, registos). No Windows é a do executável, que
/// o instalador coloca na área do utilizador. No Linux o programa fica numa pasta só de leitura,
/// por isso os dados vão para ~/.config/logbeam-uplink, acessível apenas ao utilizador.
/// </summary>
public static class AppPaths
{
    public static string DataDir { get; } = Resolve();

    private static string Resolve()
    {
        if (OperatingSystem.IsWindows()) return AppContext.BaseDirectory;

        var config = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrWhiteSpace(config))
            config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

        var dir = Path.Combine(config, "logbeam-uplink");
        Directory.CreateDirectory(dir, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return dir;
    }
}
