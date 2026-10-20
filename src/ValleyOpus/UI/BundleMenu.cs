using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.World;

namespace ValleyOpus.UI;

/// <summary>
/// A room's bundle board (doc 14 §3, doc 16 bundle board), laid out like vanilla's Junimo note (SOS feedback): an
/// overview page of coloured bundle bags, and one page per bundle with its ingredients, its donation boxes and the
/// player's inventory; clicking an item donates it. Rewards stay hidden: they're paid when the bundle completes. From
/// the Library on, the overview also offers residue divination (doc 07 §3). Vanilla's note and cursor textures are
/// referenced at draw time, never copied.
/// </summary>
internal sealed class BundleMenu : IClickableMenu
{
    private const int NoteWidth = 1280;
    private const int NoteHeight = 720;
    private const int BundleBaseId = 100;
    private const int DivineId = 200;
    private const int BackId = 103;
    private const int PlayerBaseId = 1000;
    private const int IngredientBaseId = 3000;
    private const string NoteTextureName = "LooseSprites\\JunimoNote";
    private const string LibraryRoom = Ids.RowPrefix + "Library";

    /// <summary>Where vanilla puts bundle bags on a note, by bundle number (JunimoNoteMenu.getBundleLocationFromNumber).</summary>
    private static readonly Point[] BagSpots =
    {
        new(592, 136), new(392, 384), new(784, 388), new(304, 252), new(892, 252), new(588, 276), new(588, 380), new(440, 164), new(776, 164),
    };

    private readonly WorkshopRoomRow room;
    private readonly List<WorkshopBundleRow> bundles;
    private readonly List<ClickableComponent> bags = new();
    private readonly List<ClickableComponent> ingredients = new();
    private readonly List<Rectangle> boxes = new();
    private readonly List<TemporaryAnimatedSprite> sparkles = new();
    private readonly Texture2D note;
    private InventoryMenu inventory = null!;
    private ClickableComponent divineButton = null!;
    private ClickableTextureComponent backButton = null!;
    private int page = -1;
    private string? message;
    private string? hoverText;
    private Item? hovered;

    private BundleMenu(WorkshopRoomRow room)
        : base(0, 0, NoteWidth, NoteHeight, true)
    {
        this.room = room;
        this.bundles = room.Bundles.Select(id => ModServices.Content.Registry.Bundles.Get(id)).OfType<WorkshopBundleRow>().ToList();
        this.note = Game1.content.Load<Texture2D>(NoteTextureName);
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

    private WorkshopBundleRow? Current => this.page >= 0 && this.page < this.bundles.Count ? this.bundles[this.page] : null;

    private static bool IsDone(WorkshopBundleRow bundle) => ModServices.World.View.BundlesDone.Contains(bundle.Id);

    // ── Layout ──────────────────────────────────────────────────────────────────

    private void Layout()
    {
        this.initialize(Game1.uiViewport.Width / 2 - NoteWidth / 2, Game1.uiViewport.Height / 2 - NoteHeight / 2, NoteWidth, NoteHeight, true);
        int x = this.xPositionOnScreen, y = this.yPositionOnScreen;

        this.bags.Clear();
        for (int i = 0; i < this.bundles.Count; i++)
        {
            Point spot = BagSpots[i % BagSpots.Length];
            this.bags.Add(new ClickableComponent(new Rectangle(x + spot.X, y + spot.Y, 64, 64), "bundle" + i) { myID = BundleBaseId + i });
        }
        this.divineButton = new ClickableComponent(new Rectangle(x + NoteWidth / 2 - 150, y + 540, 300, 56), "divine") { myID = DivineId };
        this.backButton = new ClickableTextureComponent("Back", new Rectangle(x + borderWidth * 2 + 8, y + borderWidth * 2 + 4, 64, 64), null, null,
            Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f) { myID = BackId };

        // Vanilla's note page: a 6 x 6 inventory grid printed on the left page, so no slot frames of our own.
        this.inventory = new InventoryMenu(x + 128, y + 140, true, Game1.player.Items, this.CanDonate, 36, 6, 8, 8, drawSlots: false);
        for (int i = 0; i < this.inventory.inventory.Count; i++)
            this.inventory.inventory[i].visible = i < Game1.player.MaxItems;
        MenuKit.WireGrid(this.inventory, PlayerBaseId, upEdge: _ => BackId, downEdge: _ => -1);
        for (int i = 5; i < this.inventory.inventory.Count; i += 6)
            this.inventory.inventory[i].rightNeighborID = IngredientBaseId;

        this.ingredients.Clear();
        this.boxes.Clear();
        if (this.Current is { } bundle)
        {
            List<Rectangle> spots = RowsCenteredAt(x + 932, y + 364, bundle.Slots.Count);
            for (int i = 0; i < spots.Count; i++)
            {
                this.ingredients.Add(new ClickableComponent(spots[i], "ingredient" + i)
                {
                    myID = IngredientBaseId + i,
                    leftNeighborID = i > 0 ? IngredientBaseId + i - 1 : PlayerBaseId + 5,
                    rightNeighborID = i + 1 < spots.Count ? IngredientBaseId + i + 1 : -1,
                    upNeighborID = BackId,
                });
            }
            this.boxes.AddRange(RowsCenteredAt(x + 932, y + 540, bundle.RequiredCount));
        }

        for (int i = 0; i < this.bags.Count; i++)
        {
            this.bags[i].leftNeighborID = i > 0 ? BundleBaseId + i - 1 : -1;
            this.bags[i].rightNeighborID = i + 1 < this.bags.Count ? BundleBaseId + i + 1 : -1;
            this.bags[i].downNeighborID = this.CanDivine ? DivineId : -1;
        }
        this.divineButton.upNeighborID = BundleBaseId;
        this.backButton.downNeighborID = PlayerBaseId;
        this.backButton.rightNeighborID = IngredientBaseId;

        this.allClickableComponents = new List<ClickableComponent>();
        if (this.page < 0)
        {
            this.allClickableComponents.AddRange(this.bags);
            if (this.CanDivine)
                this.allClickableComponents.Add(this.divineButton);
        }
        else
        {
            this.allClickableComponents.Add(this.backButton);
            this.allClickableComponents.AddRange(this.inventory.inventory.Where(c => c.visible));
            this.allClickableComponents.AddRange(this.ingredients);
        }
        if (this.upperRightCloseButton != null)
            this.allClickableComponents.Add(this.upperRightCloseButton);
    }

    /// <summary>Vanilla's ingredient rows (JunimoNoteMenu.addRectangleRowsToList): up to four in one row, then two rows.</summary>
    private static List<Rectangle> RowsCenteredAt(int centerX, int centerY, int count)
    {
        var list = new List<Rectangle>();
        if (count <= 0)
            return list;
        if (count <= 4)
        {
            list.AddRange(RowOfBoxes(centerX, centerY, count));
            return list;
        }
        int first = (count + 1) / 2;
        list.AddRange(RowOfBoxes(centerX, centerY - 36, first));
        list.AddRange(RowOfBoxes(centerX, centerY + 40, count - first));
        return list;
    }

    private static IEnumerable<Rectangle> RowOfBoxes(int centerX, int centerY, int count)
    {
        int left = centerX - count * (72 + 12) / 2;
        for (int i = 0; i < count; i++)
            yield return new Rectangle(left + i * (72 + 12), centerY - 36, 72, 72);
    }

    private bool CanDonate(Item item) => this.Current is { } bundle && ModServices.World.SlotFor(bundle, item) >= 0;

    public override void snapToDefaultClickableComponent()
        => MenuKit.Snap(this, this.page < 0 ? BundleBaseId : PlayerBaseId);

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => this.Layout();

    private void OpenPage(int index)
    {
        this.page = index;
        this.message = null;
        this.sparkles.Clear();
        Game1.playSound("shwip");
        this.Layout();
        if (Game1.options.SnappyMenus)
            this.snapToDefaultClickableComponent();
    }

    private void ClosePage()
    {
        int was = this.page;
        this.page = -1;
        this.hovered = null;
        this.Layout();
        if (Game1.options.SnappyMenus)
            MenuKit.Snap(this, BundleBaseId + Math.Max(0, was));
    }

    // ── Input ───────────────────────────────────────────────────────────────────

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.upperRightCloseButton?.containsPoint(x, y) == true)
        {
            base.receiveLeftClick(x, y, playSound);
            return;
        }

        if (this.page < 0)
        {
            if (this.CanDivine && this.divineButton.containsPoint(x, y))
            {
                this.exitThisMenu();
                Divination.Ask();
                return;
            }
            for (int i = 0; i < this.bags.Count; i++)
            {
                if (!this.bags[i].containsPoint(x, y))
                    continue;
                if (IsDone(this.bundles[i]))
                    Game1.playSound("cancel");
                else
                    this.OpenPage(i);
                return;
            }
            return;
        }

        if (this.backButton.containsPoint(x, y))
        {
            this.ClosePage();
            return;
        }
        Item? item = MenuKit.ItemAt(this.inventory, x, y).Item;
        if (item is null || this.Current is not { } bundle)
            return;
        if (ModServices.World.SlotFor(bundle, item) < 0)
        {
            this.message = ModServices.T("ui.bundle.no-slot");
            Game1.playSound("cancel");
            return;
        }
        string name = item.DisplayName;
        if (!ModServices.World.Donate(Game1.player, bundle, item))
            return;
        this.message = ModServices.T("ui.bundle.donated", new { name, bundle = WorldService.BundleName(bundle) });
        if (ModServices.World.Filled(bundle) >= bundle.RequiredCount)
        {
            // Back to the overview, where the bag finishes with vanilla's sparkle (Bundle.completionAnimation).
            int index = this.page;
            this.ClosePage();
            Rectangle area = this.bags[index].bounds;
            area.Inflate(64, 64);
            this.sparkles.AddRange(Utility.sparkleWithinArea(area, 8, BagColor(index) * 0.5f));
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true) => this.receiveLeftClick(x, y, playSound);

    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape || Game1.options.doesInputListContain(Game1.options.menuButton, key))
        {
            if (this.page >= 0)
                this.ClosePage();
            else
                this.exitThisMenu();
            return;
        }
        base.receiveKeyPress(key);
    }

    public override void receiveGamePadButton(Buttons b)
    {
        if (b == Buttons.B && this.page >= 0)
        {
            this.ClosePage();
            return;
        }
        base.receiveGamePadButton(b);
    }

    public override void update(GameTime time)
    {
        base.update(time);
        for (int i = this.sparkles.Count - 1; i >= 0; i--)
        {
            if (this.sparkles[i].update(time))
                this.sparkles.RemoveAt(i);
        }
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.hovered = null;
        this.hoverText = null;
        if (this.page < 0)
        {
            for (int i = 0; i < this.bags.Count; i++)
            {
                if (!this.bags[i].containsPoint(x, y))
                    continue;
                WorkshopBundleRow bundle = this.bundles[i];
                string status = IsDone(bundle) ? ModServices.T("ui.bundle.complete") : ModServices.T("ui.bundle.progress", new { have = ModServices.World.Filled(bundle), need = bundle.RequiredCount });
                this.hoverText = $"{WorldService.BundleName(bundle)} ({status})";
            }
            return;
        }
        this.backButton.tryHover(x, y);
        this.hovered = MenuKit.ItemAt(this.inventory, x, y).Item;
        if (this.Current is { } current)
        {
            for (int i = 0; i < this.ingredients.Count; i++)
            {
                if (this.ingredients[i].containsPoint(x, y))
                    this.hoverText = MenuKit.InputName(current.Slots[i]);
            }
        }
    }

    // ── Drawing ─────────────────────────────────────────────────────────────────

    /// <summary>A bag's colour index, cycling vanilla's seven bundle colours.</summary>
    private static int ColorIndex(int bundleIndex) => bundleIndex % 7;

    private static Color BagColor(int bundleIndex) => StardewValley.Menus.Bundle.getColorFromColorIndex(ColorIndex(bundleIndex));

    /// <summary>A bundle bag's frame on the note texture (Bundle's sprite): the resting frame, or the last frame once complete.</summary>
    private static Rectangle BagSource(int bundleIndex, bool done)
    {
        int color = ColorIndex(bundleIndex);
        var source = new Rectangle(color * 256 % 512, 244 + color * 256 / 512 * 16, 16, 16);
        source.X += 16 * (done ? 15 : 1);
        return source;
    }

    public override void draw(SpriteBatch b)
    {
        if (!Game1.options.showClearBackgrounds)
            MenuKit.DrawBackdrop(b);
        int x = this.xPositionOnScreen, y = this.yPositionOnScreen;
        if (this.page < 0)
            this.DrawOverview(b, x, y);
        else
            this.DrawPage(b, x, y);

        base.draw(b);
        if (this.hovered != null)
            drawToolTip(b, this.hovered.getDescription(), this.hovered.DisplayName, this.hovered);
        else if (this.hoverText != null)
            drawHoverText(b, this.hoverText, Game1.smallFont);
        this.drawMouse(b);
    }

    private void DrawOverview(SpriteBatch b, int x, int y)
    {
        b.Draw(this.note, new Vector2(x, y), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
        SpriteText.drawStringHorizontallyCenteredAt(b, WorldService.RoomName(this.room.Id), x + this.width / 2 + 16, y + 12);
        if (this.bundles.Count == 0)
            MenuKit.DrawText(b, ModServices.T("ui.bundle.none"), x + this.width / 2 - 120, y + 300);
        for (int i = 0; i < this.bags.Count; i++)
        {
            Rectangle r = this.bags[i].bounds;
            b.Draw(this.note, new Vector2(r.X, r.Y), BagSource(i, IsDone(this.bundles[i])), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
        }
        foreach (TemporaryAnimatedSprite sparkle in this.sparkles)
            sparkle.draw(b, localPosition: true);
        if (this.CanDivine)
            MenuKit.DrawButton(b, this.divineButton, ModServices.T("ui.bundle.divine"));
        if (this.message != null)
        {
            string text = MenuKit.Fit(this.message, NoteWidth - 320);
            MenuKit.DrawText(b, text, x + (NoteWidth - (int)Game1.smallFont.MeasureString(text).X) / 2, this.divineButton.bounds.Bottom + 12);
        }
    }

    private void DrawPage(SpriteBatch b, int x, int y)
    {
        b.Draw(this.note, new Vector2(x, y), new Rectangle(320, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
        if (this.Current is not { } bundle)
            return;

        // The bundle's bag, large, with its name on vanilla's ribbon (vanilla's 32 x 32 bundle art at 4x is 128 px; ours is the bag at 8x).
        b.Draw(this.note, new Vector2(x + 872, y + 88), BagSource(this.page, false), Color.White, 0f, Vector2.Zero, 8f, SpriteEffects.None, 0.15f);
        string label = WorldService.BundleName(bundle);
        float labelWidth = Game1.dialogueFont.MeasureString(label).X;
        b.Draw(this.note, new Vector2(x + 936 - (int)labelWidth / 2 - 16, y + 228), new Rectangle(517, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
        b.Draw(this.note, new Rectangle(x + 936 - (int)labelWidth / 2, y + 228, (int)labelWidth, 68), new Rectangle(520, 266, 1, 17), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
        b.Draw(this.note, new Vector2(x + 936 + (int)labelWidth / 2, y + 228), new Rectangle(524, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
        Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(x + 936 - labelWidth / 2f, y + 236), Game1.textColor * 0.9f);

        this.backButton.draw(b);

        // What the bundle asks for: dimmed once given, with a shadow under the ones still wanted.
        for (int i = 0; i < this.ingredients.Count; i++)
        {
            Rectangle r = this.ingredients[i].bounds;
            IngredientSpec spec = bundle.Slots[i];
            bool given = ModServices.World.IsSlotFilled(bundle, i, out _);
            float alpha = given ? 0.25f : 1f;
            if (!given)
                b.Draw(Game1.shadowTexture, new Vector2(r.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, r.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            Item? shown = spec.Item != null ? ItemRegistry.Create(WorldService.Compile(spec).ItemId ?? Ids.QualifyItem(spec.Item), Math.Max(1, spec.Count), spec.MinQuality, allowNull: true) : null;
            if (shown != null)
                shown.drawInMenu(b, new Vector2(r.X + 4, r.Y + 4), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White * alpha, drawShadow: false);
            else
            {
                MenuKit.DrawText(b, "?", r.X + 28, r.Y + 20, given ? Color.Gray * 0.5f : Game1.textColor);
                if (spec.Count > 1)
                    Utility.drawTinyDigits(spec.Count, b, new Vector2(r.Right - 20, r.Bottom - 20), 3f, 1f, Color.White * alpha);
                DrawQualityStar(b, spec.MinQuality, r, alpha);
            }
        }

        // The donation boxes: one per item the bundle needs, filled in the order things were given.
        var givenItems = new List<string?>();
        for (int s = 0; s < bundle.Slots.Count; s++)
        {
            if (ModServices.World.IsSlotFilled(bundle, s, out string? itemId))
                givenItems.Add(itemId);
        }
        for (int i = 0; i < this.boxes.Count; i++)
        {
            Rectangle r = this.boxes[i];
            b.Draw(this.note, new Vector2(r.X, r.Y), new Rectangle(512, 244, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
            if (i >= givenItems.Count)
                continue;
            if (givenItems[i] is string id && id != "dev" && ItemRegistry.Create(id, allowNull: true) is Item item)
                item.drawInMenu(b, new Vector2(r.X + 4, r.Y + 4), 1f, 1f, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);
            else
                MenuKit.DrawText(b, "*", r.X + 30, r.Y + 22, Color.DarkGreen);
        }

        this.inventory.draw(b);
        if (this.message != null)
            MenuKit.DrawText(b, MenuKit.Fit(this.message, 520), x + 680, y + 620);
    }

    /// <summary>Vanilla's quality star (Object.drawInMenu's sources) in a slot's corner, for slots with no item to draw.</summary>
    private static void DrawQualityStar(SpriteBatch b, int quality, Rectangle slot, float alpha)
    {
        if (quality <= 0)
            return;
        Rectangle source = quality < 2 ? new Rectangle(338, 400, 8, 8) : quality < 4 ? new Rectangle(346, 400, 8, 8) : new Rectangle(346, 392, 8, 8);
        b.Draw(Game1.mouseCursors, new Vector2(slot.X + 8, slot.Bottom - 28), source, Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
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
