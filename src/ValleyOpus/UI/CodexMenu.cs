using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.Satchel;

namespace ValleyOpus.UI;

/// <summary>
/// The codex (doc 07, doc 16 §7): one tab per <c>Codex/Pages</c> row plus Notes. The Tier 1 page is the triangle of
/// Prime pairs; the other pages are lists of entries, with unknown ones drawn as silhouettes.
/// </summary>
internal sealed class CodexMenu : IClickableMenu
{
    private const int TabBaseId = 100;
    private const int RowBaseId = 300;
    private const int UpId = 400;
    private const int DownId = 401;
    private const int CellBaseId = 1000;
    private const int Pad = 32;
    private const int TabHeight = 56;
    private const int RowHeight = 64;

    private readonly List<Page> pages;
    private readonly List<ClickableComponent> tabs = new();
    private readonly List<ClickableComponent> rowButtons = new();
    private readonly List<Cell> cells = new();
    private readonly ClickableTextureComponent upArrow = new(Rectangle.Empty, Game1.mouseCursors, MenuKit.UpArrowSource, 4f) { myID = UpId };
    private readonly ClickableTextureComponent downArrow = new(Rectangle.Empty, Game1.mouseCursors, MenuKit.DownArrowSource, 4f) { myID = DownId };

    private List<Row> rows = new();
    private List<PrimeRow> primes = new();
    private string? placeholder;
    private int current;
    private int scroll;
    private int visibleRows;
    private int contentTop;
    private int cellSize;
    private Point gridOrigin;
    private string? hoverTitle;
    private string? hoverText;

    private enum PageKind { Primes, Tier1, Tier2, Apparatus, Unlocks, Notes, Other }

    private sealed record Page(PageKind Kind, string Title);

    private sealed record Row(string? IconId, bool Silhouette, string Title, string? Detail, string? Hover, bool Header = false);

    private sealed record Cell(GridCell Data, ClickableComponent Button, string? Title, string Hover);

    private CodexMenu()
    {
        this.pages = BuildPages();
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    public static void Open()
    {
        if (!Context.IsWorldReady || ModServices.Content is null)
            return;
        try
        {
            Game1.activeClickableMenu = new CodexMenu();
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"The codex couldn't open: {ex}");
        }
    }

    private static List<Page> BuildPages()
    {
        var pages = new List<Page>();
        IEnumerable<CodexPageRow> rows = ModServices.Content.Registry.CodexPages.All.OrderBy(p => p.SortOrder);
        foreach (CodexPageRow row in rows)
        {
            PageKind kind = Enum.TryParse(row.Group, out PageKind parsed) && parsed is not PageKind.Notes and not PageKind.Other ? parsed : PageKind.Other;
            string title = kind == PageKind.Other ? row.Id : ModServices.T("ui.codex.page." + kind);
            pages.Add(new Page(kind, title));
        }
        if (pages.Count == 0)
        {
            foreach (PageKind kind in new[] { PageKind.Primes, PageKind.Tier1, PageKind.Tier2, PageKind.Apparatus, PageKind.Unlocks })
                pages.Add(new Page(kind, ModServices.T("ui.codex.page." + kind)));
        }
        pages.Add(new Page(PageKind.Notes, ModServices.T("ui.codex.page.Notes")));
        return pages;
    }

    private Page CurrentPage => this.pages[this.current];

    // ── Layout ──────────────────────────────────────────────────────────────────

    private void Layout()
    {
        int width = Math.Min(1040, Game1.uiViewport.Width - 32);
        int height = Math.Min(720, Game1.uiViewport.Height - 32);
        Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        this.initialize((int)topLeft.X, (int)topLeft.Y, width, height, true);

        int left = this.xPositionOnScreen + Pad;
        int innerWidth = width - Pad * 2 - 48;
        int tabWidth = innerWidth / Math.Max(1, this.pages.Count);
        this.tabs.Clear();
        for (int i = 0; i < this.pages.Count; i++)
        {
            this.tabs.Add(new ClickableComponent(new Rectangle(left + i * tabWidth, this.yPositionOnScreen + Pad, tabWidth - 4, TabHeight), "tab" + i)
            {
                myID = TabBaseId + i,
                leftNeighborID = i > 0 ? TabBaseId + i - 1 : -1,
                rightNeighborID = i < this.pages.Count - 1 ? TabBaseId + i + 1 : -1,
            });
        }
        this.contentTop = this.yPositionOnScreen + Pad + TabHeight + 24;
        this.BuildPage();
    }

    /// <summary>Builds the current page's entries once, so drawing does no registry or codex work.</summary>
    private void BuildPage()
    {
        this.rows = new List<Row>();
        this.cells.Clear();
        this.rowButtons.Clear();
        this.placeholder = null;
        this.hoverText = null;
        this.hoverTitle = null;

        try
        {
            switch (this.CurrentPage.Kind)
            {
                case PageKind.Primes:
                    this.rows = PrimeRows();
                    break;
                case PageKind.Tier1:
                    this.BuildGrid();
                    break;
                case PageKind.Tier2:
                    this.rows = TierRows(2);
                    break;
                case PageKind.Apparatus:
                    this.rows = ApparatusRows();
                    break;
                case PageKind.Unlocks:
                    this.placeholder = ModServices.T("codex.unlocks-placeholder");
                    break;
                case PageKind.Notes:
                    this.rows = NoteRows();
                    break;
                default:
                    this.placeholder = ModServices.T("ui.codex.empty-page");
                    break;
            }
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"The codex page '{this.CurrentPage.Kind}' failed to build: {ex}");
            this.placeholder = ModServices.T("ui.error");
        }

        int left = this.xPositionOnScreen + Pad;
        int listWidth = this.width - Pad * 2 - 64;
        int bottom = this.yPositionOnScreen + this.height - Pad;
        this.visibleRows = Math.Max(1, (bottom - this.contentTop) / RowHeight);
        this.scroll = Math.Clamp(this.scroll, 0, Math.Max(0, this.rows.Count - this.visibleRows));
        int shown = Math.Min(this.visibleRows, this.rows.Count);
        int tabAbove = TabBaseId + this.current;
        for (int i = 0; i < shown; i++)
        {
            this.rowButtons.Add(new ClickableComponent(new Rectangle(left, this.contentTop + i * RowHeight, listWidth, RowHeight), "row" + i)
            {
                myID = RowBaseId + i,
                upNeighborID = i > 0 ? RowBaseId + i - 1 : tabAbove,
                downNeighborID = i < shown - 1 ? RowBaseId + i + 1 : DownId,
                rightNeighborID = i < shown / 2 ? UpId : DownId,
            });
        }
        int arrowX = left + listWidth + 12;
        this.upArrow.bounds = new Rectangle(arrowX, this.contentTop, 44, 48);
        this.upArrow.leftNeighborID = RowBaseId;
        this.upArrow.upNeighborID = tabAbove;
        this.upArrow.downNeighborID = DownId;
        this.downArrow.bounds = new Rectangle(arrowX, this.contentTop + this.visibleRows * RowHeight - 48, 44, 48);
        this.downArrow.leftNeighborID = RowBaseId + Math.Max(0, shown - 1);
        this.downArrow.upNeighborID = UpId;

        int firstContent = this.cells.Count > 0 ? this.cells[0].Button.myID : shown > 0 ? RowBaseId : -1;
        foreach (ClickableComponent tab in this.tabs)
            tab.downNeighborID = firstContent;

        this.allClickableComponents = new List<ClickableComponent>(this.tabs);
        this.allClickableComponents.AddRange(this.rowButtons);
        this.allClickableComponents.AddRange(this.cells.Select(c => c.Button));
        if (this.HasScroll)
        {
            this.allClickableComponents.Add(this.upArrow);
            this.allClickableComponents.Add(this.downArrow);
        }
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    private bool HasScroll => this.rows.Count > this.visibleRows;

    private void BuildGrid()
    {
        GameContent content = ModServices.Content;
        this.primes = content.Grid.Primes.ToList();
        IReadOnlyList<GridCell> grid = content.Grid.TierOne(ModServices.Farm.Codex);
        int n = this.primes.Count;
        if (n == 0)
        {
            this.placeholder = ModServices.T("ui.codex.empty-page");
            return;
        }

        int areaWidth = this.width - Pad * 2;
        int areaHeight = this.yPositionOnScreen + this.height - Pad - this.contentTop;
        this.cellSize = Math.Max(24, Math.Min(64, Math.Min(areaWidth, areaHeight) / (n + 1)));
        this.gridOrigin = new Point(this.xPositionOnScreen + Pad + (areaWidth - (n + 1) * this.cellSize) / 2, this.contentTop);

        foreach (GridCell cell in grid)
        {
            int r = cell.Row, c = cell.Column;
            var bounds = new Rectangle(this.gridOrigin.X + (c + 1) * this.cellSize, this.gridOrigin.Y + (r + 1) * this.cellSize, this.cellSize, this.cellSize);
            var button = new ClickableComponent(bounds, $"cell{r}_{c}")
            {
                myID = CellId(r, c),
                leftNeighborID = c - 1 >= r ? CellId(r, c - 1) : -1,
                rightNeighborID = c + 1 < n ? CellId(r, c + 1) : -1,
                upNeighborID = r > 0 ? CellId(r - 1, c) : TabBaseId + this.current,
                downNeighborID = r + 1 <= c ? CellId(r + 1, c) : -1,
            };

            string pair = ModServices.T("ui.codex.pair", new { a = content.NameOf(cell.PrimeA), b = content.NameOf(cell.PrimeB) });
            string? title = null;
            string hover = pair;
            if (cell.State == CellState.Known && cell.OutputId != null)
                title = content.NameOf(cell.OutputId);
            else if (cell.State == CellState.Tried)
                hover = pair + "\n" + ModServices.T("ui.codex.tried");
            this.cells.Add(new Cell(cell, button, title, hover));
        }
    }

    private static int CellId(int row, int column) => CellBaseId + row * 100 + column;

    private static List<Row> PrimeRows()
    {
        GameContent content = ModServices.Content;
        return content.Grid.Primes
            .Select(p =>
            {
                string? itemId = content.Registry.ResolveItemId(p.Id);
                return new Row(itemId, false, content.NameOf(p.Id), MenuKit.ItemDescription(itemId), null);
            })
            .ToList();
    }

    private static List<Row> TierRows(int tier)
    {
        GameContent content = ModServices.Content;
        FarmCodex codex = ModServices.Farm.Codex;
        var rows = new List<Row>();
        foreach (SubstanceRow substance in content.Grid.Tier(tier))
        {
            string? itemId = content.Registry.ResolveItemId(substance.Id);
            List<RecipeRow> routes = content.Registry.Recipes.All
                .Where(r => r.Output.Id == substance.Id || (itemId != null && content.Registry.ResolveItemId(r.Output.Id) == itemId))
                .ToList();
            RecipeRow? discovered = routes.FirstOrDefault(r => codex.IsDiscovered(r.Id));
            bool known = discovered != null || (itemId != null && codex.Has(CodexKeys.Item(itemId)));
            if (!known)
            {
                rows.Add(new Row(itemId, true, "???", null, null));
                continue;
            }
            string name = content.NameOf(substance.Id);
            string? inputs = discovered != null ? MenuKit.InputNames(discovered) : null;
            // TODO: lore lines (Registry.Lore rows with TargetId == the item and Threshold <= codex.Data.Counts[CodexKeys.Count(itemId)]);
            // their TextKeys live in the content pack's i18n, so they need a pack translation lookup before they can show.
            rows.Add(new Row(itemId, false, name, inputs, inputs));
        }
        return rows;
    }

    private static List<Row> ApparatusRows()
    {
        GameContent content = ModServices.Content;
        return content.Registry.Apparatus.All
            .Where(a => a.ItemId != null)
            .Select(a =>
            {
                string itemId = content.Registry.ResolveItemId(a.ItemId!) ?? a.ItemId!;
                return new Row(itemId, false, MenuKit.ItemName(itemId), MenuKit.ItemDescription(itemId), null);
            })
            .ToList();
    }

    private static List<Row> NoteRows()
    {
        IEnumerable<string> keys = ModServices.Farm.Codex.Data.Keys.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
        var rows = new List<Row>();

        List<string> tried = keys.Where(k => k.StartsWith(CodexKeys.TriedPrefix, StringComparison.Ordinal)).ToList();
        if (tried.Count > 0)
        {
            rows.Add(new Row(null, false, ModServices.T("ui.codex.notes-tried"), null, null, Header: true));
            foreach (string key in tried)
            {
                string[] ids = key[CodexKeys.TriedPrefix.Length..].Split('+', StringSplitOptions.RemoveEmptyEntries);
                rows.Add(new Row(ids.FirstOrDefault(), false, string.Join(" + ", ids.Select(MenuKit.ItemName)), null, null));
            }
        }

        List<string> assayed = keys.Where(k => k.StartsWith(CodexKeys.AssayPrefix, StringComparison.Ordinal)).ToList();
        if (assayed.Count > 0)
        {
            rows.Add(new Row(null, false, ModServices.T("ui.codex.notes-assayed"), null, null, Header: true));
            foreach (string key in assayed)
            {
                string id = key[CodexKeys.AssayPrefix.Length..];
                string? profile = null;
                try
                {
                    profile = SatchelService.ProfileText(ItemRegistry.Create(id));
                }
                catch (Exception ex)
                {
                    ModServices.Log.Trace($"Codex notes: no profile for {id}: {ex.Message}");
                }
                rows.Add(new Row(id, false, MenuKit.ItemName(id), profile, profile));
            }
        }

        if (rows.Count == 0)
            rows.Add(new Row(null, false, ModServices.T("ui.codex.notes-empty"), null, null));
        return rows;
    }

    public override void snapToDefaultClickableComponent()
    {
        int id = this.cells.Count > 0 ? this.cells[0].Button.myID : this.rowButtons.Count > 0 ? RowBaseId : TabBaseId + this.current;
        MenuKit.Snap(this, id);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    // ── Input ───────────────────────────────────────────────────────────────────

    private void ShowPage(int index)
    {
        index = (index + this.pages.Count) % this.pages.Count;
        if (index == this.current)
            return;
        this.current = index;
        this.scroll = 0;
        Game1.playSound("smallSelect");
        this.BuildPage();
        if (Game1.options.SnappyMenus)
            MenuKit.Snap(this, TabBaseId + this.current);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.upperRightCloseButton?.containsPoint(x, y) == true)
        {
            base.receiveLeftClick(x, y, playSound);
            return;
        }
        for (int i = 0; i < this.tabs.Count; i++)
        {
            if (this.tabs[i].containsPoint(x, y))
            {
                this.ShowPage(i);
                return;
            }
        }
        if (this.HasScroll && this.upArrow.containsPoint(x, y))
            this.Scroll(-1);
        else if (this.HasScroll && this.downArrow.containsPoint(x, y))
            this.Scroll(1);
    }

    private void Scroll(int delta)
    {
        int next = Math.Clamp(this.scroll + delta, 0, Math.Max(0, this.rows.Count - this.visibleRows));
        if (next == this.scroll)
            return;
        this.scroll = next;
        Game1.playSound("shwip");
    }

    public override void receiveScrollWheelAction(int direction) => this.Scroll(direction > 0 ? -1 : 1);

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

    public override void receiveGamePadButton(Buttons b)
    {
        if (b is Buttons.LeftShoulder or Buttons.LeftTrigger)
            this.ShowPage(this.current - 1);
        else if (b is Buttons.RightShoulder or Buttons.RightTrigger)
            this.ShowPage(this.current + 1);
        else
            base.receiveGamePadButton(b);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.hoverTitle = null;
        this.hoverText = null;
        foreach (Cell cell in this.cells)
        {
            if (cell.Button.containsPoint(x, y))
            {
                this.hoverTitle = cell.Title;
                this.hoverText = cell.Hover;
            }
        }
        for (int i = 0; i < this.rowButtons.Count; i++)
        {
            int index = this.scroll + i;
            if (index < this.rows.Count && this.rowButtons[i].containsPoint(x, y) && this.rows[index].Hover is string hover)
            {
                this.hoverTitle = this.rows[index].Title;
                this.hoverText = hover;
            }
        }
        this.upArrow.tryHover(x, y);
        this.downArrow.tryHover(x, y);
    }

    // ── Drawing ─────────────────────────────────────────────────────────────────

    public override void draw(SpriteBatch b)
    {
        MenuKit.DrawBackdrop(b);
        MenuKit.DrawPanel(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);

        for (int i = 0; i < this.tabs.Count; i++)
        {
            ClickableComponent tab = this.tabs[i];
            if (i == this.current)
                b.Draw(Game1.staminaRect, new Rectangle(tab.bounds.X, tab.bounds.Bottom + 2, tab.bounds.Width, 4), Color.SaddleBrown);
            MenuKit.DrawButton(b, tab, this.pages[i].Title);
        }

        int left = this.xPositionOnScreen + Pad;
        if (this.placeholder != null)
            MenuKit.DrawWrapped(b, this.placeholder, left, this.contentTop + 8, this.width - Pad * 2, Color.DimGray);
        if (this.cells.Count > 0)
            this.DrawGrid(b);
        for (int i = 0; i < this.rowButtons.Count; i++)
        {
            int index = this.scroll + i;
            if (index >= this.rows.Count)
                break;
            this.DrawRow(b, this.rowButtons[i].bounds, this.rows[index]);
        }
        if (this.HasScroll)
        {
            this.upArrow.draw(b);
            this.downArrow.draw(b);
        }

        base.draw(b);
        if (this.hoverText != null)
            drawHoverText(b, this.hoverText, Game1.smallFont, boldTitleText: this.hoverTitle);
        this.drawMouse(b);
    }

    private void DrawRow(SpriteBatch b, Rectangle r, Row row)
    {
        if (row.Header)
        {
            MenuKit.DrawText(b, row.Title, r.X, r.Y + 24, Color.SaddleBrown);
            b.Draw(Game1.staminaRect, new Rectangle(r.X, r.Bottom - 6, r.Width, 2), Color.SaddleBrown * 0.5f);
            return;
        }
        int textLeft = r.X + 8;
        if (row.IconId != null)
        {
            MenuKit.DrawItemIcon(b, row.IconId, new Rectangle(r.X + 8, r.Y + 8, 48, 48), row.Silhouette ? Color.Black * 0.6f : Color.White);
            textLeft = r.X + 68;
        }
        int textWidth = r.Right - textLeft - 8;
        if (row.Detail is null)
            MenuKit.DrawText(b, MenuKit.Fit(row.Title, textWidth), textLeft, r.Y + 18);
        else
        {
            MenuKit.DrawText(b, MenuKit.Fit(row.Title, textWidth), textLeft, r.Y + 4);
            MenuKit.DrawText(b, MenuKit.Fit(row.Detail, textWidth, 0.8f), textLeft, r.Y + 34, Color.DimGray, 0.8f);
        }
        b.Draw(Game1.staminaRect, new Rectangle(r.X, r.Bottom - 2, r.Width, 2), Color.SaddleBrown * 0.2f);
    }

    private void DrawGrid(SpriteBatch b)
    {
        int size = this.cellSize;
        int inset = Math.Max(2, size / 10);
        GameContent content = ModServices.Content;
        for (int i = 0; i < this.primes.Count; i++)
        {
            string? itemId = content.Registry.ResolveItemId(this.primes[i].Id);
            MenuKit.DrawItemIcon(b, itemId, new Rectangle(this.gridOrigin.X + (i + 1) * size + inset, this.gridOrigin.Y + inset, size - inset * 2, size - inset * 2), Color.White);
            MenuKit.DrawItemIcon(b, itemId, new Rectangle(this.gridOrigin.X + inset, this.gridOrigin.Y + (i + 1) * size + inset, size - inset * 2, size - inset * 2), Color.White);
        }

        foreach (Cell cell in this.cells)
        {
            Rectangle r = cell.Button.bounds;
            var inner = new Rectangle(r.X + 2, r.Y + 2, r.Width - 4, r.Height - 4);
            MenuKit.DrawSlot(b, inner);
            if (cell.Button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                b.Draw(Game1.staminaRect, inner, Color.White * 0.25f);

            switch (cell.Data.State)
            {
                case CellState.Known:
                {
                    var icon = new Rectangle(r.X + inset, r.Y + inset, r.Width - inset * 2, r.Height - inset * 2);
                    string? outputItem = cell.Data.OutputId != null ? content.Registry.ResolveItemId(cell.Data.OutputId) : null;
                    if (!MenuKit.DrawItemIcon(b, outputItem, icon, Color.White))
                        DrawCentred(b, MenuKit.Fit(cell.Title ?? "?", r.Width - 4, 0.6f), r, Game1.textColor, 0.6f);
                    break;
                }
                case CellState.Warm:
                    b.Draw(Game1.staminaRect, inner, Color.Gold * 0.35f);
                    DrawCentred(b, "?", r, Color.DarkGoldenrod, 1f);
                    break;
                case CellState.Tried:
                    DrawCentred(b, "x", r, Color.DarkRed, 1f);
                    break;
                default:
                    DrawCentred(b, "?", r, Color.Gray, 1f);
                    break;
            }
        }
    }

    private static void DrawCentred(SpriteBatch b, string text, Rectangle r, Color color, float scale)
    {
        Vector2 size = Game1.smallFont.MeasureString(text) * scale;
        MenuKit.DrawText(b, text, r.X + (r.Width - (int)size.X) / 2, r.Y + (r.Height - (int)size.Y) / 2, color, scale);
    }
}
