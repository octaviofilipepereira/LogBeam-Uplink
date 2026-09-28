// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.UI.Localization;

namespace LogBeam.Tests.Localization;

public class LocalizationTests
{
    [Fact]
    public void EveryKey_HasTextInAllFourLanguages()
    {
        Assert.Empty(L.KeysWithMissingText());
    }

    [Fact]
    public void Languages_ArePtEnEsFr()
    {
        Assert.Equal(new[] { "PT", "EN", "ES", "FR" }, L.Languages);
    }

    [Theory]
    [InlineData("PT", "Sair")]
    [InlineData("EN", "Exit")]
    [InlineData("ES", "Salir")]
    [InlineData("FR", "Quitter")]
    [InlineData("XX", "Sair")]   // língua desconhecida → português
    public void Get_ReturnsTextInCurrentLanguage(string lang, string expected)
    {
        var previous = L.Lang;
        try
        {
            L.Lang = lang;
            Assert.Equal(expected, L.Get("btn_exit"));
        }
        finally
        {
            L.Lang = previous;
        }
    }
}
