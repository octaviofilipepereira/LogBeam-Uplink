// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Api;

namespace LogBeam.Tests.Api;

public class LogbookCredentialsTests
{
    [Theory]
    [InlineData("ff8fe16483", "ff8fe16483")]
    [InlineData("  FF8FE16483 ", "ff8fe16483")]
    [InlineData("https://app.logbeam.org/view.php?id=ff8fe16483", "ff8fe16483")]
    [InlineData("app.logbeam.org/view.php?lang=pt&id=FF8FE16483#globe", "ff8fe16483")]
    [InlineData("https://app.logbeam.org/embed3d/ff8fe16483", "ff8fe16483")]
    public void ExtractInstanceId_AcceptsIdOrLink(string input, string expected)
    {
        Assert.Equal(expected, LogbookCredentials.ExtractInstanceId(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ff8fe1648")]                                   // 9 caracteres
    [InlineData("ff8fe164831")]                                 // 11 caracteres
    [InlineData("ff8fe1648z")]                                  // não hexadecimal
    [InlineData("https://app.logbeam.org/view.php?id=ff8fe164831")]
    [InlineData("https://app.logbeam.org/op/CT7BFV")]
    public void ExtractInstanceId_RejectsInvalid(string? input)
    {
        Assert.Null(LogbookCredentials.ExtractInstanceId(input));
    }

    [Theory]
    [InlineData("4a20e9c445f14412771b50f66b39fad39dee1a4d2687dc9654e4e308fe471adf")]
    [InlineData(" 4A20E9C445F14412771B50F66B39FAD39DEE1A4D2687DC9654E4E308FE471ADF ")]
    public void IsApiKey_Accepts64Hex(string input)
    {
        Assert.True(LogbookCredentials.IsApiKey(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Abcdefghijklmnopqrstuvwxy")]                    // 25 caracteres, como no erro de 02/10/2026
    [InlineData("4a20e9c445f14412771b50f66b39fad39dee1a4d2687dc9654e4e308fe471ad")]   // 63
    [InlineData("4a20e9c445f14412771b50f66b39fad39dee1a4d2687dc9654e4e308fe471adg")]  // 'g'
    public void IsApiKey_RejectsInvalid(string? input)
    {
        Assert.False(LogbookCredentials.IsApiKey(input));
    }

    [Fact]
    public void NormalizeApiKey_TrimsAndLowercases()
    {
        Assert.Equal("abc", LogbookCredentials.NormalizeApiKey("  ABC "));
    }
}
