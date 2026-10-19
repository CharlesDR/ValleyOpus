using System.Diagnostics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using ValleyOpus.Effects;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using SObject = StardewValley.Object;

namespace ValleyOpus.Spikes;

/// <summary>
/// S3 re-run on the B2 fix: build the placeholder Lab with an apparatus and a chest inside, then save, remove the
/// mod, load and save once more, reinstall and check (TESTING.md has the steps).
/// </summary>
internal sealed class BuildingSpike : ISpike
{
    public const string LabId = Ids.RowPrefix + "Spike_Lab";

    public string Id => "S3";

    public string Summary => "Lab strip on save (Charles option C). Args: build | check";

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
                log.Error("use 'valleyopus_spike S3 build' first, then 'valleyopus_spike S3 check' after the steps in TESTING.md");
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
        if (!farm.buildStructure(LabId, origin, Game1.player, out Building? lab, skipSafetyChecks: true) || lab?.GetIndoors() is not GameLocation inside)
        {
            log.Fail($"couldn't build the spike Lab at {origin}; move to an open area and retry");
            return;
        }
        var athanor = (SObject)ItemRegistry.Create("(BC)" + Ids.RowPrefix + "Athanor");
        inside.objects.Add(new Vector2(3, 5), athanor);
        athanor.TileLocation = new Vector2(3, 5);
        var chest = new Chest(playerChest: true, new Vector2(5, 5));
        chest.Items.Add(ItemRegistry.Create("(O)" + Ids.RowPrefix + "Ignis", 4));
        inside.objects.Add(new Vector2(5, 5), chest);
        log.Look($"built the spike Lab at {origin} with an Athanor and a chest (4 Ignis) inside. Follow TESTING.md: sleep, check the Lab is still there, then the removal round trip");
    }

    private void Check(SpikeLog log)
    {
        int labs = Game1.getFarm().buildings.Count(b => b.buildingType.Value == LabId);
        int chestsByHouse = Game1.getFarm().objects.Values.OfType<Chest>().Count(c => c.Items.Any(i => i?.QualifiedItemId == "(BC)" + Ids.RowPrefix + "Athanor"));
        log.Note($"spike Labs on the farm: {labs}; chests by the farmhouse holding the rescued Athanor: {chestsByHouse}");
        if (labs == 1)
            log.Pass("the Lab survived the save (stripped and restored), as expected when the mod stayed installed");
        else if (chestsByHouse > 0)
            log.Pass("after a save played without the mod, the Lab stayed gone and its contents are in chests beside the farmhouse");
        else
            log.Fail("no Lab and no rescue chest; note the steps you took and attach the SMAPI log");
    }
}

/// <summary>S9: frame time with 50 apparatus under Automate and the overlay on (doc 09 §8).</summary>
internal sealed class PerfSpike : ISpike
{
    private readonly IModHelper helper;
    private readonly Stopwatch frame = new();
    private readonly List<double> samples = new();
    private SpikeLog? log;

    public PerfSpike(IModHelper helper) => this.helper = helper;

    public string Id => "S9";

    public string Summary => "Frame time with many apparatus. Args: place | measure";

    public void Run(SpikeLog log, string[] args)
    {
        if (args.FirstOrDefault() == "place")
        {
            int placed = Diagnostics.Commands.PlaceFifty(Game1.player);
            log.Look($"placed {placed} Retorts with chests of Terra below them, set to Saxum. Hold the Satchel so the overlay draws, then run 'valleyopus_spike S9 measure'");
            return;
        }
        this.log = log;
        this.samples.Clear();
        this.helper.Events.Display.Rendering += this.OnRendering;
        this.helper.Events.Display.Rendered += this.OnRendered;
        log.Note("measuring 300 frames...");
    }

    private void OnRendering(object? sender, RenderingEventArgs e) => this.frame.Restart();

    private void OnRendered(object? sender, RenderedEventArgs e)
    {
        this.samples.Add(this.frame.Elapsed.TotalMilliseconds);
        if (this.samples.Count < 300)
            return;
        this.helper.Events.Display.Rendering -= this.OnRendering;
        this.helper.Events.Display.Rendered -= this.OnRendered;
        double average = this.samples.Average(), worst = this.samples.Max();
        string line = $"draw time over 300 frames: average {average:0.00} ms, worst {worst:0.00} ms";
        if (average < 8)
            this.log?.Pass(line);
        else
            this.log?.Fail(line + "; fallback: lower the particle cap, arrows for the hovered chain only");
    }
}

/// <summary>S15: the custom buff handler and our fertilizers through vanilla's queries (doc 22 §4).</summary>
internal sealed class EffectsSpike : ISpike
{
    public string Id => "S15";

    public string Summary => "Undine's Gift buff handler and fertilizer queries. Args: (none)";

    public void Run(SpikeLog log, string[] args)
    {
        Farmer who = Game1.player;
        var ros = (SObject)ItemRegistry.Create("(O)" + Ids.RowPrefix + "RosMaialis");
        foreach (Buff buff in ros.GetFoodOrDrinkBuffs())
            who.applyBuff(buff);
        double chance = EffectService.HandlerChance(who, "UndinesGift");
        if (chance > 0)
            log.Pass($"drinking Ros Maialis applied a buff whose Data/Buffs row names the UndinesGift handler (chance {chance})");
        else
            log.Fail("the Ros Maialis buff isn't active or its Data/Buffs row has no handler field; fallback: per-tick buff checks");

        GameLocation location = who.currentLocation;
        Vector2 tile = who.GetGrabTile();
        if (!location.terrainFeatures.TryGetValue(tile, out TerrainFeature? feature) || feature is not HoeDirt dirt)
        {
            log.Look("face a tilled, empty soil tile and run S15 again to test the fertilizers");
            return;
        }
        dirt.fertilizer.Value = "(O)" + Ids.RowPrefix + "TerraViva";
        float retention = dirt.GetFertilizerWaterRetentionChance();
        int quality = Enumerable.Range(0, 20).Max(_ => dirt.GetFertilizerQualityBoostLevel());
        dirt.fertilizer.Value = null;
        string state = $"Terra Viva on {tile}: water retention {retention}, quality level {quality}";
        if (retention >= 0.33f && quality >= 2)
            log.Pass(state);
        else
            log.Fail(state + " (expected at least 0.33 and 2)");
        log.Look("by hand: hold Caput Mortuum and click tilled soil; it should be placed like vanilla fertilizer. Drink Ros Maialis and water crops: about half the waterings should use no water");
    }
}
