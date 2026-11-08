// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Updates;

/// <summary>
/// Verificação automática em segundo plano, enquanto o Uplink estiver aberto: de minuto a minuto vê se
/// é altura de verificar (<see cref="UpdateSchedule"/>) e dispara <see cref="UpdateAvailable"/> quando
/// há versão nova. Como e quando avisar o operador fica com a interface (<see cref="UpdateNotice"/>).
/// </summary>
public sealed class UpdateWatcher : IDisposable
{
    private readonly Func<CancellationToken, Task<UpdateCheckResult>> _check;
    private readonly Func<DateTime> _clock;
    private readonly TimeSpan _tick;
    private readonly UpdateSchedule _schedule;
    private readonly CancellationTokenSource _cts = new();
    private Task? _loop;

    /// <summary>Há versão nova. Corre fora da thread da interface.</summary>
    public event Action<UpdateCheckResult>? UpdateAvailable;

    /// <summary>Cada verificação feita, com ou sem versão nova (para o log).</summary>
    public event Action<UpdateCheckResult>? Checked;

    public UpdateWatcher(Func<CancellationToken, Task<UpdateCheckResult>> check,
                         Func<DateTime>? clockUtc = null, TimeSpan? tick = null)
    {
        _check    = check;
        _clock    = clockUtc ?? (() => DateTime.UtcNow);
        _tick     = tick ?? TimeSpan.FromMinutes(1);
        _schedule = new UpdateSchedule(_clock());
    }

    public DateTime NextCheckUtc => _schedule.NextCheckUtc;

    public void Start() => _loop ??= Task.Run(() => RunAsync(_cts.Token));

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (_schedule.IsDue(_clock()))
                {
                    var result = await _check(ct);
                    _schedule.Record(result, _clock());
                    Raise(Checked, result);
                    if (result.HasUpdate) Raise(UpdateAvailable, result);
                }
                await Task.Delay(NextWait(_schedule.NextCheckUtc - _clock(), _tick), ct);
            }
        }
        catch (OperationCanceledException)
        {
            // a aplicação está a terminar
        }
    }

    /// <summary>
    /// Espera até à hora da próxima verificação, mas nunca mais do que um passo (para o relógio do sistema
    /// voltar a ser lido depois de o PC estar suspenso) nem menos do que um instante (sem ciclo apertado).
    /// </summary>
    internal static TimeSpan NextWait(TimeSpan untilDue, TimeSpan tick)
    {
        var min = TimeSpan.FromMilliseconds(Math.Min(tick.TotalMilliseconds, 200));
        if (untilDue < min) return min;
        return untilDue < tick ? untilDue + TimeSpan.FromMilliseconds(10) : tick;
    }

    /// <summary>Um erro no aviso não pode parar as verificações seguintes.</summary>
    private static void Raise(Action<UpdateCheckResult>? handler, UpdateCheckResult result)
    {
        try { handler?.Invoke(result); }
        catch { /* o aviso falhou; a próxima verificação tenta de novo */ }
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }
}
