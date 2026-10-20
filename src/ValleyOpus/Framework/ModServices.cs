using StardewModdingAPI;
using ValleyOpus.Engine.Model;
using ValleyOpus.Effects;
using ValleyOpus.Machines;
using ValleyOpus.Routes;
using ValleyOpus.Satchel;
using ValleyOpus.Skill;
using ValleyOpus.State;

namespace ValleyOpus.Framework;

/// <summary>
/// The adapter's services (doc 09 §3), one instance each, reachable from Harmony patches and menus. Set once in
/// <see cref="ModEntry.Entry"/>; dependencies run one way: UI → services → engine.
/// </summary>
internal static class ModServices
{
    public static IModHelper Helper { get; set; } = null!;
    public static IMonitor Monitor { get; set; } = null!;
    public static IManifest Manifest { get; set; } = null!;
    public static SmapiLog Log { get; set; } = null!;
    public static ModConfig Config { get; set; } = new();
    public static GameContent Content { get; set; } = null!;
    public static FarmState Farm { get; set; } = null!;
    public static AlchemySkill Skill { get; set; } = null!;
    public static MachineService Machines { get; set; } = null!;
    public static ChainService Chains { get; set; } = null!;
    public static SatchelService Satchel { get; set; } = null!;
    public static EffectService Effects { get; set; } = null!;
    public static World.WorldService World { get; set; } = null!;

    /// <summary>The asset the base pack fills with its row strings (names, hints, letters), already translated by CP.</summary>
    public const string StringsAsset = Ids.KeyPrefix + "Strings";

    /// <summary>
    /// A translated UI string (every player-facing string is i18n, doc 11 §6): the mod's own i18n first, then the
    /// content packs' row strings, so a pack's <c>NameKey</c> translates with the pack.
    /// </summary>
    public static string T(string key, object? tokens = null)
    {
        Translation own = Helper.Translation.Get(key, tokens);
        if (own.HasValue())
            return own.ToString();
        try
        {
            if (StardewValley.Game1.content?.Load<Dictionary<string, string>>(StringsAsset) is { } strings && strings.TryGetValue(key, out string? text))
                return Fill(text, tokens);
        }
        catch (Exception)
        {
            // before the asset exists (title screen): fall through
        }
        return own.ToString();
    }

    /// <summary>Whether a key has text in either source.</summary>
    public static bool HasT(string key)
    {
        if (Helper.Translation.Get(key).HasValue())
            return true;
        try
        {
            return StardewValley.Game1.content?.Load<Dictionary<string, string>>(StringsAsset)?.ContainsKey(key) == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string Fill(string text, object? tokens)
    {
        if (tokens is null)
            return text;
        foreach (var property in tokens.GetType().GetProperties())
            text = text.Replace("{{" + property.Name + "}}", property.GetValue(tokens)?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
        return text;
    }
}

/// <summary>The player's config.json (the GMCM page arrives in B3; doc 16 §15).</summary>
internal sealed class ModConfig
{
    /// <summary>Trace every resolver decision, chaining transfer and Automate load at Debug level.</summary>
    public bool Verbose { get; set; }

    /// <summary>Opens the codex; unbound by default (doc 16: keys unbound, reached through the Satchel).</summary>
    public SButton CodexKey { get; set; } = SButton.None;

    /// <summary>Show the chain overlay while the Satchel is held.</summary>
    public bool ShowOverlay { get; set; } = true;

    /// <summary>Discovery toasts: All, FirstOnly, or None (doc 16 §15).</summary>
    public string Toasts { get; set; } = "All";

    /// <summary>Machines chaining moves per second, for performance (doc 09 §8).</summary>
    public int ChainBudgetPerSecond { get; set; } = 200;

    /// <summary>The Azoth Diffuser's radius in tiles (doc 15: a config setting, default 7).</summary>
    public int DiffuserRadius { get; set; } = 7;

    /// <summary>Allow the dev commands that grant unlocks and fill bundles (always allowed in test builds).</summary>
    public bool DevCommands { get; set; } = true;
}
