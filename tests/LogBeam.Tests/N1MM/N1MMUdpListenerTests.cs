// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Xml.Linq;
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

    // ─── NormalizeBand ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("20m", "20m")]
    [InlineData("40M", "40m")]
    [InlineData("20", "20m")]
    [InlineData("14", "20m")]
    [InlineData("144", "2m")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void NormalizeBand_MapsN1mmValues_ToAdifFormat(string? input, string expected)
    {
        var result = N1MMUdpListener.NormalizeBand(input);

        Assert.Equal(expected, result);
    }

    // ─── NormalizeMode ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("USB", "SSB")]
    [InlineData("LSB", "SSB")]
    [InlineData("CW", "CW")]
    [InlineData("ft8", "FT8")]
    [InlineData("PSK63", "PSK")]
    [InlineData(null, "")]
    public void NormalizeMode_MapsN1mmValues_ToAdifFormat(string? input, string expected)
    {
        var result = N1MMUdpListener.NormalizeMode(input);

        Assert.Equal(expected, result);
    }
}
