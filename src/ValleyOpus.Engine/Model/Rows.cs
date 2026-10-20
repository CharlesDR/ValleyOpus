using System.Text.Json.Serialization;

namespace ValleyOpus.Engine.Model;

// Registry rows as content packs write them (doc 12 data shape, doc 13 data shape, doc 03 data shape).
// Every row carries a FormatVersion so older pack formats can be upgraded in memory (doc 17 §5f).
// A row's Id defaults to its dictionary key.

public interface IRow
{
    string Id { get; set; }
    int FormatVersion { get; set; }
}

/// <summary>Rows are read-only once loaded; only the registry sets <see cref="Id"/> (from the key) and the format version.</summary>
public abstract class RowBase : IRow
{
    public string Id { get; set; } = "";
    public int FormatVersion { get; set; } = 1;
}

public static class PrimeGroups
{
    public const string Element = "Element";
    public const string Principle = "Principle";

    /// <summary>Rare Primes (Aether) only come from rows on the explicit source list (doc 12 §2 rule 6).</summary>
    public const string Rare = "Rare";
}

public sealed class PrimeRow : RowBase
{
    public string? ItemId { get; init; }
    public string Group { get; init; } = PrimeGroups.Element;
    public string? Color { get; init; }
    public string? NameKey { get; init; }
    public string? GlossKey { get; init; }
    public int SortOrder { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubstanceClass { Item, Power, ProcessState, Intermediate }

public sealed class SubstanceRow : RowBase
{
    public string? ItemId { get; init; }
    public SubstanceClass Class { get; init; } = SubstanceClass.Item;
    public bool Bottleable { get; init; }
    public int Tier { get; init; }
    public string? Polarity { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? NameKey { get; init; }
    public string? GlossKey { get; init; }
    public string? FlavorKey { get; init; }
}

public sealed class IngredientSpec
{
    /// <summary>An exact item, Prime or substance ID.</summary>
    public string? Item { get; init; }

    /// <summary>A context tag, e.g. <c>category_fish</c>.</summary>
    public string? Tag { get; init; }

    /// <summary>A vanilla category number.</summary>
    public int? Category { get; init; }

    public int Count { get; init; } = 1;
    public int MinQuality { get; init; }

    public override string ToString()
        => (Item ?? (Tag != null ? "#" + Tag : "cat" + Category)) + (Count != 1 ? " x" + Count : "");
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionUse { Required, Bonus, Variant }

public sealed class ConditionEffect
{
    public int QualitySteps { get; init; }
    public double YieldBonus { get; init; }
    public double SpeedMultiplier { get; init; } = 1;
}

public sealed class RecipeConditionRef
{
    public string ConditionId { get; init; } = "";
    public ConditionUse Use { get; init; } = ConditionUse.Required;

    /// <summary>A soft Required condition degrades a ritual instead of blocking it (doc 12 §8).</summary>
    public bool Soft { get; init; }

    /// <summary>Requires the condition NOT to hold: the base row of a Variant pair requires its negation (doc 12 §4).</summary>
    public bool Negate { get; init; }

    public ConditionEffect? Effect { get; init; }
}

public sealed class RecipeOutput
{
    /// <summary>The output ID of a stage advance: the advanced input itself comes back, higher (doc 12 §5).</summary>
    public const string SameAsInput = Ids.RowPrefix + "SameAsInput";

    /// <summary>A substance ID, or a vanilla item ID.</summary>
    public string Id { get; init; } = "";
    public int Count { get; init; } = 1;
    public string? Flavor { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QualityMode { Fixed, InheritMin, InheritAverage, Advance }

public sealed class QualityRule
{
    public QualityMode Mode { get; init; } = QualityMode.InheritMin;

    /// <summary>The stage for <see cref="QualityMode.Fixed"/>, or the steps for <see cref="QualityMode.Advance"/>.</summary>
    public int Steps { get; init; }

    /// <summary>A vanilla quality (0, 1, 2 or 4), lower than the apparatus cap; null = the apparatus cap.</summary>
    public int? Cap { get; init; }
    public int Floor { get; init; }
}

public sealed class ResidueAmount
{
    public int Count { get; init; }
    public double Chance { get; init; } = 1;
}

public sealed class ResidueRule
{
    public ResidueAmount? Success { get; init; }
    public ResidueAmount? Partial { get; init; }
    public ResidueAmount? Failure { get; init; }
}

public sealed class DiscoveryRule
{
    public bool TableAttemptable { get; init; } = true;
    public bool Hidden { get; init; }
    public List<string> HintKeys { get; init; } = new();
}

public sealed class RecipeRow : RowBase
{
    public List<IngredientSpec> Inputs { get; init; } = new();
    public string Operation { get; init; } = "";
    public List<string> ApparatusKinds { get; init; } = new();
    public List<string> ApparatusIds { get; init; } = new();

    /// <summary>The neighbour states the recipe runs in; required and never empty (doc 13 §1a).</summary>
    public List<string> NeighbourStates { get; init; } = new();
    public List<RecipeConditionRef> Conditions { get; init; } = new();
    public RecipeOutput Output { get; init; } = new();
    public QualityRule Quality { get; init; } = new();
    public ResidueRule? Residue { get; init; }
    public int? Minutes { get; init; }
    public int? Days { get; init; }
    public bool HandMadeOnly { get; init; }
    public string? TwinGroup { get; init; }
    public DiscoveryRule Discovery { get; init; } = new();
    public string? SecretPool { get; init; }

    /// <summary>An unlock the farm must hold before the recipe exists at all: only Tier 3 uses it (doc 14 §1).</summary>
    public string? RequiresUnlock { get; init; }

    [JsonIgnore]
    public int InputCount => this.Inputs.Sum(p => p.Count);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionKind { Gsq, Adjacency, Catalyst, Power }

public sealed class AdjacencyTest
{
    public string Tag { get; init; } = "";

    /// <summary><c>Cardinal</c> (4 directions) or <c>All</c> (8 directions).</summary>
    public string Directions { get; init; } = "All";
    public int Range { get; init; } = 1;
}

public sealed class CatalystTest
{
    public string? Item { get; init; }
    public int Shades { get; init; }
    public int Count { get; init; } = 1;
}

public sealed class ConditionRow : RowBase
{
    public ConditionKind Kind { get; init; } = ConditionKind.Gsq;
    public string? Query { get; init; }
    public AdjacencyTest? Adjacency { get; init; }
    public CatalystTest? Catalyst { get; init; }
    public string? Power { get; init; }

    /// <summary>
    /// Whether the player controls this condition. Adjacency, catalysts and power always are; a GSQ is environmental
    /// (weekday, weather, season, moon, time) unless marked, e.g. a location query (doc 13 §1a).
    /// </summary>
    public bool? PlayerControlled { get; init; }
    public string? NameKey { get; init; }
    public string? HintKey { get; init; }

    [JsonIgnore]
    public bool IsPlayerControlled => this.PlayerControlled ?? this.Kind != ConditionKind.Gsq;
}

public sealed class OperationRow : RowBase
{
    public string? NameKey { get; init; }
}

public static class ApparatusKinds
{
    public const string Engineered = "Engineered";
    public const string MinorArcane = "MinorArcane";
    public const string RitualAltar = "RitualAltar";
    public const string RitualCircle = "RitualCircle";
    public const string Table = "Table";
    public const string Specialist = "Specialist";

    /// <summary>Appended vanilla machine rules (Furnace, Keg); Automate drives them natively, so hand-made-only can't hold there.</summary>
    public const string Vanilla = "Vanilla";
}

public sealed class NeighbourStateTest
{
    /// <summary>The power kinds that must be adjacent, exactly; empty with <see cref="None"/> means no power at all.</summary>
    public List<string> Power { get; init; } = new();

    /// <summary>Whether a heat source must (true) or must not (false) be adjacent; null = heat doesn't matter.</summary>
    public bool? Heat { get; init; }

    /// <summary>No adjacent power.</summary>
    public bool None { get; init; }
}

public sealed class NeighbourStateRow
{
    public string Id { get; init; } = "";
    public NeighbourStateTest Test { get; init; } = new();
}

public sealed class ApparatusOperation
{
    public string Operation { get; init; } = "";
    public List<string> States { get; init; } = new();
}

public sealed class DecomposeModifiers
{
    public double YieldMultiplier { get; init; } = 1;
    public ResidueAmount? ResidueRate { get; init; }

    /// <summary>Keep only the top N Primes of the profile (the Athanor keeps two).</summary>
    public int? Concentration { get; init; }
    public int? Minutes { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TriggerMode { Auto, Hand }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AutomationMode { Full, StageOnly, None }

public sealed class PowerSpec
{
    public string Emits { get; init; } = "";
    public int MaxConsumers { get; init; }
    public string Mode { get; init; } = "Duration";
    public int Amount { get; init; }
    public bool StormCharge { get; init; }
}

public sealed class ApparatusRow : RowBase
{
    public string Kind { get; init; } = ApparatusKinds.Engineered;
    public List<NeighbourStateRow> NeighbourStates { get; init; } = new();
    public List<ApparatusOperation> Operations { get; init; } = new();
    public DecomposeModifiers? Decompose { get; init; }
    public int? QualityCap { get; init; }
    public int TrayCapacity { get; init; } = 2;
    public TriggerMode Trigger { get; init; } = TriggerMode.Auto;
    public AutomationMode Automation { get; init; } = AutomationMode.Full;
    public bool Chaining { get; init; } = true;
    public PowerSpec? Power { get; init; }
    public FuelSpec? Fuel { get; init; }
    public HeatSpec? Heat { get; init; }

    /// <summary>The item that places this apparatus, e.g. <c>(BC)CharlesDR.ValleyOpus_Athanor</c>.</summary>
    public string? ItemId { get; init; }
    public string? FeedbackStyle { get; init; }
    public string? UnlockFlag { get; init; }
    public string? NameKey { get; init; }
    public string? HintKey { get; init; }
}

public sealed class SecretPoolRow : RowBase
{
    public int PickCount { get; init; } = 1;
}

public sealed class TagWeightRow : RowBase
{
    public Dictionary<string, double> Weights { get; init; } = new();
    public bool AetherSource { get; init; }
}

public sealed class CategoryDefaultRow : RowBase
{
    public Dictionary<string, double> Weights { get; init; } = new();
}

public sealed class DecompositionOverrideRow : RowBase
{
    public Dictionary<string, double>? Profile { get; init; }
    public int? Units { get; init; }
    public bool Refuse { get; init; }
}

public sealed class DecompositionSettingsRow : RowBase
{
    public double K { get; init; } = 1;
    public double E { get; init; } = 0.5;
    public double QualityBonus { get; init; } = 0.1;
    public double Threshold { get; init; } = 0.10;
    public double ExchangeRatio { get; init; } = 0.8;
    public ResidueAmount DefaultResidue { get; init; } = new() { Count = 1, Chance = 0.25 };

    /// <summary>The item the residue rolls produce (Caput Mortuum); null in fixtures that don't care.</summary>
    public string? ResidueItem { get; init; }
}
