using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using ValleyOpus.Framework;
using ValleyOpus.Routes;
using ValleyOpus.Satchel;
using SObject = StardewValley.Object;

namespace ValleyOpus.UI;

/// <summary>
/// In-world aids (doc 16 §5–6): the apparatus hover tooltip, and while the Satchel is held the light chain overlay
/// with each apparatus's West intake and East spout marked. State is per screen; links refresh once a second.
/// </summary>
internal sealed class HudOverlay
{
    private const int LinkRefreshTicks = 60;
    private const int TooltipRefreshTicks = 15;

    private static readonly Color IntakeColor = Color.CornflowerBlue;
    private static readonly Color SpoutColor = Color.Orange;

    private readonly PerScreen<ScreenState> state = new(() => new ScreenState());
    private bool loggedError;

    /// <summary>One screen's cached overlay and tooltip data.</summary>
    private sealed class ScreenState
    {
        public GameLocation? LinksLocation;
        public int LinksTick = int.MinValue;
        public List<ChainLink> Links = new();
        public List<Vector2> Apparatus = new();

        public SObject? TooltipObject;
        public int TooltipTick = int.MinValue;
        public List<string> TooltipLines = new();
    }

    public HudOverlay(IModHelper helper)
    {
        helper.Events.Display.RenderedHud += this.OnRenderedHud;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
    }

    // ── Machine tooltip ─────────────────────────────────────────────────────────

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.activeClickableMenu != null || ModServices.Content is null || Game1.currentLocation is null)
            return;
        try
        {
            SObject? machine = this.HoveredApparatus();
            if (machine is null)
                return;
            ScreenState screen = this.state.Value;
            if (!ReferenceEquals(screen.TooltipObject, machine) || Game1.ticks - screen.TooltipTick >= TooltipRefreshTicks)
            {
                screen.TooltipObject = machine;
                screen.TooltipTick = Game1.ticks;
                screen.TooltipLines = ModServices.Machines.Describe(machine, Game1.player);
            }
            if (screen.TooltipLines.Count > 0)
                IClickableMenu.drawHoverText(e.SpriteBatch, string.Join("\n", screen.TooltipLines), Game1.smallFont, boldTitleText: machine.DisplayName);
            else
                IClickableMenu.drawHoverText(e.SpriteBatch, machine.DisplayName, Game1.smallFont);
        }
        catch (Exception ex)
        {
            this.LogOnce("tooltip", ex);
        }
    }

    /// <summary>The apparatus under the cursor (or the controller's grab tile); a big craftable's sprite also covers the tile above it.</summary>
    private SObject? HoveredApparatus()
    {
        Vector2 tile = Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse ? Game1.player.GetGrabTile() : Game1.currentCursorTile;
        GameLocation location = Game1.currentLocation;
        foreach (Vector2 candidate in new[] { tile, tile + new Vector2(0, 1) })
        {
            if (location.objects.TryGetValue(candidate, out SObject obj) && ModServices.Content.ApparatusOf(obj) != null)
                return obj;
        }
        return null;
    }

    // ── Chain overlay ───────────────────────────────────────────────────────────

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!Context.IsWorldReady || !ModServices.Config.ShowOverlay || ModServices.Content is null || Game1.currentLocation is null)
            return;
        if (Game1.player.CurrentItem?.QualifiedItemId != SatchelService.SatchelItemId)
            return;
        try
        {
            ScreenState screen = this.state.Value;
            GameLocation location = Game1.currentLocation;
            if (!ReferenceEquals(screen.LinksLocation, location) || Game1.ticks - screen.LinksTick >= LinkRefreshTicks || Game1.ticks < screen.LinksTick)
            {
                screen.LinksLocation = location;
                screen.LinksTick = Game1.ticks;
                screen.Links = ChainService.Links(location).ToList();
                screen.Apparatus = location.objects.Pairs
                    .Where(p => ModServices.Content.ApparatusOf(p.Value) != null)
                    .Select(p => p.Key)
                    .ToList();
            }

            SpriteBatch b = e.SpriteBatch;
            foreach (Vector2 tile in screen.Apparatus)
            {
                Vector2 origin = Game1.GlobalToLocal(Game1.viewport, tile * 64f);
                b.Draw(Game1.staminaRect, new Rectangle((int)origin.X, (int)origin.Y + 20, 8, 24), null, IntakeColor * 0.85f, 0f, Vector2.Zero, SpriteEffects.None, 0.99f);
                b.Draw(Game1.staminaRect, new Rectangle((int)origin.X + 56, (int)origin.Y + 20, 8, 24), null, SpoutColor * 0.85f, 0f, Vector2.Zero, SpriteEffects.None, 0.99f);
            }
            foreach (ChainLink link in screen.Links)
                DrawLink(b, link);
        }
        catch (Exception ex)
        {
            this.LogOnce("overlay", ex);
        }
    }

    private static void DrawLink(SpriteBatch b, ChainLink link)
    {
        Vector2 from = Game1.GlobalToLocal(Game1.viewport, link.From * 64f + new Vector2(32, 32));
        Vector2 to = Game1.GlobalToLocal(Game1.viewport, link.To * 64f + new Vector2(32, 32));
        Color color = link.Accepts ? Color.LimeGreen : Color.Red;
        Vector2 direction = to - from;
        if (direction.LengthSquared() < 1f)
            return;
        direction.Normalize();
        Vector2 start = from + direction * 16f;
        Vector2 end = to - direction * 16f;

        DrawLine(b, start, end, color, 4f);
        float angle = (float)Math.Atan2(direction.Y, direction.X);
        const float headLength = 14f, headAngle = 2.6f;
        DrawLine(b, end, end + new Vector2((float)Math.Cos(angle + headAngle), (float)Math.Sin(angle + headAngle)) * headLength, color, 4f);
        DrawLine(b, end, end + new Vector2((float)Math.Cos(angle - headAngle), (float)Math.Sin(angle - headAngle)) * headLength, color, 4f);

        if (!link.Accepts)
        {
            Vector2 mid = (start + end) / 2f;
            DrawLine(b, mid + new Vector2(-8, -8), mid + new Vector2(8, 8), Color.Red, 4f);
            DrawLine(b, mid + new Vector2(-8, 8), mid + new Vector2(8, -8), Color.Red, 4f);
        }
    }

    private static void DrawLine(SpriteBatch b, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 delta = end - start;
        float angle = (float)Math.Atan2(delta.Y, delta.X);
        b.Draw(Game1.staminaRect, start, null, color, angle, new Vector2(0f, 0.5f), new Vector2(delta.Length(), thickness), SpriteEffects.None, 1f);
    }

    private void LogOnce(string what, Exception ex)
    {
        if (this.loggedError)
            return;
        this.loggedError = true;
        ModServices.Log.Error($"The {what} drawing failed; further errors are hidden until restart. {ex}");
    }
}
