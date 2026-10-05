// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Net;

namespace LogBeam.Service.Models;

/// <summary>
/// Configurações gerais da aplicação
/// </summary>
public class AppSettings
{
    public string MyCallsign { get; set; } = string.Empty;
    public decimal MyLatitude { get; set; }
    public decimal MyLongitude { get; set; }
    public HamQthSettings HamQth { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public N1mmSettings N1mm { get; set; } = new();
    public WsjtxSettings Wsjtx { get; set; } = new();
    public Log4omSettings Log4om { get; set; } = new();
    public TelemetrySettings Telemetry { get; set; } = new();
    public ClubLogSettings ClubLog { get; set; } = new();
    public string LogLevel { get; set; } = "Information";
    public string LogPath { get; set; } = "logs/logbeam.log";
}

/// <summary>
/// Configurações do listener UDP do N1MM+
/// </summary>
public class N1mmSettings
{
    public int UdpPort { get; set; } = 12060;

    /// <summary>127.0.0.1 = só este computador; 0.0.0.0 = toda a rede local.</summary>
    public string ListenAddress { get; set; } = UdpListenAddress.Default;
}

/// <summary>
/// Configurações do listener UDP do WSJT-X (também usado pelo JTDX e compatíveis).
/// </summary>
public class WsjtxSettings
{
    public bool Enabled { get; set; } = false;
    public int  UdpPort { get; set; } = 2237;

    /// <summary>127.0.0.1 = só este computador; 0.0.0.0 = toda a rede local.</summary>
    public string ListenAddress { get; set; } = UdpListenAddress.Default;
}

/// <summary>
/// Configurações do receptor de ADIF em texto por UDP, usado pelo Log4OM
/// (ligação UDP OUTBOUND, mensagem ADIF_MESSAGE, "Broadcast" desligado, destino 127.0.0.1).
/// </summary>
public class Log4omSettings
{
    public bool Enabled { get; set; } = false;
    public int  UdpPort { get; set; } = 2333;

    /// <summary>127.0.0.1 = só este computador; 0.0.0.0 = toda a rede local.</summary>
    public string ListenAddress { get; set; } = UdpListenAddress.Default;
}

/// <summary>
/// Dados da instalação e relatórios de erros enviados ao LogBeam (4.26, 4.27) — só com
/// consentimento explícito do operador.
/// </summary>
public class TelemetrySettings
{
    /// <summary>null = ainda não foi perguntado; true/false = resposta do operador.</summary>
    public bool? Consent { get; set; }

    /// <summary>UUID gerado quando o operador aceita; apagado quando retira o consentimento.</summary>
    public string InstallationId { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsActive => Consent == true && !string.IsNullOrWhiteSpace(InstallationId);
}

/// <summary>
/// Configurações para API HamQTH (hamqth.com)
/// </summary>
public class HamQthSettings
{
    /// <summary>
    /// Username HamQTH
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password HamQTH (será cifrado com DPAPI antes de guardar)
    /// </summary>
    public string PasswordEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Cache TTL em minutos (quantos minutos guardar dados de QTH em cache)
    /// </summary>
    public int CacheTtlMinutes { get; set; } = 1440; // 24 horas
}

/// <summary>
/// Configurações para API Web (LogBeam backend)
/// </summary>
public class ApiSettings
{
    public const string DefaultBaseUrl = "https://api.logbeam.org";

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// Logbooks LogBeam para onde enviar cada QSO. Cada perfil activo recebe uma
    /// cópia do QSO — útil para expedições (logbook da expedição + pessoal em simultâneo).
    /// </summary>
    public List<ApiProfile> Profiles { get; set; } = new();

    /// <summary>
    /// Timeout em segundos para chamadas HTTP
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número de tentativas de retry em caso de falha
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Tempo inicial de espera em segundos (exponencial: 2s, 4s, 8s)
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
}

/// <summary>
/// Um logbook LogBeam de destino (Instance ID + API Key).
/// </summary>
public class ApiProfile
{
    public string Id         { get; set; } = Guid.NewGuid().ToString("N");
    public string Name       { get; set; } = string.Empty;

    /// <summary>Identificador do logbook (10 chars hex, visível no URL view.php?id=...)</summary>
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>Chave de API gerada em app.logbeam.org → Gerir Logbook → API Keys</summary>
    public string ApiKey     { get; set; } = string.Empty;

    public bool   Enabled    { get; set; } = true;
}

/// <summary>
/// Configurações para upload em tempo real para o ClubLog
/// </summary>
public class ClubLogSettings
{
    public bool Enabled { get; set; } = false;
    public string Email { get; set; } = string.Empty;
    /// <summary>Password cifrada com DPAPI</summary>
    public string PasswordEncrypted { get; set; } = string.Empty;
    /// <summary>Callsign a usar; se vazio, usa MyCallsign</summary>
    public string Callsign { get; set; } = string.Empty;
}
