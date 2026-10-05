// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using LogBeam.Core.Models;
using LogBeam.Service.Models;
using LogBeam.Service.Services;
using LogBeam.Tests.Telemetry;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LogBeam.Tests.Service;

public class ApiClientServiceTests
{
    private readonly FakeHttpHandler _server = new();

    private ApiClientService Client(int retries = 1, int retryDelaySeconds = 10) =>
        new(new HttpClient(_server),
            Options.Create(new AppSettings { Api = new ApiSettings { BaseUrl = "https://api.example.org", RetryCount = retries, RetryDelaySeconds = retryDelaySeconds } }),
            NullLogger<ApiClientService>.Instance);

    private static readonly ApiProfile Profile = new() { Id = "p1", Name = "Teste", InstanceId = "abcdef0123", ApiKey = new string('a', 64) };
    private static QsoRecord Qso() => new() { Call = "EA1ABC", Band = "20m", Mode = "FT8", QsoDate = "20261005", TimeOn = "120000" };

    private static HttpResponseMessage Json(HttpStatusCode code, string json) => new(code) { Content = new StringContent(json) };

    private static HttpResponseMessage TooMany(int seconds)
    {
        var r = Json(HttpStatusCode.TooManyRequests, "{\"status\":\"error\"}");
        r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds));
        return r;
    }

    [Theory]
    [InlineData("{\"status\":\"success\",\"data\":{\"duplicate\":true,\"message\":\"QSO já existe\"}}", true)]
    [InlineData("{\"status\":\"success\",\"data\":{\"id\":123}}", false)]
    [InlineData("{\"status\":\"success\"}", false)]
    public async Task DuplicateIsReadFromTheResponse(string body, bool expected)
    {
        _server.Respond = _ => Json(HttpStatusCode.OK, body);
        var result = await Client().SendQsoAsync(Qso(), Profile, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(expected, result.Duplicate);
    }

    [Fact]
    public async Task TooManyRequestsWaitsForRetryAfterInsteadOfTheBackoff()
    {
        var calls = 0;
        _server.Respond = _ => ++calls == 1 ? TooMany(1) : Json(HttpStatusCode.OK, "{\"status\":\"success\"}");

        var watch  = Stopwatch.StartNew();
        var result = await Client(retries: 1, retryDelaySeconds: 10).SendQsoAsync(Qso(), Profile, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, _server.Requests.Count);
        Assert.InRange(watch.Elapsed.TotalSeconds, 0.9, 5);   // 1 s do Retry-After, não os 10 s do intervalo
    }

    [Fact]
    public async Task LongRetryAfterGoesToTheQueueWithoutWaiting()
    {
        _server.Respond = _ => TooMany(600);

        var watch  = Stopwatch.StartNew();
        var result = await Client(retries: 3).SendQsoAsync(Qso(), Profile, CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(result.IsPermanentFailure);   // vai para a fila offline
        Assert.Single(_server.Requests);
        Assert.InRange(watch.Elapsed.TotalSeconds, 0, 2);
    }
}
