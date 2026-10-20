namespace ValleyOpus.Engine.Model;

/// <summary>The mod's fixed names (doc 09 §11, doc 17 §1).</summary>
public static class Ids
{
    /// <summary>The C# mod's unique ID.</summary>
    public const string ModId = "CharlesDR.ValleyOpus";

    /// <summary>Prefix of every item, recipe, unlock, Prime and row ID.</summary>
    public const string RowPrefix = ModId + "_";

    /// <summary>Prefix of every registry asset, <c>modData</c> key and save-data key.</summary>
    public const string KeyPrefix = ModId + "/";

    /// <summary>The recipe memory a machine holds when it is set to decompose (doc 13 §6a).</summary>
    public const string DecomposeRecipe = RowPrefix + "Decompose";

    /// <summary>Context tag on every reagent the Satchel auto-collects (doc 15).</summary>
    public const string ReagentTag = "valleyopus_reagent";

    /// <summary>Context tag on every product with an outside use (doc 15).</summary>
    public const string ProductTag = "valleyopus_product";

    /// <summary>Qualifies an item ID the way the game does; an unqualified ID is assumed to be an object <c>(O)</c>.</summary>
    public static string QualifyItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return id;
        return id.StartsWith('(') ? id : "(O)" + id;
    }
}

/// <summary>The registry assets of doc 12, doc 13 and doc 03, by their asset name suffix.</summary>
public static class RegistryNames
{
    public const string Primes = "Primes";
    public const string Substances = "Substances";
    public const string Recipes = "Recipes";
    public const string Conditions = "Conditions";
    public const string Operations = "Operations";
    public const string Apparatus = "Apparatus";
    public const string SecretPools = "SecretPools";
    public const string TagWeights = "Decomposition/TagWeights";
    public const string CategoryDefaults = "Decomposition/CategoryDefaults";
    public const string Overrides = "Decomposition/Overrides";
    public const string Settings = "Decomposition/Settings";
    public const string Products = "Products";
    public const string Effects = "Effects";
    public const string XpSources = "XpSources";
    public const string CodexPages = "Codex/Pages";
    public const string CodexLore = "Codex/Lore";
    public const string Milestones = "Milestones";
    public const string Unlocks = "Unlocks";
    public const string WorkshopRooms = "WorkshopRooms";
    public const string WorkshopBundles = "WorkshopBundles";
    public const string LeyNodes = "LeyNodes";
    public const string Hints = "Hints";
    public const string Contracts = "Contracts";
    public const string Familiars = "Familiars";
    public const string Professions = "Professions";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Primes, Substances, Recipes, Conditions, Operations, Apparatus, SecretPools,
        TagWeights, CategoryDefaults, Overrides, Settings,
        Products, Effects, XpSources, CodexPages, CodexLore, Milestones,
        Unlocks, WorkshopRooms, WorkshopBundles, LeyNodes, Hints, Contracts, Familiars, Professions
    };

    /// <summary>The full asset name, e.g. <c>CharlesDR.ValleyOpus/Recipes</c>.</summary>
    public static string AssetName(string registry) => Ids.KeyPrefix + registry;
}
