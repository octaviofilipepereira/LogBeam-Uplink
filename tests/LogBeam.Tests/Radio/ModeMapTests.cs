// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Radio;

namespace LogBeam.Tests.Radio;

public class ModeMapTests
{
    [Theory]
    [InlineData("USB", "SSB")]
    [InlineData("LSB", "SSB")]
    [InlineData("CW", "CW")]
    [InlineData("CWU", "CW")]
    [InlineData("RTTYR", "RTTY")]
    [InlineData("ft8", "FT8")]
    [InlineData("PSK63", "PSK63")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Normalize_Mode(string? mode, string expected)
    {
        Assert.Equal(expected, ModeMap.Normalize(mode));
    }

    [Theory]
    [InlineData("MFSK", "FT4", "FT4")]     // WSJT-X
    [InlineData("MFSK", "FST4", "FST4")]
    [InlineData("SSB", "USB", "SSB")]      // Log4OM
    [InlineData("SSB", "LSB", "SSB")]
    [InlineData("PSK", "psk31", "PSK31")]
    [InlineData("FT8", "", "FT8")]
    public void Normalize_UsesSubmode_WhenPresent(string mode, string submode, string expected)
    {
        Assert.Equal(expected, ModeMap.Normalize(mode, submode));
    }
}
