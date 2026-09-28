// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Models;
using LogBeam.Core.Wsjtx;
using Microsoft.Extensions.Logging.Abstractions;

namespace LogBeam.Tests.Wsjtx;

public class WsjtxUdpListenerTests
{
    // ─── Pacotes reais (Fixtures/Wsjtx) ────────────────────────────────────

    private static List<QsoRecord> Receive(string fixture)
    {
        var listener = new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance);
        var received = new List<QsoRecord>();
        listener.QsoReceived += (_, qso) => received.Add(qso);
        listener.ProcessDatagram(Fixture.Bytes(fixture));
        return received;
    }

    [Fact]
    public void LoggedAdif_Ft8_UsesQsoStartTime()
    {
        var qso = Assert.Single(Receive("Wsjtx/logged_adif_ft8.bin"));

        Assert.Equal("CT1ABC", qso.Call);          // o WSJT-X enviou em minúsculas
        Assert.Equal("CS7BFV", qso.MyCall);
        Assert.Equal("40m", qso.Band);
        Assert.Equal("FT8", qso.Mode);
        Assert.Equal("7.099015", qso.Freq);
        // TIME_ON 21:25:15 — o pacote só chegou às 21:26:38, quando o QSO foi registado
        Assert.Equal(new DateTime(2026, 9, 28, 21, 25, 15, DateTimeKind.Utc), qso.QsoTimeUtc());
    }

    [Fact]
    public void LoggedAdif_Ft4_IsSentAsFt4_NotMfsk()
    {
        var qso = Assert.Single(Receive("Wsjtx/logged_adif_ft4.bin"));

        Assert.Equal("PY1AB", qso.Call);
        Assert.Equal("FT4", qso.Mode);
        Assert.Equal("40m", qso.Band);
    }

    [Theory]
    [InlineData("Wsjtx/heartbeat.bin")]
    [InlineData("Wsjtx/status.bin")]
    [InlineData("Wsjtx/qso_logged_ft8.bin")]
    public void OtherMessageTypes_AreIgnored(string fixture)
    {
        Assert.Empty(Receive(fixture));
    }

    [Fact]
    public void NonWsjtxDatagram_IsIgnored()
    {
        var listener = new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance);
        var received = 0;
        listener.QsoReceived += (_, _) => received++;

        listener.ProcessDatagram(Fixture.Bytes("Log4OM/adif_message_ssb.adi"));

        Assert.Equal(0, received);
    }
}
