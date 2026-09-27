// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Models;

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
        AddField(parts, "BAND", NormaliseBand(qso.Band));
        AddField(parts, "MODE", NormaliseMode(qso.Mode));
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

    /// <summary>
    /// Normalises N1MM band names to ADIF standard (e.g. "20M" → "20m").
    /// </summary>
    private static string NormaliseBand(string band) =>
        band.ToUpperInvariant() switch
        {
            // Nomes ADIF standard (ex: "20M")
            "160M" => "160m",
            "80M"  => "80m",
            "60M"  => "60m",
            "40M"  => "40m",
            "30M"  => "30m",
            "20M"  => "20m",
            "17M"  => "17m",
            "15M"  => "15m",
            "12M"  => "12m",
            "10M"  => "10m",
            "6M"   => "6m",
            "2M"   => "2m",
            "70CM" => "70cm",
            // Valores numéricos MHz enviados pelo N1MM+ no campo <band>
            "1.8"  => "160m",
            "3.5" or "3.7" or "3" => "80m",
            "5"    => "60m",
            "7"    => "40m",
            "10"   => "30m",
            "14"   => "20m",
            "18"   => "17m",
            "21"   => "15m",
            "24"   => "12m",
            "28"   => "10m",
            "50"   => "6m",
            "144"  => "2m",
            "432" or "430" => "70cm",
            _      => band.ToLower()
        };

    /// <summary>
    /// Normalises N1MM mode names to ADIF standard (e.g. USB/LSB → SSB).
    /// </summary>
    private static string NormaliseMode(string mode) =>
        mode.ToUpperInvariant() switch
        {
            "USB" or "LSB" => "SSB",
            "CWU" or "CWL" => "CW",
            "RTTY" or "RTTYR" => "RTTY",
            "FT8"  => "FT8",
            "FT4"  => "FT4",
            "PSK31" or "PSK63" => "PSK31",
            "AM"   => "AM",
            "FM"   => "FM",
            _      => mode.ToUpperInvariant()
        };
}
