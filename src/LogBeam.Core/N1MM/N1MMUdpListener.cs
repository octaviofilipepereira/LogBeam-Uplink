// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using LogBeam.Core.Models;
using LogBeam.Core.Net;
using LogBeam.Core.Radio;
using Microsoft.Extensions.Logging;

namespace LogBeam.Core.N1MM;

/// <summary>
/// Listens for UDP broadcasts from N1MM+ and parses QSO XML messages.
/// </summary>
public class N1MMUdpListener : IDisposable
{
    private readonly ILogger<N1MMUdpListener> _logger;
    private readonly int _port;
    private readonly IPAddress _listenAddress;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public event EventHandler<QsoRecord>? QsoReceived;
    public bool IsRunning { get; private set; }

    public N1MMUdpListener(ILogger<N1MMUdpListener> logger, int port = 12060, string? listenAddress = null)
    {
        _logger = logger;
        _port = port;
        _listenAddress = UdpListenAddress.Parse(listenAddress);
    }

    public void Start()
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        _udpClient = UdpListenAddress.Open(_listenAddress, _port);
        IsRunning = true;

        _logger.LogInformation("N1MM listener started on {Address}:{Port}.", _listenAddress, _port);

        Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        _udpClient?.Close();
        IsRunning = false;

        _logger.LogInformation("N1MM listener stopped.");
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(ct);
                if (!UdpListenAddress.Accept(_listenAddress, result.RemoteEndPoint)) continue;
                var xml = Encoding.UTF8.GetString(result.Buffer);
                ProcessXml(xml);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving UDP packet.");
            }
        }
    }

    /// <summary>
    /// Trata uma mensagem XML do N1MM+ ("External UDP Broadcasts"). Só "contactinfo" (QSO novo)
    /// é enviado; "contactreplace" (QSO editado) e "contactdelete" (QSO apagado) ficam só no log
    /// — propagar edições e eliminações exige endpoints novos no servidor (previsto para a 2.6).
    /// As restantes mensagens (RadioInfo, AppInfo, spots...) são ignoradas.
    /// </summary>
    internal void ProcessXml(string xml)
    {
        try
        {
            var root = XDocument.Parse(xml).Root;
            if (root == null) return;

            switch (root.Name.LocalName)
            {
                case "contactinfo":
                    break;
                case "contactreplace":
                    _logger.LogInformation("N1MM+: QSO {Call} editado no N1MM+. A alteração não é enviada ao LogBeam nesta versão.",
                        root.Element("call")?.Value);
                    return;
                case "contactdelete":
                    _logger.LogInformation("N1MM+: QSO {Call} apagado no N1MM+. A eliminação não é enviada ao LogBeam nesta versão.",
                        root.Element("call")?.Value);
                    return;
                default:
                    return;
            }

            var freq = ParseFreqMHz(root);
            var (qsoDate, timeOn) = ParseTimestamp(root.Element("timestamp")?.Value);

            var qso = new QsoRecord
            {
                Id          = root.Element("ID")?.Value ?? Guid.NewGuid().ToString(),
                MyCall      = Value(root, "mycall"),
                Call        = Value(root, "call").ToUpperInvariant(),
                Band        = BandPlan.Resolve(root.Element("band")?.Value, freq),
                Freq        = freq,
                Mode        = ModeMap.Normalize(root.Element("mode")?.Value),
                QsoDate     = qsoDate,
                TimeOn      = timeOn,
                RstSent     = Value(root, "snt"),
                RstRcvd     = Value(root, "rcv"),
                GridSquare  = Value(root, "gridsquare"),
                Name        = Value(root, "name"),
                TxPower     = Value(root, "power"),
                Comment     = Value(root, "comment"),
                ContestName = Value(root, "contestname"),
                Operator    = Value(root, "operator"),
                EventType   = N1mmEventType.Add
            };

            if (string.IsNullOrWhiteSpace(qso.Call))
            {
                _logger.LogDebug("Received contactinfo without callsign — ignored.");
                return;
            }

            _logger.LogDebug("QSO received: {Call} on {Band} {Mode} {Freq}MHz",
                qso.Call, qso.Band, qso.Mode, string.IsNullOrEmpty(qso.Freq) ? "(sem freq)" : qso.Freq);

            QsoReceived?.Invoke(this, qso);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse N1MM XML: {Xml}",
                xml[..Math.Min(200, xml.Length)]);
        }
    }

    private static string Value(XElement root, string name) => root.Element(name)?.Value.Trim() ?? string.Empty;

    /// <summary>
    /// Campo "timestamp" do N1MM+ (hora UTC do QSO, "AAAA-MM-DD HH:MM:SS") → QSO_DATE e TIME_ON.
    /// Inválido ou em falta → vazios, e a hora do QSO passa a ser a de chegada do pacote.
    /// </summary>
    internal static (string QsoDate, string TimeOn) ParseTimestamp(string? raw) =>
        DateTime.TryParseExact(raw?.Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var t)
            ? (t.ToString("yyyyMMdd", CultureInfo.InvariantCulture), t.ToString("HHmmss", CultureInfo.InvariantCulture))
            : (string.Empty, string.Empty);

    /// <summary>
    /// Lê txfreq / rxfreq / freq do XML do N1MM+ e converte para MHz (formato ADIF).
    /// N1MM+ envia em Hz (inteiro ou decimal). Ignora elementos vazios.
    /// </summary>
    internal string ParseFreqMHz(XElement? root)
    {
        string? raw = null;
        foreach (var name in new[] { "txfreq", "rxfreq", "freq" })
        {
            var v = root?.Element(name)?.Value?.Trim();
            if (!string.IsNullOrEmpty(v)) { raw = v; break; }
        }

        if (raw == null)
        {
            _logger.LogDebug("N1MM n\u00e3o enviou freq (txfreq/rxfreq/freq vazios)");
            return string.Empty;
        }

        if (!double.TryParse(raw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var hz) || hz <= 0)
        {
            _logger.LogDebug("N1MM freq n\u00e3o parseable: '{Raw}'", raw);
            return string.Empty;
        }

        // N1MM+ envia txfreq/rxfreq em unidades de 10 Hz (não Hz)
        // Exemplo: 14.1542 MHz → N1MM envia 1415420 → 1415420 / 100_000 = 14.1542 MHz
        var mhz = hz >= 1000 ? hz / 100_000.0 : hz;
        var result = mhz.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
        _logger.LogDebug("N1MM freq: raw='{Raw}' -> {MHz} MHz", raw, result);
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _udpClient?.Dispose();
        _cts?.Dispose();
        _disposed = true;
    }
}
