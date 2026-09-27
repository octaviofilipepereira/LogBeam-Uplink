// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;

namespace LogBeam.Core.Net;

/// <summary>
/// Endereço onde os receptores UDP (N1MM+, WSJT-X) escutam.
/// Por omissão só este computador (127.0.0.1): escutar na rede local tem de ser
/// pedido expressamente (ex. 0.0.0.0), porque qualquer máquina da rede poderia
/// enviar pacotes e pôr QSOs falsos no logbook do operador.
/// </summary>
public static class UdpListenAddress
{
    public const string Default = "127.0.0.1";

    /// <summary>Endereço configurado; vazio ou inválido → 127.0.0.1.</summary>
    public static IPAddress Parse(string? value) =>
        IPAddress.TryParse(value?.Trim(), out var addr) ? addr : IPAddress.Loopback;
}
