using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using SObject = StardewValley.Object;

namespace ValleyOpus.Spikes;

/// <summary>
/// S3 (doc 17 §6): a building whose data goes missing is dropped cleanly on load, and our BigCraftables that
/// became Error Items turn back into machines on reinstall. Run <c>build</c>, save, remove the mod and the spike
/// pack, load and save, reinstall, load, then <c>check</c> (TESTING.md has the steps).
/// </summary>
internal sealed class BuildingSpike : ISpike
{
    public const string LabId = "CharlesDR.ValleyOpus_Spike_Lab";

    public string Id => "S3";

    public string Summary => "Removing the mod: missing building dropped, Error Items become machines again. Args: build | check";

    public void Run(SpikeLog log, string[] args)
    {
        switch (args.FirstOrDefault())
        {
            case "build":
                this.Build(log);
                break;
            case "check":
                this.Check(log);
                break;
            default:
                log.Error("use 'valleyopus_spike S3 build' first, then 'valleyopus_spike S3 check' after the removal round trip");
                break;
        }
    }

    private void Build(SpikeLog log)
    {
        Farm farm = Game1.getFarm();
        if (Game1.currentLocation != farm)
        {
            log.Error("stand on the farm first");
            return;
        }

        Vector2 origin = Game1.player.Tile + new Vector2(2, -4);
        if (!farm.buildStructure(LabId, origin, Game1.player, out Building? lab, skipSafetyChecks: true) || lab is null)
        {
            log.Fail($"couldn't build the spike Lab at {origin}; move to an open area and retry");
            return;
        }

        int placed = 0;
        GameLocation? inside = lab.GetIndoors();
        foreach (GameLocation location in new[] { farm, inside }.OfType<GameLocation>())
        {
            Vector2 tile = location == farm ? origin + new Vector2(-2, 4) : new Vector2(3, 5);
            if (location.isObjectAtTile((int)tile.X, (int)tile.Y))
                continue;
            var machine = (SObject)ItemRegistry.Create("(BC)" + MachineSpike.ApparatusId);
            machine.TileLocation = tile;
            location.objects.Add(tile, machine);
            placed++;
        }
        log.Look($"built the spike Lab at {origin} and placed {placed} Spike Apparatus (one outside, one inside). Now sleep to save, quit, remove the ValleyOpus folders and follow TESTING.md");
    }

    private void Check(SpikeLog log)
    {
        int labs = Game1.getFarm().buildings.Count(b => b.buildingType.Value == LabId);
        log.Note($"spike Labs on the farm now: {labs} (none expected after a save made without the mod: the building is dropped)");

        int machines = 0, working = 0;
        foreach (GameLocation location in AllLocations())
        {
            foreach (SObject obj in location.objects.Values.Where(o => o.ItemId == MachineSpike.ApparatusId))
            {
                machines++;
                bool isMachine = obj.GetMachineData() != null && ItemRegistry.GetData(obj.QualifiedItemId) != null;
                if (isMachine)
                    working++;
                log.Note($"{location.NameOrUniqueName} {obj.TileLocation}: {(isMachine ? "a machine again" : "still an Error Item")}");
            }
        }

        if (machines == 0)
            log.Look("no Spike Apparatus found. If the save was made while the mod was removed, vanilla may have deleted the Error Items: note that as the result");
        else if (working == machines)
            log.Pass($"all {machines} Spike Apparatus are machines again");
        else
            log.Fail($"{machines - working} of {machines} are still Error Items");
        log.Look("did the save load without the mod with no crash and no red errors? Note it in the spike table");
    }

    internal static IEnumerable<GameLocation> AllLocations()
    {
        foreach (GameLocation location in Game1.locations)
        {
            yield return location;
            foreach (Building building in location.buildings)
                if (building.GetIndoors() is GameLocation indoors)
                    yield return indoors;
        }
    }
}

/// <summary>S17 (doc 18): where Robin's menu puts our building, with and without SVE.</summary>
internal sealed class RobinMenuSpike : ISpike
{
    private readonly IModHelper helper;

    public RobinMenuSpike(IModHelper helper) => this.helper = helper;

    public string Id => "S17";

    public string Summary => "Robin's building menu order with SVE's extra buildings";

    public void Run(SpikeLog log, string[] args)
    {
        List<string> robin = Game1.buildingData.Where(p => p.Value.Builder == "Robin").Select(p => p.Key).ToList();
        int index = robin.IndexOf(BuildingSpike.LabId);
        log.Note($"SVE installed: {this.helper.ModRegistry.IsLoaded("FlashShifter.SVECode")}; Robin sells {robin.Count} building types; ours is number {index + 1} in data order");
        log.Look("open Robin's building menu and note which page and position the Spike Lab is on");
    }
}

/// <summary>S12 (doc 17 §4): the Satchel's per-player global inventory persists, and chest mods leave it alone.</summary>
internal sealed class SatchelSpike : ISpike
{
    public string Id => "S12";

    public string Summary => "Per-player global inventory persists across save and load. Args: setup | check";

    private static string Key => $"CharlesDR.ValleyOpus_Satchel_{Game1.player.UniqueMultiplayerID}";

    public void Run(SpikeLog log, string[] args)
    {
        var inventory = Game1.player.team.GetOrCreateGlobalInventory(Key);
        switch (args.FirstOrDefault())
        {
            case "setup":
                inventory.Clear();
                inventory.Add(ItemRegistry.Create("(O)388", 7));
                inventory.Add(ItemRegistry.Create("(O)390", 3));
                log.Look($"put 7 Wood and 3 Stone in global inventory {Key}. Sleep to save, quit to title, load again, then run 'valleyopus_spike S12 check'. Also open Chests Anywhere or Convenient Inventory if installed: the bag should not be listed");
                break;

            case "check":
                int wood = inventory.Where(i => i?.QualifiedItemId == "(O)388").Sum(i => i.Stack);
                int stone = inventory.Where(i => i?.QualifiedItemId == "(O)390").Sum(i => i.Stack);
                if (wood == 7 && stone == 3)
                    log.Pass($"{Key} kept 7 Wood and 3 Stone across save and load");
                else
                    log.Fail($"{Key} has {wood} Wood and {stone} Stone (expected 7 and 3)");
                log.Note("co-op part (farmhands see their own bag) is deferred with the other co-op runs (doc 17 §8b)");
                break;

            default:
                log.Error("use 'valleyopus_spike S12 setup', then save, reload and 'valleyopus_spike S12 check'");
                break;
        }
    }
}

/// <summary>S13 (doc 17 §7a): the Town warp behind the Community Center, with and without SVE.</summary>
internal sealed class WarpSpike : ISpike
{
    public const string WorkshopId = "CharlesDR.ValleyOpus_Spike_Workshop";
    private const string Warp = "MagicWarp " + WorkshopId + " 6 9";
    private static Point? placed; // set by 'place' for this session

    private readonly IModHelper helper;

    public WarpSpike(IModHelper helper) => this.helper = helper;

    // The spike pack's EditMap tile: SVE's tree line covers 52,7, so with SVE it's 53,9 behind the CC roof (52,9 has a bush).
    private Point tile => placed ?? (this.helper.ModRegistry.IsLoaded("FlashShifter.SVECode") ? new Point(53, 9) : new Point(52, 7));

    public string Id => "S13";

    public string Summary => "Town warp behind the CC. Args: (none) | scan | place <x> <y> | back";

    public void Run(SpikeLog log, string[] args)
    {
        GameLocation town = Game1.getLocationFromName("Town");
        switch (args.FirstOrDefault())
        {
            case "back":
                Game1.warpFarmer("Town", tile.X, tile.Y + 1, flip: false);
                return;

            case "scan":
                var open = new List<string>();
                for (int y = 0; y <= 14; y++)
                    for (int x = 44; x <= 66; x++)
                        if (town.isTilePassable(new Vector2(x, y)) && !town.isObjectAtTile(x, y))
                            open.Add($"{x},{y}");
                log.Note($"walkable tiles around the Community Center (x 44-66, y 0-14): {string.Join(" ", open)}");
                return;

            case "place" when args.Length >= 3 && int.TryParse(args[1], out int x2) && int.TryParse(args[2], out int y2):
                placed = new Point(x2, y2);
                town.setTileProperty(x2, y2, "Back", "TouchAction", Warp);
                log.Look($"put the warp on Town {x2},{y2} for this session; walk onto it");
                return;
        }

        bool sve = this.helper.ModRegistry.IsLoaded("FlashShifter.SVECode");
        string? property = town.doesTileHaveProperty(tile.X, tile.Y, "TouchAction", "Back");
        bool walkable = town.isTilePassable(new Vector2(tile.X, tile.Y));
        bool destination = Game1.getLocationFromName(WorkshopId) != null;
        string state = $"SVE {(sve ? "installed" : "not installed")}; tile {tile.X},{tile.Y}: TouchAction '{property ?? "none"}', walkable {walkable}; workshop location exists {destination}";
        if (property == Warp && walkable && destination)
            log.Pass(state);
        else
            log.Fail(state + ". Try 'valleyopus_spike S13 scan' to list open tiles behind the CC, then 'S13 place <x> <y>'");
        log.Look("walk behind the Community Center onto the warp tile: you should land in a shed-like room. 'valleyopus_spike S13 back' returns you");
    }
}

/// <summary>S5 (doc 07, doc 16): the vanilla hold-up pauses the clock in single player.</summary>
internal sealed class HoldUpSpike : ISpike
{
    private readonly IModHelper helper;
    private int ticksLeft;
    private int ticksTimePassed;
    private int startTime;
    private SpikeLog? log;

    public HoldUpSpike(IModHelper helper) => this.helper = helper;

    public string Id => "S5";

    public string Summary => "The vanilla hold-up pauses the clock in single player";

    public void Run(SpikeLog log, string[] args)
    {
        if (Context.IsMultiplayer)
            log.Note("multiplayer session: the clock never pauses here by design; run this solo");
        this.log = log;
        this.startTime = Game1.timeOfDay;
        this.ticksLeft = 120; // two seconds of the hold-up animation, before any message box
        this.ticksTimePassed = 0;
        Game1.player.holdUpItemThenMessage(ItemRegistry.Create("(O)382"), showMessage: true);
        this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
    }

    private void OnUpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
    {
        if (Game1.shouldTimePass())
            this.ticksTimePassed++;
        if (--this.ticksLeft > 0)
            return;

        this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        string state = $"clock ran on {this.ticksTimePassed} of 120 ticks; time {this.startTime} -> {Game1.timeOfDay}";
        if (this.ticksTimePassed == 0)
            this.log?.Pass("the hold-up paused the clock: " + state);
        else
            this.log?.Fail("the clock kept running during the hold-up; fallback: a single-player-only postfix (Harmony patch 6): " + state);
    }
}
