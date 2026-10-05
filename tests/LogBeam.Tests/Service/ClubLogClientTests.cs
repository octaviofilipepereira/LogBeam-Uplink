// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using LogBeam.Core.Models;
using LogBeam.Service.Models;
using LogBeam.Service.Services;
using LogBeam.Tests.Telemetry;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LogBeam.Tests.Service;

public class ClubLogClientTests
{
    private const string AppKey = "chave-de-teste";
    private readonly FakeHttpHandler _server = new();

    private static AppSettings Settings(bool enabled = true, string clCall = "", string myCall = "CT7BFV") => new()
    {
        MyCallsign = myCall,
        ClubLog    = new ClubLogSettings { Enabled = enabled, Email = "op@example.com", PasswordEncrypted = "app-pass", Callsign = clCall }
    };

    private ClubLogClient Client(AppSettings s, string? key = AppKey) =>
        new(new HttpClient(_server), Options.Create(s), NullLogger<ClubLogClient>.Instance, key);

    private static QsoRecord Qso(string myCall = "") => new()
    {
        Call = "EA1ABC", Band = "20m", Mode = "SSB", Freq = "14.200", MyCall = myCall,
        QsoDate = "20261005", TimeOn = "120000"
    };

    private static Dictionary<string, string> Form(string body) =>
        body.Split('&').Select(p => p.Split('=', 2))
            .ToDictionary(p => WebUtility.UrlDecode(p[0]), p => WebUtility.UrlDecode(p[1]));

    [Fact]
    public async Task SendsCredentialsAppKeyAndAdif()
    {
        _server.Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("OK") };

        Assert.Equal(ClubLogResult.Ok, await Client(Settings()).UploadAsync(Qso(), CancellationToken.None));

        var req = Assert.Single(_server.Requests);
        Assert.Equal("https://clublog.org/realtime.php", req.Url);
        var form = Form(req.Body!);
        Assert.Equal("op@example.com", form["email"]);
        Assert.Equal("app-pass", form["password"]);
        Assert.Equal("CT7BFV", form["callsign"]);
        Assert.Equal(AppKey, form["api"]);
        Assert.Contains("<CALL:6>EA1ABC", form["adif"]);
    }

    [Theory]
    [InlineData("cs7xyz", "CT7BFV", "CT7BFV/P", "CS7XYZ")]   // separador ClubLog primeiro
    [InlineData("",       "ct7bfv", "CT7BFV/P", "CT7BFV")]   // depois a Estação
    [InlineData("",       "",       "ct7bfv/p", "CT7BFV/P")] // por fim o do programa de log
    public void CallsignOrder(string clCall, string myCall, string qsoMyCall, string expected)
    {
        Assert.Equal(expected, Client(Settings(clCall: clCall, myCall: myCall)).CallsignFor(Qso(qsoMyCall)));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest,          ClubLogResult.Rejected)]
    [InlineData(HttpStatusCode.InternalServerError, ClubLogResult.RetryLater)]
    [InlineData(HttpStatusCode.ServiceUnavailable,  ClubLogResult.RetryLater)]
    public async Task StatusCodesMapToResult(HttpStatusCode status, ClubLogResult expected)
    {
        _server.Respond = _ => new HttpResponseMessage(status) { Content = new StringContent("x") };
        Assert.Equal(expected, await Client(Settings()).UploadAsync(Qso(), CancellationToken.None));
    }

    [Fact]
    public async Task NoNetworkMeansRetryLater()
    {
        _server.Respond = _ => throw new HttpRequestException("No such host is known.");
        Assert.Equal(ClubLogResult.RetryLater, await Client(Settings()).UploadAsync(Qso(), CancellationToken.None));
    }

    [Fact]
    public async Task ForbiddenSuspendsFurtherUploads()
    {
        _server.Respond = _ => new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Invalid login") };
        var client = Client(Settings());

        Assert.Equal(ClubLogResult.Suspended, await client.UploadAsync(Qso(), CancellationToken.None));
        Assert.Equal(ClubLogResult.Suspended, await client.UploadAsync(Qso(), CancellationToken.None));
        Assert.Single(_server.Requests);   // o segundo nem chega a ser enviado
    }

    [Fact]
    public async Task ForbiddenReportsRejectedCredentialsOnlyOnce()
    {
        _server.Respond = _ => new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Invalid login") };
        var client = Client(Settings());
        var reported = 0;
        client.CredentialsRejected += () => reported++;

        await client.UploadAsync(Qso(), CancellationToken.None);
        await client.UploadAsync(Qso(), CancellationToken.None);

        Assert.Equal(1, reported);
    }

    [Fact]
    public async Task InactiveWithoutAppKeyCredentialsOrWhenDisabled()
    {
        Assert.False(Client(Settings(), key: "").IsActive);
        Assert.False(Client(Settings(enabled: false)).IsActive);
        var noPass = Settings(); noPass.ClubLog.PasswordEncrypted = "";
        Assert.False(Client(noPass).IsActive);

        Assert.Equal(ClubLogResult.NotConfigured, await Client(Settings(enabled: false)).UploadAsync(Qso(), CancellationToken.None));
        Assert.Empty(_server.Requests);
    }
}
