// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Drawing;
using System.Windows.Forms;
using LogBeam.UI.Localization;

namespace LogBeam.UI;

/// <summary>
/// Explica que dados da instalação e relatórios de erros são enviados ao LogBeam.
/// No primeiro arranque pede a escolha (Autorizar / Não autorizar, sem fechar pela cruz);
/// a partir do separador Avançado só informa.
/// </summary>
public sealed class ConsentForm : Form
{
    public const string PrivacyUrl = "https://logbeam.org/privacy.php";

    /// <param name="askChoice">true = primeiro arranque: Autorizar (OK) ou Não autorizar (No).</param>
    public ConsentForm(bool askChoice)
    {
        Text            = L.Get("consent_title");
        Size            = new Size(560, 510);
        StartPosition   = askChoice ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        ControlBox      = !askChoice;   // a escolha tem de ser explícita
        ShowInTaskbar   = askChoice;
        Font            = new Font("Segoe UI", 9f);
        BackColor       = Color.White;
        Icon            = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        var title = new Label
        {
            Text      = L.Get("consent_title"),
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Dock      = DockStyle.Top,
            Height    = 44,
            Padding   = new Padding(16, 12, 16, 0)
        };

        var body = new RichTextBox
        {
            Text        = L.Get("consent_body"),
            ReadOnly    = true,
            BorderStyle = BorderStyle.None,
            BackColor   = Color.White,
            Dock        = DockStyle.Fill,
            TabStop     = false
        };
        var bodyHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 8, 16, 0) };
        bodyHost.Controls.Add(body);

        var privacy = new LinkLabel
        {
            Text     = L.Get("consent_privacy"),
            Dock     = DockStyle.Bottom,
            Height   = 28,
            Padding  = new Padding(16, 4, 16, 0)
        };
        privacy.LinkClicked += (_, _) =>
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(PrivacyUrl) { UseShellExecute = true }); }
            catch { /* sem navegador associado */ }
        };

        var buttons = new FlowLayoutPanel
        {
            Dock          = DockStyle.Bottom,
            Height        = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding       = new Padding(12, 8, 12, 8),
            BackColor     = Color.FromArgb(245, 245, 245)
        };

        if (askChoice)
        {
            var allow = MakeButton(L.Get("consent_allow"), DialogResult.OK, primary: true);
            var deny  = MakeButton(L.Get("consent_deny"), DialogResult.No, primary: false);
            buttons.Controls.Add(allow);
            buttons.Controls.Add(deny);
            AcceptButton = allow;
        }
        else
        {
            var close = MakeButton(L.Get("btn_close"), DialogResult.OK, primary: true);
            buttons.Controls.Add(close);
            AcceptButton = close;
            CancelButton = close;
        }

        Controls.Add(bodyHost);
        Controls.Add(title);
        Controls.Add(privacy);
        Controls.Add(buttons);
    }

    private static Button MakeButton(string text, DialogResult result, bool primary)
    {
        var b = new Button
        {
            Text         = text,
            DialogResult = result,
            AutoSize     = true,
            MinimumSize  = new Size(120, 34),
            FlatStyle    = FlatStyle.Flat,
            BackColor    = primary ? Color.FromArgb(0, 102, 204) : Color.White,
            ForeColor    = primary ? Color.White : Color.FromArgb(40, 40, 40),
            Margin       = new Padding(6, 0, 0, 0)
        };
        b.FlatAppearance.BorderSize  = primary ? 0 : 1;
        b.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
        return b;
    }
}
