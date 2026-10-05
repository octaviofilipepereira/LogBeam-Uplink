// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Reflection;
using LogBeam.Core.Adif;
using LogBeam.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceAppSettings = LogBeam.Service.Models.AppSettings;

namespace LogBeam.Service.Services;

public enum ClubLogResult
{
    /// <summary>Aceite (200: OK, duplicado ou actualizado).</summary>
    Ok,
    /// <summary>Recusado pelo ClubLog (400): reenviar não adianta.</summary>
    Rejected,
    /// <summary>Erro do ClubLog (5xx) ou sem rede: fica na fila offline.</summary>
    RetryLater,
    /// <summary>Credenciais recusadas (403): envios suspensos até o serviço reiniciar.</summary>
    Suspended,
    /// <summary>ClubLog desligado, sem chave de aplicação nesta compilação ou sem credenciais.</summary>
    NotConfigured
}

/// <summary>
/// Envio em tempo real para o ClubLog (realtime.php, ADIF por HTTP POST). É um destino do
/// QsoProcessor, como os logbooks LogBeam: recebe os QSOs de todos os programas e usa a mesma
/// fila offline (entradas com <see cref="QueueId"/>).
///
/// Por código HTTP: 200 aceite · 400 recusado · 403 credenciais inválidas, suspende todos os
/// envios (o ClubLog bloqueia o IP a quem insiste) · 5xx ou sem rede, tentar mais tarde.
/// </summary>
public class ClubLogClient
{
    public const string QueueId = "clublog";
    private const string RealtimeUrl = "https://clublog.org/realtime.php";

    // Chave de aplicação LogBeam no ClubLog. Não está no código: entra na compilação (ver
    // LogBeam.Service.csproj). Sem ela, o envio para o ClubLog fica desactivado.
    private static readonly string? BuildApiKey = typeof(ClubLogClient).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(a => a.Key == "ClubLogApiKey")?.Value;

    private readonly HttpClient _http;
    private readonly ServiceAppSettings _settings;
    private readonly string? _appApiKey;
    private readonly ILogger<ClubLogClient> _logger;

    // 403 recebido: não voltar a enviar até o serviço reiniciar (ex. depois de corrigir as credenciais).
    private volatile bool _suspended;

    /// <param name="appApiKey">Chave de aplicação; por omissão, a da compilação (os testes passam a sua).</param>
    public ClubLogClient(HttpClient http, IOptions<ServiceAppSettings> options, ILogger<ClubLogClient> logger,
                         string? appApiKey = null)
    {
        _http      = http;
        _settings  = options.Value;
        _appApiKey = appApiKey ?? BuildApiKey;
        _logger    = logger;
    }

    /// <summary>Activo nas definições, com chave de aplicação e credenciais.</summary>
    public bool IsActive =>
        _settings.ClubLog.Enabled
        && !string.IsNullOrWhiteSpace(_appApiKey)
        && !string.IsNullOrWhiteSpace(_settings.ClubLog.Email)
        && !string.IsNullOrWhiteSpace(_settings.ClubLog.PasswordEncrypted);

    /// <summary>Explica no log porque é que o ClubLog não está activo (chamado uma vez, no arranque).</summary>
    public void LogStatus()
    {
        var cfg = _settings.ClubLog;
        if (!cfg.Enabled)                                    _logger.LogInformation("ClubLog desactivado nas configurações.");
        else if (string.IsNullOrWhiteSpace(_appApiKey))     _logger.LogWarning("ClubLog: esta compilação não tem chave de aplicação do ClubLog. Envio desactivado.");
        else if (!IsActive)                                  _logger.LogWarning("ClubLog: e-mail ou App Password por preencher. Envio desactivado.");
        else _logger.LogInformation("ClubLog activo para {Email} ({Call})", cfg.Email, CallsignFor(new QsoRecord()) ?? "indicativo do programa de log");
    }

    /// <summary>Indicativo da conta: o do separador ClubLog, senão o da Estação, senão o do programa de log.</summary>
    public string? CallsignFor(QsoRecord qso)
    {
        foreach (var c in new[] { _settings.ClubLog.Callsign, _settings.MyCallsign, qso.MyCall })
            if (!string.IsNullOrWhiteSpace(c)) return c.Trim().ToUpperInvariant();
        return null;
    }

    public async Task<ClubLogResult> UploadAsync(QsoRecord qso, CancellationToken ct)
    {
        if (!IsActive) return ClubLogResult.NotConfigured;
        if (_suspended) return ClubLogResult.Suspended;

        var callsign = CallsignFor(qso);
        if (callsign is null)
        {
            _logger.LogWarning("ClubLog: indicativo não definido. QSO {Call} não enviado.", qso.Call);
            return ClubLogResult.Rejected;
        }

        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("email",    _settings.ClubLog.Email),
            new KeyValuePair<string, string>("password", _settings.ClubLog.PasswordEncrypted),   // já decifrada pelo SettingsManager
            new KeyValuePair<string, string>("callsign", callsign),
            new KeyValuePair<string, string>("adif",     AdifBuilder.Build(qso)),
            new KeyValuePair<string, string>("api",      _appApiKey!),
        });

        try
        {
            _logger.LogDebug("ClubLog: a enviar QSO {Call} ({Band}/{Mode})...", qso.Call, qso.Band, qso.Mode);
            using var resp = await _http.PostAsync(RealtimeUrl, form, ct);
            var body = (await resp.Content.ReadAsStringAsync(ct)).Trim();

            switch ((int)resp.StatusCode)
            {
                case 200:
                    _logger.LogInformation("ClubLog ✓ {Call}: {Msg}", qso.Call, body);
                    return ClubLogResult.Ok;

                case 403:
                    _suspended = true;
                    _logger.LogError("ClubLog 403: credenciais recusadas. Envios suspensos para o ClubLog não bloquear o IP. " +
                                     "Verifique o e-mail, a App Password e o indicativo no separador ClubLog. Resposta: {Msg}", body);
                    return ClubLogResult.Suspended;

                case >= 400 and < 500:
                    _logger.LogWarning("ClubLog recusou o QSO {Call} ({Code}): {Msg}", qso.Call, (int)resp.StatusCode, body);
                    return ClubLogResult.Rejected;

                default:
                    _logger.LogWarning("ClubLog respondeu {Code} ao QSO {Call}; fica na fila para mais tarde. Resposta: {Msg}",
                                       (int)resp.StatusCode, qso.Call, body);
                    return ClubLogResult.RetryLater;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning("ClubLog sem ligação ao enviar o QSO {Call} ({Error}); fica na fila para mais tarde.", qso.Call, ex.Message);
            return ClubLogResult.RetryLater;
        }
    }
}
