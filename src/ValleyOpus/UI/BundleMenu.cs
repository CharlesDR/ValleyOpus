using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.World;

namespace ValleyOpus.UI;

/// <summary>
/// A room's bundle board (doc 14 §3, doc 16 bundle board): its bundles with their slots and rewards, the player's
/// inventory below; clicking an item donates it to the first slot it fills. From the Library on, the board also
/// offers residue divination (doc 07 §3).
/// </summary>
internal sealed class BundleMenu : IClickableMenu
{
    private const int Pad = 32;
    private const int BundleRowHeight = 112;
    private const int PlayerBaseId = 1000;
    private const int DivineId = 200;
    private const string LibraryRoom = Ids.RowPrefix + "Library";

    private readonly WorkshopRoomRow room;
    private readonly List<WorkshopBundleRow> bundles;
    private readonly List<List<ClickableComponent>> slotBoxes = new();
    private InventoryMenu inventory = null!;
    private ClickableComponent divineButton = null!;
    private int bundlesTop;
    private int inventoryLabelTop;
    private string? message;
    private string? hoverText;
    private Item? hovered;

    private BundleMenu(WorkshopRoomRow room)
    {
        this.room = room;
        this.bundles = room.Bundles.Select(id => ModServices.Content.Registry.Bundles.Get(id)).OfType<WorkshopBundleRow>().ToList();
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    public static void Open(string roomId)
    {
        if (!Context.IsWorldReady || ModServices.Content.Registry.Rooms.Get(WorldService.Full(roomId)) is not WorkshopRoomRow room)
            return;
        Game1.activeClickableMenu = new BundleMenu(room);
    }

    private bool CanDivine => ModServices.World.IsRestored(LibraryRoom);

    private void Layout()
    {
        int playerRows = MenuKit.Rows(Game1.player.MaxItems);
        int innerWidth = 12 * 64;
        int width = innerWidth + Pad * 2;
        int height = Pad + 60 + this.bundles.Count * BundleRowHeight + 64 + 36 + playerRows * MenuKit.InventoryRowHeight + Pad;
        Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        this.initialize((int)topLeft.X, (int)topLeft.Y, width, height, true);

        int left = this.xPositionOnScreen + Pad;
        this.bundlesTop = this.yPositionOnScreen + Pad + 60;
        this.slotBoxes.Clear();
        for (int b = 0; b < this.bundles.Count; b++)
        {
            var boxes = new List<ClickableComponent>();
            int y = this.bundlesTop + b * BundleRowHeight + 36;
            for (int i = 0; i < this.bundles[b].Slots.Count; i++)
                boxes.Add(new ClickableComponent(new Rectangle(left + i * 72, y, 64, 64), $"b{b}s{i}") { myID = 100 + b * 20 + i });
            this.slotBoxes.Add(boxes);
        }
        int divineTop = this.bundlesTop + this.bundles.Count * BundleRowHeight;
        this.divineButton = new ClickableComponent(new Rectangle(left + innerWidth - 300, divineTop, 300, 56), "divine") { myID = DivineId, downNeighborID = PlayerBaseId + 11 };
        this.inventoryLabelTop = divineTop + 64;
        this.inventory = new InventoryMenu(left, this.inventoryLabelTop + 36, true, Game1.player.Items, item => this.bundles.Any(bundle => ModServices.World.SlotFor(bundle, item) >= 0));
        MenuKit.WireGrid(this.inventory, PlayerBaseId, upEdge: _ => DivineId, downEdge: _ => -1);

        this.allClickableComponents = new List<ClickableComponent>();
        this.allClickableComponents.AddRange(this.slotBoxes.SelectMany(s => s));
        if (this.CanDivine)
            this.allClickableComponents.Add(this.divineButton);
        this.allClickableComponents.AddRange(this.inventory.inventory);
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    public override void snapToDefaultClickableComponent() => MenuKit.Snap(this, PlayerBaseId);

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => this.Layout();

    // ── Input ───────────────────────────────────────────────────────────────────

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.upperRightCloseButton?.containsPoint(x, y) == true)
        {
            base.receiveLeftClick(x, y, playSound);
            return;
        }
        if (this.CanDivine && this.divineButton.containsPoint(x, y))
        {
            this.exitThisMenu();
            Divination.Ask();
            return;
        }
        Item? item = MenuKit.ItemAt(this.inventory, x, y).Item;
        if (item is null)
            return;
        foreach (WorkshopBundleRow bundle in this.bundles)
        {
            if (ModServices.World.SlotFor(bundle, item) < 0)
                continue;
            bool done = ModServices.World.Donate(Game1.player, bundle, item);
            this.message = done ? ModServices.T("ui.bundle.donated", new { name = item.DisplayName, bundle = WorldService.BundleName(bundle) }) : null;
            return;
        }
        this.message = ModServices.T("ui.bundle.no-slot");
        Game1.playSound("cancel");
    }

    public override void receiveRightClick(int x, int y, bool playSound = true) => this.receiveLeftClick(x, y, playSound);

    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape || Game1.options.doesInputListContain(Game1.options.menuButton, key))
        {
            this.exitThisMenu();
            return;
        }
        base.receiveKeyPress(key);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.hovered = MenuKit.ItemAt(this.inventory, x, y).Item;
        this.hoverText = null;
        for (int b = 0; b < this.slotBoxes.Count; b++)
        {
            for (int i = 0; i < this.slotBoxes[b].Count; i++)
            {
                if (this.slotBoxes[b][i].containsPoint(x, y))
                    this.hoverText = MenuKit.InputName(this.bundles[b].Slots[i]);
            }
        }
    }

    // ── Drawing ─────────────────────────────────────────────────────────────────

    public override void draw(SpriteBatch b)
    {
        MenuKit.DrawBackdrop(b);
        MenuKit.DrawPanel(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        int left = this.xPositionOnScreen + Pad;
        MenuKit.DrawText(b, ModServices.T("ui.bundle.title", new { room = WorldService.RoomName(this.room.Id) }), left, this.yPositionOnScreen + Pad, font: Game1.dialogueFont);

        if (this.bundles.Count == 0)
            MenuKit.DrawText(b, ModServices.T("ui.bundle.none"), left, this.bundlesTop);
        for (int i = 0; i < this.bundles.Count; i++)
        {
            WorkshopBundleRow bundle = this.bundles[i];
            int top = this.bundlesTop + i * BundleRowHeight;
            bool done = ModServices.World.View.BundlesDone.Contains(bundle.Id);
            string status = done ? ModServices.T("ui.bundle.complete") : ModServices.T("ui.bundle.progress", new { have = ModServices.World.Filled(bundle), need = bundle.RequiredCount });
            MenuKit.DrawText(b, $"{WorldService.BundleName(bundle)}  ({status})", left, top, done ? Color.DarkGreen : null);
            for (int s = 0; s < bundle.Slots.Count; s++)
            {
                Rectangle r = this.slotBoxes[i][s].bounds;
                MenuKit.DrawSlot(b, r);
                bool filled = ModServices.World.IsSlotFilled(bundle, s, out string? itemId);
                IngredientSpec spec = bundle.Slots[s];
                string? icon = filled && itemId != "dev" ? itemId : spec.Item != null ? WorldService.Compile(spec).ItemId : null;
                if (icon is null || !MenuKit.DrawItemIcon(b, icon, new Rectangle(r.X + 8, r.Y + 8, 48, 48), filled ? Color.White : Color.Black * 0.35f))
                    MenuKit.DrawText(b, "?", r.X + 26, r.Y + 18, filled ? Color.DarkGreen : Color.Gray);
                if (spec.Count > 1)
                    Utility.drawTinyDigits(spec.Count, b, new Vector2(r.Right - 20, r.Bottom - 20), 3f, 1f, Color.White);
            }
            if (bundle.Reward is { } reward)
            {
                int rx = left + 12 * 64 - 64;
                MenuKit.DrawText(b, ModServices.T("ui.bundle.reward"), rx - 100, top + 56, Color.DimGray);
                MenuKit.DrawItemIcon(b, Ids.QualifyItem(reward.Item), new Rectangle(rx, top + 36, 64, 64), done ? Color.Gray : Color.White);
                if (reward.Count > 1)
                    Utility.drawTinyDigits(reward.Count, b, new Vector2(rx + 44, top + 80), 3f, 1f, Color.White);
            }
        }

        if (this.CanDivine)
            MenuKit.DrawButton(b, this.divineButton, ModServices.T("ui.bundle.divine"));
        if (this.message != null)
            MenuKit.DrawText(b, MenuKit.Fit(this.message, 12 * 64 - 320), left, this.divineButton.bounds.Y + 14);

        MenuKit.DrawText(b, ModServices.T("ui.table.inventory"), left, this.inventoryLabelTop);
        this.inventory.draw(b);

        base.draw(b);
        if (this.hovered != null)
            drawToolTip(b, this.hovered.getDescription(), this.hovered.DisplayName, this.hovered);
        else if (this.hoverText != null)
            drawHoverText(b, this.hoverText, Game1.smallFont);
        this.drawMouse(b);
    }
}

/// <summary>Residue divination at a bundle board (doc 07 §3, doc 16): spend Caput Mortuum for an unseen hint.</summary>
internal static class Divination
{
    public static void Ask()
    {
        List<HintRow> hints = ModServices.World.AvailableHints("Divination");
        string? residue = ModServices.Content.Registry.Settings.ResidueItem;
        if (hints.Count == 0 || residue is null)
        {
            Game1.drawObjectDialogue(ModServices.T("divination.nothing"));
            return;
        }
        int cost = Math.Max(1, hints.Min(h => h.DivinationCost));
        int have = Game1.player.Items.Where(i => i?.QualifiedItemId == Ids.QualifyItem(residue)).Sum(i => i.Stack);
        var responses = new List<Response> { new("1", ModServices.T("divination.one", new { cost })) };
        if (ModServices.World.View.Divinations >= 5)
        {
            foreach (int n in new[] { 5, 10 }.Where(n => n <= hints.Count))
                responses.Add(new Response(n.ToString(), ModServices.T("divination.many", new { count = n, cost = cost * n })));
        }
        responses.Add(new Response("No", ModServices.T("divination.cancel")));
        Game1.currentLocation.createQuestionDialogue(ModServices.T("divination.ask", new { have, name = MenuKit.ItemName(Ids.QualifyItem(residue)) }), responses.ToArray(), (who, answer) =>
        {
            if (!int.TryParse(answer, out int count))
                return;
            Buy(who, count, cost, Ids.QualifyItem(residue));
        });
    }

    private static void Buy(Farmer who, int count, int cost, string residue)
    {
        int total = count * cost;
        int have = who.Items.Where(i => i?.QualifiedItemId == residue).Sum(i => i.Stack);
        if (have < total)
        {
            Game1.drawObjectDialogue(ModServices.T("divination.short", new { need = total }));
            return;
        }
        List<HintRow> pool = ModServices.World.AvailableHints("Divination").Where(h => Math.Max(1, h.DivinationCost) <= cost).ToList();
        var lines = new List<string>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            HintRow pick = pool[Game1.random.Next(pool.Count)];
            pool.Remove(pick);
            lines.Add(ModServices.World.Reveal(who, pick));
        }
        who.Items.ReduceId(residue, total);
        ModServices.World.CountDivination(lines.Count);
        Game1.playSound("fireball");
        Game1.drawObjectDialogue(string.Join("^^", lines));
    }
}
