using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.SaveSerialization;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.State;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.State;

/// <summary>
/// The S3 fix (Charles, option C, doc 17 §6): on every save the host takes our buildings out of the world and
/// keeps them, serialized, in the envelope; they go straight back after the save and on load. A save written while
/// the mod is removed therefore has no building it can't read. If the mod comes back after such a save, the Lab
/// stays gone, and everything that was inside it goes into chests beside the farmhouse.
/// </summary>
internal sealed class BuildingStrip
{
    private readonly SmapiLog log;
    private readonly List<(GameLocation Location, Building Building)> removed = new();

    public BuildingStrip(SmapiLog log) => this.log = log;

    private static bool IsOurs(Building building) => building.buildingType.Value?.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) == true;

    private static XmlSerializer Serializer => SaveSerializer.GetSerializer(typeof(Building));

    /// <summary>Saving: serialize and remove our buildings.</summary>
    public void BeforeSave(FarmEnvelope envelope)
    {
        envelope.StrippedBuildings.Clear();
        this.removed.Clear();
        foreach (GameLocation location in Game1.locations.Where(l => l.IsBuildableLocation()))
        {
            foreach (Building building in location.buildings.Where(IsOurs).ToList())
            {
                try
                {
                    using var writer = new StringWriter();
                    Serializer.Serialize(writer, building);
                    envelope.StrippedBuildings.Add(new StrippedBuilding
                    {
                        Location = location.NameOrUniqueName,
                        BuildingType = building.buildingType.Value,
                        TileX = building.tileX.Value,
                        TileY = building.tileY.Value,
                        Xml = writer.ToString(),
                    });
                    location.buildings.Remove(building);
                    this.removed.Add((location, building));
                }
                catch (Exception ex)
                {
                    this.log.Error($"Couldn't set aside {building.buildingType.Value} at {building.tileX.Value},{building.tileY.Value} for the save; it stays in the world (removing the mod may then drop it). {ex}");
                }
            }
        }
        envelope.StrippedAtDay = (int)Game1.stats.DaysPlayed;
        if (this.removed.Count > 0)
            this.log.Trace($"Set aside {this.removed.Count} building(s) for the save.");
    }

    /// <summary>Saved: put the same building instances straight back.</summary>
    public void AfterSave(FarmEnvelope envelope)
    {
        foreach ((GameLocation location, Building building) in this.removed)
            if (!location.buildings.Contains(building))
                location.buildings.Add(building);
        this.removed.Clear();
        envelope.StrippedBuildings.Clear();
    }

    /// <summary>
    /// Save loaded: restore our buildings from the envelope. If the save was written on a later day, it was played
    /// without the mod; the Lab stays gone and its contents go to chests by the farmhouse.
    /// </summary>
    public void AfterLoad(FarmEnvelope envelope)
    {
        if (envelope.StrippedBuildings.Count == 0)
            return;

        bool playedWithout = (int)Game1.stats.DaysPlayed != envelope.StrippedAtDay;
        var rescued = new List<Item>();
        foreach (StrippedBuilding stripped in envelope.StrippedBuildings)
        {
            try
            {
                using var reader = XmlReader.Create(new StringReader(stripped.Xml));
                if (Serializer.Deserialize(reader) is not Building building)
                    throw new InvalidOperationException("the XML isn't a building");
                GameLocation? location = Game1.getLocationFromName(stripped.Location);
                if (playedWithout || location is null)
                {
                    rescued.AddRange(Contents(building));
                    continue;
                }
                location.buildings.Add(building);
                building.load();
            }
            catch (Exception ex)
            {
                this.log.Error($"Couldn't restore {stripped.BuildingType} at {stripped.TileX},{stripped.TileY}: {ex}");
            }
        }
        envelope.StrippedBuildings.Clear();

        if (playedWithout)
        {
            int chests = PlaceInChests(rescued);
            this.log.Warn($"This save was played without ValleyOpus after day {envelope.StrippedAtDay}, so its Labs stay gone (doc 17 §6). {rescued.Count} item stack(s) from inside them are in {chests} chest(s) beside the farmhouse.");
            if (rescued.Count > 0)
                Game1.addMailForTomorrow(Ids.RowPrefix + "LabContentsRescued", noLetter: true);
        }
    }

    /// <summary>Every item inside a building: placed objects (with chest contents), furniture.</summary>
    private static IEnumerable<Item> Contents(Building building)
    {
        GameLocation? indoors = building.GetIndoors();
        if (indoors is null)
            yield break;
        foreach (SObject obj in indoors.objects.Values.ToList())
        {
            if (obj is Chest chest)
                foreach (Item item in chest.Items.Where(i => i != null))
                    yield return item;
            if (obj.heldObject.Value is SObject held && obj is not Chest)
                yield return held;
            obj.heldObject.Value = null;
            if (obj is Chest c)
                c.Items.Clear();
            yield return obj;
        }
        foreach (Furniture furniture in indoors.furniture.ToList())
            yield return furniture;
    }

    /// <summary>Puts items into new chests on free tiles near the farmhouse door; returns how many chests.</summary>
    private static int PlaceInChests(List<Item> items)
    {
        if (items.Count == 0)
            return 0;
        Farm farm = Game1.getFarm();
        Point door = farm.GetMainFarmHouseEntry();
        int chests = 0;
        int index = 0;
        for (int radius = 2; radius < 12 && index < items.Count; radius++)
        {
            for (int dx = -radius; dx <= radius && index < items.Count; dx++)
            {
                var tile = new Vector2(door.X + dx, door.Y + radius);
                if (!farm.CanItemBePlacedHere(tile))
                    continue;
                var chest = new Chest(playerChest: true, tile);
                while (index < items.Count && chest.Items.Count < Chest.capacity)
                    chest.Items.Add(items[index++]);
                farm.objects.Add(tile, chest);
                chests++;
            }
        }
        if (index < items.Count)
            foreach (Item item in items.Skip(index))
                Game1.createItemDebris(item, new Vector2(door.X, door.Y + 2) * 64f, 2, farm);
        return chests;
    }
}
