// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Net.Sockets;
using LogBeam.Core.Adif;
using LogBeam.Core.Models;
using LogBeam.Core.Net;
using Microsoft.Extensions.Logging;

namespace LogBeam.Core.Wsjtx;

/// <summary>
/// Escuta os broadcasts UDP do WSJT-X (e compatíveis: JTDX, GridTracker, etc).
/// Usa apenas a mensagem "Logged ADIF" (tipo 12) do protocolo — o WSJT-X envia-a
/// automaticamente sempre que regista um QSO, já com o registo ADIF completo e
/// normalizado, evitando ter de decifrar manualmente os restantes tipos de mensagem
/// binária (QDateTime do Qt, etc).
/// </summary>
public class WsjtxUdpListener : IDisposable
{
    private const uint MagicNumber = 0xadbccbda;
    private const uint LoggedAdifMessageType = 12;

    private readonly ILogger<WsjtxUdpListener> _logger;
    private readonly int _port;
    private readonly IPAddress _listenAddress;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public event EventHandler<QsoRecord>? QsoReceived;
    public bool IsRunning { get; private set; }

    public WsjtxUdpListener(ILogger<WsjtxUdpListener> logger, int port = 2237, string? listenAddress = null)
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

        _logger.LogInformation("WSJT-X listener started on {Address}:{Port}.", _listenAddress, _port);

        Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        _udpClient?.Close();
        IsRunning = false;

        _logger.LogInformation("WSJT-X listener stopped.");
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(ct);
                ProcessDatagram(result.Buffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving WSJT-X UDP packet.");
            }
        }
    }

    internal void ProcessDatagram(byte[] buffer)
    {
        try
        {
            var reader = new WsjtxBinaryReader(buffer);
            if (reader.ReadUInt32() != MagicNumber) return; // não é um pacote WSJT-X

            reader.ReadUInt32();          // schema version — não usado
            var type = reader.ReadUInt32();
            reader.ReadUtf8String();      // id do remetente — não usado

            if (type != LoggedAdifMessageType) return; // só nos interessa "Logged ADIF"

            var adif = reader.ReadUtf8String();
            if (string.IsNullOrWhiteSpace(adif)) return;

            // A mensagem traz um cabeçalho ADIF (até <EOH>) e um registo.
            var qso = AdifRecordParser.SplitRecords(adif).Select(AdifRecordParser.Parse).FirstOrDefault(q => q != null);
            if (qso == null)
            {
                _logger.LogDebug("WSJT-X Logged ADIF sem callsign — ignorado.");
                return;
            }

            _logger.LogDebug("WSJT-X QSO received: {Call} {Band} {Mode}", qso.Call, qso.Band, qso.Mode);
            QsoReceived?.Invoke(this, qso);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao processar datagrama WSJT-X.");
        }
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

/// <summary>Leitor big-endian mínimo para o formato binário QDataStream usado pelo WSJT-X.</summary>
internal class WsjtxBinaryReader
{
    private readonly byte[] _buf;
    private int _pos;

    public WsjtxBinaryReader(byte[] buf) => _buf = buf;

    public uint ReadUInt32()
    {
        var v = (uint)((_buf[_pos] << 24) | (_buf[_pos + 1] << 16) | (_buf[_pos + 2] << 8) | _buf[_pos + 3]);
        _pos += 4;
        return v;
    }

    /// <summary>String Qt "utf8": quint32 comprimento em bytes (0xFFFFFFFF = nula) + bytes UTF-8.</summary>
    public string ReadUtf8String()
    {
        var len = ReadUInt32();
        if (len == 0xFFFFFFFF) return string.Empty;
        var s = System.Text.Encoding.UTF8.GetString(_buf, _pos, (int)len);
        _pos += (int)len;
        return s;
    }
}
