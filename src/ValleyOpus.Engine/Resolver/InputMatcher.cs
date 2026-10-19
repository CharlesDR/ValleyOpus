using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Resolver;

/// <summary>An ingredient spec with its item ID resolved (a Prime or substance ID becomes its item).</summary>
public sealed record CompiledSpec(IngredientSpec Spec, string? ItemId)
{
    public bool Matches(ItemInfo item, int quality)
    {
        if (quality < this.Spec.MinQuality)
            return false;
        if (this.Spec.Item != null)
            return this.ItemId != null && item.QualifiedId.Equals(this.ItemId, StringComparison.OrdinalIgnoreCase);
        if (this.Spec.Tag != null)
            return item.HasTag(this.Spec.Tag);
        if (this.Spec.Category is int category)
            return item.Category == category;
        return false;
    }
}

/// <summary>
/// Matches loaded items to a recipe's inputs as a multiset: order never matters, each unit of an item fills one unit
/// of a spec, and every unit must be used (doc 12 §3). Solved as a bipartite matching, since a tag spec and an
/// exact-item spec can compete for the same item.
/// </summary>
public static class InputMatcher
{
    /// <summary>Whether the items fill every input slot exactly, with nothing left over.</summary>
    public static bool MatchesExactly(IReadOnlyList<CompiledSpec> specs, IReadOnlyList<ItemStack> items)
    {
        int slots = specs.Sum(s => s.Spec.Count);
        int units = items.Sum(i => i.Count);
        return slots == units && MatchAll(specs, items);
    }

    /// <summary>Whether every item fits some distinct slot, so loading more could complete the recipe.</summary>
    public static bool CanStillComplete(IReadOnlyList<CompiledSpec> specs, IReadOnlyList<ItemStack> items)
    {
        int slots = specs.Sum(s => s.Spec.Count);
        int units = items.Sum(i => i.Count);
        return units <= slots && MatchAll(specs, items);
    }

    private static bool MatchAll(IReadOnlyList<CompiledSpec> specs, IReadOnlyList<ItemStack> items)
    {
        var slots = new List<CompiledSpec>();
        foreach (CompiledSpec spec in specs)
            for (int i = 0; i < spec.Spec.Count; i++)
                slots.Add(spec);

        var units = new List<ItemStack>();
        foreach (ItemStack stack in items)
            for (int i = 0; i < stack.Count; i++)
                units.Add(stack);

        int[] slotOwner = Enumerable.Repeat(-1, slots.Count).ToArray();
        for (int u = 0; u < units.Count; u++)
        {
            if (!TryAssign(u, units, slots, slotOwner, new bool[slots.Count]))
                return false;
        }
        return true;
    }

    private static bool TryAssign(int unit, List<ItemStack> units, List<CompiledSpec> slots, int[] slotOwner, bool[] visited)
    {
        for (int s = 0; s < slots.Count; s++)
        {
            if (visited[s] || !slots[s].Matches(units[unit].Item, units[unit].Quality))
                continue;
            visited[s] = true;
            if (slotOwner[s] < 0 || TryAssign(slotOwner[s], units, slots, slotOwner, visited))
            {
                slotOwner[s] = unit;
                return true;
            }
        }
        return false;
    }
}
