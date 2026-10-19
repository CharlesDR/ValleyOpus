using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Machines;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Net;
using ValleyOpus.Engine.Routes;
using ValleyOpus.Engine.Skill;
using ValleyOpus.Engine.State;
using static ValleyOpus.Tests.TestData;

namespace ValleyOpus.Tests;

/// <summary>Machine state, skill, codex sync, chain geometry and the envelope's B2 sections.</summary>
public class SystemsTests
{
    [Fact]
    public void MachineData_RoundTripsThroughModData()
    {
        var data = new Dictionary<string, string>();
        var state = new MachineData { TrayId = "t1", RecipeMemory = "R", Pinned = true, Owner = 42, Recipe = "R", Locked = true, Kindled = true, Fuel = 3 };
        state.Extras.Add(("(O)A", 2));
        state.Extras.Add(("(O)B", 1));
        state.Write(data);
        Assert.Equal("1", data[ObjectData.SchemaKey]);

        MachineData back = MachineData.Read(data);
        Assert.Equal(("(O)A", 2), back.TakeNextExtra());
        Assert.Single(back.Extras);
        Assert.True(back.Pinned && back.Locked && back.Kindled);
        Assert.Equal(42, back.Owner);

        back.EndRun();
        back.Write(data);
        Assert.False(data.ContainsKey(MachineData.RecipeKey));
        Assert.Equal("R", data[MachineData.RecipeMemoryKey]); // memory outlives a run
    }

    [Fact]
    public void Skill_LevelsBankPastFour()
    {
        Assert.Equal(0, SkillMath.LevelFor(99));
        Assert.Equal(1, SkillMath.LevelFor(100));
        Assert.Equal(4, SkillMath.LevelFor(1300));
        Assert.Equal(4, SkillMath.LevelFor(99999));
        Assert.Equal(36, SkillMath.SatchelCapacity(2));
    }

    [Fact]
    public void Xp_OncePerKeyAndScaledForAutomation()
    {
        var raw = Raw(
            (RegistryNames.XpSources, "d", new { Event = "Discovery", Amount = 100, TierScale = true, OncePer = "Recipe" }),
            (RegistryNames.XpSources, "a", new { Event = "AutoCollect", Amount = 2, CapPerAction = 20, Scale = 0.25 }));
        var registry = Engine.Registry.RegistryLoader.Load(raw);
        var xp = new XpCalculator(registry);
        var keys = new HashSet<string>();
        XpGrant? first = xp.For(XpEvents.Discovery, "R", 2, 1, keys.Contains);
        Assert.Equal(200, first!.Amount);
        keys.Add(first.OnceKey!);
        Assert.Null(xp.For(XpEvents.Discovery, "R", 2, 1, keys.Contains));
        Assert.Equal(0.5, xp.For(XpEvents.AutoCollect, "x", 1, 1, keys.Contains)!.Amount);
        Assert.Equal(5, xp.For(XpEvents.AutoCollect, "x", 1, 50, keys.Contains)!.Amount); // capped at 20 × 0.25
    }

    [Fact]
    public void CodexSync_HostKeepsFirstAttributionAndBroadcastsOnlyNews()
    {
        var host = new FarmCodex();
        host.Record("recipe:A", 1, 1);
        CodexDelta delta = CodexSync.ApplyRequest(host, new CodexRequest { Keys = { "recipe:A", "recipe:B" }, Counts = { ["count:x"] = 2 }, Day = 5 }, 7);
        Assert.Single(delta.Keys);
        Assert.Equal(7, delta.Keys["recipe:B"].Player);
        Assert.Equal(2, delta.Counts["count:x"]);

        var mirror = new FarmCodex();
        Assert.Equal(new[] { "recipe:B" }, CodexSync.ApplyDelta(mirror, delta));
        Assert.Empty(CodexSync.ApplyDelta(mirror, delta));
    }

    [Fact]
    public void ChainGeometry_HeatGoesToTheFirstTwoCardinalConsumers()
    {
        var consumers = new HashSet<(int, int)> { (5, 4), (6, 5), (5, 6), (4, 5) };
        bool Wants((int X, int Y) t) => consumers.Contains(t);
        // Athanor at (5,5): N (5,4) and E (6,5) are served, S and W aren't.
        Assert.True(ChainGeometry.ReceivesHeat((6, 5), new[] { (5, 5) }, Wants, 2));
        Assert.False(ChainGeometry.ReceivesHeat((4, 5), new[] { (5, 5) }, Wants, 2));
        Assert.Equal((6, 5), ChainGeometry.Step((5, 5), ChainGeometry.Spout));
        Assert.Equal(24, ChainGeometry.Within((0, 0), 2).Count());
    }

    [Fact]
    public void Envelope_KeepsStrippedBuildings()
    {
        var envelope = new FarmEnvelope { StrippedAtDay = 12 };
        envelope.StrippedBuildings.Add(new StrippedBuilding { BuildingType = "CharlesDR.ValleyOpus_Lab", TileX = 3, TileY = 4, Xml = "<Building />" });
        FarmEnvelope back = EnvelopeJson.Read(EnvelopeJson.ToNode(envelope));
        Assert.Equal(12, back.StrippedAtDay);
        Assert.Equal("<Building />", back.StrippedBuildings.Single().Xml);
    }
}
