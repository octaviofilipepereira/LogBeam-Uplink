// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Xml;
using LogBeam.Service.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogBeam.Service.Services;

/// <summary>
/// Serviço para fazer lookup de QTH (localização) via API HamQTH (hamqth.com).
/// Usa as credenciais configuradas na secção "HamQth" do settings.json.
/// </summary>
public class HamQthLookupService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AppSettings> _settings;
    private readonly ILogger<HamQthLookupService> _logger;

    // Cache de QTH para evitar chamadas repetidas
    private readonly Dictionary<string, (QthLocation qth, DateTime expires)> _cache = new();

    // Cache da session key HamQTH (válida ~20 min)
    private string? _sessionKey;
    private DateTime _sessionKeyExpires = DateTime.MinValue;

    public HamQthLookupService(HttpClient httpClient, IOptions<AppSettings> settings, ILogger<HamQthLookupService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        // Configurar timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(settings.Value.Api.TimeoutSeconds);
    }

    /// <summary>
    /// Faz lookup de QTH para um callsign
    /// Usa cache para evitar chamadas repetidas
    /// </summary>
    public async Task<QthLocation?> LookupQthAsync(string callsign, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(callsign))
        {
            _logger.LogWarning("Callsign vazio fornecido para lookup");
            return null;
        }

        // Normalizar callsign
        var normalizedCallsign = callsign.ToUpperInvariant();

        // Verificar cache
        if (_cache.TryGetValue(normalizedCallsign, out var cached))
        {
            if (DateTime.UtcNow < cached.expires)
            {
                _logger.LogDebug("QTH para {Callsign} obtido do cache", normalizedCallsign);
                return cached.qth;
            }
            else
            {
                _cache.Remove(normalizedCallsign);
            }
        }

        // Fazer lookup via HamQTH API
        try
        {
            var qth = await QueryHamQthAsync(normalizedCallsign, ct);

            if (qth != null)
            {
                // Guardar em cache
                var ttl = _settings.Value.HamQth.CacheTtlMinutes;
                _cache[normalizedCallsign] = (qth, DateTime.UtcNow.AddMinutes(ttl));

                _logger.LogInformation("QTH encontrado para {Callsign}: {City}, {Country}", normalizedCallsign, qth.City, qth.Country);
            }
            else
            {
                _logger.LogWarning("QTH não encontrado para {Callsign}", normalizedCallsign);
            }

            return qth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer lookup QTH para {Callsign}", normalizedCallsign);
            return null;
        }
    }

    /// <summary>
    /// Query à API HamQTH
    /// </summary>
    private async Task<QthLocation?> QueryHamQthAsync(string callsign, CancellationToken ct)
    {
        var username = _settings.Value.HamQth.Username;
        var password = _settings.Value.HamQth.PasswordEncrypted;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("Credenciais HamQTH não configuradas");
            return null;
        }

        try
        {
            var sessionKey = await GetHamQthSessionKeyAsync(username, password, ct);
            if (string.IsNullOrEmpty(sessionKey))
            {
                _logger.LogWarning("Falha ao obter session key HamQTH");
                return null;
            }

            return await QueryHamQthCallsignAsync(sessionKey, callsign, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na query HamQTH para {Callsign}", callsign);
            return null;
        }
    }

    /// <summary>
    /// Obtém session key da API HamQTH (com cache de 20 minutos)
    /// </summary>
    private async Task<string?> GetHamQthSessionKeyAsync(string username, string password, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_sessionKey) && DateTime.UtcNow < _sessionKeyExpires)
            return _sessionKey;

        _sessionKey = null;
        var url = $"https://www.hamqth.com/xml.php?u={Uri.EscapeDataString(username)}&p={Uri.EscapeDataString(password)}&prg=LogBeam";

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HamQTH login falhou com status {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var doc = new XmlDocument();
            doc.LoadXml(content);

            var errorNode = doc.SelectSingleNode("//*[local-name()='error']");
            if (errorNode != null)
            {
                _logger.LogWarning("HamQTH login erro: {Error}", errorNode.InnerText);
                return null;
            }

            var keyNode = doc.SelectSingleNode("//*[local-name()='session_id']");
            if (keyNode == null || string.IsNullOrEmpty(keyNode.InnerText))
            {
                _logger.LogWarning("HamQTH login não retornou session_id");
                return null;
            }

            _sessionKey = keyNode.InnerText;
            _sessionKeyExpires = DateTime.UtcNow.AddMinutes(20);
            _logger.LogInformation("HamQTH session key obtida com sucesso");
            return _sessionKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter session key HamQTH");
            return null;
        }
    }

    /// <summary>
    /// Query callsign via HamQTH com session key
    /// </summary>
    private async Task<QthLocation?> QueryHamQthCallsignAsync(string sessionKey, string callsign, CancellationToken ct)
    {
        var url = $"https://www.hamqth.com/xml.php?id={Uri.EscapeDataString(sessionKey)}&callsign={Uri.EscapeDataString(callsign)}&prg=LogBeam";

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HamQTH query para {Callsign} falhou com status {StatusCode}", callsign, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var doc = new XmlDocument();
            doc.LoadXml(content);

            var errorNode = doc.SelectSingleNode("//*[local-name()='error']");
            if (errorNode != null)
            {
                if (errorNode.InnerText.Contains("session", StringComparison.OrdinalIgnoreCase) ||
                    errorNode.InnerText.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    _sessionKey = null;
                }
                _logger.LogWarning("HamQTH erro para {Callsign}: {Error}", callsign, errorNode.InnerText);
                return null;
            }

            // HamQTH usa <latitude>, <longitude>, <adr_city>, <country>
            var latNode     = doc.SelectSingleNode("//*[local-name()='latitude']");
            var lonNode     = doc.SelectSingleNode("//*[local-name()='longitude']");
            var cityNode    = doc.SelectSingleNode("//*[local-name()='adr_city']");
            var countryNode = doc.SelectSingleNode("//*[local-name()='country']");

            if (latNode == null || lonNode == null ||
                !decimal.TryParse(latNode.InnerText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) ||
                !decimal.TryParse(lonNode.InnerText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon))
            {
                _logger.LogWarning("HamQTH não retornou coordenadas válidas para {Callsign} (lat={Lat}, lon={Lon})",
                    callsign, latNode?.InnerText ?? "(null)", lonNode?.InnerText ?? "(null)");
                return null;
            }

            return new QthLocation
            {
                Callsign = callsign,
                Latitude = lat,
                Longitude = lon,
                City    = cityNode?.InnerText ?? string.Empty,
                Country = countryNode?.InnerText ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer query HamQTH para {Callsign}", callsign);
            return null;
        }
    }
}
