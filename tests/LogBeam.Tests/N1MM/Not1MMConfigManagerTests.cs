// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json.Nodes;
using LogBeam.Core.N1MM;
using Microsoft.Extensions.Logging.Abstractions;

namespace LogBeam.Tests.N1MM;

/// <summary>
/// Testes sobre uma cópia temporária de Fixtures/Not1MM/not1mm.json (preferências reais do Not1MM,
/// criadas no primeiro arranque, com o envio de pacotes N1MM desligado, como vem de origem).
/// </summary>
public class Not1MMConfigManagerTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "logbeam-not1mm-" + Guid.NewGuid().ToString("N"));
    private readonly Not1MMConfigManager _mgr;

    public Not1MMConfigManagerTests()
    {
        Directory.CreateDirectory(_folder);
        _mgr = new Not1MMConfigManager(NullLogger<Not1MMConfigManager>.Instance, Path.Combine(_folder, Not1MMConfigManager.FileName));
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    /// <summary>Copia as preferências de exemplo, alterando as chaves indicadas.</summary>
    private void WritePrefs(bool packets = false, bool contact = false, string port = "127.0.0.1:12060")
    {
        var root = (JsonObject)JsonNode.Parse(Fixture.Text("Not1MM/not1mm.json"))!;
        root["send_n1mm_packets"] = packets;
        root["send_n1mm_contact"] = contact;
        root["n1mm_contactport"]  = port;
        File.WriteAllText(_mgr.ConfigPath, root.ToJsonString());
    }

    private JsonObject Prefs() => (JsonObject)JsonNode.Parse(File.ReadAllText(_mgr.ConfigPath))!;

    private string[] Backups() => Directory.GetFiles(_folder, Not1MMConfigManager.FileName + ".backup_*");

    // ─── ReadStatus ───────────────────────────────────────────────────────

    [Fact]
    public void ReadStatus_NoFile_IsNotFound() =>
        Assert.Equal(N1MMBroadcastStatus.IniNotFound, _mgr.ReadStatus(12060));

    [Fact]
    public void ReadStatus_AsCreatedByNot1MM_IsNotConfigured()
    {
        WritePrefs();
        Assert.Equal(N1MMBroadcastStatus.NotConfigured, _mgr.ReadStatus(12060));
    }

    [Fact]
    public void ReadStatus_PacketsWithoutContacts_IsNotConfigured()
    {
        WritePrefs(packets: true);
        Assert.Equal(N1MMBroadcastStatus.NotConfigured, _mgr.ReadStatus(12060));
    }

    [Fact]
    public void ReadStatus_EnabledToThisComputer_IsConfiguredCorrectly()
    {
        WritePrefs(packets: true, contact: true);
        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(12060));
    }

    [Fact]
    public void ReadStatus_Localhost_IsConfiguredCorrectly()
    {
        WritePrefs(packets: true, contact: true, port: "localhost:12060");
        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(12060));
    }

    [Fact]
    public void ReadStatus_OtherPort_IsDifferentPort()
    {
        WritePrefs(packets: true, contact: true, port: "127.0.0.1:12061");
        Assert.Equal(N1MMBroadcastStatus.ConfiguredDifferentPort, _mgr.ReadStatus(12060));
    }

    [Fact]
    public void ReadStatus_InvalidJson_IsNotFound()
    {
        File.WriteAllText(_mgr.ConfigPath, "{ isto não é json");
        Assert.Equal(N1MMBroadcastStatus.IniNotFound, _mgr.ReadStatus(12060));
    }

    // ─── ApplyConfiguration ───────────────────────────────────────────────

    [Fact]
    public void Apply_AsCreatedByNot1MM_EnablesAndKeepsTheOtherKeys()
    {
        WritePrefs();
        var before = Prefs();

        Assert.Equal(N1MMApplyResult.Applied, _mgr.ApplyConfiguration(12060));

        var after = Prefs();
        Assert.True(after["send_n1mm_packets"]!.GetValue<bool>());
        Assert.True(after["send_n1mm_contact"]!.GetValue<bool>());
        Assert.Equal("127.0.0.1:12060", after["n1mm_contactport"]!.GetValue<string>());
        Assert.Equal(before.Count, after.Count);
        foreach (var (key, value) in before)
        {
            if (key is "send_n1mm_packets" or "send_n1mm_contact" or "n1mm_contactport") continue;
            Assert.True(JsonNode.DeepEquals(value, after[key]), $"a chave {key} mudou");
        }
        Assert.Equal(N1MMBroadcastStatus.ConfiguredCorrectly, _mgr.ReadStatus(12060));
        Assert.Single(Backups());
    }

    [Fact]
    public void Apply_OtherPort_UsesThePortOfTheN1mmTab()
    {
        WritePrefs(packets: true, contact: true, port: "127.0.0.1:12060");
        Assert.Equal(N1MMApplyResult.Applied, _mgr.ApplyConfiguration(12061));
        Assert.Equal("127.0.0.1:12061", Prefs()["n1mm_contactport"]!.GetValue<string>());
    }

    [Fact]
    public void Apply_AlreadyConfigured_DoesNotTouchTheFile()
    {
        WritePrefs(packets: true, contact: true);
        var bytes = File.ReadAllBytes(_mgr.ConfigPath);

        Assert.Equal(N1MMApplyResult.AlreadyConfigured, _mgr.ApplyConfiguration(12060));
        Assert.Equal(bytes, File.ReadAllBytes(_mgr.ConfigPath));
        Assert.Empty(Backups());
    }

    [Fact]
    public void Apply_NoFile_Fails() =>
        Assert.Equal(N1MMApplyResult.Failed, _mgr.ApplyConfiguration(12060));

    [Fact]
    public void Apply_InvalidJson_FailsAndKeepsTheFile()
    {
        File.WriteAllText(_mgr.ConfigPath, "{ isto não é json");
        Assert.Equal(N1MMApplyResult.Failed, _mgr.ApplyConfiguration(12060));
        Assert.Equal("{ isto não é json", File.ReadAllText(_mgr.ConfigPath));
    }
}
