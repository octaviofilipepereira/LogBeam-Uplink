// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Service.Telemetry;

namespace LogBeam.Tests.Telemetry;

public class TelemetryStoreTests
{
    private DateTime _now = new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

    private TelemetryStore NewStore(string? path = null) => new(path, () => _now);

    [Fact]
    public void SameErrorIsGroupedWithCount()
    {
        var store = NewStore();
        store.Add("N1MM", "System.IO.IOException", "Falha", "");
        store.Add("N1MM", "System.IO.IOException", "Falha", "");
        store.Add("N1MM", "System.IO.IOException", "Outra falha", "");

        var batch = store.NextBatch();
        Assert.Equal(2, batch.Count);
        Assert.Equal(2, batch.Single(p => p.Message == "Falha").Count);
    }

    [Fact]
    public void SameErrorIsSentAtMostOncePerHour()
    {
        var store = NewStore();
        store.Add("UI", "X", "Falha", "");
        store.MarkSent(store.NextBatch().Single());

        _now = _now.AddMinutes(10);
        store.Add("UI", "X", "Falha", "");
        Assert.Empty(store.NextBatch());

        _now = _now.AddMinutes(50);
        var again = Assert.Single(store.NextBatch());
        Assert.Equal(1, again.Count);
    }

    [Fact]
    public void AtMostTwentyReportsPerHourInTotal()
    {
        var store = NewStore();
        for (var i = 0; i < 25; i++) store.Add("Service", "X", $"Falha {i}", "");

        var first = store.NextBatch();
        Assert.Equal(TelemetryStore.MaxReportsPerHour, first.Count);
        foreach (var p in first) store.MarkSent(p);
        Assert.Empty(store.NextBatch());

        _now = _now.AddHours(1);
        Assert.Equal(5, store.NextBatch().Count);
    }

    [Fact]
    public void PendingListIsLimitedAndDropsTheOldest()
    {
        var store = NewStore();
        for (var i = 0; i < TelemetryStore.MaxPending + 5; i++) store.Add("Service", "X", $"Falha {i}", "");

        Assert.Equal(TelemetryStore.MaxPending, store.PendingCount);
        _now = _now.AddHours(1);
        Assert.Equal("Falha 5", store.NextBatch()[0].Message);
    }

    [Fact]
    public void MessageAndStackTraceAreScrubbedAndTruncated()
    {
        var store = NewStore();
        store.Add("Service", "X", @"C:\Users\Joao\a.json " + new string('m', 2000), new string('s', 9000));

        var p = store.NextBatch().Single();
        Assert.StartsWith(@"C:\Users\<user>\a.json", p.Message);
        Assert.Equal(ErrorReport.MaxMessage, p.Message.Length);
        Assert.Equal(ErrorReport.MaxStackTrace, p.StackTrace.Length);
    }

    [Fact]
    public void StateSurvivesRestart()
    {
        var path = Path.Combine(Path.GetTempPath(), $"logbeam-telemetry-{Guid.NewGuid():N}.json");
        try
        {
            var store = NewStore(path);
            store.Add("ClubLog", "X", "Falha", "");
            store.LastInstallFingerprint = "abc";

            var reopened = NewStore(path);
            Assert.Equal(1, reopened.PendingCount);
            Assert.Equal("abc", reopened.LastInstallFingerprint);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DamagedFileStartsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), $"logbeam-telemetry-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, "{ isto não é json");
            var store = NewStore(path);
            Assert.Equal(0, store.PendingCount);
            store.Add("UI", "X", "Falha", "");   // e continua a funcionar
            Assert.Equal(1, NewStore(path).PendingCount);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RevokeClearsEverythingAndKeepsTheIdToDelete()
    {
        var path = Path.Combine(Path.GetTempPath(), $"logbeam-telemetry-{Guid.NewGuid():N}.json");
        try
        {
            var store = NewStore(path);
            store.Add("UI", "X", "Falha", "");
            store.LastInstallFingerprint = "abc";

            store.Revoke("11111111-2222-3333-4444-555555555555");

            var reopened = NewStore(path);
            Assert.Equal(0, reopened.PendingCount);
            Assert.Equal(string.Empty, reopened.LastInstallFingerprint);
            Assert.Equal("11111111-2222-3333-4444-555555555555", reopened.PendingDeletionId);
        }
        finally { File.Delete(path); }
    }
}
