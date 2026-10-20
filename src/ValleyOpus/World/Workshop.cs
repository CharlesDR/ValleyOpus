using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;

namespace ValleyOpus.World;

/// <summary>
/// The workshop under the Community Center (doc 14 §2): one big room reached by the door behind the CC. Each workshop
/// room is a corner of it whose bundle board appears once the room opens, like the vanilla CC's bundles (Charles, SOS
/// Pass B). The boards, the Lab shrine and the door are map actions (vanilla's action registry); the only Town edit is
/// the door tile, used with the action button.
/// </summary>
internal static class Workshop
{
    public const string EnterAction = Ids.RowPrefix + "Enter";
    public const string BundlesAction = Ids.RowPrefix + "Bundles";
    public const string ShrineAction = Ids.RowPrefix + "Shrine";
    public const string HallLocation = Ids.RowPrefix + "Workshop";

    /// <summary>Map property listing each room's board: "&lt;room&gt; &lt;x&gt; &lt;y&gt;" triples (the board's top-left tile).</summary>
    private const string BoardsProperty = "ValleyOpus_Boards";
    private const string BoardSheet = "untitled tile sheet";
    private static readonly int[] BoardTiles = { 241, 242, 273, 274 };

    private const string IntroReadyFlag = Ids.RowPrefix + "IntroReady";

    public static void Register(IModHelper helper)
    {
        GameLocation.RegisterTileAction(EnterAction, (location, args, who, tile) =>
        {
            Enter(who);
            return true;
        });
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
        helper.Events.Player.Warped += (_, e) =>
        {
            if (!e.IsLocalPlayer)
                return;
            if (e.NewLocation.Name == HallLocation)
                RefreshBoards(e.NewLocation);
            // Oriel's intro waits for the next Town visit after the CC opening scene (Charles, SOS Pass A): leaving Town
            // once the door is open arms it, so the two scenes never play back to back.
            if (e.OldLocation.Name == "Town" && e.NewLocation.Name != "Town" && e.Player.mailReceived.Contains("ccDoorUnlock"))
                e.Player.mailReceived.Add(IntroReadyFlag);
        };
        helper.Events.GameLoop.DayStarted += (_, _) => RefreshBoards(Game1.getLocationFromName(HallLocation));
        helper.Events.GameLoop.UpdateTicked += (_, e) =>
        {
            if (e.IsMultipleOf(60) && Context.IsWorldReady && Game1.currentLocation?.Name == HallLocation)
            {
                RefreshBoards(Game1.currentLocation);
                Sparkle(Game1.currentLocation);
            }
        };
    }

    /// <summary>Each room's board tile, read from the workshop map.</summary>
    public static IEnumerable<(string Room, Point Tile)> Boards(GameLocation location)
    {
        if (!location.TryGetMapProperty(BoardsProperty, out string? raw) || raw is null)
            yield break;
        string[] parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i + 2 < parts.Length; i += 3)
            if (int.TryParse(parts[i + 1], out int x) && int.TryParse(parts[i + 2], out int y))
                yield return (parts[i], new Point(x, y));
    }

    /// <summary>Draws in the board of every open room and clears sealed ones (cheap: only changed tiles are touched).</summary>
    public static void RefreshBoards(GameLocation? location)
    {
        if (location?.Map is null || ModServices.World is null || ModServices.Content is null)
            return;
        foreach ((string room, Point tile) in Boards(location))
        {
            bool open = ModServices.World.IsOpen(room);
            bool shown = location.getTileIndexAt(tile.X, tile.Y + 1, "Buildings", BoardSheet) == BoardTiles[2];
            if (open == shown)
                continue;
            for (int dx = 0; dx < 2; dx++)
            {
                if (open)
                {
                    location.setMapTile(tile.X + dx, tile.Y, BoardTiles[dx], "Front", BoardSheet);
                    location.setMapTile(tile.X + dx, tile.Y + 1, BoardTiles[2 + dx], "Buildings", BoardSheet);
                    location.setTileProperty(tile.X + dx, tile.Y + 2, "Buildings", "Action", $"{BundlesAction} {Ids.RowPrefix}{room}");
                }
                else
                {
                    location.removeMapTile(tile.X + dx, tile.Y, "Front");
                    location.removeMapTile(tile.X + dx, tile.Y + 1, "Buildings");
                    location.removeTileProperty(tile.X + dx, tile.Y + 2, "Buildings", "Action");
                }
            }
        }
    }

    /// <summary>A sparkle on each board with work left to do, so it's obvious where to look (Charles, SOS Pass A).</summary>
    private static void Sparkle(GameLocation location)
    {
        foreach ((string room, Point tile) in Boards(location))
        {
            if (ModServices.World.IsOpen(room) && !ModServices.World.IsRestored(room) && Game1.random.NextDouble() < 0.6)
                Utility.addSprinklesToLocation(location, tile.X, tile.Y, 2, 2, 900, 150, Color.White);
        }
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
    private static readonly Vector2[] StarterSpots = { new(24, 16), new(26, 16), new(34, 16), new(36, 16), new(24, 14), new(26, 14), new(36, 14), new(38, 14) };

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
