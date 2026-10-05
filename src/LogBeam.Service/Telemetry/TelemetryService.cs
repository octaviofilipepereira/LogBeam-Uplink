// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Globalization;
using System.Runtime.InteropServices;
using LogBeam.Service.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogBeam.Service.Telemetry;

/// <summary>Versão da aplicação e língua da interface (vêm da UI, que o serviço não conhece).</summary>
public record TelemetryContext(string AppVersion, string Language);

/// <summary>
/// Com consentimento do operador: envia os dados da instalação no arranque (só quando mudaram
/// desde o último envio) e, de minuto a minuto, os relatórios de erros pendentes.
/// Sem consentimento não faz nada.
/// </summary>
public class TelemetryService : BackgroundService
{
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(60);

    private readonly AppSettings _settings;
    private readonly TelemetryClient _client;
    private readonly TelemetryStore _store;
    private readonly TelemetryContext _context;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(IOptions<AppSettings> options, TelemetryClient client, TelemetryStore store,
                            TelemetryContext context, ILogger<TelemetryService> logger)
    {
        _settings = options.Value;
        _client   = client;
        _store    = store;
        _context  = context;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Um apagamento pendente (consentimento retirado) é tratado mesmo sem consentimento activo.
        if (!_settings.Telemetry.IsActive && string.IsNullOrEmpty(_store.PendingDeletionId)) return;

        try
        {
            await RunCycleAsync(stoppingToken);

            using var timer = new PeriodicTimer(FlushInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCycleAsync(stoppingToken);
                if (!_settings.Telemetry.IsActive && string.IsNullOrEmpty(_store.PendingDeletionId)) return;
            }
        }
        catch (OperationCanceledException) { }
    }

    internal async Task RunCycleAsync(CancellationToken ct)
    {
        await DeletePendingAsync(ct);
        if (!_settings.Telemetry.IsActive) return;

        await SendInstallIfChangedAsync(ct);
        await FlushErrorsAsync(ct);
    }

    private async Task DeletePendingAsync(CancellationToken ct)
    {
        var id = _store.PendingDeletionId;
        if (string.IsNullOrEmpty(id)) return;

        if (await _client.DeleteInstallAsync(id, ct) != TelemetrySendResult.RetryLater)
            _store.PendingDeletionId = string.Empty;
    }

    /// <summary>Dados da instalação, como o contrato os define (só o necessário).</summary>
    public static InstallInfo BuildInstallInfo(AppSettings settings, TelemetryContext context)
    {
        var programs = new List<string> { "N1MM" };   // o receptor do N1MM+ está sempre activo
        if (settings.Wsjtx.Enabled)  programs.Add("WSJTX");
        if (settings.Log4om.Enabled) programs.Add("LOG4OM");

        return new InstallInfo(
            settings.Telemetry.InstallationId,
            Callsign(settings),
            context.AppVersion,
            Environment.Version.ToString(),
            RuntimeInformation.OSDescription.Trim(),
            RuntimeInformation.OSArchitecture.ToString(),
            context.Language,
            programs,
            new InstallDestinations(
                settings.Api.Profiles.Any(p => p.Enabled && !string.IsNullOrWhiteSpace(p.InstanceId)
                                                         && !string.IsNullOrWhiteSpace(p.ApiKey)),
                settings.ClubLog.Enabled));
    }

    /// <summary>Indicativo do operador: o da estação ou, na falta dele, o do ClubLog; null se nenhum.</summary>
    public static string? Callsign(AppSettings settings)
    {
        var call = !string.IsNullOrWhiteSpace(settings.MyCallsign) ? settings.MyCallsign : settings.ClubLog.Callsign;
        return string.IsNullOrWhiteSpace(call) ? null : call.Trim().ToUpperInvariant();
    }

    private async Task SendInstallIfChangedAsync(CancellationToken ct)
    {
        if (_installRejected) return;

        var info        = BuildInstallInfo(_settings, _context);
        var fingerprint = info.Fingerprint();
        if (fingerprint == _store.LastInstallFingerprint) return;

        switch (await _client.SendInstallAsync(info, ct))
        {
            case TelemetrySendResult.Ok:
                _store.LastInstallFingerprint = fingerprint;
                break;
            case TelemetrySendResult.Rejected:
                // Reenviar o mesmo de minuto a minuto não adianta: tenta-se no próximo arranque.
                _installRejected = true;
                _logger.LogInformation("Dados da instalação recusados pelo servidor; nova tentativa no próximo arranque.");
                break;
            default:
                _logger.LogInformation("Dados da instalação não enviados (sem ligação); nova tentativa dentro de um minuto.");
                break;
        }
    }

    private bool _installRejected;

    private async Task FlushErrorsAsync(CancellationToken ct)
    {
        foreach (var pending in _store.NextBatch())
        {
            var report = new ErrorReport
            {
                InstallationId = _settings.Telemetry.InstallationId,
                Callsign       = Callsign(_settings),
                AppVersion     = _context.AppVersion,
                OccurredAt     = pending.FirstAtUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Component      = pending.Component,
                ErrorType      = pending.ErrorType,
                Message        = pending.Message,
                StackTrace     = pending.StackTrace,
                Count          = pending.Count
            };

            var result = await _client.SendErrorAsync(report, ct);
            if (result == TelemetrySendResult.RetryLater) return;   // sem rede ou 429: fica tudo para o próximo ciclo

            // Ok ou recusado (dados inválidos): reenviar não adianta.
            _store.MarkSent(pending);
        }
    }
}
