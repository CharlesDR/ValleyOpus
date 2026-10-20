using Microsoft.Xna.Framework;
using StardewValley;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;

namespace ValleyOpus.World;

/// <summary>
/// The workshop under the Community Center (doc 14 §2): the door behind the CC, the Entrance Hall's room doors, the
/// bundle boards and the Lab shrine, all as map actions on our own maps (vanilla's action registry, no map patches
/// beyond the one door tile in Town, which the player interacts with).
/// </summary>
internal static class Workshop
{
    public const string EnterAction = Ids.RowPrefix + "Enter";
    public const string DoorAction = Ids.RowPrefix + "Door";
    public const string BundlesAction = Ids.RowPrefix + "Bundles";
    public const string ShrineAction = Ids.RowPrefix + "Shrine";
    public const string HallLocation = Ids.RowPrefix + "Workshop_EntranceHall";

    public static void Register()
    {
        GameLocation.RegisterTileAction(EnterAction, (location, args, who, tile) =>
        {
            Enter(who);
            return true;
        });
        GameLocation.RegisterTouchAction(DoorAction, (location, args, who, tile) => Door(who, args.ElementAtOrDefault(1)));
        GameLocation.RegisterTileAction(BundlesAction, (location, args, who, tile) =>
        {
            if (who.IsLocalPlayer && args.ElementAtOrDefault(1) is string room)
                UI.BundleMenu.Open(room);
            return true;
        });
        GameLocation.RegisterTileAction(ShrineAction, (location, args, who, tile) =>
        {
            if (who.IsLocalPlayer)
                ModServices.World.Familiars.UseShrine(location);
            return true;
        });
    }

    /// <summary>The door behind the Community Center: opens once the intro is seen (doc 14 §2).</summary>
    public static void Enter(Farmer who)
    {
        if (!who.IsLocalPlayer)
            return;
        if (!ModServices.World.IsUnlocked(WorldService.IntroUnlock))
        {
            Game1.drawObjectDialogue(ModServices.T("workshop.locked"));
            return;
        }
        Warp(who, HallLocation);
    }

    /// <summary>A room door in the Entrance Hall: open rooms let you in, sealed ones say what they wait for.</summary>
    public static void Door(Farmer who, string? roomId)
    {
        if (!who.IsLocalPlayer || roomId is null || ModServices.Content.Registry.Rooms.Get(WorldService.Full(roomId)) is not WorkshopRoomRow room)
            return;
        if (!ModServices.World.IsOpen(room.Id))
        {
            IEnumerable<string> needs = (room.Opens.AllOf ?? new()).Concat(room.Opens.AnyOf ?? new()).Where(r => !ModServices.World.IsRestored(r)).Select(WorldService.RoomName);
            string joiner = room.Opens.AnyOf is { Count: > 0 } ? ModServices.T("workshop.or") : ", ";
            Game1.drawObjectDialogue(ModServices.T("workshop.sealed", new { room = WorldService.RoomName(room.Id), needs = string.Join(joiner, needs) }));
            return;
        }
        Warp(who, room.Location);
    }

    /// <summary>
    /// Sets the intro's apparatus down in the Entrance Hall instead of filling the player's backpack (Charles, SOS Pass A):
    /// they're vanilla machines there, so a pickaxe or axe picks them up. A blocked spot is skipped; with no spot left, the item goes to the host.
    /// </summary>
    public static void PlaceStarterEquipment(IReadOnlyList<string> itemIds)
    {
        if (itemIds.Count == 0)
            return;
        GameLocation? hall = Game1.getLocationFromName(HallLocation);
        if (hall?.Map is null)
        {
            ModServices.Log.Error($"The workshop location {HallLocation} doesn't exist, so the starter apparatus go to the host's backpack instead.");
            Game1.MasterPlayer.addItemsByMenuIfNecessary(itemIds.Select(id => ItemRegistry.Create(id)).ToList());
            return;
        }
        var spots = new Queue<Vector2>(StarterSpots.Where(spot => hall.CanItemBePlacedHere(spot)));
        foreach (string id in itemIds)
        {
            if (ItemRegistry.Create(id, allowNull: true) is StardewValley.Object machine && spots.TryDequeue(out Vector2 spot))
            {
                machine.TileLocation = spot;
                hall.objects.Add(spot, machine);
                ModServices.Log.Trace($"Placed {id} in the Entrance Hall at {spot}.");
            }
            else
                Game1.MasterPlayer.addItemByMenuIfNecessary(ItemRegistry.Create(id));
        }
    }

    /// <summary>Where the intro's apparatus stand in the Entrance Hall: either side of the path from the stairs to the doors.</summary>
    private static readonly Vector2[] StarterSpots = { new(9, 7), new(11, 7), new(19, 7), new(21, 7), new(9, 9), new(11, 9), new(19, 9), new(21, 9) };

    private static void Warp(Farmer who, string locationName)
    {
        GameLocation? target = Game1.getLocationFromName(locationName);
        if (target?.Map is null)
        {
            ModServices.Log.Error($"The workshop location {locationName} doesn't exist; is the base pack installed?");
            return;
        }
        int width = target.Map.Layers[0].LayerWidth, height = target.Map.Layers[0].LayerHeight;
        Point arrival = target.GetData()?.DefaultArrivalTile is Point tile ? tile : new Point(width / 2, height - 3);
        Game1.playSound("doorClose");
        Game1.warpFarmer(locationName, arrival.X, arrival.Y, 0);
    }
}
