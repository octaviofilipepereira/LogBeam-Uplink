// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Reflection;
using LogBeam.Core.Adif;
using LogBeam.Core.Models;
using LogBeam.Core.N1MM;
using LogBeam.Core.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceAppSettings    = LogBeam.Service.Models.AppSettings;
using ServiceClubLogSettings = LogBeam.Service.Models.ClubLogSettings;

namespace LogBeam.Service.Services;

/// <summary>
/// Faz upload em tempo real de cada QSO recebido do N1MM+ para o ClubLog.
/// Usa a API realtime.php do ClubLog (HTTP POST com ADIF).
///
/// Comportamento por status HTTP:
///   200 → QSO aceite (OK, Duplicate ou Modified)
///   400 → QSO rejeitado pelo parser ADIF — regista aviso, continua
///   403 → Credenciais inválidas — suspende todos os envios (evita bloqueio de IP)
///   500 → Erro interno do ClubLog — regista erro, continua
/// </summary>
public class ClubLogUploadService : BackgroundService
{
    private const string RealtimeUrl = "https://clublog.org/realtime.php";

    // API key da aplicação LogBeam registada no ClubLog. Não está no código: entra na
    // compilação (ver LogBeam.Service.csproj). Sem ela, o envio para o ClubLog fica desactivado.
    private static readonly string? AppApiKey = typeof(ClubLogUploadService).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(a => a.Key == "ClubLogApiKey")?.Value;

    private readonly N1MMUdpListener _listener;
    private readonly ILogger<ClubLogUploadService> _logger;
    private readonly ServiceClubLogSettings _cfg;
    private readonly HttpClient _http;

    // Se true, recebemos um 403 → parar envios para não bloquear IP do utilizador
    private volatile bool _suspended;

    public ClubLogUploadService(
        N1MMUdpListener listener,
        IOptions<ServiceAppSettings> options,
        IHttpClientFactory httpFactory,
        ILogger<ClubLogUploadService> logger)
    {
        _listener = listener;
        _cfg      = options.Value.ClubLog;
        _http     = httpFactory.CreateClient("clublog");
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_cfg.Enabled)
        {
            _logger.LogInformation("ClubLog upload desactivado nas configurações.");
            return;
        }

        if (string.IsNullOrWhiteSpace(AppApiKey))
        {
            _logger.LogWarning("ClubLog: esta compilação não tem chave de aplicação do ClubLog. Upload desactivado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_cfg.Email) || string.IsNullOrWhiteSpace(_cfg.PasswordEncrypted))
        {
            _logger.LogWarning("ClubLog: email ou password não configurados. Upload desactivado.");
            return;
        }

        _logger.LogInformation("ClubLog upload em tempo real activo para {Email}", _cfg.Email);

        _listener.QsoReceived += (_, qso) => _ = UploadAsync(qso, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task UploadAsync(QsoRecord qso, CancellationToken ct)
    {
        if (_suspended || ct.IsCancellationRequested) return;
        if (qso.EventType == N1mmEventType.Delete)    return;

        // Decifrar password (já foi decifrada pelo SettingsManager)
        var password = _cfg.PasswordEncrypted;

        var callsign = string.IsNullOrWhiteSpace(_cfg.Callsign)
            ? qso.MyCall
            : _cfg.Callsign;

        if (string.IsNullOrWhiteSpace(callsign))
        {
            _logger.LogWarning("ClubLog: callsign não definido. QSO {Call} ignorado.", qso.Call);
            return;
        }

        var adif = AdifBuilder.Build(qso);

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("email",    _cfg.Email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("callsign", callsign),
            new KeyValuePair<string, string>("adif",     adif),
            new KeyValuePair<string, string>("api",      AppApiKey!),
        });

        try
        {
            _logger.LogDebug("ClubLog: a enviar QSO {Call} ({Band}/{Mode})...", qso.Call, qso.Band, qso.Mode);

            var resp = await _http.PostAsync(RealtimeUrl, form, ct);
            var body = (await resp.Content.ReadAsStringAsync(ct)).Trim();

            switch ((int)resp.StatusCode)
            {
                case 200:
                    _logger.LogInformation("ClubLog ✓ {Call}: {Msg}", qso.Call, body);
                    break;

                case 400:
                    // QSO rejeitado pelo parser — provavelmente dados inválidos, mas não é crítico
                    _logger.LogWarning("ClubLog rejeitou QSO {Call} (400): {Msg}", qso.Call, body);
                    break;

                case 403:
                    // CRÍTICO: parar imediatamente para evitar bloqueio de IP
                    _suspended = true;
                    _logger.LogError(
                        "ClubLog 403 — credenciais inválidas! Upload suspenso para evitar bloqueio de IP. " +
                        "Verifica email, App Password e callsign na tab ClubLog. Resposta: {Msg}", body);
                    break;

                case 500:
                    _logger.LogError("ClubLog 500 — erro interno do servidor. QSO {Call} não enviado. Tentar mais tarde. Resposta: {Msg}", qso.Call, body);
                    break;

                default:
                    _logger.LogWarning("ClubLog resposta inesperada {Code} para {Call}: {Msg}",
                        (int)resp.StatusCode, qso.Call, body);
                    break;
            }
        }
        catch (OperationCanceledException) { }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ClubLog: erro de rede ao enviar QSO {Call}", qso.Call);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClubLog: erro inesperado ao enviar QSO {Call}", qso.Call);
        }
    }
}
