// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Updates;

namespace LogBeam.Tests.Updates;

/// <summary>Quando verificar e quando avisar: um minuto depois de arrancar, 15 minutos se falhar, uma vez por dia.</summary>
public class UpdateScheduleTests
{
    private static readonly DateTime Start = new(2026, 11, 8, 7, 0, 0, DateTimeKind.Utc);
    private static readonly UpdateCheckResult Ok     = new(false, "2.6.1", UpdateChecker.DefaultDownloadPage, null);
    private static readonly UpdateCheckResult Failed = new(false, null, null, "sem rede");

    [Fact]
    public void FirstCheckOneMinuteAfterStart()
    {
        var s = new UpdateSchedule(Start);
        Assert.False(s.IsDue(Start));
        Assert.False(s.IsDue(Start.AddSeconds(59)));
        Assert.True(s.IsDue(Start.AddMinutes(1)));
    }

    [Fact]
    public void AfterAFailureTriesAgainIn15Minutes()
    {
        var s = new UpdateSchedule(Start);
        var t = Start.AddMinutes(1);
        s.Record(Failed, t);
        Assert.False(s.IsDue(t.AddMinutes(14)));
        Assert.True(s.IsDue(t.AddMinutes(15)));
    }

    [Fact]
    public void AfterASuccessChecksAgainNextDay()
    {
        var s = new UpdateSchedule(Start);
        var t = Start.AddMinutes(1);
        s.Record(Ok, t);
        Assert.False(s.IsDue(t.AddHours(23)));
        Assert.True(s.IsDue(t.AddDays(1)));
    }

    [Fact]
    public void AfterSuspendTheCheckIsDueAtOnce()
    {
        // PC suspenso três dias: ao acordar, a hora da verificação já passou.
        var s = new UpdateSchedule(Start);
        s.Record(Ok, Start.AddMinutes(1));
        Assert.True(s.IsDue(Start.AddDays(3)));
    }

    [Fact]
    public void ClockMovedBackDoesNotDelayMoreThanADay()
    {
        var s = new UpdateSchedule(Start);
        s.Record(Ok, Start.AddMinutes(1));            // próxima: Start + 1 dia + 1 min
        var back = Start.AddDays(-30);                // relógio acertado um mês para trás
        Assert.False(s.IsDue(back));
        Assert.True(s.IsDue(back.AddDays(1)));
    }

    [Fact]
    public void NoticeAtMostOncePerDayForTheSameVersion()
    {
        var notice = Start;
        Assert.True(UpdateNotice.ShouldNotify("2.6.1", null, null, notice));
        Assert.False(UpdateNotice.ShouldNotify("2.6.1", "2.6.1", notice, notice.AddHours(5)));
        Assert.False(UpdateNotice.ShouldNotify("2.6.1", "2.6.1", notice, notice.AddHours(23).AddMinutes(59)));
        Assert.True(UpdateNotice.ShouldNotify("2.6.1", "2.6.1", notice, notice.AddDays(1)));
    }

    [Fact]
    public void ANewerVersionIsAnnouncedAtOnce()
    {
        Assert.True(UpdateNotice.ShouldNotify("2.6.2", "2.6.1", Start, Start.AddMinutes(10)));
    }

    [Fact]
    public void NoticeInTheFutureDoesNotSilenceTheNextOnes()
    {
        Assert.True(UpdateNotice.ShouldNotify("2.6.1", "2.6.1", Start.AddDays(30), Start));
    }
}
