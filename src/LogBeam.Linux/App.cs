// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace LogBeam.Linux;

public sealed class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
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
}
