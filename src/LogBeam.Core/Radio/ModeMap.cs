// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Radio;

/// <summary>
/// Tabela única de modos. Os programas de logging mandam o modo de formas diferentes:
/// o WSJT-X regista o FT4 como MODE=MFSK SUBMODE=FT4, o Log4OM manda MODE=SSB SUBMODE=USB,
/// o N1MM+ manda USB/LSB. O LogBeam quer o modo que o operador reconhece (FT4, SSB, CW...).
/// </summary>
public static class ModeMap
{
    /// <summary>
    /// Modo a enviar, em maiúsculas: o SUBMODE quando existir (USB/LSB passam a SSB);
    /// senão o MODE, com as variantes dos programas convertidas (USB/LSB → SSB, CWU/CWL → CW, RTTYR → RTTY).
    /// </summary>
    public static string Normalize(string? mode, string? submode = null)
    {
        var sub = submode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (sub.Length > 0)
            return sub is "USB" or "LSB" ? "SSB" : sub;

        var m = mode?.Trim().ToUpperInvariant() ?? string.Empty;
        return m switch
        {
            "USB" or "LSB" => "SSB",
            "CWU" or "CWL" => "CW",
            "RTTYR"        => "RTTY",
            _              => m
        };
    }
}
