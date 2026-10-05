// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Models;
using LogBeam.Core.N1MM;
using LogBeam.Service.Services;
using LogBeam.Service.Settings;
using LogBeam.Service.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ServiceAppSettings = LogBeam.Service.Models.AppSettings;

namespace LogBeam.UI;

/// <summary>
/// Arranca e para o host do LogBeam Service dentro do processo da UI.
/// </summary>
public class ServiceRunner : IDisposable
{
    private IHost?            _host;
    private CancellationTokenSource _cts = new();
    private Task?             _runTask;

    // Criado uma vez: os erros pendentes sobrevivem aos reinícios do serviço (e ficam em disco).
    private readonly TelemetryStore _telemetryStore = new(Path.Combine(AppContext.BaseDirectory, "telemetry.json"));

    public TelemetryStore TelemetryStore => _telemetryStore;

    /// <summary>Língua da interface, enviada nos dados da instalação; aplica-se no próximo arranque do serviço.</summary>
    public string Language { get; set; } = "PT";

    public bool IsRunning => _host is not null && !(_cts.IsCancellationRequested);

    public event Action<string>? StatusChanged;
    public event Action<QsoRecord, bool>? QsoResult;
    public event Action<QsoRecord>? QsoConfirmedByLogbeam;
    public event Action<QsoRecord>? QsoAlreadyLogged;
    public event Action<string, int>? ListenerFailed;
    public event Action? ClubLogCredentialsRejected;

    public void Start()
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();

        var settingsManager = new SettingsManager();
        var appSettings     = settingsManager.Load();

        ConfigureSerilog(appSettings, _telemetryStore);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<Microsoft.Extensions.Options.IOptions<ServiceAppSettings>>(
                    Microsoft.Extensions.Options.Options.Create(appSettings));
                services.AddSingleton(settingsManager);

                services.AddHttpClient("logbeam")
                    .ConfigureHttpClient(c =>
                    {
                        c.Timeout = TimeSpan.FromSeconds(appSettings.Api.TimeoutSeconds);
                        c.DefaultRequestHeaders.UserAgent.ParseAdd(AppVersion.UserAgent);
                    });
                services.AddHttpClient("clublog")
                    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));

                services.AddSingleton(sp => new N1MMUdpListener(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<N1MMUdpListener>>(),
                    appSettings.N1mm.UdpPort,
                    appSettings.N1mm.ListenAddress));
                services.AddSingleton(sp => new LogBeam.Core.Wsjtx.WsjtxUdpListener(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LogBeam.Core.Wsjtx.WsjtxUdpListener>>(),
                    appSettings.Wsjtx.UdpPort,
                    appSettings.Wsjtx.ListenAddress));
                services.AddSingleton(sp => new LogBeam.Core.Adif.AdifUdpListener(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LogBeam.Core.Adif.AdifUdpListener>>(),
                    appSettings.Log4om.UdpPort,
                    appSettings.Log4om.ListenAddress));
                // Cliente com nome: o registo como cliente tipado ficava sem efeito (o singleton recebia um
                // HttpClient sem configuração, com o tempo limite do .NET em vez do das definições).
                services.AddSingleton(sp => new ApiClientService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("logbeam"),
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceAppSettings>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ApiClientService>>()));
                services.AddSingleton<QsoQueueService>();
                services.AddSingleton<SessionLogService>();
                services.AddSingleton<QsoProcessor>();
                services.AddSingleton(sp => new ClubLogClient(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("clublog"),
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceAppSettings>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ClubLogClient>>()));
                services.AddHostedService(sp => sp.GetRequiredService<QsoProcessor>());

                // Dados da instalação e relatórios de erros (4.26, 4.27): só actuam com consentimento.
                services.AddSingleton(_telemetryStore);
                services.AddSingleton(new TelemetryContext(AppVersion.Current, Language));
                services.AddHttpClient("telemetry").ConfigureHttpClient(c =>
                {
                    c.Timeout = TimeSpan.FromSeconds(15);
                    c.DefaultRequestHeaders.UserAgent.ParseAdd(AppVersion.UserAgent);
                });
                services.AddSingleton(sp => new TelemetryClient(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("telemetry"),
                    appSettings.Api.BaseUrl));
                services.AddHostedService<TelemetryService>();
            })
            .Build();

        var processor = _host.Services.GetRequiredService<QsoProcessor>();
        processor.QsoResult += (qso, success) => QsoResult?.Invoke(qso, success);
        processor.QsoConfirmedByLogbeam += qso => QsoConfirmedByLogbeam?.Invoke(qso);
        processor.QsoAlreadyLogged      += qso => QsoAlreadyLogged?.Invoke(qso);
        processor.ListenerFailed        += (program, port) => ListenerFailed?.Invoke(program, port);
        processor.ClubLogCredentialsRejected += () => ClubLogCredentialsRejected?.Invoke();

        _runTask = Task.Run(async () =>
        {
            try
            {
                StatusChanged?.Invoke("running");
                await _host.RunAsync(_cts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro no ServiceRunner");
                StatusChanged?.Invoke("error");
                return;
            }
            StatusChanged?.Invoke("stopped");
        });
    }

    /// <summary>QSOs enviados com sucesso desde que o serviço arrancou (vazio se parado).</summary>
    public List<QsoRecord> GetSessionLog() =>
        _host?.Services.GetRequiredService<SessionLogService>().Snapshot() ?? new List<QsoRecord>();

    public void Stop()
    {
        if (_host is null) return;
        _cts.Cancel();
        _runTask?.Wait(TimeSpan.FromSeconds(5));
        _host.Dispose();
        _host    = null;
        _runTask = null;
    }

    private static void ConfigureSerilog(ServiceAppSettings settings, TelemetryStore telemetryStore)
    {
        var exeDir  = AppContext.BaseDirectory;
        var logPath = Path.IsPathRooted(settings.LogPath)
            ? settings.LogPath
            : Path.Combine(exeDir, settings.LogPath);

        var level = Enum.TryParse<Serilog.Events.LogEventLevel>(
            settings.LogLevel, ignoreCase: true, out var parsed)
            ? parsed : Serilog.Events.LogEventLevel.Information;

        var config = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        // Relatórios de erros só com consentimento do operador.
        if (settings.Telemetry.IsActive)
            config.WriteTo.Sink(new ErrorReportSink(telemetryStore), Serilog.Events.LogEventLevel.Error);

        Log.Logger = config.CreateLogger();
    }

    public void Dispose() => Stop();
}
