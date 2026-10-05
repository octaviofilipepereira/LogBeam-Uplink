// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Text;
using System.Text.Json;

namespace LogBeam.Service.Telemetry;

public enum TelemetrySendResult
{
    /// <summary>Aceite (200).</summary>
    Ok,
    /// <summary>Recusado por dados inválidos (4xx excepto 429): não adianta reenviar.</summary>
    Rejected,
    /// <summary>Sem rede, 5xx ou 429: tentar mais tarde.</summary>
    RetryLater
}

/// <summary>
/// Envia os dados da instalação e os relatórios de erros para {BaseUrl}/api/uplink
/// (POST /install, POST /error, DELETE /install/{id}). Sem API key, por contrato.
/// Respeita o Retry-After de uma resposta 429.
/// </summary>
public class TelemetryClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly Func<DateTime> _utcNow;

    /// <summary>Até quando não se deve enviar nada (429 com Retry-After).</summary>
    public DateTime? PausedUntilUtc { get; private set; }

    public TelemetryClient(HttpClient http, string apiBaseUrl, Func<DateTime>? utcNow = null)
    {
        _http    = http;
        _baseUrl = apiBaseUrl.TrimEnd('/') + "/api/uplink";
        _utcNow  = utcNow ?? (() => DateTime.UtcNow);
    }

    public Task<TelemetrySendResult> SendInstallAsync(InstallInfo info, CancellationToken ct) =>
        PostAsync("install", info, ct);

    public Task<TelemetrySendResult> SendErrorAsync(ErrorReport report, CancellationToken ct) =>
        PostAsync("error", report, ct);

    /// <summary>Pede ao servidor que apague a instalação e os erros associados (consentimento retirado).</summary>
    public async Task<TelemetrySendResult> DeleteInstallAsync(string installationId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/install/{Uri.EscapeDataString(installationId)}");
        return await SendAsync(request, ct);
    }

    private async Task<TelemetrySendResult> PostAsync<T>(string path, T body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{path}")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        return await SendAsync(request, ct);
    }

    private async Task<TelemetrySendResult> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (PausedUntilUtc is { } until && _utcNow() < until) return TelemetrySendResult.RetryLater;

        try
        {
            using var response = await _http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode) return TelemetrySendResult.Ok;

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromMinutes(5);
                PausedUntilUtc = _utcNow() + wait;
                return TelemetrySendResult.RetryLater;
            }

            return (int)response.StatusCode >= 500 ? TelemetrySendResult.RetryLater : TelemetrySendResult.Rejected;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return TelemetrySendResult.RetryLater;   // sem rede ou timeout
        }
    }
}
