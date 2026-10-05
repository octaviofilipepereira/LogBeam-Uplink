// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Service.Telemetry;

namespace LogBeam.Tests.Telemetry;

public class PathScrubberTests
{
    [Theory]
    [InlineData(@"C:\Users\Joao Silva\AppData\Local\x.json", @"C:\Users\<user>\AppData\Local\x.json")]
    [InlineData(@"c:/users/joao/Documents/a.txt",            @"c:/users/<user>/Documents/a.txt")]
    [InlineData(@"at X() in D:\Users\JOAO~1\src\a.cs:line 3", @"at X() in D:\Users\<user>\src\a.cs:line 3")]
    [InlineData(@"Could not find file 'C:\Users\ana'",        @"Could not find file 'C:\Users\<user>'")]
    [InlineData(@"C:\Program Files\LogBeam\settings.json",    @"C:\Program Files\LogBeam\settings.json")]
    public void RemovesUserFolderName(string input, string expected)
    {
        Assert.Equal(expected, PathScrubber.Scrub(input, null, null));
    }

    [Fact]
    public void RemovesUserAndMachineNamesAsWholeWords()
    {
        var s = PathScrubber.Scrub("Access denied for joao on DESKTOP-AB12 (joao@desktop-ab12)", "joao", "DESKTOP-AB12");
        Assert.Equal("Access denied for <user> on <pc> (<user>@<pc>)", s);
    }

    [Fact]
    public void CurrentUserNameWithApostropheIsRemovedFromPaths()
    {
        var s = PathScrubber.Scrub(@"Could not find file 'C:\Users\O'Brien\a.json'", "O'Brien", null);
        Assert.Equal(@"Could not find file 'C:\Users\<user>\a.json'", s);
    }

    [Fact]
    public void DoesNotBreakIdentifiersThatContainTheUserName()
    {
        var s = PathScrubber.Scrub("at LogBeam.UI.UserSettings.Load() user", "user", null);
        Assert.Equal("at LogBeam.UI.UserSettings.Load() <user>", s);
    }

    [Fact]
    public void IgnoresVeryShortNames()
    {
        Assert.Equal("ab cd", PathScrubber.Scrub("ab cd", "ab", "cd"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptyInputGivesEmptyString(string? input)
    {
        Assert.Equal(string.Empty, PathScrubber.Scrub(input, "joao", "PC"));
    }
}
