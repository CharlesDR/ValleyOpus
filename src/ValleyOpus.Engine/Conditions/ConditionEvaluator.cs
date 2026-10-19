using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Conditions;

/// <summary>The result of checking a recipe's conditions against a snapshot (doc 12 §4).</summary>
public sealed record ConditionOutcome(bool Runnable, bool Partial, IReadOnlyList<string> FailedRequired, IReadOnlyList<RecipeConditionRef> MetBonuses)
{
    public static readonly ConditionOutcome AllMet = new(true, false, Array.Empty<string>(), Array.Empty<RecipeConditionRef>());

    public int BonusQualitySteps => this.MetBonuses.Sum(b => b.Effect?.QualitySteps ?? 0);
}

/// <summary>Evaluates condition records from a <see cref="ConditionSnapshot"/> only (doc 09 §3 <c>Conditions</c>).</summary>
public sealed class ConditionEvaluator
{
    private readonly ContentRegistry registry;

    public ConditionEvaluator(ContentRegistry registry) => this.registry = registry;

    /// <summary>Whether one condition holds. An unknown condition never holds.</summary>
    public bool Holds(string conditionId, ConditionSnapshot snapshot)
        => this.registry.Conditions.TryGet(conditionId, out ConditionRow row) && Holds(row, snapshot);

    public static bool Holds(ConditionRow row, ConditionSnapshot snapshot)
    {
        switch (row.Kind)
        {
            case ConditionKind.Gsq:
                return row.Query != null && snapshot.Queries.TryGetValue(row.Query, out bool result) && result;

            case ConditionKind.Adjacency:
                if (row.Adjacency is not { } adj)
                    return false;
                bool cardinal = adj.Directions.Equals("Cardinal", StringComparison.OrdinalIgnoreCase);
                return snapshot.Neighbours.Any(n =>
                    (n.Dx != 0 || n.Dy != 0)
                    && Math.Abs(n.Dx) <= adj.Range && Math.Abs(n.Dy) <= adj.Range
                    && (!cardinal || n.Dx == 0 || n.Dy == 0)
                    && n.Tags.Contains(adj.Tag));

            case ConditionKind.Catalyst:
                if (row.Catalyst is not { } cat)
                    return false;
                if (cat.Shades > 0 && snapshot.Shades < cat.Shades)
                    return false;
                return cat.Item is null
                    || (snapshot.Catalysts.TryGetValue(Ids.QualifyItem(cat.Item), out int have) && have >= cat.Count);

            case ConditionKind.Power:
                return row.Power != null && snapshot.ChargedPower.Contains(row.Power);

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks a recipe's Required and Variant conditions (a Variant row requires its condition; its base row requires
    /// the negation, doc 12 §4) and collects met Bonus conditions. A failed soft condition makes the run partial
    /// instead of blocking it.
    /// </summary>
    public ConditionOutcome Check(RecipeRow recipe, ConditionSnapshot snapshot)
    {
        if (recipe.Conditions.Count == 0)
            return ConditionOutcome.AllMet;

        var failed = new List<string>();
        var bonuses = new List<RecipeConditionRef>();
        bool partial = false;
        foreach (RecipeConditionRef reference in recipe.Conditions)
        {
            bool holds = this.Holds(reference.ConditionId, snapshot);
            if (reference.Use == ConditionUse.Bonus)
            {
                if (holds)
                    bonuses.Add(reference);
                continue;
            }

            if (holds != reference.Negate)
                continue;
            if (reference.Soft)
                partial = true;
            else
                failed.Add(reference.ConditionId);
        }
        return new ConditionOutcome(failed.Count == 0, partial, failed, bonuses);
    }
}
