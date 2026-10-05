// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Net.Sockets;

namespace LogBeam.Core.Net;

/// <summary>
/// Endereço onde os receptores UDP (N1MM+, WSJT-X, Log4OM) escutam.
/// Por omissão só este computador (127.0.0.1): escutar na rede local tem de ser
/// pedido expressamente (ex. 0.0.0.0), porque qualquer máquina da rede poderia
/// enviar pacotes e pôr QSOs falsos no logbook do operador.
///
/// Um endereço multicast (224.0.0.0 a 239.255.255.255, ex. 239.255.0.1) permite partilhar a
/// porta do WSJT-X/JTDX com o JTAlert ou o GridTracker: todos recebem todos os pacotes. Com
/// um endereço normal, dois programas na mesma porta não funcionam (o Windows entrega cada
/// pacote a um só), por isso a porta abre-se em exclusivo e, se estiver ocupada, falha logo.
/// </summary>
public static class UdpListenAddress
{
    public const string Default = "127.0.0.1";

    /// <summary>Endereço configurado; vazio ou inválido → 127.0.0.1.</summary>
    public static IPAddress Parse(string? value) =>
        IPAddress.TryParse(value?.Trim(), out var addr) ? addr : IPAddress.Loopback;

    public static bool IsMulticast(IPAddress address) =>
        address.AddressFamily == AddressFamily.InterNetwork && (address.GetAddressBytes()[0] & 0xF0) == 224;

    /// <summary>
    /// Abre o receptor. Multicast: porta partilhada, grupo só na interface local (127.0.0.1),
    /// como o WSJT-X envia por omissão; usar <see cref="Accept"/> para ignorar pacotes de fora.
    /// </summary>
    public static UdpClient Open(IPAddress address, int port)
    {
        if (!IsMulticast(address))
            return new UdpClient(new IPEndPoint(address, port));

        var client = new UdpClient(AddressFamily.InterNetwork) { ExclusiveAddressUse = false };
        try
        {
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            client.JoinMulticastGroup(address, IPAddress.Loopback);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Em multicast a porta fica aberta em todas as interfaces (é assim que o grupo funciona);
    /// só se aceitam pacotes deste computador, para a rede local não poder pôr QSOs no logbook.
    /// </summary>
    public static bool Accept(IPAddress listenAddress, IPEndPoint remote) =>
        !IsMulticast(listenAddress) || IPAddress.IsLoopback(remote.Address);
}
