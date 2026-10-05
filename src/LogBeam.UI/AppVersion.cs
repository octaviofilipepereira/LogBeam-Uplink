// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Reflection;

namespace LogBeam.UI;

/// <summary>Versão da aplicação, lida da compilação (origem única: Directory.Build.props).</summary>
public static class AppVersion
{
    public static readonly string Current =
        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(AppVersion).Assembly.GetName().Version?.ToString(3)
        ?? "0.0.0";

    /// <summary>Nome com a versão, para títulos: "LogBeam Uplink v2.5.0 by CT7BFV".</summary>
    public static string Title => $"LogBeam Uplink v{Current} by CT7BFV";
}
