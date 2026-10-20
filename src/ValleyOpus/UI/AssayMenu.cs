using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using ValleyOpus.Framework;

namespace ValleyOpus.UI;

/// <summary>The Assayer's Lens on the inventory (doc 16 §11): click an item to assay it and read its Prime profile.</summary>
internal sealed class AssayMenu : IClickableMenu
{
    private const int PlayerBaseId = 1000;
    private const int Pad = 32;
    private const int ResultHeight = 120;

    private readonly Farmer who;
    private InventoryMenu playerInventory = null!;
    private int resultTop;
    private Item? assayed;
    private string? profile;
    private Item? hovered;

    private AssayMenu(Farmer who)
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
        Game1.activeClickableMenu = new AssayMenu(who);
    }

    private void Layout()
    {
        int playerRows = MenuKit.PlayerInventoryRows;
        int width = 12 * 64 + Pad * 2;
        int height = Pad + 52 + ResultHeight + 16 + playerRows * MenuKit.InventoryRowHeight + Pad;
        Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        this.initialize((int)topLeft.X, (int)topLeft.Y, width, height, true);

        this.resultTop = this.yPositionOnScreen + Pad + 52;
        int playerTop = this.resultTop + ResultHeight + 16;
        this.playerInventory = new InventoryMenu(this.xPositionOnScreen + Pad, playerTop, true, this.who.Items, InventoryMenu.highlightAllItems);
        MenuKit.WireGrid(this.playerInventory, PlayerBaseId, upEdge: _ => -1, downEdge: _ => -1);

        this.allClickableComponents = new List<ClickableComponent>(this.playerInventory.inventory);
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    public override void snapToDefaultClickableComponent() => MenuKit.Snap(this, PlayerBaseId);

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.upperRightCloseButton?.containsPoint(x, y) == true)
        {
            base.receiveLeftClick(x, y, playSound);
            return;
        }
        Item? item = MenuKit.ItemAt(this.playerInventory, x, y).Item;
        if (item is null)
            return;
        try
        {
            this.profile = ModServices.Satchel.Assay(this.who, item);
            this.assayed = item;
            Game1.playSound("smallSelect");
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"Assaying {item.QualifiedItemId} failed: {ex}");
            this.assayed = item;
            this.profile = ModServices.T("ui.error");
        }
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
        this.hovered = MenuKit.ItemAt(this.playerInventory, x, y).Item;
    }

    public override void draw(SpriteBatch b)
    {
        MenuKit.DrawBackdrop(b);
        MenuKit.DrawPanel(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        int left = this.xPositionOnScreen + Pad;
        int innerWidth = this.width - Pad * 2;
        MenuKit.DrawText(b, ModServices.T("ui.assay.title"), left, this.yPositionOnScreen + Pad, font: Game1.dialogueFont);

        if (this.assayed is null)
            MenuKit.DrawWrapped(b, ModServices.T("ui.assay.hint"), left, this.resultTop + 8, innerWidth, Color.DimGray);
        else
        {
            var slot = new Rectangle(left, this.resultTop + 8, 64, 64);
            MenuKit.DrawSlot(b, slot);
            this.assayed.drawInMenu(b, new Vector2(slot.X, slot.Y), 1f, 1f, 0.9f, StackDrawType.Hide);
            MenuKit.DrawText(b, MenuKit.Fit(this.assayed.DisplayName, innerWidth - 80), left + 80, this.resultTop + 8);
            MenuKit.DrawWrapped(b, this.profile ?? "", left + 80, this.resultTop + 44, innerWidth - 80, Color.DimGray);
        }

        this.playerInventory.draw(b);
        base.draw(b);
        if (this.hovered != null)
            drawToolTip(b, this.hovered.getDescription(), this.hovered.DisplayName, this.hovered);
        this.drawMouse(b);
    }
}
