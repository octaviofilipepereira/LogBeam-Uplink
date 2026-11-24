// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using LogBeam.Core.Platform;
using Serilog;

namespace LogBeam.Linux;

public sealed class App : Application
{
    private static int _trayMenuFaults;

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Dispatcher.UIThread.UnhandledException += OnUiUnhandledException;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // --tray: arranque com o sistema, só na área de notificação.
            var startHidden = desktop.Args?.Contains(LinuxStartup.TrayArgument, StringComparer.OrdinalIgnoreCase) == true;
            var window = new MainWindow(startHidden);
            SingleInstance.Listen(window.ShowWindow);
            if (!startHidden) desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Um erro do Avalonia no menu do ícone (<see cref="TrayMenuFault"/>) já não fecha o Uplink: fica
    /// no log como aviso, o primeiro com o rasto e os seguintes só contados. Os outros erros seguem
    /// como antes, para o registo de Program.cs.
    /// </summary>
    private static void OnUiUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (!TrayMenuFault.IsTrayMenuFault(e.Exception)) return;
        e.Handled = true;

        try
        {
            var log = Log.ForContext("SourceContext", typeof(App).FullName);
            if (++_trayMenuFaults == 1)
                log.Warning(e.Exception, "Menu do ícone: erro do Avalonia no D-Bus, ignorado");
            else
                log.Debug("Menu do ícone: erro do Avalonia no D-Bus, ignorado (total: {Count})", _trayMenuFaults);
        }
        catch { /* o registo não pode causar um segundo erro */ }
    }
}
