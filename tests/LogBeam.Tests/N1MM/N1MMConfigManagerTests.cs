// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text;
using LogBeam.Core.N1MM;
using Microsoft.Extensions.Logging.Abstractions;

namespace LogBeam.Tests.N1MM;

/// <summary>
/// Testes sobre uma cópia temporária de Fixtures/N1MM/N1MM Logger.ini (formato real do N1MM+:
/// CRLF, Latin-1, várias secções).
/// </summary>
public class N1MMConfigManagerTests : IDisposable
{
    private static readonly Encoding Latin1 = Encoding.Latin1;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "logbeam-n1mm-" + Guid.NewGuid().ToString("N"));
    private readonly N1MMConfigManager _mgr = new(NullLogger<N1MMConfigManager>.Instance);

    public N1MMConfigManagerTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    private string IniPath => Path.Combine(_folder, N1MMConfigManager.IniFileName);

    /// <summary>Copia o ficheiro de exemplo, aplicando substituições de texto.</summary>
    private void WriteIni(params (string From, string To)[] changes)
    {
        var text = Latin1.GetString(Fixture.Bytes("N1MM/N1MM Logger.ini"));
        foreach (var (from, to) in changes) text = text.Replace(from, to);
        File.WriteAllText(IniPath, text, Latin1);
    }

    private string[] Lines() => Latin1.GetString(File.ReadAllBytes(IniPath)).Split("\r\n");

    // ─── ReadStatus ───────────────────────────────────────────────────────

    [Fact]
    public void ReadStatus_RealFormat_IsConfiguredCorrectly()
    {
        WriteIni();
        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(_folder, 12060));
    }

    [Fact]
    public void ReadStatus_BroadcastOff_IsNotConfigured()
    {
        WriteIni(("IsBroadcastContact=True", "IsBroadcastContact=False"));
        Assert.Equal(N1MMBroadcastStatus.NotConfigured, _mgr.ReadStatus(_folder, 12060));
    }

    [Fact]
    public void ReadStatus_OnlyOtherPort_IsDifferentPort()
    {
        WriteIni(("BroadcastContactAddr=127.0.0.1:12060", "BroadcastContactAddr=127.0.0.1:12061"));
        Assert.Equal(N1MMBroadcastStatus.ConfiguredDifferentPort, _mgr.ReadStatus(_folder, 12060));
    }

    [Fact]
    public void ReadStatus_OurPortAmongSeveralTargets_IsConfiguredCorrectly()
    {
        WriteIni(("BroadcastContactAddr=127.0.0.1:12060", "BroadcastContactAddr=192.168.1.20:12061 localhost:12060"));
        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(_folder, 12060));
    }

    [Fact]
    public void ReadStatus_MissingFile_IsIniNotFound()
    {
        Assert.Equal(N1MMBroadcastStatus.IniNotFound, _mgr.ReadStatus(_folder, 12060));
    }

    // ─── ApplyConfiguration ───────────────────────────────────────────────

    [Fact]
    public void Apply_AlreadyConfigured_DoesNotTouchFile()
    {
        WriteIni();
        var before = File.ReadAllBytes(IniPath);

        Assert.Equal(N1MMApplyResult.AlreadyConfigured, _mgr.ApplyConfiguration(_folder, "127.0.0.1", 12060));

        Assert.Equal(before, File.ReadAllBytes(IniPath));
        Assert.Empty(Directory.GetFiles(_folder, "*.backup*"));
    }

    [Fact]
    public void Apply_KeepsOtherTargets_AndChangesOnlyTwoLines()
    {
        WriteIni(("BroadcastContactAddr=127.0.0.1:12060", "BroadcastContactAddr=192.168.1.20:12061"),
                 ("IsBroadcastContact=True", "IsBroadcastContact=False"));
        var before = Lines();

        Assert.Equal(N1MMApplyResult.Applied, _mgr.ApplyConfiguration(_folder, "127.0.0.1", 12060));

        var after = Lines();
        Assert.Equal(before.Length, after.Length);
        var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).Select(i => after[i]).ToList();
        Assert.Equal(new[] { "BroadcastContactAddr=192.168.1.20:12061 127.0.0.1:12060", "IsBroadcastContact=True" }, changed);
        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(_folder, 12060));
        Assert.Single(Directory.GetFiles(_folder, "*.backup*"));
    }

    [Fact]
    public void Apply_PreservesNonAsciiBytesAndLineEndings()
    {
        WriteIni(("IsBroadcastContact=True", "IsBroadcastContact=False"));

        _mgr.ApplyConfiguration(_folder, "127.0.0.1", 12060);

        var text = Latin1.GetString(File.ReadAllBytes(IniPath));
        Assert.Contains("Operador=José Simões", text);
        Assert.DoesNotContain("\n", text.Replace("\r\n", ""));
    }

    [Fact]
    public void Apply_KeyMissing_IsInsertedInSection()
    {
        WriteIni(("BroadcastContactAddr=127.0.0.1:12060\r\n", ""));

        Assert.Equal(N1MMApplyResult.Applied, _mgr.ApplyConfiguration(_folder, "127.0.0.1", 12060));

        var lines = Lines().ToList();
        var header = lines.IndexOf("[ExternalBroadcast]");
        var next = lines.FindIndex(header + 1, l => l.StartsWith('['));
        Assert.Contains("BroadcastContactAddr=127.0.0.1:12060", lines.GetRange(header, next - header));
    }

    [Fact]
    public void Apply_SectionMissing_IsAppended()
    {
        File.WriteAllText(IniPath, "\r\n[ApplicationColors]\r\nColorText=0\r\n", Latin1);

        Assert.Equal(N1MMApplyResult.Applied, _mgr.ApplyConfiguration(_folder, "127.0.0.1", 12060));

        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(_folder, 12060));
        Assert.EndsWith("\r\n", Latin1.GetString(File.ReadAllBytes(IniPath)));
    }

    [Fact]
    public void Apply_MissingFile_Fails()
    {
        Assert.Equal(N1MMApplyResult.Failed, _mgr.ApplyConfiguration(_folder, "127.0.0.1", 12060));
    }
}
