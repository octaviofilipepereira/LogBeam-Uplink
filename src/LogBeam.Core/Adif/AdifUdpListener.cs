// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;
using System.Net.Sockets;
using System.Text;
using LogBeam.Core.Models;
using LogBeam.Core.Net;
using Microsoft.Extensions.Logging;

namespace LogBeam.Core.Adif;

/// <summary>
/// Escuta QSOs enviados por UDP como ADIF em texto simples — o formato da ligação
/// "UDP OUTBOUND / ADIF_MESSAGE" do Log4OM (porta habitual 2333).
/// O Log4OM envia um QSO por datagrama e só quando o QSO é criado: edições e eliminações
/// não geram mensagem. Com "Broadcast" activo no Log4OM, os pacotes não chegam a um
/// receptor em 127.0.0.1 — o operador tem de o desligar e indicar 127.0.0.1 como destino.
/// </summary>
public class AdifUdpListener : IDisposable
{
    private readonly ILogger<AdifUdpListener> _logger;
    private readonly int _port;
    private readonly IPAddress _listenAddress;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public event EventHandler<QsoRecord>? QsoReceived;
    public bool IsRunning { get; private set; }

    public AdifUdpListener(ILogger<AdifUdpListener> logger, int port = 2333, string? listenAddress = null)
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

        _logger.LogInformation("Receptor ADIF (Log4OM) activo em {Address}:{Port}.", _listenAddress, _port);

        Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        _udpClient?.Close();
        IsRunning = false;

        _logger.LogInformation("Receptor ADIF (Log4OM) parado.");
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(ct);
                if (!UdpListenAddress.Accept(_listenAddress, result.RemoteEndPoint)) continue;
                ProcessDatagram(result.Buffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao receber pacote UDP no receptor ADIF.");
            }
        }
    }

    internal void ProcessDatagram(byte[] buffer)
    {
        try
        {
            var text = Encoding.UTF8.GetString(buffer);

            foreach (var record in AdifRecordParser.SplitRecords(text))
            {
                var qso = AdifRecordParser.Parse(record);
                if (qso == null)
                {
                    _logger.LogDebug("Registo ADIF sem CALL — ignorado.");
                    continue;
                }

                _logger.LogDebug("QSO recebido por ADIF: {Call} {Band} {Mode}", qso.Call, qso.Band, qso.Mode);
                QsoReceived?.Invoke(this, qso);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao processar datagrama ADIF.");
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
