// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Avalonia;
using LogBeam.Core.Platform;
using LogBeam.UI;
using Serilog;

namespace LogBeam.Linux;

internal static class Program
{
    /// <summary>Sem ecrã (Raspberry Pi, serviço do sistema): recebe e envia QSOs, sem janela.</summary>
    private const string ServiceArgument = "--service";

    [STAThread]
    private static int Main(string[] args)
    {
        // A pasta de trabalho é a de dados: o host do .NET vigia a pasta corrente, que a partir do
        // menu seria a pasta pessoal inteira.
        Directory.SetCurrentDirectory(AppPaths.DataDir);
        Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.ForContext("SourceContext", typeof(Program).FullName).Fatal(ex, "Erro não tratado");
                Log.CloseAndFlush();
            }
        };

        if (args.Contains(ServiceArgument)) return RunWithoutWindow();

        // Instância única: a segunda cópia pede à primeira que mostre a janela e termina.
        if (SingleInstance.AskFirstToShow()) return 0;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();

    private static int RunWithoutWindow()
    {
        using var stop   = new ManualResetEventSlim();
        using var runner = new ServiceRunner();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Set(); };
        AppDomain.CurrentDomain.ProcessExit += (_, _) => stop.Set();
        runner.Start();
        stop.Wait();
        return 0;
    }
}
