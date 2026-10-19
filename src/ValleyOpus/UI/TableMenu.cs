using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.UI;

/// <summary>
/// The Experiment Table (doc 16 §3): input slots filled one item at a time from the inventory or the Satchel, and a
/// Try button that makes one attempt and reports its outcome in the menu.
/// </summary>
internal sealed class TableMenu : IClickableMenu
{
    private const int SlotBaseId = 100;
    private const int TryId = 200;
    private const int PlayerBaseId = 1000;
    private const int SatchelBaseId = 2000;
    private const int Pad = 32;

    private readonly SObject table;
    private readonly List<(Item Item, IInventory From)> picked = new();
    private readonly List<ClickableComponent> slots = new();
    private ClickableComponent tryButton = null!;
    private InventoryMenu playerInventory = null!;
    private InventoryMenu satchelInventory = null!;
    private int capacity;
    private int outcomeTop;
    private int playerLabelTop;
    private int satchelLabelTop;
    private string? outcome;
    private Item? hovered;

    private TableMenu(SObject table)
    {
        this.table = table;
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    public static void Open(SObject table)
    {
        if (!Context.IsWorldReady || ModServices.Content?.ApparatusOf(table) is null)
            return;
        Game1.activeClickableMenu = new TableMenu(table);
    }

    private Inventory SatchelItems => ModServices.Satchel.Inventory(Game1.player);

    private void Layout()
    {
        this.capacity = Math.Max(1, ModServices.Farm.Codex.Data.TableCapacity);
        int satchelCapacity = ModServices.Satchel.Capacity(Game1.player);
        int satchelRows = MenuKit.Rows(satchelCapacity);
        int playerRows = MenuKit.Rows(Game1.player.MaxItems);

        int innerWidth = 12 * 64;
        int width = innerWidth + Pad * 2;
        int height = Pad + 52 + 80 + 72 + 36 + playerRows * MenuKit.InventoryRowHeight + 36 + satchelRows * MenuKit.InventoryRowHeight + Pad;
        Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        this.initialize((int)topLeft.X, (int)topLeft.Y, width, height, showUpperRightCloseButton: true);

        int left = this.xPositionOnScreen + Pad;
        int y = this.yPositionOnScreen + Pad + 52;

        const int gap = 48, tryWidth = 160;
        int rowWidth = this.capacity * 64 + (this.capacity - 1) * gap + 32 + tryWidth;
        int slotX = left + (innerWidth - rowWidth) / 2;
        this.slots.Clear();
        for (int i = 0; i < this.capacity; i++)
        {
            this.slots.Add(new ClickableComponent(new Rectangle(slotX + i * (64 + gap), y, 64, 64), "slot" + i)
            {
                myID = SlotBaseId + i,
                leftNeighborID = i > 0 ? SlotBaseId + i - 1 : -1,
                rightNeighborID = i < this.capacity - 1 ? SlotBaseId + i + 1 : TryId,
                downNeighborID = PlayerBaseId + Math.Min(11, i),
            });
        }
        this.tryButton = new ClickableComponent(new Rectangle(slotX + this.capacity * (64 + gap) - gap + 32, y, tryWidth, 64), "try")
        {
            myID = TryId,
            leftNeighborID = SlotBaseId + this.capacity - 1,
            downNeighborID = PlayerBaseId + 11,
        };

        this.outcomeTop = y + 80;
        this.playerLabelTop = this.outcomeTop + 72;
        int playerTop = this.playerLabelTop + 36 + 16;
        this.playerInventory = new InventoryMenu(left, playerTop, true, Game1.player.Items, InventoryMenu.highlightAllItems);
        this.satchelLabelTop = playerTop - 16 + playerRows * MenuKit.InventoryRowHeight;
        int satchelTop = this.satchelLabelTop + 36 + 4;
        this.satchelInventory = new InventoryMenu(left, satchelTop, false, this.SatchelItems, InventoryMenu.highlightAllItems, satchelCapacity, satchelRows);

        int playerColumns = Math.Max(1, this.playerInventory.capacity / Math.Max(1, this.playerInventory.rows));
        MenuKit.WireGrid(this.playerInventory, PlayerBaseId,
            upEdge: col => col < this.capacity ? SlotBaseId + col : TryId,
            downEdge: col => SatchelBaseId + Math.Min(col, Math.Max(0, this.satchelInventory.inventory.Count - 1)));
        MenuKit.WireGrid(this.satchelInventory, SatchelBaseId,
            upEdge: col => PlayerBaseId + Math.Min(this.playerInventory.inventory.Count - 1, (this.playerInventory.inventory.Count - 1) / playerColumns * playerColumns + col),
            downEdge: _ => -1);

        this.allClickableComponents = new List<ClickableComponent>();
        this.allClickableComponents.AddRange(this.slots);
        this.allClickableComponents.Add(this.tryButton);
        this.allClickableComponents.AddRange(this.playerInventory.inventory);
        this.allClickableComponents.AddRange(this.satchelInventory.inventory);
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    public override void snapToDefaultClickableComponent()
        => MenuKit.Snap(this, this.picked.Count < this.capacity ? SlotBaseId + this.picked.Count : TryId);

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    // ── Input ───────────────────────────────────────────────────────────────────

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.upperRightCloseButton?.containsPoint(x, y) == true)
        {
            base.receiveLeftClick(x, y, playSound);
            return;
        }

        for (int i = 0; i < this.slots.Count; i++)
        {
            if (!this.slots[i].containsPoint(x, y))
                continue;
            if (i < this.picked.Count)
            {
                this.picked.RemoveAt(i);
                Game1.playSound("dwop");
            }
            return;
        }

        if (this.tryButton.containsPoint(x, y))
        {
            this.Try();
            return;
        }

        Item? fromPlayer = MenuKit.ItemAt(this.playerInventory, x, y).Item;
        if (fromPlayer != null)
        {
            this.Pick(fromPlayer, Game1.player.Items);
            return;
        }
        Item? fromSatchel = MenuKit.ItemAt(this.satchelInventory, x, y).Item;
        if (fromSatchel != null)
            this.Pick(fromSatchel, this.SatchelItems);
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
        => this.receiveLeftClick(x, y, playSound);

    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape || Game1.options.doesInputListContain(Game1.options.menuButton, key))
        {
            this.exitThisMenu();
            return;
        }
        base.receiveKeyPress(key);
    }

    public override void receiveGamePadButton(Buttons b)
    {
        if (b == Buttons.Y)
            this.Try();
        else
            base.receiveGamePadButton(b);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.hovered = MenuKit.ItemAt(this.playerInventory, x, y).Item ?? MenuKit.ItemAt(this.satchelInventory, x, y).Item;
        for (int i = 0; i < this.slots.Count && i < this.picked.Count; i++)
        {
            if (this.slots[i].containsPoint(x, y))
                this.hovered = this.picked[i].Item;
        }
    }

    /// <summary>Puts one of an item into the next free slot; the item stays where it is until the attempt.</summary>
    private void Pick(Item item, IInventory from)
    {
        if (this.picked.Count >= this.capacity)
        {
            this.outcome = ModServices.T("ui.table.slots-full");
            Game1.playSound("cancel");
            return;
        }
        int used = this.picked.Count(p => ReferenceEquals(p.Item, item));
        if (used >= item.Stack)
        {
            Game1.playSound("cancel");
            return;
        }
        this.picked.Add((item, from));
        this.outcome = null;
        Game1.playSound("smallSelect");
        if (Game1.options.SnappyMenus && this.picked.Count >= this.capacity)
            MenuKit.Snap(this, TryId);
    }

    private void Try()
    {
        if (this.picked.Count == 0)
        {
            Game1.playSound("cancel");
            return;
        }
        var inputs = this.picked.ToList();
        this.picked.Clear();
        try
        {
            TableOutcome result = ModServices.Machines.AttemptTable(this.table, inputs, Game1.player);
            this.outcome = result.Kind switch
            {
                TableOutcomeKind.Success => ModServices.T("ui.table.success", new { name = ModServices.Content.NameOf(result.Recipe!.Row.Output.Id) }),
                TableOutcomeKind.Blocked => ModServices.T("table.blocked"),
                TableOutcomeKind.Undefined => ModServices.T("table.undefined"),
                TableOutcomeKind.TooManyInputs => ModServices.T("ui.table.too-many"),
                _ => null,
            };
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"The Table attempt failed: {ex}");
            this.outcome = ModServices.T("ui.error");
        }
    }

    private bool AlreadyTried
        => this.picked.Count == this.capacity
            && ModServices.Farm.Codex.Has(CodexKeys.Tried(this.picked.Select(p => p.Item.QualifiedItemId)));

    // ── Drawing ─────────────────────────────────────────────────────────────────

    public override void draw(SpriteBatch b)
    {
        MenuKit.DrawBackdrop(b);
        MenuKit.DrawPanel(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        int left = this.xPositionOnScreen + Pad;
        int innerWidth = this.width - Pad * 2;
        MenuKit.DrawText(b, ModServices.T("ui.table.title"), left, this.yPositionOnScreen + Pad, font: Game1.dialogueFont);

        for (int i = 0; i < this.slots.Count; i++)
        {
            Rectangle r = this.slots[i].bounds;
            MenuKit.DrawSlot(b, r);
            if (i < this.picked.Count)
                this.picked[i].Item.drawInMenu(b, new Vector2(r.X, r.Y), 1f, 1f, 0.9f, StackDrawType.Hide);
            if (i < this.slots.Count - 1)
                MenuKit.DrawText(b, "+", r.Right + 16, r.Y + 16, font: Game1.dialogueFont);
        }
        MenuKit.DrawButton(b, this.tryButton, ModServices.T("ui.table.try"), this.picked.Count > 0);

        int lineY = this.outcomeTop;
        if (this.AlreadyTried)
            lineY += MenuKit.DrawWrapped(b, ModServices.T("table.already-tried"), left, lineY, innerWidth, Color.DarkRed);
        if (this.outcome != null)
            MenuKit.DrawWrapped(b, this.outcome, left, lineY, innerWidth);
        else if (this.picked.Count == 0 && !this.AlreadyTried)
            MenuKit.DrawWrapped(b, ModServices.T("ui.table.hint"), left, lineY, innerWidth, Color.DimGray);

        MenuKit.DrawText(b, ModServices.T("ui.table.inventory"), left, this.playerLabelTop);
        this.playerInventory.draw(b);
        MenuKit.DrawText(b, ModServices.T("ui.table.satchel"), left, this.satchelLabelTop);
        this.satchelInventory.draw(b);

        base.draw(b);
        if (this.hovered != null)
            drawToolTip(b, this.hovered.getDescription(), this.hovered.DisplayName, this.hovered);
        this.drawMouse(b);
    }
}
