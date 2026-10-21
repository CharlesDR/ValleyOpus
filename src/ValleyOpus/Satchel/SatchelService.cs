using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Decomposition;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Skill;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using SObject = StardewValley.Object;

namespace ValleyOpus.Satchel;

/// <summary>
/// The <c>Satchel</c> module (doc 03b, doc 16 §4): one global inventory per player (D10) that auto-collects reagents,
/// counted in item types; holding it and clicking an apparatus opens the picker. Also the Assayer's Lens.
/// </summary>
internal sealed class SatchelService
{
    public const string SatchelItemId = "(O)" + Ids.RowPrefix + "Satchel";
    public const string LensItemId = "(O)" + Ids.RowPrefix + "Lens";
    private const string InventoryPrefix = Ids.RowPrefix + "Satchel_";

    private static bool adding;
    private readonly SmapiLog log;

    public SatchelService(IModHelper helper, SmapiLog log)
    {
        this.log = log;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private static SatchelService Self => ModServices.Satchel;

    public Inventory Inventory(Farmer who) => Game1.player.team.GetOrCreateGlobalInventory(InventoryPrefix + who.UniqueMultiplayerID);

    public static bool Carries(Farmer who, string qualifiedId) => who.Items.Any(i => i?.QualifiedItemId == qualifiedId);

    /// <summary>Type slots: 24, plus 12 at Alchemy level 2 (doc 03b, doc 05).</summary>
    public int Capacity(Farmer who) => SkillMath.SatchelCapacity(ModServices.Skill.Level(who));

    /// <summary>Reagents go in automatically; products only by hand (doc 15).</summary>
    public static bool IsReagent(Item item) => item.HasContextTag(Ids.ReagentTag);

    /// <summary>What the Satchel can hold at all (the guard): our Elements, never a tool or the Satchel itself.</summary>
    public static bool CanHold(Item? item)
        => item != null && !item.HasContextTag("valleyopus_tool")
            && (item.HasContextTag(Ids.ReagentTag) || item.HasContextTag(Ids.ProductTag) || item.HasContextTag("valleyopus_substance"));

    /// <summary>Whether the Satchel has room for this item: a stack of its type with space, or a free type slot.</summary>
    public bool HasRoom(Farmer who, Item item)
    {
        Inventory bag = this.Inventory(who);
        if (bag.Any(i => i != null && i.canStackWith(item) && i.Stack < i.maximumStackSize()))
            return true;
        return bag.Count(i => i != null) < this.Capacity(who);
    }

    /// <summary>Adds to the Satchel; returns what didn't fit.</summary>
    public Item? Add(Farmer who, Item item)
    {
        Inventory bag = this.Inventory(who);
        foreach (Item stack in bag.Where(i => i != null && i.canStackWith(item)))
        {
            item.Stack = stack.addToStack(item);
            if (item.Stack <= 0)
                return null;
        }
        if (bag.Count(i => i != null) >= this.Capacity(who))
            return item;
        bag.RemoveEmptySlots();
        bag.Add(item);
        return null;
    }

    // ── Auto-collect (Harmony patch 5 and the pickup check) ─────────────────────

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventory), new[] { typeof(Item), typeof(List<Item>) }),
            prefix: new HarmonyMethod(typeof(SatchelService), nameof(AddItemPrefix)));
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), new[] { typeof(Item) }),
            postfix: new HarmonyMethod(typeof(SatchelService), nameof(CouldAcceptPostfix)));
    }

    private static bool AddItemPrefix(Farmer __instance, ref Item __0, ref Item? __result)
    {
        try
        {
            if (adding || __0 is null || !IsReagent(__0) || !Carries(__instance, SatchelItemId) || !Self.HasRoom(__instance, __0))
                return true;
            adding = true;
            Item? left = Self.Add(__instance, __0);
            if (left is null)
            {
                __result = null;
                return false;
            }
            __0 = left;
            return true;
        }
        catch (Exception ex)
        {
            Self.log.Error($"Satchel auto-collect failed for {__0?.QualifiedItemId}: {ex}");
            return true;
        }
        finally
        {
            adding = false;
        }
    }

    private static void CouldAcceptPostfix(Farmer __instance, Item item, ref bool __result)
    {
        if (!__result && item != null && IsReagent(item) && Carries(__instance, SatchelItemId) && Self.HasRoom(__instance, item))
            __result = true;
    }

    // ── Clicks with the Satchel or the Lens held ────────────────────────────────

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree)
            return;
        if (ModServices.Config.CodexKey != SButton.None && e.Button == ModServices.Config.CodexKey)
        {
            UI.CodexMenu.Open();
            return;
        }
        if (!(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
            return;

        Farmer who = Game1.player;
        string? held = who.CurrentItem?.QualifiedItemId;
        if (held != SatchelItemId && held != LensItemId)
            return;

        var tile = e.Cursor.GrabTile;
        SObject? target = who.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y);
        if (target is null || !Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, who))
            target = who.currentLocation.getObjectAtTile((int)who.GetGrabTile().X, (int)who.GetGrabTile().Y);

        ModServices.Helper.Input.Suppress(e.Button);
        if (held == SatchelItemId)
        {
            ApparatusRow? apparatus = ModServices.Content.ApparatusOf(target);
            if (apparatus?.Kind == ApparatusKinds.Table)
                UI.TableMenu.Open(target!);
            else if (apparatus != null && target!.heldObject.Value is null && !MachineService.HasHeldOutput(target))
                UI.ApparatusPanel.Open(target, pickerMode: true);
            else if (apparatus != null)
                target!.checkForAction(who);
            else
                UI.SatchelMenu.Open(who);
        }
        else
        {
            if (target is Chest chest)
                this.AssayChest(who, chest);
            else if (target != null)
                this.ShowAssay(who, target);
            else
                UI.AssayMenu.Open(who);
        }
    }

    // ── The Assayer's Lens (doc 03 "Assay", doc 05 level 4) ─────────────────────

    /// <summary>Assays one item: records the key, gives XP once, and returns its profile line.</summary>
    public string Assay(Farmer who, Item item)
    {
        ModServices.Farm.Record(who, new[] { CodexKeys.Assay(item.QualifiedItemId) });
        ModServices.Skill.Grant(who, XpEvents.Assay, item.QualifiedItemId);
        return ProfileText(item);
    }

    /// <summary>"Aqua 3 · Sal 1", or why it can't be decomposed.</summary>
    public static string ProfileText(Item item)
    {
        var decomposer = ModServices.Content.Pipeline.Decomposer;
        var info = GameContent.ToInfo(item);
        if (ModServices.Content.Registry.PrimeForItem(item.QualifiedItemId) != null)
            return ModServices.T("assay.prime");
        if (decomposer.IsRefused(info))
            return ModServices.T("assay.refused");
        DecompositionResult result = decomposer.Decompose(info, item.Quality, null);
        if (result.Refused || result.Total == 0)
            return ModServices.T("assay.nothing");
        return string.Join(" · ", result.Units.Select(u => $"{ModServices.Content.NameOf(u.Prime)} {u.Units}"));
    }

    /// <summary>Whether this player has assayed an item (profile lines show in its tooltip from then on).</summary>
    public static bool IsAssayed(Item item) => ModServices.Farm.Codex.Has(CodexKeys.Assay(item.QualifiedItemId));

    private void ShowAssay(Farmer who, SObject target)
    {
        string text = this.Assay(who, target);
        Game1.drawObjectDialogue($"{target.DisplayName}: {text}");
    }

    private void AssayChest(Farmer who, Chest chest)
    {
        if (!SkillMath.LensAssaysChests(ModServices.Skill.Level(who)))
        {
            Game1.showRedMessage(ModServices.T("assay.chest-locked"));
            return;
        }
        var lines = chest.Items.Where(i => i != null).GroupBy(i => i.QualifiedItemId).Select(g => $"{g.First().DisplayName}: {this.Assay(who, g.First())}").ToList();
        Game1.drawObjectDialogue(lines.Count == 0 ? ModServices.T("assay.empty") : string.Join("^", lines.Take(12)));
    }
}
