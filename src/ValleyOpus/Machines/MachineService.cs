using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using ValleyOpus.Engine;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Machines;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Randomness;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Framework;
using GameMachineData = StardewValley.GameData.Machines.MachineData;
using MachineItemOutput = StardewValley.GameData.Machines.MachineItemOutput;
using SObject = StardewValley.Object;

namespace ValleyOpus.Machines;

/// <summary>
/// The <c>Machines</c> module (doc 09 §5a, doc 13 §6): the staging layer every loader goes through (hand, the
/// Satchel picker, chaining, Automate), the <c>OutputMethod</c> behind every apparatus rule, collection with
/// discovery and XP, locked outputs held out of reach, the Experiment Table's attempts, and kindling.
/// </summary>
internal sealed class MachineService
{
    /// <summary>The farm-wide store locked outputs move to when they finish (doc 13 §6.5, D25).</summary>
    public const string LockedStoreId = Ids.RowPrefix + "LockedOutputs";
    public const string HeldOutputKey = Ids.KeyPrefix + "HeldOutput";
    private const string TrayPrefix = Ids.RowPrefix + "Tray_";

    private static int autoLoadDepth;
    private static LoadSource? sourceOverride;
    private static RunPlan? pendingRun;

    private readonly SmapiLog log;

    public MachineService(SmapiLog log) => this.log = log;

    private static MachineService Self => ModServices.Machines;

    /// <summary>The last refusal reason per machine, for the hover tooltip (doc 16 §2).</summary>
    public Dictionary<SObject, StageRefusal> LastRefusal { get; } = new(ReferenceEqualityComparer.Instance);

    // ── Patches (doc 09 §14: 1 drop-in prefix, 7 AttemptAutoLoad prefix/finalizer, 8 minutesElapsed postfix) ──

    public static void Patch(Harmony harmony, IMonitor monitor)
    {
        harmony.Patch(AccessTools.Method(typeof(SObject), nameof(SObject.performObjectDropInAction)),
            prefix: new HarmonyMethod(typeof(MachineService), nameof(DropInPrefix)));
        harmony.Patch(AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
            prefix: new HarmonyMethod(typeof(MachineService), nameof(CheckForActionPrefix)));
        harmony.Patch(AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            postfix: new HarmonyMethod(typeof(MachineService), nameof(MinutesElapsedPostfix)));

        var autoLoad = AccessTools.Method(typeof(SObject), nameof(SObject.AttemptAutoLoad), new[] { typeof(IInventory), typeof(Farmer) });
        if (autoLoad is null)
            monitor.Log("Couldn't find Object.AttemptAutoLoad; Automate loads will count as hand loads.", LogLevel.Warn);
        else
            harmony.Patch(autoLoad,
                prefix: new HarmonyMethod(typeof(MachineService), nameof(AutoLoadPrefix)),
                finalizer: new HarmonyMethod(typeof(MachineService), nameof(AutoLoadFinalizer)));
    }

    private static void AutoLoadPrefix() => autoLoadDepth++;

    private static void AutoLoadFinalizer() => autoLoadDepth = Math.Max(0, autoLoadDepth - 1);

    private static LoadSource CurrentSource => sourceOverride ?? (autoLoadDepth > 0 ? LoadSource.Automate : LoadSource.Hand);

    // ── State helpers ───────────────────────────────────────────────────────────

    public static MachineData State(SObject machine) => MachineData.Read(machine.modData);

    public static void Save(SObject machine, MachineData state) => state.Write(machine.modData);

    /// <summary>The machine's tray: a global inventory, created on first use (doc 17 §1).</summary>
    public static Inventory Tray(SObject machine, MachineData state, bool create)
    {
        if (state.TrayId is null)
        {
            if (!create)
                return new Inventory();
            state.TrayId = TrayPrefix + Guid.NewGuid().ToString("N");
            Save(machine, state);
        }
        return Game1.player.team.GetOrCreateGlobalInventory(state.TrayId);
    }

    public static IReadOnlyList<Item> TrayItems(SObject machine)
        => Tray(machine, State(machine), create: false).Where(i => i != null).ToList();

    private static List<ItemStack> Stacks(IEnumerable<Item> items)
        => items.Where(i => i != null).Select(i => new ItemStack(GameContent.ToInfo(i), i.Quality, i.Stack)).ToList();

    public MachineContext Context(SObject machine, ApparatusRow apparatus, MachineData state, LoadSource source, Farmer? who)
        => new(apparatus, Neighbours.Read(machine, apparatus, who), source, state.RecipeMemory, apparatus.Kind == ApparatusKinds.Table ? ModServices.Farm.Codex.Data.TableCapacity : null);

    /// <summary>Whether the machine holds a finished locked output in the store.</summary>
    public static bool HasHeldOutput(SObject machine) => machine.modData.ContainsKey(HeldOutputKey);

    // ── Staging ─────────────────────────────────────────────────────────────────

    /// <summary>Offers one item to a machine as a given loader; items are taken from <paramref name="from"/> (null = the player's inventory).</summary>
    public bool TryStage(SObject machine, Item item, Farmer who, LoadSource source, IInventory? from = null, bool probe = false)
    {
        LoadSource? previousSource = sourceOverride;
        IInventory? previousFrom = SObject.autoLoadFrom;
        sourceOverride = source;
        SObject.autoLoadFrom = from;
        try
        {
            return machine.performObjectDropInAction(item, probe, who);
        }
        finally
        {
            sourceOverride = previousSource;
            SObject.autoLoadFrom = previousFrom;
        }
    }

    private static bool DropInPrefix(SObject __instance, Item dropInItem, bool probe, Farmer who, ref bool __result)
    {
        ApparatusRow? apparatus = ModServices.Content?.ApparatusOf(__instance);
        if (apparatus is null)
            return true;
        try
        {
            __result = Self.Stage(__instance, apparatus, dropInItem, probe, who);
        }
        catch (Exception ex)
        {
            Self.log.Error($"Staging failed on {__instance.QualifiedItemId} at {__instance.Location?.NameOrUniqueName} {__instance.TileLocation} (day {Game1.stats.DaysPlayed}): {ex}");
            __result = false;
        }
        return false;
    }

    private bool Stage(SObject machine, ApparatusRow apparatus, Item? item, bool probe, Farmer who)
    {
        LoadSource source = CurrentSource;
        if (item is null || machine.heldObject.Value != null || HasHeldOutput(machine))
            return false;
        if (item.HasContextTag("valleyopus_tool"))
            return false; // the Satchel and Lens act on machines through their own click (doc 16 §4)
        if (apparatus.Kind == ApparatusKinds.Table)
        {
            if (!probe && source == LoadSource.Hand && who.IsLocalPlayer && Game1.activeClickableMenu is null)
                UI.TableMenu.Open(machine);
            return false;
        }

        MachineData state = State(machine);
        Inventory tray = Tray(machine, state, create: !probe);
        List<ItemStack> trayStacks = Stacks(tray);
        MachineContext context = this.Context(machine, apparatus, state, source, who);
        var offered = new ItemStack(GameContent.ToInfo(item), item.Quality, 1);
        StagePlan plan = ModServices.Content.Stager.Offer(context, state, trayStacks, offered, ModServices.Farm.Codex);

        this.Trace(machine, $"{source} offers {item.QualifiedItemId}: {plan.Action}{(plan.Accepted ? "" : " (" + plan.Refusal + ")")}{(probe ? " [probe]" : "")}");
        if (!plan.Accepted)
        {
            this.LastRefusal[machine] = plan.Refusal;
            if (!probe && source == LoadSource.Hand && who.IsLocalPlayer)
                Game1.showRedMessage(Reason(plan.Refusal, plan.FailedConditions));
            return false;
        }
        this.LastRefusal.Remove(machine);
        if (probe)
            return true;

        switch (plan.Action)
        {
            case StageAction.Fuel:
                state.Fuel += 1;
                Save(machine, state);
                SObject.ConsumeInventoryItem(who, item, 1);
                machine.Location?.playSound("fireball");
                return true;

            case StageAction.Stage:
            case StageAction.AwaitSeal:
                Item one = item.getOne();
                Utility.addItemToThisInventoryList(one, tray, listMaxSpace: 36);
                SObject.ConsumeInventoryItem(who, item, 1);
                machine.Location?.playSound("Ship");
                if (plan.Action == StageAction.AwaitSeal && source == LoadSource.Hand && who.IsLocalPlayer)
                    Game1.addHUDMessage(new HUDMessage(ModServices.T("machine.await-seal")) { noIcon = true });
                return true;

            case StageAction.Start:
            {
                List<ItemStack> all = trayStacks.Append(offered).ToList();
                RunPlan run = ModServices.Content.Stager.BuildRecipeRun(context, plan.Match!, all);
                if (run.ProcessState != null)
                {
                    SObject.ConsumeInventoryItem(who, item, 1);
                    tray.Clear();
                    this.FinishProcessState(machine, state, run, who, source);
                    return true;
                }
                return this.Place(machine, state, run, item, who, source, tray);
            }

            case StageAction.Decompose:
            {
                RunPlan? run = ModServices.Content.Stager.BuildDecomposeRun(apparatus, offered, ResidueRoll(machine, Ids.DecomposeRecipe));
                return run != null && this.Place(machine, state, run, item, who, source, tray);
            }
        }
        return false;
    }

    /// <summary>Starts a run through the vanilla output path, so ready-time modifiers and other mods see a normal machine (S1, S19).</summary>
    private bool Place(SObject machine, MachineData state, RunPlan run, Item lastItem, Farmer who, LoadSource source, Inventory tray)
    {
        GameMachineData? data = machine.GetMachineData();
        if (data is null)
        {
            this.log.Error($"{machine.QualifiedItemId} has no Data/Machines rule; is the base pack installed?");
            return false;
        }

        pendingRun = run;
        bool placed;
        try
        {
            placed = machine.PlaceInMachine(data, lastItem, false, who);
        }
        finally
        {
            pendingRun = null;
        }
        if (!placed)
            return false;

        tray.Clear();
        this.StartRun(machine, state, run, who, source);
        return true;
    }

    /// <summary>The vanilla output path's callback: returns the run the staging layer planned (doc 13 §6).</summary>
    public static Item? OutputMethod(SObject machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
    {
        overrideMinutesUntilReady = null;
        if (pendingRun is not RunPlan run || run.OutputItemId is null)
            return null; // only our staging layer starts our apparatus
        overrideMinutesUntilReady = run.Minutes;
        return ItemRegistry.Create(run.OutputItemId, run.Count, run.Quality);
    }

    private void StartRun(SObject machine, MachineData state, RunPlan run, Farmer who, LoadSource source)
    {
        bool manual = source is LoadSource.Hand or LoadSource.Picker;
        state.Recipe = run.RecipeId;
        state.Locked = run.Locked;
        state.HandLoaded = manual;
        state.Bonus = run.Bonus;
        state.Extras = run.Extras.ToList();
        if (manual)
        {
            if (!state.Pinned)
                state.RecipeMemory = run.RecipeId;
            state.Owner = who.UniqueMultiplayerID;
        }
        if (ModServices.Content.ApparatusOf(machine)?.Fuel is { } fuel)
            state.Fuel = Math.Max(0, state.Fuel - fuel.PerRun);
        Save(machine, state);
        this.Trace(machine, $"started {run.RecipeId}: {run.OutputItemId} x{run.Count} q{run.Quality} in {run.Minutes} min{(run.Locked ? ", locked" : "")}{(run.Extras.Count > 0 ? ", extras " + MachineData.FormatExtras(run.Extras) : "")}");
    }

    /// <summary>A run that makes no item: kindling an Athanor with Flamma Viva (doc 13 §2).</summary>
    private void FinishProcessState(SObject machine, MachineData state, RunPlan run, Farmer who, LoadSource source)
    {
        if (ModServices.Content.ApparatusOf(machine)?.Heat?.RequiresFlag == "Kindled")
            state.Kindled = true;
        if (source is LoadSource.Hand or LoadSource.Picker)
            state.Owner = who.UniqueMultiplayerID;
        Save(machine, state);
        machine.Location?.playSound("fireball");
        machine.Location?.temporarySprites.Add(new TemporaryAnimatedSprite(30, machine.TileLocation * 64f + new Vector2(0, -64), Color.OrangeRed, 8, false, 70f));
        this.Discover(who, run.RecipeId, null, run.Tier, null, handMade: true, celebrateItem: null, message: ModServices.T("machine.kindled"));
    }

    // ── Collection ──────────────────────────────────────────────────────────────

    private static bool CheckForActionPrefix(SObject __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
    {
        ApparatusRow? apparatus = ModServices.Content?.ApparatusOf(__instance);
        if (apparatus is null)
            return true;
        try
        {
            __result = Self.Action(__instance, apparatus, who, justCheckingForActivity);
        }
        catch (Exception ex)
        {
            Self.log.Error($"Using {__instance.QualifiedItemId} at {__instance.TileLocation} failed (day {Game1.stats.DaysPlayed}): {ex}");
            __result = false;
        }
        return false;
    }

    private bool Action(SObject machine, ApparatusRow apparatus, Farmer who, bool justChecking)
    {
        if (apparatus.Kind == ApparatusKinds.Table)
        {
            if (!justChecking && who.IsLocalPlayer)
                UI.TableMenu.Open(machine);
            return true;
        }

        if (HasHeldOutput(machine))
        {
            if (!justChecking)
                this.CollectHeld(machine, who);
            return true;
        }
        if (machine.readyForHarvest.Value && machine.heldObject.Value != null)
        {
            if (!justChecking)
                this.CollectByHand(machine, who);
            return true;
        }
        if (justChecking)
            return TrayItems(machine).Count > 0;
        if (who.IsLocalPlayer && machine.heldObject.Value is null)
            UI.ApparatusPanel.Open(machine, pickerMode: false);
        return true;
    }

    /// <summary>A hand collection: the held output and every extra at once, with discovery for a locked first make.</summary>
    private void CollectByHand(SObject machine, Farmer who)
    {
        MachineData state = State(machine);
        var items = new List<Item>();
        if (machine.heldObject.Value is SObject held)
            items.Add(held);
        foreach ((string id, int count) in state.Extras)
            items.Add(ItemRegistry.Create(id, count));
        this.Give(who, items, machine);
        ResetMachine(machine);
        this.AfterCollect(machine, state, who, items, handMade: true);
    }

    /// <summary>Collecting a locked output from the store by hand (doc 13 §6.5).</summary>
    private void CollectHeld(SObject machine, Farmer who)
    {
        string entry = machine.modData[HeldOutputKey];
        Inventory store = Game1.player.team.GetOrCreateGlobalInventory(LockedStoreId);
        Item? output = store.FirstOrDefault(i => i != null && i.modData.TryGetValue(HeldOutputKey, out string? e) && e == entry);
        machine.modData.Remove(HeldOutputKey);
        MachineData state = State(machine);
        var items = new List<Item>();
        if (output is null)
            this.log.Warn($"{machine.QualifiedItemId} at {machine.TileLocation} pointed to a held output that's gone; freeing the machine.");
        else
        {
            store.Remove(output);
            store.RemoveEmptySlots();
            output.modData.Remove(HeldOutputKey);
            items.Add(output);
        }
        foreach ((string id, int count) in state.Extras)
            items.Add(ItemRegistry.Create(id, count));
        this.Give(who, items, machine);
        this.AfterCollect(machine, state, who, items, handMade: true);
    }

    private void Give(Farmer who, List<Item> items, SObject machine)
    {
        foreach (Item item in items)
        {
            if (!who.addItemToInventoryBool(item))
                Game1.createItemDebris(item, machine.TileLocation * 64f + new Vector2(32, 32), who.FacingDirection, machine.Location);
        }
        machine.Location?.playSound("coin");
    }

    private static void ResetMachine(SObject machine)
    {
        machine.heldObject.Value = null;
        machine.readyForHarvest.Value = false;
        machine.MinutesUntilReady = 0;
        machine.showNextIndex.Value = false;
    }

    /// <summary>Discovery, counts and XP once a run's output is collected.</summary>
    private void AfterCollect(SObject machine, MachineData state, Farmer who, List<Item> items, bool handMade)
    {
        string? recipe = state.Recipe;
        int count = items.Sum(i => i.Stack);
        state.EndRun();
        Save(machine, state);

        if (recipe != null && recipe != Ids.DecomposeRecipe && items.FirstOrDefault() is Item output)
        {
            int tier = ModServices.Content.Registry.Recipes.Get(recipe) is { } row && ModServices.Content.Registry.Substances.TryGet(row.Output.Id, out SubstanceRow sub) ? sub.Tier : 1;
            this.Discover(who, recipe, output, tier, state.Bonus, handMade, celebrateItem: output, message: null, count: output.Stack);
        }

        if (handMade)
            ModServices.Skill.Grant(who, XpEvents.HandCollect, recipe ?? "", count: count);
        else if (Game1.getOnlineFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == state.Owner) is Farmer owner)
            ModServices.Skill.Grant(owner, XpEvents.AutoCollect, recipe ?? "", count: count); // automated output credits the owner while online (doc 05)
    }

    /// <summary>Records a craft in the codex; the first make celebrates and gives discovery XP.</summary>
    public void Discover(Farmer who, string recipeId, Item? output, int tier, string? bonus, bool handMade, Item? celebrateItem, string? message, int count = 1)
    {
        var keys = new List<string> { CodexKeys.Recipe(recipeId) };
        var counts = new Dictionary<string, int>();
        if (output != null)
        {
            keys.Add(CodexKeys.Item(output.QualifiedItemId));
            counts[CodexKeys.Count(output.QualifiedItemId)] = count;
            if (handMade)
                counts[CodexKeys.HandCount(output.QualifiedItemId)] = count;
        }
        if (bonus != null)
            keys.Add(CodexKeys.Effect(bonus));

        List<string> fresh = ModServices.Farm.Record(who, keys, counts);
        ModServices.Skill.Grant(who, XpEvents.Discovery, recipeId, tier); // once per recipe per player (doc 05)
        if (fresh.Contains(CodexKeys.Recipe(recipeId)))
        {
            this.log.Info($"Discovery: {who.Name} made {recipeId} for the first time.");
            if (who.IsLocalPlayer)
            {
                string name = output?.DisplayName ?? ModServices.Content.NameOf(ModServices.Content.Registry.Recipes.Get(recipeId)?.Output.Id ?? recipeId);
                ModServices.Farm.Celebrate(celebrateItem, message ?? ModServices.T("discovery.new", new { name }));
            }
        }
        if (bonus != null && fresh.Contains(CodexKeys.Effect(bonus)))
        {
            ModServices.Skill.Grant(who, XpEvents.EffectDiscovery, bonus);
            if (who.IsLocalPlayer)
                ModServices.Farm.Toast(ModServices.T("discovery.effect", new { name = ModServices.Content.Registry.Conditions.Get(bonus)?.NameKey is string key ? ModServices.T(key) : bonus }));
        }
        ModServices.Farm.CheckMilestones(who);
    }

    // ── Timer: locked outputs, extras, automated collection (patch 8) ──────────

    private static void MinutesElapsedPostfix(SObject __instance)
    {
        if (ModServices.Content?.ApparatusOf(__instance) is null || HasHeldOutput(__instance))
            return;
        try
        {
            Self.OnTick(__instance);
        }
        catch (Exception ex)
        {
            Self.log.Error($"Machine timer failed on {__instance.QualifiedItemId} at {__instance.TileLocation}: {ex}");
        }
    }

    private void OnTick(SObject machine)
    {
        if (!Context.IsMainPlayer)
            return; // the host owns machine state in multiplayer
        MachineData state = State(machine);
        if (state.Recipe is null)
            return;

        // A locked first make leaves the slot the moment it finishes, so Automate and chaining never see it (D25).
        if (state.Locked && machine.readyForHarvest.Value && machine.heldObject.Value is SObject output)
        {
            string entry = Guid.NewGuid().ToString("N");
            output.modData[HeldOutputKey] = entry;
            Game1.player.team.GetOrCreateGlobalInventory(LockedStoreId).Add(output);
            machine.modData[HeldOutputKey] = entry;
            ResetMachine(machine);
            this.Trace(machine, $"held out locked output {output.QualifiedItemId}");
            return;
        }

        // Something else (Automate) took the output: hand over the next extra, or close the run.
        if (machine.heldObject.Value is null && !state.Locked)
        {
            if (state.TakeNextExtra() is (string id, int count))
            {
                machine.heldObject.Value = (SObject)ItemRegistry.Create(id, count);
                machine.readyForHarvest.Value = true;
                Save(machine, state);
                return;
            }
            Farmer owner = Game1.getFarmerMaybeOffline(state.Owner) ?? Game1.MasterPlayer;
            this.AfterCollect(machine, state, owner, new List<Item>(), handMade: false);
        }
    }

    /// <summary>Takes a ready output for chaining; extras follow one by one. Returns null when nothing is ready.</summary>
    public Item? TakeOutput(SObject machine)
    {
        if (!machine.readyForHarvest.Value || machine.heldObject.Value is not SObject output || HasHeldOutput(machine))
            return null;
        MachineData state = State(machine);
        if (state.Locked)
            return null;
        machine.heldObject.Value = null;
        machine.readyForHarvest.Value = false;
        if (state.TakeNextExtra() is (string id, int count))
        {
            machine.heldObject.Value = (SObject)ItemRegistry.Create(id, count);
            machine.readyForHarvest.Value = true;
            Save(machine, state);
        }
        else
        {
            Farmer owner = Game1.getFarmerMaybeOffline(state.Owner) ?? Game1.MasterPlayer;
            this.AfterCollect(machine, state, owner, new List<Item> { output }, handMade: false);
        }
        return output;
    }

    // ── Tray view, seal and picker (doc 16 §5) ──────────────────────────────────

    /// <summary>Gives a tray item back to the player (tray take-back, doc 16).</summary>
    public void TakeBack(SObject machine, Item item, Farmer who)
    {
        MachineData state = State(machine);
        Inventory tray = Tray(machine, state, create: false);
        if (!tray.Contains(item))
            return;
        tray.Remove(item);
        tray.RemoveEmptySlots();
        if (!who.addItemToInventoryBool(item))
            Game1.createItemDebris(item, who.getStandingPosition(), who.FacingDirection, who.currentLocation);
    }

    /// <summary>An empty-hand seal: decompose a lone item or start the tray's exact recipe.</summary>
    public StageRefusal Seal(SObject machine, Farmer who)
    {
        ApparatusRow? apparatus = ModServices.Content.ApparatusOf(machine);
        if (apparatus is null)
            return StageRefusal.NoRecipe;
        MachineData state = State(machine);
        Inventory tray = Tray(machine, state, create: false);
        List<ItemStack> stacks = Stacks(tray);
        MachineContext context = this.Context(machine, apparatus, state, LoadSource.Hand, who);
        StagePlan plan = ModServices.Content.Stager.Seal(context, state, stacks, ModServices.Farm.Codex);
        if (!plan.Accepted)
            return plan.Refusal;

        RunPlan? run = plan.Action == StageAction.Decompose
            ? ModServices.Content.Stager.BuildDecomposeRun(apparatus, stacks[0], ResidueRoll(machine, Ids.DecomposeRecipe))
            : ModServices.Content.Stager.BuildRecipeRun(context, plan.Match!, stacks);
        if (run is null)
            return StageRefusal.NotDecomposable;

        tray.Clear();
        if (run.ProcessState != null)
        {
            this.FinishProcessState(machine, state, run, who, LoadSource.Hand);
            return StageRefusal.None;
        }
        // Sealing sets the output directly: there is no item in hand for the vanilla output path.
        machine.heldObject.Value = (SObject)ItemRegistry.Create(run.OutputItemId!, run.Count, run.Quality);
        machine.MinutesUntilReady = run.Minutes;
        machine.readyForHarvest.Value = false;
        machine.Location?.playSound("Ship");
        this.StartRun(machine, state, run, who, LoadSource.Hand);
        return StageRefusal.None;
    }

    /// <summary>Discovered recipes this machine can run now, in its current neighbour state (the picker, doc 16 §4).</summary>
    public IReadOnlyList<CompiledRecipe> Runnable(SObject machine, Farmer who)
    {
        ApparatusRow? apparatus = ModServices.Content.ApparatusOf(machine);
        if (apparatus is null)
            return Array.Empty<CompiledRecipe>();
        MachineContext context = this.Context(machine, apparatus, State(machine), LoadSource.Picker, who);
        return ModServices.Content.Pipeline.Index.All
            .Where(r => r.RunsOn(apparatus, context.State) && ModServices.Farm.Codex.IsDiscovered(r.Id) && !ModServices.Farm.Codex.IsDisabledOnFarm(r.Id))
            .OrderBy(r => r.Id, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Loads every input of a recipe from the Satchel, then the inventory (the picker, doc 16 §4).</summary>
    public StageRefusal LoadRecipe(SObject machine, CompiledRecipe recipe, Farmer who)
    {
        Inventory satchel = ModServices.Satchel.Inventory(who);
        foreach (CompiledSpec spec in recipe.Specs)
        {
            for (int n = 0; n < spec.Spec.Count; n++)
            {
                (Item? item, IInventory? from) = Find(spec, satchel, who);
                if (item is null)
                    return StageRefusal.NoRecipe;
                if (!this.TryStage(machine, item, who, LoadSource.Picker, from))
                    return this.LastRefusal.TryGetValue(machine, out StageRefusal why) ? why : StageRefusal.NoRecipe;
            }
        }
        return StageRefusal.None;
    }

    /// <summary>How many full loads of a recipe the player carries (Satchel and inventory).</summary>
    public int Loads(CompiledRecipe recipe, Farmer who)
    {
        var all = ModServices.Satchel.Inventory(who).Concat(who.Items).Where(i => i != null).ToList();
        int loads = int.MaxValue;
        foreach (CompiledSpec spec in recipe.Specs)
        {
            int have = all.Where(i => spec.Matches(GameContent.ToInfo(i), i.Quality)).Sum(i => i.Stack);
            loads = Math.Min(loads, have / Math.Max(1, spec.Spec.Count));
        }
        return loads == int.MaxValue ? 0 : loads;
    }

    private static (Item?, IInventory?) Find(CompiledSpec spec, Inventory satchel, Farmer who)
    {
        Item? inSatchel = satchel.FirstOrDefault(i => i != null && spec.Matches(GameContent.ToInfo(i), i.Quality));
        if (inSatchel != null)
            return (inSatchel, satchel);
        Item? carried = who.Items.FirstOrDefault(i => i != null && spec.Matches(GameContent.ToInfo(i), i.Quality));
        return (carried, who.Items);
    }

    public void SetPinned(SObject machine, bool pinned)
    {
        MachineData state = State(machine);
        state.Pinned = pinned;
        Save(machine, state);
    }

    // ── The Experiment Table (doc 07, doc 16 §3) ────────────────────────────────

    /// <summary>
    /// One attempt at the Table with the given item instances (each taken from the inventory it sits in). Success
    /// makes the output and records the recipe; an undefined pairing gives residue, a tried mark and a little XP;
    /// a blocked one consumes nothing.
    /// </summary>
    public TableOutcome AttemptTable(SObject table, IReadOnlyList<(Item Item, IInventory From)> inputs, Farmer who)
    {
        ApparatusRow apparatus = ModServices.Content.ApparatusOf(table)!;
        MachineContext context = this.Context(table, apparatus, State(table), LoadSource.Hand, who);
        List<ItemStack> stacks = inputs.Select(i => new ItemStack(GameContent.ToInfo(i.Item), i.Item.Quality, 1)).ToList();
        TableOutcome outcome = ModServices.Content.Pipeline.Resolver.AttemptAtTable(context, stacks, ModServices.Farm.Codex);
        this.Trace(table, $"Table attempt {outcome.TriedSignature}: {outcome.Kind} {outcome.Recipe?.Id}");

        if (outcome.Kind is TableOutcomeKind.Blocked or TableOutcomeKind.TooManyInputs)
            return outcome;

        foreach ((Item item, IInventory from) in inputs)
            Consume(item, from);

        if (outcome.Kind == TableOutcomeKind.Success)
        {
            RecipeRow row = outcome.Recipe!.Row;
            int tier = ModServices.Content.Registry.Substances.TryGet(row.Output.Id, out SubstanceRow sub) ? sub.Tier : 1;
            string? itemId = ModServices.Content.Registry.ResolveItemId(row.Output.Id);
            bool isItem = sub?.Class == SubstanceClass.Item && itemId != null;
            Item? made = isItem ? ItemRegistry.Create(itemId!, Math.Max(1, row.Output.Count)) : null;
            if (made != null && !who.addItemToInventoryBool(made))
                Game1.createItemDebris(made, who.getStandingPosition(), who.FacingDirection, who.currentLocation);
            if (!isItem)
                table.Location?.temporarySprites.Add(new TemporaryAnimatedSprite(5, table.TileLocation * 64f + new Vector2(0, -32), Color.White, 8, false, 80f));
            table.Location?.playSound(isItem ? "newArtifact" : "steam");
            this.Discover(who, row.Id, made, tier, null, handMade: true, celebrateItem: made,
                message: isItem ? null : ModServices.T("table.power-reveal", new { name = ModServices.Content.NameOf(row.Output.Id) }));
            return outcome;
        }

        // Undefined: residue, a tried mark and a little XP once per pairing (doc 07).
        if (ModServices.Content.Registry.Settings.ResidueItem is string residue)
        {
            Item caput = ItemRegistry.Create(residue);
            if (!who.addItemToInventoryBool(caput))
                Game1.createItemDebris(caput, who.getStandingPosition(), who.FacingDirection, who.currentLocation);
        }
        table.Location?.playSound("dirtyHit");
        List<string> fresh = ModServices.Farm.Record(who, new[] { CodexKeys.Tried(inputs.Select(i => i.Item.QualifiedItemId)) });
        ModServices.Skill.Grant(who, XpEvents.TriedKey, outcome.TriedSignature);
        if (fresh.Count > 0)
            ModServices.Farm.Toast(ModServices.T("table.tried"));
        return outcome;
    }

    private static void Consume(Item item, IInventory from)
    {
        item.Stack -= 1;
        if (item.Stack <= 0)
        {
            int index = from.IndexOf(item);
            if (index >= 0)
                from[index] = null;
        }
    }

    // ── Text ────────────────────────────────────────────────────────────────────

    /// <summary>The reason line for a refusal (doc 16 §2's fixed set).</summary>
    public static string Reason(StageRefusal why, IReadOnlyList<string>? failed = null)
    {
        string text = ModServices.T("reason." + why);
        if (failed is { Count: > 0 })
            text += " " + string.Join(", ", failed.Select(f => ModServices.Content.Registry.Conditions.Get(f)?.NameKey is string key ? ModServices.T(key) : f));
        return text;
    }

    /// <summary>The machine's hover lines (doc 16 §2 tooltip order): status, recipe memory, tray, neighbour state, fuel.</summary>
    public List<string> Describe(SObject machine, Farmer who)
    {
        var lines = new List<string>();
        ApparatusRow? apparatus = ModServices.Content.ApparatusOf(machine);
        if (apparatus is null)
            return lines;
        MachineData state = State(machine);

        if (HasHeldOutput(machine))
            lines.Add(ModServices.T("tooltip.locked-ready"));
        else if (machine.readyForHarvest.Value)
            lines.Add(ModServices.T("tooltip.ready"));
        else if (machine.heldObject.Value is SObject working)
            lines.Add(ModServices.T("tooltip.working", new { name = working.DisplayName, minutes = machine.MinutesUntilReady }));
        else if (this.LastRefusal.TryGetValue(machine, out StageRefusal why))
            lines.Add(Reason(why));

        if (apparatus.Kind != ApparatusKinds.Table)
        {
            string memory = state.RecipeMemory switch
            {
                null => ModServices.T("tooltip.memory-unset"),
                Ids.DecomposeRecipe => ModServices.T("tooltip.memory-decompose"),
                string id => ModServices.Content.NameOf(ModServices.Content.Registry.Recipes.Get(id)?.Output.Id ?? id),
            };
            lines.Add(ModServices.T("tooltip.memory", new { memory }) + (state.Pinned ? " " + ModServices.T("tooltip.pinned") : ""));
        }

        IReadOnlyList<Item> tray = TrayItems(machine);
        if (tray.Count > 0)
            lines.Add(ModServices.T("tooltip.tray", new { items = string.Join(", ", tray.Select(i => i.Stack > 1 ? $"{i.DisplayName} x{i.Stack}" : i.DisplayName)) }));

        ConditionSnapshot snapshot = Neighbours.Read(machine, apparatus, who);
        if (apparatus.NeighbourStates.Count > 1 && snapshot.NeighbourState != null)
            lines.Add(ModServices.T("tooltip.state", new { state = ModServices.T("state." + snapshot.NeighbourState) }));
        if (apparatus.Heat != null)
            lines.Add(ModServices.T(state.Kindled ? "tooltip.kindled" : "tooltip.not-kindled"));
        if (apparatus.Fuel is { } fuel)
            lines.Add(ModServices.T("tooltip.fuel", new { have = state.Fuel, max = fuel.Capacity }));
        return lines;
    }

    private static SeededRandom ResidueRoll(SObject machine, string recipeId)
        => SeededRandom.ForRoll((int)Game1.stats.DaysPlayed, machine.Location?.NameOrUniqueName ?? "", (int)machine.TileLocation.X, (int)machine.TileLocation.Y, recipeId + Game1.timeOfDay);

    private void Trace(SObject machine, string message)
    {
        if (ModServices.Config.Verbose || this.log.Verbose)
            this.log.Trace($"{machine.Name} at {machine.Location?.NameOrUniqueName} {machine.TileLocation}: {message}");
    }
}
