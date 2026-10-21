using StardewModdingAPI;
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

    /// <summary>A translated UI string (every player-facing string is i18n, doc 11 §6).</summary>
    public static string T(string key, object? tokens = null) => Helper.Translation.Get(key, tokens).ToString();
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
}
