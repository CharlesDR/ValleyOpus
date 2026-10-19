using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.UI;

/// <summary>Drawing and layout helpers the ValleyOpus menus share: icons, slots, text buttons, lists and inventory wiring.</summary>
internal static class MenuKit
{
    /// <summary>The height of one inventory row as <see cref="InventoryMenu"/> lays it out (64 px slot + 4 px gap).</summary>
    public const int InventoryRowHeight = 68;

    public static readonly Rectangle UpArrowSource = new(421, 459, 11, 12);
    public static readonly Rectangle DownArrowSource = new(421, 472, 11, 12);
    private static readonly Rectangle ButtonSource = new(432, 439, 9, 9);

    public static void DrawBackdrop(SpriteBatch b)
        => b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

    public static void DrawPanel(SpriteBatch b, int x, int y, int width, int height)
        => IClickableMenu.drawTextureBox(b, x, y, width, height, Color.White);

    public static void DrawSlot(SpriteBatch b, Rectangle bounds)
        => b.Draw(Game1.menuTexture, bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);

    /// <summary>Draws an item's sprite scaled to fit an area; returns false when the item ID is unknown.</summary>
    public static bool DrawItemIcon(SpriteBatch b, string? qualifiedId, Rectangle area, Color color)
    {
        if (qualifiedId is null)
            return false;
        ParsedItemData? data = ItemRegistry.GetData(qualifiedId);
        if (data is null)
            return false;
        Texture2D texture = data.GetTexture();
        Rectangle source = data.GetSourceRect();
        if (source.Width <= 0 || source.Height <= 0)
            return false;
        float scale = Math.Min(area.Width / (float)source.Width, area.Height / (float)source.Height);
        int w = (int)(source.Width * scale), h = (int)(source.Height * scale);
        b.Draw(texture, new Rectangle(area.X + (area.Width - w) / 2, area.Y + (area.Height - h) / 2, w, h), source, color);
        return true;
    }

    public static void DrawText(SpriteBatch b, string text, int x, int y, Color? color = null, float scale = 1f, SpriteFont? font = null)
        => Utility.drawTextWithShadow(b, text, font ?? Game1.smallFont, new Vector2(x, y), color ?? Game1.textColor, scale);

    /// <summary>Draws wrapped text and returns its height.</summary>
    public static int DrawWrapped(SpriteBatch b, string text, int x, int y, int width, Color? color = null)
    {
        string wrapped = Game1.parseText(text, Game1.smallFont, width);
        DrawText(b, wrapped, x, y, color);
        return (int)Game1.smallFont.MeasureString(wrapped).Y;
    }

    public static int WrappedHeight(string text, int width)
        => (int)Game1.smallFont.MeasureString(Game1.parseText(text, Game1.smallFont, width)).Y;

    /// <summary>A text button: a small texture box with a centred label.</summary>
    public static void DrawButton(SpriteBatch b, ClickableComponent button, string label, bool enabled = true)
    {
        bool hovered = enabled && button.containsPoint(Game1.getMouseX(), Game1.getMouseY());
        Rectangle r = button.bounds;
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, ButtonSource, r.X, r.Y, r.Width, r.Height, hovered ? Color.Wheat : Color.White, 4f, false);
        string text = Fit(label, r.Width - 16);
        Vector2 size = Game1.smallFont.MeasureString(text);
        DrawText(b, text, r.X + (r.Width - (int)size.X) / 2, r.Y + (r.Height - (int)size.Y) / 2 + 2, enabled ? Game1.textColor : Color.Gray);
    }

    /// <summary>Cuts a line to a width with an ellipsis.</summary>
    public static string Fit(string text, int width, float scale = 1f)
    {
        SpriteFont font = Game1.smallFont;
        if (font.MeasureString(text).X * scale <= width)
            return text;
        string cut = text;
        while (cut.Length > 1 && font.MeasureString(cut + "...").X * scale > width)
            cut = cut[..^1];
        return cut + "...";
    }

    /// <summary>The slot index and item under a point in an inventory menu.</summary>
    public static (int Index, Item? Item) ItemAt(InventoryMenu menu, int x, int y)
    {
        for (int i = 0; i < menu.inventory.Count; i++)
        {
            if (menu.inventory[i].containsPoint(x, y))
                return (i, i < menu.actualInventory.Count ? menu.actualInventory[i] : null);
        }
        return (-1, null);
    }

    /// <summary>Renumbers an inventory grid's controller IDs from <paramref name="baseId"/> and links its edges outward.</summary>
    public static void WireGrid(InventoryMenu menu, int baseId, Func<int, int> upEdge, Func<int, int> downEdge)
    {
        int columns = Math.Max(1, menu.capacity / Math.Max(1, menu.rows));
        int count = menu.inventory.Count;
        for (int i = 0; i < count; i++)
        {
            int row = i / columns, col = i % columns;
            ClickableComponent c = menu.inventory[i];
            c.myID = baseId + i;
            c.leftNeighborID = col > 0 ? baseId + i - 1 : -1;
            c.rightNeighborID = col < columns - 1 && i + 1 < count ? baseId + i + 1 : -1;
            c.upNeighborID = row > 0 ? baseId + i - columns : upEdge(col);
            c.downNeighborID = i + columns < count ? baseId + i + columns : downEdge(col);
        }
    }

    public static int Rows(int capacity, int columns = 12) => Math.Max(1, (capacity + columns - 1) / columns);

    public static string ItemName(string qualifiedId) => ItemRegistry.GetData(qualifiedId)?.DisplayName ?? qualifiedId;

    public static string? ItemDescription(string? qualifiedId) => qualifiedId is null ? null : ItemRegistry.GetData(qualifiedId)?.Description;

    /// <summary>A recipe's inputs as one line: "Ignis + Aqua x2".</summary>
    public static string InputNames(RecipeRow row) => string.Join(" + ", row.Inputs.Select(InputName));

    public static string InputName(IngredientSpec spec)
    {
        string name = spec.Item != null ? ModServices.Content.NameOf(spec.Item)
            : spec.Tag != null ? ModServices.T("ui.input.tag", new { tag = spec.Tag })
            : spec.Category is int category ? ModServices.T("ui.input.category", new { category = SObject.GetCategoryDisplayName(category) })
            : "?";
        return spec.Count > 1 ? ModServices.T("ui.input.count", new { name, count = spec.Count }) : name;
    }

    /// <summary>Snaps the controller cursor to a component by ID, or to the first component when it is missing.</summary>
    public static void Snap(IClickableMenu menu, int id)
    {
        ClickableComponent? target = menu.getComponentWithID(id) ?? menu.allClickableComponents?.FirstOrDefault();
        if (target is null)
            return;
        menu.currentlySnappedComponent = target;
        menu.snapCursorToCurrentSnappedComponent();
    }
}
