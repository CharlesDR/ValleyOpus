using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;

namespace ValleyOpus.World;

/// <summary>
/// Finch's wagon stands in Cindersap Forest beside the travelling cart's spot (Charles, SOS Pass B: out in the world,
/// not the workshop). The spot is read from the Forest map itself (vanilla's <c>TravelingCartPosition</c>), so it
/// follows SVE or any other mod that moves the cart; Finch is moved to the nearest open tile each morning.
/// </summary>
internal static class FinchWagon
{
    private const string FinchName = Ids.RowPrefix + "Finch";
    private const int WagonSpriteId = 0x5F1C7;
    private static readonly Rectangle WagonSource = new(142, 1382, 109, 70);

    /// <summary>Where Finch stands, relative to the travelling cart's tile: just east of the cart.</summary>
    private static readonly Point Offset = new(10, 1);

    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += (_, _) =>
        {
            if (Context.IsMainPlayer)
                PlaceFinch();
            if (Game1.currentLocation is Forest forest)
                AddWagonSprite(forest);
        };
        helper.Events.Player.Warped += (_, e) =>
        {
            if (e.IsLocalPlayer && e.NewLocation is Forest forest)
                AddWagonSprite(forest);
        };
    }

    private static Forest? TheForest => Game1.getLocationFromName("Forest") as Forest;

    /// <summary>Puts Finch on the nearest open tile to his spot (host only; NPC positions sync to farmhands).</summary>
    public static void PlaceFinch()
    {
        try
        {
            if (TheForest is not Forest forest || Game1.getCharacterFromName(FinchName) is not NPC finch)
                return;
            Point tile = FindSpot(forest);
            if (finch.currentLocation != forest || finch.TilePoint != tile)
                Game1.warpCharacter(finch, forest, new Vector2(tile.X, tile.Y));
            finch.faceDirection(2);
            ModServices.Log.Trace($"Finch's wagon is in the Forest at {tile}.");
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"Couldn't place Finch's wagon: {ex}");
        }
    }

    /// <summary>The open tile nearest the cart's spot plus the offset, searched outward in rings.</summary>
    private static Point FindSpot(Forest forest)
    {
        Point cart = forest.GetTravelingMerchantCartTile();
        var wanted = new Point(cart.X + Offset.X, cart.Y + Offset.Y);
        for (int radius = 0; radius <= 10; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius)
                        continue;
                    var tile = new Point(wanted.X + dx, wanted.Y + dy);
                    if (IsOpen(forest, tile) && IsOpen(forest, new Point(tile.X, tile.Y + 1)))
                        return tile;
                }
            }
        }
        return wanted;
    }

    private static bool IsOpen(GameLocation location, Point tile)
        => location.isTileOnMap(tile.X, tile.Y)
            && location.isTilePassable(new xTile.Dimensions.Location(tile.X, tile.Y), Game1.viewport)
            && !location.IsTileOccupiedBy(new Vector2(tile.X, tile.Y), CollisionMask.Buildings | CollisionMask.Furniture | CollisionMask.Objects | CollisionMask.TerrainFeatures);

    /// <summary>
    /// Draws the wagon behind Finch: vanilla's cart, referenced from the game's own Cursors sheet at load time (never
    /// copied) and tinted so it reads as a different wagon. It's a world sprite, so it sorts with Finch and the player.
    /// </summary>
    private static void AddWagonSprite(Forest forest)
    {
        try
        {
            forest.removeTemporarySpritesWithIDLocal(WagonSpriteId);
            if (!ModServices.World.IsUnlocked(WorldService.Full("FinchRoute")) || forest.getCharacterFromName(FinchName) is not NPC finch)
                return;
            Point tile = finch.TilePoint;
            var position = new Vector2(tile.X * 64 - WagonSource.Width * 2 + 32, tile.Y * 64 - WagonSource.Height * 4 + 16);
            forest.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", WagonSource, position, false, 0f, new Color(210, 180, 255))
            {
                id = WagonSpriteId,
                scale = 4f,
                animationLength = 1,
                interval = 999999f,
                totalNumberOfLoops = 9999,
                layerDepth = (tile.Y * 64 - 8) / 10000f,
            });
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"Couldn't draw Finch's wagon: {ex}");
        }
    }
}
