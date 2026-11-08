// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json;

namespace LogBeam.Core.Updates;

/// <summary>Resultado de uma verificação. <c>Error</c> preenchido: a verificação falhou (sem rede, manifesto inválido).</summary>
public sealed record UpdateCheckResult(bool HasUpdate, string? LatestVersion, string? DownloadUrl, string? Error);

/// <summary>
/// Verifica se há uma versão mais recente do LogBeam Uplink no manifesto do logbeam.org
/// (<c>{"version": "2.6.1", "url": "https://logbeam.org/uplink/"}</c>). Os outros campos do
/// manifesto (instalador, pacotes Linux) são da página de download e não contam aqui.
/// Nunca lança excepções: as falhas voltam em <see cref="UpdateCheckResult.Error"/>.
/// </summary>
public sealed class UpdateChecker
{
    public const string DefaultManifestUrl  = "https://logbeam.org/uplink/version.json";
    public const string DefaultDownloadPage = "https://logbeam.org/uplink/";

    /// <summary>Endereço alternativo do manifesto, para ensaios com um servidor local.</summary>
    public const string ManifestUrlVariable = "LOGBEAM_UPLINK_MANIFEST_URL";

    private readonly string _currentVersion;
    private readonly string _manifestUrl;
    private readonly HttpMessageHandler? _handler;

    public UpdateChecker(string currentVersion, HttpMessageHandler? handler = null, string? manifestUrl = null)
    {
        _currentVersion = currentVersion;
        _handler        = handler;
        _manifestUrl    = manifestUrl
            ?? (Environment.GetEnvironmentVariable(ManifestUrlVariable) is { Length: > 0 } fromEnv ? fromEnv : DefaultManifestUrl);
    }

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken ct)
    {
        try
        {
            using var http = _handler is null ? new HttpClient() : new HttpClient(_handler, disposeHandler: false);
            http.Timeout = TimeSpan.FromSeconds(15);
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"LogBeam-Uplink/{_currentVersion}");
            var json = await http.GetStringAsync(_manifestUrl, ct);
            return Compare(json, _currentVersion);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(false, null, null, ex.Message);
        }
    }

    /// <summary>Compara o manifesto com a versão instalada. A página de download só é aceite em https.</summary>
    public static UpdateCheckResult Compare(string manifestJson, string currentVersion)
    {
        try
        {
            using var doc = JsonDocument.Parse(manifestJson);
            var root   = doc.RootElement;
            var latest = StringProperty(root, "version")?.Trim();
            var url    = StringProperty(root, "url")?.Trim();
            if (url is null || !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) url = DefaultDownloadPage;

            if (string.IsNullOrEmpty(latest))
                return new UpdateCheckResult(false, null, null, "manifesto sem campo 'version'");
            if (!TryParseVersion(latest, out var latestVer))
                return new UpdateCheckResult(false, latest, url, $"versão inválida no manifesto: {latest}");

            var newer = TryParseVersion(currentVersion, out var currentVer) && latestVer > currentVer;
            return new UpdateCheckResult(newer, latest, url, null);
        }
        catch (JsonException ex)
        {
            return new UpdateCheckResult(false, null, null, $"manifesto inválido: {ex.Message}");
        }
    }

    private static string? StringProperty(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object
        && root.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>"2.6.1", também com sufixo ("2.6.1-beta", "2.6.1+abc"), que não conta para a comparação.</summary>
    private static bool TryParseVersion(string text, out Version version)
    {
        var core = text.Split('-', '+')[0];
        if (Version.TryParse(core, out var parsed))
        {
            version = parsed;
            return true;
        }
        version = new Version(0, 0);
        return false;
    }
}
