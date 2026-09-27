// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Models;

/// <summary>
/// Represents a single QSO record parsed from N1MM+ UDP broadcast.
/// </summary>
public class QsoRecord
{
    public string Id { get; set; } = string.Empty;
    public string MyCall { get; set; } = string.Empty;
    public string Call { get; set; } = string.Empty;
    public string Band { get; set; } = string.Empty;
    public string Freq { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string QsoDate { get; set; } = string.Empty;
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
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

public enum N1mmEventType
{
    Add,
    Update,
    Delete
}
