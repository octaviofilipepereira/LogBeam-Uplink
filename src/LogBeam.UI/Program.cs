// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.UI.Localization;
using Serilog;

namespace LogBeam.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Erros não tratados: ficam no log (e, com consentimento, nos relatórios de erros) e o
        // operador é avisado, em vez de a aplicação fechar sem explicação.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Report(e.Exception, fatal: false);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) Report(ex, fatal: true);
        };

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
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
