namespace ValleyOpus.Engine.Conditions;

/// <summary>
/// Everything a condition can depend on, read from the game once by the adapter's <c>Neighbours</c> module when
/// inputs are loaded (or at trigger time for rituals) and stored with the machine, so every screen and every test
/// gets the same answer from the same snapshot (doc 09 §5b, doc 12 §3).
/// </summary>
public sealed record ConditionSnapshot
{
    public static readonly ConditionSnapshot Empty = new();

    /// <summary>The apparatus's neighbour state, or null when no state matches its surroundings.</summary>
    public string? NeighbourState { get; init; }

    /// <summary>Game state query results, keyed by the query string. A query that's missing counts as false.</summary>
    public IReadOnlyDictionary<string, bool> Queries { get; init; } = new Dictionary<string, bool>();

    /// <summary>Objects and map tiles around the apparatus, with their tags (including heat, darkness and shrine tile flags).</summary>
    public IReadOnlyList<NeighbourFact> Neighbours { get; init; } = Array.Empty<NeighbourFact>();

    /// <summary>Catalyst items available to the run, by qualified item ID.</summary>
    public IReadOnlyDictionary<string, int> Catalysts { get; init; } = new Dictionary<string, int>();

    /// <summary>Shades available from a carried Gloam Lantern.</summary>
    public int Shades { get; init; }

    /// <summary>Power kinds adjacent and currently charged (an empty source pauses recipes that need it, doc 13 §1a).</summary>
    public IReadOnlySet<string> ChargedPower { get; init; } = new HashSet<string>();
}

/// <summary>Something at an offset from the apparatus: an object or a map tile, with its tags.</summary>
public sealed record NeighbourFact(int Dx, int Dy, IReadOnlySet<string> Tags)
{
    public static NeighbourFact Of(int dx, int dy, params string[] tags) => new(dx, dy, new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase));
}
