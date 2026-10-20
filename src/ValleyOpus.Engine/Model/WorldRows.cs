namespace ValleyOpus.Engine.Model;

// Rows of doc 14 (Unlocks, WorkshopRooms, WorkshopBundles), doc 13 (LeyNodes), doc 07 (Hints), doc 15 §6 (Contracts),
// doc 18 (Familiars) and doc 05 (Professions).

public static class GrantTypes
{
    public const string CraftingRecipe = "CraftingRecipe";
    public const string Item = "Item";
    public const string Flag = "Flag";
    public const string Mail = "Mail";
    public const string Capacity = "Capacity";

    /// <summary>A big craftable set down once in the workshop's Entrance Hall for the farm to pick up (Charles, SOS Pass A).</summary>
    public const string Place = "Place";
}

public static class UnlockSources
{
    public const string Intro = "Intro";
    public const string WorkshopRoom = "WorkshopRoom";
    public const string Delay = "Delay";
    public const string Skill = "Skill";
    public const string Friendship = "Friendship";
    public const string Storm = "Storm";
    public const string Discoveries = "Discoveries";
    public const string AnyOf = "AnyOf";
    public const string Discovery = "Discovery";
    public const string Contract = "Contract";
}

public sealed class UnlockGrant
{
    public string Type { get; init; } = "";
    public string Ref { get; init; } = "";
    public int Count { get; init; }
}

public sealed class UnlockSource
{
    public string Type { get; init; } = "";
    public string Ref { get; init; } = "";
}

/// <summary>One farm-wide flag and what it grants (doc 14 §1).</summary>
public sealed class UnlockRow : RowBase
{
    public string Kind { get; init; } = "Feature";
    public List<UnlockGrant> Grants { get; init; } = new();
    public UnlockSource Source { get; init; } = new();

    /// <summary>Unlock IDs (prefixed) or game state queries; all must hold before the source can fire.</summary>
    public List<string> Requires { get; init; } = new();

    /// <summary>Informational only: read by the sweep, never by the game.</summary>
    public string? Pace { get; init; }
    public string? NameKey { get; init; }
    public string? SourceKey { get; init; }
}

public sealed class RoomOpens
{
    public List<string>? AllOf { get; init; }
    public List<string>? AnyOf { get; init; }
}

public sealed class WorkshopRoomRow : RowBase
{
    public int Tier { get; init; }
    public string? Motif { get; init; }
    public string? Host { get; init; }
    public RoomOpens Opens { get; init; } = new();
    public List<string> Bundles { get; init; } = new();
    public List<string> Unlocks { get; init; } = new();
    public string Location { get; init; } = "";
    public string? NameKey { get; init; }
}

public sealed class ItemReward
{
    public string Item { get; init; } = "";
    public int Count { get; init; } = 1;
}

/// <summary>A workshop bundle (doc 14 §3): ingredient-spec slots, N of M, CC-style.</summary>
public sealed class WorkshopBundleRow : RowBase
{
    public string Room { get; init; } = "";
    public List<IngredientSpec> Slots { get; init; } = new();

    /// <summary>How many slots complete the bundle; null = all of them.</summary>
    public int? Required { get; init; }

    /// <summary>Each slot needs a different item from the others.</summary>
    public bool Distinct { get; init; }
    public ItemReward? Reward { get; init; }
    public string? NameKey { get; init; }

    public int RequiredCount => Math.Min(this.Required ?? this.Slots.Count, this.Slots.Count);
}

public sealed class TilePoint
{
    public int X { get; init; }
    public int Y { get; init; }
}

/// <summary>A ley node: a Ley Tap placed on its tile draws its profile each morning (doc 13, doc 14 §7).</summary>
public sealed class LeyNodeRow : RowBase
{
    public string Location { get; init; } = "";
    public TilePoint Tile { get; init; } = new();
    public Dictionary<string, double> Profile { get; init; } = new();
    public int Units { get; init; } = 2;
    public string? RevealedBy { get; init; }
    public string? NameKey { get; init; }
}

public sealed class HintRow : RowBase
{
    public string Target { get; init; } = "";
    public string Channel { get; init; } = "";
    public string Reveals { get; init; } = "Inputs";
    public string? Requires { get; init; }
    public int DivinationCost { get; init; }
    public string? TextKey { get; init; }
}

/// <summary>One of Dr. Sorenson's bulk contracts (doc 15 §6).</summary>
public sealed class ContractRow : RowBase
{
    public string Item { get; init; } = "";
    public int Count { get; init; }
    public int Gold { get; init; }
    public string? RewardUnlock { get; init; }
    public string? Pace { get; init; }
    public string? NameKey { get; init; }
}

public sealed class FamiliarMeet
{
    public string Location { get; init; } = "";
    public string Condition { get; init; } = "TRUE";
}

public sealed class FamiliarRow : RowBase
{
    public string Element { get; init; } = "";
    public List<string> Operations { get; init; } = new();
    public string? Signature { get; init; }
    public FamiliarMeet Meet { get; init; } = new();
    public string? BondRecipe { get; init; }
    public List<IngredientSpec> LikedGifts { get; init; } = new();
    public int Sprite { get; init; }
    public string? NameKey { get; init; }
}

public sealed class PerkRow
{
    public string Variable { get; init; } = "";
    public List<string> Scope { get; init; } = new();
    public string Op { get; init; } = "Add";
    public double Value { get; init; }

    /// <summary>A situation the perk needs: <c>Fed</c>, <c>NearKindled</c>, or a condition ID.</summary>
    public string? Bonus { get; init; }
}

public sealed class ProfessionRow : RowBase
{
    public int Level { get; init; } = 5;
    public string? Parent { get; init; }
    public string? NameKey { get; init; }
    public string? DescKey { get; init; }
    public List<PerkRow> Perks { get; init; } = new();
}
