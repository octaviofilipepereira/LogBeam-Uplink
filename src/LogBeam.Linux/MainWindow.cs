// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LogBeam.Core.Adif;
using LogBeam.Core.Api;
using LogBeam.Core.Models;
using LogBeam.Core.N1MM;
using LogBeam.Core.Net;
using LogBeam.Core.Platform;
using LogBeam.Service.Models;
using LogBeam.Service.Settings;
using LogBeam.UI;
using LogBeam.UI.Localization;
using LogBeam.UI.Models;
using LogBeam.UI.Settings;

namespace LogBeam.Linux;

/// <summary>
/// Janela principal: os mesmos separadores e o mesmo comportamento da versão Windows.
/// Fechar a janela deixa o serviço a correr, com o ícone na área de notificação.
/// </summary>
public sealed class MainWindow : Window
{
    private static readonly IBrush Dark     = new SolidColorBrush(Color.FromRgb(24, 24, 24));
    private static readonly IBrush MenuDark = new SolidColorBrush(Color.FromRgb(36, 36, 36));
    private static readonly IBrush LangOff  = new SolidColorBrush(Color.FromRgb(55, 55, 55));
    private static readonly IBrush Bar      = new SolidColorBrush(Color.FromRgb(242, 242, 242));
    private static readonly IBrush Green    = new SolidColorBrush(Color.FromRgb(0, 128, 0));

    private readonly SettingsManager _mgr = new();
    private readonly UiPrefsManager  _prefsMgr = new();
    private AppSettings   _settings;
    private readonly UiPreferences _prefs;
    private readonly PosixSignalRegistration _sigterm;

    private readonly ServiceRunner _runner = new();
    private readonly List<Action>  _texts = new();   // reaplicam os textos ao mudar de língua
    private readonly Dictionary<string, Button> _langButtons = new();
    private int  _qsoCount;
    private bool _exiting;
    private bool _startHidden;

    // Tray
    private TrayIcon _tray = null!;
    private NativeMenuItem _tmiOpen = null!, _tmiSendTo = null!, _tmiStart = null!, _tmiStop = null!, _tmiExit = null!;

    // Separadores
    private TabControl _tabs = null!;
    private TabItem    _pgApi = null!;

    // Station
    private TextBox _txtCallsign = null!;

    // API
    private TextBox _txtApiUrl = null!;
    private NumericUpDown _nTimeout = null!, _nRetry = null!, _nDelay = null!;
    private StackPanel _pnlProfiles = null!;
    private Button _btnTestApi = null!, _btnShowKeys = null!;
    private readonly List<ProfileRow> _rows = new();
    private ProfileRow? _currentRow;
    private bool _showKeys;

    // N1MM
    private NumericUpDown _nN1mmPort = null!;
    private TextBox   _txtN1mmAddr = null!;
    private TextBlock _lN1mmStatus = null!;
    private string?   _n1mmFolder;

    // WSJT-X
    private CheckBox _chkWsjtxEnabled = null!;
    private NumericUpDown _nWsjtxPort = null!;
    private TextBox  _txtWsjtxAddr = null!;

    // Log4OM
    private CheckBox _chkLog4omEnabled = null!;
    private NumericUpDown _nLog4omPort = null!;

    // ClubLog
    private CheckBox  _chkClEnabled = null!;
    private TextBox   _txtClEmail = null!, _txtClPass = null!, _txtClCall = null!;
    private TextBlock _lClError = null!;
    private bool      _clubLogRejected;

    // Avançado
    private ComboBox _cboLogLevel = null!;
    private TextBox  _txtLogPath = null!;
    private CheckBox _chkNotifyEachQso = null!, _chkStart = null!, _chkTelemetry = null!;

    // Barra inferior
    private TextBlock _lblStatus = null!;
    private Button    _btnSave = null!;

    private DateTime _importantNoticeUntil;
    private bool ImportantNoticeShowing => DateTime.UtcNow < _importantNoticeUntil;

    /// <summary>Linha de um logbook no separador API.</summary>
    private sealed class ProfileRow
    {
        public required ApiProfile Source { get; init; }
        public required CheckBox Enabled  { get; init; }
        public required TextBox  Name     { get; init; }
        public required TextBox  Instance { get; init; }
        public required TextBox  Key      { get; init; }
        public required Grid     View     { get; init; }
    }

    public MainWindow(bool startHidden)
    {
        _startHidden = startHidden;
        _settings = _mgr.Load();
        _prefs    = _prefsMgr.LoadPrefs();
        // Primeira vez: a língua do sistema, se for uma das quatro.
        if (!File.Exists(Path.Combine(AppPaths.DataDir, "ui.json")))
        {
            var lang = (Environment.GetEnvironmentVariable("LANG") ?? "").Split('_')[0].ToUpperInvariant();
            if (L.Languages.Contains(lang)) _prefs.Language = lang;
        }
        L.Lang    = _prefs.Language;

        Title     = AppVersion.Title;
        Width     = 780;
        Height    = 760;
        MinWidth  = 700;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = new WindowIcon(LogoStream());

        Content = BuildContent();
        BuildTray();
        PopulateForm();
        UpdateAllText();

        _runner.StatusChanged              += s => OnUi(() => OnServiceStatusChanged(s));
        _runner.QsoResult                  += (q, ok) => OnUi(() => OnQsoResult(q, ok));
        _runner.QsoConfirmedByLogbeam      += q => OnUi(() => Notifier.Show(L.Get("balloon_confirmed_title"), string.Format(L.Get("balloon_confirmed_text"), q.Call), 5000));
        _runner.QsoAlreadyLogged           += q => OnUi(() => OnQsoAlreadyLogged(q));
        _runner.ListenerFailed             += (p, port) => OnUi(() => OnListenerFailed(p, port));
        _runner.ClubLogCredentialsRejected += () => OnUi(OnClubLogCredentialsRejected);

        Closing += (_, e) =>
        {
            if (_exiting) return;
            e.Cancel = true;
            Hide(); // o serviço continua, na área de notificação
        };
        Opened += OnFirstOpened;

        // Fim de sessão ou desligar: sair como no botão Sair (sem isto, só o serviço parava e a janela ficava).
        _sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, c => { c.Cancel = true; OnUi(ExitApp); });

        if (!startHidden) return;
        if (_settings.Telemetry.Consent is null) Show();
        else StartService();
    }

    private static Stream LogoStream() =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream("logbeam.png")!;

    private static void OnUi(Action action) => Dispatcher.UIThread.Post(action);

    private async void OnFirstOpened(object? s, EventArgs e)
    {
        Opened -= OnFirstOpened;
        // Primeiro arranque: dados da instalação e relatórios de erros só com autorização explícita.
        if (_settings.Telemetry.Consent is null) await AskTelemetryConsent();
        StartService();
    }

    private void StartService()
    {
        _runner.Language = L.Lang;
        _runner.Start();
        UpdateTray();
    }

    // ─── Tray ───────────────────────────────────────────────────────────
    private void BuildTray()
    {
        _tmiOpen   = new NativeMenuItem();
        _tmiSendTo = new NativeMenuItem { Menu = new NativeMenu() };
        _tmiStart  = new NativeMenuItem();
        _tmiStop   = new NativeMenuItem();
        _tmiExit   = new NativeMenuItem();

        _tmiOpen.Click  += (_, _) => ShowWindow();
        _tmiStart.Click += (_, _) => { _qsoCount = 0; _runner.Start(); UpdateTray(); };
        _tmiStop.Click  += async (_, _) => { await Task.Run(_runner.Stop); UpdateTray(); };
        _tmiExit.Click  += (_, _) => ExitApp();

        var menu = new NativeMenu();
        menu.Add(_tmiOpen);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(_tmiSendTo);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(_tmiStart);
        menu.Add(_tmiStop);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(_tmiExit);

        _tray = new TrayIcon { Icon = new WindowIcon(LogoStream()), Menu = menu, IsVisible = true };
        _tray.Clicked += (_, _) => ShowWindow();
        TrayIcon.SetIcons(Application.Current!, new TrayIcons { _tray });

        UpdateTray();
        RefreshTrayProfiles();
    }

    private void UpdateTray()
    {
        var running = _runner.IsRunning;
        _tray.ToolTipText = running ? string.Format(L.Get("tray_running_count"), _qsoCount) : L.Get("tray_stopped");

        _tmiStart.IsEnabled = !running;
        _tmiStop.IsEnabled  = running;
        _tmiOpen.Header     = L.Get("tray_open");
        _tmiSendTo.Header   = L.Get("tray_send_to");
        _tmiStart.Header    = L.Get("tray_start");
        _tmiStop.Header     = L.Get("tray_stop");
        _tmiExit.Header     = L.Get("tray_exit");
    }

    /// <summary>Submenu "Enviar para": um item por logbook, marcado se estiver activo.</summary>
    private void RefreshTrayProfiles()
    {
        var menu = _tmiSendTo.Menu!;
        menu.Items.Clear();
        foreach (var p in _settings.Api.Profiles)
        {
            var item = new NativeMenuItem(string.IsNullOrWhiteSpace(p.Name) ? p.InstanceId : p.Name)
            {
                ToggleType = NativeMenuItemToggleType.CheckBox,
                IsChecked  = p.Enabled
            };
            var id = p.Id;
            item.Click += (_, _) => OnTrayToggleProfile(id);
            menu.Add(item);
        }

        if (menu.Items.Count == 0)
            menu.Add(new NativeMenuItem(L.Get("tray_no_logbooks")) { IsEnabled = false });
    }

    /// <summary>Liga/desliga um logbook a partir da área de notificação: grava logo e reinicia o serviço.</summary>
    private async void OnTrayToggleProfile(string id)
    {
        var profile = _settings.Api.Profiles.FirstOrDefault(p => p.Id == id);
        if (profile is null) return;

        profile.Enabled = !profile.Enabled;
        try
        {
            _mgr.Save(_settings);
            await RestartServiceAsync();
        }
        catch (Exception ex)
        {
            profile.Enabled = !profile.Enabled;
            SetStatus("err", $"{L.Get("status_error")}: {ex.Message}");
        }

        // A janela pode ter alterações por gravar: muda-se só a caixa deste logbook.
        foreach (var row in _rows)
            if (row.Source.Id == id) row.Enabled.IsChecked = profile.Enabled;

        RefreshTrayProfiles();
        UpdateTray();
    }

    private void OnQsoResult(QsoRecord qso, bool success)
    {
        if (success) _qsoCount++;
        UpdateTray();
        // Por omissão só as falhas geram aviso; o aviso por QSO enviado é opcional (separador Avançado).
        if ((success && !_prefs.NotifyEachQso) || ImportantNoticeShowing) return;
        Notifier.Show(L.Get(success ? "balloon_qso_sent" : "balloon_qso_failed"), $"{qso.Call} — {qso.Band} {qso.Mode}", 4000, !success);
    }

    /// <summary>O QSO já existia em todos os logbooks: não conta como enviado; aviso só com "Avisar a cada QSO".</summary>
    private void OnQsoAlreadyLogged(QsoRecord qso)
    {
        if (!_prefs.NotifyEachQso || ImportantNoticeShowing) return;
        Notifier.Show(L.Get("balloon_qso_duplicate"), $"{qso.Call} — {qso.Band} {qso.Mode}", 4000);
    }

    /// <summary>Um receptor não arrancou (porta ocupada): aviso na barra de estado e notificação.</summary>
    private void OnListenerFailed(string program, int port)
    {
        var text = string.Format(L.Get("status_listener_failed"), program, port);
        SetStatus("err", text);
        Notifier.Show(L.Get("balloon_listener_failed"), text, 8000, warning: true);
    }

    /// <summary>
    /// O ClubLog recusou as credenciais: uma notificação (o serviço só a dispara uma vez) e a mensagem
    /// de erro no separador ClubLog, que fica até as definições serem gravadas de novo.
    /// </summary>
    private void OnClubLogCredentialsRejected()
    {
        _clubLogRejected = true;
        _lClError.Text   = L.Get("status_clublog_rejected");
        Notifier.Show(L.Get("balloon_clublog_rejected"), L.Get("balloon_clublog_rejected_text"), 10000, warning: true);
        _importantNoticeUntil = DateTime.UtcNow.AddSeconds(10);
    }

    internal void ShowWindow()
    {
        _startHidden = false;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private async Task RestartServiceAsync(string? revokedInstallationId = null)
    {
        await Task.Run(_runner.Stop);
        if (revokedInstallationId is not null) _runner.TelemetryStore.Revoke(revokedInstallationId);
        _runner.Language = L.Lang;
        _runner.Start();
    }

    private async void ExitApp()
    {
        if (_exiting) return;
        _exiting = true;

        SetStatus("", L.Get("busy_stopping"));
        await Task.Run(_runner.Stop);

        _tray.IsVisible = false;
        _tray.Dispose();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private void OnServiceStatusChanged(string status)
    {
        UpdateTray();
        SetStatus(status == "running" ? "ok" : status == "error" ? "err" : "",
            L.Get(status == "running" ? "svc_started" : status == "error" ? "svc_error" : "svc_stopped"));
    }

    // ─── Construção da janela ───────────────────────────────────────────
    private void Text(string key, Action<string> set) => _texts.Add(() => set(L.Get(key)));

    private Control BuildContent()
    {
        var dock = new DockPanel { LastChildFill = true };

        var menu = BuildMenu();
        DockPanel.SetDock(menu, Dock.Top);
        dock.Children.Add(menu);

        var top = BuildTopPanel();
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);

        var bottom = BuildBottomPanel();
        DockPanel.SetDock(bottom, Dock.Bottom);
        dock.Children.Add(bottom);

        _tabs = new TabControl { Margin = new Thickness(8, 4) };
        BuildStationTab();
        BuildApiTab();
        BuildN1mmTab();
        BuildWsjtxTab();
        BuildLog4omTab();
        BuildClubLogTab();
        BuildAdvancedTab();
        dock.Children.Add(_tabs);
        return dock;
    }

    private Menu BuildMenu()
    {
        MenuItem Item(string key, Action click)
        {
            var mi = new MenuItem { Foreground = Brushes.Black };
            Text(key, t => mi.Header = t);
            mi.Click += (_, _) => click();
            return mi;
        }
        MenuItem Head(string key, params MenuItem[] items)
        {
            var mi = new MenuItem { Foreground = Brushes.White, ItemsSource = items };
            Text(key, t => mi.Header = t);
            return mi;
        }

        return new Menu
        {
            Background = MenuDark,
            ItemsSource = new[]
            {
                Head("menu_file", Item("menu_export_adif", OnExportAdif)),
                Head("menu_help", Item("menu_about", OnAbout), Item("menu_n1mm_help", OnN1mmHelp))
            }
        };
    }

    private Control BuildTopPanel()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };

        var title = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        title.Children.Add(new Image { Source = new Bitmap(LogoStream()), Width = 26, Height = 26 });
        title.Children.Add(new TextBlock
        {
            Text = AppVersion.Title, Foreground = Brushes.White, FontSize = 16, FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center
        });
        grid.Children.Add(title);

        var langs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        foreach (var lang in L.Languages)
        {
            var btn = new Button { Content = lang, Foreground = Brushes.White, FontWeight = FontWeight.Bold, Width = 46, HorizontalContentAlignment = HorizontalAlignment.Center };
            btn.Click += (_, _) => ChangeLanguage(lang);
            _langButtons[lang] = btn;
            langs.Children.Add(btn);
        }
        Grid.SetColumn(langs, 2);
        grid.Children.Add(langs);

        return new Border { Background = Dark, Padding = new Thickness(12, 8), Child = grid };
    }

    private Control BuildBottomPanel()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };

        _lblStatus = new TextBlock
        {
            Foreground = Brushes.DimGray, FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0)
        };
        grid.Children.Add(_lblStatus);

        _btnSave = Dialogs.Primary("");
        var cancel = Dialogs.Secondary("");
        var exit   = Dialogs.Primary("");
        exit.Background = new SolidColorBrush(Color.FromRgb(180, 40, 40));
        Text("btn_save",   t => _btnSave.Content = t);
        Text("btn_cancel", t => cancel.Content = t);
        Text("btn_exit",   t => exit.Content = t);
        _btnSave.Click += OnSave;
        cancel.Click   += OnCancel;
        exit.Click     += (_, _) => ExitApp();

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(_btnSave);
        buttons.Children.Add(cancel);
        buttons.Children.Add(exit);
        Grid.SetColumn(buttons, 1);
        grid.Children.Add(buttons);

        return new Border { Background = Bar, Padding = new Thickness(12, 8), Child = grid };
    }

    private TabItem AddTab(string key, StackPanel body)
    {
        body.Margin  = new Thickness(16, 12);
        body.Spacing = 8;
        var tab = new TabItem { Content = new ScrollViewer { Content = body }, FontSize = 14 };
        Text(key, t => tab.Header = t);
        _tabs.Items.Add(tab);
        return tab;
    }

    private Grid Row(string key, Control control)
    {
        var grid  = new Grid { ColumnDefinitions = new ColumnDefinitions("170,*") };
        var label = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 0, 10, 0), TextWrapping = TextWrapping.Wrap };
        Text(key, t => label.Text = t);
        grid.Children.Add(label);
        Grid.SetColumn(control, 1);
        grid.Children.Add(control);
        return grid;
    }

    private TextBlock Hint(string key)
    {
        var hint = new TextBlock { Foreground = Brushes.DimGray, FontSize = 12, TextWrapping = TextWrapping.Wrap };
        Text(key, t => hint.Text = t);
        return hint;
    }

    private CheckBox Check(string key)
    {
        var check = new CheckBox();
        Text(key, t => check.Content = t);
        return check;
    }

    private static NumericUpDown Number(int min, int max) => new()
    {
        Minimum = min, Maximum = max, Increment = 1, FormatString = "0", HorizontalAlignment = HorizontalAlignment.Stretch
    };

    private static TextBox Upper()
    {
        var box = new TextBox();
        box.TextChanged += (_, _) =>
        {
            var up = box.Text?.ToUpperInvariant();
            if (up == box.Text) return;
            var caret = box.CaretIndex;
            box.Text = up;
            box.CaretIndex = caret;
        };
        return box;
    }

    private void BuildStationTab()
    {
        var p = new StackPanel();
        _txtCallsign = Upper();
        p.Children.Add(Row("lbl_callsign", _txtCallsign));
        p.Children.Add(Hint("hint_station"));
        AddTab("tab_station", p);
    }

    private void BuildApiTab()
    {
        var p = new StackPanel();

        _txtApiUrl = new TextBox();
        p.Children.Add(Row("lbl_api_url", _txtApiUrl));

        var head = ProfileGrid();
        foreach (var (col, key) in new[] { (0, "col_profile_enabled"), (1, "col_profile_name"), (2, "col_profile_instance"), (3, "col_profile_key") })
        {
            var h = new TextBlock { FontWeight = FontWeight.SemiBold, FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis };
            Text(key, t => h.Text = t);
            Grid.SetColumn(h, col);
            head.Children.Add(h);
        }
        _pnlProfiles = new StackPanel { Spacing = 4 };
        p.Children.Add(head);
        p.Children.Add(_pnlProfiles);

        var add    = Dialogs.Primary("");
        var remove = Dialogs.Primary("");
        _btnShowKeys = Dialogs.Primary("");
        Text("btn_profile_add",    t => add.Content = t);
        Text("btn_profile_remove", t => remove.Content = t);
        _texts.Add(UpdateShowKeysText);
        add.Click    += (_, _) => AddProfileRow(new ApiProfile());
        remove.Click += (_, _) => RemoveCurrentRow();
        _btnShowKeys.Click += (_, _) =>
        {
            _showKeys = !_showKeys;
            UpdateShowKeysText();
            foreach (var r in _rows) r.Key.RevealPassword = _showKeys;
        };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(add);
        buttons.Children.Add(remove);
        buttons.Children.Add(_btnShowKeys);
        p.Children.Add(buttons);
        p.Children.Add(Hint("hint_api_key"));

        _nTimeout = Number(5, 300);
        _nRetry   = Number(0, 10);
        _nDelay   = Number(1, 60);
        p.Children.Add(Row("lbl_api_timeout", _nTimeout));
        p.Children.Add(Row("lbl_api_retry", _nRetry));
        p.Children.Add(Row("lbl_api_retry_delay", _nDelay));

        _btnTestApi = Dialogs.Primary("");
        Text("btn_test_api", t => _btnTestApi.Content = t);
        _btnTestApi.Click += OnTestApi;
        p.Children.Add(_btnTestApi);

        _pgApi = AddTab("tab_api", p);
    }

    private void UpdateShowKeysText() => _btnShowKeys.Content = L.Get(_showKeys ? "btn_hide_keys" : "btn_show_keys");

    private static Grid ProfileGrid() => new() { ColumnDefinitions = new ColumnDefinitions("70,*,*,2*"), ColumnSpacing = 6 };

    private void AddProfileRow(ApiProfile profile)
    {
        var enabled  = new CheckBox { IsChecked = profile.Enabled, HorizontalAlignment = HorizontalAlignment.Center };
        var name     = new TextBox { Text = profile.Name };
        var instance = new TextBox { Text = profile.InstanceId };
        var key      = new TextBox { Text = profile.ApiKey, PasswordChar = '●', RevealPassword = _showKeys };

        var view = ProfileGrid();
        Grid.SetColumn(name, 1);
        Grid.SetColumn(instance, 2);
        Grid.SetColumn(key, 3);
        view.Children.Add(enabled);
        view.Children.Add(name);
        view.Children.Add(instance);
        view.Children.Add(key);

        var row = new ProfileRow { Source = profile, Enabled = enabled, Name = name, Instance = instance, Key = key, View = view };
        void Select(object? s, RoutedEventArgs e) => _currentRow = row;
        name.GotFocus += Select;
        instance.GotFocus += Select;
        key.GotFocus += Select;

        // O link do logbook passa a Instance ID; a API Key fica sem espaços e em minúsculas e
        // aparece em claro só enquanto é editada.
        instance.LostFocus += (_, _) =>
        {
            if (LogbookCredentials.ExtractInstanceId(instance.Text) is { } id) { instance.Text = id; ClearError(instance); }
        };
        key.GotFocus += (_, _) => key.RevealPassword = true;
        key.LostFocus += (_, _) =>
        {
            var raw = key.Text ?? string.Empty;
            key.Text = LogbookCredentials.NormalizeApiKey(raw);
            if (LogbookCredentials.IsApiKey(raw)) ClearError(key);
            key.RevealPassword = _showKeys;
        };

        _rows.Add(row);
        _pnlProfiles.Children.Add(view);
    }

    private void RemoveCurrentRow()
    {
        var row = _currentRow ?? _rows.LastOrDefault();
        if (row is null) return;
        _rows.Remove(row);
        _pnlProfiles.Children.Remove(row.View);
        _currentRow = null;
    }

    private static void ClearError(TextBox box)
    {
        box.ClearValue(BorderBrushProperty);
        ToolTip.SetTip(box, null);
    }

    /// <summary>
    /// Verifica o formato do Instance ID e da API Key de cada perfil. Os campos errados ficam a
    /// vermelho, com a explicação ao passar o rato, e o cursor vai para o primeiro. Devolve uma
    /// mensagem curta para a barra de estado, ou null. As linhas vazias são ignoradas.
    /// </summary>
    private string? ProfileFormatError()
    {
        string? first = null;
        foreach (var row in _rows)
        {
            ClearError(row.Instance);
            ClearError(row.Key);

            var name = row.Name.Text?.Trim() ?? string.Empty;
            var iid  = row.Instance.Text ?? string.Empty;
            var key  = row.Key.Text ?? string.Empty;
            if (name.Length == 0 && string.IsNullOrWhiteSpace(iid) && string.IsNullOrWhiteSpace(key)) continue;

            var label = name.Length > 0 ? name : iid.Trim();
            if (LogbookCredentials.ExtractInstanceId(iid) is null)
                first ??= MarkError(row.Instance, L.Get("status_bad_instance"), $"{label}: {L.Get("status_bad_instance_short")}");
            if (!LogbookCredentials.IsApiKey(key))
                first ??= MarkError(row.Key, L.Get("status_bad_api_key"), $"{label}: {L.Get("status_bad_api_key_short")}");
        }
        return first;
    }

    private string MarkError(TextBox box, string detail, string message)
    {
        box.BorderBrush = Brushes.Crimson;
        ToolTip.SetTip(box, detail);
        _tabs.SelectedItem = _pgApi;
        box.Focus();
        return message;
    }

    private void BuildN1mmTab()
    {
        var p = new StackPanel();
        _nN1mmPort   = Number(1024, 65535);
        _txtN1mmAddr = new TextBox();
        p.Children.Add(Row("lbl_n1mm_port", _nN1mmPort));
        p.Children.Add(Row("lbl_n1mm_addr", _txtN1mmAddr));
        p.Children.Add(Hint("hint_n1mm"));

        var detect = Dialogs.Primary("");
        var auto   = Dialogs.Primary("");
        Text("btn_n1mm_detect",     t => detect.Content = t);
        Text("btn_n1mm_autoconfig", t => auto.Content = t);
        detect.Click += OnN1mmDetect;
        auto.Click   += OnN1mmAutoConfig;
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(detect);
        buttons.Children.Add(auto);
        p.Children.Add(buttons);

        _lN1mmStatus = new TextBlock { Foreground = Brushes.DimGray, FontSize = 12, TextWrapping = TextWrapping.Wrap };
        p.Children.Add(_lN1mmStatus);
        AddTab("tab_n1mm", p);
    }

    // Criado a cada uso com o logger global do Serilog (substituído quando o serviço reinicia).
    private static N1MMConfigManager N1mmConfig() =>
        new(Microsoft.Extensions.Logging.LoggerFactoryExtensions.CreateLogger<N1MMConfigManager>(
            new Serilog.Extensions.Logging.SerilogLoggerFactory(Serilog.Log.Logger)));

    private void OnN1mmDetect(object? s, RoutedEventArgs e)
    {
        var n1mmConfig = N1mmConfig();
        _n1mmFolder = n1mmConfig.FindN1MMFolder();
        if (_n1mmFolder is null)
        {
            _lN1mmStatus.Text = L.Get("n1mm_status_not_found");
            return;
        }

        _lN1mmStatus.Text = n1mmConfig.ReadStatus(_n1mmFolder, (int)(_nN1mmPort.Value ?? 12060)) switch
        {
            N1MMBroadcastStatus.ConfiguredCorrectly     => L.Get("n1mm_status_ok"),
            N1MMBroadcastStatus.ConfiguredDifferentPort => L.Get("n1mm_status_wrong_port"),
            N1MMBroadcastStatus.NotConfigured           => L.Get("n1mm_status_not_configured"),
            _                                           => L.Get("n1mm_status_ini_missing")
        };
    }

    private async void OnN1mmAutoConfig(object? s, RoutedEventArgs e)
    {
        var n1mmConfig = N1mmConfig();
        _n1mmFolder ??= n1mmConfig.FindN1MMFolder();
        if (_n1mmFolder is null)
        {
            _lN1mmStatus.Text = L.Get("n1mm_status_not_found");
            return;
        }

        var port = (int)(_nN1mmPort.Value ?? 12060);
        if (n1mmConfig.ReadStatus(_n1mmFolder, port) != N1MMBroadcastStatus.ConfiguredCorrectly
            && n1mmConfig.IsN1MMRunning()
            && !await Dialogs.YesNo(this, L.Get("n1mm_confirm_title"), L.Get("n1mm_confirm_running")))
            return;

        _lN1mmStatus.Text = n1mmConfig.ApplyConfiguration(_n1mmFolder, "127.0.0.1", port) switch
        {
            N1MMApplyResult.AlreadyConfigured => L.Get("n1mm_status_ok"),
            N1MMApplyResult.Applied           => L.Get("n1mm_status_configured"),
            _                                 => L.Get("n1mm_status_config_failed")
        };
    }

    private void BuildWsjtxTab()
    {
        var p = new StackPanel();
        _chkWsjtxEnabled = Check("lbl_wsjtx_enabled");
        _nWsjtxPort      = Number(1024, 65535);
        _txtWsjtxAddr    = new TextBox();
        p.Children.Add(_chkWsjtxEnabled);
        p.Children.Add(Row("lbl_wsjtx_port", _nWsjtxPort));
        p.Children.Add(Row("lbl_n1mm_addr", _txtWsjtxAddr));
        p.Children.Add(Hint("hint_wsjtx"));
        AddTab("tab_wsjtx", p);
    }

    /// <summary>Log4OM: ADIF em texto por UDP (ligação UDP OUTBOUND, mensagem ADIF_MESSAGE).</summary>
    private void BuildLog4omTab()
    {
        var p = new StackPanel();
        _chkLog4omEnabled = Check("lbl_log4om_enabled");
        _nLog4omPort      = Number(1024, 65535);
        p.Children.Add(_chkLog4omEnabled);
        p.Children.Add(Row("lbl_log4om_port", _nLog4omPort));
        p.Children.Add(Hint("hint_log4om"));
        AddTab("tab_log4om", p);
    }

    private void BuildClubLogTab()
    {
        var p = new StackPanel();
        _chkClEnabled = Check("lbl_cl_enabled");
        _txtClEmail   = new TextBox();
        _txtClPass    = new TextBox { PasswordChar = '●' };
        _txtClCall    = Upper();
        _lClError     = new TextBlock { Foreground = Brushes.Crimson, FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap };
        p.Children.Add(Hint("hint_cl"));
        p.Children.Add(_chkClEnabled);
        p.Children.Add(Row("lbl_cl_email", _txtClEmail));
        p.Children.Add(Row("lbl_cl_pass", _txtClPass));
        p.Children.Add(Row("lbl_cl_call", _txtClCall));
        p.Children.Add(_lClError);
        AddTab("tab_clublog", p);
    }

    private void BuildAdvancedTab()
    {
        var p = new StackPanel();
        _cboLogLevel = new ComboBox { ItemsSource = new[] { "Debug", "Information", "Warning", "Error" }, HorizontalAlignment = HorizontalAlignment.Stretch };
        _txtLogPath  = new TextBox();
        p.Children.Add(Row("lbl_log_level", _cboLogLevel));
        p.Children.Add(Row("lbl_log_path", _txtLogPath));

        _chkNotifyEachQso = Check("lbl_notify_each_qso");
        _chkStart         = Check("lbl_start_with_session");
        _chkTelemetry     = Check("lbl_telemetry");
        p.Children.Add(_chkNotifyEachQso);
        p.Children.Add(_chkStart);
        p.Children.Add(_chkTelemetry);

        var what = new Button { Background = Brushes.Transparent, Foreground = Dialogs.Blue, Padding = new Thickness(26, 0, 0, 0) };
        Text("lnk_telemetry_what", t => what.Content = t);
        what.Click += async (_, _) => await Dialogs.Consent(this, askChoice: false);
        p.Children.Add(what);
        AddTab("tab_advanced", p);
    }

    /// <summary>Primeiro arranque: pergunta se pode enviar os dados da instalação e os relatórios de erros.</summary>
    private async Task AskTelemetryConsent()
    {
        var allow = await Dialogs.Consent(this, askChoice: true);

        _settings.Telemetry.Consent        = allow;
        _settings.Telemetry.InstallationId = allow ? Guid.NewGuid().ToString() : string.Empty;
        try { _mgr.Save(_settings); }
        catch (IOException) { /* sem escrita: volta a perguntar no próximo arranque */ }
        _chkTelemetry.IsChecked = allow;
    }

    /// <summary>
    /// Aplica a caixa do separador Avançado. Devolve o identificador da instalação cujo
    /// consentimento foi retirado (para apagar no servidor), ou null.
    /// </summary>
    private string? ApplyTelemetryChoice(bool wanted)
    {
        var t = _settings.Telemetry;
        if (wanted)
        {
            t.Consent = true;
            if (string.IsNullOrWhiteSpace(t.InstallationId)) t.InstallationId = Guid.NewGuid().ToString();
            return null;
        }

        var revoked = t.Consent == true && !string.IsNullOrWhiteSpace(t.InstallationId) ? t.InstallationId : null;
        t.Consent        = false;
        t.InstallationId = string.Empty;
        return revoked;
    }

    // ─── Idioma ────────────────────────────────────────────────────────────
    private void ChangeLanguage(string lang)
    {
        L.Lang           = lang;
        _runner.Language = lang;   // vai nos dados da instalação no próximo arranque do serviço
        _prefs.Language  = lang;
        _prefsMgr.SavePrefs(_prefs);
        UpdateAllText();
        UpdateTray();
        RefreshTrayProfiles();
    }

    private void UpdateAllText()
    {
        foreach (var apply in _texts) apply();
        _lClError.Text = _clubLogRejected ? L.Get("status_clublog_rejected") : "";
        foreach (var (lang, btn) in _langButtons)
            btn.Background = L.Lang == lang ? Dialogs.Blue : LangOff;
    }

    // ─── Formulário ↔ Dados ────────────────────────────────────────────────
    private static decimal Clamp(decimal v, decimal min, decimal max) => v < min ? min : v > max ? max : v;

    private void PopulateForm()
    {
        _txtCallsign.Text = _settings.MyCallsign;

        _txtApiUrl.Text = _settings.Api.BaseUrl;
        _nTimeout.Value = Clamp(_settings.Api.TimeoutSeconds, 5, 300);
        _nRetry.Value   = Clamp(_settings.Api.RetryCount, 0, 10);
        _nDelay.Value   = Clamp(_settings.Api.RetryDelaySeconds, 1, 60);

        _rows.Clear();
        _pnlProfiles.Children.Clear();
        _currentRow = null;
        foreach (var p in _settings.Api.Profiles) AddProfileRow(p);

        _nN1mmPort.Value  = Clamp(_settings.N1mm.UdpPort, 1024, 65535);
        _txtN1mmAddr.Text = string.IsNullOrEmpty(_settings.N1mm.ListenAddress) ? UdpListenAddress.Default : _settings.N1mm.ListenAddress;

        _chkWsjtxEnabled.IsChecked = _settings.Wsjtx.Enabled;
        _nWsjtxPort.Value          = Clamp(_settings.Wsjtx.UdpPort, 1024, 65535);
        _txtWsjtxAddr.Text         = string.IsNullOrEmpty(_settings.Wsjtx.ListenAddress) ? UdpListenAddress.Default : _settings.Wsjtx.ListenAddress;

        _chkLog4omEnabled.IsChecked = _settings.Log4om.Enabled;
        _nLog4omPort.Value          = Clamp(_settings.Log4om.UdpPort, 1024, 65535);

        _chkClEnabled.IsChecked = _settings.ClubLog.Enabled;
        _txtClEmail.Text        = _settings.ClubLog.Email;
        _txtClPass.Text         = _settings.ClubLog.PasswordEncrypted;
        _txtClCall.Text         = _settings.ClubLog.Callsign;

        _cboLogLevel.SelectedItem = _settings.LogLevel;
        if (_cboLogLevel.SelectedIndex < 0) _cboLogLevel.SelectedIndex = 1;
        _txtLogPath.Text = _settings.LogPath;

        _chkNotifyEachQso.IsChecked = _prefs.NotifyEachQso;
        _chkStart.IsChecked         = LinuxStartup.IsEnabled();
        _chkTelemetry.IsChecked     = _settings.Telemetry.IsActive;
    }

    private void CollectForm()
    {
        _settings.MyCallsign = (_txtCallsign.Text ?? "").Trim().ToUpperInvariant();

        var baseUrl = (_txtApiUrl.Text ?? "").Trim().TrimEnd('/');
        _settings.Api.BaseUrl           = baseUrl.Length > 0 ? baseUrl : ApiSettings.DefaultBaseUrl;
        _settings.Api.TimeoutSeconds    = (int)(_nTimeout.Value ?? 30);
        _settings.Api.RetryCount        = (int)(_nRetry.Value ?? 3);
        _settings.Api.RetryDelaySeconds = (int)(_nDelay.Value ?? 2);

        var profiles = new List<ApiProfile>();
        foreach (var row in _rows)
        {
            var rawId = row.Instance.Text?.Trim() ?? string.Empty;
            var profile = new ApiProfile
            {
                Id         = row.Source.Id,
                Enabled    = row.Enabled.IsChecked == true,
                Name       = row.Name.Text?.Trim() ?? string.Empty,
                InstanceId = LogbookCredentials.ExtractInstanceId(rawId) ?? rawId,
                ApiKey     = LogbookCredentials.NormalizeApiKey(row.Key.Text)
            };
            if (!string.IsNullOrEmpty(profile.Name) || !string.IsNullOrEmpty(profile.InstanceId) || !string.IsNullOrEmpty(profile.ApiKey))
                profiles.Add(profile);
        }
        _settings.Api.Profiles = profiles;

        _settings.N1mm.UdpPort       = (int)(_nN1mmPort.Value ?? 12060);
        _settings.N1mm.ListenAddress = (_txtN1mmAddr.Text ?? "").Trim();

        _settings.Wsjtx.Enabled       = _chkWsjtxEnabled.IsChecked == true;
        _settings.Wsjtx.UdpPort       = (int)(_nWsjtxPort.Value ?? 2237);
        _settings.Wsjtx.ListenAddress = (_txtWsjtxAddr.Text ?? "").Trim();

        _settings.Log4om.Enabled = _chkLog4omEnabled.IsChecked == true;
        _settings.Log4om.UdpPort = (int)(_nLog4omPort.Value ?? 2333);

        _settings.ClubLog.Enabled           = _chkClEnabled.IsChecked == true;
        _settings.ClubLog.Email             = (_txtClEmail.Text ?? "").Trim();
        _settings.ClubLog.PasswordEncrypted = _txtClPass.Text ?? "";
        _settings.ClubLog.Callsign          = (_txtClCall.Text ?? "").Trim().ToUpperInvariant();

        _settings.LogLevel = _cboLogLevel.SelectedItem?.ToString() ?? "Information";
        _settings.LogPath  = (_txtLogPath.Text ?? "").Trim();

        _prefs.NotifyEachQso = _chkNotifyEachQso.IsChecked == true;
        _prefsMgr.SavePrefs(_prefs);

        LinuxStartup.Set(_chkStart.IsChecked == true);
    }

    // ─── Eventos ───────────────────────────────────────────────────────────
    private async void OnSave(object? s, RoutedEventArgs e)
    {
        // Uma chave ou Instance ID inválidos fariam a API recusar todos os QSOs (401/404), que
        // seriam descartados: não se grava até estarem certos.
        if (ProfileFormatError() is { } formatError) { SetStatus("err", formatError); return; }

        CollectForm();
        var revoked = ApplyTelemetryChoice(_chkTelemetry.IsChecked == true);
        _btnSave.IsEnabled = false;
        try
        {
            _mgr.Save(_settings);
            SetStatus("", L.Get("busy_restarting"));
            _qsoCount = 0;
            // O serviço novo volta a tentar o ClubLog com as definições gravadas; se forem recusadas, avisa de novo.
            _clubLogRejected = false;
            _lClError.Text   = "";
            await RestartServiceAsync(revoked);
            SetStatus("ok", L.Get("status_saved"));
        }
        catch (Exception ex)
        {
            SetStatus("err", $"{L.Get("status_error")}: {ex.Message}");
        }
        finally { _btnSave.IsEnabled = true; }
        RefreshTrayProfiles();
    }

    private void OnCancel(object? s, RoutedEventArgs e)
    {
        _settings = _mgr.Load();
        PopulateForm();
        RefreshTrayProfiles();
        SetStatus("", "");
    }

    private async void OnTestApi(object? s, RoutedEventArgs e)
    {
        var url = (_txtApiUrl.Text ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(url)) { SetStatus("err", L.Get("status_no_url")); return; }
        if (ProfileFormatError() is { } formatError) { SetStatus("err", formatError); return; }

        var profiles = new List<(string Name, string InstanceId, string ApiKey)>();
        foreach (var row in _rows)
        {
            if (row.Enabled.IsChecked != true) continue;
            var name = row.Name.Text ?? "";
            var iid  = LogbookCredentials.ExtractInstanceId(row.Instance.Text);
            var key  = LogbookCredentials.NormalizeApiKey(row.Key.Text);
            if (iid is not null && key.Length > 0)
                profiles.Add((string.IsNullOrEmpty(name) ? iid : name, iid, key));
        }

        if (profiles.Count == 0) { SetStatus("err", L.Get("status_no_credentials")); return; }

        _btnTestApi.IsEnabled = false;
        SetStatus("", L.Get("status_testing"));
        var results = new List<(bool Ok, string Text)>();
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            foreach (var p in profiles)
            {
                try
                {
                    // /apikey/verify exige a chave (GET /instance/{id} é público e aceitava qualquer chave).
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/instance/{p.InstanceId}/apikey/verify");
                    request.Headers.Add("X-API-Key", p.ApiKey);
                    var r = await http.SendAsync(request);

                    results.Add(r.IsSuccessStatusCode
                        ? (true, $"{p.Name}: {string.Format(L.Get("status_api_ok"), ReadCallsign(await r.Content.ReadAsStringAsync()))}")
                        : r.StatusCode == System.Net.HttpStatusCode.Unauthorized ? (false, $"{p.Name}: {L.Get("status_api_unauthorized")}")
                        : r.StatusCode == System.Net.HttpStatusCode.NotFound     ? (false, $"{p.Name}: {L.Get("status_api_not_found")}")
                        : (false, $"{p.Name}: HTTP {(int)r.StatusCode}"));
                }
                catch (Exception ex)
                {
                    results.Add((false, $"{p.Name}: {ex.Message}"));
                }
            }

            SetStatus(results.TrueForAll(r => r.Ok) ? "ok" : "err", string.Join("   ", results.Select(r => r.Text)));
        }
        finally { _btnTestApi.IsEnabled = true; }
    }

    /// <summary>Indicativo do logbook na resposta de /apikey/verify ({"data":{"valid":true,"callsign":"…"}}).</summary>
    private static string ReadCallsign(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("data").GetProperty("callsign").GetString() ?? "?";
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException) { return "?"; }
    }

    private async void OnExportAdif()
    {
        var qsos = _runner.GetSessionLog();
        if (qsos.Count == 0)
        {
            SetStatus("", L.Get("status_export_empty"));
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = $"logbeam_sessao_{DateTime.Now:yyyyMMdd_HHmmss}.adi",
            DefaultExtension  = "adi",
            FileTypeChoices   = new[] { new FilePickerFileType("ADIF") { Patterns = new[] { "*.adi" } } }
        });
        if (file is null) return;

        try
        {
            var sb = new StringBuilder();
            sb.Append($"LogBeam Uplink {AppVersion.Current} — exportação de sessão\n");
            sb.Append("<ADIF_VER:5>3.1.4<PROGRAMID:8>LogBeam<EOH>\n");
            foreach (var qso in qsos)
                sb.Append(AdifBuilder.Build(qso)).Append('\n');

            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(sb.ToString());
            SetStatus("ok", string.Format(L.Get("status_export_ok"), qsos.Count));
        }
        catch (Exception ex)
        {
            SetStatus("err", $"{L.Get("status_export_failed")}: {ex.Message}");
        }
    }

    private async void OnAbout() =>
        await Dialogs.Show(this, string.Format(L.Get("about_title"), AppVersion.Current), AppVersion.Title,
            string.Format(L.Get("about_body"), AppVersion.Current), 500, 380);

    private async void OnN1mmHelp() =>
        await Dialogs.Show(this, L.Get("n1mm_help_title"), L.Get("n1mm_help_title"), L.Get("n1mm_help_body"), 560, 480);

    private void SetStatus(string type, string msg)
    {
        _lblStatus.Text       = msg;
        _lblStatus.Foreground = type == "ok" ? Green : type == "err" ? Brushes.Crimson : Brushes.DimGray;
        ToolTip.SetTip(_lblStatus, string.IsNullOrEmpty(msg) ? null : msg);
    }
}
