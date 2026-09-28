// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text;
using LogBeam.Core.Adif;
using LogBeam.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace LogBeam.Tests.Adif;

public class AdifUdpListenerTests
{
    private static List<QsoRecord> Receive(byte[] datagram)
    {
        var listener = new AdifUdpListener(NullLogger<AdifUdpListener>.Instance);
        var received = new List<QsoRecord>();
        listener.QsoReceived += (_, qso) => received.Add(qso);
        listener.ProcessDatagram(datagram);
        return received;
    }

    [Fact]
    public void Log4omAdifMessage_Ssb_IsReadCorrectly()
    {
        var qso = Assert.Single(Receive(Fixture.Bytes("Log4OM/adif_message_ssb.adi")));

        Assert.Equal("CT1ABC", qso.Call);
        Assert.Equal("CT7BFV", qso.MyCall);
        Assert.Equal("20m", qso.Band);
        Assert.Equal("SSB", qso.Mode);             // MODE=SSB SUBMODE=USB
        Assert.Equal("14.201000", qso.Freq);
        Assert.Equal("59", qso.RstSent);
        Assert.Equal("59", qso.RstRcvd);
        Assert.Equal("100.0", qso.TxPower);
        Assert.Equal(new DateTime(2026, 9, 28, 21, 53, 3, DateTimeKind.Utc), qso.QsoTimeUtc());
    }

    [Fact]
    public void DatagramWithTwoRecords_RaisesTwoQsos()
    {
        var datagram = Encoding.UTF8.GetBytes("<CALL:5>EA1AA <BAND:3>20m <MODE:2>CW <EOR>\r\n<CALL:5>EA2BB <BAND:3>40m <MODE:2>CW <EOR>\r\n");

        var received = Receive(datagram);

        Assert.Equal(new[] { "EA1AA", "EA2BB" }, received.Select(q => q.Call));
    }

    [Fact]
    public void DatagramWithoutCall_IsIgnored()
    {
        Assert.Empty(Receive(Encoding.UTF8.GetBytes("<BAND:3>20m <MODE:2>CW <EOR>")));
    }
}
