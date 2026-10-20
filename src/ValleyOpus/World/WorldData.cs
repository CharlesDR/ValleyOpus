namespace ValleyOpus.World;

/// <summary>
/// The host's world progress (doc 14, doc 15 §6, doc 18 §4), saved in the envelope's <c>Unlocks</c> section and
/// mirrored into the host farmer's <c>modData</c> so every player reads the same copy. Unlocks themselves are mail
/// flags on the host (<c>CharlesDR.ValleyOpus_Unlock_&lt;Id&gt;</c>), so content packs can test them with
/// <c>PLAYER_HAS_MAIL Host</c>.
/// </summary>
internal sealed class WorldData
{
    /// <summary>Unlock ID → the day it was granted.</summary>
    public Dictionary<string, int> Granted { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Delay-sourced unlock ID → the day its requirements first held.</summary>
    public Dictionary<string, int> Since { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Storm-sourced unlocks waiting for the morning after a storm.</summary>
    public HashSet<string> StormSeen { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Bundle ID → slot index → the item donated there.</summary>
    public Dictionary<string, Dictionary<int, string>> Bundles { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Completed bundles (rewards paid).</summary>
    public HashSet<string> BundlesDone { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Contract ID → the day it was delivered.</summary>
    public Dictionary<string, int> Contracts { get; set; } = new(StringComparer.Ordinal);

    /// <summary>How many times the farm has divined at a bundle board (bulk surprise opens at 5, doc 16).</summary>
    public int Divinations { get; set; }

    /// <summary>Familiar local ID → its state.</summary>
    public Dictionary<string, FamiliarState> Familiars { get; set; } = new(StringComparer.Ordinal);
}

internal sealed class FamiliarState
{
    public int MetDay { get; set; } = -1;

    /// <summary>The bonded Lab's building ID (its GUID), or null while the spirit waits in the workshop.</summary>
    public string? Lab { get; set; }
    public int BondPoints { get; set; }
    public int LastGreetedDay { get; set; } = -1;
    public int LastGiftDay { get; set; } = -1;
}

/// <summary>A farmhand's request to the host (doc 17 §3): the host owns world progress.</summary>
internal sealed class WorldRequest
{
    public string Kind { get; set; } = "";
    public string Id { get; set; } = "";
    public int Slot { get; set; }
    public string? ItemId { get; set; }
}

internal static class WorldMessages
{
    public const string Request = "WorldRequest";
}
