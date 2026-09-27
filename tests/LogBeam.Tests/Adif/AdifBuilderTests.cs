// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Adif;
using LogBeam.Core.Models;

namespace LogBeam.Tests.Adif;

public class AdifBuilderTests
{
    private static QsoRecord MakeQso(string band = "20M", string mode = "USB") => new()
    {
        MyCall  = "CT7BFV",
        Call    = "CT7XYZ",
        Band    = band,
        Mode    = mode,
        Freq    = "14.200",
        QsoDate = "20260719",
        TimeOn  = "1930",
        RstSent = "59",
        RstRcvd = "59"
    };

    [Theory]
    [InlineData("20M", "20m")]
    [InlineData("14", "20m")]
    [InlineData("70CM", "70cm")]
    [InlineData("432", "70cm")]
    [InlineData("144", "2m")]
    public void Build_NormalisesBand(string inputBand, string expectedAdifBand)
    {
        var adif = AdifBuilder.Build(MakeQso(band: inputBand));

        Assert.Contains($"<BAND:{expectedAdifBand.Length}>{expectedAdifBand}", adif);
    }

    [Theory]
    [InlineData("USB", "SSB")]
    [InlineData("LSB", "SSB")]
    [InlineData("CWU", "CW")]
    [InlineData("CWL", "CW")]
    [InlineData("FT8", "FT8")]
    [InlineData("PSK63", "PSK31")]
    public void Build_NormalisesMode(string inputMode, string expectedAdifMode)
    {
        var adif = AdifBuilder.Build(MakeQso(mode: inputMode));

        Assert.Contains($"<MODE:{expectedAdifMode.Length}>{expectedAdifMode}", adif);
    }

    [Fact]
    public void Build_IncludesRequiredCoreFields()
    {
        var adif = AdifBuilder.Build(MakeQso());

        Assert.Contains("<CALL:6>CT7XYZ", adif);
        Assert.Contains("<STATION_CALLSIGN:6>CT7BFV", adif);
        Assert.Contains("<QSO_DATE:8>20260719", adif);
        Assert.Contains("<TIME_ON:4>1930", adif);
        Assert.Contains("<RST_SENT:2>59", adif);
        Assert.Contains("<RST_RCVD:2>59", adif);
        Assert.EndsWith("<EOR>", adif);
    }

    [Fact]
    public void Build_OmitsEmptyOptionalFields()
    {
        var qso = MakeQso();
        qso.GridSquare = "";
        qso.Name       = "";
        qso.Comment    = "";

        var adif = AdifBuilder.Build(qso);

        Assert.DoesNotContain("<GRIDSQUARE", adif);
        Assert.DoesNotContain("<NAME", adif);
        Assert.DoesNotContain("<COMMENT", adif);
    }

    [Fact]
    public void Build_IncludesOptionalFieldsWhenPresent()
    {
        var qso = MakeQso();
        qso.GridSquare = "IM58";
        qso.Name       = "Octávio";

        var adif = AdifBuilder.Build(qso);

        Assert.Contains("<GRIDSQUARE:4>IM58", adif);
        Assert.Contains($"<NAME:{qso.Name.Length}>{qso.Name}", adif);
    }

    [Fact]
    public void Build_FallsBackToReceivedAt_WhenN1mmSendsNoDateTime()
    {
        var qso = MakeQso();
        qso.QsoDate    = "";
        qso.TimeOn     = "";
        qso.ReceivedAt = new DateTime(2026, 7, 19, 21, 5, 0, DateTimeKind.Utc);

        var adif = AdifBuilder.Build(qso);

        Assert.Contains("<QSO_DATE:8>20260719", adif);
        Assert.Contains("<TIME_ON:4>2105", adif);
    }
}
