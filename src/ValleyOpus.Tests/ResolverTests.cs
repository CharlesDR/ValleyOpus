using ValleyOpus.Engine;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Resolver;
using static ValleyOpus.Tests.TestData;

namespace ValleyOpus.Tests;

/// <summary>Recipe matching, staging and the Table's outcomes (doc 12 §3, doc 13 §6, doc 07), over the fixture pack.</summary>
public class ResolverTests
{
    private readonly ContentPipeline pipeline;
    private readonly ItemCatalog catalog;
    private readonly FarmCodex codex = new();

    public ResolverTests()
    {
        (this.pipeline, _, this.catalog) = LoadFixtures();
        this.codex.ApplySecretPicks(this.pipeline.Registry, "game-1");
    }

    private ApparatusRow App(string name) => this.pipeline.Registry.Apparatus.Get(F + name)!;

    private ItemStack S(string name, int quality = 0, int count = 1) => new(Item(this.catalog, name), quality, count);

    private static MachineContext Ctx(ApparatusRow app, string? state, LoadSource source = LoadSource.Hand, string? memory = null, params (string Query, bool Value)[] queries)
        => new(app, new ConditionSnapshot { NeighbourState = state, Queries = queries.ToDictionary(q => q.Query, q => q.Value) }, source, memory);

    [Fact]
    public void Match_IsOrderInsensitiveAndExact()
    {
        RecipeResolver r = this.pipeline.Resolver;
        MatchResult m = r.Match(Ctx(App("Retort"), "Cold"), new[] { S("Terra"), S("Ignis") }, this.codex);
        Assert.Equal(MatchKind.Matched, m.Kind);
        Assert.Equal(F + "IgnisTerra", m.Recipe!.Id);

        // An extra item means no match: no partial matches (doc 12 §3).
        Assert.Equal(MatchKind.None, r.Match(Ctx(App("Retort"), "Cold"), new[] { S("Terra"), S("Ignis"), S("Aqua") }, this.codex).Kind);
    }

    [Fact]
    public void Match_ByHandUndiscovered_IsLocked_AndAutomationCantRunIt()
    {
        RecipeResolver r = this.pipeline.Resolver;
        var tray = new[] { S("Terra"), S("Ignis") };
        Assert.True(r.Match(Ctx(App("Retort"), "Cold"), tray, this.codex).Locked);
        Assert.Equal(MatchKind.None, r.Match(Ctx(App("Retort"), "Cold", LoadSource.Automate, F + "IgnisTerra"), tray, this.codex).Kind);

        this.codex.Record(CodexKeys.Recipe(F + "IgnisTerra"), 1, 1);
        MatchResult automated = r.Match(Ctx(App("Retort"), "Cold", LoadSource.Automate, F + "IgnisTerra"), tray, this.codex);
        Assert.Equal(MatchKind.Matched, automated.Kind);
        Assert.False(automated.Locked);
    }

    [Fact]
    public void ModeSwitch_SameInputsDifferByNeighbourState()
    {
        RecipeResolver r = this.pipeline.Resolver;
        var tray = new[] { S("AquaAqua"), S("Terra") };
        Assert.Equal(F + "Mud", r.Match(Ctx(App("Alembic"), "Cold"), tray, this.codex).Recipe!.Id);
        Assert.Equal(F + "Spirit", r.Match(Ctx(App("Alembic"), "Heated"), tray, this.codex).Recipe!.Id);
        Assert.Equal(MatchKind.None, r.Match(Ctx(App("Alembic"), null), tray, this.codex).Kind);
    }

    [Fact]
    public void CanExtend_AcceptsOnlyItemsTowardARunnableRecipe()
    {
        RecipeResolver r = this.pipeline.Resolver;
        MachineContext cold = Ctx(App("Alembic"), "Cold");
        Assert.True(r.CanExtend(cold, Array.Empty<ItemStack>(), S("Terra"), this.codex).Accepted);
        Assert.True(r.CanExtend(cold, new[] { S("Terra") }, S("AquaAqua"), this.codex).Accepted);
        Assert.Equal(StageRefusal.NoRecipe, r.CanExtend(cold, new[] { S("Terra") }, S("Terra"), this.codex).Refusal);
        Assert.Equal(StageRefusal.NoRecipe, r.CanExtend(cold, Array.Empty<ItemStack>(), S("Ember"), this.codex).Refusal);
        Assert.Equal(StageRefusal.TrayFull, r.CanExtend(cold, new[] { S("Terra"), S("AquaAqua") }, S("Terra"), this.codex).Refusal);
        Assert.Equal(StageRefusal.NoNeighbourState, r.CanExtend(Ctx(App("Alembic"), null), Array.Empty<ItemStack>(), S("Terra"), this.codex).Refusal);
    }

    [Fact]
    public void CanExtend_AutomatedLoadsOnlyTheRememberedDiscoveredRecipe()
    {
        RecipeResolver r = this.pipeline.Resolver;
        ApparatusRow alembic = App("Alembic");
        Assert.Equal(StageRefusal.NoRecipeMemory, r.CanExtend(Ctx(alembic, "Cold", LoadSource.Automate), Array.Empty<ItemStack>(), S("Terra"), this.codex).Refusal);
        Assert.Equal(StageRefusal.NotDiscovered, r.CanExtend(Ctx(alembic, "Cold", LoadSource.Chaining, F + "Mud"), Array.Empty<ItemStack>(), S("Terra"), this.codex).Refusal);
        this.codex.Record(CodexKeys.Recipe(F + "Mud"), 1, 1);
        Assert.True(r.CanExtend(Ctx(alembic, "Cold", LoadSource.Chaining, F + "Mud"), Array.Empty<ItemStack>(), S("Terra"), this.codex).Accepted);
        Assert.Equal(StageRefusal.NoRecipe, r.CanExtend(Ctx(alembic, "Cold", LoadSource.Chaining, F + "Spirit"), Array.Empty<ItemStack>(), S("Terra"), this.codex).Refusal);
        Assert.Equal(StageRefusal.NotAutomatable, r.CanExtend(Ctx(App("Table"), "Table", LoadSource.Automate, F + "IgnisTerra"), Array.Empty<ItemStack>(), S("Terra"), this.codex).Refusal);
    }

    [Fact]
    public void CanExtend_GatedMachineWaitsIdle()
    {
        RecipeResolver r = this.pipeline.Resolver;
        ApparatusRow retort = App("Retort");
        var tray = new[] { S("TerraTerra") };
        StageDecision dark = r.CanExtend(Ctx(retort, "Cold", queries: ("CharlesDR.ValleyOpus_DARKNESS", true)), tray, S("Mud"), this.codex);
        Assert.True(dark.Accepted);
        StageDecision light = r.CanExtend(Ctx(retort, "Cold", queries: ("CharlesDR.ValleyOpus_DARKNESS", false)), tray, S("Mud"), this.codex);
        Assert.Equal(StageRefusal.WaitingForConditions, light.Refusal);
        Assert.Equal(new[] { F + "Dark" }, light.FailedConditions);
    }

    [Fact]
    public void Table_SuccessBlockedAndUndefined()
    {
        RecipeResolver r = this.pipeline.Resolver;
        MachineContext table = Ctx(App("Table"), "Table");

        TableOutcome success = r.AttemptAtTable(table, new[] { S("Aqua"), S("Ignis") }, this.codex);
        Assert.Equal(TableOutcomeKind.Success, success.Kind);
        Assert.Equal(SubstanceClass.Power, this.pipeline.Registry.Substances.Get(success.Recipe!.Row.Output.Id)!.Class); // revealed with a puff

        TableOutcome doubled = r.AttemptAtTable(table, new[] { S("Ignis", count: 2) }, this.codex);
        Assert.Equal(F + "IgnisIgnis", doubled.Recipe!.Id);

        // Terra + Aqua-Aqua is an Alembic recipe: the Table names the missing apparatus and consumes nothing.
        TableOutcome blocked = r.AttemptAtTable(table, new[] { S("Terra"), S("AquaAqua") }, this.codex);
        Assert.Equal(TableOutcomeKind.Blocked, blocked.Kind);

        TableOutcome undefined = r.AttemptAtTable(table, new[] { S("Ember"), S("Puddle") }, this.codex);
        Assert.Equal(TableOutcomeKind.Undefined, undefined.Kind);
        Assert.Equal(r.AttemptAtTable(table, new[] { S("Puddle"), S("Ember") }, this.codex).TriedSignature, undefined.TriedSignature);

        // Three inputs on a two-slot Table aren't attemptable (doc 12 R2); with the third slot they are.
        Assert.Equal(TableOutcomeKind.TooManyInputs, r.AttemptAtTable(table, new[] { S("Ignis"), S("Aqua"), S("Terra") }, this.codex).Kind);
        Assert.Equal(TableOutcomeKind.Success, r.AttemptAtTable(table with { Capacity = 3 }, new[] { S("Ignis"), S("Aqua"), S("Terra") }, this.codex).Kind);
    }

    [Fact]
    public void Secret_UnpickedCandidatesAreUndefined()
    {
        RecipeResolver r = this.pipeline.Resolver;
        MachineContext table = Ctx(App("Table"), "Table");
        string picked = Assert.Single(this.codex.Data.SecretPicks[F + "Pool"]);
        var trays = new Dictionary<string, ItemStack[]>
        {
            [F + "SecretA"] = new[] { S("IgnisIgnis"), S("AquaAqua") },
            [F + "SecretB"] = new[] { S("IgnisIgnis"), S("TerraTerra") },
            [F + "SecretC"] = new[] { S("AquaAqua"), S("TerraTerra") },
        };
        foreach ((string id, ItemStack[] tray) in trays)
            Assert.Equal(id == picked ? TableOutcomeKind.Success : TableOutcomeKind.Undefined, r.AttemptAtTable(table, tray, this.codex).Kind);
    }

    [Fact]
    public void Conditions_BonusesAreCollected()
    {
        ConditionOutcome outcome = this.pipeline.Resolver.Conditions.Check(
            this.pipeline.Registry.Recipes.Get(F + "Spirit")!,
            new ConditionSnapshot { Neighbours = new[] { NeighbourFact.Of(2, -2, "flower_item") } });
        Assert.True(outcome.Runnable);
        Assert.Equal(1, outcome.BonusQualitySteps);
    }
}
