using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Resolver;

/// <summary>Who is putting an item into a machine. Hand and the Satchel picker are manual; chaining and Automate are automated.</summary>
public enum LoadSource { Hand, Picker, Chaining, Automate }

/// <summary>What the farm's codex says about a recipe (implemented by <see cref="Codex.FarmCodex"/>).</summary>
public interface IRecipeAvailability
{
    bool IsDiscovered(string recipeId);

    /// <summary>Disabled on this farm: an unpicked secret candidate (doc 12 §11).</summary>
    bool IsDisabledOnFarm(string recipeId);
}

/// <summary>The machine a staging or match call is about.</summary>
/// <param name="Apparatus">The apparatus row.</param>
/// <param name="Snapshot">The condition snapshot, including the neighbour state.</param>
/// <param name="Source">Who is loading.</param>
/// <param name="RecipeMemory">The last recipe set by hand; automated loaders only ever load it (doc 13 §6a).</param>
/// <param name="Capacity">The tray capacity if it differs from the row (the Table's third slot, doc 12 R2).</param>
public sealed record MachineContext(ApparatusRow Apparatus, ConditionSnapshot Snapshot, LoadSource Source, string? RecipeMemory = null, int? Capacity = null)
{
    public bool Automated => this.Source is LoadSource.Chaining or LoadSource.Automate;
    public string? State => this.Snapshot.NeighbourState;
    public int TrayCapacity => this.Capacity ?? this.Apparatus.TrayCapacity;
}

/// <summary>Why a machine refused an item; each maps to one reason line in the UI (doc 16 §2).</summary>
public enum StageRefusal
{
    None,
    TrayFull,
    NoNeighbourState,
    NoRecipe,
    HandMadeOnly,
    NotDiscovered,
    NotAutomatable,
    NoRecipeMemory,
    WaitingForConditions,
}

public sealed record StageDecision(StageRefusal Refusal, IReadOnlyList<string> FailedConditions)
{
    public static readonly StageDecision Accept = new(StageRefusal.None, Array.Empty<string>());

    public bool Accepted => this.Refusal == StageRefusal.None;

    public static StageDecision Refuse(StageRefusal why, IReadOnlyList<string>? failed = null) => new(why, failed ?? Array.Empty<string>());
}

public enum MatchKind { None, Matched, Ambiguous }

/// <summary>The exact match for a full tray; <see cref="Locked"/> when a manual first attempt runs an undiscovered recipe.</summary>
public sealed record MatchResult(MatchKind Kind, CompiledRecipe? Recipe, bool Locked, ConditionOutcome? Conditions)
{
    public static readonly MatchResult NoMatch = new(MatchKind.None, null, false, null);
}

public enum TableOutcomeKind
{
    /// <summary>A defined, Table-attemptable recipe whose Required conditions hold.</summary>
    Success,

    /// <summary>A defined recipe that isn't Table-attemptable here, or whose conditions fail: nothing is consumed.</summary>
    Blocked,

    /// <summary>No defined recipe: residue, a little XP and a <c>tried:</c> key.</summary>
    Undefined,

    /// <summary>More items than the Table holds.</summary>
    TooManyInputs,
}

public sealed record TableOutcome(TableOutcomeKind Kind, CompiledRecipe? Recipe, ConditionOutcome? Conditions, string TriedSignature);

/// <summary>
/// The one resolver (doc 09 §5a, doc 12 §3, doc 13 §6): every loader asks it whether an item extends a tray, and
/// every machine asks it for the single recipe a full tray makes. It never ranks candidates.
/// </summary>
public sealed class RecipeResolver
{
    private readonly RecipeIndex index;
    private readonly ConditionEvaluator conditions;

    public RecipeResolver(ContentRegistry registry, RecipeIndex index)
    {
        this.Registry = registry;
        this.index = index;
        this.conditions = new ConditionEvaluator(registry);
    }

    public ContentRegistry Registry { get; }

    public RecipeIndex Index => this.index;

    public ConditionEvaluator Conditions => this.conditions;

    /// <summary>Can this item go into this machine's tray now? (doc 13 §6.1)</summary>
    public StageDecision CanExtend(MachineContext context, IReadOnlyList<ItemStack> tray, ItemStack item, IRecipeAvailability codex)
    {
        ApparatusRow apparatus = context.Apparatus;
        if (tray.Sum(t => t.Count) + item.Count > context.TrayCapacity)
            return StageDecision.Refuse(StageRefusal.TrayFull);

        if (context.Automated)
        {
            if (apparatus.Automation == AutomationMode.None || (context.Source == LoadSource.Chaining && !apparatus.Chaining))
                return StageDecision.Refuse(StageRefusal.NotAutomatable);
            if (context.RecipeMemory is null)
                return StageDecision.Refuse(StageRefusal.NoRecipeMemory); // a fresh machine waits for a hand set (doc 13 §6a)
        }

        // The Table takes any combination by hand; undefined ones give residue (doc 12 §3).
        if (apparatus.Kind == ApparatusKinds.Table && !context.Automated)
            return StageDecision.Accept;

        if (context.State is null)
            return StageDecision.Refuse(StageRefusal.NoNeighbourState);

        var withItem = tray.Append(item).ToList();
        List<CompiledRecipe> fits = this.index.Using(item.Item)
            .Where(r => r.RunsOn(apparatus, context.State) && !codex.IsDisabledOnFarm(r.Id))
            .Where(r => InputMatcher.CanStillComplete(r.Specs, withItem))
            .ToList();
        if (fits.Count == 0)
            return StageDecision.Refuse(StageRefusal.NoRecipe);

        if (context.Automated)
        {
            fits = fits.Where(r => r.Id == context.RecipeMemory).ToList();
            if (fits.Count == 0)
                return StageDecision.Refuse(StageRefusal.NoRecipe);
            fits = fits.Where(r => !r.Row.HandMadeOnly).ToList();
            if (fits.Count == 0)
                return StageDecision.Refuse(StageRefusal.HandMadeOnly);
            fits = fits.Where(r => codex.IsDiscovered(r.Id)).ToList();
            if (fits.Count == 0)
                return StageDecision.Refuse(StageRefusal.NotDiscovered);
        }

        // Auto-trigger machines check Required conditions when loading, so a gated machine waits idle (doc 12 §4);
        // hand-triggered rituals check them at trigger time instead.
        if (apparatus.Trigger == TriggerMode.Auto)
        {
            IReadOnlyList<string>? failed = null;
            fits = fits.Where(r =>
            {
                ConditionOutcome outcome = this.conditions.Check(r.Row, context.Snapshot);
                failed ??= outcome.Runnable ? null : outcome.FailedRequired;
                return outcome.Runnable;
            }).ToList();
            if (fits.Count == 0)
                return StageDecision.Refuse(StageRefusal.WaitingForConditions, failed);
        }

        return StageDecision.Accept;
    }

    /// <summary>The single recipe a full tray makes on this machine, if any.</summary>
    public MatchResult Match(MachineContext context, IReadOnlyList<ItemStack> tray, IRecipeAvailability codex)
    {
        if (tray.Count == 0 || context.State is null)
            return MatchResult.NoMatch;

        var survivors = new List<(CompiledRecipe Recipe, ConditionOutcome Outcome)>();
        foreach (CompiledRecipe recipe in this.index.Using(tray[0].Item))
        {
            if (!recipe.RunsOn(context.Apparatus, context.State) || codex.IsDisabledOnFarm(recipe.Id))
                continue;
            if (!InputMatcher.MatchesExactly(recipe.Specs, tray))
                continue;
            if (context.Automated && (recipe.Row.HandMadeOnly || !codex.IsDiscovered(recipe.Id) || recipe.Id != context.RecipeMemory))
                continue;
            ConditionOutcome outcome = this.conditions.Check(recipe.Row, context.Snapshot);
            if (!outcome.Runnable)
                continue;
            survivors.Add((recipe, outcome));
        }

        if (survivors.Count == 0)
            return MatchResult.NoMatch;
        if (survivors.Count > 1)
            return new MatchResult(MatchKind.Ambiguous, null, false, null); // the validator should make this impossible

        (CompiledRecipe match, ConditionOutcome conditions) = survivors[0];
        return new MatchResult(MatchKind.Matched, match, !codex.IsDiscovered(match.Id), conditions);
    }

    /// <summary>An attempt at the Experiment Table, with doc 07's three outcomes.</summary>
    public TableOutcome AttemptAtTable(MachineContext context, IReadOnlyList<ItemStack> tray, IRecipeAvailability codex)
    {
        string signature = Codex.CodexKeys.TriedSignature(tray.SelectMany(t => Enumerable.Repeat(t.Item.QualifiedId, t.Count)));
        if (tray.Sum(t => t.Count) > context.TrayCapacity)
            return new TableOutcome(TableOutcomeKind.TooManyInputs, null, null, signature);
        if (tray.Count == 0)
            return new TableOutcome(TableOutcomeKind.Undefined, null, null, signature);

        List<CompiledRecipe> defined = this.index.Using(tray[0].Item)
            .Where(r => !codex.IsDisabledOnFarm(r.Id) && InputMatcher.MatchesExactly(r.Specs, tray))
            .ToList();

        List<CompiledRecipe> atTable = defined
            .Where(r => r.Row.Discovery.TableAttemptable && r.RunsOn(context.Apparatus, context.State))
            .ToList();
        if (atTable.Count == 1)
        {
            ConditionOutcome outcome = this.conditions.Check(atTable[0].Row, context.Snapshot);
            return new TableOutcome(outcome.Runnable ? TableOutcomeKind.Success : TableOutcomeKind.Blocked, atTable[0], outcome, signature);
        }

        if (defined.Count > 0)
            return new TableOutcome(TableOutcomeKind.Blocked, defined[0], null, signature);

        return new TableOutcome(TableOutcomeKind.Undefined, null, null, signature);
    }
}
