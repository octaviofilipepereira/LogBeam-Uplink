// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Updates;

namespace LogBeam.Tests.Updates;

/// <summary>Verificação em segundo plano, com um relógio falso que o teste faz andar.</summary>
public class UpdateWatcherTests
{
    private sealed class Clock
    {
        private readonly object _lock = new();
        private DateTime _now = new(2026, 11, 8, 7, 0, 0, DateTimeKind.Utc);
        public DateTime Now { get { lock (_lock) return _now; } }
        public void Advance(TimeSpan by) { lock (_lock) _now += by; }
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 300 && !condition(); i++) await Task.Delay(10);
        Assert.True(condition());
    }

    [Fact]
    public async Task ChecksOnlyWhenDueAndAnnouncesTheNewVersion()
    {
        var clock = new Clock();
        var calls = 0;
        var found = new List<string>();
        using var w = new UpdateWatcher(_ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(new UpdateCheckResult(true, "2.6.1", UpdateChecker.DefaultDownloadPage, null));
        }, () => clock.Now, TimeSpan.FromMilliseconds(5));
        w.UpdateAvailable += r => { lock (found) found.Add(r.LatestVersion!); };
        w.Start();

        await Task.Delay(100);
        Assert.Equal(0, Volatile.Read(ref calls));           // ainda dentro do primeiro minuto

        clock.Advance(TimeSpan.FromMinutes(1));
        await WaitUntil(() => { lock (found) return found.Count == 1; });
        Assert.Equal(1, Volatile.Read(ref calls));

        clock.Advance(TimeSpan.FromHours(23));
        await Task.Delay(100);
        Assert.Equal(1, Volatile.Read(ref calls));           // só uma verificação por dia

        clock.Advance(TimeSpan.FromHours(1));
        await WaitUntil(() => Volatile.Read(ref calls) == 2);
    }

    [Fact]
    public async Task AFailedCheckIsRetriedAndAFailingNoticeDoesNotStopTheWatcher()
    {
        var clock = new Clock();
        var calls = 0;
        using var w = new UpdateWatcher(_ =>
        {
            var n = Interlocked.Increment(ref calls);
            return Task.FromResult(n == 1
                ? new UpdateCheckResult(false, null, null, "sem rede")
                : new UpdateCheckResult(true, "2.6.1", UpdateChecker.DefaultDownloadPage, null));
        }, () => clock.Now, TimeSpan.FromMilliseconds(5));
        w.UpdateAvailable += _ => throw new InvalidOperationException("aviso falhou");
        w.Start();

        clock.Advance(TimeSpan.FromMinutes(1));
        await WaitUntil(() => Volatile.Read(ref calls) == 1);

        clock.Advance(TimeSpan.FromMinutes(15));           // nova tentativa depois da falha
        await WaitUntil(() => Volatile.Read(ref calls) == 2);

        clock.Advance(TimeSpan.FromDays(1));                // o erro no aviso não parou o ciclo
        await WaitUntil(() => Volatile.Read(ref calls) == 3);
    }

    [Fact]
    public void WaitsUntilTheCheckIsDueNotAFullStepMore()
    {
        var tick = TimeSpan.FromMinutes(1);
        // A 0,3 s da hora: acorda logo a seguir à hora, não um minuto depois.
        var w = UpdateWatcher.NextWait(TimeSpan.FromMilliseconds(300), tick);
        Assert.InRange(w.TotalMilliseconds, 300, 400);
        // Hora já passada ou muito perto: um instante, sem ciclo apertado.
        Assert.Equal(TimeSpan.FromMilliseconds(200), UpdateWatcher.NextWait(TimeSpan.FromSeconds(-5), tick));
        // Longe da hora: um passo, para voltar a ler o relógio (PC suspenso).
        Assert.Equal(tick, UpdateWatcher.NextWait(TimeSpan.FromHours(20), tick));
    }

    [Fact]
    public async Task StopsWhenDisposed()
    {
        var clock = new Clock();
        var calls = 0;
        var w = new UpdateWatcher(_ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(new UpdateCheckResult(false, "2.6.0", UpdateChecker.DefaultDownloadPage, null));
        }, () => clock.Now, TimeSpan.FromMilliseconds(5));
        w.Start();
        w.Dispose();
        w.Dispose();                                        // duas vezes não faz mal

        clock.Advance(TimeSpan.FromDays(2));
        await Task.Delay(100);
        Assert.Equal(0, Volatile.Read(ref calls));
    }
}
