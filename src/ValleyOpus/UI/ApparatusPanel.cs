using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using SObject = StardewValley.Object;

namespace ValleyOpus.UI;

/// <summary>
/// One panel for an apparatus (doc 16 §4–5): its hover lines, the tray with take-back, Seal, the pin toggle, and the
/// recipes it can run now with how many loads the player carries. The picker opens with the recipe list focused.
/// </summary>
internal sealed class ApparatusPanel : IClickableMenu
{
    private const int TrayBaseId = 100;
    private const int SealId = 200;
    private const int PinId = 201;
    private const int RowBaseId = 300;
    private const int UpId = 400;
    private const int DownId = 401;
    private const int Pad = 32;
    private const int RowHeight = 64;
    private const int PanelWidth = 760;

    private readonly SObject machine;
    private readonly bool pickerMode;
    private readonly List<ClickableComponent> trayButtons = new();
    private readonly List<ClickableComponent> rowButtons = new();
    private readonly ClickableComponent sealButton = new(Rectangle.Empty, "seal") { myID = SealId };
    private readonly ClickableComponent pinButton = new(Rectangle.Empty, "pin") { myID = PinId };
    private readonly ClickableTextureComponent upArrow = new(Rectangle.Empty, Game1.mouseCursors, MenuKit.UpArrowSource, 4f) { myID = UpId };
    private readonly ClickableTextureComponent downArrow = new(Rectangle.Empty, Game1.mouseCursors, MenuKit.DownArrowSource, 4f) { myID = DownId };

    private List<string> lines = new();
    private IReadOnlyList<Item> tray = Array.Empty<Item>();
    private List<(CompiledRecipe Recipe, int Loads)> recipes = new();
    private bool pinned;
    private string? message;
    private string? hoverText;
    private int scroll;
    private int visibleRows;
    private int linesTop;
    private int trayTop;
    private int buttonsTop;
    private int messageTop;
    private int listTop;

    private ApparatusPanel(SObject machine, bool pickerMode)
    {
        this.machine = machine;
        this.pickerMode = pickerMode;
        this.Refresh();
        if (pickerMode && MachineService.State(machine).RecipeMemory is string memory)
        {
            int index = this.recipes.FindIndex(r => r.Recipe.Id == memory);
            if (index >= this.visibleRows)
                this.scroll = Math.Min(index, Math.Max(0, this.recipes.Count - this.visibleRows));
        }
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    public static void Open(SObject machine, bool pickerMode)
    {
        if (!Context.IsWorldReady || ModServices.Content?.ApparatusOf(machine) is null)
            return;
        Game1.activeClickableMenu = new ApparatusPanel(machine, pickerMode);
    }

    private Farmer Who => Game1.player;

    /// <summary>Re-reads the machine and the player's carried items, then lays the panel out again.</summary>
    private void Refresh()
    {
        this.lines = ModServices.Machines.Describe(this.machine, this.Who);
        this.tray = MachineService.TrayItems(this.machine);
        this.pinned = MachineService.State(this.machine).Pinned;
        this.recipes = ModServices.Machines.Runnable(this.machine, this.Who)
            .Select(r => (r, ModServices.Machines.Loads(r, this.Who)))
            .ToList();
        this.Layout();
    }

    private void Layout()
    {
        int innerWidth = PanelWidth - Pad * 2;
        int linesHeight = this.lines.Sum(l => MenuKit.WrappedHeight(l, innerWidth));
        int fixedHeight = Pad + 52 + linesHeight + 12 + 36 + 72 + 80 + 40 + 36 + Pad;
        int room = Game1.uiViewport.Height - 32 - fixedHeight;
        this.visibleRows = Math.Clamp(room / RowHeight, 2, 6);
        int height = fixedHeight + this.visibleRows * RowHeight;
        Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(PanelWidth, height);
        this.initialize((int)topLeft.X, (int)topLeft.Y, PanelWidth, height, showUpperRightCloseButton: true);
        this.scroll = Math.Clamp(this.scroll, 0, Math.Max(0, this.recipes.Count - this.visibleRows));

        int left = this.xPositionOnScreen + Pad;
        this.linesTop = this.yPositionOnScreen + Pad + 52;
        this.trayTop = this.linesTop + linesHeight + 12;
        this.buttonsTop = this.trayTop + 36 + 72;
        this.messageTop = this.buttonsTop + 80;
        this.listTop = this.messageTop + 40 + 36;

        bool hasTray = this.tray.Count > 0;
        int firstRow = this.recipes.Count > 0 ? RowBaseId : -1;
        int buttonAbove = hasTray ? TrayBaseId : -1;

        this.trayButtons.Clear();
        for (int i = 0; i < this.tray.Count; i++)
        {
            this.trayButtons.Add(new ClickableComponent(new Rectangle(left + i * 72, this.trayTop + 36, 64, 64), "tray" + i)
            {
                myID = TrayBaseId + i,
                leftNeighborID = i > 0 ? TrayBaseId + i - 1 : -1,
                rightNeighborID = i < this.tray.Count - 1 ? TrayBaseId + i + 1 : -1,
                downNeighborID = hasTray ? SealId : PinId,
            });
        }

        this.sealButton.bounds = hasTray ? new Rectangle(left, this.buttonsTop, 200, 64) : Rectangle.Empty;
        this.sealButton.upNeighborID = buttonAbove;
        this.sealButton.rightNeighborID = PinId;
        this.sealButton.downNeighborID = firstRow;
        this.pinButton.bounds = new Rectangle(left + (hasTray ? 216 : 0), this.buttonsTop, 260, 64);
        this.pinButton.upNeighborID = buttonAbove;
        this.pinButton.leftNeighborID = hasTray ? SealId : -1;
        this.pinButton.downNeighborID = firstRow;

        int rowWidth = innerWidth - 64;
        this.rowButtons.Clear();
        int shown = Math.Min(this.visibleRows, this.recipes.Count);
        for (int i = 0; i < shown; i++)
        {
            this.rowButtons.Add(new ClickableComponent(new Rectangle(left, this.listTop + i * RowHeight, rowWidth, RowHeight), "row" + i)
            {
                myID = RowBaseId + i,
                upNeighborID = i > 0 ? RowBaseId + i - 1 : (hasTray ? SealId : PinId),
                downNeighborID = i < shown - 1 ? RowBaseId + i + 1 : DownId,
                rightNeighborID = i < shown / 2 ? UpId : DownId,
            });
        }
        int arrowX = left + rowWidth + 16;
        this.upArrow.bounds = new Rectangle(arrowX, this.listTop, 44, 48);
        this.upArrow.leftNeighborID = RowBaseId;
        this.upArrow.downNeighborID = DownId;
        this.downArrow.bounds = new Rectangle(arrowX, this.listTop + this.visibleRows * RowHeight - 48, 44, 48);
        this.downArrow.leftNeighborID = RowBaseId + Math.Max(0, shown - 1);
        this.downArrow.upNeighborID = UpId;

        this.allClickableComponents = new List<ClickableComponent>();
        this.allClickableComponents.AddRange(this.trayButtons);
        if (hasTray)
            this.allClickableComponents.Add(this.sealButton);
        this.allClickableComponents.Add(this.pinButton);
        this.allClickableComponents.AddRange(this.rowButtons);
        if (this.recipes.Count > this.visibleRows)
        {
            this.allClickableComponents.Add(this.upArrow);
            this.allClickableComponents.Add(this.downArrow);
        }
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    public override void snapToDefaultClickableComponent()
    {
        int id;
        if (this.pickerMode && this.recipes.Count > 0)
        {
            string? memory = MachineService.State(this.machine).RecipeMemory;
            int index = this.recipes.FindIndex(r => r.Recipe.Id == memory) - this.scroll;
            id = RowBaseId + (index >= 0 && index < this.rowButtons.Count ? index : 0);
        }
        else
            id = this.tray.Count > 0 ? TrayBaseId : PinId;
        MenuKit.Snap(this, id);
    }

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
        try
        {
            this.Click(x, y);
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"The apparatus panel failed on {this.machine.QualifiedItemId}: {ex}");
            this.message = ModServices.T("ui.error");
        }
    }

    private void Click(int x, int y)
    {
        for (int i = 0; i < this.trayButtons.Count; i++)
        {
            if (!this.trayButtons[i].containsPoint(x, y) || i >= this.tray.Count)
                continue;
            ModServices.Machines.TakeBack(this.machine, this.tray[i], this.Who);
            Game1.playSound("dwop");
            this.message = null;
            this.Refresh();
            this.Resnap(Math.Min(TrayBaseId + i, TrayBaseId + this.tray.Count - 1));
            return;
        }

        if (this.tray.Count > 0 && this.sealButton.containsPoint(x, y))
        {
            StageRefusal why = ModServices.Machines.Seal(this.machine, this.Who);
            if (why == StageRefusal.None)
            {
                this.exitThisMenu();
                return;
            }
            Game1.playSound("cancel");
            this.message = MachineService.Reason(why);
            this.Refresh();
            return;
        }

        if (this.pinButton.containsPoint(x, y))
        {
            ModServices.Machines.SetPinned(this.machine, !this.pinned);
            Game1.playSound("drumkit6");
            this.Refresh();
            this.Resnap(PinId);
            return;
        }

        if (this.recipes.Count > this.visibleRows)
        {
            if (this.upArrow.containsPoint(x, y))
            {
                this.Scroll(-1);
                return;
            }
            if (this.downArrow.containsPoint(x, y))
            {
                this.Scroll(1);
                return;
            }
        }

        for (int i = 0; i < this.rowButtons.Count; i++)
        {
            int index = this.scroll + i;
            if (!this.rowButtons[i].containsPoint(x, y) || index >= this.recipes.Count)
                continue;
            (CompiledRecipe recipe, int loads) = this.recipes[index];
            if (loads <= 0)
            {
                Game1.playSound("cancel");
                this.message = ModServices.T("ui.panel.missing");
                return;
            }
            StageRefusal why = ModServices.Machines.LoadRecipe(this.machine, recipe, this.Who);
            if (why == StageRefusal.None)
            {
                this.exitThisMenu();
                return;
            }
            Game1.playSound("cancel");
            this.message = MachineService.Reason(why);
            this.Refresh();
            return;
        }
    }

    private void Scroll(int delta)
    {
        int max = Math.Max(0, this.recipes.Count - this.visibleRows);
        int next = Math.Clamp(this.scroll + delta, 0, max);
        if (next == this.scroll)
            return;
        this.scroll = next;
        Game1.playSound("shwip");
    }

    private void Resnap(int id)
    {
        if (Game1.options.SnappyMenus)
            MenuKit.Snap(this, id);
    }

    public override void receiveScrollWheelAction(int direction)
        => this.Scroll(direction > 0 ? -1 : 1);

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
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
        this.hoverText = null;
        for (int i = 0; i < this.trayButtons.Count && i < this.tray.Count; i++)
        {
            if (this.trayButtons[i].containsPoint(x, y))
                this.hoverText = ModServices.T("ui.panel.take-back", new { name = this.tray[i].DisplayName });
        }
        for (int i = 0; i < this.rowButtons.Count; i++)
        {
            int index = this.scroll + i;
            if (this.rowButtons[i].containsPoint(x, y) && index < this.recipes.Count && this.recipes[index].Loads <= 0)
                this.hoverText = ModServices.T("ui.panel.missing");
        }
        this.upArrow.tryHover(x, y);
        this.downArrow.tryHover(x, y);
    }

    // ── Drawing ─────────────────────────────────────────────────────────────────

    public override void draw(SpriteBatch b)
    {
        MenuKit.DrawBackdrop(b);
        MenuKit.DrawPanel(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        int left = this.xPositionOnScreen + Pad;
        int innerWidth = this.width - Pad * 2;

        string title = this.machine.DisplayName + (this.pickerMode ? "  ·  " + ModServices.T("ui.panel.picker") : "");
        MenuKit.DrawText(b, MenuKit.Fit(title, innerWidth - 48), left, this.yPositionOnScreen + Pad, font: Game1.dialogueFont);

        int y = this.linesTop;
        foreach (string line in this.lines)
            y += MenuKit.DrawWrapped(b, line, left, y, innerWidth, Color.DimGray);

        MenuKit.DrawText(b, ModServices.T(this.tray.Count > 0 ? "ui.panel.tray" : "ui.panel.tray-empty"), left, this.trayTop);
        for (int i = 0; i < this.trayButtons.Count && i < this.tray.Count; i++)
        {
            Rectangle r = this.trayButtons[i].bounds;
            MenuKit.DrawSlot(b, r);
            this.tray[i].drawInMenu(b, new Vector2(r.X, r.Y), 1f);
        }

        if (this.tray.Count > 0)
            MenuKit.DrawButton(b, this.sealButton, ModServices.T("ui.panel.seal"));
        MenuKit.DrawButton(b, this.pinButton, ModServices.T(this.pinned ? "ui.panel.pinned" : "ui.panel.unpinned"));

        if (this.message != null)
            MenuKit.DrawText(b, MenuKit.Fit(this.message, innerWidth), left, this.messageTop, Color.DarkRed);

        MenuKit.DrawText(b, ModServices.T("ui.panel.recipes"), left, this.listTop - 36);
        if (this.recipes.Count == 0)
            MenuKit.DrawWrapped(b, ModServices.T("ui.panel.no-recipes"), left, this.listTop + 8, innerWidth, Color.DimGray);
        for (int i = 0; i < this.rowButtons.Count; i++)
        {
            int index = this.scroll + i;
            if (index >= this.recipes.Count)
                break;
            this.DrawRow(b, this.rowButtons[i], this.recipes[index].Recipe, this.recipes[index].Loads);
        }
        if (this.recipes.Count > this.visibleRows)
        {
            this.upArrow.draw(b);
            this.downArrow.draw(b);
        }

        base.draw(b);
        if (this.hoverText != null)
            drawHoverText(b, this.hoverText, Game1.smallFont);
        this.drawMouse(b);
    }

    private void DrawRow(SpriteBatch b, ClickableComponent row, CompiledRecipe recipe, int loads)
    {
        Rectangle r = row.bounds;
        bool hovered = row.containsPoint(Game1.getMouseX(), Game1.getMouseY());
        if (hovered)
            b.Draw(Game1.staminaRect, r, Color.Wheat * 0.6f);
        float alpha = loads > 0 ? 1f : 0.45f;
        string outputId = recipe.Row.Output.Id;
        if (!MenuKit.DrawItemIcon(b, ModServices.Content.Registry.ResolveItemId(outputId), new Rectangle(r.X + 8, r.Y + 8, 48, 48), Color.White * alpha))
            MenuKit.DrawText(b, "?", r.X + 24, r.Y + 16, Game1.textColor * alpha);

        string count = ModServices.T("ui.panel.loads", new { count = loads });
        int countWidth = (int)Game1.smallFont.MeasureString(count).X;
        int textWidth = r.Width - 72 - countWidth - 16;
        MenuKit.DrawText(b, MenuKit.Fit(ModServices.Content.NameOf(outputId), textWidth), r.X + 64, r.Y + 4, Game1.textColor * alpha);
        MenuKit.DrawText(b, MenuKit.Fit(MenuKit.InputNames(recipe.Row), (int)(textWidth / 0.8f), 0.8f), r.X + 64, r.Y + 34, Color.DimGray * alpha, 0.8f);
        MenuKit.DrawText(b, count, r.Right - countWidth - 8, r.Y + 18, Game1.textColor * alpha);
        b.Draw(Game1.staminaRect, new Rectangle(r.X, r.Bottom - 2, r.Width, 2), Color.SaddleBrown * 0.3f);
    }
}
