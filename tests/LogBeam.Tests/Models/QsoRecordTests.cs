// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Models;

namespace LogBeam.Tests.Models;

public class QsoRecordTests
{
    private static readonly DateTime Received = new(2026, 9, 28, 21, 26, 38, DateTimeKind.Utc);

    [Theory]
    [InlineData("20260928", "212515", 21, 25, 15)]
    [InlineData("20260928", "2125", 21, 25, 0)]
    [InlineData(" 20260928 ", " 212515 ", 21, 25, 15)]
    public void QsoTimeUtc_UsesQsoDateAndTimeOn(string date, string time, int h, int m, int s)
    {
        var qso = new QsoRecord { QsoDate = date, TimeOn = time, ReceivedAt = Received };

        var t = qso.QsoTimeUtc();

        Assert.Equal(new DateTime(2026, 9, 28, h, m, s, DateTimeKind.Utc), t);
        Assert.Equal(DateTimeKind.Utc, t.Kind);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("20260928", "")]
    [InlineData("20260928", "2561")]
    [InlineData("2026-09-28", "21:25")]
    public void QsoTimeUtc_FallsBackToReceivedAt_WhenMissingOrInvalid(string date, string time)
    {
        var qso = new QsoRecord { QsoDate = date, TimeOn = time, ReceivedAt = Received };

        Assert.Equal(Received, qso.QsoTimeUtc());
    }
}
