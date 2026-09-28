// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Adif;

namespace LogBeam.Tests.Adif;

public class AdifRecordParserTests
{
    [Fact]
    public void Parse_ExtractsCoreFields()
    {
        const string adif = "<call:6>CT7XYZ<band:3>20m<mode:3>FT8<freq:6>14.074<qso_date:8>20260719" +
                             "<time_on:4>1930<rst_sent:3>-10<rst_rcvd:3>-08<gridsquare:4>IM58<name:5>Bruno<eor>";

        var qso = AdifRecordParser.Parse(adif);

        Assert.NotNull(qso);
        Assert.Equal("CT7XYZ", qso!.Call);
        Assert.Equal("20m", qso.Band);
        Assert.Equal("FT8", qso.Mode);
        Assert.Equal("14.074", qso.Freq);
        Assert.Equal("20260719", qso.QsoDate);
        Assert.Equal("1930", qso.TimeOn);
        Assert.Equal("-10", qso.RstSent);
        Assert.Equal("-08", qso.RstRcvd);
        Assert.Equal("IM58", qso.GridSquare);
        Assert.Equal("Bruno", qso.Name);
    }

    [Fact]
    public void Parse_ReturnsNull_WhenNoCallField()
    {
        Assert.Null(AdifRecordParser.Parse("<band:3>20m<mode:3>FT8<eor>"));
    }

    [Fact]
    public void Parse_IgnoresUnknownFields()
    {
        var qso = AdifRecordParser.Parse("<call:6>CT7XYZ<app_wsjtx_aptype:2>DX<eor>");

        Assert.Equal("CT7XYZ", qso!.Call);
    }

    [Fact]
    public void Parse_HandlesFieldsWithDataTypeIndicator()
    {
        // ADIF permite um terceiro segmento opcional: <FIELD:len:type>valor
        var qso = AdifRecordParser.Parse("<call:6>CT7XYZ<freq:6:N>14.074<eor>");

        Assert.Equal("14.074", qso!.Freq);
    }

    [Fact]
    public void Parse_ValueContainingFieldLikeText_IsNotReadAsField()
    {
        var qso = AdifRecordParser.Parse("<call:6>CT7XYZ<comment:10><mode:2>AM<mode:3>FT8<eor>");

        Assert.Equal("<mode:2>AM", qso!.Comment);
        Assert.Equal("FT8", qso.Mode);
    }

    [Fact]
    public void Parse_BandMissing_UsesFrequency()
    {
        var qso = AdifRecordParser.Parse("<call:6>CT7XYZ<freq:5>7.074<mode:3>FT8<eor>");

        Assert.Equal("40m", qso!.Band);
    }

    [Fact]
    public void SplitRecords_SkipsHeaderAndSplitsOnEor()
    {
        const string adif = "LogBeam export<adif_ver:5>3.1.0<EOH>\n<call:5>EA1AA<eor>\n<call:5>EA2BB<EOR>\n";

        var records = AdifRecordParser.SplitRecords(adif).Select(AdifRecordParser.Parse).ToList();

        Assert.Equal(new[] { "EA1AA", "EA2BB" }, records.Select(q => q!.Call));
    }
}
