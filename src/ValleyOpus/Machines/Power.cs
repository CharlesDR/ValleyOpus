using Microsoft.Xna.Framework;
using StardewValley;
using ValleyOpus.Engine.Machines;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Routes;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.Machines;

/// <summary>
/// Power sources (doc 13 §1a, §3 "Supplying power"): Boilers hold Vapor for a duration, Lightning Coils hold Fulgur
/// charges, a kindled Athanor gives Flamma Viva for good. Adjacency alone sets a neighbour's state; the charge only
/// decides whether a recipe that needs the power can run now, read once in the load-time snapshot.
/// </summary>
internal static class Power
{
    /// <summary>Absolute game minute until which a duration source (Boiler) is charged.</summary>
    public const string PoweredUntilKey = Ids.KeyPrefix + "PoweredUntil";

    /// <summary>Charges left in a charge source (Lightning Coil).</summary>
    public const string ChargesKey = Ids.KeyPrefix + "Charges";

    public const string Vapor = Ids.RowPrefix + "Vapor";
    public const string Fulgur = Ids.RowPrefix + "Fulgur";
    public const string FlammaViva = Ids.RowPrefix + "FlammaViva";

    /// <summary>Game minutes since the farm began; overnight counts as the hours slept, close enough for a charge.</summary>
    public static int Now => (int)Game1.stats.DaysPlayed * 1440 + Utility.ConvertTimeToMinutes(Game1.timeOfDay);

    /// <summary>What a tile's neighbours supply: power kinds adjacent (charged or not), and which are charged.</summary>
    public sealed record Supply(HashSet<string> Adjacent, HashSet<string> Charged, List<SObject> Sources);

    public static Supply Read(GameLocation location, (int X, int Y) tile, SObject? self)
    {
        var adjacent = new HashSet<string>(StringComparer.Ordinal);
        var charged = new HashSet<string>(StringComparer.Ordinal);
        var sources = new List<SObject>();

        if (self != null && IsKindledAthanor(self))
            charged.Add(FlammaViva);

        foreach ((int x, int y) in ChainGeometry.Cardinals(tile))
        {
            if (location.getObjectAtTile(x, y) is not SObject source || ModServices.Content.ApparatusOf(source) is not { } row)
                continue;
            if (IsKindledAthanor(source))
                charged.Add(FlammaViva);
            if (row.Power is not { } spec || !Feeds(location, (x, y), tile, spec.MaxConsumers + PowerReachBonus(source)))
                continue;
            adjacent.Add(spec.Emits);
            if (IsCharged(source, spec))
            {
                charged.Add(spec.Emits);
                sources.Add(source);
            }
        }
        return new Supply(adjacent, charged, sources);
    }

    private static bool IsKindledAthanor(SObject obj)
        => ModServices.Content.ApparatusOf(obj) is { Heat.RequiresFlag: "Kindled" } && MachineService.State(obj).Kindled;

    /// <summary>A source feeds its first N neighbours that are apparatus, in N, E, S, W order (doc 13 §3).</summary>
    private static bool Feeds(GameLocation location, (int X, int Y) source, (int X, int Y) consumer, int maxConsumers)
    {
        int n = 0;
        foreach ((int x, int y) in ChainGeometry.Cardinals(source))
        {
            bool isApparatus = location.getObjectAtTile(x, y) is SObject obj && ModServices.Content.ApparatusOf(obj) != null;
            if (!isApparatus)
                continue;
            if (x == consumer.X && y == consumer.Y)
                return n < maxConsumers;
            n++;
        }
        return true; // the consumer isn't placed yet (a probe): count it as fed
    }

    /// <summary>Mechanist: each Boiler powers one more neighbour (doc 05).</summary>
    private static int PowerReachBonus(SObject source)
    {
        Farmer? owner = Game1.getFarmerMaybeOffline(MachineService.State(source).Owner);
        return owner != null && Bonuses.HasProfession(owner, Bonuses.Mechanist) ? 1 : 0;
    }

    public static bool IsCharged(SObject source, PowerSpec spec)
        => spec.Mode == "Charges"
            ? Charges(source) > 0
            : source.modData.TryGetValue(PoweredUntilKey, out string? raw) && int.TryParse(raw, out int until) && until > Now;

    public static int Charges(SObject source) => source.modData.TryGetValue(ChargesKey, out string? raw) && int.TryParse(raw, out int n) ? n : 0;

    public static int MinutesLeft(SObject source)
        => source.modData.TryGetValue(PoweredUntilKey, out string? raw) && int.TryParse(raw, out int until) ? Math.Max(0, until - Now) : 0;

    /// <summary>Charge capacity of a Coil: its amount, plus one for a Mechanist owner (doc 05).</summary>
    public static int Capacity(SObject source, PowerSpec spec) => spec.Amount + PowerReachBonus(source);

    /// <summary>A source's own fuel recipe finished (Ignis + Aqua → Vapor in a Boiler): charge it.</summary>
    public static bool Charge(SObject source, string? processState, bool fed)
    {
        if (ModServices.Content.ApparatusOf(source)?.Power is not { } spec || processState != spec.Emits)
            return false;
        if (spec.Mode == "Charges")
        {
            source.modData[ChargesKey] = Math.Min(Capacity(source, spec) * 2, Charges(source) + spec.Amount).ToString();
        }
        else
        {
            double length = spec.Amount * Bonuses.ChargeLengthMultiplier(source, fed);
            int from = Math.Max(Now, MinutesLeft(source) > 0 ? int.Parse(source.modData[PoweredUntilKey]) : Now);
            source.modData[PoweredUntilKey] = (from + (int)length).ToString();
        }
        source.Location?.temporarySprites.Add(new TemporaryAnimatedSprite(5, source.TileLocation * 64f + new Vector2(0, -48), Color.LightBlue, 8, false, 80f));
        return true;
    }

    /// <summary>A powered run started: each Fulgur run spends one charge from the Coil that feeds it.</summary>
    public static void Spend(SObject machine, string recipeId)
    {
        if (machine.Location is not GameLocation location || ModServices.Content.Registry.Recipes.Get(recipeId) is not { } recipe)
            return;
        foreach (RecipeConditionRef reference in recipe.Conditions.Where(c => c.Use == ConditionUse.Required))
        {
            if (ModServices.Content.Registry.Conditions.Get(reference.ConditionId) is not { Kind: ConditionKind.Power, Power: { } kind })
                continue;
            Supply supply = Read(location, ((int)machine.TileLocation.X, (int)machine.TileLocation.Y), machine);
            SObject? coil = supply.Sources.FirstOrDefault(s => ModServices.Content.ApparatusOf(s)?.Power is { Mode: "Charges" } p && p.Emits == kind);
            if (coil != null)
                coil.modData[ChargesKey] = Math.Max(0, Charges(coil) - 1).ToString();
        }
    }

    /// <summary>Storms fill outdoor Coils each morning; a Sylph's Lab gives indoor Coils a small daily chance (doc 13, doc 18).</summary>
    public static void OnDayStarted()
    {
        Utility.ForEachLocation(location =>
        {
            foreach (SObject obj in location.objects.Values)
            {
                if (ModServices.Content.ApparatusOf(obj)?.Power is not { Mode: "Charges" } spec)
                    continue;
                bool storm = spec.StormCharge && location.IsOutdoors && location.GetWeather().IsLightning;
                bool sylph = !location.IsOutdoors && Bonuses.FamiliarIn(location) == "Sylph" && Game1.random.NextDouble() < 0.15;
                if (storm)
                    obj.modData[ChargesKey] = Math.Max(Charges(obj), Capacity(obj, spec)).ToString();
                else if (sylph)
                    obj.modData[ChargesKey] = Math.Min(Capacity(obj, spec), Charges(obj) + 1).ToString();
            }
            return true;
        });
    }

    /// <summary>The hover line for a source.</summary>
    public static string? Describe(SObject source, ApparatusRow apparatus)
    {
        if (apparatus.Power is not { } spec)
            return null;
        return spec.Mode == "Charges"
            ? ModServices.T("tooltip.charges", new { have = Charges(source), max = Capacity(source, spec) })
            : MinutesLeft(source) is int left and > 0
                ? ModServices.T("tooltip.powered", new { hours = (left + 59) / 60 })
                : ModServices.T("tooltip.unpowered");
    }
}
