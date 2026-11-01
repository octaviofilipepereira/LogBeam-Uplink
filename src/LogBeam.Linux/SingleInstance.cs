// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net.Sockets;
using Avalonia.Threading;
using LogBeam.Core.Platform;

namespace LogBeam.Linux;

/// <summary>
/// Uma instância por utilizador (as portas UDP só servem uma). A primeira escuta num socket local;
/// a segunda liga-se a ele, o que basta para a primeira mostrar a janela.
/// </summary>
internal static class SingleInstance
{
    private static string SocketPath => Path.Combine(AppPaths.DataDir, "instance.sock");

    /// <summary>Devolve true se já havia uma instância a correr (que recebeu o pedido).</summary>
    public static bool AskFirstToShow()
    {
        if (!File.Exists(SocketPath)) return false;
        try
        {
            using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            client.Connect(new UnixDomainSocketEndPoint(SocketPath));
            return true;
        }
        catch (SocketException)
        {
            File.Delete(SocketPath); // ficou de uma sessão que não terminou bem
            return false;
        }
    }

    public static void Listen(Action show)
    {
        var server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        server.Bind(new UnixDomainSocketEndPoint(SocketPath));
        server.Listen(4);

        new Thread(() =>
        {
            while (true)
            {
                using var client = server.Accept();
                Dispatcher.UIThread.Post(show);
            }
        }) { IsBackground = true, Name = "LogBeam.ShowRequest" }.Start();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try { File.Delete(SocketPath); }
            catch (IOException) { /* a terminar */ }
        };
    }
}
