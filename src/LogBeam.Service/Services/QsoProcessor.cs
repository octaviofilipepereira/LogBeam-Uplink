// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.N1MM;
using LogBeam.Core.Wsjtx;
using LogBeam.Service.Models;
using LogBeam.Service.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogBeam.Service.Services;

/// <summary>
/// Orquestra o processamento de QSOs:
/// 1. Recebe eventos do N1MMUdpListener (e, se activo, do WsjtxUdpListener)
/// 2. Envia para cada logbook LogBeam activo (HamQTH/DXCC são resolvidos pelo servidor)
/// 3. Em caso de falha num perfil, coloca esse QSO na fila offline e tenta reenviar periodicamente
/// </summary>
public class QsoProcessor : BackgroundService
{
    private static readonly TimeSpan QueueFlushInterval = TimeSpan.FromSeconds(60);

    private readonly N1MMUdpListener _listener;
    private readonly WsjtxUdpListener _wsjtxListener;
    private readonly ApiClientService _apiClient;
    private readonly QsoQueueService _queue;
    private readonly SessionLogService _sessionLog;
    private readonly IOptions<AppSettings> _settings;
    private readonly ILogger<QsoProcessor> _logger;

    /// <summary>Disparado após cada QSO ser processado (sucesso se pelo menos um perfil aceitar), incluindo reenvios da fila.</summary>
    public event Action<Core.Models.QsoRecord, bool>? QsoResult;

    /// <summary>Disparado quando um QSO é confirmado por outro logbook LogBeam (correspondência automática).</summary>
    public event Action<Core.Models.QsoRecord>? QsoConfirmedByLogbeam;

    public QsoProcessor(
        N1MMUdpListener listener,
        WsjtxUdpListener wsjtxListener,
        ApiClientService apiClient,
        QsoQueueService queue,
        SessionLogService sessionLog,
        IOptions<AppSettings> settings,
        ILogger<QsoProcessor> logger)
    {
        _listener       = listener;
        _wsjtxListener  = wsjtxListener;
        _apiClient      = apiClient;
        _queue          = queue;
        _sessionLog     = sessionLog;
        _settings       = settings;
        _logger         = logger;
    }

    /// <summary>
    /// Método que roda quando o serviço é iniciado
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QsoProcessor iniciado");

        try
        {
            // Cada listener é arrancado isoladamente: se a porta UDP já estiver
            // ocupada por outra aplicação (ex. GridTracker na 2237, um segundo
            // N1MM na 12060), o Start() lança SocketException — sem este
            // try/catch por listener, essa excepção subia até ao catch-all
            // abaixo e matava o QsoProcessor inteiro, desligando também o
            // listener que estava a funcionar bem.
            _listener.QsoReceived += (_, qso) => _ = ProcessQsoAsync(qso, stoppingToken);
            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível iniciar o listener N1MM (porta ocupada?). Este listener fica desactivado.");
            }

            if (_settings.Value.Wsjtx.Enabled)
            {
                _wsjtxListener.QsoReceived += (_, qso) => _ = ProcessQsoAsync(qso, stoppingToken);
                try
                {
                    _wsjtxListener.Start();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Não foi possível iniciar o listener WSJT-X (porta ocupada?). Este listener fica desactivado.");
                }
            }

            // Reenviar QSOs pendentes da fila offline periodicamente
            using var timer = new PeriodicTimer(QueueFlushInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await FlushQueueAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Shutdown normal — não é erro
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro fatal no QsoProcessor");
            throw;
        }
    }

    /// <summary>
    /// Processa um QSO: envia para todos os perfis (logbooks) activos; em falha por perfil, guarda na fila offline.
    /// </summary>
    private async Task ProcessQsoAsync(Core.Models.QsoRecord qso, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Processando QSO: {Callsign} | {Band} | {Mode} | freq={Freq}",
                qso.Call, qso.Band, qso.Mode, string.IsNullOrEmpty(qso.Freq) ? "(vazio)" : qso.Freq + " MHz");

            var profiles = _settings.Value.Api.Profiles.Where(p => p.Enabled).ToList();
            if (profiles.Count == 0)
            {
                _logger.LogWarning("Nenhum logbook LogBeam configurado/activo — QSO {Callsign} não enviado.", qso.Call);
                QsoResult?.Invoke(qso, false);
                return;
            }

            var anySuccess = false;
            var anyConfirmed = false;

            foreach (var profile in profiles)
            {
                var result = await _apiClient.SendQsoAsync(qso, profile, ct);
                if (result.Success)
                {
                    anySuccess = true;
                    if (result.ConfirmedLogbeam) anyConfirmed = true;
                }
                else if (result.IsPermanentFailure)
                {
                    // Erro permanente (4xx): reenviar nunca vai resolver — descartar em
                    // vez de enfileirar, para não bloquear para sempre os QSOs seguintes
                    // deste perfil (ver achado 2.1 do plano de trabalhos Windows).
                    _logger.LogError("Falha permanente ao enviar QSO para API: {Callsign} (perfil {Profile}). Descartado (não vai para a fila offline).", qso.Call, profile.Name);
                }
                else
                {
                    _logger.LogError("Falha ao enviar QSO para API: {Callsign} (perfil {Profile}). Colocado na fila offline.", qso.Call, profile.Name);
                    _queue.Enqueue(qso, profile.Id);
                }
            }

            if (anySuccess)
                _sessionLog.Add(qso);

            QsoResult?.Invoke(qso, anySuccess);
            if (anyConfirmed)
                QsoConfirmedByLogbeam?.Invoke(qso);
        }
        catch (OperationCanceledException)
        {
            // Esperado quando a aplicação está a desligar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar QSO");
        }
    }

    /// <summary>
    /// Tenta reenviar os QSOs pendentes na fila offline, agrupados por perfil e pela ordem de chegada.
    /// Pára, por perfil, ao primeiro que continue a falhar — esse logbook provavelmente ainda está inacessível.
    /// </summary>
    private async Task FlushQueueAsync(CancellationToken ct)
    {
        var pending = _queue.Snapshot();
        if (pending.Count == 0) return;

        _logger.LogInformation("A tentar reenviar {Count} QSOs pendentes da fila offline.", pending.Count);

        var profilesById = _settings.Value.Api.Profiles.ToDictionary(p => p.Id);

        foreach (var group in pending.GroupBy(item => item.ProfileId))
        {
            if (!profilesById.TryGetValue(group.Key, out var profile))
            {
                // Perfil já não existe (foi removido) — descarta os QSOs pendentes para ele
                foreach (var item in group) _queue.Remove(item);
                continue;
            }

            foreach (var item in group)
            {
                var result = await _apiClient.SendQsoAsync(item.Qso, profile, ct);
                if (!result.Success)
                {
                    if (result.IsPermanentFailure)
                    {
                        // Erro permanente: reenviar não vai resolver — descartar este
                        // item e continuar para o próximo da fila (não bloquear os
                        // restantes QSOs deste perfil por causa de um só inválido).
                        _logger.LogError("Reenvio da fila offline com falha permanente em {Callsign} (perfil {Profile}). Descartado.", item.Qso.Call, profile.Name);
                        _queue.Remove(item);
                        continue;
                    }

                    _logger.LogWarning("Reenvio da fila offline ainda a falhar em {Callsign} (perfil {Profile}). Nova tentativa no próximo ciclo.", item.Qso.Call, profile.Name);
                    break;
                }

                _queue.Remove(item);
                _sessionLog.Add(item.Qso);
                _logger.LogInformation("QSO reenviado com sucesso a partir da fila offline: {Callsign} (perfil {Profile})", item.Qso.Call, profile.Name);
                QsoResult?.Invoke(item.Qso, true);
                if (result.ConfirmedLogbeam)
                    QsoConfirmedByLogbeam?.Invoke(item.Qso);
            }
        }
    }

    /// <summary>
    /// Método chamado quando o serviço está a desligar
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("QsoProcessor a desligar");
        _listener.Stop();
        _wsjtxListener.Stop();
        await base.StopAsync(cancellationToken);
    }
}
