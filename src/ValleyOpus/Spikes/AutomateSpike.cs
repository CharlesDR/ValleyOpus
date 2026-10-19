using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;
#if AUTOMATE
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
#endif

namespace ValleyOpus.Spikes;

/// <summary>
/// S2 (doc 13 §6.3): does our Automate factory get our machines, or does Automate's generic Data/Machines
/// handler take them first; and are locked outputs refused? The desk check of Automate's source says the
/// generic handler wins (its default factory is asked first, and a claimed machine never reaches ours), so
/// this spike also carries the candidate fallback, a narrow patch on Automate's default factory, behind
/// <c>valleyopus_spike S2 patch</c>.
/// </summary>
internal sealed class AutomateSpike : ISpike
{
    private static IMonitor? monitor;
    private static bool patchActive;

    /// <summary>How many times Automate asked our factory about a spike apparatus.</summary>
    public static int FactoryCalls { get; set; }

    private readonly IModHelper helper;
    private readonly Harmony harmony;

    public AutomateSpike(IModHelper helper, Harmony harmony, IMonitor log)
    {
        this.helper = helper;
        this.harmony = harmony;
        monitor = log;
    }

    public string Id => "S2";

    public string Summary => "Automate: our factory vs its generic handler; locked outputs. Args: (none) | lock | patch";

    public static bool IsLocked(SObject machine) => machine.modData.ContainsKey(MachineSpike.LockedKey);

    /// <summary>Registers our factory when Automate is installed (doc 09 §9). Call on GameLaunched.</summary>
    public void Register()
    {
#if AUTOMATE
        IAutomateAPI? api = this.helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
        if (api is null)
        {
            monitor?.Log("Automate isn't installed; the Automate integration is off.", LogLevel.Trace);
            return;
        }
        api.AddFactory(new SpikeFactory());
        monitor?.Log("Automate integration on: registered the ValleyOpus automation factory.", LogLevel.Info);
#else
        if (this.helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
            monitor?.Log("Automate is installed, but this build was compiled without it; rebuild with Automate in the game's Mods folder.", LogLevel.Warn);
#endif
    }

    public void Run(SpikeLog log, string[] args)
    {
        if (!this.helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
        {
            log.Fail("Automate isn't installed; run S2 in test configuration 3");
            return;
        }
#if !AUTOMATE
        log.Fail("this build was compiled without Automate.dll; rebuild with Automate installed");
#else
        string mode = args.Length > 0 ? args[0] : "";
        if (mode == "lock")
        {
            if (Game1.currentLocation.getObjectAtTile((int)SpikeRunner.FacingTile().X, (int)SpikeRunner.FacingTile().Y) is SObject machine && MachineSpike.IsSpikeApparatus(machine))
            {
                machine.modData[MachineSpike.LockedKey] = "true";
                log.Look($"locked the output of the apparatus at {machine.TileLocation}. When it's ready, does Automate move the output into the chest? It should not.");
            }
            else
                log.Error("face a Spike Apparatus first");
            return;
        }

        if (mode == "patch")
        {
            this.ApplyFallbackPatch(log);
            return;
        }

        log.Note($"factory asked about our apparatus {FactoryCalls} time(s); Automate loaded items through the vanilla drop-in path {MachineSpike.AutomateDropIns} time(s); fallback patch {(patchActive ? "on" : "off")}");
        if (FactoryCalls > 0)
            log.Pass("Automate asked our factory about our machines, so the generic handler didn't claim them");
        else if (MachineSpike.AutomateDropIns > 0)
            log.Fail("Automate's generic Data/Machines handler is running our machines (it loads through the vanilla drop-in path); our factory is never asked. Try 'valleyopus_spike S2 patch'.");
        else
            log.Look("no Automate activity yet: put a Spike Apparatus next to a chest holding Wood and Stone, wait an in-game hour, then run S2 again");
#endif
    }

#if AUTOMATE
    /// <summary>Candidate fallback: make Automate's default factory skip our machines, so ours is asked next.</summary>
    private void ApplyFallbackPatch(SpikeLog log)
    {
        if (patchActive)
        {
            log.Note("the fallback patch is already on");
            return;
        }
        Type? type = AccessTools.TypeByName("Pathoschild.Stardew.Automate.Framework.AutomationFactory");
        var method = type is null ? null : AccessTools.Method(type, "GetFor", new[] { typeof(SObject), typeof(GameLocation), typeof(Vector2).MakeByRefType() });
        if (method is null)
        {
            log.Fail("couldn't find Automate's default factory method; this Automate version differs from the desk check");
            return;
        }
        this.harmony.Patch(method, prefix: new HarmonyMethod(typeof(AutomateSpike), nameof(DefaultFactoryPrefix)));
        patchActive = true;
        monitor?.Log("This mod patches Automate (spike S2). If you notice issues with Automate, check without ValleyOpus before reporting them to Automate.", LogLevel.Info);
        log.Look("fallback patch on. Run 'automate reset' (Automate's own command), wait an in-game hour by the apparatus and chest, then run S2 again");
    }

    private static bool DefaultFactoryPrefix(SObject obj, ref IAutomatable? __result)
    {
        if (!MachineSpike.IsSpikeApparatus(obj))
            return true;
        __result = null;
        return false;
    }

    /// <summary>Our factory: claims the spike apparatus.</summary>
    private sealed class SpikeFactory : IAutomationFactory
    {
        public IAutomatable? GetFor(SObject obj, GameLocation location, in Vector2 tile)
        {
            if (!MachineSpike.IsSpikeApparatus(obj))
                return null;
            FactoryCalls++;
            return new SpikeMachine(obj, location, tile);
        }

        public IAutomatable? GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile) => null;
        public IAutomatable? GetFor(Building building, GameLocation location, in Vector2 tile) => null;
        public IAutomatable? GetForTile(GameLocation location, in Vector2 tile) => null;
    }

    /// <summary>Our machine wrapper: loads Wood + Stone through the staging layer; refuses locked output.</summary>
    private sealed class SpikeMachine : IMachine
    {
        private readonly SObject machine;

        public SpikeMachine(SObject machine, GameLocation location, Vector2 tile)
        {
            this.machine = machine;
            this.Location = location;
            this.TileArea = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
        }

        public GameLocation Location { get; }
        public Rectangle TileArea { get; }
        public string MachineTypeID => "CharlesDR.ValleyOpus/SpikeApparatus";

        public MachineState GetState()
        {
            if (this.machine.heldObject.Value is null)
                return MachineState.Empty;
            if (!this.machine.readyForHarvest.Value)
                return MachineState.Processing;
            return IsLocked(this.machine) ? MachineState.Processing : MachineState.Done;
        }

        public ITrackedStack? GetOutput()
        {
            if (IsLocked(this.machine) || this.machine.heldObject.Value is not SObject output)
                return null;
            return new TrackedItem(output, onEmpty: _ =>
            {
                this.machine.heldObject.Value = null;
                this.machine.readyForHarvest.Value = false;
            });
        }

        public bool SetInput(IStorage input)
        {
            if (!input.TryGetIngredient(i => i.Sample.QualifiedItemId == "(O)388", 1, out IConsumable? wood)
                || !input.TryGetIngredient(i => i.Sample.QualifiedItemId == "(O)390", 1, out IConsumable? stone))
                return false;

            Item? first = wood.Take();
            Item? last = stone.Take();
            if (first is null || last is null)
                return false;
            return this.machine.performObjectDropInAction(first, false, Game1.player)
                && this.machine.performObjectDropInAction(last, false, Game1.player);
        }
    }
#endif
}
