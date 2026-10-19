using System.Globalization;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Randomness;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Decomposition;

/// <summary>An item's Prime profile: weights that sum to 1, ordered by weight, then Prime sort order.</summary>
public sealed record PrimeProfile(IReadOnlyList<(string Prime, double Weight)> Weights, string Source)
{
    public static readonly PrimeProfile EmptyProfile = new(Array.Empty<(string, double)>(), "none");

    public bool IsEmpty => this.Weights.Count == 0;
}

/// <summary>What decomposing one item gives.</summary>
/// <param name="Refused">The item can't be decomposed (an override's <c>Refuse</c>, Caput Mortuum, hand-made-only items, capstones).</param>
/// <param name="Units">Prime units by Prime ID, in profile order.</param>
/// <param name="Clamped">Equivalent exchange reduced the units (the validator and the sweep list every clamp).</param>
/// <param name="ResidueCount">Caput Mortuum produced by the residue roll.</param>
public sealed record DecompositionResult(bool Refused, IReadOnlyList<(string Prime, int Units)> Units, bool Clamped, int UnclampedTotal, int ResidueCount, PrimeProfile Profile)
{
    public int Total => this.Units.Sum(u => u.Units);

    public static DecompositionResult Refuse(PrimeProfile profile) => new(true, Array.Empty<(string, int)>(), false, 0, 0, profile);
}

/// <summary>
/// Decomposition is one built-in rule over four data tables, not a recipe per item (doc 03 "The computation",
/// doc 12 §2). Profiles are cached per item ID; build a new decomposer after a data reload.
/// </summary>
public sealed class Decomposer
{
    private readonly ContentRegistry registry;
    private readonly IItemCatalog catalog;
    private readonly Dictionary<string, PrimeProfile> profileCache = new(StringComparer.OrdinalIgnoreCase);

    public Decomposer(ContentRegistry registry, IItemCatalog catalog)
    {
        this.registry = registry;
        this.catalog = catalog;
    }

    /// <summary>The item's profile before any apparatus concentration.</summary>
    public PrimeProfile ProfileOf(ItemInfo item)
    {
        if (this.profileCache.TryGetValue(item.QualifiedId, out PrimeProfile? cached))
            return cached;
        PrimeProfile profile = this.ComputeProfile(item);
        this.profileCache[item.QualifiedId] = profile;
        return profile;
    }

    public bool IsRefused(ItemInfo item)
        => this.registry.Overrides.TryGet(item.QualifiedId, out DecompositionOverrideRow o) && o.Refuse;

    /// <param name="item">The item.</param>
    /// <param name="quality">Its vanilla quality, which raises yield only (Primes carry no quality).</param>
    /// <param name="modifiers">The decomposing apparatus's route modifiers, or null for none.</param>
    /// <param name="residueRoll">The deterministic roll for this decomposition (day, location, tile), or null to skip residue.</param>
    public DecompositionResult Decompose(ItemInfo item, int quality, DecomposeModifiers? modifiers, SeededRandom? residueRoll = null)
    {
        DecompositionSettingsRow settings = this.registry.Settings;
        this.registry.Overrides.TryGet(item.QualifiedId, out DecompositionOverrideRow? pinned);

        PrimeProfile profile = this.ProfileOf(item);
        if (pinned?.Refuse == true || profile.IsEmpty)
            return DecompositionResult.Refuse(profile);

        if (modifiers?.Concentration is int top && top > 0 && top < profile.Weights.Count)
            profile = new PrimeProfile(Normalize(profile.Weights.Take(top)), profile.Source + "+concentrated");

        int units = pinned?.Units ?? Math.Max(1, (int)Math.Floor(
            settings.K
            * Math.Pow(Math.Max(0, item.Price), settings.E)
            * (1 + settings.QualityBonus * quality)
            * (modifiers?.YieldMultiplier ?? 1)));
        int unclamped = units;

        // Equivalent exchange: the Primes' value stays at or below ratio x price (doc 12 §2 rule 5).
        double limit = settings.ExchangeRatio * item.Price;
        List<(string Prime, int Units)> split = this.Split(profile, units);
        while (units > 0 && this.ValueOf(split) > limit)
        {
            units--;
            split = this.Split(profile, units);
        }

        int residue = 0;
        ResidueAmount rate = modifiers?.ResidueRate ?? settings.DefaultResidue;
        if (residueRoll != null && rate.Count > 0 && residueRoll.Chance(rate.Chance))
            residue = rate.Count;

        return new DecompositionResult(false, split.Where(s => s.Units > 0).ToList(), units < unclamped, unclamped, residue, profile);
    }

    /// <summary>Largest-remainder split: the total is exact and deterministic; ties go to the lower Prime sort order.</summary>
    public List<(string Prime, int Units)> Split(PrimeProfile profile, int units)
    {
        var shares = profile.Weights
            .Select(w => (w.Prime, Exact: w.Weight * units, Floor: (int)Math.Floor(w.Weight * units + 1e-9)))
            .ToList();
        int remaining = units - shares.Sum(s => s.Floor);
        var order = shares
            .Select((s, i) => (Index: i, Remainder: s.Exact - s.Floor, Sort: this.SortOrderOf(s.Prime)))
            .OrderByDescending(s => Math.Round(s.Remainder, 9))
            .ThenBy(s => s.Sort)
            .ThenBy(s => s.Index)
            .ToList();
        var result = shares.Select(s => (s.Prime, Units: s.Floor)).ToList();
        for (int i = 0; i < remaining && order.Count > 0; i++)
        {
            int index = order[i % order.Count].Index;
            result[index] = (result[index].Prime, result[index].Units + 1);
        }
        return result;
    }

    private double ValueOf(IEnumerable<(string Prime, int Units)> split)
    {
        double total = 0;
        foreach ((string prime, int units) in split)
        {
            if (units > 0 && this.registry.Primes.TryGet(prime, out PrimeRow row) && row.ItemId != null)
                total += units * (this.catalog.Get(row.ItemId)?.Price ?? 0);
        }
        return total;
    }

    private PrimeProfile ComputeProfile(ItemInfo item)
    {
        DecompositionSettingsRow settings = this.registry.Settings;

        if (this.registry.Overrides.TryGet(item.QualifiedId, out DecompositionOverrideRow pinned) && pinned.Profile is { Count: > 0 })
            return this.Finish(pinned.Profile, settings.Threshold, "override");

        var sum = new Dictionary<string, double>(StringComparer.Ordinal);
        bool matched = false;
        void AddRow(TagWeightRow row)
        {
            matched = true;
            foreach ((string prime, double weight) in row.Weights)
            {
                if (!this.registry.Primes.TryGet(prime, out PrimeRow primeRow))
                    continue;
                if (primeRow.Group == PrimeGroups.Rare && !row.AetherSource)
                    continue; // Aether only comes from the explicit source list (doc 12 §2 rule 6)
                sum[prime] = sum.GetValueOrDefault(prime) + weight;
            }
        }

        foreach (string tag in item.Tags.OrderBy(t => t, StringComparer.Ordinal))
            if (this.registry.TagWeights.TryGet(tag, out TagWeightRow row))
                AddRow(row);
        if (this.registry.TagWeights.TryGet(item.Category.ToString(CultureInfo.InvariantCulture), out TagWeightRow categoryRow))
            AddRow(categoryRow);

        if (matched && sum.Values.Any(v => v > 0))
            return this.Finish(sum, settings.Threshold, "tags");

        if (this.registry.CategoryDefaults.TryGet(item.Category.ToString(CultureInfo.InvariantCulture), out CategoryDefaultRow categoryDefault))
            return this.Finish(this.WithoutRare(categoryDefault.Weights), settings.Threshold, "category default");
        if (this.registry.CategoryDefaults.TryGet("Default", out CategoryDefaultRow globalDefault))
            return this.Finish(this.WithoutRare(globalDefault.Weights), settings.Threshold, "global default");

        return PrimeProfile.EmptyProfile;
    }

    private Dictionary<string, double> WithoutRare(Dictionary<string, double> weights)
        => weights.Where(w => this.registry.Primes.TryGet(w.Key, out PrimeRow p) && p.Group != PrimeGroups.Rare)
            .ToDictionary(w => w.Key, w => w.Value);

    /// <summary>Normalize to 1, drop Primes under the readability threshold, renormalize (doc 03 step 2).</summary>
    private PrimeProfile Finish(IReadOnlyDictionary<string, double> weights, double threshold, string source)
    {
        var positive = weights.Where(w => w.Value > 0 && this.registry.Primes.Contains(w.Key)).Select(w => (w.Key, w.Value)).ToList();
        if (positive.Count == 0)
            return PrimeProfile.EmptyProfile;
        var normalized = Normalize(positive);
        var kept = normalized.Where(w => w.Weight >= threshold).ToList();
        if (kept.Count == 0)
            kept = normalized.Take(1).ToList();
        return new PrimeProfile(this.Order(Normalize(kept)), source);
    }

    private IReadOnlyList<(string Prime, double Weight)> Order(IEnumerable<(string Prime, double Weight)> weights)
        => weights.OrderByDescending(w => w.Weight).ThenBy(w => this.SortOrderOf(w.Prime)).ThenBy(w => w.Prime, StringComparer.Ordinal).ToList();

    private static List<(string Prime, double Weight)> Normalize(IEnumerable<(string Prime, double Weight)> weights)
    {
        var list = weights.ToList();
        double total = list.Sum(w => w.Weight);
        return total <= 0 ? new() : list.Select(w => (w.Prime, w.Weight / total)).ToList();
    }

    private int SortOrderOf(string prime) => this.registry.Primes.TryGet(prime, out PrimeRow row) ? row.SortOrder : int.MaxValue;
}
