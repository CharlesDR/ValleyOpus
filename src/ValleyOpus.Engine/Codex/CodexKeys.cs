namespace ValleyOpus.Engine.Codex;

/// <summary>The namespaced discovery keys of doc 12 §10 and doc 07.</summary>
public static class CodexKeys
{
    public const string RecipePrefix = "recipe:";
    public const string ItemPrefix = "item:";
    public const string TriedPrefix = "tried:";
    public const string AssayPrefix = "assay:";
    public const string EffectPrefix = "effect:";
    public const string CountPrefix = "count:";
    public const string HandCountPrefix = "count-hand:";
    public const string HintPrefix = "hint:";
    public const string MilestonePrefix = "milestone:";

    public static string Recipe(string recipeId) => RecipePrefix + recipeId;
    public static string Item(string qualifiedItemId) => ItemPrefix + qualifiedItemId;
    public static string Tried(IEnumerable<string> inputItemIds) => TriedPrefix + TriedSignature(inputItemIds);
    public static string Assay(string qualifiedItemId) => AssayPrefix + qualifiedItemId;
    public static string Effect(string conditionId) => EffectPrefix + conditionId;
    public static string Count(string qualifiedItemId) => CountPrefix + qualifiedItemId;
    public static string HandCount(string qualifiedItemId) => HandCountPrefix + qualifiedItemId;
    public static string Hint(string hintId) => HintPrefix + hintId;
    public static string Milestone(string milestoneId) => MilestonePrefix + milestoneId;

    /// <summary>
    /// The sorted input item IDs, so "A + B" and "B + A" are one entry; duplicates stay ("A + A" differs from "A").
    /// Conditions never change the signature (doc 07).
    /// </summary>
    public static string TriedSignature(IEnumerable<string> inputItemIds)
        => string.Join("+", inputItemIds.OrderBy(id => id, StringComparer.Ordinal));
}
