// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Service.Models;
using LogBeam.Service.Telemetry;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LogBeam.Tests.Telemetry;

public class TelemetryServiceTests
{
    private const string Id = "11111111-2222-3333-4444-555555555555";
    private static readonly TelemetryContext Context = new("2.5.0", "PT");

    private readonly FakeHttpHandler _server = new();
    private readonly TelemetryStore _store = new(null);

    private static AppSettings Settings(bool consent) => new()
    {
        MyCallsign = "ct7bfv",
        Telemetry  = new TelemetrySettings { Consent = consent, InstallationId = consent ? Id : string.Empty }
    };

    private TelemetryService NewService(AppSettings settings) =>
        new(Options.Create(settings), new TelemetryClient(new HttpClient(_server), "https://api.example.org"),
            _store, Context, NullLogger<TelemetryService>.Instance);

    /// <summary>Arranca o serviço, espera pelo primeiro ciclo (n pedidos) e pára-o.</summary>
    private async Task RunFirstCycle(TelemetryService service, int expectedRequests)
    {
        await service.StartAsync(CancellationToken.None);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (_server.Requests.Count < expectedRequests && DateTime.UtcNow < deadline)
            await Task.Delay(20);
        await Task.Delay(100);   // o ciclo termina de gravar o estado
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task WithoutConsentNothingIsSent()
    {
        _store.Add("UI", "X", "Falha", "");
        await RunFirstCycle(NewService(Settings(consent: false)), expectedRequests: 0);

        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task WithConsentSendsInstallThenPendingErrors()
    {
        _store.Add("N1MM", "System.IO.IOException", "Falha", "");
        _store.Add("N1MM", "System.IO.IOException", "Falha", "");

        await RunFirstCycle(NewService(Settings(consent: true)), expectedRequests: 2);

        Assert.Equal(new[] { "https://api.example.org/api/uplink/install", "https://api.example.org/api/uplink/error" },
                     _server.Requests.Select(r => r.Url));
        Assert.Contains("\"callsign\":\"CT7BFV\"", _server.Requests[0].Body);
        Assert.Contains("\"count\":2", _server.Requests[1].Body);
        Assert.Equal(0, _store.PendingCount);
        Assert.NotEqual(string.Empty, _store.LastInstallFingerprint);
    }

    [Fact]
    public async Task UnchangedInstallIsNotResent()
    {
        var settings = Settings(consent: true);
        _store.LastInstallFingerprint = TelemetryService.BuildInstallInfo(settings, Context).Fingerprint();

        await RunFirstCycle(NewService(settings), expectedRequests: 0);

        Assert.Empty(_server.Requests);
    }

    [Fact]
    public async Task RejectedInstallIsNotRetriedUntilNextStart()
    {
        _server.Respond = _ => new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        var service = NewService(Settings(consent: true));

        await service.RunCycleAsync(CancellationToken.None);
        await service.RunCycleAsync(CancellationToken.None);
        Assert.Single(_server.Requests);

        await NewService(Settings(consent: true)).RunCycleAsync(CancellationToken.None);   // próximo arranque
        Assert.Equal(2, _server.Requests.Count);
    }

    [Fact]
    public async Task InstallWithoutNetworkIsRetriedNextCycle()
    {
        _server.Respond = _ => throw new HttpRequestException("No such host is known.");
        var service = NewService(Settings(consent: true));

        await service.RunCycleAsync(CancellationToken.None);
        await service.RunCycleAsync(CancellationToken.None);
        Assert.Equal(2, _server.Requests.Count);
    }

    [Fact]
    public async Task ErrorsStayPendingWithoutNetwork()
    {
        _server.Respond = _ => throw new HttpRequestException("No such host is known.");
        _store.Add("UI", "X", "Falha", "");

        await RunFirstCycle(NewService(Settings(consent: true)), expectedRequests: 2);

        Assert.Equal(1, _store.PendingCount);
        Assert.Equal(string.Empty, _store.LastInstallFingerprint);
    }

    [Fact]
    public async Task RevokedInstallationIsDeletedOnTheServer()
    {
        _store.Revoke(Id);

        await RunFirstCycle(NewService(Settings(consent: false)), expectedRequests: 1);

        var req = Assert.Single(_server.Requests);
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.Equal($"https://api.example.org/api/uplink/install/{Id}", req.Url);
        Assert.Equal(string.Empty, _store.PendingDeletionId);
    }

    [Fact]
    public async Task DeletionIsKeptForLaterWithoutNetwork()
    {
        _server.Respond = _ => throw new HttpRequestException("No such host is known.");
        _store.Revoke(Id);

        await RunFirstCycle(NewService(Settings(consent: false)), expectedRequests: 1);

        Assert.Equal(Id, _store.PendingDeletionId);
    }

    [Fact]
    public void InstallInfoListsOnlyEnabledProgramsAndFallsBackToClubLogCallsign()
    {
        var settings = Settings(consent: true);
        settings.MyCallsign       = "";
        settings.ClubLog.Callsign = "cs7abc";
        settings.Log4om.Enabled   = true;

        var info = TelemetryService.BuildInstallInfo(settings, Context);

        Assert.Equal(new[] { "N1MM", "LOG4OM" }, info.LoggingPrograms);
        Assert.Equal("CS7ABC", info.Callsign);
        Assert.Equal(Id, info.InstallationId);
        Assert.Equal("PT", info.Language);
        Assert.Equal(new InstallDestinations(LogBeam: false, ClubLog: false), info.Destinations);
    }

    [Fact]
    public void InstallInfoDestinationsCountOnlyUsableLogbooksAndTheClubLogSwitch()
    {
        var settings = Settings(consent: true);
        settings.ClubLog.Enabled = true;
        settings.Api.Profiles.Add(new ApiProfile { Enabled = false, InstanceId = "abcdef0123", ApiKey = new string('a', 64) });
        settings.Api.Profiles.Add(new ApiProfile { Enabled = true,  InstanceId = "",           ApiKey = new string('a', 64) });

        Assert.Equal(new InstallDestinations(LogBeam: false, ClubLog: true),
                     TelemetryService.BuildInstallInfo(settings, Context).Destinations);   // só ClubLog

        settings.Api.Profiles.Add(new ApiProfile { Enabled = true, InstanceId = "abcdef0123", ApiKey = new string('b', 64) });
        Assert.Equal(new InstallDestinations(LogBeam: true, ClubLog: true),
                     TelemetryService.BuildInstallInfo(settings, Context).Destinations);
    }

    [Fact]
    public void CallsignIsNullWhenNoneIsSet()
    {
        Assert.Null(TelemetryService.Callsign(new AppSettings()));
    }
}
