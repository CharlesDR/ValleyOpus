using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;

namespace ValleyOpus.Effects;

/// <summary>
/// The Gloam Lantern and its Shades (doc 04): carried lanterns collect Shades from monsters that fall near the
/// player, split between several lanterns as <c>0.75^log₂(n)</c> each; recipes with a Shades catalyst spend them
/// from the loading player's lanterns (the lantern is kept).
/// </summary>
internal sealed class Shades
{
    public const string LanternId = "(O)" + Ids.RowPrefix + "GloamLantern";
    public const string ShadesKey = Ids.KeyPrefix + "Shades";
    public const string CapacityUnlock = Ids.RowPrefix + "LanternUpgrade";

    private readonly SmapiLog log;

    public Shades(IModHelper helper, SmapiLog log)
    {
        this.log = log;
        helper.Events.World.NpcListChanged += this.OnNpcListChanged;
    }

    /// <summary>Shades one lantern holds: 20, or 40 after the lantern upgrade (doc 14).</summary>
    public static int Capacity => ModServices.World?.IsUnlocked(CapacityUnlock) == true ? 40 : 20;

    public static IEnumerable<Item> Lanterns(Farmer who) => who.Items.Where(i => i?.QualifiedItemId == LanternId);

    public static int Held(Item lantern) => lantern.modData.TryGetValue(ShadesKey, out string? raw) && int.TryParse(raw, out int n) ? n : 0;

    /// <summary>All Shades a player carries.</summary>
    public static int Carried(Farmer who) => Lanterns(who).Sum(Held);

    /// <summary>Spends Shades from carried lanterns, fullest first; false (and nothing spent) when there aren't enough.</summary>
    public static bool Spend(Farmer who, int amount)
    {
        if (amount <= 0)
            return true;
        if (Carried(who) < amount)
            return false;
        foreach (Item lantern in Lanterns(who).OrderByDescending(Held))
        {
            int take = Math.Min(amount, Held(lantern));
            lantern.modData[ShadesKey] = (Held(lantern) - take).ToString();
            amount -= take;
            if (amount == 0)
                break;
        }
        return true;
    }

    private void OnNpcListChanged(object? sender, NpcListChangedEventArgs e)
    {
        if (!e.IsCurrentLocation)
            return;
        try
        {
            foreach (NPC npc in e.Removed)
            {
                if (npc is not Monster monster || monster.Health > 0)
                    continue;
                List<Item> lanterns = Lanterns(Game1.player).ToList();
                if (lanterns.Count == 0)
                    continue;
                double shades = Math.Clamp(monster.MaxHealth / 40.0, 1, 5);
                double each = shades * Math.Pow(0.75, Math.Log2(lanterns.Count)) * (1 + ShadeBonus(Game1.player));
                foreach (Item lantern in lanterns)
                {
                    int gain = (int)Math.Floor(each) + (Game1.random.NextDouble() < each % 1 ? 1 : 0);
                    lantern.modData[ShadesKey] = Math.Min(Capacity, Held(lantern) + gain).ToString();
                }
                Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(4, monster.Position, Color.MediumPurple * 0.8f, 8, false, 60f));
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"Couldn't add Shades after a monster fell: {ex}");
        }
    }

    /// <summary>Potions that add a share to Shades gathered (doc 04): the Ectoplasm-based Spectral buff.</summary>
    private static double ShadeBonus(Farmer who) => EffectService.HasHandler(who, "Spectral") ? 0.25 : 0;
}
