// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Models;
using LogBeam.Core.Radio;

namespace LogBeam.Core.Adif;

/// <summary>
/// Builds ADIF strings from QsoRecord objects.
/// </summary>
public static class AdifBuilder
{
    public static string Build(QsoRecord qso)
    {
        var parts = new List<string>();

        // Fallback para ReceivedAt quando N1MM não envia data/hora no XML
        var qsoDate = !string.IsNullOrWhiteSpace(qso.QsoDate)
            ? qso.QsoDate
            : qso.ReceivedAt.ToUniversalTime().ToString("yyyyMMdd");
        var timeOn = !string.IsNullOrWhiteSpace(qso.TimeOn)
            ? qso.TimeOn
            : qso.ReceivedAt.ToUniversalTime().ToString("HHmm");

        AddField(parts, "CALL", qso.Call);
        AddField(parts, "BAND", BandPlan.Resolve(qso.Band, qso.Freq));
        AddField(parts, "MODE", ModeMap.Normalize(qso.Mode));
        AddField(parts, "FREQ", qso.Freq);
        AddField(parts, "QSO_DATE", qsoDate);
        AddField(parts, "TIME_ON", timeOn);
        AddField(parts, "RST_SENT", qso.RstSent);
        AddField(parts, "RST_RCVD", qso.RstRcvd);
        AddField(parts, "STATION_CALLSIGN", qso.MyCall);

        if (!string.IsNullOrWhiteSpace(qso.GridSquare))
            AddField(parts, "GRIDSQUARE", qso.GridSquare);

        if (!string.IsNullOrWhiteSpace(qso.Name))
            AddField(parts, "NAME", qso.Name);

        if (!string.IsNullOrWhiteSpace(qso.TxPower))
            AddField(parts, "TX_PWR", qso.TxPower);

        if (!string.IsNullOrWhiteSpace(qso.Comment))
            AddField(parts, "COMMENT", qso.Comment);

        parts.Add("<EOR>");

        return string.Join("", parts);
    }

    private static void AddField(List<string> parts, string field, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        parts.Add($"<{field}:{value.Length}>{value}");
    }
}
