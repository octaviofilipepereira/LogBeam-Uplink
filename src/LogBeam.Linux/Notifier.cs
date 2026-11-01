// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.ComponentModel;
using System.Diagnostics;

namespace LogBeam.Linux;

/// <summary>Notificações do ambiente de trabalho, pelo notify-send.</summary>
internal static class Notifier
{
    public static void Show(string title, string text, int milliseconds, bool warning = false)
    {
        try
        {
            var info = new ProcessStartInfo("notify-send") { UseShellExecute = false };
            foreach (var arg in new[] { "-a", "LogBeam Uplink", "-i", "logbeam-uplink", "-t", milliseconds.ToString(),
                                        "-u", warning ? "critical" : "normal", title, text })
                info.ArgumentList.Add(arg);
            Process.Start(info)?.Dispose();
        }
        catch (Win32Exception) { /* sem notify-send: fica a mensagem na janela */ }
    }

    public static void OpenUrl(string url)
    {
        try
        {
            var info = new ProcessStartInfo("xdg-open") { UseShellExecute = false };
            info.ArgumentList.Add(url);
            Process.Start(info)?.Dispose();
        }
        catch (Win32Exception) { /* sem navegador associado */ }
    }
}
