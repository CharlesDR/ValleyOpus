using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using ValleyOpus.Framework;
using ValleyOpus.Satchel;

namespace ValleyOpus.UI;

/// <summary>
/// The Satchel grid (doc 16 §4): the Satchel's contents above the player's inventory, items moved between them by
/// click (right-click moves one), a type count against capacity, and the button that opens the codex.
/// </summary>
internal sealed class SatchelMenu : IClickableMenu
{
    private const int CodexId = 100;
    private const int SatchelBaseId = 1000;
    private const int PlayerBaseId = 2000;
    private const int Pad = 32;

    private readonly Farmer who;
    private readonly ClickableComponent codexButton = new(Rectangle.Empty, "codex") { myID = CodexId };
    private InventoryMenu satchelInventory = null!;
    private InventoryMenu playerInventory = null!;
    private int countTop;
    private int playerLabelTop;
    private string? message;
    private Item? hovered;

    private SatchelMenu(Farmer who)
    {
        this.who = who;
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    public static void Open(Farmer who)
    {
        if (!Context.IsWorldReady || ModServices.Content is null)
            return;
        Game1.activeClickableMenu = new SatchelMenu(who);
    }

    private Inventory Bag => ModServices.Satchel.Inventory(this.who);

    private void Layout()
    {
        int capacity = ModServices.Satchel.Capacity(this.who);
        int satchelRows = MenuKit.Rows(capacity);
        int playerRows = MenuKit.Rows(this.who.MaxItems);
        int innerWidth = 12 * 64;
        int width = innerWidth + Pad * 2;
        int height = Pad + 52 + 40 + satchelRows * MenuKit.InventoryRowHeight + 16 + 36 + 16 + playerRows * MenuKit.InventoryRowHeight + 40 + Pad;
        Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        this.initialize((int)topLeft.X, (int)topLeft.Y, width, height, true);

        int left = this.xPositionOnScreen + Pad;
        int top = this.yPositionOnScreen + Pad;
        this.codexButton.bounds = new Rectangle(left + innerWidth - 200, top, 200, 64);
        this.countTop = top + 52;
        int satchelTop = this.countTop + 40 + 4;
        this.satchelInventory = new InventoryMenu(left, satchelTop, false, this.Bag, InventoryMenu.highlightAllItems, capacity, satchelRows);
        this.playerLabelTop = satchelTop + satchelRows * MenuKit.InventoryRowHeight + 16;
        int playerTop = this.playerLabelTop + 36 + 16;
        this.playerInventory = new InventoryMenu(left, playerTop, true, this.who.Items, item => SatchelService.CanHold(item));

        int satchelColumns = Math.Max(1, this.satchelInventory.capacity / Math.Max(1, this.satchelInventory.rows));
        int satchelCount = this.satchelInventory.inventory.Count;
        MenuKit.WireGrid(this.satchelInventory, SatchelBaseId,
            upEdge: _ => CodexId,
            downEdge: col => PlayerBaseId + col);
        MenuKit.WireGrid(this.playerInventory, PlayerBaseId,
            upEdge: col => SatchelBaseId + Math.Min(satchelCount - 1, (satchelCount - 1) / satchelColumns * satchelColumns + col),
            downEdge: _ => -1);
        this.codexButton.downNeighborID = SatchelBaseId + Math.Min(satchelCount - 1, satchelColumns - 1);

        this.allClickableComponents = new List<ClickableComponent> { this.codexButton };
        this.allClickableComponents.AddRange(this.satchelInventory.inventory);
        this.allClickableComponents.AddRange(this.playerInventory.inventory);
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    public override void snapToDefaultClickableComponent() => MenuKit.Snap(this, SatchelBaseId);

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    // ── Input ───────────────────────────────────────────────────────────────────

    public override void receiveLeftClick(int x, int y, bool playSound = true) => this.Click(x, y, wholeStack: true, playSound);

    public override void receiveRightClick(int x, int y, bool playSound = true) => this.Click(x, y, wholeStack: false, playSound);

    private void Click(int x, int y, bool wholeStack, bool playSound)
    {
        if (this.upperRightCloseButton?.containsPoint(x, y) == true)
        {
            base.receiveLeftClick(x, y, playSound);
            return;
        }
        if (this.codexButton.containsPoint(x, y))
        {
            Game1.playSound("bigSelect");
            CodexMenu.Open();
            return;
        }

        try
        {
            (int playerIndex, Item? carried) = MenuKit.ItemAt(this.playerInventory, x, y);
            if (carried != null)
            {
                this.ToSatchel(playerIndex, carried, wholeStack);
                return;
            }
            Item? stored = MenuKit.ItemAt(this.satchelInventory, x, y).Item;
            if (stored != null)
                this.ToPlayer(stored, wholeStack);
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"The Satchel menu failed to move an item: {ex}");
            this.message = ModServices.T("ui.error");
        }
    }

    private void ToSatchel(int index, Item item, bool wholeStack)
    {
        if (!SatchelService.CanHold(item))
        {
            this.message = ModServices.T("ui.satchel.cannot-hold", new { name = item.DisplayName });
            Game1.playSound("cancel");
            return;
        }

        int before = item.Stack;
        if (wholeStack)
        {
            Item? left = ModServices.Satchel.Add(this.who, item);
            if (left is null)
                this.who.Items[index] = null;
        }
        else
        {
            Item? left = ModServices.Satchel.Add(this.who, item.getOne());
            if (left is null)
            {
                item.Stack -= 1;
                if (item.Stack <= 0)
                    this.who.Items[index] = null;
            }
        }

        bool moved = this.who.Items[index] is null || item.Stack < before;
        this.message = moved ? null : ModServices.T("ui.satchel.full");
        Game1.playSound(moved ? "dwop" : "cancel");
    }

    private void ToPlayer(Item item, bool wholeStack)
    {
        Inventory bag = this.Bag;
        Item moving = wholeStack ? item : item.getOne();
        int amount = moving.Stack;
        Item? left = AddToPlayer(this.who, moving);
        int moved = amount - (left?.Stack ?? 0);
        if (moved <= 0)
        {
            this.message = ModServices.T("ui.satchel.inventory-full");
            Game1.playSound("cancel");
            return;
        }

        if (wholeStack)
        {
            if (left is null)
                bag.Remove(item);
        }
        else
        {
            item.Stack -= moved;
            if (item.Stack <= 0)
                bag.Remove(item);
        }
        bag.RemoveEmptySlots();
        this.message = null;
        Game1.playSound("dwop");
    }

    /// <summary>
    /// Puts an item straight into the player's slots: stacks first, then the first empty slot. It bypasses
    /// <c>addItemToInventory</c>, whose Satchel auto-collect would put a reagent straight back.
    /// </summary>
    private static Item? AddToPlayer(Farmer who, Item item)
    {
        Inventory slots = who.Items;
        int max = Math.Min(slots.Count, who.MaxItems);
        for (int i = 0; i < max; i++)
        {
            if (slots[i] is Item stack && stack.canStackWith(item))
            {
                item.Stack = stack.addToStack(item);
                if (item.Stack <= 0)
                    return null;
            }
        }
        for (int i = 0; i < max; i++)
        {
            if (slots[i] is null)
            {
                slots[i] = item;
                return null;
            }
        }
        return item;
    }

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
        this.hovered = MenuKit.ItemAt(this.satchelInventory, x, y).Item ?? MenuKit.ItemAt(this.playerInventory, x, y).Item;
    }

    // ── Drawing ─────────────────────────────────────────────────────────────────

    public override void draw(SpriteBatch b)
    {
        MenuKit.DrawBackdrop(b);
        MenuKit.DrawPanel(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        int left = this.xPositionOnScreen + Pad;
        int top = this.yPositionOnScreen + Pad;
        MenuKit.DrawText(b, ModServices.T("ui.satchel.title"), left, top, font: Game1.dialogueFont);
        MenuKit.DrawButton(b, this.codexButton, ModServices.T("ui.satchel.codex"));

        int types = this.Bag.Count(i => i != null);
        int capacity = ModServices.Satchel.Capacity(this.who);
        MenuKit.DrawText(b, ModServices.T("ui.satchel.types", new { count = types, capacity }), left, this.countTop, types >= capacity ? Color.DarkRed : Game1.textColor);
        this.satchelInventory.draw(b);

        MenuKit.DrawText(b, ModServices.T("ui.satchel.inventory"), left, this.playerLabelTop);
        this.playerInventory.draw(b);

        if (this.message != null)
            MenuKit.DrawText(b, MenuKit.Fit(this.message, this.width - Pad * 2), left, this.yPositionOnScreen + this.height - Pad - 36, Color.DarkRed);

        base.draw(b);
        if (this.hovered != null)
            drawToolTip(b, this.hovered.getDescription(), this.hovered.DisplayName, this.hovered);
        this.drawMouse(b);
    }
}
