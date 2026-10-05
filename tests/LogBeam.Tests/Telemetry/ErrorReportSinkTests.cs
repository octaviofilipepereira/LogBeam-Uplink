// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Service.Telemetry;
using Serilog.Events;
using Serilog.Parsing;

namespace LogBeam.Tests.Telemetry;

public class ErrorReportSinkTests
{
    private static LogEvent Event(LogEventLevel level, string template, string? source, Exception? ex = null,
                                  params LogEventProperty[] extra)
    {
        var props = extra.ToList();
        if (source is not null) props.Add(new LogEventProperty("SourceContext", new ScalarValue(source)));
        return new LogEvent(DateTimeOffset.UtcNow, level, ex, new MessageTemplateParser().Parse(template), props);
    }

    [Theory]
    [InlineData("LogBeam.Service.Services.ApiClientService", "ApiClient")]
    [InlineData("LogBeam.Core.N1MM.N1MMUdpListener",          "N1MM")]
    [InlineData("LogBeam.Core.N1MM.N1MMConfigManager",        "N1MM")]
    [InlineData("LogBeam.Core.Wsjtx.WsjtxUdpListener",        "WSJTX")]
    [InlineData("LogBeam.Core.Adif.AdifUdpListener",          "Log4OM")]
    [InlineData("LogBeam.Service.Services.ClubLogUploadService", "ClubLog")]
    [InlineData("LogBeam.UI.Program",                         "UI")]
    [InlineData("LogBeam.Service.Services.QsoProcessor",      "Service")]
    [InlineData("",                                           "Service")]
    public void ComponentFromSource(string source, string expected)
    {
        Assert.Equal(expected, ErrorReportSink.ComponentOf(source));
    }

    [Fact]
    public void ErrorIsStoredWithTemplateNotValues()
    {
        var store = new TelemetryStore(null);
        var sink  = new ErrorReportSink(store);

        sink.Emit(Event(LogEventLevel.Error, "Falha a enviar o QSO com {Call}", "LogBeam.Service.Services.ApiClientService",
                        new HttpRequestException("Connection refused"),
                        new LogEventProperty("Call", new ScalarValue("EA1ABC"))));

        var p = store.NextBatch().Single();
        Assert.Equal("ApiClient", p.Component);
        Assert.Equal("System.Net.Http.HttpRequestException", p.ErrorType);
        Assert.Equal("Falha a enviar o QSO com {Call} | Connection refused", p.Message);
        Assert.DoesNotContain("EA1ABC", p.Message);
    }

    [Fact]
    public void GenericSourceUsesTheClassThatThrew()
    {
        // O QsoProcessor regista a falha do receptor do N1MM+ (porta ocupada): o componente é N1MM.
        using var blocker = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp) { ExclusiveAddressUse = true };
        blocker.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 0));
        var port = ((System.Net.IPEndPoint)blocker.LocalEndPoint!).Port;

        using var listener = new LogBeam.Core.N1MM.N1MMUdpListener(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LogBeam.Core.N1MM.N1MMUdpListener>.Instance, port, "127.0.0.1");
        var ex = Record.Exception(() => listener.Start());

        Assert.NotNull(ex);
        Assert.Equal("N1MM", ErrorReportSink.ComponentOf("LogBeam.Service.Services.QsoProcessor", ex));
        Assert.Equal("ApiClient", ErrorReportSink.ComponentOf("LogBeam.Service.Services.ApiClientService", ex));
    }

    [Fact]
    public void ErrorWithoutExceptionHasGenericType()
    {
        var store = new TelemetryStore(null);
        new ErrorReportSink(store).Emit(Event(LogEventLevel.Fatal, "Sem rede", "LogBeam.UI.Program"));

        var p = store.NextBatch().Single();
        Assert.Equal("LogError", p.ErrorType);
        Assert.Equal("UI", p.Component);
        Assert.Equal(string.Empty, p.StackTrace);
    }

    [Fact]
    public void WarningsAreIgnored()
    {
        var store = new TelemetryStore(null);
        new ErrorReportSink(store).Emit(Event(LogEventLevel.Warning, "Aviso", "LogBeam.UI.Program"));
        Assert.Equal(0, store.PendingCount);
    }

    [Fact]
    public void ErrorsOfTheTelemetryItselfAreIgnored()
    {
        var store = new TelemetryStore(null);
        new ErrorReportSink(store).Emit(Event(LogEventLevel.Error, "Falha", "LogBeam.Service.Telemetry.TelemetryService"));
        Assert.Equal(0, store.PendingCount);
    }
}
