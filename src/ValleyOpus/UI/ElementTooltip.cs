using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Decomposition;
using ValleyOpus.Framework;
using ValleyOpus.Satchel;
using SObject = StardewValley.Object;

namespace ValleyOpus.UI;

/// <summary>
/// The Element row in item tooltips (Charles, SOS feedback): once the intro has played, any ordinary item that
/// decomposes shows the icons of the Primes it gives that the farm has already discovered, with their units, and one
/// greyed "?" vial when it also gives a Prime not yet discovered. Tools, quest items, recipes and anything that can't
/// be trashed are left alone. The row is reserved through vanilla's tooltip hooks (extra height, then drawn after the
/// description), so the box grows to fit.
/// </summary>
internal static class ElementTooltip
{
    private const int CacheTicks = 600;
    private const int IconSize = 32;
    private const int IconGap = 8;
    private const int RowHeight = IconSize + 12;

    /// <summary>Item and quality → its Prime units as (qualified Prime item ID, units), or null when it has none.</summary>
    private static readonly Dictionary<string, List<(string ItemId, int Units)>?> Cache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> RecordedHeld = new(StringComparer.Ordinal);
    private static int cacheTick = int.MinValue;
    private static bool loggedError;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Item), nameof(Item.getExtraSpaceNeededForTooltipSpecialIcons)),
            postfix: new HarmonyMethod(typeof(ElementTooltip), nameof(ExtraSpacePostfix)));
        harmony.Patch(AccessTools.Method(typeof(Item), nameof(Item.drawTooltip)),
            postfix: new HarmonyMethod(typeof(ElementTooltip), nameof(DrawTooltipPostfix)));
    }

    private static void ExtraSpacePostfix(Item __instance, int minWidth, int startingHeight, ref Point __result)
    {
        try
        {
            if (Row(__instance) is not { } row)
                return;
            int width = 32 + Slots(row) * (IconSize + IconGap);
            int height = (__result.Y != 0 ? __result.Y : startingHeight) + RowHeight;
            int baseWidth = __result.X != 0 ? __result.X : minWidth;
            __result = new Point(width > baseWidth ? width : __result.X, height);
        }
        catch (Exception ex)
        {
            LogOnce(__instance, ex);
        }
    }

    private static void DrawTooltipPostfix(Item __instance, SpriteBatch spriteBatch, ref int x, ref int y, float alpha)
    {
        try
        {
            if (Row(__instance) is not { } row)
                return;
            int left = x + 16;
            int top = y + 16;
            foreach ((string itemId, int units, bool known) in Entries(row))
            {
                if (!known)
                    continue;
                ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
                spriteBatch.Draw(data.GetTexture(), new Rectangle(left, top, IconSize, IconSize), data.GetSourceRect(), Color.White * alpha);
                Utility.drawTinyDigits(units, spriteBatch, new Vector2(left + IconSize - Utility.getWidthOfTinyDigitString(units, 2f), top + IconSize - 12), 2f, 1f, Color.White * alpha);
                left += IconSize + IconGap;
            }
            if (Entries(row).Any(e => !e.Known))
                DrawUnknown(spriteBatch, row[0].ItemId, left, top, alpha);
            y += RowHeight;
        }
        catch (Exception ex)
        {
            LogOnce(__instance, ex);
        }
    }

    /// <summary>A Prime's vial, greyed, with a "?" over it: the same art as the Element icons.</summary>
    private static void DrawUnknown(SpriteBatch b, string vialItemId, int left, int top, float alpha)
    {
        ParsedItemData data = ItemRegistry.GetDataOrErrorItem(vialItemId);
        b.Draw(data.GetTexture(), new Rectangle(left, top, IconSize, IconSize), data.GetSourceRect(), Color.DimGray * 0.8f * alpha);
        Vector2 size = Game1.smallFont.MeasureString("?");
        var at = new Vector2(left + (IconSize - size.X) / 2, top + (IconSize - size.Y) / 2 + 2);
        b.DrawString(Game1.smallFont, "?", at + new Vector2(2, 2), Game1.textShadowColor * alpha);
        b.DrawString(Game1.smallFont, "?", at, Color.White * alpha);
    }

    private static int Slots(List<(string ItemId, int Units)> row)
    {
        var entries = Entries(row).ToList();
        return entries.Count(e => e.Known) + (entries.Any(e => !e.Known) ? 1 : 0);
    }

    private static IEnumerable<(string ItemId, int Units, bool Known)> Entries(List<(string ItemId, int Units)> row)
        => row.Select(r => (r.ItemId, r.Units, Discovered(r.ItemId)));

    /// <summary>
    /// A Prime is discovered once the farm codex has its item (made at an apparatus). Holding one counts too: the first
    /// time this player is seen holding it, the codex records it, so the row matches what the player has had.
    /// </summary>
    private static bool Discovered(string primeItemId)
    {
        string key = CodexKeys.Item(primeItemId);
        if (ModServices.Farm.Codex.Has(key))
            return true;
        Farmer who = Game1.player;
        bool held = who.Items.Any(i => i?.QualifiedItemId == primeItemId)
            || ModServices.Satchel.Inventory(who).Any(i => i?.QualifiedItemId == primeItemId);
        if (held && RecordedHeld.Add(primeItemId + "|" + who.UniqueMultiplayerID))
            ModServices.Farm.Record(who, new[] { key });
        return held;
    }

    /// <summary>The item's Prime units, or null when it shouldn't show a row.</summary>
    private static List<(string ItemId, int Units)>? Row(Item item)
    {
        if (!Context.IsWorldReady || ModServices.Content is null || ModServices.Farm is null || ModServices.World is null)
            return null;
        if (item is not SObject obj || item is Tool || obj.questItem.Value || obj.IsRecipe || !obj.canBeTrashed())
            return null;
        if (!ModServices.World.IsUnlocked(World.WorldService.IntroUnlock))
            return null;

        if (Game1.ticks - cacheTick >= CacheTicks || Game1.ticks < cacheTick)
        {
            Cache.Clear();
            cacheTick = Game1.ticks;
        }
        string cacheKey = item.QualifiedItemId + "|" + item.Quality;
        if (Cache.TryGetValue(cacheKey, out var cached))
            return cached;
        return Cache[cacheKey] = Compute(obj);
    }

    private static List<(string ItemId, int Units)>? Compute(SObject item)
    {
        if (ModServices.Content.Registry.PrimeForItem(item.QualifiedItemId) != null)
            return null;
        var decomposer = ModServices.Content.Pipeline.Decomposer;
        var info = GameContent.ToInfo(item);
        if (decomposer.IsRefused(info))
            return null;
        DecompositionResult result = decomposer.Decompose(info, item.Quality, null);
        if (result.Refused || result.Total == 0)
            return null;
        var row = new List<(string ItemId, int Units)>();
        foreach ((string prime, int units) in result.Units)
        {
            if (units > 0 && ModServices.Content.Registry.ResolveItemId(prime) is string itemId)
                row.Add((itemId, units));
        }
        return row.Count == 0 ? null : row;
    }

    private static void LogOnce(Item? item, Exception ex)
    {
        if (loggedError)
            return;
        loggedError = true;
        ModServices.Log?.Error($"The tooltip Element row failed for {item?.QualifiedItemId}; further errors are hidden. {ex}");
    }
}
