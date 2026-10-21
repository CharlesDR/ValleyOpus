using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Decomposition;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Quality;
using ValleyOpus.Engine.Randomness;
using ValleyOpus.Engine.Registry;
using ValleyOpus.Engine.Resolver;

namespace ValleyOpus.Engine.Machines;

public enum StageAction
{
    /// <summary>The item stays with the loader; <see cref="StagePlan.Refusal"/> says why.</summary>
    Refuse,

    /// <summary>The item goes into the tray; the tray isn't a full recipe yet.</summary>
    Stage,

    /// <summary>The item goes into the tray and could be decomposed alone or start a longer recipe: an empty-hand click seals it.</summary>
    AwaitSeal,

    /// <summary>The tray plus this item exactly match a recipe: consume the tray and start it.</summary>
    Start,

    /// <summary>Decompose this one item now.</summary>
    Decompose,

    /// <summary>The item tops up the fuel slot.</summary>
    Fuel,
}

/// <summary>What the staging layer does with one offered item (doc 13 §6).</summary>
public sealed record StagePlan(StageAction Action, StageRefusal Refusal, MatchResult? Match = null, IReadOnlyList<string>? FailedConditions = null)
{
    public bool Accepted => this.Action != StageAction.Refuse;

    public static StagePlan Refuse(StageRefusal why, IReadOnlyList<string>? failed = null) => new(StageAction.Refuse, why, null, failed);
}

/// <summary>One run's outputs, ready for the adapter to put in the machine.</summary>
/// <param name="RecipeId">The recipe, or <see cref="Ids.DecomposeRecipe"/>.</param>
/// <param name="OutputItemId">The held output's qualified item ID; null for a process state (kindling).</param>
/// <param name="ProcessState">The process-state substance a run sets instead of making an item.</param>
/// <param name="Extras">Further outputs (more Primes, residue), handed over together on a hand collection.</param>
/// <param name="Bonus">The first met Bonus condition, for the <c>effect:</c> key.</param>
public sealed record RunPlan(
    string RecipeId,
    string? OutputItemId,
    int Count,
    int Quality,
    int Minutes,
    IReadOnlyList<(string ItemId, int Count)> Extras,
    bool Locked,
    string? ProcessState,
    string? Bonus,
    int Tier);

/// <summary>
/// The staging layer's rules, free of the game (doc 13 §6, doc 09 §5a): every loader (hand, picker, chaining,
/// Automate) offers items one at a time; this decides whether each is staged, starts a recipe, starts a
/// decomposition, waits for an empty-hand seal or is refused, and builds the run.
/// </summary>
public sealed class Stager
{
    /// <summary>Placeholder minutes when a recipe sets none: Tier 1 two hours, Tier 2 four (doc 11).</summary>
    public static int DefaultMinutes(int tier) => tier <= 1 ? 120 : 240;

    public const int DefaultDecomposeMinutes = 120;

    private readonly ContentRegistry registry;
    private readonly RecipeResolver resolver;
    private readonly Decomposer decomposer;

    public Stager(ContentPipeline pipeline)
        : this(pipeline.Registry, pipeline.Resolver, pipeline.Decomposer) { }

    public Stager(ContentRegistry registry, RecipeResolver resolver, Decomposer decomposer)
    {
        this.registry = registry;
        this.resolver = resolver;
        this.decomposer = decomposer;
    }

    /// <summary>Whether this apparatus can decompose this item: a decomposing apparatus, not a Prime, and not refused.</summary>
    public bool CanDecompose(ApparatusRow apparatus, ItemInfo item)
        => apparatus.Decompose != null
            && this.registry.PrimeForItem(item.QualifiedId) is null
            && !this.decomposer.IsRefused(item)
            && !this.decomposer.ProfileOf(item).IsEmpty;

    public bool IsFuel(ApparatusRow apparatus, ItemInfo item)
        => apparatus.Fuel is { } fuel && Ids.QualifyItem(fuel.Item).Equals(item.QualifiedId, StringComparison.OrdinalIgnoreCase);

    /// <summary>Decides what to do with one offered item.</summary>
    public StagePlan Offer(MachineContext context, MachineData state, IReadOnlyList<ItemStack> tray, ItemStack item, IRecipeAvailability codex)
    {
        ApparatusRow apparatus = context.Apparatus;

        if (this.IsFuel(apparatus, item.Item))
            return state.Fuel + item.Count <= apparatus.Fuel!.Capacity
                ? new StagePlan(StageAction.Fuel, StageRefusal.None)
                : StagePlan.Refuse(StageRefusal.FuelFull);

        if (state.Recipe != null)
            return StagePlan.Refuse(StageRefusal.Busy);

        bool decomposable = this.CanDecompose(apparatus, item.Item) && item.Count == 1;

        if (context.Automated)
        {
            if (apparatus.Automation == AutomationMode.None || (context.Source == LoadSource.Chaining && !apparatus.Chaining))
                return StagePlan.Refuse(StageRefusal.NotAutomatable);
            if (context.RecipeMemory == Ids.DecomposeRecipe)
            {
                if (tray.Count > 0 || !decomposable)
                    return StagePlan.Refuse(StageRefusal.NotDecomposable);
                return this.WithFuel(apparatus, state, new StagePlan(StageAction.Decompose, StageRefusal.None));
            }
        }

        StageDecision decision = this.resolver.CanExtend(context, tray, item, codex);

        if (!context.Automated && tray.Count == 0 && decomposable)
            return decision.Accepted
                ? new StagePlan(StageAction.AwaitSeal, StageRefusal.None)
                : this.WithFuel(apparatus, state, new StagePlan(StageAction.Decompose, StageRefusal.None));

        if (!decision.Accepted)
            return StagePlan.Refuse(decision.Refusal == StageRefusal.NoRecipe && apparatus.Decompose != null && tray.Count == 0 ? StageRefusal.NotDecomposable : decision.Refusal, decision.FailedConditions);

        var withItem = tray.Append(item).ToList();
        MatchResult match = this.resolver.Match(context, withItem, codex);
        if (match.Kind != MatchKind.Matched)
            return new StagePlan(StageAction.Stage, StageRefusal.None);

        return this.StartPlan(context, state, match);
    }

    /// <summary>An empty-hand click on a waiting tray: decompose a lone item, or start the tray's exact recipe.</summary>
    public StagePlan Seal(MachineContext context, MachineData state, IReadOnlyList<ItemStack> tray, IRecipeAvailability codex)
    {
        if (state.Recipe != null)
            return StagePlan.Refuse(StageRefusal.Busy);
        if (tray.Count == 0)
            return StagePlan.Refuse(StageRefusal.NoRecipe);

        MatchResult match = this.resolver.Match(context, tray, codex);
        if (match.Kind == MatchKind.Matched)
            return this.StartPlan(context, state, match);

        if (tray.Count == 1 && tray[0].Count == 1 && this.CanDecompose(context.Apparatus, tray[0].Item))
            return this.WithFuel(context.Apparatus, state, new StagePlan(StageAction.Decompose, StageRefusal.None));

        return StagePlan.Refuse(StageRefusal.NoRecipe);
    }

    private StagePlan StartPlan(MachineContext context, MachineData state, MatchResult match)
    {
        CompiledRecipe recipe = match.Recipe!;
        if (!context.Automated && state.Pinned && state.RecipeMemory != null && state.RecipeMemory != recipe.Id)
            return StagePlan.Refuse(StageRefusal.Pinned);
        if (this.ProcessStateOf(recipe.Row) != null && context.Apparatus.Heat?.RequiresFlag == "Kindled" && state.Kindled)
            return StagePlan.Refuse(StageRefusal.AlreadyDone);
        return this.WithFuel(context.Apparatus, state, new StagePlan(StageAction.Start, StageRefusal.None, match));
    }

    private StagePlan WithFuel(ApparatusRow apparatus, MachineData state, StagePlan plan)
        => apparatus.Fuel is { } fuel && state.Fuel < fuel.PerRun ? StagePlan.Refuse(StageRefusal.NoFuel) : plan;

    /// <summary>
    /// The non-item substance a recipe makes, if any: a process state, or a Power substance (Flamma Viva made in an
    /// Athanor kindles it, doc 13 §2; Vapor and Fulgur at the Table are reveals with a puff).
    /// </summary>
    public string? ProcessStateOf(RecipeRow recipe)
        => this.registry.Substances.TryGet(recipe.Output.Id, out SubstanceRow output) && output.Class is SubstanceClass.ProcessState or SubstanceClass.Power ? output.Id : null;

    /// <summary>The run a matched recipe makes.</summary>
    public RunPlan BuildRecipeRun(MachineContext context, MatchResult match, IReadOnlyList<ItemStack> tray)
    {
        CompiledRecipe recipe = match.Recipe!;
        RecipeRow row = recipe.Row;
        int tier = this.registry.Substances.TryGet(row.Output.Id, out SubstanceRow substance) ? substance.Tier : 1;
        ConditionOutcome conditions = match.Conditions ?? ConditionOutcome.AllMet;

        int quality = QualityCalculator.Compute(
            row.Quality,
            tray.SelectMany(t => Enumerable.Repeat(t.Quality, t.Count)),
            context.Apparatus,
            new[] { conditions.BonusQualitySteps });

        double speed = conditions.MetBonuses.Select(b => b.Effect?.SpeedMultiplier ?? 1).DefaultIfEmpty(1).Aggregate((a, b) => a * b);
        int minutes = Math.Max(10, (int)Math.Round((row.Minutes ?? DefaultMinutes(tier)) * speed));

        string? process = this.ProcessStateOf(row);
        return new RunPlan(
            RecipeId: recipe.Id,
            OutputItemId: process is null ? this.registry.ResolveItemId(row.Output.Id) : null,
            Count: Math.Max(1, row.Output.Count),
            Quality: quality,
            Minutes: minutes,
            Extras: Array.Empty<(string, int)>(),
            Locked: match.Locked && !context.Automated,
            ProcessState: process,
            Bonus: conditions.MetBonuses.FirstOrDefault()?.ConditionId,
            Tier: tier);
    }

    /// <summary>The run a decomposition makes: the first Prime is the held output, the rest and the residue are extras.</summary>
    public RunPlan? BuildDecomposeRun(ApparatusRow apparatus, ItemStack item, SeededRandom? residueRoll)
    {
        DecompositionResult result = this.decomposer.Decompose(item.Item, item.Quality, apparatus.Decompose, residueRoll);
        if (result.Refused || result.Total == 0)
            return null;

        var outputs = new List<(string ItemId, int Count)>();
        foreach ((string prime, int units) in result.Units)
        {
            string? itemId = this.registry.ResolveItemId(prime);
            if (itemId != null && units > 0)
                outputs.Add((itemId, units));
        }
        if (result.ResidueCount > 0 && this.registry.Settings.ResidueItem is string residue)
            outputs.Add((Ids.QualifyItem(residue), result.ResidueCount));
        if (outputs.Count == 0)
            return null;

        return new RunPlan(
            RecipeId: Ids.DecomposeRecipe,
            OutputItemId: outputs[0].ItemId,
            Count: outputs[0].Count,
            Quality: 0,
            Minutes: apparatus.Decompose?.Minutes ?? DefaultDecomposeMinutes,
            Extras: outputs.Skip(1).ToList(),
            Locked: false,
            ProcessState: null,
            Bonus: null,
            Tier: 0);
    }
}
