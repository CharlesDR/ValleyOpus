using System.Text.Json.Nodes;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.State;
using static ValleyOpus.Tests.TestData;

namespace ValleyOpus.Tests;

/// <summary>Codex keys, the secret pick, the save envelope and migrations (doc 07, doc 12 §10–11, doc 17 §5).</summary>
public class StateTests
{
    [Fact]
    public void TriedSignature_IsSortedAndKeepsDuplicates()
    {
        Assert.Equal(CodexKeys.Tried(new[] { "(O)B", "(O)A" }), CodexKeys.Tried(new[] { "(O)A", "(O)B" }));
        Assert.Equal("tried:(O)A+(O)A", CodexKeys.Tried(new[] { "(O)A", "(O)A" }));
        Assert.NotEqual(CodexKeys.Tried(new[] { "(O)A" }), CodexKeys.Tried(new[] { "(O)A", "(O)A" }));
    }

    [Fact]
    public void Codex_FirstRecordKeepsAttribution()
    {
        var codex = new FarmCodex();
        Assert.True(codex.Record(CodexKeys.Recipe("R"), day: 3, player: 111));
        Assert.False(codex.Record(CodexKeys.Recipe("R"), day: 3, player: 222));
        Assert.Equal(111, codex.Data.Keys[CodexKeys.Recipe("R")].Player);
        Assert.True(codex.IsDiscovered("R"));
        Assert.Equal(2, new[] { codex.CountCraft("(O)X", handMade: true), codex.CountCraft("(O)X", handMade: false) }.Max());
        Assert.Equal(1, codex.Data.Counts[CodexKeys.HandCount("(O)X")]);
    }

    [Fact]
    public void Secret_PickIsStablePerFarmAndRerollReplacesIt()
    {
        var registry = LoadFixtures().Pipeline.Registry;
        var a = new FarmCodex();
        a.ApplySecretPicks(registry, "farm-123");
        var b = new FarmCodex();
        b.ApplySecretPicks(registry, "farm-123");
        Assert.Equal(a.Data.SecretPicks[F + "Pool"], b.Data.SecretPicks[F + "Pool"]);

        string first = Assert.Single(a.Data.SecretPicks[F + "Pool"]);
        a.Reroll(registry, "farm-123");
        string second = Assert.Single(a.Data.SecretPicks[F + "Pool"]);
        Assert.NotEqual(first, second);
        Assert.True(a.IsDisabledOnFarm(first));
        Assert.False(a.IsDisabledOnFarm(second));
        Assert.Equal(1, a.Data.RerollCount);

        // Different farms spread across the candidates.
        var picks = Enumerable.Range(0, 30).Select(i => SecretPicker.Pick(new[] { "x", "y", "z" }, 1, "farm-" + i, 0, "pool", null)[0]).Distinct();
        Assert.Equal(3, picks.Count());
    }

    [Fact]
    public void Envelope_RoundTrips()
    {
        var envelope = new FarmEnvelope { ModVersion = "0.1.0-alpha.1" };
        envelope.Codex.Keys[CodexKeys.Item("(O)X")] = new CodexEntry { Day = 4, Player = 99 };
        envelope.Codex.SecretPicks["pool"] = new() { "r" };
        envelope.Players["99"] = new PlayerRecord { XpKeys = { "first-table" }, SatchelCapacity = 12 };
        envelope.Unlocks["future"] = 1;

        FarmEnvelope back = EnvelopeJson.Read(JsonNode.Parse(EnvelopeJson.Write(envelope))!.AsObject());
        Assert.Equal(EnvelopeJson.Write(envelope), EnvelopeJson.Write(back));
        Assert.Equal(1, back.SchemaVersion);
    }

    [Fact]
    public void Migration_RunsEveryStepInOrderAndKeepsABackup()
    {
        var steps = new[]
        {
            new MigrationStep(1, "rename Codex.Keys to Codex.Discovered", json =>
            {
                JsonObject codex = json["Codex"]!.AsObject();
                JsonNode keys = codex["Keys"]!;
                codex.Remove("Keys");
                codex["Discovered"] = keys;
                return 1;
            }),
            new MigrationStep(2, "and back again", json =>
            {
                JsonObject codex = json["Codex"]!.AsObject();
                JsonNode keys = codex["Discovered"]!;
                codex.Remove("Discovered");
                codex["Keys"] = keys;
                return 1;
            }),
        };
        var saved = EnvelopeJson.ToNode(new FarmEnvelope());
        saved["Codex"]!["Keys"]!["item:(O)X"] = new JsonObject { ["Day"] = 1, ["Player"] = 7 };

        LoadResult result = new MigrationRunner(steps, currentVersion: 3).Load(saved, "0.1.0-alpha.3");
        Assert.Equal(LoadMode.Migrated, result.Mode);
        Assert.Equal(1, result.FromVersion);
        Assert.Equal(3, result.Envelope.SchemaVersion);
        Assert.True(result.Envelope.Codex.Keys.ContainsKey("item:(O)X"));
        Assert.Equal(2, result.Log.Count);
        Assert.Equal(1, result.Backup!["SchemaVersion"]!.GetValue<int>());
        Assert.True(result.CanWrite);
    }

    [Fact]
    public void Migration_NewerSaveIsReadOnly_AndMissingStepFailsSafely()
    {
        var newer = EnvelopeJson.ToNode(new FarmEnvelope { SchemaVersion = 9 });
        LoadResult readOnly = new MigrationRunner().Load(newer, "0.1.0");
        Assert.Equal(LoadMode.ReadOnly, readOnly.Mode);
        Assert.False(readOnly.CanWrite);

        LoadResult failed = new MigrationRunner(Array.Empty<MigrationStep>(), currentVersion: 2).Load(EnvelopeJson.ToNode(new FarmEnvelope()), "0.2.0");
        Assert.Equal(LoadMode.Failed, failed.Mode);
        Assert.False(failed.CanWrite);

        Assert.Equal(LoadMode.Fresh, new MigrationRunner().Load(null, "0.1.0").Mode);
        Assert.Equal(LoadMode.Current, new MigrationRunner().Load(EnvelopeJson.ToNode(new FarmEnvelope()), "0.1.0").Mode);
    }

    [Fact]
    public void ObjectData_MigratesOnlyOurObjects()
    {
        var vanilla = new Dictionary<string, string> { ["SomeOtherMod/Key"] = "x" };
        Assert.False(ObjectData.Migrate(vanilla));
        Assert.False(vanilla.ContainsKey(ObjectData.SchemaKey));

        var ours = new Dictionary<string, string> { [ObjectData.Key("Tray")] = "[]" };
        var steps = new[] { new ObjectMigrationStep(1, d => { d[ObjectData.Key("RecipeMemory")] = ""; return true; }) };
        Assert.True(ObjectData.Migrate(ours, steps, current: 2));
        Assert.Equal("2", ours[ObjectData.SchemaKey]);
        Assert.True(ours.ContainsKey(ObjectData.Key("RecipeMemory")));
        Assert.False(ObjectData.Migrate(ours, steps, current: 2));
        Assert.Equal(Ids.KeyPrefix + "Schema", ObjectData.SchemaKey);
    }
}
