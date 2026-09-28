// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.RegularExpressions;
using LogBeam.Core.Models;
using LogBeam.Core.Radio;

namespace LogBeam.Core.Adif;

/// <summary>
/// Lê registos ADIF em texto ("&lt;TAG:len&gt;valor"), como os que o WSJT-X (mensagem "Logged ADIF")
/// e o Log4OM (ligação ADIF_MESSAGE) enviam por UDP.
/// </summary>
public static class AdifRecordParser
{
    private static readonly Regex FieldRegex =
        new(@"<(\w+):(\d+)(?::[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EndOfRecordRegex =
        new(@"<eor>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Registos contidos num texto ADIF: ignora o cabeçalho (até &lt;EOH&gt;, se existir) e separa
    /// por &lt;EOR&gt;. Um texto sem &lt;EOR&gt; conta como um único registo.
    /// </summary>
    public static IEnumerable<string> SplitRecords(string adif)
    {
        var eoh = adif.IndexOf("<eoh>", StringComparison.OrdinalIgnoreCase);
        var body = eoh >= 0 ? adif[(eoh + 5)..] : adif;

        foreach (var record in EndOfRecordRegex.Split(body))
            if (!string.IsNullOrWhiteSpace(record))
                yield return record;
    }

    /// <summary>
    /// Campos de um registo, com nomes sem distinção de maiúsculas. A leitura avança pelo
    /// comprimento declarado de cada valor, para um valor que contenha "&lt;" não ser lido como campo.
    /// </summary>
    public static Dictionary<string, string> ReadFields(string record)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var m = FieldRegex.Match(record);

        while (m.Success)
        {
            var next = m.Index + m.Length;
            if (int.TryParse(m.Groups[2].Value, out var len) && next + len <= record.Length)
            {
                fields[m.Groups[1].Value] = record.Substring(next, len);
                next += len;
            }
            m = FieldRegex.Match(record, next);
        }

        return fields;
    }

    /// <summary>QSO a partir de um registo ADIF; null se não tiver CALL.</summary>
    public static QsoRecord? Parse(string record)
    {
        var f = ReadFields(record);
        if (!f.TryGetValue("CALL", out var call) || string.IsNullOrWhiteSpace(call))
            return null;

        var freq = Get(f, "FREQ");

        return new QsoRecord
        {
            Call       = call.Trim().ToUpperInvariant(),
            MyCall     = Get(f, "STATION_CALLSIGN"),
            Band       = BandPlan.Resolve(Get(f, "BAND"), freq),
            Mode       = ModeMap.Normalize(Get(f, "MODE"), Get(f, "SUBMODE")),
            Freq       = freq,
            QsoDate    = Get(f, "QSO_DATE"),
            TimeOn     = Get(f, "TIME_ON"),
            RstSent    = Get(f, "RST_SENT"),
            RstRcvd    = Get(f, "RST_RCVD"),
            GridSquare = Get(f, "GRIDSQUARE"),
            Name       = Get(f, "NAME"),
            TxPower    = Get(f, "TX_PWR"),
            Comment    = Get(f, "COMMENT"),
            Operator   = Get(f, "OPERATOR"),
            EventType  = N1mmEventType.Add
        };
    }

    private static string Get(Dictionary<string, string> fields, string name) =>
        fields.TryGetValue(name, out var v) ? v.Trim() : string.Empty;
}
