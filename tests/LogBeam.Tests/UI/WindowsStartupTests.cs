// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.UI;
using Microsoft.Win32;

namespace LogBeam.Tests.UI;

/// <summary>Usa uma chave própria em HKCU (nunca a chave Run verdadeira), apagada no fim.</summary>
public sealed class WindowsStartupTests : IDisposable
{
    private readonly string _key = @"Software\LogBeamUplinkTests-" + Guid.NewGuid().ToString("N");

    public void Dispose() => Registry.CurrentUser.DeleteSubKeyTree(_key, throwOnMissingSubKey: false);

    [Fact]
    public void EnableWritesQuotedPathWithTrayArgumentAndDisableRemovesIt()
    {
        Assert.False(WindowsStartup.IsEnabled(_key));

        WindowsStartup.Set(true, @"C:\Programas\LogBeam Uplink\LogBeam.exe", _key);
        Assert.True(WindowsStartup.IsEnabled(_key));
        using (var k = Registry.CurrentUser.OpenSubKey(_key))
            Assert.Equal("\"C:\\Programas\\LogBeam Uplink\\LogBeam.exe\" --tray", k!.GetValue(WindowsStartup.ValueName));

        WindowsStartup.Set(false, @"C:\Programas\LogBeam Uplink\LogBeam.exe", _key);
        Assert.False(WindowsStartup.IsEnabled(_key));
    }

    [Fact]
    public void DisableWhenAbsentDoesNothing()
    {
        WindowsStartup.Set(false, @"C:\x\LogBeam.exe", _key);
        Assert.False(WindowsStartup.IsEnabled(_key));
    }

    [Theory]
    [InlineData("4a20e9c445f14412771b50f66b39fad39dee1a4d2687dc9654e4e308fe471adf", "●●●●●●●●●●●●1adf")]
    [InlineData("abcd", "●●●●")]
    [InlineData("ab", "●●")]
    public void ApiKeyMaskKeepsOnlyTheLastFourCharacters(string key, string expected)
    {
        Assert.Equal(expected, MainForm.MaskApiKey(key));
    }
}
