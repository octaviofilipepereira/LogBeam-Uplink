// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using LogBeam.Core.Net;

namespace LogBeam.Tests.Net;

public class UdpListenAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not an address")]
    public void Parse_EmptyOrInvalid_ReturnsLoopback(string? value)
    {
        Assert.Equal(IPAddress.Loopback, UdpListenAddress.Parse(value));
    }

    [Fact]
    public void Parse_Default_ReturnsLoopback()
    {
        Assert.Equal(IPAddress.Loopback, UdpListenAddress.Parse(UdpListenAddress.Default));
    }

    [Fact]
    public void Parse_AllInterfaces_OnlyWhenExplicit()
    {
        Assert.Equal(IPAddress.Any, UdpListenAddress.Parse("0.0.0.0"));
    }

    [Fact]
    public void Parse_NetworkAddress_IsUsed()
    {
        Assert.Equal(IPAddress.Parse("192.168.1.10"), UdpListenAddress.Parse(" 192.168.1.10 "));
    }
}
