// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json.Serialization;

namespace LogBeam.Service.Telemetry;

/// <summary>
/// Dados da instalação (4.26) — corpo de POST /api/uplink/install. Só o necessário: nada de
/// nome do PC, utilizador, números de série nem IP.
/// </summary>
public record InstallInfo(
    [property: JsonPropertyName("installation_id")]  string InstallationId,
    [property: JsonPropertyName("callsign")]         string? Callsign,
    [property: JsonPropertyName("app_version")]      string AppVersion,
    [property: JsonPropertyName("dotnet_version")]   string DotnetVersion,
    [property: JsonPropertyName("os")]               string Os,
    [property: JsonPropertyName("os_arch")]          string OsArch,
    [property: JsonPropertyName("language")]         string Language,
    [property: JsonPropertyName("logging_programs")] IReadOnlyList<string> LoggingPrograms,
    [property: JsonPropertyName("destinations")]     InstallDestinations Destinations)
{
    /// <summary>Identifica o conteúdo, para só reenviar quando algum valor mudar.</summary>
    public string Fingerprint() =>
        string.Join("|", InstallationId, Callsign, AppVersion, DotnetVersion, Os, OsArch, Language,
                    string.Join(",", LoggingPrograms), Destinations.LogBeam, Destinations.ClubLog);
}

/// <summary>Destinos ligados (2.5.1): só sim ou não, sem os logbooks nem as contas.</summary>
public record InstallDestinations(
    [property: JsonPropertyName("logbeam")] bool LogBeam,
    [property: JsonPropertyName("clublog")] bool ClubLog);

/// <summary>Relatório de erro (4.27) — corpo de POST /api/uplink/error.</summary>
public record ErrorReport
{
    public const int MaxMessage    = 1000;
    public const int MaxStackTrace = 8000;

    [JsonPropertyName("installation_id")] public string InstallationId { get; init; } = string.Empty;
    [JsonPropertyName("callsign")]        public string? Callsign      { get; init; }
    [JsonPropertyName("app_version")]     public string AppVersion     { get; init; } = string.Empty;

    /// <summary>UTC, "AAAA-MM-DD HH:MM:SS".</summary>
    [JsonPropertyName("occurred_at")]     public string OccurredAt     { get; init; } = string.Empty;

    /// <summary>ApiClient, N1MM, WSJTX, Log4OM, ClubLog, Service ou UI.</summary>
    [JsonPropertyName("component")]       public string Component      { get; init; } = string.Empty;
    [JsonPropertyName("error_type")]      public string ErrorType      { get; init; } = string.Empty;
    [JsonPropertyName("message")]         public string Message        { get; init; } = string.Empty;
    [JsonPropertyName("stack_trace")]     public string StackTrace     { get; init; } = string.Empty;

    /// <summary>Ocorrências do mesmo erro agrupadas na última hora.</summary>
    [JsonPropertyName("count")]           public int Count             { get; init; } = 1;
}
