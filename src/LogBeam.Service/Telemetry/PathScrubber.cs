// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.RegularExpressions;

namespace LogBeam.Service.Telemetry;

/// <summary>
/// Retira dos relatórios de erros o nome do utilizador do Windows (que aparece nos caminhos,
/// C:\Users\Nome\…, e muitas vezes é o nome verdadeiro da pessoa) e o nome do computador.
/// </summary>
public static class PathScrubber
{
    // Pára nos caracteres que o Windows não aceita em nomes de utilizador e na apóstrofe (que
    // costuma fechar o caminho numa mensagem de erro).
    private static readonly Regex UserFolder = new(
        @"(?<prefix>[A-Za-z]:[\\/]+Users[\\/]+)(?<user>[^\\/:*?""<>|'\[\];=,+\r\n]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Scrub(string? text) => Scrub(text, Environment.UserName, Environment.MachineName);

    public static string Scrub(string? text, string? userName, string? machineName)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Primeiro os nomes deste computador (cobre nomes com apóstrofe), depois qualquer outra pasta de utilizador.
        var s = ReplaceWord(text, userName, "<user>");
        s = ReplaceWord(s, machineName, "<pc>");
        return UserFolder.Replace(s, "${prefix}<user>");
    }

    // Só a palavra inteira, para um utilizador "user" não estragar "UserSettings" no rasto da pilha.
    private static string ReplaceWord(string text, string? word, string replacement) =>
        string.IsNullOrWhiteSpace(word) || word.Length < 3
            ? text
            : Regex.Replace(text, $@"(?<![\w-]){Regex.Escape(word)}(?![\w-])", replacement, RegexOptions.IgnoreCase);
}
