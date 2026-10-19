using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;
using ValleyOpus.Engine.Model;
using SObject = StardewValley.Object;

namespace ValleyOpus.Spikes;

/// <summary>
/// S1 and S19 (doc 13 §6, doc 09 §1a): our staging tray calls the vanilla output path with the last item as the
/// trigger, the <c>Data/Machines</c> <c>OutputMethod</c> returns our match and sets our minutes, and vanilla
/// <c>ReadyTimeModifiers</c> (other mods' speed edits) apply on top. The spike recipe is Wood + Stone → Coal,
/// 120 minutes; the real resolver replaces it in batch B2.
/// </summary>
internal sealed class MachineSpike : ISpike
{
    public const string ApparatusId = "CharlesDR.ValleyOpus_Spike_Apparatus";
    public const string FastApparatusId = "CharlesDR.ValleyOpus_Spike_ApparatusFast";
    public const string TrayKey = Ids.KeyPrefix + "Tray";
    public const string LockedKey = Ids.KeyPrefix + "Locked";
    public const int RecipeMinutes = 120;

    private static readonly string[] RecipeInputs = { "(O)388", "(O)390" }; // Wood, Stone
    private const string RecipeOutput = "(O)382"; // Coal

    private static IMonitor? monitor;

    /// <summary>Counts items staged through vanilla's auto-load (Automate or a hopper), for spike S2.</summary>
    public static int AutomatedDropIns { get; private set; }

    public string Id => "S1";

    public string Summary => "Staging calls the vanilla output path; OutputMethod returns our match. Also runs S19 (ReadyTimeModifiers).";

    public static void Patch(Harmony harmony, IMonitor log)
    {
        monitor = log;
        harmony.Patch(
            original: AccessTools.Method(typeof(SObject), nameof(SObject.performObjectDropInAction)),
            prefix: new HarmonyMethod(typeof(MachineSpike), nameof(DropInPrefix)));
    }

    public static bool IsSpikeApparatus(SObject? obj)
        => obj != null && obj.bigCraftable.Value && (obj.ItemId == ApparatusId || obj.ItemId == FastApparatusId);

    public void Run(SpikeLog log, string[] args)
    {
        this.Check(log, ApparatusId, expectedMinutes: RecipeMinutes, label: "S1");
        this.Check(log, FastApparatusId, expectedMinutes: RecipeMinutes / 2, label: "S19");
        log.Look("by hand: place a Spike Apparatus, hold Wood and click it, then Stone; it should start making Coal (2 hours) and look like any vanilla machine");
    }

    private void Check(SpikeLog log, string itemId, int expectedMinutes, string label)
    {
        GameLocation location = Game1.currentLocation;
        Vector2 tile = this.FreeTileNear(location, SpikeRunner.FacingTile());
        var machine = (SObject)ItemRegistry.Create("(BC)" + itemId);
        machine.TileLocation = tile;
        location.objects.Add(tile, machine);
        log.Note($"{label}: placed {itemId} at {tile}");

        Item wood = ItemRegistry.Create(RecipeInputs[0]);
        Item stone = ItemRegistry.Create(RecipeInputs[1]);

        if (!machine.performObjectDropInAction(wood, probe: false, Game1.player))
        {
            log.Fail($"{label}: the first item wasn't staged");
            return;
        }
        if (machine.heldObject.Value != null)
        {
            log.Fail($"{label}: the machine started on one item; the tray should wait for the whole recipe");
            return;
        }

        bool probed = machine.performObjectDropInAction(stone, probe: true, Game1.player);
        if (!probed || machine.heldObject.Value != null || Tray(machine).Count != 1)
        {
            log.Fail($"{label}: probe changed the machine or refused a valid item (accepted={probed}, held={machine.heldObject.Value?.QualifiedItemId}, tray={Tray(machine).Count})");
            return;
        }

        bool placed = machine.performObjectDropInAction(stone, probe: false, Game1.player);
        SObject? held = machine.heldObject.Value;
        string state = $"placed={placed}, held={held?.QualifiedItemId ?? "nothing"}, minutes={machine.MinutesUntilReady}, tray={Tray(machine).Count}";
        if (!placed || held?.QualifiedItemId != RecipeOutput)
        {
            log.Fail($"{label}: the output path didn't return our match ({state})");
            return;
        }

        if (label == "S1")
        {
            if (machine.MinutesUntilReady == RecipeMinutes)
                log.Pass($"{label}: staging called the output path, OutputMethod returned Coal and set our {RecipeMinutes} minutes ({state})");
            else
                log.Fail($"{label}: output came back but minutes are {machine.MinutesUntilReady}, not our {RecipeMinutes} ({state})");
        }
        else if (machine.MinutesUntilReady == expectedMinutes)
            log.Pass($"{label}: a x0.5 ReadyTimeModifier applied on top of our {RecipeMinutes} minutes ({state})");
        else if (machine.MinutesUntilReady == RecipeMinutes)
            log.Fail($"{label}: ReadyTimeModifiers did NOT apply on top of OutputMethod minutes; fallback: apply them ourselves ({state})");
        else
            log.Fail($"{label}: unexpected minutes ({state})");
    }

    private Vector2 FreeTileNear(GameLocation location, Vector2 start)
    {
        for (int radius = 0; radius < 6; radius++)
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var tile = new Vector2(start.X + dx, start.Y + dy);
                    if (!location.isObjectAtTile((int)tile.X, (int)tile.Y) && location.isTilePassable(tile))
                        return tile;
                }
        return start;
    }

    public static List<string> Tray(SObject machine)
        => machine.modData.TryGetValue(TrayKey, out string? raw) && raw.Length > 0 ? raw.Split(',').ToList() : new List<string>();

    private static void SetTray(SObject machine, List<string> tray)
    {
        if (tray.Count == 0)
            machine.modData.Remove(TrayKey);
        else
            machine.modData[TrayKey] = string.Join(",", tray);
    }

    /// <summary>Whether adding the item keeps the tray on its way to Wood + Stone.</summary>
    private static bool Extends(List<string> tray, Item item)
        => RecipeInputs.Contains(item.QualifiedItemId) && !tray.Contains(item.QualifiedItemId);

    /// <summary>
    /// The staging layer (Harmony patch 1 of doc 09 §14): for our machines only, hold items in the tray until the
    /// recipe is complete, then call the vanilla output path with the last item as the trigger.
    /// </summary>
    private static bool DropInPrefix(SObject __instance, Item dropInItem, bool probe, Farmer who, ref bool __result)
    {
        if (!IsSpikeApparatus(__instance))
            return true;

        try
        {
            // a held-out locked output waits for hand collection (S2, D25)
            if (__instance.heldObject.Value != null || dropInItem is null || AutomateSpike.HasHeldOutput(__instance))
            {
                __result = false;
                return false;
            }

            List<string> tray = Tray(__instance);
            if (!Extends(tray, dropInItem))
            {
                __result = false;
                return false;
            }

            if (tray.Count + 1 < RecipeInputs.Length)
            {
                if (!probe)
                {
                    if (AutomateSpike.AutoLoading)
                        AutomatedDropIns++;
                    tray.Add(dropInItem.QualifiedItemId);
                    SetTray(__instance, tray);
                    SObject.ConsumeInventoryItem(who, dropInItem, 1);
                    __instance.Location?.playSound("Ship");
                }
                __result = true;
                return false;
            }

            MachineData? data = __instance.GetMachineData();
            if (data is null)
            {
                monitor?.Log($"{__instance.QualifiedItemId} has no Data/Machines entry", LogLevel.Error);
                __result = false;
                return false;
            }

            bool placed = __instance.PlaceInMachine(data, dropInItem, probe, who);
            if (placed && !probe)
            {
                if (AutomateSpike.AutoLoading)
                    AutomatedDropIns++;
                SetTray(__instance, new List<string>());
            }
            __result = placed;
            return false;
        }
        catch (Exception ex)
        {
            monitor?.Log($"staging failed on {__instance.QualifiedItemId} at {__instance.TileLocation}: {ex}", LogLevel.Error);
            __result = false;
            return false;
        }
    }

    /// <summary>
    /// The <c>OutputMethod</c> on the spike apparatus's <c>Data/Machines</c> rule: returns the tray's match, or
    /// nothing. Never changes anything when <paramref name="probe"/> is set.
    /// </summary>
    public static Item? OutputMethod(SObject machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
    {
        overrideMinutesUntilReady = null;
        List<string> loaded = Tray(machine).Append(inputItem.QualifiedItemId).OrderBy(i => i).ToList();
        if (!loaded.SequenceEqual(RecipeInputs.OrderBy(i => i)))
            return null;

        overrideMinutesUntilReady = RecipeMinutes;
        if (!probe)
            monitor?.Log($"OutputMethod: {machine.QualifiedItemId} at {machine.TileLocation} makes {RecipeOutput} in {RecipeMinutes} minutes", LogLevel.Trace);
        return ItemRegistry.Create(RecipeOutput);
    }
}
