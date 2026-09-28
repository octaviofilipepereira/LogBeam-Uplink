// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Globalization;

namespace LogBeam.Core.Radio;

/// <summary>
/// Tabela única de bandas (ADIF 3.1). Converte o valor de banda enviado pelos programas de
/// logging — ADIF ("20m", "20M") ou N1MM+ (início da banda em MHz: "14", "3.5", "420") — e as
/// frequências em MHz para a banda ADIF, em minúsculas, como a API do LogBeam a espera.
/// </summary>
public static class BandPlan
{
    private static readonly (string Band, double LowMHz, double HighMHz)[] Bands =
    {
        ("2190m", 0.1357, 0.1378), ("630m", 0.472, 0.479), ("560m", 0.501, 0.504),
        ("160m", 1.8, 2.0),        ("80m", 3.5, 4.0),      ("60m", 5.06, 5.45),
        ("40m", 7.0, 7.3),         ("30m", 10.1, 10.15),   ("20m", 14.0, 14.35),
        ("17m", 18.068, 18.168),   ("15m", 21.0, 21.45),   ("12m", 24.89, 24.99),
        ("10m", 28.0, 29.7),       ("8m", 40.0, 45.0),     ("6m", 50.0, 54.0),
        ("5m", 54.000001, 69.9),   ("4m", 70.0, 71.0),     ("2m", 144.0, 148.0),
        ("1.25m", 222.0, 225.0),   ("70cm", 420.0, 450.0), ("33cm", 902.0, 928.0),
        ("23cm", 1240.0, 1300.0),  ("13cm", 2300.0, 2450.0), ("9cm", 3300.0, 3500.0),
        ("6cm", 5650.0, 5925.0),   ("3cm", 10000.0, 10500.0),
    };

    // Valores do campo <band> do N1MM+. Alguns ficam fora dos limites ADIF (ex. "5" nos 60 m),
    // por isso não chega converter o número como frequência.
    private static readonly Dictionary<string, string> N1mmBands = new()
    {
        ["1.8"] = "160m", ["3.5"] = "80m", ["5"] = "60m",  ["7"] = "40m",   ["10"] = "30m",
        ["14"] = "20m",   ["18"] = "17m",  ["21"] = "15m", ["24"] = "12m",  ["28"] = "10m",
        ["50"] = "6m",    ["70"] = "4m",   ["144"] = "2m", ["222"] = "1.25m", ["420"] = "70cm",
        ["902"] = "33cm", ["1240"] = "23cm", ["2300"] = "13cm", ["3300"] = "9cm",
        ["5650"] = "6cm", ["10000"] = "3cm",
    };

    /// <summary>Banda ADIF de uma frequência em MHz; vazio se não pertencer a nenhuma banda.</summary>
    public static string FromFrequencyMHz(double mhz)
    {
        foreach (var (band, low, high) in Bands)
            if (mhz >= low && mhz <= high) return band;
        return string.Empty;
    }

    /// <summary>
    /// Banda ADIF a partir do valor de banda do programa; se não for reconhecido, a partir da
    /// frequência (MHz, ponto decimal). Sem nenhum dos dois, devolve o valor original em minúsculas.
    /// </summary>
    public static string Resolve(string? band, string? freqMHz = null)
    {
        var b = band?.Trim().ToLowerInvariant() ?? string.Empty;

        if (b.Length > 0)
        {
            if (Bands.Any(x => x.Band == b)) return b;
            if (N1mmBands.TryGetValue(b, out var n1mm)) return n1mm;
            if (TryParseMHz(b, out var bandMHz))
            {
                var fromBand = FromFrequencyMHz(bandMHz);
                if (fromBand.Length > 0) return fromBand;
            }
        }

        if (TryParseMHz(freqMHz, out var mhz))
        {
            var fromFreq = FromFrequencyMHz(mhz);
            if (fromFreq.Length > 0) return fromFreq;
        }

        return b;
    }

    private static bool TryParseMHz(string? value, out double mhz) =>
        double.TryParse(value?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out mhz) && mhz > 0;
}
