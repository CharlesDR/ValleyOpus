using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Codex;

/// <summary>What a codex cell shows (doc 07, doc 16 §6).</summary>
public enum CellState
{
    /// <summary>Not made yet: a silhouette, or "???".</summary>
    Unknown,

    /// <summary>Not made yet, but a neighbouring cell is: the warm-cell hint (doc 07 "Grid").</summary>
    Warm,

    /// <summary>This pairing was tried with no result.</summary>
    Tried,

    /// <summary>Discovered.</summary>
    Known,
}

/// <summary>One cell of the Tier 1 triangle: the Primes on its row and column and the recipe there, if any.</summary>
public sealed record GridCell(int Row, int Column, string PrimeA, string PrimeB, string? RecipeId, string? OutputId, CellState State);

/// <summary>
/// The codex's derived layouts (doc 07 data shape: entries derive from Primes, Substances and Recipes, so packs get
/// pages without extra rows). The Tier 1 grid is the 8 × 8 triangle of Prime pairs.
/// </summary>
public sealed class CodexGrid
{
    private readonly ContentRegistry registry;

    public CodexGrid(ContentRegistry registry) => this.registry = registry;

    public IReadOnlyList<PrimeRow> Primes => this.registry.Primes.All.Where(p => p.ItemId != null).OrderBy(p => p.SortOrder).ToList();

    /// <summary>The Tier 1 triangle, row by row (row ≤ column).</summary>
    public IReadOnlyList<GridCell> TierOne(FarmCodex codex)
    {
        IReadOnlyList<PrimeRow> primes = this.Primes;
        var cells = new List<GridCell>();
        for (int r = 0; r < primes.Count; r++)
            for (int c = r; c < primes.Count; c++)
            {
                RecipeRow? recipe = this.PairRecipe(primes[r], primes[c]);
                cells.Add(new GridCell(r, c, primes[r].Id, primes[c].Id, recipe?.Id, recipe?.Output.Id, CellState.Unknown));
            }

        var known = new HashSet<(int, int)>(cells.Where(c => this.IsKnown(c, codex)).Select(c => (c.Row, c.Column)));
        return cells.Select(c =>
        {
            if (known.Contains((c.Row, c.Column)))
                return c with { State = CellState.Known };
            if (this.WasTried(c, codex))
                return c with { State = CellState.Tried };
            bool warm = known.Contains((c.Row - 1, c.Column)) || known.Contains((c.Row + 1, c.Column))
                || known.Contains((c.Row, c.Column - 1)) || known.Contains((c.Row, c.Column + 1));
            return c with { State = warm ? CellState.Warm : CellState.Unknown };
        }).ToList();
    }

    /// <summary>Tier 2+ entries: every Item-class substance above Tier 1, by tier, then sort order of the output's recipe.</summary>
    public IReadOnlyList<SubstanceRow> Tier(int tier)
        => this.registry.Substances.All.Where(s => s.Tier == tier && s.ItemId != null).ToList();

    /// <summary>The recipe on a Prime pair cell: the one whose two inputs are exactly these Primes.</summary>
    public RecipeRow? PairRecipe(PrimeRow a, PrimeRow b)
    {
        string[] want = new[] { a.Id, b.Id }.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        return this.registry.Recipes.All
            .Where(r => r.InputCount == 2 && r.Inputs.All(i => i.Item != null))
            .Where(r => this.registry.Substances.TryGet(r.Output.Id, out SubstanceRow s) && s.Class != SubstanceClass.ProcessState)
            .FirstOrDefault(r => r.Inputs.SelectMany(i => Enumerable.Repeat(i.Item!, i.Count)).OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(want));
    }

    private bool IsKnown(GridCell cell, FarmCodex codex)
        => cell.RecipeId != null && (codex.IsDiscovered(cell.RecipeId) || (this.registry.ResolveItemId(cell.OutputId!) is string item && codex.Has(CodexKeys.Item(item))));

    private bool WasTried(GridCell cell, FarmCodex codex)
    {
        string? a = this.registry.ResolveItemId(cell.PrimeA), b = this.registry.ResolveItemId(cell.PrimeB);
        return a != null && b != null && codex.Has(CodexKeys.Tried(new[] { a, b }));
    }

    /// <summary>
    /// The milestones a discovery completes that weren't recorded yet. Triggers: <c>FirstOfTier:n</c>,
    /// <c>TierComplete:n</c>, <c>RowComplete:&lt;Prime&gt;</c> and <c>Process:&lt;substance&gt;</c>.
    /// </summary>
    public IReadOnlyList<MilestoneRow> NewMilestones(FarmCodex codex)
    {
        var reached = new List<MilestoneRow>();
        List<GridCell> grid = this.TierOne(codex).ToList();
        foreach (MilestoneRow milestone in this.registry.Milestones.All)
        {
            if (codex.Has(CodexKeys.Milestone(milestone.Id)))
                continue;
            string[] parts = milestone.Trigger.Split(':', 2);
            string arg = parts.Length > 1 ? parts[1] : "";
            bool met = parts[0] switch
            {
                "FirstOfTier" => int.TryParse(arg, out int t1) && this.KnownOfTier(codex, t1) >= 1,
                "TierComplete" => int.TryParse(arg, out int t2) && this.TierRecipes(t2).Any() && this.TierRecipes(t2).All(codex.IsDiscovered),
                "RowComplete" => grid.Where(c => (c.PrimeA == arg || c.PrimeB == arg) && c.RecipeId != null).ToList() is { Count: > 0 } row && row.All(c => c.State == CellState.Known),
                "Process" => this.registry.Recipes.All.Where(r => r.Output.Id == arg).Any(r => codex.IsDiscovered(r.Id)),
                _ => false,
            };
            if (met)
                reached.Add(milestone);
        }
        return reached;
    }

    private IEnumerable<string> TierRecipes(int tier)
        => this.registry.Recipes.All
            .Where(r => this.registry.Substances.TryGet(r.Output.Id, out SubstanceRow s) && s.Tier == tier && s.Class == SubstanceClass.Item)
            .Select(r => r.Id);

    private int KnownOfTier(FarmCodex codex, int tier) => this.TierRecipes(tier).Count(codex.IsDiscovered);
}
