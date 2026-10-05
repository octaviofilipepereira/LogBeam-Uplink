// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Net.Sockets;
using LogBeam.Core.Models;
using LogBeam.Core.Net;
using LogBeam.Core.Wsjtx;
using Microsoft.Extensions.Logging.Abstractions;

namespace LogBeam.Tests.Net;

/// <summary>
/// Porta do WSJT-X partilhada por multicast (como com o JTAlert ou o GridTracker): dois
/// receptores na mesma porta recebem ambos o mesmo pacote real.
/// </summary>
public class MulticastTests
{
    private static readonly IPAddress Group = IPAddress.Parse("239.255.0.1");

    [Theory]
    [InlineData("239.255.0.1", true)]
    [InlineData("224.0.0.1", true)]
    [InlineData("127.0.0.1", false)]
    [InlineData("0.0.0.0", false)]
    [InlineData("240.0.0.1", false)]
    public void MulticastRange(string address, bool expected)
    {
        Assert.Equal(expected, UdpListenAddress.IsMulticast(IPAddress.Parse(address)));
    }

    [Fact]
    public void InMulticastOnlyLocalPacketsAreAccepted()
    {
        Assert.True(UdpListenAddress.Accept(Group, new IPEndPoint(IPAddress.Loopback, 5000)));
        Assert.False(UdpListenAddress.Accept(Group, new IPEndPoint(IPAddress.Parse("192.168.1.50"), 5000)));
        Assert.True(UdpListenAddress.Accept(IPAddress.Any, new IPEndPoint(IPAddress.Parse("192.168.1.50"), 5000)));   // 0.0.0.0 foi pedido
    }

    private static int FreeUdpPort()
    {
        using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
    }

    [Fact]
    public async Task TwoListenersOnTheSameMulticastPortBothReceiveTheQso()
    {
        var port = FreeUdpPort();
        var first  = new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance, port, "239.255.0.1");
        var second = new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance, port, "239.255.0.1");
        var gotFirst  = new TaskCompletionSource<QsoRecord>(TaskCreationOptions.RunContinuationsAsynchronously);
        var gotSecond = new TaskCompletionSource<QsoRecord>(TaskCreationOptions.RunContinuationsAsynchronously);
        first.QsoReceived  += (_, q) => gotFirst.TrySetResult(q);
        second.QsoReceived += (_, q) => gotSecond.TrySetResult(q);

        first.Start();
        second.Start();   // não falha: a porta é partilhada
        try
        {
            // Como o WSJT-X: multicast pela interface local.
            using var sender = new UdpClient(AddressFamily.InterNetwork);
            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.Loopback.GetAddressBytes());
            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
            var packet = Fixture.Bytes("Wsjtx/logged_adif_ft8.bin");
            await sender.SendAsync(packet, packet.Length, new IPEndPoint(Group, port));

            var both = Task.WhenAll(gotFirst.Task, gotSecond.Task);
            Assert.Same(both, await Task.WhenAny(both, Task.Delay(TimeSpan.FromSeconds(5))));
            Assert.Equal("CT1ABC", gotFirst.Task.Result.Call);
            Assert.Equal("CT1ABC", gotSecond.Task.Result.Call);
        }
        finally
        {
            first.Stop();
            second.Stop();
        }
    }

    [Fact]
    public void UnicastPortStillOpensExclusively()
    {
        var port = FreeUdpPort();
        var first = new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance, port, "127.0.0.1");
        first.Start();
        try
        {
            var second = new WsjtxUdpListener(NullLogger<WsjtxUdpListener>.Instance, port, "127.0.0.1");
            Assert.Throws<SocketException>(() => second.Start());   // falha clara, em vez de roubar pacotes
        }
        finally { first.Stop(); }
    }
}
