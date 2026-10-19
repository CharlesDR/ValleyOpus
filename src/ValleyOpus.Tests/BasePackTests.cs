using ValleyOpus.Engine;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Machines;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Packs;
using ValleyOpus.Engine.Pricing;
using ValleyOpus.Engine.Resolver;
using static ValleyOpus.Tests.TestData;

namespace ValleyOpus.Tests;

/// <summary>The base pack's slice content (doc 11, doc 22 B2): the data test, prices and the slice's machine path.</summary>
public class BasePackTests
{
    private const string P = Ids.RowPrefix;
    private readonly ContentPipeline pipeline;
    private readonly PackReadResult read;
    private readonly ItemCatalog catalog;
    private readonly FarmCodex codex = new();

    public BasePackTests()
    {
        this.read = ContentPackReader.Read(BasePack);
        this.catalog = new ItemCatalog(this.read.Items.All.Concat(VanillaLike).Append(ItemInfo.Create("(O)382", 15, -15, "id_o_382")));
        this.pipeline = ContentPipeline.Build(this.read.Registries, this.catalog, catalogIsComplete: false);
    }

    private ApparatusRow App(string name) => this.pipeline.Registry.Apparatus.Get(P + name)!;

    private ItemStack S(string name, int quality = 0) => new(this.catalog.Get("(O)" + P + name)!, quality);

    private static MachineContext Ctx(ApparatusRow app, string state, LoadSource source = LoadSource.Hand, string? memory = null, params NeighbourFact[] neighbours)
        => new(app, new ConditionSnapshot { NeighbourState = state, Neighbours = neighbours }, source, memory);

    [Fact]
    public void DataTest_BasePackHasNoFindings()
    {
        Assert.Empty(this.read.Findings);
        Assert.Empty(this.pipeline.Findings);
    }

    [Fact]
    public void SliceCounts_MatchDoc11()
    {
        var r = this.pipeline.Registry;
        Assert.Equal(8, r.Primes.Count);
        Assert.Equal(6, r.Apparatus.Count);
        Assert.Equal(46, this.read.Items.All.Count(i => i.QualifiedId.StartsWith("(O)" + P) && !i.HasTag("valleyopus_tool")));
        Assert.Equal(36, r.Recipes.All.Count(x => r.Substances.Get(x.Output.Id)?.Tier == 1));
        Assert.Equal(4, r.Recipes.All.Count(x => r.Substances.Get(x.Output.Id)?.Tier == 2));
    }

    [Fact]
    public void Ids_AndSortNames_FollowS11()
    {
        // Every authored object's internal Name carries its sort key, so Primes sort before substances (doc 09 §11).
        Assert.All(this.pipeline.Registry.Primes.All, p => Assert.StartsWith("(O)" + P, p.ItemId));
        Assert.Equal(P + "Ignis", this.pipeline.Registry.Primes.All.OrderBy(p => p.SortOrder).First().Id);
    }

    [Fact]
    public void Prices_AreDerivedFromInputs()
    {
        var prices = new PriceCalculator(this.pipeline.Registry, this.catalog);
        // Tier 1: round(1.2 × (1 + 1)) = 2; with Aether: round(1.2 × (1 + 5)) = 7.
        Assert.Equal(2, prices.PriceOf("(O)" + P + "Calx"));
        Assert.Equal(7, prices.PriceOf("(O)" + P + "Sol"));
        // Tier 2: round(1.4 × (Pulvis 2 + Calx 2)) = 6; Caput Mortuum is fixed at 2, so Smaltum = round(1.4 × 4) = 6.
        Assert.Equal(6, prices.PriceOf("(O)" + P + "Vitrum"));
        Assert.Equal(2, prices.PriceOf("(O)" + P + "CaputMortuum"));
        Assert.Equal(6, prices.PriceOf("(O)" + P + "Smaltum"));
        Assert.All(prices.ComputeAll().Values, v => Assert.True(v >= 1));
    }

    [Fact]
    public void Athanor_KindlesWithTwoIgnis()
    {
        var stager = new Stager(this.pipeline);
        var state = new MachineData();
        var ctx = Ctx(App("Athanor"), "Open");
        StagePlan first = stager.Offer(ctx, state, Array.Empty<ItemStack>(), S("Ignis"), this.codex);
        Assert.Equal(StageAction.Stage, first.Action); // a Prime is never decomposed
        StagePlan second = stager.Offer(ctx, state, new[] { S("Ignis") }, S("Ignis"), this.codex);
        Assert.Equal(StageAction.Start, second.Action);
        RunPlan run = stager.BuildRecipeRun(ctx, second.Match!, new[] { S("Ignis"), S("Ignis") });
        Assert.Equal(P + "FlammaViva", run.ProcessState);
        Assert.Null(run.OutputItemId);

        state.Kindled = true;
        Assert.Equal(StageRefusal.AlreadyDone, stager.Offer(ctx, state, new[] { S("Ignis") }, S("Ignis"), this.codex).Refusal);
    }

    [Fact]
    public void Alembic_DistillsOnlyWhenHeated()
    {
        var stager = new Stager(this.pipeline);
        var tray = new[] { S("Aqua") };
        Assert.Equal(StageAction.Start, stager.Offer(Ctx(App("Alembic"), "Heated"), new MachineData(), tray, S("Aqua"), this.codex).Action);
        Assert.Equal(StageAction.Refuse, stager.Offer(Ctx(App("Alembic"), "Cold"), new MachineData(), tray, S("Aqua"), this.codex).Action);
        Assert.Equal(StageAction.Start, stager.Offer(Ctx(App("Alembic"), "Cold"), new MachineData(), new[] { S("Aqua") }, S("Terra"), this.codex).Action); // Limus
    }

    [Fact]
    public void Decomposition_SubstanceWaitsForSeal_WhenALongerRecipeCouldFollow()
    {
        var stager = new Stager(this.pipeline);
        var ctx = Ctx(App("Athanor"), "Open");
        // Pulvis can start Vitrum (with Calx), so a hand load waits for a second item or an empty-hand seal.
        Assert.Equal(StageAction.AwaitSeal, stager.Offer(ctx, new MachineData(), Array.Empty<ItemStack>(), S("Pulvis"), this.codex).Action);
        Assert.Equal(StageAction.Decompose, stager.Seal(ctx, new MachineData(), new[] { S("Pulvis") }, this.codex).Action);
        // Limus starts nothing on the Athanor, so it decomposes at once.
        Assert.Equal(StageAction.Decompose, stager.Offer(ctx, new MachineData(), Array.Empty<ItemStack>(), S("Limus"), this.codex).Action);
        // Caput Mortuum refuses decomposition (doc 15), so it only stages, waiting for Calx (Smaltum).
        Assert.False(stager.CanDecompose(App("Athanor"), S("CaputMortuum").Item));
        Assert.Equal(StageAction.Stage, stager.Offer(ctx, new MachineData(), Array.Empty<ItemStack>(), S("CaputMortuum"), this.codex).Action);
    }

    [Fact]
    public void Decomposition_RunPutsFirstPrimeInTheSlot()
    {
        var stager = new Stager(this.pipeline);
        ItemInfo tuna = this.catalog.Get("(O)130")!;
        RunPlan? run = stager.BuildDecomposeRun(App("Athanor"), new ItemStack(tuna), null);
        Assert.NotNull(run);
        Assert.Equal(Ids.DecomposeRecipe, run!.RecipeId);
        Assert.Equal("(O)" + P + "Aqua", run.OutputItemId);
        Assert.True(run.Count + run.Extras.Sum(e => e.Count) >= 2);
    }

    [Fact]
    public void Calcinator_NeedsFuel()
    {
        var stager = new Stager(this.pipeline);
        var ctx = Ctx(App("Calcinator"), "Open");
        Assert.Equal(StageRefusal.NoFuel, stager.Offer(ctx, new MachineData(), Array.Empty<ItemStack>(), S("Limus"), this.codex).Refusal);
        Assert.Equal(StageAction.Fuel, stager.Offer(ctx, new MachineData(), Array.Empty<ItemStack>(), new ItemStack(this.catalog.Get("(O)382")!), this.codex).Action);
        Assert.Equal(StageAction.Decompose, stager.Offer(ctx, new MachineData { Fuel = 1 }, Array.Empty<ItemStack>(), S("Limus"), this.codex).Action);
    }

    [Fact]
    public void Automated_LoadsOnlyTheRememberedDiscoveredRecipe()
    {
        var stager = new Stager(this.pipeline);
        var auto = Ctx(App("Retort"), "Open", LoadSource.Automate, memory: P + "Saxum");
        // Undiscovered: refused even with memory set.
        Assert.Equal(StageAction.Refuse, stager.Offer(auto, new MachineData(), Array.Empty<ItemStack>(), S("Terra"), this.codex).Action);
        this.codex.Record(CodexKeys.Recipe(P + "Saxum"), 1, 1);
        Assert.Equal(StageAction.Stage, stager.Offer(auto, new MachineData(), Array.Empty<ItemStack>(), S("Terra"), this.codex).Action);
        // Another recipe's input is refused (memory is Saxum).
        Assert.Equal(StageAction.Refuse, stager.Offer(auto, new MachineData(), Array.Empty<ItemStack>(), S("Sal"), this.codex).Action);
        // A fresh machine takes nothing (D4).
        var fresh = Ctx(App("Retort"), "Open", LoadSource.Automate);
        Assert.Equal(StageRefusal.NoRecipeMemory, stager.Offer(fresh, new MachineData(), Array.Empty<ItemStack>(), S("Terra"), this.codex).Refusal);
    }

    [Fact]
    public void Pinned_MachineRefusesAnotherRecipeByHand()
    {
        var stager = new Stager(this.pipeline);
        var ctx = Ctx(App("Retort"), "Open");
        var pinned = new MachineData { RecipeMemory = P + "Saxum", Pinned = true };
        Assert.Equal(StageRefusal.Pinned, stager.Offer(ctx, pinned, new[] { S("Sal") }, S("Sal"), this.codex).Refusal);
        Assert.Equal(StageAction.Start, stager.Offer(ctx, pinned, new[] { S("Terra") }, S("Terra"), this.codex).Action);
    }

    [Fact]
    public void FlowerBonus_RaisesQualityOneStep()
    {
        var stager = new Stager(this.pipeline);
        var flowers = NeighbourFact.Of(1, 1, "flower_item");
        var ctx = Ctx(App("Retort"), "Open", neighbours: flowers);
        StagePlan plan = stager.Offer(ctx, new MachineData(), new[] { S("Terra") }, S("Terra"), this.codex);
        RunPlan run = stager.BuildRecipeRun(ctx, plan.Match!, new[] { S("Terra"), S("Terra") });
        Assert.Equal(1, run.Quality);
        Assert.Equal(P + "Flowers", run.Bonus);
        Assert.True(run.Locked); // a first, undiscovered make by hand is held back
    }

    [Fact]
    public void Table_SucceedsOnTableOnlyRecipes()
    {
        var table = App("ExperimentTable");
        var ctx = Ctx(table, "Table");
        TableOutcome outcome = this.pipeline.Resolver.AttemptAtTable(ctx, new[] { S("Aether"), S("Aether") }, this.codex);
        Assert.Equal(TableOutcomeKind.Success, outcome.Kind);
        Assert.Equal(P + "QuintaEssentia", outcome.Recipe!.Id);
        Assert.Equal(TableOutcomeKind.Undefined, this.pipeline.Resolver.AttemptAtTable(ctx, new[] { S("Calx"), S("Saxum") }, this.codex).Kind);
    }

    [Fact]
    public void CodexGrid_HasEveryPairAndWarmsNeighbours()
    {
        var grid = new CodexGrid(this.pipeline.Registry);
        IReadOnlyList<GridCell> cells = grid.TierOne(this.codex);
        Assert.Equal(36, cells.Count);
        Assert.All(cells, c => Assert.NotNull(c.RecipeId));

        this.codex.Record(CodexKeys.Recipe(P + "Saxum"), 1, 1); // Terra + Terra
        cells = grid.TierOne(this.codex);
        GridCell saxum = cells.Single(c => c.RecipeId == P + "Saxum");
        Assert.Equal(CellState.Known, saxum.State);
        Assert.Contains(cells, c => c.State == CellState.Warm && Math.Abs(c.Row - saxum.Row) + Math.Abs(c.Column - saxum.Column) == 1);
        Assert.Contains(grid.NewMilestones(this.codex), m => m.Id == P + "FirstTier1");
    }
}
