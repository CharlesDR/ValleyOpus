using StardewValley;
using StardewValley.Buildings;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Engine.Routes;
using ValleyOpus.Framework;
using ValleyOpus.Skill;
using SObject = StardewValley.Object;

namespace ValleyOpus.Machines;

/// <summary>
/// The bonuses a run gets from where it stands and who loaded it (doc 05 combine table, doc 18 §1 and §3): the Lab
/// tier, a bonded Familiar and profession perks. They add, then clamp (speed at most +50%), and never change which
/// recipe runs. Quality chances are rolled once at load, like every other step.
/// </summary>
internal static class Bonuses
{
    public const string LabTierField = Ids.RowPrefix + "LabTier";
    public const string FamiliarKey = Ids.KeyPrefix + "Familiar";

    public const string Mechanist = "Mechanist";
    public const string Mystic = "Mystic";
    public const string Engineer = "Engineer";
    public const string Artificer = "Artificer";
    public const string Hermeticist = "Hermeticist";
    public const string Theurgist = "Theurgist";

    private static readonly HashSet<string> LabKinds = new(StringComparer.Ordinal)
    {
        ApparatusKinds.Engineered, ApparatusKinds.MinorArcane, ApparatusKinds.RitualAltar, ApparatusKinds.RitualCircle,
    };

    /// <summary>Speed and quality chance per Lab tier (doc 18 §1, placeholders).</summary>
    private static readonly (double Speed, double Chance)[] LabTiers = { (0, 0), (0.10, 0.05), (0.15, 0.10), (0.20, 0.15) };

    public sealed record Result(double SpeedMultiplier, int ExtraQualitySteps, int CapRaise, IReadOnlyList<string> Notes);

    /// <summary>The Lab tier of the building a location is inside, or 0.</summary>
    public static int LabTier(GameLocation? location)
    {
        Building? building = location?.ParentBuilding;
        if (building?.GetData()?.CustomFields is { } fields && fields.TryGetValue(LabTierField, out string? raw) && int.TryParse(raw, out int tier))
            return Math.Clamp(tier, 0, 3);
        return 0;
    }

    /// <summary>The local ID of the Familiar bonded to the Lab a location is inside (e.g. <c>Sylph</c>), or null.</summary>
    public static string? FamiliarIn(GameLocation? location)
        => location?.ParentBuilding?.modData.TryGetValue(FamiliarKey, out string? id) == true && !string.IsNullOrEmpty(id) ? id : null;

    public static bool HasProfession(Farmer who, string localId) => AlchemySkill.HasProfession(who, localId);

    /// <summary>The whole bonus for a load; <paramref name="who"/> is the loading player, or the owner for automation.</summary>
    public static Result For(SObject machine, ApparatusRow apparatus, Farmer? who, bool roll = true)
    {
        GameLocation? location = machine.Location;
        var notes = new List<string>();
        double speed = 0, chance = 0;
        int steps = 0, capRaise = 0;

        if (LabKinds.Contains(apparatus.Kind))
        {
            int tier = LabTier(location);
            if (tier > 0)
            {
                speed += LabTiers[tier].Speed;
                chance += LabTiers[tier].Chance;
                notes.Add(ModServices.T("bonus.lab", new { tier, speed = (int)(LabTiers[tier].Speed * 100), chance = (int)(LabTiers[tier].Chance * 100) }));
            }
            if (FamiliarIn(location) is string familiar && FavoursApparatus(familiar, apparatus))
            {
                speed += 0.10;
                chance += 0.10;
                notes.Add(ModServices.T("bonus.familiar", new { name = FamiliarName(familiar) }));
            }
        }

        if (who != null && apparatus.Kind != ApparatusKinds.Specialist && apparatus.Kind != ApparatusKinds.Table)
        {
            foreach (ProfessionRow profession in ModServices.Content.Registry.Professions.All)
            {
                string local = Local(profession.Id);
                if (!HasProfession(who, local))
                    continue;
                foreach (PerkRow perk in profession.Perks)
                {
                    if (!InScope(perk, apparatus) || !BonusHolds(perk, machine))
                        continue;
                    switch (perk.Variable)
                    {
                        case "Speed":
                            speed += perk.Value;
                            break;
                        case "QualityChance":
                            chance += perk.Value;
                            break;
                        case "QualitySteps":
                            steps += (int)perk.Value;
                            break;
                        case "QualityCap":
                            capRaise = Math.Max(capRaise, (int)perk.Value - (apparatus.QualityCap ?? (int)perk.Value));
                            break;
                    }
                }
            }
        }

        speed = Math.Min(0.5, speed);
        if (roll && chance > 0 && Game1.random.NextDouble() < Math.Min(0.5, chance))
            steps++;
        return new Result(1 - speed, steps, Math.Max(0, capRaise), notes);
    }

    /// <summary>Fuel per run for a Calcinator: Engineer (fed) and a Salamander Lab cut it, at least one per interval.</summary>
    public static int FuelCost(SObject machine, int perRun, Farmer? owner, bool fed)
    {
        double factor = 1;
        if (fed && owner != null && HasProfession(owner, Engineer))
            factor *= 0.75;
        if (FamiliarIn(machine.Location) == "Salamander")
            factor *= 0.5;
        double cost = perRun * factor;
        int whole = (int)Math.Floor(cost);
        if (Game1.random.NextDouble() < cost - whole)
            whole++;
        return whole;
    }

    /// <summary>Boiler charge length: Engineer (fed) +25%, an Undine Lab +50% (doc 05, doc 18 §3).</summary>
    public static double ChargeLengthMultiplier(SObject source, bool fed)
    {
        double factor = 1;
        Farmer? owner = Game1.getFarmerMaybeOffline(MachineService.State(source).Owner);
        if (fed && owner != null && HasProfession(owner, Engineer))
            factor *= 1.25;
        if (FamiliarIn(source.Location) == "Undine")
            factor *= 1.5;
        return factor;
    }

    /// <summary>A Gnome Lab saves a catalyst (Shades included) a quarter of the time.</summary>
    public static bool SavesCatalyst(SObject machine) => FamiliarIn(machine.Location) == "Gnome" && Game1.random.NextDouble() < 0.25;

    private static bool FavoursApparatus(string familiar, ApparatusRow apparatus)
    {
        FamiliarRow? row = ModServices.Content.Registry.Familiars.Get(Ids.RowPrefix + familiar);
        return row != null && apparatus.Operations.Any(o => row.Operations.Contains(o.Operation))
            || familiar == "Salamander" && apparatus.Decompose != null;
    }

    public static string FamiliarName(string local)
        => ModServices.Content.Registry.Familiars.Get(Ids.RowPrefix + local)?.NameKey is string key ? ModServices.T(key) : local;

    private static bool InScope(PerkRow perk, ApparatusRow apparatus)
        => perk.Scope.Count == 0 || perk.Scope.Contains(apparatus.Kind) || perk.Scope.Contains(apparatus.Id);

    /// <summary>Situational perks: a powered Boiler beside it (<c>PowerVapor</c>), or a kindled Athanor within two tiles.</summary>
    private static bool BonusHolds(PerkRow perk, SObject machine)
    {
        if (perk.Bonus is null || perk.Bonus == "Fed")
            return perk.Bonus is null; // Fed applies to fuel and charges, handled where they're spent
        if (machine.Location is not GameLocation location)
            return false;
        var tile = ((int)machine.TileLocation.X, (int)machine.TileLocation.Y);
        return perk.Bonus switch
        {
            "PowerVapor" => Power.Read(location, tile, machine).Charged.Contains(Power.Vapor),
            "NearKindled" => ChainGeometry.Within(tile, 2).Any(t => location.getObjectAtTile(t.Item1, t.Item2) is SObject o
                && ModServices.Content.ApparatusOf(o) is { Heat.RequiresFlag: "Kindled" } && MachineService.State(o).Kindled),
            _ => false,
        };
    }

    public static string Local(string id) => id.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) ? id[Ids.RowPrefix.Length..] : id;
}
