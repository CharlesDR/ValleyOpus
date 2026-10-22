using Microsoft.Xna.Framework;
using StardewValley;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.Machines;

/// <summary>
/// What an apparatus does when a cycle finishes (doc 07, doc 16 §5): arcane apparatus glow and shimmer with a chime,
/// engineered ones give a clunk and a puff of steam, and blended ones both. Drawn with the game's own effect sheet and
/// sounds, shown to everyone in the location, and only while someone is there to see it, so a Lab that finished
/// overnight doesn't burst all at once when the player walks in.
/// </summary>
internal static class Feedback
{
    private const int SoundGapTicks = 45;
    private static readonly Dictionary<string, int> LastSound = new();

    public static void CycleDone(SObject machine)
    {
        if (!ModServices.Config.MachineFeedback || machine.Location is not GameLocation location || location.farmers.Count == 0)
            return;
        string style = ModServices.Content.ApparatusOf(machine)?.FeedbackStyle ?? "Arcane";
        bool arcane = style is "Arcane" or "Blended";
        bool mechanical = style is "Mechanical" or "Blended";
        // The machine's sprite is two tiles tall; its top is a tile above its tile position.
        Vector2 top = machine.TileLocation * 64f + new Vector2(0, -64);
        float depth = (machine.TileLocation.Y * 64f + 65) / 10000f;
        var sprites = new List<TemporaryAnimatedSprite>();
        if (arcane)
        {
            sprites.Add(new TemporaryAnimatedSprite(10, top + new Vector2(0, -8), Color.LightCyan * 0.9f, 8, false, 60f) { scale = 0.75f, layerDepth = depth });
            sprites.Add(new TemporaryAnimatedSprite(11, top + new Vector2(-20, 12), Color.White, 8, false, 70f) { scale = 0.5f, delayBeforeAnimationStart = 120, layerDepth = depth });
            sprites.Add(new TemporaryAnimatedSprite(11, top + new Vector2(20, -20), Color.White, 8, false, 70f) { scale = 0.5f, delayBeforeAnimationStart = 240, layerDepth = depth });
        }
        if (mechanical)
        {
            sprites.Add(new TemporaryAnimatedSprite(27, top + new Vector2(8, -24), Color.White * 0.8f, 8, false, 80f) { scale = 0.75f, motion = new Vector2(0, -0.5f), layerDepth = depth });
            sprites.Add(new TemporaryAnimatedSprite(5, top + new Vector2(-12, 16), Color.LightGray * 0.7f, 8, false, 70f) { scale = 0.5f, layerDepth = depth });
        }
        Game1.Multiplayer.broadcastSprites(location, sprites.ToArray());

        // One sound per location at a time: a row of machines finishing together makes one chime or clunk.
        if (LastSound.TryGetValue(location.NameOrUniqueName, out int last) && Game1.ticks - last < SoundGapTicks)
            return;
        LastSound[location.NameOrUniqueName] = Game1.ticks;
        if (mechanical)
            location.playSound("clank", machine.TileLocation);
        if (arcane)
            location.playSound("crystal", machine.TileLocation, pitch: mechanical ? 1800 : 1200);
    }
}
