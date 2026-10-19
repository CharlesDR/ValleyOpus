using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Conditions;

/// <summary>
/// Picks an apparatus's neighbour state from what's adjacent. States are mutually exclusive by construction (the
/// validator checks it), so at most one matches; arrangement, not charge, decides it (doc 13 §1a).
/// </summary>
public static class NeighbourStates
{
    /// <param name="apparatus">The apparatus row.</param>
    /// <param name="adjacentPower">Power kinds of adjacent sources, charged or not.</param>
    /// <param name="heatAdjacent">Whether a heat source or heat tile is adjacent.</param>
    public static string? Resolve(ApparatusRow apparatus, IReadOnlySet<string> adjacentPower, bool heatAdjacent)
    {
        foreach (NeighbourStateRow state in apparatus.NeighbourStates)
        {
            if (Matches(state.Test, adjacentPower, heatAdjacent))
                return state.Id;
        }
        return null;
    }

    public static bool Matches(NeighbourStateTest test, IReadOnlySet<string> adjacentPower, bool heatAdjacent)
    {
        if (test.None && adjacentPower.Count > 0)
            return false;
        if (test.Power.Count > 0 && !(adjacentPower.Count == test.Power.Count && test.Power.All(adjacentPower.Contains)))
            return false;
        if (test.Heat is bool heat && heat != heatAdjacent)
            return false;
        return true;
    }

    /// <summary>
    /// Pairs of states that can both match some arrangement, found by trying every combination of the power kinds
    /// the states mention, with and without heat.
    /// </summary>
    public static IEnumerable<(string A, string B)> Overlaps(ApparatusRow apparatus)
    {
        List<string> kinds = apparatus.NeighbourStates.SelectMany(s => s.Test.Power).Distinct().ToList();
        if (kinds.Count > 12)
            kinds = kinds.Take(12).ToList(); // 4096 combinations is plenty for any real apparatus

        var seen = new HashSet<(string, string)>();
        for (int mask = 0; mask < 1 << kinds.Count; mask++)
        {
            var power = new HashSet<string>(kinds.Where((_, i) => (mask & (1 << i)) != 0));
            foreach (bool heat in new[] { false, true })
            {
                List<string> matching = apparatus.NeighbourStates.Where(s => Matches(s.Test, power, heat)).Select(s => s.Id).ToList();
                for (int i = 0; i < matching.Count; i++)
                    for (int j = i + 1; j < matching.Count; j++)
                        if (seen.Add((matching[i], matching[j])))
                            yield return (matching[i], matching[j]);
            }
        }
    }
}
