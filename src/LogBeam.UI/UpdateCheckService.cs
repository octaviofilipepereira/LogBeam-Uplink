// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json;

namespace LogBeam.UI;

public record UpdateCheckResult(bool HasUpdate, string? LatestVersion, string? DownloadUrl, string? Error);

/// <summary>
/// Verifica se há uma versão mais recente do LogBeam Uplink, consultando um
/// manifesto JSON simples: { "version": "2.6.0", "url": "https://..." }.
///
/// NOTA: este manifesto ainda não existe no servidor — precisa de ser publicado
/// em https://logbeam.org/uplink/version.json antes desta funcionalidade ter efeito.
/// Até lá, a verificação falha silenciosamente (sem internet/404 tratam-se da
/// mesma forma) e a aplicação continua a funcionar normalmente.
/// </summary>
public class UpdateCheckService
{
    private const string VersionManifestUrl = "https://logbeam.org/uplink/version.json";

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            var json = await http.GetStringAsync(VersionManifestUrl, ct);

            using var doc = JsonDocument.Parse(json);
            var latest = doc.RootElement.TryGetProperty("version", out var v) ? v.GetString() : null;
            var url    = doc.RootElement.TryGetProperty("url", out var u) ? u.GetString() : null;

            if (string.IsNullOrEmpty(latest))
                return new UpdateCheckResult(false, null, null, "manifesto sem campo 'version'");

            if (Version.TryParse(latest, out var latestVer) && Version.TryParse(AppVersion.Current, out var curVer)
                && latestVer > curVer)
                return new UpdateCheckResult(true, latest, url, null);

            return new UpdateCheckResult(false, latest, url, null);
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(false, null, null, ex.Message);
        }
    }
}
