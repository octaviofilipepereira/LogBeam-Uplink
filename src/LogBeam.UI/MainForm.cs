// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Drawing;
using System.Windows.Forms;
using LogBeam.Core.Models;
using LogBeam.Service.Models;
using LogBeam.Service.Settings;
using LogBeam.UI.Localization;
using LogBeam.UI.Models;
using LogBeam.UI.Settings;

namespace LogBeam.UI;

public class MainForm : Form
{
    private readonly SettingsManager  _mgr     = new();
    private readonly UiPrefsManager   _prefsMgr = new();
    private AppSettings   _settings = new();
    private UiPreferences _prefs    = new();

    // Service
    private readonly ServiceRunner _runner = new();
    private NotifyIcon  _tray     = null!;
    private ContextMenuStrip _trayMenu = null!;
    private ToolStripMenuItem _tmiStart = null!, _tmiStop = null!;
    private int _qsoCount;
    private Icon? _iconBrand;
    // Menu
    private MenuStrip     _menuStrip = null!;

    // Top panel
    private Panel  _pnlTop  = null!;
    private Label  _lblTitle = null!;
    private readonly Dictionary<string, Button> _langButtons = new();

    // Tabs
    private TabControl _tabs      = null!;
    private TabPage    _pgStation = null!, _pgApi = null!, _pgHamQth = null!, _pgAdvanced = null!, _pgN1mm = null!, _pgWsjtx = null!, _pgClubLog = null!;

    // N1MM tab
    private Label         _lN1mmPort = null!, _lN1mmAddr = null!, _lN1mmHint = null!, _lN1mmStatus = null!;
    private NumericUpDown _nN1mmPort = null!;
    private TextBox       _txtN1mmAddr = null!;
    private Button         _btnN1mmDetect = null!, _btnN1mmAutoConfig = null!;
    private string? _n1mmFolder;

    // Criado a cada uso com o logger global do Serilog (configurado pelo ServiceRunner e
    // substituído quando o serviço reinicia), para os erros do N1MM+ ficarem no log.
    private static LogBeam.Core.N1MM.N1MMConfigManager N1mmConfig() =>
        new(Microsoft.Extensions.Logging.LoggerFactoryExtensions.CreateLogger<LogBeam.Core.N1MM.N1MMConfigManager>(
            new Serilog.Extensions.Logging.SerilogLoggerFactory(Serilog.Log.Logger)));

    // WSJT-X tab
    private CheckBox       _chkWsjtxEnabled = null!;
    private Label          _lWsjtxPort = null!, _lWsjtxHint = null!;
    private NumericUpDown  _nWsjtxPort = null!;

    // ClubLog tab
    private Label    _lClHint = null!, _lClEmail = null!, _lClPass = null!, _lClCall = null!;
    private CheckBox _chkClEnabled = null!;
    private TextBox  _txtClEmail = null!, _txtClPass = null!, _txtClCall = null!;

    // Station tab
    private Label         _lCallsign = null!, _lLat = null!, _lLon = null!, _lQthHint = null!;
    private TextBox       _txtCallsign = null!;
    private NumericUpDown _nLat = null!, _nLon = null!;

    // API tab
    private Label         _lApiUrl = null!, _lApiTimeout = null!,
                          _lApiRetry = null!, _lApiDelay = null!, _lApiHint = null!;
    private TextBox       _txtApiUrl = null!;
    private NumericUpDown _nTimeout = null!, _nRetry = null!, _nDelay = null!;
    private Button        _btnTestApi = null!, _btnProfileAdd = null!, _btnProfileRemove = null!;
    private DataGridView  _gridProfiles = null!;

    // HamQTH tab
    private Label         _lHamQthInfo = null!, _lHamQthUser = null!, _lHamQthPass = null!, _lHamQthCache = null!;
    private TextBox       _txtHamQthUser = null!, _txtHamQthPass = null!;
    private NumericUpDown _nHamQthCache = null!;

    // Advanced tab
    private Label    _lLogLevel = null!, _lLogPath = null!;
    private ComboBox _cboLogLevel = null!;
    private CheckBox _chkNotifyEachQso = null!;
    private TextBox  _txtLogPath = null!;

    // Bottom panel
    private Panel  _pnlBottom = null!;
    private Button _btnSave = null!, _btnCancel = null!, _btnExit = null!;
    private Label  _lblStatus = null!;

    // ──────────────────────────────────────────────────────────────────────
    public MainForm()
    {
        _settings = _mgr.Load();
        _prefs    = _prefsMgr.LoadPrefs();
        L.Lang    = _prefs.Language;
        InitializeComponent();
        BuildTrayIcon();
        PopulateForm();
        _runner.StatusChanged           += OnServiceStatusChanged;
        _runner.QsoResult               += OnQsoResult;
        _runner.QsoConfirmedByLogbeam   += OnQsoConfirmedByLogbeam;
        // arrancar o serviço automaticamente ao abrir
        _runner.Start();

        _ = CheckForUpdateAsync(silent: true);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        Text            = "LogBeam Uplink v2.5 by CT7BFV";
        Size            = new Size(760, 760);
        MinimumSize     = new Size(700, 680);
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 9f);
        BackColor       = SystemColors.Control;
        // minimizar para tray em vez de fechar
        FormClosing += OnFormClosing;
        Resize      += OnFormResize;
        Load        += (_, _) => UpdateTray();

        BuildMenu();
        BuildTopPanel();
        BuildBottomPanel();
        BuildTabs();
        UpdateAllText();

        ResumeLayout(false);
        PerformLayout();
    }

    // ─── Tray ───────────────────────────────────────────────────────────
    private static Icon LoadBrandIcon()
    {
        try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application; }
        catch { return SystemIcons.Application; }
    }

    private void BuildTrayIcon()
    {
        _iconBrand = LoadBrandIcon();
        Icon       = _iconBrand;   // barra de título e barra de tarefas (sem isto, a janela fica com o ícone genérico)

        _trayMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9f) };

        var tmiOpen = new ToolStripMenuItem();
        tmiOpen.Click += (_, _) => ShowWindow();

        _tmiStart = new ToolStripMenuItem();
        _tmiStart.Click += (_, _) => { _qsoCount = 0; _runner.Start(); UpdateTray(); };

        _tmiStop = new ToolStripMenuItem();
        _tmiStop.Click += (_, _) => { _runner.Stop(); UpdateTray(); };

        _trayMenu.Items.Add(tmiOpen);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(_tmiStart);
        _trayMenu.Items.Add(_tmiStop);
        _trayMenu.Items.Add(new ToolStripSeparator());

        var tmiExit = new ToolStripMenuItem();
        tmiExit.Click += (_, _) => ExitApp();
        _trayMenu.Items.Add(tmiExit);

        _tray = new NotifyIcon
        {
            Icon             = _iconBrand,
            ContextMenuStrip = _trayMenu,
            Visible          = true
        };
        _tray.DoubleClick += (_, _) => ShowWindow();

        // textos iniciais
        tmiOpen.Text  = L.Get("tray_open");
        tmiExit.Text  = L.Get("tray_exit");
        UpdateTray();
    }

    private void UpdateTray()
    {
        var running = _runner.IsRunning;
        _tray.Text = running
            ? string.Format(L.Get("tray_running_count"), _qsoCount)
            : L.Get("tray_stopped");

        _tmiStart.Enabled    = !running;
        _tmiStop.Enabled     = running;
        _tmiStart.Text       = L.Get("tray_start");
        _tmiStop.Text        = L.Get("tray_stop");
    }

    private void OnQsoResult(QsoRecord qso, bool success)
    {
        if (IsDisposed || !IsHandleCreated) return;
        Invoke(() =>
        {
            if (success) _qsoCount++;
            UpdateTray();
            // Por omissão só as falhas geram aviso; o aviso por QSO enviado é opcional (separador Avançado).
            if (success && !_prefs.NotifyEachQso) return;
            _tray.BalloonTipIcon = success ? ToolTipIcon.Info : ToolTipIcon.Warning;
            _tray.BalloonTipTitle = success ? L.Get("balloon_qso_sent") : L.Get("balloon_qso_failed");
            _tray.BalloonTipText  = $"{qso.Call} — {qso.Band} {qso.Mode}";
            _tray.ShowBalloonTip(4000);
        });
    }

    private void OnQsoConfirmedByLogbeam(QsoRecord qso)
    {
        if (IsDisposed || !IsHandleCreated) return;
        Invoke(() =>
        {
            _tray.BalloonTipIcon  = ToolTipIcon.Info;
            _tray.BalloonTipTitle = L.Get("balloon_confirmed_title");
            _tray.BalloonTipText  = string.Format(L.Get("balloon_confirmed_text"), qso.Call);
            _tray.ShowBalloonTip(5000);
        });
    }

    private void ShowWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void OnFormResize(object? s, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
            Hide();
    }

    private void OnFormClosing(object? s, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide(); // minimizar para tray, não terminar
        }
    }

    private void ExitApp()
    {
        _runner.Stop();
        _tray.Visible = false;
        _tray.Dispose();
        if (_iconBrand is not null && !ReferenceEquals(_iconBrand, SystemIcons.Application))
            _iconBrand.Dispose();
        Application.Exit();
    }

    private void OnServiceStatusChanged(string status)
    {
        if (IsDisposed || !IsHandleCreated) return;
        Invoke(() =>
        {
            UpdateTray();
            SetStatus(status == "running" ? "ok" :
                      status == "error"   ? "err" : "",
                L.Get(status == "running" ? "svc_started" :
                      status == "error"   ? "svc_error"   : "svc_stopped"));
        });
    }

    // ─── Menu ──────────────────────────────────────────────────────────────
    private ToolStripMenuItem _miFile = null!, _miExportAdif = null!;
    private ToolStripMenuItem _miHelp = null!, _miAbout = null!, _miN1mmHelp = null!, _miCheckUpdate = null!;
    private readonly UpdateCheckService _updateCheck = new();

    private void BuildMenu()
    {
        _menuStrip = new MenuStrip { BackColor = Color.FromArgb(36, 36, 36), ForeColor = Color.White };

        _miFile       = new ToolStripMenuItem { ForeColor = Color.White, BackColor = Color.FromArgb(36, 36, 36) };
        _miExportAdif = new ToolStripMenuItem { ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 50) };
        _miExportAdif.Click += OnExportAdif;
        _miFile.DropDownItems.Add(_miExportAdif);
        _menuStrip.Items.Add(_miFile);

        _miHelp        = new ToolStripMenuItem { ForeColor = Color.White, BackColor = Color.FromArgb(36, 36, 36) };
        _miAbout       = new ToolStripMenuItem { ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 50) };
        _miN1mmHelp    = new ToolStripMenuItem { ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 50) };
        _miCheckUpdate = new ToolStripMenuItem { ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 50) };
        _miAbout.Click       += OnAbout;
        _miN1mmHelp.Click    += OnN1mmHelp;
        _miCheckUpdate.Click += (_, _) => _ = CheckForUpdateAsync(silent: false);
        _miHelp.DropDownItems.Add(_miAbout);
        _miHelp.DropDownItems.Add(_miN1mmHelp);
        _miHelp.DropDownItems.Add(_miCheckUpdate);
        _menuStrip.Items.Add(_miHelp);
        MainMenuStrip = _menuStrip;
        Controls.Add(_menuStrip);
    }

    /// <summary>
    /// Verifica se há uma versão mais recente. silent=true (arranque): não mostra nada
    /// se já estiver actualizado ou se a verificação falhar. silent=false (menu manual):
    /// mostra sempre um resultado.
    /// </summary>
    private async Task CheckForUpdateAsync(bool silent)
    {
        var result = await _updateCheck.CheckAsync(CancellationToken.None);
        if (IsDisposed || !IsHandleCreated) return;

        Invoke(() =>
        {
            if (result.HasUpdate)
            {
                var msg = string.Format(L.Get("update_available_body"), result.LatestVersion, AppVersion.Current);
                var openDownload = MessageBox.Show(this, msg, L.Get("update_available_title"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
                if (openDownload && !string.IsNullOrEmpty(result.DownloadUrl))
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(result.DownloadUrl) { UseShellExecute = true }); }
                    catch { /* utilizador pode copiar o link manualmente do About */ }
                }
            }
            else if (!silent)
            {
                MessageBox.Show(this,
                    result.Error != null ? L.Get("update_check_failed") : L.Get("update_up_to_date"),
                    L.Get("update_available_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        });
    }

    private void OnExportAdif(object? s, EventArgs e)
    {
        var qsos = _runner.GetSessionLog();
        if (qsos.Count == 0)
        {
            SetStatus("", L.Get("status_export_empty"));
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter   = "ADIF (*.adi)|*.adi",
            FileName = $"logbeam_sessao_{DateTime.Now:yyyyMMdd_HHmmss}.adi"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("LogBeam Uplink 2.5 — exportação de sessão\n");
            sb.Append($"<ADIF_VER:5>3.1.4<PROGRAMID:8>LogBeam<EOH>\n");
            foreach (var qso in qsos)
                sb.Append(LogBeam.Core.Adif.AdifBuilder.Build(qso)).Append('\n');

            File.WriteAllText(dlg.FileName, sb.ToString());
            SetStatus("ok", string.Format(L.Get("status_export_ok"), qsos.Count));
        }
        catch (Exception ex)
        {
            SetStatus("err", $"{L.Get("status_export_failed")}: {ex.Message}");
        }
    }

    private void OnAbout(object? s, EventArgs e)
    {
        using var dlg = new Form
        {
            Text            = L.Get("about_title"),
            Size            = new Size(480, 360),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            Font            = new Font("Segoe UI", 9f),
            BackColor       = Color.White
        };

        var logo = new Label
        {
            Text      = "📡  LogBeam Uplink v2.5 by CT7BFV",
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Dock      = DockStyle.Top,
            Height    = 40,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var txt = new RichTextBox
        {
            Text        = L.Get("about_body"),
            ReadOnly    = true,
            BorderStyle = BorderStyle.None,
            BackColor   = Color.White,
            Font        = new Font("Segoe UI", 9f),
            Dock        = DockStyle.Fill,
            Margin      = new Padding(16)
        };

        var btn = new Button
        {
            Text        = "OK",
            DialogResult = DialogResult.OK,
            Dock        = DockStyle.Bottom,
            Height      = 36,
            FlatStyle   = FlatStyle.Flat,
            BackColor   = Color.FromArgb(0, 102, 204),
            ForeColor   = Color.White
        };
        btn.FlatAppearance.BorderSize = 0;

        dlg.Controls.Add(txt);
        dlg.Controls.Add(logo);
        dlg.Controls.Add(btn);
        dlg.AcceptButton = btn;
        dlg.ShowDialog(this);
    }

    private void OnN1mmHelp(object? s, EventArgs e)
    {
        using var dlg = new Form
        {
            Text            = L.Get("n1mm_help_title"),
            Size            = new Size(520, 440),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            Font            = new Font("Segoe UI", 9f),
            BackColor       = Color.White
        };
        var txt = new RichTextBox
        {
            Text        = L.Get("n1mm_help_body"),
            ReadOnly    = true,
            BorderStyle = BorderStyle.None,
            BackColor   = Color.White,
            Font        = new Font("Consolas", 9.5f),
            Dock        = DockStyle.Fill,
            Padding     = new Padding(12)
        };
        var btn = new Button
        {
            Text         = "OK",
            DialogResult = DialogResult.OK,
            Dock         = DockStyle.Bottom,
            Height       = 36,
            FlatStyle    = FlatStyle.Flat,
            BackColor    = Color.FromArgb(0, 102, 204),
            ForeColor    = Color.White
        };
        btn.FlatAppearance.BorderSize = 0;
        dlg.Controls.Add(txt);
        dlg.Controls.Add(btn);
        dlg.AcceptButton = btn;
        dlg.ShowDialog(this);
    }

    // ─── Top Panel ─────────────────────────────────────────────────────────
    private void BuildTopPanel()
    {
        _pnlTop = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 46,
            BackColor = Color.FromArgb(24, 24, 24)
        };

        _lblTitle = new Label
        {
            Text      = "📡  LogBeam Uplink v2.5 by CT7BFV",
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            AutoSize  = true,
            Location  = new Point(12, 11)
        };

        _pnlTop.Controls.Add(_lblTitle);
        foreach (var lang in L.Languages)
        {
            var btn = MakeLangButton(lang);
            btn.Click += (_, _) => ChangeLanguage(lang);
            _langButtons[lang] = btn;
            _pnlTop.Controls.Add(btn);
        }
        _pnlTop.Resize += (_, _) => PositionLangButtons();

        Controls.Add(_pnlTop);
        PositionLangButtons();
    }

    private Button MakeLangButton(string lang) => new()
    {
        Text      = lang,
        Width     = 46,
        Height    = 27,
        Top       = 10,
        FlatStyle = FlatStyle.Flat,
        ForeColor = Color.White,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        BackColor = L.Lang == lang ? Color.FromArgb(0, 102, 204) : Color.FromArgb(55, 55, 55),
        Cursor    = Cursors.Hand
    };

    private void PositionLangButtons()
    {
        // Da direita para a esquerda, pela ordem inversa: PT EN ES FR fica alinhado à direita.
        var left = _pnlTop.Width - 12;
        foreach (var lang in L.Languages.Reverse())
        {
            var btn = _langButtons[lang];
            left -= btn.Width;
            btn.Left = left;
            left -= 4;
        }
    }

    // ─── Bottom Panel ──────────────────────────────────────────────────────
    private void BuildBottomPanel()
    {
        _pnlBottom = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 52,
            BackColor = Color.FromArgb(242, 242, 242)
        };

        _lblStatus = new Label
        {
            AutoSize  = true,
            Location  = new Point(12, 17),
            ForeColor = Color.DimGray,
            Font      = new Font("Segoe UI", 8.5f)
        };

        _btnSave   = MakeActionButton("", Color.FromArgb(0, 102, 204), Color.White);
        _btnCancel = MakeActionButton("", Color.FromArgb(180, 180, 180), Color.Black);
        _btnExit   = MakeActionButton("", Color.FromArgb(180, 40, 40),  Color.White);
        _btnSave.Click   += OnSave;
        _btnCancel.Click += OnCancel;
        _btnExit.Click   += (_, _) => ExitApp();

        _pnlBottom.Controls.Add(_lblStatus);
        _pnlBottom.Controls.Add(_btnSave);
        _pnlBottom.Controls.Add(_btnCancel);
        _pnlBottom.Controls.Add(_btnExit);
        _pnlBottom.Resize += (_, _) => PositionBottomButtons();

        Controls.Add(_pnlBottom);
        PositionBottomButtons();
    }

    private static Button MakeActionButton(string text, Color back, Color fore) => new()
    {
        Text      = text,
        Width     = 110,
        Height    = 34,
        Top       = 9,
        FlatStyle = FlatStyle.Flat,
        BackColor = back,
        ForeColor = fore,
        Cursor    = Cursors.Hand
    };

    private void PositionBottomButtons()
    {
        _btnExit.Left   = _pnlBottom.Width - _btnExit.Width - 12;
        _btnCancel.Left = _btnExit.Left - _btnCancel.Width - 8;
        _btnSave.Left   = _btnCancel.Left - _btnSave.Width - 8;
    }

    // ─── Tabs ──────────────────────────────────────────────────────────────
    private void BuildTabs()
    {
        _tabs = new TabControl
        {
            Dock    = DockStyle.Fill,
            Padding = new Point(14, 4),
            Font    = new Font("Segoe UI", 9f)
        };

        _pgStation  = new TabPage { UseVisualStyleBackColor = true };
        _pgApi      = new TabPage { UseVisualStyleBackColor = true };
        _pgHamQth   = new TabPage { UseVisualStyleBackColor = true };
        _pgN1mm     = new TabPage { UseVisualStyleBackColor = true };
        _pgWsjtx    = new TabPage { UseVisualStyleBackColor = true };
        _pgClubLog  = new TabPage { UseVisualStyleBackColor = true };
        _pgAdvanced = new TabPage { UseVisualStyleBackColor = true };

        _tabs.TabPages.AddRange(new[] { _pgStation, _pgApi, _pgHamQth, _pgN1mm, _pgWsjtx, _pgClubLog, _pgAdvanced });
        Controls.Add(_tabs);
        _tabs.BringToFront();

        BuildStationTab();
        BuildApiTab();
        BuildHamQthTab();
        BuildN1mmTab();
        BuildWsjtxTab();
        BuildClubLogTab();
        BuildAdvancedTab();
    }

    private static TableLayoutPanel MakeTable(TabPage page, int rows)
    {
        var t = new TableLayoutPanel
        {
            Dock       = DockStyle.Fill,
            ColumnCount = 2,
            RowCount   = rows,
            Padding    = new Padding(20, 14, 20, 14),
            AutoScroll = true
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < rows; i++)
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        page.Controls.Add(t);
        return t;
    }

    private static Label AddRow(TableLayoutPanel t, int row, Control ctrl)
    {
        var lbl = new Label
        {
            Dock        = DockStyle.Fill,
            TextAlign   = ContentAlignment.MiddleRight,
            Margin      = new Padding(0, 0, 8, 0)
        };
        ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
        ctrl.Margin = new Padding(0, 5, 4, 5);
        t.Controls.Add(lbl, 0, row);
        t.Controls.Add(ctrl, 1, row);
        return lbl;
    }

    private static Label AddHint(TableLayoutPanel t, int row, int height = 44)
    {
        t.RowStyles[row] = new RowStyle(SizeType.Absolute, height);
        var lbl = new Label
        {
            Dock      = DockStyle.Fill,
            ForeColor = Color.DimGray,
            Font      = new Font("Segoe UI", 8f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(2, 0, 2, 0)
        };
        t.SetColumnSpan(lbl, 2);
        t.Controls.Add(lbl, 0, row);
        return lbl;
    }

    private void BuildStationTab()
    {
        var t = MakeTable(_pgStation, 5);

        _txtCallsign = new TextBox { CharacterCasing = CharacterCasing.Upper };
        _lCallsign   = AddRow(t, 0, _txtCallsign);

        _nLat = new NumericUpDown { DecimalPlaces = 6, Minimum = -90m,  Maximum = 90m,  Increment = 0.001m };
        _lLat = AddRow(t, 1, _nLat);

        _nLon = new NumericUpDown { DecimalPlaces = 6, Minimum = -180m, Maximum = 180m, Increment = 0.001m };
        _lLon = AddRow(t, 2, _nLon);

        t.RowStyles[3] = new RowStyle(SizeType.Absolute, 8);
        _lQthHint = AddHint(t, 4, 48);
    }

    private void BuildApiTab()
    {
        var t = MakeTable(_pgApi, 9);

        _txtApiUrl   = new TextBox();
        _lApiUrl     = AddRow(t, 0, _txtApiUrl);

        t.RowStyles[1] = new RowStyle(SizeType.Absolute, 150);
        _gridProfiles = new DataGridView
        {
            Dock                    = DockStyle.Fill,
            AllowUserToAddRows      = false,
            AllowUserToDeleteRows   = false,
            RowHeadersVisible       = false,
            SelectionMode           = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect             = false,
            AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill,
            Margin                  = new Padding(0, 4, 0, 4),
            Font                    = new Font("Segoe UI", 8.5f)
        };
        _gridProfiles.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled",    HeaderText = "",  FillWeight = 8  });
        _gridProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",       FillWeight = 22 });
        _gridProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "InstanceId", FillWeight = 25 });
        _gridProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "ApiKey",     FillWeight = 45 });
        t.Controls.Add(new Label(), 0, 1);
        t.Controls.Add(_gridProfiles, 1, 1);

        t.RowStyles[2] = new RowStyle(SizeType.AutoSize);
        var btnPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor   = AnchorStyles.Left | AnchorStyles.Top,
            Margin   = new Padding(0, 4, 0, 4)
        };
        _btnProfileAdd = MakeSecondaryButton();
        _btnProfileAdd.Click += OnAddProfile;
        _btnProfileRemove = MakeSecondaryButton();
        _btnProfileRemove.Click += OnRemoveProfile;
        _btnProfileRemove.Margin = new Padding(8, 3, 0, 3);
        btnPanel.Controls.Add(_btnProfileAdd);
        btnPanel.Controls.Add(_btnProfileRemove);
        t.Controls.Add(new Label(), 0, 2);
        t.Controls.Add(btnPanel, 1, 2);

        _lApiHint    = AddHint(t, 3, 48);

        _nTimeout    = new NumericUpDown { Minimum = 5,  Maximum = 300, Value = 30 };
        _lApiTimeout = AddRow(t, 4, _nTimeout);

        _nRetry      = new NumericUpDown { Minimum = 0,  Maximum = 10,  Value = 3  };
        _lApiRetry   = AddRow(t, 5, _nRetry);

        _nDelay      = new NumericUpDown { Minimum = 1,  Maximum = 60,  Value = 2  };
        _lApiDelay   = AddRow(t, 6, _nDelay);

        t.RowStyles[7] = new RowStyle(SizeType.Absolute, 10);
        t.RowStyles[8] = new RowStyle(SizeType.AutoSize);

        _btnTestApi = new Button
        {
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            Cursor    = Cursors.Hand,
            Anchor    = AnchorStyles.Left | AnchorStyles.Top,
            AutoSize  = true,
            Margin    = new Padding(0, 3, 0, 3),
            Padding   = new Padding(14, 4, 14, 4)
        };
        _btnTestApi.FlatAppearance.BorderSize = 0;
        _btnTestApi.Click += OnTestApi;

        t.Controls.Add(new Label(), 0, 8);
        t.Controls.Add(_btnTestApi, 1, 8);
    }

    private void OnAddProfile(object? s, EventArgs e)
    {
        var idx = _gridProfiles.Rows.Add(true, "", "", "");
        _gridProfiles.Rows[idx].Tag = new ApiProfile();
    }

    private void OnRemoveProfile(object? s, EventArgs e)
    {
        if (_gridProfiles.CurrentRow is { } row && !row.IsNewRow)
            _gridProfiles.Rows.Remove(row);
    }

    private void BuildHamQthTab()
    {
        var t = MakeTable(_pgHamQth, 5);

        _lHamQthInfo = AddHint(t, 0, 52);

        _txtHamQthUser = new TextBox();
        _lHamQthUser   = AddRow(t, 1, _txtHamQthUser);

        _txtHamQthPass = new TextBox { PasswordChar = '●' };
        _lHamQthPass   = AddRow(t, 2, _txtHamQthPass);

        _nHamQthCache = new NumericUpDown { Minimum = 60, Maximum = 10080, Value = 1440, Increment = 60 };
        _lHamQthCache = AddRow(t, 3, _nHamQthCache);
    }

    private void BuildN1mmTab()
    {
        var t = MakeTable(_pgN1mm, 7);

        _nN1mmPort   = new NumericUpDown { Minimum = 1024, Maximum = 65535, Value = 12060 };
        _lN1mmPort   = AddRow(t, 0, _nN1mmPort);

        _txtN1mmAddr = new TextBox { Text = LogBeam.Core.Net.UdpListenAddress.Default };
        _lN1mmAddr   = AddRow(t, 1, _txtN1mmAddr);

        t.RowStyles[2] = new RowStyle(SizeType.Absolute, 8);
        _lN1mmHint = AddHint(t, 3, 52);

        t.RowStyles[4] = new RowStyle(SizeType.Absolute, 10);
        t.RowStyles[5] = new RowStyle(SizeType.AutoSize);

        var btnPanel = new FlowLayoutPanel
        {
            AutoSize     = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor       = AnchorStyles.Left | AnchorStyles.Top,
            Margin       = new Padding(0, 4, 0, 4)
        };
        _btnN1mmDetect = MakeSecondaryButton();
        _btnN1mmDetect.Click += OnN1mmDetect;
        _btnN1mmAutoConfig = MakeSecondaryButton();
        _btnN1mmAutoConfig.Click += OnN1mmAutoConfig;
        _btnN1mmAutoConfig.Margin = new Padding(8, 3, 0, 3);
        btnPanel.Controls.Add(_btnN1mmDetect);
        btnPanel.Controls.Add(_btnN1mmAutoConfig);
        t.Controls.Add(new Label(), 0, 5);
        t.Controls.Add(btnPanel, 1, 5);

        _lN1mmStatus = AddHint(t, 6, 44);
    }

    private void BuildWsjtxTab()
    {
        var t = MakeTable(_pgWsjtx, 4);

        _chkWsjtxEnabled = new CheckBox { AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 0, 6) };
        t.Controls.Add(new Label(), 0, 0);
        t.Controls.Add(_chkWsjtxEnabled, 1, 0);

        _nWsjtxPort = new NumericUpDown { Minimum = 1024, Maximum = 65535, Value = 2237 };
        _lWsjtxPort = AddRow(t, 1, _nWsjtxPort);

        t.RowStyles[2] = new RowStyle(SizeType.Absolute, 8);
        _lWsjtxHint = AddHint(t, 3, 60);
    }

    private static Button MakeSecondaryButton() => new()
    {
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(0, 102, 204),
        ForeColor = Color.White,
        Cursor    = Cursors.Hand,
        AutoSize  = true,
        Padding   = new Padding(14, 4, 14, 4)
    };

    private void OnN1mmDetect(object? s, EventArgs e)
    {
        var n1mmConfig = N1mmConfig();
        _n1mmFolder = n1mmConfig.FindN1MMFolder();
        if (_n1mmFolder is null)
        {
            _lN1mmStatus.Text = L.Get("n1mm_status_not_found");
            return;
        }

        var status = n1mmConfig.ReadStatus(_n1mmFolder, (int)_nN1mmPort.Value);
        _lN1mmStatus.Text = status switch
        {
            LogBeam.Core.N1MM.N1MMBroadcastStatus.ConfiguredCorrectly    => L.Get("n1mm_status_ok"),
            LogBeam.Core.N1MM.N1MMBroadcastStatus.ConfiguredDifferentPort => L.Get("n1mm_status_wrong_port"),
            LogBeam.Core.N1MM.N1MMBroadcastStatus.NotConfigured           => L.Get("n1mm_status_not_configured"),
            _                                                              => L.Get("n1mm_status_ini_missing")
        };
    }

    private void OnN1mmAutoConfig(object? s, EventArgs e)
    {
        var n1mmConfig = N1mmConfig();
        _n1mmFolder ??= n1mmConfig.FindN1MMFolder();
        if (_n1mmFolder is null)
        {
            _lN1mmStatus.Text = L.Get("n1mm_status_not_found");
            return;
        }

        var port = (int)_nN1mmPort.Value;
        if (n1mmConfig.ReadStatus(_n1mmFolder, port) != LogBeam.Core.N1MM.N1MMBroadcastStatus.ConfiguredCorrectly
            && n1mmConfig.IsN1MMRunning())
        {
            var confirm = MessageBox.Show(L.Get("n1mm_confirm_running"), L.Get("n1mm_confirm_title"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
        }

        _lN1mmStatus.Text = n1mmConfig.ApplyConfiguration(_n1mmFolder, "127.0.0.1", port) switch
        {
            LogBeam.Core.N1MM.N1MMApplyResult.AlreadyConfigured => L.Get("n1mm_status_ok"),
            LogBeam.Core.N1MM.N1MMApplyResult.Applied           => L.Get("n1mm_status_configured"),
            _                                                    => L.Get("n1mm_status_config_failed")
        };
    }

    private void BuildClubLogTab()
    {
        var t = MakeTable(_pgClubLog, 6);

        _lClHint = AddHint(t, 0, 68);

        _chkClEnabled = new CheckBox
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 6, 0, 6)
        };
        t.Controls.Add(new Label(), 0, 1);
        t.Controls.Add(_chkClEnabled, 1, 1);

        _txtClEmail = new TextBox();
        _lClEmail   = AddRow(t, 2, _txtClEmail);

        _txtClPass  = new TextBox { PasswordChar = '\u25cf' };
        _lClPass    = AddRow(t, 3, _txtClPass);

        _txtClCall  = new TextBox { CharacterCasing = CharacterCasing.Upper };
        _lClCall    = AddRow(t, 4, _txtClCall);
    }

    private void BuildAdvancedTab()
    {
        var t = MakeTable(_pgAdvanced, 3);

        _cboLogLevel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cboLogLevel.Items.AddRange(new object[] { "Debug", "Information", "Warning", "Error" });
        _cboLogLevel.SelectedIndex = 1;
        _lLogLevel = AddRow(t, 0, _cboLogLevel);

        _txtLogPath = new TextBox();
        _lLogPath   = AddRow(t, 1, _txtLogPath);

        _chkNotifyEachQso = new CheckBox { AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 0, 6) };
        t.Controls.Add(new Label(), 0, 2);
        t.Controls.Add(_chkNotifyEachQso, 1, 2);
    }

    // ─── Idioma ────────────────────────────────────────────────────────────
    private void ChangeLanguage(string lang)
    {
        L.Lang            = lang;
        _prefs.Language   = lang;
        _prefsMgr.SavePrefs(_prefs);
        UpdateAllText();
    }

    private void UpdateAllText()
    {
        _pgStation.Text  = L.Get("tab_station");
        _pgApi.Text      = L.Get("tab_api");
        _pgHamQth.Text   = L.Get("tab_hamqth");
        _pgAdvanced.Text = L.Get("tab_advanced");
        _pgClubLog.Text  = L.Get("tab_clublog");

        _lCallsign.Text  = L.Get("lbl_callsign");
        _lLat.Text       = L.Get("lbl_lat");
        _lLon.Text       = L.Get("lbl_lon");
        _lQthHint.Text   = L.Get("hint_qth");

        _lApiUrl.Text        = L.Get("lbl_api_url");
        _lApiHint.Text       = L.Get("hint_api_key");
        _lApiTimeout.Text    = L.Get("lbl_api_timeout");
        _lApiRetry.Text      = L.Get("lbl_api_retry");
        _lApiDelay.Text      = L.Get("lbl_api_retry_delay");
        _btnTestApi.Text     = L.Get("btn_test_api");
        _btnProfileAdd.Text    = L.Get("btn_profile_add");
        _btnProfileRemove.Text = L.Get("btn_profile_remove");
        _gridProfiles.Columns["Name"].HeaderText       = L.Get("col_profile_name");
        _gridProfiles.Columns["InstanceId"].HeaderText = L.Get("col_profile_instance");
        _gridProfiles.Columns["ApiKey"].HeaderText     = L.Get("col_profile_key");

        _lHamQthInfo.Text  = L.Get("hint_hamqth");
        _lHamQthUser.Text  = L.Get("lbl_hamqth_user");
        _lHamQthPass.Text  = L.Get("lbl_hamqth_pass");
        _lHamQthCache.Text = L.Get("lbl_hamqth_cache");

        _pgN1mm.Text     = L.Get("tab_n1mm");
        _lN1mmPort.Text  = L.Get("lbl_n1mm_port");
        _lN1mmAddr.Text  = L.Get("lbl_n1mm_addr");
        _lN1mmHint.Text  = L.Get("hint_n1mm");
        _btnN1mmDetect.Text     = L.Get("btn_n1mm_detect");
        _btnN1mmAutoConfig.Text = L.Get("btn_n1mm_autoconfig");

        _pgWsjtx.Text           = L.Get("tab_wsjtx");
        _chkWsjtxEnabled.Text   = L.Get("lbl_wsjtx_enabled");
        _lWsjtxPort.Text        = L.Get("lbl_wsjtx_port");
        _lWsjtxHint.Text        = L.Get("hint_wsjtx");

        _lClHint.Text       = L.Get("hint_cl");
        _chkClEnabled.Text  = L.Get("lbl_cl_enabled");
        _lClEmail.Text      = L.Get("lbl_cl_email");
        _lClPass.Text       = L.Get("lbl_cl_pass");
        _lClCall.Text       = L.Get("lbl_cl_call");

        _lLogLevel.Text  = L.Get("lbl_log_level");
        _lLogPath.Text   = L.Get("lbl_log_path");
        _chkNotifyEachQso.Text = L.Get("lbl_notify_each_qso");

        _miN1mmHelp.Text = L.Get("menu_n1mm_help");

        _btnSave.Text    = L.Get("btn_save");
        _btnCancel.Text  = L.Get("btn_cancel");
        _btnExit.Text    = L.Get("btn_exit");

        foreach (var (lang, btn) in _langButtons)
            btn.BackColor = L.Lang == lang ? Color.FromArgb(0, 102, 204) : Color.FromArgb(55, 55, 55);

        _miFile.Text       = L.Get("menu_file");
        _miExportAdif.Text = L.Get("menu_export_adif");

        _miHelp.Text        = L.Get("menu_help");
        _miAbout.Text       = L.Get("menu_about");
        _miCheckUpdate.Text = L.Get("menu_check_update");
    }

    // ─── Form ↔ Dados ──────────────────────────────────────────────────────
    private static decimal Clamp(decimal v, decimal min, decimal max) =>
        v < min ? min : v > max ? max : v;

    private void PopulateForm()
    {
        _txtCallsign.Text = _settings.MyCallsign;
        _nLat.Value       = Clamp(_settings.MyLatitude,  -90m, 90m);
        _nLon.Value       = Clamp(_settings.MyLongitude, -180m, 180m);

        _txtApiUrl.Text   = _settings.Api.BaseUrl;
        _nTimeout.Value   = Clamp(_settings.Api.TimeoutSeconds,    5, 300);
        _nRetry.Value     = Clamp(_settings.Api.RetryCount,         0, 10);
        _nDelay.Value     = Clamp(_settings.Api.RetryDelaySeconds,  1, 60);

        _gridProfiles.Rows.Clear();
        foreach (var p in _settings.Api.Profiles)
        {
            var rowIdx = _gridProfiles.Rows.Add(p.Enabled, p.Name, p.InstanceId, p.ApiKey);
            _gridProfiles.Rows[rowIdx].Tag = p;
        }

        _nN1mmPort.Value   = Clamp(_settings.N1mm.UdpPort, 1024, 65535);
        _txtN1mmAddr.Text  = string.IsNullOrEmpty(_settings.N1mm.ListenAddress) ? LogBeam.Core.Net.UdpListenAddress.Default : _settings.N1mm.ListenAddress;

        _chkWsjtxEnabled.Checked = _settings.Wsjtx.Enabled;
        _nWsjtxPort.Value        = Clamp(_settings.Wsjtx.UdpPort, 1024, 65535);

        _chkClEnabled.Checked = _settings.ClubLog.Enabled;
        _txtClEmail.Text      = _settings.ClubLog.Email;
        _txtClPass.Text       = _settings.ClubLog.PasswordEncrypted;
        _txtClCall.Text       = _settings.ClubLog.Callsign;

        _txtHamQthUser.Text  = _settings.HamQth.Username;
        _txtHamQthPass.Text  = _settings.HamQth.PasswordEncrypted;
        _nHamQthCache.Value  = Clamp(_settings.HamQth.CacheTtlMinutes, 60, 10080);

        var idx = _cboLogLevel.Items.IndexOf(_settings.LogLevel);
        _cboLogLevel.SelectedIndex = idx >= 0 ? idx : 1;
        _txtLogPath.Text  = _settings.LogPath;
        _chkNotifyEachQso.Checked = _prefs.NotifyEachQso;
    }

    private void CollectForm()
    {
        _settings.MyCallsign = _txtCallsign.Text.Trim().ToUpperInvariant();
        _settings.MyLatitude  = _nLat.Value;
        _settings.MyLongitude = _nLon.Value;

        _settings.Api.BaseUrl           = _txtApiUrl.Text.Trim().TrimEnd('/');
        _settings.Api.TimeoutSeconds    = (int)_nTimeout.Value;
        _settings.Api.RetryCount        = (int)_nRetry.Value;
        _settings.Api.RetryDelaySeconds = (int)_nDelay.Value;

        _gridProfiles.EndEdit();
        var profiles = new List<ApiProfile>();
        foreach (DataGridViewRow row in _gridProfiles.Rows)
        {
            if (row.IsNewRow) continue;
            var existing = row.Tag as ApiProfile;
            var profile = new ApiProfile
            {
                Id         = existing?.Id ?? Guid.NewGuid().ToString("N"),
                Enabled    = row.Cells["Enabled"].Value is bool b && b,
                Name       = row.Cells["Name"].Value?.ToString()?.Trim() ?? string.Empty,
                InstanceId = row.Cells["InstanceId"].Value?.ToString()?.Trim() ?? string.Empty,
                ApiKey     = row.Cells["ApiKey"].Value?.ToString()?.Trim() ?? string.Empty
            };
            if (!string.IsNullOrEmpty(profile.Name) || !string.IsNullOrEmpty(profile.InstanceId) || !string.IsNullOrEmpty(profile.ApiKey))
                profiles.Add(profile);
        }
        _settings.Api.Profiles = profiles;

        _settings.N1mm.UdpPort       = (int)_nN1mmPort.Value;
        _settings.N1mm.ListenAddress = _txtN1mmAddr.Text.Trim();

        _settings.Wsjtx.Enabled = _chkWsjtxEnabled.Checked;
        _settings.Wsjtx.UdpPort = (int)_nWsjtxPort.Value;

        _settings.ClubLog.Enabled             = _chkClEnabled.Checked;
        _settings.ClubLog.Email               = _txtClEmail.Text.Trim();
        _settings.ClubLog.PasswordEncrypted   = _txtClPass.Text;
        _settings.ClubLog.Callsign            = _txtClCall.Text.Trim().ToUpperInvariant();

        _settings.HamQth.Username          = _txtHamQthUser.Text.Trim();
        _settings.HamQth.PasswordEncrypted = _txtHamQthPass.Text;
        _settings.HamQth.CacheTtlMinutes   = (int)_nHamQthCache.Value;

        _settings.LogLevel = _cboLogLevel.SelectedItem?.ToString() ?? "Information";
        _settings.LogPath  = _txtLogPath.Text.Trim();

        _prefs.NotifyEachQso = _chkNotifyEachQso.Checked;
        _prefsMgr.SavePrefs(_prefs);
    }

    // ─── Eventos ───────────────────────────────────────────────────────────
    private void OnSave(object? s, EventArgs e)
    {
        CollectForm();
        try
        {
            _mgr.Save(_settings);
            _runner.Stop();
            _qsoCount = 0;
            _runner.Start();
            SetStatus("ok", L.Get("status_saved"));
        }
        catch (Exception ex)
        {
            SetStatus("err", $"{L.Get("status_error")}: {ex.Message}");
        }
    }

    private void OnCancel(object? s, EventArgs e)
    {
        _settings = _mgr.Load();
        PopulateForm();
        SetStatus("", "");
    }

    private async void OnTestApi(object? s, EventArgs e)
    {
        _gridProfiles.EndEdit();
        var url = _txtApiUrl.Text.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(url)) { SetStatus("err", L.Get("status_no_url")); return; }

        var profiles = new List<(string Name, string InstanceId, string ApiKey)>();
        foreach (DataGridViewRow row in _gridProfiles.Rows)
        {
            if (row.IsNewRow) continue;
            if (row.Cells["Enabled"].Value is not bool enabled || !enabled) continue;
            var name = row.Cells["Name"].Value?.ToString() ?? "";
            var iid  = row.Cells["InstanceId"].Value?.ToString()?.Trim() ?? "";
            var key  = row.Cells["ApiKey"].Value?.ToString()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(iid) && !string.IsNullOrEmpty(key))
                profiles.Add((string.IsNullOrEmpty(name) ? iid : name, iid, key));
        }

        if (profiles.Count == 0) { SetStatus("err", L.Get("status_no_credentials")); return; }

        _btnTestApi.Enabled = false;
        SetStatus("", L.Get("status_testing"));
        var results = new List<string>();
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            foreach (var p in profiles)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/instance/{p.InstanceId}");
                    request.Headers.Add("X-API-Key", p.ApiKey);
                    var r = await http.SendAsync(request);

                    results.Add(r.IsSuccessStatusCode ? $"{p.Name}: {L.Get("status_api_ok")}"
                        : r.StatusCode == System.Net.HttpStatusCode.Unauthorized ? $"{p.Name}: {L.Get("status_api_unauthorized")}"
                        : r.StatusCode == System.Net.HttpStatusCode.NotFound     ? $"{p.Name}: {L.Get("status_api_not_found")}"
                        : $"{p.Name}: HTTP {(int)r.StatusCode}");
                }
                catch (Exception ex)
                {
                    results.Add($"{p.Name}: {ex.Message}");
                }
            }

            var allOk = results.TrueForAll(r => r.EndsWith(L.Get("status_api_ok")));
            SetStatus(allOk ? "ok" : "err", string.Join("   ", results));
        }
        finally { _btnTestApi.Enabled = true; }
    }

    private void SetStatus(string type, string msg)
    {
        _lblStatus.Text      = msg;
        _lblStatus.ForeColor = type == "ok"  ? Color.FromArgb(0, 128, 0) :
                               type == "err" ? Color.Crimson : Color.DimGray;
    }
}

