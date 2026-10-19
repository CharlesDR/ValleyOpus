using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Machines;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Routes;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.Machines;

/// <summary>
/// The <c>Neighbours</c> module (doc 09 §5b): reads an apparatus's surroundings once into a condition snapshot:
/// its neighbour state (heat from a kindled Athanor), objects and crops within range 2 with their tags, and every
/// game state query our conditions use.
/// </summary>
internal static class Neighbours
{
    public const string DarknessQuery = Ids.RowPrefix + "DARKNESS";
    public const string InLabQuery = Ids.RowPrefix + "IN_LAB";

    /// <summary>The map tile property that makes a tile dark (Lab alcoves, doc 18).</summary>
    public const string DarkTileProperty = "ValleyOpus_Dark";

    private static readonly Dictionary<string, IReadOnlySet<string>> TagCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers our game state queries (doc 11 §3, doc 18).</summary>
    public static void RegisterQueries()
    {
        GameStateQuery.Register(DarknessQuery, (string[] query, GameStateQueryContext context) =>
        {
            if (context.TargetItem is not SObject machine || (machine.Location ?? context.Location) is not GameLocation location)
                return false;
            return location.doesTileHaveProperty((int)machine.TileLocation.X, (int)machine.TileLocation.Y, DarkTileProperty, "Back") != null;
        });
        GameStateQuery.Register(InLabQuery, (string[] query, GameStateQueryContext context) =>
            context.Location?.ParentBuilding?.buildingType.Value?.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) == true);
    }

    public static void ClearCache() => TagCache.Clear();

    /// <summary>The snapshot for one apparatus now.</summary>
    public static ConditionSnapshot Read(SObject machine, ApparatusRow apparatus, Farmer? who)
    {
        GameLocation? location = machine.Location;
        if (location is null)
            return ConditionSnapshot.Empty;
        var tile = ((int)machine.TileLocation.X, (int)machine.TileLocation.Y);

        bool heat = ReceivesHeat(location, tile);
        string? state = NeighbourStates.Resolve(apparatus, new HashSet<string>(), heat);

        var facts = new List<NeighbourFact>();
        foreach ((int x, int y) in ChainGeometry.Within(tile, 2))
        {
            IReadOnlySet<string>? tags = TagsAt(location, x, y);
            if (tags is { Count: > 0 })
                facts.Add(new NeighbourFact(x - tile.Item1, y - tile.Item2, tags));
        }

        var queries = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (ConditionRow row in ModServices.Content.Registry.Conditions.All.Where(c => c.Kind == ConditionKind.Gsq && c.Query != null))
            queries[row.Query!] = GameStateQuery.CheckConditions(row.Query, location, who ?? Game1.player, targetItem: machine);

        return new ConditionSnapshot { NeighbourState = state, Neighbours = facts, Queries = queries };
    }

    /// <summary>
    /// Whether a kindled heat source next to this tile heats it: each heats at most its first two cardinal neighbours
    /// that want heat, in N, E, S, W order (doc 13 §2).
    /// </summary>
    public static bool ReceivesHeat(GameLocation location, (int X, int Y) tile)
    {
        var sources = new List<(int X, int Y)>();
        int maxConsumers = 2;
        foreach ((int x, int y) in ChainGeometry.Cardinals(tile))
        {
            if (location.getObjectAtTile(x, y) is SObject source && ModServices.Content.ApparatusOf(source) is { Heat: { } heatSpec }
                && (heatSpec.RequiresFlag is null || (heatSpec.RequiresFlag == "Kindled" && MachineData.Read(source.modData).Kindled)))
            {
                sources.Add((x, y));
                maxConsumers = heatSpec.MaxConsumers;
            }
        }
        return sources.Count > 0 && ChainGeometry.ReceivesHeat(tile, sources, t => WantsHeat(location, t), maxConsumers);
    }

    /// <summary>An apparatus with a heated neighbour state.</summary>
    public static bool WantsHeat(GameLocation location, (int X, int Y) tile)
        => location.getObjectAtTile(tile.X, tile.Y) is SObject obj
            && ModServices.Content.ApparatusOf(obj) is { } apparatus
            && apparatus.NeighbourStates.Any(s => s.Test.Heat == true);

    /// <summary>Tags of what stands on a tile: a placed object, or a crop's harvest item (flowers by tag).</summary>
    private static IReadOnlySet<string>? TagsAt(GameLocation location, int x, int y)
    {
        var tile = new Vector2(x, y);
        if (location.objects.TryGetValue(tile, out SObject? obj))
        {
            if (obj is IndoorPot pot && pot.hoeDirt.Value?.crop is Crop potted)
                return HarvestTags(potted);
            if (obj.bigCraftable.Value)
                return null;
            return new HashSet<string>(obj.GetContextTags(), StringComparer.OrdinalIgnoreCase);
        }
        if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature? feature) && feature is HoeDirt { crop: Crop crop })
            return HarvestTags(crop);
        return null;
    }

    private static IReadOnlySet<string>? HarvestTags(Crop crop)
    {
        string? harvest = crop.indexOfHarvest.Value;
        if (string.IsNullOrEmpty(harvest) || crop.dead.Value)
            return null;
        if (!TagCache.TryGetValue(harvest, out IReadOnlySet<string>? tags))
            TagCache[harvest] = tags = ItemRegistry.Create(harvest, allowNull: true) is Item item
                ? new HashSet<string>(item.GetContextTags(), StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();
        return tags;
    }
}
