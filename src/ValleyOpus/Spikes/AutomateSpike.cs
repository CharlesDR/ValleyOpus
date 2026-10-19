using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using ValleyOpus.Engine.Model;
using SObject = StardewValley.Object;

namespace ValleyOpus.Spikes;

/// <summary>
/// S2 (doc 13 §6.3, D25): Automate runs our machines through its own generic <c>Data/Machines</c> handler
/// (Charles chose option B after the desk check). Its loads go through vanilla's auto-load, which offers chest items
/// one at a time to our drop-in prefix, so the staging tray still decides; locked outputs leave the machine slot the
/// moment they finish, so Automate never collects them. No code here references Automate.
/// </summary>
internal sealed class AutomateSpike : ISpike
{
    /// <summary>The farm-wide global inventory holding locked outputs until they're collected by hand.</summary>
    public const string LockedStoreId = "CharlesDR.ValleyOpus_LockedOutputs";

    /// <summary>On a machine and on its held-out output: the entry that pairs them.</summary>
    public const string HeldKey = Ids.KeyPrefix + "HeldOutput";

    private static IMonitor? monitor;
    private static int autoLoadDepth;

    /// <summary>Whether the current drop-in call comes from vanilla's auto-load (Automate or a hopper).</summary>
    public static bool AutoLoading => autoLoadDepth > 0;

    /// <summary>How many locked outputs were moved out of a machine slot this session.</summary>
    public static int HeldOut { get; private set; }

    private readonly IModHelper helper;

    public AutomateSpike(IModHelper helper) => this.helper = helper;

    public string Id => "S2";

    public string Summary => "Automate's generic handler runs our machines; locked outputs held out of its reach. Args: (none) | lock";

    public static bool IsLocked(SObject machine) => machine.modData.ContainsKey(MachineSpike.LockedKey);

    public static bool HasHeldOutput(SObject machine) => machine.modData.ContainsKey(HeldKey);

    /// <summary>Harmony patches 7 and 8 of doc 09 §14, for our machines only.</summary>
    public static void Patch(Harmony harmony, IMonitor log)
    {
        monitor = log;
        var autoLoad = AccessTools.Method(typeof(SObject), nameof(SObject.AttemptAutoLoad), new[] { typeof(IInventory), typeof(Farmer) });
        if (autoLoad is null)
            log.Log("Couldn't find Object.AttemptAutoLoad; automated loads will count as hand loads (spike S2).", LogLevel.Warn);
        else
            harmony.Patch(autoLoad,
                prefix: new HarmonyMethod(typeof(AutomateSpike), nameof(AutoLoadPrefix)),
                finalizer: new HarmonyMethod(typeof(AutomateSpike), nameof(AutoLoadFinalizer)));

        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            postfix: new HarmonyMethod(typeof(AutomateSpike), nameof(MinutesElapsedPostfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
            prefix: new HarmonyMethod(typeof(AutomateSpike), nameof(CheckForActionPrefix)));
    }

    public void Run(SpikeLog log, string[] args)
    {
        if (args.FirstOrDefault() == "lock")
        {
            var tile = SpikeRunner.FacingTile();
            if (Game1.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y) is SObject machine && MachineSpike.IsSpikeApparatus(machine))
            {
                machine.modData[MachineSpike.LockedKey] = "true";
                log.Look($"locked the outputs of the apparatus at {machine.TileLocation}. When it finishes, the Coal should leave the machine (it looks empty) and never reach Automate's chest; clicking the machine by hand gives it to you");
            }
            else
                log.Error("face a Spike Apparatus first");
            return;
        }

        if (!this.helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
            log.Note("Automate isn't installed; run S2 in test configuration 3");

        var probe = (SObject)ItemRegistry.Create("(BC)" + MachineSpike.ApparatusId);
        log.Note($"spike apparatus has the 'machine_input' context tag Automate requires: {probe.HasContextTag("machine_input")}");
        log.Note($"automated loads that reached the staging tray: {MachineSpike.AutomatedDropIns}; locked outputs held out: {HeldOut}; waiting in the store: {Game1.player.team.GetOrCreateGlobalInventory(LockedStoreId).Count(i => i != null)}");

        if (MachineSpike.AutomatedDropIns >= 2)
            log.Pass("Automate loaded our apparatus through vanilla's auto-load, one item per pass, and our staging tray took each item");
        else if (MachineSpike.AutomatedDropIns == 1)
            log.Look("Automate loaded one item so far; wait another in-game hour (the tray fills one item per pass) and run S2 again");
        else
            log.Look("no automated loads yet: put a Spike Apparatus next to a chest holding Wood and Stone, wait an in-game hour, then run S2 again. If it stays at 0, check the 'machine_input' line above");
        log.Look("also check: the chest lost exactly one Wood and one Stone per Coal, and after 'S2 lock' no Coal ever reached the chest");
    }

    private static void AutoLoadPrefix() => autoLoadDepth++;

    private static void AutoLoadFinalizer() => autoLoadDepth = Math.Max(0, autoLoadDepth - 1);

    /// <summary>Patch 8: a locked output that just finished moves into the store before any automation can see it.</summary>
    private static void MinutesElapsedPostfix(SObject __instance)
    {
        try
        {
            if (!MachineSpike.IsSpikeApparatus(__instance) || !IsLocked(__instance) || HasHeldOutput(__instance)
                || !__instance.readyForHarvest.Value || __instance.heldObject.Value is not SObject output)
                return;

            string entry = Guid.NewGuid().ToString("N");
            output.modData[HeldKey] = entry;
            Game1.player.team.GetOrCreateGlobalInventory(LockedStoreId).Add(output);
            __instance.modData[HeldKey] = entry;
            __instance.heldObject.Value = null;
            __instance.readyForHarvest.Value = false;
            HeldOut++;
            monitor?.Log($"held out locked output {output.QualifiedItemId} from {__instance.QualifiedItemId} at {__instance.TileLocation}", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            monitor?.Log($"holding out a locked output failed on {__instance.QualifiedItemId}: {ex}", LogLevel.Error);
        }
    }

    /// <summary>Hand collection of a held-out output: gives it to the player and frees the machine.</summary>
    private static bool CheckForActionPrefix(SObject __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
    {
        if (!MachineSpike.IsSpikeApparatus(__instance) || !__instance.modData.TryGetValue(HeldKey, out string? entry))
            return true;

        try
        {
            __result = true;
            if (justCheckingForActivity)
                return false;

            var store = Game1.player.team.GetOrCreateGlobalInventory(LockedStoreId);
            Item? output = store.FirstOrDefault(i => i != null && i.modData.TryGetValue(HeldKey, out string? e) && e == entry);
            if (output is null)
            {
                monitor?.Log($"{__instance.QualifiedItemId} at {__instance.TileLocation} pointed to a held output that's gone; freeing the machine", LogLevel.Warn);
                __instance.modData.Remove(HeldKey);
                return false;
            }

            output.modData.Remove(HeldKey);
            if (!who.addItemToInventoryBool(output))
            {
                output.modData[HeldKey] = entry;
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                return false;
            }
            store.Remove(output);
            store.RemoveEmptySlots();
            __instance.modData.Remove(HeldKey);
            who.currentLocation?.playSound("coin");
            return false;
        }
        catch (Exception ex)
        {
            monitor?.Log($"collecting a held output failed on {__instance.QualifiedItemId}: {ex}", LogLevel.Error);
            return true;
        }
    }
}
