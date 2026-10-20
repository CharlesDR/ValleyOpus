using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Randomness;
using ValleyOpus.Engine.Registry;
using ValleyOpus.Engine.Resolver;

namespace ValleyOpus.Engine.Codex;

/// <summary>When a key was set and by whom, for the shared-codex notice (doc 12 §10).</summary>
public sealed class CodexEntry
{
    public int Day { get; set; }
    public long Player { get; set; }
}

/// <summary>The farm codex as saved in the host envelope (doc 17 §1). Plain data, so it round-trips as JSON.</summary>
public sealed class CodexData
{
    public Dictionary<string, CodexEntry> Keys { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Counts { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, List<string>> SecretPicks { get; set; } = new(StringComparer.Ordinal);
    public int RerollCount { get; set; }
    public int TableCapacity { get; set; } = 2;
}

/// <summary>
/// The farm's discovery state and the per-farm secret pick (doc 07, doc 12 §10–11). The host owns the writes;
/// a farmhand's copy is a mirror filled from snapshots and deltas.
/// </summary>
public sealed class FarmCodex : IRecipeAvailability
{
    private readonly HashSet<string> disabledSecrets = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> requiresUnlock = new(StringComparer.Ordinal);

    /// <summary>Whether the farm holds an unlock; null treats every unlock as held (tests, tools).</summary>
    public Func<string, bool>? IsUnlocked { get; set; }

    public FarmCodex(CodexData? data = null) => this.Data = data ?? new CodexData();

    public CodexData Data { get; }

    public bool Has(string key) => this.Data.Keys.ContainsKey(key);

    /// <summary>Records a key; returns false if it was already set (the first recorded keeps attribution, doc 07).</summary>
    public bool Record(string key, int day, long player)
    {
        if (this.Data.Keys.ContainsKey(key))
            return false;
        this.Data.Keys[key] = new CodexEntry { Day = day, Player = player };
        return true;
    }

    /// <summary>Counts a craft for codex depth lines; returns the new count.</summary>
    public int CountCraft(string qualifiedItemId, bool handMade)
    {
        string key = CodexKeys.Count(qualifiedItemId);
        int count = this.Data.Counts[key] = this.Data.Counts.GetValueOrDefault(key) + 1;
        if (handMade)
        {
            string handKey = CodexKeys.HandCount(qualifiedItemId);
            this.Data.Counts[handKey] = this.Data.Counts.GetValueOrDefault(handKey) + 1;
        }
        return count;
    }

    public bool IsDiscovered(string recipeId) => this.Has(CodexKeys.Recipe(recipeId));

    /// <summary>An unpicked secret candidate, or a recipe whose story unlock the farm doesn't hold yet (doc 14 §1).</summary>
    public bool IsDisabledOnFarm(string recipeId)
        => this.disabledSecrets.Contains(recipeId)
            || (this.requiresUnlock.TryGetValue(recipeId, out string? unlock) && this.IsUnlocked != null && !this.IsUnlocked(unlock));

    /// <summary>
    /// Picks each pool's secret recipes for this farm if not picked yet, and disables the unpicked candidates.
    /// Call after every registry load.
    /// </summary>
    public void ApplySecretPicks(ContentRegistry registry, string gameId)
    {
        this.disabledSecrets.Clear();
        this.requiresUnlock.Clear();
        foreach (RecipeRow recipe in registry.Recipes.All.Where(r => !string.IsNullOrEmpty(r.RequiresUnlock)))
            this.requiresUnlock[recipe.Id] = recipe.RequiresUnlock!;
        foreach (SecretPoolRow pool in registry.SecretPools.All)
        {
            List<string> candidates = Candidates(registry, pool.Id);
            if (!this.Data.SecretPicks.TryGetValue(pool.Id, out List<string>? picks) || picks.Count == 0)
                this.Data.SecretPicks[pool.Id] = picks = SecretPicker.Pick(candidates, pool.PickCount, gameId, this.Data.RerollCount, pool.Id, exclude: null);
            foreach (string candidate in candidates.Where(c => !picks.Contains(c)))
                this.disabledSecrets.Add(candidate);
        }
    }

    /// <summary>
    /// The re-roll ritual: increments the count and re-picks every pool. A discovered secret is replaced, and its
    /// codex entry stays as lore (Charles, 2026-10-18).
    /// </summary>
    public void Reroll(ContentRegistry registry, string gameId)
    {
        this.Data.RerollCount++;
        foreach (SecretPoolRow pool in registry.SecretPools.All)
        {
            List<string> candidates = Candidates(registry, pool.Id);
            List<string> previous = this.Data.SecretPicks.GetValueOrDefault(pool.Id) ?? new();
            this.Data.SecretPicks[pool.Id] = SecretPicker.Pick(candidates, pool.PickCount, gameId, this.Data.RerollCount, pool.Id, exclude: previous);
        }
        this.ApplySecretPicks(registry, gameId);
    }

    private static List<string> Candidates(ContentRegistry registry, string poolId)
        => registry.Recipes.All.Where(r => r.SecretPool == poolId).Select(r => r.Id).ToList();
}

/// <summary>The deterministic secret pick: <c>hash(game ID, re-roll count)</c> over each candidate (doc 12 §11).</summary>
public static class SecretPicker
{
    public static List<string> Pick(IReadOnlyList<string> candidates, int count, string gameId, int rerollCount, string poolId, IReadOnlyCollection<string>? exclude)
    {
        IEnumerable<string> pool = candidates;
        if (exclude is { Count: > 0 } && candidates.Count(c => !exclude.Contains(c)) >= count)
            pool = candidates.Where(c => !exclude.Contains(c));
        return pool
            .OrderBy(c => StableHash.Of(gameId, rerollCount.ToString(), poolId, c))
            .ThenBy(c => c, StringComparer.Ordinal)
            .Take(Math.Max(0, count))
            .ToList();
    }
}
