// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Wsjtx;

namespace LogBeam.Tests.Wsjtx;

public class WsjtxUdpListenerTests
{
    [Fact]
    public void ParseAdifRecord_ExtractsCoreFields()
    {
        const string adif = "<call:6>CT7XYZ<band:3>20m<mode:3>FT8<freq:6>14.074<qso_date:8>20260719" +
                             "<time_on:4>1930<rst_sent:3>-10<rst_rcvd:3>-08<gridsquare:4>IM58<name:5>Bruno<eor>";

        var qso = WsjtxUdpListener.ParseAdifRecord(adif);

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
    public void ParseAdifRecord_ReturnsNull_WhenNoCallField()
    {
        const string adif = "<band:3>20m<mode:3>FT8<eor>";

        var qso = WsjtxUdpListener.ParseAdifRecord(adif);

        Assert.Null(qso);
    }

    [Fact]
    public void ParseAdifRecord_IgnoresUnknownFields()
    {
        const string adif = "<call:6>CT7XYZ<app_wsjtx_aptype:2>DX<eor>";

        var qso = WsjtxUdpListener.ParseAdifRecord(adif);

        Assert.NotNull(qso);
        Assert.Equal("CT7XYZ", qso!.Call);
    }

    [Fact]
    public void ParseAdifRecord_HandlesFieldsWithDataTypeIndicator()
    {
        // ADIF permite um terceiro segmento opcional: <FIELD:len:type>valor
        const string adif = "<call:6>CT7XYZ<freq:6:N>14.074<eor>";

        var qso = WsjtxUdpListener.ParseAdifRecord(adif);

        Assert.NotNull(qso);
        Assert.Equal("14.074", qso!.Freq);
    }
}
