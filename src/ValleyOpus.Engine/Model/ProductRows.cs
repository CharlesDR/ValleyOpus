using System.Text.Json.Serialization;

namespace ValleyOpus.Engine.Model;

// Rows of doc 15 (Products, Effects), doc 05 (XpSources) and doc 07 (Codex/Pages, Codex/Lore, Milestones).

public static class ProductFamilies
{
    public const string Consumable = "Consumable";
    public const string Fertilizer = "Fertilizer";
    public const string Sellable = "Sellable";
    public const string Reagent = "Reagent";
}

public static class UseKinds
{
    public const string Edible = "Edible";
    public const string Apply = "Apply";
    public const string Fertilizer = "Fertilizer";
    public const string Material = "Material";
    public const string Gift = "Gift";
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PriceMode { Derived, Fixed }

/// <summary>How an item's price is set (doc 15 §6): <c>round(Markup × Σ input prices + Flat)</c>, or a fixed value.</summary>
public sealed class PriceRule
{
    public PriceMode Mode { get; init; } = PriceMode.Derived;

    /// <summary>The markup; null = the tier's band (Tier 1 ×1.2, Tier 2 ×1.4).</summary>
    public double? Markup { get; init; }
    public int Flat { get; init; }

    /// <summary>The price for <see cref="PriceMode.Fixed"/>.</summary>
    public int Value { get; init; }

    /// <summary>The recipe the derived price is computed from; null = the cheapest route.</summary>
    public string? ReferenceRecipe { get; init; }
}

/// <summary>One typed use of a product (doc 15 data shape); only the fields of its <see cref="Kind"/> are read.</summary>
public sealed class ProductUse
{
    public string Kind { get; init; } = UseKinds.Edible;

    // Edible
    public int Energy { get; init; }
    public int Health { get; init; }
    public bool IsDrink { get; init; }
    public List<string> Effects { get; init; } = new();

    // Fertilizer
    public int QualityLevel { get; init; }
    public double QualityChance { get; init; }
    public double SpeedBoost { get; init; }
    public double WaterRetention { get; init; }

    // Material
    public string? System { get; init; }
    public string? Ref { get; init; }
}

/// <summary>A product: an item with at least one use outside the mod (doc 15 §1). Keyed by substance or item ID.</summary>
public sealed class ProductRow : RowBase
{
    public string Family { get; init; } = ProductFamilies.Sellable;
    public List<ProductUse> Uses { get; init; } = new();
    public PriceRule Price { get; init; } = new();
    public string CategoryRole { get; init; } = "Artisan";

    /// <summary>Whether the Satchel picks it up; null = only the Reagent family (doc 15 player-facing spec).</summary>
    public bool? AutoCollect { get; init; }
    public List<string> UsesKeys { get; init; } = new();

    [JsonIgnore]
    public bool CollectsToSatchel => this.AutoCollect ?? this.Family == ProductFamilies.Reagent;

    public ProductUse? Use(string kind) => this.Uses.FirstOrDefault(u => u.Kind == kind);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EffectKind { VanillaStats, Custom }

/// <summary>An effect (doc 15 §3); each becomes one <c>Data/Buffs</c> row, custom ones naming their handler.</summary>
public sealed class EffectRow : RowBase
{
    public EffectKind Kind { get; init; } = EffectKind.VanillaStats;
    public Dictionary<string, double> Stats { get; init; } = new();
    public string? Handler { get; init; }
    public Dictionary<string, string> Params { get; init; } = new();

    /// <summary>Duration in game minutes.</summary>
    public int Duration { get; init; } = 120;
    public string Tier { get; init; } = "Weak";
    public string? NameKey { get; init; }
    public string? DescKey { get; init; }
}

public static class XpEvents
{
    public const string Discovery = "Discovery";
    public const string EffectDiscovery = "EffectDiscovery";
    public const string HandCollect = "HandCollect";
    public const string AutoCollect = "AutoCollect";
    public const string Assay = "Assay";
    public const string TriedKey = "TriedKey";
    public const string RitualTrigger = "RitualTrigger";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Discovery, EffectDiscovery, HandCollect, AutoCollect, Assay, TriedKey, RitualTrigger };
}

/// <summary>One XP source (doc 05 data shape).</summary>
public sealed class XpSourceRow : RowBase
{
    public string Event { get; init; } = "";
    public int Amount { get; init; }

    /// <summary>Multiply by the recipe's tier (first discovery: 100 × tier).</summary>
    public bool TierScale { get; init; }

    /// <summary>Recipe / Effect / Item / Key, or null for every time.</summary>
    public string? OncePer { get; init; }

    /// <summary>The most XP one action gives (hand collection of a stack); 0 = no cap.</summary>
    public int CapPerAction { get; init; }

    /// <summary>A fraction of the hand amount, for automated output (doc 05: a quarter, placeholder).</summary>
    public double Scale { get; init; } = 1;
}

/// <summary>A codex page (doc 07); its entries are derived from Primes, Substances and Recipes.</summary>
public sealed class CodexPageRow : RowBase
{
    public string Group { get; init; } = "";
    public int SortOrder { get; init; }
    public string? TitleKey { get; init; }
}

/// <summary>A lore line that appears once an item has been made <see cref="Threshold"/> times (doc 07).</summary>
public sealed class LoreRow : RowBase
{
    public string TargetId { get; init; } = "";
    public int Threshold { get; init; } = 1;
    public string? TextKey { get; init; }
}

/// <summary>
/// A milestone (doc 07): <c>Trigger</c> is <c>FirstOfTier:&lt;n&gt;</c>, <c>RowComplete:&lt;Prime&gt;</c>,
/// <c>TierComplete:&lt;n&gt;</c> or <c>Process:&lt;substance&gt;</c>.
/// </summary>
public sealed class MilestoneRow : RowBase
{
    public string Trigger { get; init; } = "";
    public string Celebration { get; init; } = "Toast";
    public string? TextKey { get; init; }
}

/// <summary>An apparatus's fuel slot (doc 13): any loader can top it up; each run burns <see cref="PerRun"/>.</summary>
public sealed class FuelSpec
{
    public string Item { get; init; } = "";
    public int Capacity { get; init; } = 10;
    public int PerRun { get; init; } = 1;
}

/// <summary>An apparatus that heats its neighbours once a flag is set (the kindled Athanor, doc 13 §2).</summary>
public sealed class HeatSpec
{
    /// <summary>The machine flag that must be set, e.g. <c>Kindled</c>; null = always hot.</summary>
    public string? RequiresFlag { get; init; }
    public int MaxConsumers { get; init; } = 2;
}
