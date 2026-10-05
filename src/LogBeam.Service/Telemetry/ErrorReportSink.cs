// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Serilog.Core;
using Serilog.Events;

namespace LogBeam.Service.Telemetry;

/// <summary>
/// Recebe do Serilog os erros da aplicação (nível Error ou Fatal) e guarda-os para envio.
/// Só é ligado quando o operador deu consentimento. A mensagem é o modelo do log (sem os valores),
/// o que agrupa ocorrências iguais e evita enviar dados de QSOs.
/// </summary>
public sealed class ErrorReportSink : ILogEventSink
{
    private readonly TelemetryStore _store;

    public ErrorReportSink(TelemetryStore store) => _store = store;

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Error) return;

        var source = logEvent.Properties.TryGetValue("SourceContext", out var sc)
            ? sc.ToString().Trim('"')
            : string.Empty;
        if (source.Contains(".Telemetry.", StringComparison.Ordinal)) return;   // não reportar falhas do próprio envio

        var ex      = logEvent.Exception;
        var message = ex is null
            ? logEvent.MessageTemplate.Text
            : $"{logEvent.MessageTemplate.Text} | {ex.Message}";

        _store.Add(ComponentOf(source, ex), ex?.GetType().FullName ?? "LogError", message, ex?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Como <see cref="ComponentOf(string)"/>; se a origem for genérica (ex. o QsoProcessor a arrancar
    /// o receptor do N1MM+), usa a primeira classe do LogBeam onde a excepção foi lançada.
    /// </summary>
    public static string ComponentOf(string sourceContext, Exception? ex)
    {
        var component = ComponentOf(sourceContext);
        if (component != "Service" || ex is null) return component;

        var thrower = new System.Diagnostics.StackTrace(ex, false).GetFrames()
            .Select(f => f.GetMethod()?.DeclaringType?.FullName)
            .FirstOrDefault(n => n is not null && n.StartsWith("LogBeam.", StringComparison.Ordinal));
        return thrower is null ? component : ComponentOf(thrower);
    }

    /// <summary>Componente do contrato (ApiClient, N1MM, WSJTX, Log4OM, ClubLog, UI, Service) a partir da origem do log.</summary>
    public static string ComponentOf(string sourceContext) => sourceContext switch
    {
        var s when s.Contains("ApiClient", StringComparison.Ordinal)       => "ApiClient",
        var s when s.Contains(".N1MM.", StringComparison.Ordinal)          => "N1MM",
        var s when s.Contains(".Wsjtx.", StringComparison.Ordinal)         => "WSJTX",
        var s when s.Contains("AdifUdpListener", StringComparison.Ordinal) => "Log4OM",
        var s when s.Contains("ClubLog", StringComparison.Ordinal)         => "ClubLog",
        var s when s.StartsWith("LogBeam.UI", StringComparison.Ordinal)    => "UI",
        _                                                                  => "Service"
    };
}
