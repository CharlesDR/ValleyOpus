using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Inventories;
using StardewValley.Objects;
using ValleyOpus.Engine.Machines;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Engine.Routes;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using SObject = StardewValley.Object;

namespace ValleyOpus.Routes;

/// <summary>One link the overlay draws: from a machine's spout to the machine or chest it feeds.</summary>
internal sealed record ChainLink(Vector2 From, Vector2 To, bool ToChest, bool Accepts);

/// <summary>
/// The <c>Routes</c> module (doc 13 §7, doc 09 §5c): directional chaining behind one route interface. A ready
/// output goes East into the next apparatus through the same staging layer as every loader, or South into a chest;
/// an idle machine with a remembered recipe pulls from the chest to its South. Host only, budgeted per second.
/// </summary>
internal interface IItemRoute
{
    /// <summary>Moves what it can for one machine; returns how many transfers it made.</summary>
    int Run(SObject machine, GameLocation location);
}

internal sealed class ChainService : IItemRoute
{
    private readonly SmapiLog log;

    public ChainService(IModHelper helper, SmapiLog log)
    {
        this.log = log;
        helper.Events.GameLoop.OneSecondUpdateTicked += this.OnSecond;
    }

    private void OnSecond(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || !Context.IsMainPlayer)
            return;
        int budget = ModServices.Config.ChainBudgetPerSecond;
        try
        {
            foreach (GameLocation location in Locations())
            {
                foreach (SObject machine in location.objects.Values.Where(IsChainable).ToList())
                {
                    budget -= this.Run(machine, location);
                    if (budget <= 0)
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"Chaining failed: {ex}");
        }
    }

    private static IEnumerable<GameLocation> Locations()
    {
        foreach (GameLocation location in Game1.locations)
        {
            yield return location;
            foreach (Building building in location.buildings)
                if (building.GetIndoors() is GameLocation indoors)
                    yield return indoors;
        }
    }

    private static bool IsChainable(SObject obj)
        => ModServices.Content.ApparatusOf(obj) is { Chaining: true, Automation: not AutomationMode.None, Kind: not ApparatusKinds.Table };

    public int Run(SObject machine, GameLocation location)
    {
        Vector2 tile = machine.TileLocation;
        int moves = 0;

        if (machine.readyForHarvest.Value && machine.heldObject.Value is SObject output && !MachineService.HasHeldOutput(machine) && !MachineService.State(machine).Locked)
        {
            Vector2 east = Step(tile, ChainGeometry.Spout);
            Vector2 south = Step(tile, ChainGeometry.Buffer);
            if (location.getObjectAtTile((int)east.X, (int)east.Y) is SObject next && IsChainable(next) && this.Accepts(next, output))
            {
                Item? taken = ModServices.Machines.TakeOutput(machine);
                if (taken != null)
                {
                    var carrier = new Inventory { taken };
                    while (taken.Stack > 0 && carrier.Contains(taken) && ModServices.Machines.TryStage(next, taken, Game1.MasterPlayer, LoadSource.Chaining, carrier))
                        moves++;
                    Item? left = carrier.FirstOrDefault(i => i != null);
                    if (left != null)
                        this.Park(machine, location, south, left);
                }
            }
            else if (location.getObjectAtTile((int)south.X, (int)south.Y) is Chest chest && chest.playerChest.Value)
            {
                Item? taken = ModServices.Machines.TakeOutput(machine);
                if (taken != null)
                {
                    Item? left = chest.addItem(taken);
                    if (left != null)
                        PutBack(machine, left);
                    moves++;
                }
            }
        }

        if (machine.heldObject.Value is null && !MachineService.HasHeldOutput(machine) && MachineService.State(machine).RecipeMemory != null)
        {
            Vector2 south = Step(tile, ChainGeometry.Buffer);
            if (location.getObjectAtTile((int)south.X, (int)south.Y) is Chest source && source.playerChest.Value)
            {
                IInventory items = source.GetItemsForPlayer();
                foreach (Item item in items.Where(i => i != null).ToList())
                {
                    if (machine.heldObject.Value != null)
                        break;
                    while (item.Stack > 0 && items.Contains(item) && machine.heldObject.Value is null
                        && ModServices.Machines.TryStage(machine, item, Game1.MasterPlayer, LoadSource.Chaining, items))
                        moves++;
                }
                items.RemoveEmptySlots();
            }
        }
        return moves;
    }

    private bool Accepts(SObject next, Item item)
        => ModServices.Machines.TryStage(next, item, Game1.MasterPlayer, LoadSource.Chaining, new Inventory { item }, probe: true);

    private void Park(SObject machine, GameLocation location, Vector2 south, Item left)
    {
        if (location.getObjectAtTile((int)south.X, (int)south.Y) is Chest chest && chest.playerChest.Value && chest.addItem(left) is null)
            return;
        PutBack(machine, left);
    }

    /// <summary>Puts a leftover back in the machine's slot, ready for hand or Automate collection.</summary>
    private static void PutBack(SObject machine, Item left)
    {
        if (left is SObject obj && machine.heldObject.Value is null)
        {
            machine.heldObject.Value = obj;
            machine.readyForHarvest.Value = true;
        }
        else
            Game1.createItemDebris(left, machine.TileLocation * 64f, 0, machine.Location);
    }

    private static Vector2 Step(Vector2 tile, Facing facing)
    {
        (int x, int y) = ChainGeometry.Step(((int)tile.X, (int)tile.Y), facing);
        return new Vector2(x, y);
    }

    /// <summary>The links the overlay draws in a location: every apparatus's spout to an apparatus or chest (doc 13 §7).</summary>
    public static IEnumerable<ChainLink> Links(GameLocation location)
    {
        foreach (SObject machine in location.objects.Values.Where(IsChainable))
        {
            Vector2 tile = machine.TileLocation;
            Vector2 east = Step(tile, ChainGeometry.Spout);
            Vector2 south = Step(tile, ChainGeometry.Buffer);
            if (location.getObjectAtTile((int)east.X, (int)east.Y) is SObject next && IsChainable(next))
            {
                bool accepts = machine.heldObject.Value is not SObject held || ModServices.Machines.TryStage(next, held, Game1.MasterPlayer, LoadSource.Chaining, new Inventory { held }, probe: true);
                yield return new ChainLink(tile, east, false, accepts);
            }
            if (location.getObjectAtTile((int)south.X, (int)south.Y) is Chest { playerChest.Value: true })
                yield return new ChainLink(tile, south, true, true);
        }
    }
}
