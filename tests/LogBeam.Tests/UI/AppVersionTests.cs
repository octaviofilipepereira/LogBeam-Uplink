// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Xml.Linq;
using LogBeam.UI;

namespace LogBeam.Tests.UI;

public class AppVersionTests
{
    /// <summary>A versão da aplicação é a do Directory.Build.props (a mesma que o release.yml exige na etiqueta).</summary>
    [Fact]
    public void CurrentComesFromDirectoryBuildProps()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Directory.Build.props"))) dir = dir.Parent;
        Assert.NotNull(dir);

        var expected = XDocument.Load(Path.Combine(dir!.FullName, "Directory.Build.props"))
            .Descendants("Version").Single().Value;

        Assert.Matches(@"^\d+\.\d+\.\d+$", AppVersion.Current);
        Assert.Equal(expected, AppVersion.Current);
        Assert.Equal($"LogBeam Uplink v{expected} by CT7BFV", AppVersion.Title);
    }
}
