// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using LogBeam.Core.Adif;
using LogBeam.Core.Models;
using LogBeam.Core.N1MM;
using LogBeam.Core.Wsjtx;
using LogBeam.Service.Models;
using LogBeam.Service.Services;
using LogBeam.Tests.Telemetry;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LogBeam.Tests.Service;

/// <summary>
/// Circuito principal com servidores falsos: o logbook LogBeam (api.example.org) e o ClubLog
/// (clublog.org) respondem o que cada teste mandar. Os receptores UDP não são arrancados.
/// </summary>
public sealed class QsoProcessorTests : IDisposable
{
    private readonly FakeHttpHandler _server = new();
    private readonly string _queueFile = Path.Combine(Path.GetTempPath(), $"logbeam-queue-{Guid.NewGuid():N}.json");
    private HttpStatusCode _logbeamStatus = HttpStatusCode.OK;
    private HttpStatusCode _clubLogStatus = HttpStatusCode.OK;

    public QsoProcessorTests()
    {
        _server.Respond = req => req.RequestUri!.Host == "clublog.org"
            ? new HttpResponseMessage(_clubLogStatus) { Content = new StringContent("OK") }
            : new HttpResponseMessage(_logbeamStatus) { Content = new StringContent("{\"status\":\"success\",\"data\":{}}") };
    }

    public void Dispose() { if (File.Exists(_queueFile)) File.Delete(_queueFile); }

    private static AppSettings Settings(bool clubLog = true) => new()
    {
        MyCallsign = "CT7BFV",
        Api = new ApiSettings
        {
            BaseUrl = "https://api.example.org", RetryCount = 0,
            Profiles = { new ApiProfile { Id = "p1", Name = "Teste", InstanceId = "abcdef0123", ApiKey = new string('a', 64) } }
        },
        ClubLog = new ClubLogSettings { Enabled = clubLog, Email = "op@example.com", PasswordEncrypted = "app-pass" }
    };

    private (QsoProcessor Processor, QsoQueueService Queue) Build(AppSettings settings)
    {
        var options = Options.Create(settings);
        var queue   = new QsoQueueService(NullLogger<QsoQueueService>.Instance, _queueFile);
        var processor = new QsoProcessor(
            new N1MMUdpListener(NullLogger<N1MMUdpListener>.Instance),
            new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance),
            new AdifUdpListener(NullLogger<AdifUdpListener>.Instance),
            new ApiClientService(new HttpClient(_server), options, NullLogger<ApiClientService>.Instance),
            new ClubLogClient(new HttpClient(_server), options, NullLogger<ClubLogClient>.Instance, "chave-de-teste"),
            queue, new SessionLogService(), options, NullLogger<QsoProcessor>.Instance);
        return (processor, queue);
    }

    private static QsoRecord Qso() => new() { Call = "EA1ABC", Band = "20m", Mode = "FT8", QsoDate = "20261005", TimeOn = "120000" };

    private int Sent(string host) => _server.Requests.Count(r => new Uri(r.Url).Host == host);

    [Fact]
    public async Task QsoGoesToLogbookAndClubLog()
    {
        var (processor, queue) = Build(Settings());
        await processor.ProcessQsoAsync(Qso(), CancellationToken.None);

        Assert.Equal(1, Sent("api.example.org"));
        Assert.Equal(1, Sent("clublog.org"));
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task ClubLogDownIsQueuedAndResentLater()
    {
        _clubLogStatus = HttpStatusCode.ServiceUnavailable;
        var (processor, queue) = Build(Settings());
        await processor.ProcessQsoAsync(Qso(), CancellationToken.None);

        var pending = Assert.Single(queue.Snapshot());
        Assert.Equal(ClubLogClient.QueueId, pending.ProfileId);

        _clubLogStatus = HttpStatusCode.OK;
        await processor.FlushQueueAsync(CancellationToken.None);

        Assert.Equal(0, queue.Count);
        Assert.Equal(2, Sent("clublog.org"));
        Assert.Equal(1, Sent("api.example.org"));   // o logbook não recebe o QSO outra vez
    }

    [Fact]
    public async Task ClubLogRejectionIsNotQueued()
    {
        _clubLogStatus = HttpStatusCode.BadRequest;
        var (processor, queue) = Build(Settings());
        await processor.ProcessQsoAsync(Qso(), CancellationToken.None);

        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task PendingClubLogEntriesAreDroppedWhenClubLogIsTurnedOff()
    {
        _clubLogStatus = HttpStatusCode.ServiceUnavailable;
        var (processor, _) = Build(Settings());
        await processor.ProcessQsoAsync(Qso(), CancellationToken.None);

        var (afterTurnOff, queue) = Build(Settings(clubLog: false));   // mesma fila em disco
        Assert.Equal(1, queue.Count);
        await afterTurnOff.FlushQueueAsync(CancellationToken.None);

        Assert.Equal(0, queue.Count);
        Assert.Equal(1, Sent("clublog.org"));
    }

    [Fact]
    public async Task LogbookDownIsQueuedPerProfileAndResent()
    {
        _logbeamStatus = HttpStatusCode.InternalServerError;
        var (processor, queue) = Build(Settings(clubLog: false));
        await processor.ProcessQsoAsync(Qso(), CancellationToken.None);
        Assert.Equal("p1", Assert.Single(queue.Snapshot()).ProfileId);

        _logbeamStatus = HttpStatusCode.OK;
        await processor.FlushQueueAsync(CancellationToken.None);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task LogbookPermanentFailureIsDiscarded()
    {
        _logbeamStatus = HttpStatusCode.Unauthorized;   // chave revogada
        var (processor, queue) = Build(Settings(clubLog: false));
        await processor.ProcessQsoAsync(Qso(), CancellationToken.None);

        Assert.Equal(0, queue.Count);
    }
}
