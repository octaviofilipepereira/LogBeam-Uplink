// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Text.Json;
using LogBeam.Service.Telemetry;

namespace LogBeam.Tests.Telemetry;

public class TelemetryClientTests
{
    private const string Id = "11111111-2222-3333-4444-555555555555";

    private readonly FakeHttpHandler _server = new();
    private DateTime _now = new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

    private TelemetryClient NewClient(string baseUrl = "https://api.example.org/") =>
        new(new HttpClient(_server), baseUrl, () => _now);

    private static InstallInfo Install() =>
        new(Id, "CT7BFV", "2.5.0", "8.0.10", "Microsoft Windows 10.0.26200", "X64", "PT", new[] { "N1MM", "WSJTX" });

    [Fact]
    public async Task InstallIsPostedWithContractFieldNames()
    {
        var result = await NewClient().SendInstallAsync(Install(), CancellationToken.None);

        Assert.Equal(TelemetrySendResult.Ok, result);
        var req = Assert.Single(_server.Requests);
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Equal("https://api.example.org/api/uplink/install", req.Url);
        Assert.False(req.HasApiKey);

        using var json = JsonDocument.Parse(req.Body!);
        var names = json.RootElement.EnumerateObject().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "installation_id", "callsign", "app_version", "dotnet_version", "os", "os_arch",
                             "language", "logging_programs" }, names);
        Assert.Equal("WSJTX", json.RootElement.GetProperty("logging_programs")[1].GetString());
    }

    [Fact]
    public async Task ErrorIsPostedWithContractFieldNames()
    {
        var report = new ErrorReport
        {
            InstallationId = Id, Callsign = null, AppVersion = "2.5.0", OccurredAt = "2026-10-04 12:00:00",
            Component = "N1MM", ErrorType = "System.IO.IOException", Message = "Falha", StackTrace = "at X()", Count = 3
        };

        await NewClient().SendErrorAsync(report, CancellationToken.None);

        var req = Assert.Single(_server.Requests);
        Assert.Equal("https://api.example.org/api/uplink/error", req.Url);
        using var json = JsonDocument.Parse(req.Body!);
        var names = json.RootElement.EnumerateObject().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "installation_id", "callsign", "app_version", "occurred_at", "component", "error_type",
                             "message", "stack_trace", "count" }, names);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("callsign").ValueKind);
        Assert.Equal(3, json.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task DeleteUsesTheInstallationId()
    {
        var result = await NewClient("https://api.example.org").DeleteInstallAsync(Id, CancellationToken.None);

        Assert.Equal(TelemetrySendResult.Ok, result);
        var req = Assert.Single(_server.Requests);
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.Equal($"https://api.example.org/api/uplink/install/{Id}", req.Url);
        Assert.Null(req.Body);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest,          TelemetrySendResult.Rejected)]
    [InlineData(HttpStatusCode.NotFound,            TelemetrySendResult.Rejected)]
    [InlineData(HttpStatusCode.RequestEntityTooLarge, TelemetrySendResult.Rejected)]
    [InlineData(HttpStatusCode.InternalServerError, TelemetrySendResult.RetryLater)]
    [InlineData(HttpStatusCode.ServiceUnavailable,  TelemetrySendResult.RetryLater)]
    public async Task StatusCodesMapToResult(HttpStatusCode status, TelemetrySendResult expected)
    {
        _server.Respond = _ => new HttpResponseMessage(status);
        Assert.Equal(expected, await NewClient().SendInstallAsync(Install(), CancellationToken.None));
    }

    [Fact]
    public async Task NoNetworkMeansRetryLater()
    {
        _server.Respond = _ => throw new HttpRequestException("No such host is known.");
        Assert.Equal(TelemetrySendResult.RetryLater, await NewClient().SendInstallAsync(Install(), CancellationToken.None));
    }

    [Fact]
    public async Task TooManyRequestsPausesForRetryAfter()
    {
        var client = NewClient();
        _server.Respond = _ =>
        {
            var r = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            r.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(120));
            return r;
        };

        Assert.Equal(TelemetrySendResult.RetryLater, await client.SendInstallAsync(Install(), CancellationToken.None));
        Assert.Equal(_now.AddSeconds(120), client.PausedUntilUtc);

        // Durante a pausa nem se contacta o servidor.
        _server.Respond = _ => new HttpResponseMessage(HttpStatusCode.OK);
        _now = _now.AddSeconds(60);
        Assert.Equal(TelemetrySendResult.RetryLater, await client.SendInstallAsync(Install(), CancellationToken.None));
        Assert.Single(_server.Requests);

        _now = _now.AddSeconds(61);
        Assert.Equal(TelemetrySendResult.Ok, await client.SendInstallAsync(Install(), CancellationToken.None));
        Assert.Equal(2, _server.Requests.Count);
    }
}
