// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Globalization;

namespace LogBeam.Core.Models;

/// <summary>
/// QSO recebido de um programa de logging (N1MM+, WSJT-X, Log4OM).
/// </summary>
public class QsoRecord
{
    public string Id { get; set; } = string.Empty;
    public string MyCall { get; set; } = string.Empty;
    public string Call { get; set; } = string.Empty;
    public string Band { get; set; } = string.Empty;
    public string Freq { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;

    /// <summary>Data UTC do QSO, AAAAMMDD.</summary>
    public string QsoDate { get; set; } = string.Empty;

    /// <summary>Hora UTC do início do QSO, HHMM ou HHMMSS.</summary>
    public string TimeOn { get; set; } = string.Empty;

    public string RstSent { get; set; } = string.Empty;
    public string RstRcvd { get; set; } = string.Empty;
    public string GridSquare { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TxPower { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string ContestName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public N1mmEventType EventType { get; set; } = N1mmEventType.Add;

    /// <summary>Hora a que o pacote chegou ao Uplink — pode estar um ou mais minutos depois do QSO.</summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    private static readonly string[] QsoTimeFormats = { "yyyyMMddHHmmss", "yyyyMMddHHmm" };

    /// <summary>
    /// Hora UTC do QSO, tal como o programa de logging a registou (QsoDate + TimeOn).
    /// Só quando falta ou é inválida se usa ReceivedAt.
    /// </summary>
    public DateTime QsoTimeUtc() =>
        DateTime.TryParseExact(QsoDate.Trim() + TimeOn.Trim(), QsoTimeFormats, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var t)
            ? t
            : ReceivedAt.ToUniversalTime();
}

public enum N1mmEventType
{
    Add,
    Update,
    Delete
}
