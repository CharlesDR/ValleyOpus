using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.Effects;

/// <summary>
/// The <c>Effects</c> module (doc 15): the one custom buff handler in the slice (Undine's Gift), our fertilizers
/// through vanilla's fertilizer queries, and the Blacksmith bonus on our metal bars (Charles, option A). Energy
/// drinks and the Smaltum dye need no code: they are plain <c>Data/Objects</c> fields and tags.
/// </summary>
internal sealed class EffectService
{
    public const string MetalBarTag = "valleyopus_metal_bar";
    private const int BlacksmithProfession = 20;

    private static int waterBefore = -1;
    private readonly SmapiLog log;

    public EffectService(IModHelper helper, SmapiLog log)
    {
        this.log = log;
    }

    /// <summary>Whether a player has an active custom buff with this handler.</summary>
    public static bool HasHandler(Farmer who, string handler) => HandlerParam(who, handler, null) != null;

    /// <summary>A parameter of the strongest active custom buff with this handler ("" when it has none), or null.</summary>
    public static string? HandlerParam(Farmer who, string handler, string? param)
    {
        IDictionary<string, BuffData> data = DataLoader.Buffs(Game1.content);
        string? found = null;
        foreach (string id in who.buffs.AppliedBuffs.Keys)
        {
            if (!data.TryGetValue(id, out BuffData? row) || row.CustomFields is null
                || !row.CustomFields.TryGetValue(GameContent.HandlerField, out string? name) || name != handler)
                continue;
            if (param is null)
                return "";
            if (row.CustomFields.TryGetValue(Ids.KeyPrefix + param, out string? value))
                found = found is null || Compare(value, found) > 0 ? value : found;
        }
        return found;
    }

    private static int Compare(string a, string b)
        => double.TryParse(a, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double x)
            && double.TryParse(b, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double y)
                ? x.CompareTo(y) : string.CompareOrdinal(a, b);

    public static double HandlerNumber(Farmer who, string handler, string param, double fallback = 0)
        => HandlerParam(who, handler, param) is string raw && double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value) ? value : fallback;

    private static EffectService Self => ModServices.Effects;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(WateringCan), nameof(WateringCan.DoFunction)),
            prefix: new HarmonyMethod(typeof(EffectService), nameof(WaterPrefix)),
            postfix: new HarmonyMethod(typeof(EffectService), nameof(WaterPostfix)));
        harmony.Patch(AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.GetFertilizerSpeedBoost)),
            postfix: new HarmonyMethod(typeof(EffectService), nameof(SpeedPostfix)));
        harmony.Patch(AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.GetFertilizerQualityBoostLevel)),
            postfix: new HarmonyMethod(typeof(EffectService), nameof(QualityPostfix)));
        harmony.Patch(AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.GetFertilizerWaterRetentionChance)),
            postfix: new HarmonyMethod(typeof(EffectService), nameof(RetentionPostfix)));
        harmony.Patch(AccessTools.Method(typeof(SObject), nameof(SObject.sellToStorePrice)),
            postfix: new HarmonyMethod(typeof(EffectService), nameof(SellPricePostfix)));
    }

    // ── Undine's Gift: a share of waterings use no water (weak tier, doc 15 §3a) ──

    /// <summary>The chance of a free watering from an active custom buff with this handler, or 0.</summary>
    public static double HandlerChance(Farmer who, string handler)
    {
        double best = 0;
        IDictionary<string, BuffData> data = DataLoader.Buffs(Game1.content);
        foreach (string id in who.buffs.AppliedBuffs.Keys)
        {
            if (!data.TryGetValue(id, out BuffData? row) || row.CustomFields is null
                || !row.CustomFields.TryGetValue(GameContent.HandlerField, out string? name) || name != handler)
                continue;
            if (row.CustomFields.TryGetValue(Ids.KeyPrefix + "Chance", out string? raw) && double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double chance))
                best = Math.Max(best, chance);
        }
        return best;
    }

    private static void WaterPrefix(WateringCan __instance) => waterBefore = __instance.WaterLeft;

    private static void WaterPostfix(WateringCan __instance, Farmer who)
    {
        try
        {
            if (waterBefore < 0 || __instance.WaterLeft >= waterBefore)
                return;
            double chance = HandlerChance(who, "UndinesGift");
            if (chance > 0 && Game1.random.NextDouble() < chance)
                __instance.WaterLeft = waterBefore;
        }
        catch (Exception ex)
        {
            Self.log.Error($"Undine's Gift failed: {ex}");
        }
        finally
        {
            waterBefore = -1;
        }
    }

    // ── Fertilizer (doc 15 §4) ───────────────────────────────────────────────────

    private static ProductUse? OurFertilizer(HoeDirt dirt)
    {
        string? value = dirt.fertilizer.Value;
        if (string.IsNullOrEmpty(value) || !value.Contains(Ids.RowPrefix, StringComparison.Ordinal) || ModServices.Content is null)
            return null;
        foreach (string id in value.Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (ModServices.Content.Registry.ProductForItem(ItemRegistry.QualifyItemId(id) ?? id)?.Use(UseKinds.Fertilizer) is { } use)
                return use;
        }
        return null;
    }

    private static void SpeedPostfix(HoeDirt __instance, ref float __result)
    {
        if (OurFertilizer(__instance) is { SpeedBoost: > 0 } use)
            __result = Math.Max(__result, (float)use.SpeedBoost);
    }

    private static void QualityPostfix(HoeDirt __instance, ref int __result)
    {
        if (OurFertilizer(__instance) is { QualityLevel: > 0 } use && Game1.random.NextDouble() < use.QualityChance)
            __result = Math.Max(__result, use.QualityLevel);
    }

    private static void RetentionPostfix(HoeDirt __instance, ref float __result)
    {
        if (OurFertilizer(__instance) is { WaterRetention: > 0 } use)
            __result = Math.Max(__result, (float)use.WaterRetention);
    }

    // ── Blacksmith on our metal bars (spike S16; Charles chose the small patch, option A) ──

    private static void SellPricePostfix(SObject __instance, long specificPlayerID, ref int __result)
    {
        if (__instance.Category == SObject.metalResources || !__instance.HasContextTag(MetalBarTag))
            return;
        Farmer who = specificPlayerID == -1 ? Game1.player : Game1.getFarmerMaybeOffline(specificPlayerID) ?? Game1.player;
        if (who.professions.Contains(BlacksmithProfession))
            __result = (int)(__result * 1.5f);
    }
}
