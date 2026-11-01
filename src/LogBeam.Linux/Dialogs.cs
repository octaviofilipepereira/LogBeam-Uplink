// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LogBeam.UI.Localization;

namespace LogBeam.Linux;

/// <summary>Janelas pequenas: texto com botões, consentimento e pergunta sim/não.</summary>
internal static class Dialogs
{
    public static readonly IBrush Blue = new SolidColorBrush(Color.FromRgb(0, 102, 204));

    public static Button Primary(string text) => new()
    {
        Content             = text,
        MinWidth            = 120,
        MinHeight           = 34,
        Background          = Blue,
        Foreground          = Brushes.White,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment   = VerticalAlignment.Center
    };

    public static Button Secondary(string text) => new()
    {
        Content             = text,
        MinWidth            = 120,
        MinHeight           = 34,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment   = VerticalAlignment.Center
    };

    /// <summary>Janela com título, texto seleccionável e uma fila de botões em baixo.</summary>
    private static Window Text(string title, string heading, string body, double width, double height, Control buttons, Control? extra = null)
    {
        var text = new SelectableTextBlock { Text = body, TextWrapping = TextWrapping.Wrap };
        var dock = new DockPanel { LastChildFill = true };

        var bar = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            Padding    = new Thickness(12, 8),
            Child      = buttons
        };
        DockPanel.SetDock(bar, Dock.Bottom);
        dock.Children.Add(bar);

        if (extra is not null)
        {
            DockPanel.SetDock(extra, Dock.Bottom);
            dock.Children.Add(extra);
        }

        var head = new TextBlock
        {
            Text       = heading,
            FontSize   = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Blue,
            Margin     = new Thickness(16, 12, 16, 8),
            TextWrapping = TextWrapping.Wrap
        };
        DockPanel.SetDock(head, Dock.Top);
        dock.Children.Add(head);

        dock.Children.Add(new ScrollViewer { Content = text, Padding = new Thickness(16, 0, 16, 8) });

        return new Window
        {
            Title                 = title,
            Width                 = width,
            Height                = height,
            CanResize             = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background            = Brushes.White,
            Content               = dock
        };
    }

    private static StackPanel Row(params Control[] buttons)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
        foreach (var b in buttons) row.Children.Add(b);
        return row;
    }

    public static Task Show(Window owner, string title, string heading, string body, double width = 500, double height = 420)
    {
        var ok = Primary("OK");
        var dlg = Text(title, heading, body, width, height, Row(ok));
        ok.Click += (_, _) => dlg.Close();
        return dlg.ShowDialog(owner);
    }

    public static async Task<bool> YesNo(Window owner, string title, string body)
    {
        var yes = Primary(L.Get("dialog_yes"));
        var no  = Secondary(L.Get("dialog_no"));
        var dlg = Text(title, title, body, 460, 230, Row(no, yes));
        yes.Click += (_, _) => dlg.Close(true);
        no.Click  += (_, _) => dlg.Close(false);
        return await dlg.ShowDialog<bool?>(owner) == true;
    }

    /// <summary>
    /// Dados da instalação e relatórios de erros. Com <paramref name="askChoice"/> (primeiro arranque)
    /// a escolha é explícita: fechar a janela não conta como resposta.
    /// </summary>
    public static async Task<bool> Consent(Window owner, bool askChoice)
    {
        var privacy = new Button
        {
            Content    = L.Get("consent_privacy"),
            Background = Brushes.Transparent,
            Foreground = Blue,
            Margin     = new Thickness(8, 0, 0, 0)
        };
        privacy.Click += (_, _) => Notifier.OpenUrl("https://logbeam.org/privacy.php");

        var allow = Primary(L.Get(askChoice ? "consent_allow" : "btn_close"));
        var deny  = Secondary(L.Get("consent_deny"));
        var buttons = askChoice ? Row(deny, allow) : Row(allow);

        var dlg = Text(L.Get("consent_title"), L.Get("consent_title"), L.Get("consent_body"), 600, 580, buttons, privacy);
        var answered = !askChoice;
        allow.Click += (_, _) => { answered = true; dlg.Close(true); };
        deny.Click  += (_, _) => { answered = true; dlg.Close(false); };
        dlg.Closing += (_, e) => { if (!answered) e.Cancel = true; };

        return await dlg.ShowDialog<bool?>(owner) == true;
    }
}
