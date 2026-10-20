using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using SObject = StardewValley.Object;

namespace ValleyOpus.World;

/// <summary>
/// The four Familiars (doc 18 §3–4): met in the world once the Great Work Chamber opens, waiting in that chamber
/// until bonded by a hand-made rite on an altar inside a T3 Lab, then living in that Lab. Greeting and liked gifts
/// add bond points (no power). Release is free at the Lab's shrine. Drawn by us; they are not villagers.
/// </summary>
internal sealed class Familiars
{
    public const string BondStatePrefix = Ids.RowPrefix + "FamiliarBond_";
    public const string GreatWorkUnlock = Ids.RowPrefix + "GreatWork";
    public const string ChamberLocation = Ids.RowPrefix + "Workshop_GreatWorkChamber";
    public const string TextureAsset = "Mods/" + Ids.ModId + "/Familiars";

    private readonly SmapiLog log;
    private readonly WorldService world;
    private Texture2D? texture;

    public Familiars(IModHelper helper, SmapiLog log, WorldService world)
    {
        this.log = log;
        this.world = world;
        helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecond;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.Content.AssetsInvalidated += (_, e) =>
        {
            if (e.NamesWithoutLocale.Any(n => n.IsEquivalentTo(TextureAsset)))
                this.texture = null;
        };
    }

    private static IEnumerable<FamiliarRow> Rows => ModServices.Content.Registry.Familiars.All;

    private static string Local(string id) => WorldService.Local(id);

    public FamiliarState? StateOf(string local) => this.world.View.Familiars.TryGetValue(local, out FamiliarState? state) ? state : null;

    // ── Meeting (doc 18 §4.1) ───────────────────────────────────────────────────

    private void OnOneSecond(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || !e.IsMultipleOf(120) || !this.world.IsUnlocked(GreatWorkUnlock) || Game1.player.currentLocation is not GameLocation here)
            return;
        try
        {
            foreach (FamiliarRow row in Rows)
            {
                string local = Local(row.Id);
                if (this.StateOf(local) is { MetDay: >= 0 } || !MeetsHere(row, here))
                    continue;
                this.Meet(local, Game1.player);
                break;
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"Familiar meeting check failed: {ex}");
        }
    }

    private static bool MeetsHere(FamiliarRow row, GameLocation here)
    {
        string condition = row.Meet.Condition;
        if (row.Meet.Location == "UndergroundMine")
        {
            if (here is not MineShaft shaft)
                return here is VolcanoDungeon && row.Id.EndsWith("Salamander", StringComparison.Ordinal);
            int min = condition.StartsWith("MINE_LEVEL", StringComparison.Ordinal) && int.TryParse(condition.Split(' ').ElementAtOrDefault(1), out int level) ? level : 1;
            return shaft.mineLevel >= min && shaft.mineLevel < 121;
        }
        return string.Equals(here.Name, row.Meet.Location, StringComparison.OrdinalIgnoreCase)
            && GameStateQuery.CheckConditions(condition, here, Game1.player);
    }

    private void Meet(string local, Farmer who)
    {
        if (Context.IsMainPlayer)
        {
            this.Host(local).MetDay = (int)Game1.stats.DaysPlayed;
            this.world.Publish();
        }
        else
            this.world.SendFamiliar("Meet", local);
        Game1.playSound("wand");
        Game1.drawObjectDialogue(ModServices.T("familiar.met", new { name = Bonuses.FamiliarName(local) }));
        this.log.Info($"{who.Name} met the {local}.");
    }

    private FamiliarState Host(string local)
    {
        if (!this.world.HostData.Familiars.TryGetValue(local, out FamiliarState? state))
            this.world.HostData.Familiars[local] = state = new FamiliarState();
        return state;
    }

    // ── Bonding and release (doc 18 §4.2–3) ─────────────────────────────────────

    /// <summary>The bonding rite finished on an altar: bind the Familiar to the T3 Lab the altar stands in.</summary>
    public void Bond(string local, SObject altar, Farmer who)
    {
        GameLocation? location = altar.Location;
        string? problem = this.StateOf(local) is not { MetDay: >= 0 } ? "familiar.bond-unmet"
            : Bonuses.LabTier(location) < 3 ? "familiar.bond-not-t3"
            : Bonuses.FamiliarIn(location) is string other && other != local ? "familiar.bond-occupied"
            : null;
        if (problem != null)
        {
            Game1.drawObjectDialogue(ModServices.T(problem, new { name = Bonuses.FamiliarName(local) }));
            return;
        }
        if (Context.IsMainPlayer)
            this.ApplyBond(local, location!.NameOrUniqueName);
        else
            this.world.SendFamiliar("Bond", local, location!.NameOrUniqueName);
        Game1.playSound("yoba");
        Game1.drawObjectDialogue(ModServices.T("familiar.bonded", new { name = Bonuses.FamiliarName(local) }));
    }

    private static Building? LabFor(string locationName)
    {
        Building? found = null;
        Utility.ForEachBuilding(building =>
        {
            if (building.GetIndoors()?.NameOrUniqueName == locationName)
            {
                found = building;
                return false;
            }
            return true;
        });
        return found;
    }

    private static Building? LabById(string? id)
    {
        if (id is null)
            return null;
        Building? found = null;
        Utility.ForEachBuilding(building =>
        {
            if (building.id.Value.ToString() == id)
            {
                found = building;
                return false;
            }
            return true;
        });
        return found;
    }

    private void ApplyBond(string local, string labLocation)
    {
        if (LabFor(labLocation) is not Building lab)
            return;
        FamiliarState state = this.Host(local);
        if (LabById(state.Lab) is Building previous)
            previous.modData.Remove(Bonuses.FamiliarKey);
        lab.modData[Bonuses.FamiliarKey] = local;
        state.Lab = lab.id.Value.ToString();
        this.world.Publish();
        this.log.Info($"The {local} bonded to the Lab at {lab.tileX.Value},{lab.tileY.Value}.");
    }

    /// <summary>The shrine in a T3 Lab: release its Familiar (free, keeps its bond points).</summary>
    public void UseShrine(GameLocation location)
    {
        if (Bonuses.FamiliarIn(location) is not string local)
        {
            Game1.drawObjectDialogue(ModServices.T("familiar.shrine-empty"));
            return;
        }
        location.createQuestionDialogue(ModServices.T("familiar.release-ask", new { name = Bonuses.FamiliarName(local) }),
            location.createYesNoResponses(), (who, answer) =>
            {
                if (answer != "Yes")
                    return;
                if (Context.IsMainPlayer)
                    this.ApplyRelease(local);
                else
                    this.world.SendFamiliar("Release", local);
                Game1.playSound("leafrustle");
            });
    }

    private void ApplyRelease(string local)
    {
        FamiliarState state = this.Host(local);
        LabById(state.Lab)?.modData.Remove(Bonuses.FamiliarKey);
        state.Lab = null;
        this.world.Publish();
        this.log.Info($"The {local} was released back to the workshop.");
    }

    // ── Greeting and gifts (doc 18 §4.4) ────────────────────────────────────────

    private void Interact(string local, Farmer who)
    {
        FamiliarState? state = this.StateOf(local);
        if (state is null)
            return;
        int day = (int)Game1.stats.DaysPlayed;
        FamiliarRow? row = ModServices.Content.Registry.Familiars.Get(Ids.RowPrefix + local);
        Item? held = who.ActiveObject;
        if (held != null && row != null && row.LikedGifts.Any(g => WorldService.Compile(g).Matches(GameContent.ToInfo(held), held.Quality)))
        {
            if (state.LastGiftDay == day)
            {
                Game1.drawObjectDialogue(ModServices.T("familiar.gift-already"));
                return;
            }
            who.reduceActiveItemByOne();
            this.Point(local, gift: true);
            Game1.playSound("give_gift");
            Game1.drawObjectDialogue(ModServices.T("familiar.gift", new { name = Bonuses.FamiliarName(local) }));
            return;
        }
        if (state.LastGreetedDay != day)
        {
            this.Point(local, gift: false);
            Game1.playSound("dwop");
        }
        Game1.drawObjectDialogue(ModServices.T("familiar.greet", new { name = Bonuses.FamiliarName(local), hearts = Math.Min(5, state.BondPoints / 10) }));
    }

    private void Point(string local, bool gift)
    {
        if (!Context.IsMainPlayer)
        {
            this.world.SendFamiliar(gift ? "Gift" : "Greet", local);
            return;
        }
        FamiliarState state = this.Host(local);
        int day = (int)Game1.stats.DaysPlayed;
        if (gift)
            state.LastGiftDay = day;
        else
            state.LastGreetedDay = day;
        state.BondPoints = Math.Min(50, state.BondPoints + 1);
        this.world.Publish();
    }

    public void OnRequest(WorldRequest request, long from)
    {
        switch (request.Kind)
        {
            case "Meet":
                if (this.Host(request.Id).MetDay < 0)
                    this.Host(request.Id).MetDay = (int)Game1.stats.DaysPlayed;
                this.world.Publish();
                break;
            case "Bond" when request.ItemId != null:
                this.ApplyBond(request.Id, request.ItemId);
                break;
            case "Release":
                this.ApplyRelease(request.Id);
                break;
            case "Greet":
            case "Gift":
                this.Point(request.Id, request.Kind == "Gift");
                break;
        }
    }

    // ── Drawing ─────────────────────────────────────────────────────────────────

    /// <summary>Where each Familiar shows in a location: waiting ones line the chamber, a bonded one floats by its shrine.</summary>
    private IEnumerable<(string Local, FamiliarRow Row, Vector2 Tile)> Present(GameLocation location)
    {
        int i = 0;
        foreach (FamiliarRow row in Rows.OrderBy(r => r.Sprite))
        {
            string local = Local(row.Id);
            FamiliarState? state = this.StateOf(local);
            if (state is not { MetDay: >= 0 })
            {
                i++;
                continue;
            }
            if (state.Lab is null && location.Name == ChamberLocation)
                yield return (local, row, new Vector2(4 + 3 * i, 6));
            else if (state.Lab != null && Bonuses.FamiliarIn(location) == local && location.Map is { } map)
                yield return (local, row, new Vector2(map.Layers[0].LayerWidth - 3, 5));
            i++;
        }
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.currentLocation is not GameLocation location)
            return;
        try
        {
            List<FamiliarRow> sorted = Rows.OrderBy(r => r.Sprite).ToList();
            foreach ((string local, FamiliarRow row, Vector2 tile) in this.Present(location))
            {
                this.texture ??= Game1.content.Load<Texture2D>(TextureAsset);
                int column = sorted.IndexOf(row);
                float bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 400.0 + column) * 6f;
                Vector2 screen = Game1.GlobalToLocal(Game1.viewport, tile * 64f + new Vector2(0, -24 + bob));
                e.SpriteBatch.Draw(this.texture, screen, new Rectangle(column * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tile.Y * 64f + 64) / 10000f);
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"Couldn't draw Familiars: {ex}");
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.currentLocation is not GameLocation location)
            return;
        foreach ((string local, _, Vector2 tile) in this.Present(location).ToList())
        {
            if (Vector2.Distance(e.Cursor.GrabTile, tile) > 0.5f && Vector2.Distance(e.Cursor.Tile, tile) > 0.5f)
                continue;
            if (Vector2.Distance(Game1.player.Tile, tile) > 2.5f)
                continue;
            ModServices.Helper.Input.Suppress(e.Button);
            this.Interact(local, Game1.player);
            return;
        }
    }
}
