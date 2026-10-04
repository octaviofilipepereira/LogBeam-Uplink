// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.RegularExpressions;

namespace LogBeam.Core.Api;

/// <summary>
/// Formato das credenciais de um logbook LogBeam:
/// Instance ID = 10 caracteres hexadecimais (o "id" do link view.php?id=…);
/// API Key = 64 caracteres hexadecimais, gerada em "Gerir Logbook" → "API Keys".
/// </summary>
public static class LogbookCredentials
{
    private static readonly Regex RawId  = new(@"^[0-9a-fA-F]{10}$", RegexOptions.Compiled);
    private static readonly Regex LinkId = new(@"(?:[?&]id=|/embed3d/)([0-9a-fA-F]{10})(?![0-9a-fA-F])", RegexOptions.Compiled);
    private static readonly Regex Key    = new(@"^[0-9a-f]{64}$", RegexOptions.Compiled);

    /// <summary>
    /// Instance ID a partir do que o operador colou: o identificador (10 hex) ou o link do logbook
    /// (…view.php?id=…, …/embed3d/…). Devolve-o em minúsculas; null se não for reconhecido.
    /// </summary>
    public static string? ExtractInstanceId(string? input)
    {
        var s = input?.Trim() ?? string.Empty;
        if (RawId.IsMatch(s)) return s.ToLowerInvariant();

        var m = LinkId.Match(s);
        return m.Success ? m.Groups[1].Value.ToLowerInvariant() : null;
    }

    /// <summary>API Key sem espaços e em minúsculas (o servidor gera-as em minúsculas).</summary>
    public static string NormalizeApiKey(string? input) => (input ?? string.Empty).Trim().ToLowerInvariant();

    public static bool IsApiKey(string? input) => Key.IsMatch(NormalizeApiKey(input));
}
