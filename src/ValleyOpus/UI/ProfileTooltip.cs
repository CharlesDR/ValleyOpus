using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using ValleyOpus.Framework;
using ValleyOpus.Satchel;
using SObject = StardewValley.Object;

namespace ValleyOpus.UI;

/// <summary>
/// Profiles in tooltips (doc 16 §11): once an item is assayed, its description gains a profile line. Profiles are
/// cached briefly by item and quality, since descriptions are asked for every frame a tooltip is open.
/// </summary>
internal static class ProfileTooltip
{
    private const int CacheTicks = 600;
    private static readonly Dictionary<string, string?> Cache = new(StringComparer.Ordinal);
    private static int cacheTick = int.MinValue;
    private static bool loggedError;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(SObject), nameof(SObject.getDescription)),
            postfix: new HarmonyMethod(typeof(ProfileTooltip), nameof(GetDescriptionPostfix)));
        ElementTooltip.Patch(harmony);
    }

    private static void GetDescriptionPostfix(SObject __instance, ref string __result)
    {
        try
        {
            if (!Context.IsWorldReady || ModServices.Content is null || ModServices.Farm is null || __instance is null)
                return;
            if (!SatchelService.IsAssayed(__instance))
                return;
            string? profile = ProfileOf(__instance);
            if (profile != null)
                __result += "\n" + ModServices.T("tooltip.profile", new { profile });
        }
        catch (Exception ex)
        {
            if (!loggedError)
            {
                loggedError = true;
                ModServices.Log?.Error($"The tooltip profile line failed for {__instance?.QualifiedItemId}; further errors are hidden. {ex}");
            }
        }
    }

    /// <summary>The profile text, or null when the item has none (a Prime, a refused item, or nothing to give).</summary>
    private static string? ProfileOf(SObject item)
    {
        if (Game1.ticks - cacheTick >= CacheTicks || Game1.ticks < cacheTick)
        {
            Cache.Clear();
            cacheTick = Game1.ticks;
        }
        string key = item.QualifiedItemId + "|" + item.Quality;
        if (Cache.TryGetValue(key, out string? cached))
            return cached;

        string text = SatchelService.ProfileText(item);
        bool none = text == ModServices.T("assay.prime") || text == ModServices.T("assay.refused") || text == ModServices.T("assay.nothing");
        return Cache[key] = none ? null : text;
    }
}
