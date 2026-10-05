// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Text.Json;
using LogBeam.Core.Models;
using LogBeam.Service.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using ServiceAppSettings = LogBeam.Service.Models.AppSettings;

namespace LogBeam.Service.Services;

/// <summary>
/// Resultado do envio de um QSO para um logbook LogBeam.
/// IsPermanentFailure distingue erros que reenviar nunca vai resolver (4xx —
/// QSO inválido, limite atingido, API Key revogada) de falhas transitórias
/// (rede em baixo, 5xx, 429) que valem a pena voltar a tentar mais tarde.
/// Duplicate: aceite, mas o QSO já existia no logbook (o servidor não o acrescentou).
/// </summary>
public record QsoSendResult(bool Success, bool ConfirmedLogbeam, bool IsPermanentFailure = false, bool Duplicate = false);

/// <summary>
/// Serviço para enviar QSOs para a API REST do LogBeam backend
/// </summary>
public class ApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<ServiceAppSettings> _settings;
    private readonly ILogger<ApiClientService> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public ApiClientService(HttpClient httpClient, IOptions<ServiceAppSettings> settings, ILogger<ApiClientService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        // Configurar timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(settings.Value.Api.TimeoutSeconds);

        // Setup retry policy (Polly) — só para falhas transitórias. Erros 4xx
        // (exceptuando 429) nunca vão mudar por repetir o pedido: um QSO
        // inválido ou uma API Key revogada continuam inválidos na 2ª e 3ª
        // tentativa, e o único efeito de os incluir era queimar 3 tentativas
        // e (antes desta correcção) acabar sempre na fila offline, onde
        // bloqueavam todos os QSOs seguintes desse perfil para sempre.
        //
        // Um 429 respeita o Retry-After (4.15): espera o tempo pedido pelo servidor, até
        // MaxRetryAfter; acima disso não se repete aqui e o QSO vai para a fila offline.
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && IsTransientStatus(r.StatusCode)
                                                && !(RetryAfter(r) > MaxRetryAfter))
            .WaitAndRetryAsync(
                retryCount: settings.Value.Api.RetryCount,
                sleepDurationProvider: (attempt, outcome, _) =>
                    (outcome.Result is { } r ? RetryAfter(r) : null)
                    ?? TimeSpan.FromSeconds(settings.Value.Api.RetryDelaySeconds * Math.Pow(2, attempt - 1)),
                onRetryAsync: (outcome, timespan, attempt, _) =>
                {
                    _logger.LogWarning("Retry {Attempt}/{Max} após {Delay}ms. Última tentativa: {Outcome}",
                        attempt, settings.Value.Api.RetryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    return Task.CompletedTask;
                });
    }

    /// <summary>Espera máxima por um Retry-After antes de desistir e deixar o QSO na fila offline.</summary>
    public static readonly TimeSpan MaxRetryAfter = TimeSpan.FromSeconds(60);

    /// <summary>Tempo pedido pelo servidor num 429 (Retry-After em segundos ou em data); null se não houver.</summary>
    internal static TimeSpan? RetryAfter(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.TooManyRequests || response.Headers.RetryAfter is not { } ra) return null;
        if (ra.Delta is { } delta) return delta;
        if (ra.Date is { } date) { var wait = date - DateTimeOffset.UtcNow; return wait > TimeSpan.Zero ? wait : TimeSpan.Zero; }
        return null;
    }

    /// <summary>
    /// Envia QSO para um logbook LogBeam específico. A resolução de HamQTH/DXCC é feita pelo servidor.
    /// </summary>
    public async Task<QsoSendResult> SendQsoAsync(QsoRecord qso, ApiProfile profile, CancellationToken ct)
    {
        if (!ValidateProfile(profile))
            // Perfil mal configurado (URL/Instance ID/API Key em falta) nunca se
            // resolve reenviando — permanente, para não entupir a fila offline.
            return new QsoSendResult(false, false, IsPermanentFailure: true);

        try
        {
            var payload = new
            {
                callsign = qso.Call,
                band = qso.Band,
                mode = qso.Mode,
                freq = string.IsNullOrEmpty(qso.Freq) ? (double?)null : double.TryParse(qso.Freq, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : (double?)null,
                // Hora do próprio QSO, em UTC: a confirmação LogBeam-a-LogBeam compara horas e os
                // duplicados são detectados ao minuto. InvariantCulture garante ":" como separador.
                date_time = qso.QsoTimeUtc().ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                rst_sent = qso.RstSent,
                rst_received = qso.RstRcvd
            };

            var url = $"{_settings.Value.Api.BaseUrl}/api/instance/{profile.InstanceId}/qso";

            _logger.LogDebug("Enviando QSO para {Callsign} para {Url} (perfil {Profile})", qso.Call, url, profile.Name);

            // Fazer requisição com retry policy
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json")
                };
                request.Headers.Add("X-API-Key", profile.ApiKey);

                return await _httpClient.SendAsync(request, ct);
            });

            var content = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                var confirmed = ReadDataFlag(content, "confirmed_logbeam");
                var duplicate = ReadDataFlag(content, "duplicate");
                if (duplicate)
                    _logger.LogInformation("QSO já existia no logbook: {Callsign} (perfil {Profile})", qso.Call, profile.Name);
                else
                    _logger.LogInformation("QSO enviado com sucesso: {Callsign} (perfil {Profile})", qso.Call, profile.Name);
                return new QsoSendResult(true, confirmed, Duplicate: duplicate);
            }
            else
            {
                var permanent = !IsTransientStatus(response.StatusCode);
                _logger.LogError("Falha ao enviar QSO: {StatusCode} - {Content} (perfil {Profile}, {Kind})",
                    response.StatusCode, content, profile.Name, permanent ? "permanente" : "transitória");
                return new QsoSendResult(false, false, IsPermanentFailure: permanent);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;   // a aplicação está a parar
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Sem rede ou sem resposta (já esgotou os retries do Polly): o QSO fica na fila.
            // Aviso e não erro — é o ambiente do operador, não uma falha da aplicação.
            _logger.LogWarning("Sem ligação ao enviar QSO {Callsign} (perfil {Profile}): {Error}", qso.Call, profile.Name, ex.Message);
            return new QsoSendResult(false, false, IsPermanentFailure: false);
        }
        catch (Exception ex)
        {
            // Excepção inesperada — trata-se como transitória por omissão: não há forma de a
            // distinguir de um problema permanente, e o custo de errar para este lado é só
            // reenviar mais tarde, não perder o QSO.
            _logger.LogError(ex, "Erro ao enviar QSO para {Callsign} (perfil {Profile})", qso.Call, profile.Name);
            return new QsoSendResult(false, false, IsPermanentFailure: false);
        }
    }

    /// <summary>
    /// true para códigos de estado que vale a pena repetir (servidor/rede
    /// temporariamente indisponível); false para erros do próprio pedido
    /// (QSO inválido, limite atingido, API Key revogada), que vão continuar a
    /// falhar por muitas vezes que se tente.
    /// </summary>
    private static bool IsTransientStatus(HttpStatusCode statusCode)
        => (int)statusCode >= 500 || statusCode == HttpStatusCode.TooManyRequests;

    /// <summary>Lê um campo booleano de "data" na resposta (confirmed_logbeam, duplicate); false se faltar.</summary>
    private static bool ReadDataFlag(string jsonContent, string name)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.ValueKind == JsonValueKind.Object &&
                data.TryGetProperty(name, out var value))
                return value.ValueKind == JsonValueKind.True;
        }
        catch { /* resposta sem esse campo */ }
        return false;
    }

    /// <summary>
    /// Testa a ligação a um logbook: confirma que a instância existe e que a API Key é válida.
    /// </summary>
    public async Task<bool> TestConnectionAsync(ApiProfile profile, CancellationToken ct)
    {
        if (!ValidateProfile(profile))
            return false;

        try
        {
            // Endpoint dedicado (desde 2026-08-17) que exige mesmo uma X-API-Key
            // válida — GET /api/instance/{id} sozinho é público e respondia 200
            // mesmo com a chave errada, dando um falso "ligação OK".
            var url = $"{_settings.Value.Api.BaseUrl}/api/instance/{profile.InstanceId}/apikey/verify";

            _logger.LogInformation("Testando conexão com API em {Url}", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-Key", profile.ApiKey);

            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Conexão com API bem-sucedida");
                return true;
            }
            else
            {
                _logger.LogError("Teste de conexão falhou com status {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar conexão com API");
            return false;
        }
    }

    private bool ValidateProfile(ApiProfile profile)
    {
        if (string.IsNullOrEmpty(_settings.Value.Api.BaseUrl))
        {
            _logger.LogWarning("URL da API não configurada");
            return false;
        }

        if (string.IsNullOrEmpty(profile.InstanceId) || string.IsNullOrEmpty(profile.ApiKey))
        {
            _logger.LogWarning("Perfil {Profile} sem Instance ID / API Key configurados", profile.Name);
            return false;
        }

        return true;
    }
}
