// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Xml.Linq;
using LogBeam.Core.Models;
using LogBeam.Core.N1MM;
using Microsoft.Extensions.Logging.Abstractions;

namespace LogBeam.Tests.N1MM;

public class N1MMUdpListenerTests
{
    private static N1MMUdpListener MakeListener() =>
        new(NullLogger<N1MMUdpListener>.Instance);

    // ─── ParseFreqMHz ──────────────────────────────────────────────────────

    private static XElement MakeContactInfo(string? txfreq = null, string? rxfreq = null, string? freq = null)
    {
        var root = new XElement("contactinfo");
        if (txfreq != null) root.Add(new XElement("txfreq", txfreq));
        if (rxfreq != null) root.Add(new XElement("rxfreq", rxfreq));
        if (freq   != null) root.Add(new XElement("freq", freq));
        return root;
    }

    [Fact]
    public void ParseFreqMHz_ConvertsN1mmTensOfHzUnits_ToMHz()
    {
        var listener = MakeListener();
        var root = MakeContactInfo(txfreq: "1415420"); // N1MM+ envia em unidades de 10 Hz

        var result = listener.ParseFreqMHz(root);

        Assert.Equal("14.1542", result);
    }

    [Fact]
    public void ParseFreqMHz_PrefersTxfreq_OverRxfreqAndFreq()
    {
        var listener = MakeListener();
        var root = MakeContactInfo(txfreq: "1400000", rxfreq: "0700000", freq: "2100000");

        var result = listener.ParseFreqMHz(root);

        Assert.Equal("14", result);
    }

    [Fact]
    public void ParseFreqMHz_FallsBackToRxfreq_WhenTxfreqEmpty()
    {
        var listener = MakeListener();
        var root = MakeContactInfo(txfreq: "", rxfreq: "700000");

        var result = listener.ParseFreqMHz(root);

        Assert.Equal("7", result);
    }

    [Fact]
    public void ParseFreqMHz_ReturnsEmpty_WhenNoFreqElementsPresent()
    {
        var listener = MakeListener();
        var root = MakeContactInfo();

        var result = listener.ParseFreqMHz(root);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ParseFreqMHz_ReturnsEmpty_WhenValueIsNotParseable()
    {
        var listener = MakeListener();
        var root = MakeContactInfo(txfreq: "not-a-number");

        var result = listener.ParseFreqMHz(root);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ParseFreqMHz_ReturnsEmpty_WhenValueIsZeroOrNegative()
    {
        var listener = MakeListener();
        var root = MakeContactInfo(txfreq: "0");

        var result = listener.ParseFreqMHz(root);

        Assert.Equal(string.Empty, result);
    }

    // ─── Pacotes reais (Fixtures/N1MM) ─────────────────────────────────────

    private static List<QsoRecord> Receive(string fixture)
    {
        var listener = MakeListener();
        var received = new List<QsoRecord>();
        listener.QsoReceived += (_, qso) => received.Add(qso);
        listener.ProcessXml(Fixture.Text(fixture));
        return received;
    }

    [Fact]
    public void ContactInfo_Ssb_ReadsTimestampRstAndBand()
    {
        var qso = Assert.Single(Receive("N1MM/contactinfo_ssb.xml"));

        Assert.Equal("EA1ABC", qso.Call);
        Assert.Equal("CT7BFV", qso.MyCall);
        Assert.Equal("20m", qso.Band);
        Assert.Equal("SSB", qso.Mode);
        Assert.Equal("14.2", qso.Freq);
        Assert.Equal("20260928", qso.QsoDate);
        Assert.Equal("211958", qso.TimeOn);
        Assert.Equal("59", qso.RstSent);
        Assert.Equal("59", qso.RstRcvd);
        Assert.Equal(new DateTime(2026, 9, 28, 21, 19, 58, DateTimeKind.Utc), qso.QsoTimeUtc());
    }

    [Fact]
    public void ContactInfo_Cw_ReadsThreeDigitRst()
    {
        var qso = Assert.Single(Receive("N1MM/contactinfo_cw.xml"));

        Assert.Equal("CT1ABC", qso.Call);
        Assert.Equal("CW", qso.Mode);
        Assert.Equal("14.038", qso.Freq);
        Assert.Equal("599", qso.RstSent);
        Assert.Equal("599", qso.RstRcvd);
    }

    [Theory]
    [InlineData("N1MM/contactreplace.xml")]
    [InlineData("N1MM/contactdelete.xml")]
    public void EditedOrDeletedQso_IsNotSent(string fixture)
    {
        Assert.Empty(Receive(fixture));
    }

    [Theory]
    [InlineData("2026-09-28 21:19:58", "20260928", "211958")]
    [InlineData("", "", "")]
    [InlineData(null, "", "")]
    [InlineData("28/09/2026 21:19", "", "")]
    public void ParseTimestamp_ConvertsN1mmUtcTimestamp(string? raw, string expectedDate, string expectedTime)
    {
        var (date, time) = N1MMUdpListener.ParseTimestamp(raw);

        Assert.Equal(expectedDate, date);
        Assert.Equal(expectedTime, time);
    }
}
