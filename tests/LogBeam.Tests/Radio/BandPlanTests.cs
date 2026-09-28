// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Radio;

namespace LogBeam.Tests.Radio;

public class BandPlanTests
{
    [Theory]
    [InlineData("20m", "20m")]
    [InlineData("40M", "40m")]
    [InlineData("70CM", "70cm")]
    [InlineData("1.25m", "1.25m")]
    // Campo <band> do N1MM+ (início da banda em MHz)
    [InlineData("1.8", "160m")]
    [InlineData("3.5", "80m")]
    [InlineData("5", "60m")]
    [InlineData("10", "30m")]
    [InlineData("14", "20m")]
    [InlineData("28", "10m")]
    [InlineData("50", "6m")]
    [InlineData("144", "2m")]
    [InlineData("420", "70cm")]
    // Outros valores numéricos: tratados como frequência
    [InlineData("432", "70cm")]
    [InlineData("3.7", "80m")]
    public void Resolve_MapsBandValues_ToAdifBand(string band, string expected)
    {
        Assert.Equal(expected, BandPlan.Resolve(band));
    }

    [Theory]
    [InlineData(null, "14.074", "20m")]
    [InlineData("", "7.049515", "40m")]
    [InlineData("", "144.300", "2m")]
    [InlineData("desconhecida", "21.074", "15m")]
    public void Resolve_UsesFrequency_WhenBandMissingOrUnknown(string? band, string freq, string expected)
    {
        Assert.Equal(expected, BandPlan.Resolve(band, freq));
    }

    [Fact]
    public void Resolve_PrefersBandValue_OverFrequency()
    {
        Assert.Equal("20m", BandPlan.Resolve("20m", "7.074"));
    }

    [Theory]
    [InlineData(null, null, "")]
    [InlineData("", "", "")]
    [InlineData("XYZ", "", "xyz")]
    public void Resolve_WithoutUsableValues_ReturnsOriginal(string? band, string? freq, string expected)
    {
        Assert.Equal(expected, BandPlan.Resolve(band, freq));
    }

    [Theory]
    [InlineData(1.840, "160m")]
    [InlineData(10.136, "30m")]
    [InlineData(18.100, "17m")]
    [InlineData(24.915, "12m")]
    [InlineData(50.313, "6m")]
    [InlineData(432.200, "70cm")]
    [InlineData(1296.2, "23cm")]
    [InlineData(15.0, "")]
    public void FromFrequencyMHz_ReturnsAdifBand(double mhz, string expected)
    {
        Assert.Equal(expected, BandPlan.FromFrequencyMHz(mhz));
    }
}
