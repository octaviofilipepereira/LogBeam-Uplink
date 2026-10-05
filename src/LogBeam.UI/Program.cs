// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.UI.Localization;
using Serilog;

namespace LogBeam.UI;

internal static class Program
{
    // Por sessão do Windows ("Local\"): uma instância por utilizador com sessão aberta.
    private const string InstanceMutexName = @"Local\LogBeamUplink.Instance";
    private const string ShowEventName     = @"Local\LogBeamUplink.Show";

    [STAThread]
    private static void Main(string[] args)
    {
        // Instância única: uma segunda cópia (as portas UDP já estão ocupadas pela primeira) só
        // pede à primeira que mostre a janela, e termina.
        using var instance = new Mutex(initiallyOwned: true, InstanceMutexName, out var isFirst);
        if (!isFirst)
        {
            try { using var show = EventWaitHandle.OpenExisting(ShowEventName); show.Set(); }
            catch { /* a primeira ainda está a arrancar */ }
            return;
        }
        // Sem "using": a thread que espera por este evento vive até o processo terminar.
        var showRequest = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);

        // Erros não tratados: ficam no log (e, com consentimento, nos relatórios de erros) e o
        // operador é avisado, em vez de a aplicação fechar sem explicação.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Report(e.Exception, fatal: false);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) Report(ex, fatal: true);
        };

        ApplicationConfiguration.Initialize();

        // --tray: arranque com o Windows, só no tabuleiro.
        var form = new MainForm(startHidden: args.Contains(WindowsStartup.TrayArgument, StringComparer.OrdinalIgnoreCase));

        var waiter = new Thread(() =>
        {
            try
            {
                while (showRequest.WaitOne())
                {
                    if (form.IsDisposed) return;
                    form.BeginInvoke(form.ShowWindow);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
            {
                // a janela ou o evento já foram fechados: a aplicação está a terminar
            }
        }) { IsBackground = true, Name = "LogBeam.ShowRequest" };
        waiter.Start();

        Application.Run(form);
        GC.KeepAlive(showRequest);
    }

    private static void Report(Exception ex, bool fatal)
    {
        var log = Log.ForContext("SourceContext", typeof(Program).FullName);
        if (fatal)
        {
            log.Fatal(ex, "Erro não tratado");
            Log.CloseAndFlush();
        }
        else
        {
            log.Error(ex, "Erro não tratado na janela");
        }

        try
        {
            MessageBox.Show(string.Format(L.Get("crash_body"), ex.Message), L.Get("crash_title"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch { /* sem interface disponível */ }
    }
}
