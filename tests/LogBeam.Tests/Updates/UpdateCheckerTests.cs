// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using LogBeam.Core.Updates;
using LogBeam.Tests.Telemetry;

namespace LogBeam.Tests.Updates;

/// <summary>Manifesto de versões do logbeam.org: comparação com a versão instalada e falhas.</summary>
public class UpdateCheckerTests
{
    // O manifesto publicado tem mais campos (instalador, pacotes Linux): não podem estragar a leitura.
    private const string Published = """
        {
          "version": "2.6.1",
          "url": "https://logbeam.org/uplink/",
          "download": "https://github.com/octaviofilipepereira/LogBeam-Uplink/releases/download/v2.6.1/LogBeamUplink-Setup-2.6.1.exe",
          "sha256": "CEFFE1B580CF5052FE6C7B081D8C7C4C6CE7BD0CBC653ABC6ED2C222992EB9F0",
          "released": "2026-11-08",
          "size_mb": 46.7,
          "linux": { "amd64": { "download": "https://x/a.deb", "sha256": "ab", "size_mb": 36.1 } }
        }
        """;

    [Theory]
    [InlineData("2.6.0", true)]
    [InlineData("2.5.1", true)]
    [InlineData("2.6.1", false)]
    [InlineData("2.7.0", false)]
    [InlineData("2.6.0-beta", true)]
    [InlineData("2.6.1+abc123", false)]
    public void NewerVersionIsDetected(string installed, bool expected)
    {
        var r = UpdateChecker.Compare(Published, installed);
        Assert.Null(r.Error);
        Assert.Equal(expected, r.HasUpdate);
        Assert.Equal("2.6.1", r.LatestVersion);
        Assert.Equal("https://logbeam.org/uplink/", r.DownloadUrl);
    }

    [Theory]
    [InlineData("""{"url":"https://logbeam.org/uplink/"}""")]
    [InlineData("""{"version":"","url":"https://logbeam.org/uplink/"}""")]
    [InlineData("""{"version":2.7}""")]
    [InlineData("""{"version":"brevemente"}""")]
    [InlineData("""["2.7.0"]""")]
    [InlineData("<html>404</html>")]
    public void InvalidManifestIsAnErrorNotAnUpdate(string json)
    {
        var r = UpdateChecker.Compare(json, "2.6.0");
        Assert.False(r.HasUpdate);
        Assert.NotNull(r.Error);
    }

    [Theory]
    [InlineData("""{"version":"9.0.0"}""")]
    [InlineData("""{"version":"9.0.0","url":"http://logbeam.org/uplink/"}""")]
    [InlineData("""{"version":"9.0.0","url":"javascript:alert(1)"}""")]
    public void DownloadPageIsHttpsOrTheOfficialPage(string json)
    {
        var r = UpdateChecker.Compare(json, "2.6.0");
        Assert.True(r.HasUpdate);
        Assert.Equal(UpdateChecker.DefaultDownloadPage, r.DownloadUrl);
    }

    [Fact]
    public async Task ReadsTheManifestWithTheUplinkUserAgent()
    {
        var handler = new FakeHttpHandler
        {
            Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Published) }
        };
        string? userAgent = null;
        var inner = handler.Respond;
        handler.Respond = req => { userAgent = req.Headers.UserAgent.ToString(); return inner(req); };

        var r = await new UpdateChecker("2.6.0", handler, "https://logbeam.org/uplink/version.json").CheckAsync(CancellationToken.None);

        Assert.True(r.HasUpdate);
        Assert.Equal("https://logbeam.org/uplink/version.json", Assert.Single(handler.Requests).Url);
        Assert.Equal("LogBeam-Uplink/2.6.0", userAgent);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task HttpErrorIsReturnedNotThrown(HttpStatusCode status)
    {
        var handler = new FakeHttpHandler { Respond = _ => new HttpResponseMessage(status) };
        var r = await new UpdateChecker("2.6.0", handler, "https://logbeam.org/uplink/version.json").CheckAsync(CancellationToken.None);
        Assert.False(r.HasUpdate);
        Assert.NotNull(r.Error);
    }

    [Fact]
    public async Task NetworkFailureIsReturnedNotThrown()
    {
        var handler = new FakeHttpHandler { Respond = _ => throw new HttpRequestException("sem rede") };
        var r = await new UpdateChecker("2.6.0", handler, "https://logbeam.org/uplink/version.json").CheckAsync(CancellationToken.None);
        Assert.False(r.HasUpdate);
        Assert.Contains("sem rede", r.Error);
    }
}
