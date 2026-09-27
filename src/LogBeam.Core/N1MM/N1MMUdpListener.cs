// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using LogBeam.Core.Models;
using LogBeam.Core.Net;
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
        _udpClient = new UdpClient(new IPEndPoint(_listenAddress, _port));
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

    private void ProcessXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root;

            if (root?.Name.LocalName != "contactinfo") return;

            var eventTypeStr = root.Element("NetBiosName")?.Value ?? string.Empty;
            var qso = new QsoRecord
            {
                Id          = root.Element("ID")?.Value ?? Guid.NewGuid().ToString(),
                MyCall      = root.Element("mycall")?.Value ?? string.Empty,
                Call        = root.Element("call")?.Value ?? string.Empty,
                Band        = NormalizeBand(root.Element("band")?.Value),
                Freq        = ParseFreqMHz(root),
                Mode        = NormalizeMode(root.Element("mode")?.Value),
                QsoDate     = FormatDate(root.Element("qso_date")?.Value),
                TimeOn      = FormatTime(root.Element("time_on")?.Value),
                RstSent     = root.Element("rst_sent")?.Value ?? string.Empty,
                RstRcvd     = root.Element("rst_rcvd")?.Value ?? string.Empty,
                GridSquare  = root.Element("gridsquare")?.Value ?? string.Empty,
                Name        = root.Element("name")?.Value ?? string.Empty,
                TxPower     = root.Element("txpower")?.Value ?? string.Empty,
                Comment     = root.Element("comment")?.Value ?? string.Empty,
                ContestName = root.Element("contestname")?.Value ?? string.Empty,
                Operator    = root.Element("operator")?.Value ?? string.Empty,
                EventType   = ParseEventType(root.Element("IsRunQSO")?.Value)
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

    /// <summary>
    /// Normaliza o modo do N1MM para formato ADIF.
    /// USB/LSB → SSB, restantes mantêm-se.
    /// </summary>
    internal static string NormalizeMode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        return raw.Trim().ToUpperInvariant() switch
        {
            "USB" or "LSB" => "SSB",
            "AM"           => "AM",
            "FM"           => "FM",
            "CW"           => "CW",
            "RTTY"         => "RTTY",
            "FT8"          => "FT8",
            "FT4"          => "FT4",
            "PSK31" or "PSK63" or "PSK" => "PSK",
            "JS8"          => "JS8",
            "WSPR"         => "WSPR",
            "JT65"         => "JT65",
            "JT9"          => "JT9",
            var m          => m
        };
    }

    /// <summary>
    /// Normaliza o campo band do N1MM para o formato ADIF (ex: "14" → "20m", "20" → "20m").
    /// N1MM pode enviar metros ("20"), MHz ("14") ou já no formato correcto ("20m").
    /// </summary>
    internal static string NormalizeBand(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var b = raw.Trim().ToLowerInvariant();

        // Já está no formato correcto (ex: "20m", "40m")
        if (b.EndsWith("m") || b.EndsWith("cm")) return b;

        // Mapeamento de MHz → banda ADIF
        return b switch
        {
            "1.8" or "1.9"               => "160m",
            "3.5" or "3.6" or "3.7" or "3.8" => "80m",
            "5"   or "5.3"               => "60m",
            "7"                          => "40m",
            "10"  or "10.1"              => "30m",
            "14"                         => "20m",
            "18"  or "18.1"              => "17m",
            "21"                         => "15m",
            "24"  or "24.9"              => "12m",
            "28"  or "29"                => "10m",
            "50"  or "51" or "52" or "53" or "54" => "6m",
            "144" or "145" or "146"      => "2m",
            // Mapeamento de metros sem sufixo "m" (formato N1MM padrão)
            "160" or "80" or "60" or "40" or "30" or "20" or
            "17"  or "15" or "12" or "10" or "6"  or "4"  or "2" => b + "m",
            _ => raw.Trim()
        };
    }

    private static string FormatDate(string? raw) =>
        string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Replace("-", "").Replace("/", "");

    private static string FormatTime(string? raw) =>
        string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Replace(":", "");

    private static N1mmEventType ParseEventType(string? val) =>
        val switch
        {
            "1" => N1mmEventType.Update,
            _   => N1mmEventType.Add
        };

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _udpClient?.Dispose();
        _cts?.Dispose();
        _disposed = true;
    }
}
