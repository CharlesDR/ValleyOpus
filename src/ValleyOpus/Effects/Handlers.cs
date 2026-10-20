using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.Effects;

/// <summary>
/// The custom effect handlers of doc 15 §3 (one per <c>Handler</c> name on the <c>Effects</c> rows), the apply-to-
/// use products, the worn effects of rings and trinkets, and the two placed products (Azoth Diffuser, Radicula).
/// Every handler reads its numbers from the effect row's <c>Params</c>, so the balance sweep tunes data, not code.
/// </summary>
internal sealed class Handlers
{
    public const string PlanetaryGiftBuff = Ids.RowPrefix + "PlanetaryGift";
    public const string RebisBuff = Ids.RowPrefix + "RebisGift";
    public const string DiffuserId = "(BC)" + Ids.RowPrefix + "AzothDiffuser";
    public const string RadiculaId = "(BC)" + Ids.RowPrefix + "RadiculaPlant";
    public const string ElixirId = "(O)" + Ids.RowPrefix + "Elixir";
    public const string PlacedDayKey = Ids.KeyPrefix + "PlacedDay";
    public const string SwingsKey = Ids.KeyPrefix + "DissolutionSwings";
    private const string GlowLightId = Ids.RowPrefix + "Glow";

    private static readonly Dictionary<string, string> NextOre = new() { ["(O)378"] = "(O)380", ["(O)380"] = "(O)384", ["(O)384"] = "(O)386" };

    private static Handlers Self = null!;
    private static int savedToolLevel = -1;

    private readonly SmapiLog log;
    private readonly PerScreen<ScreenState> screen = new(() => new ScreenState());
    private bool removingStones;

    public Handlers(IModHelper helper, SmapiLog log)
    {
        Self = this;
        this.log = log;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecond;
        helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
        helper.Events.World.NpcListChanged += this.OnNpcListChanged;
        helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private sealed class ScreenState
    {
        public HashSet<string> Worn = new(StringComparer.Ordinal);
        public HashSet<string> AppliedByWear = new(StringComparer.Ordinal);
        public HashSet<string> SeenBuffs = new(StringComparer.Ordinal);
        public Dictionary<Monster, int> MonsterHealth = new();
        public Dictionary<Monster, int> Burning = new();
        public int LastHealth = -1;
        public bool DreamTonight;
        public int PendantCooldown;
    }

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)), postfix: new HarmonyMethod(typeof(Handlers), nameof(EatPostfix)));
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.changeFriendship)), postfix: new HarmonyMethod(typeof(Handlers), nameof(FriendshipPostfix)));
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)), postfix: new HarmonyMethod(typeof(Handlers), nameof(TakeDamagePostfix)));
        harmony.Patch(AccessTools.Method(typeof(Pickaxe), nameof(Pickaxe.DoFunction)),
            prefix: new HarmonyMethod(typeof(Handlers), nameof(ToolPrefix)), finalizer: new HarmonyMethod(typeof(Handlers), nameof(ToolFinalizer)));
        harmony.Patch(AccessTools.Method(typeof(Axe), nameof(Axe.DoFunction)),
            prefix: new HarmonyMethod(typeof(Handlers), nameof(ToolPrefix)), finalizer: new HarmonyMethod(typeof(Handlers), nameof(ToolFinalizer)));
        harmony.Patch(AccessTools.Method(typeof(SObject), nameof(SObject.PlaceInMachine)), postfix: new HarmonyMethod(typeof(Handlers), nameof(PlaceInMachinePostfix)));
    }

    // ── Reading active effects ──────────────────────────────────────────────────

    private static bool Has(Farmer who, string handler) => EffectService.HasHandler(who, handler);

    /// <summary>A handler's number, made a little stronger by an active Quintessence (doc 15 §3a).</summary>
    private static double Number(Farmer who, string handler, string param, double fallback)
    {
        double value = EffectService.HandlerNumber(who, handler, param, fallback);
        if (handler != "Quintessence" && param is "Chance" or "Bonus" && Has(who, "Quintessence"))
            value *= 1 + EffectService.HandlerNumber(who, "Quintessence", "Bonus", 0.1);
        return value;
    }

    private static bool Roll(Farmer who, string handler, double fallback) => Has(who, handler) && Game1.random.NextDouble() < Number(who, handler, "Chance", fallback);

    // ── Ticks ───────────────────────────────────────────────────────────────────

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        Farmer who = Game1.player;
        try
        {
            int elapsed = Game1.currentGameTime?.ElapsedGameTime.Milliseconds ?? 16;
            bool timePasses = Game1.shouldTimePass();

            // Fixative: other buffs stop running down; the strong tier also hurries debuffs off (doc 15).
            if (timePasses && Has(who, "Fixative"))
            {
                bool strong = EffectService.HandlerNumber(who, "Fixative", "Minutes", 2) >= 8;
                IDictionary<string, BuffData> rows = DataLoader.Buffs(Game1.content);
                foreach (Buff buff in who.buffs.AppliedBuffs.Values)
                {
                    if (buff.millisecondsDuration <= 0 || IsOurs(buff.id, "Fixative"))
                        continue;
                    bool debuff = rows.TryGetValue(buff.id, out BuffData? row) && row.IsDebuff;
                    if (debuff && strong)
                        buff.millisecondsDuration = Math.Max(1, buff.millisecondsDuration - elapsed);
                    else if (!debuff)
                        buff.millisecondsDuration += elapsed;
                }
            }

            // Hourglass: the clock runs slower, single player only (doc 15 §3a, Charles).
            if (timePasses && !Context.IsMultiplayer && Has(who, "Hourglass"))
                Game1.gameTimeInterval -= (int)(elapsed * EffectService.HandlerNumber(who, "Hourglass", "Slow", 0.25));

            // Glow and the Censer give off light.
            this.UpdateGlow(who);

            // Spectral and Vial Pendant react to taking a hit.
            ScreenState state = this.screen.Value;
            if (state.LastHealth >= 0 && who.health < state.LastHealth)
            {
                if (Has(who, "Spectral"))
                {
                    who.temporarilyInvincible = true;
                    who.temporaryInvincibilityTimer = 1200 - (int)(EffectService.HandlerNumber(who, "Spectral", "Seconds", 3) * 1000);
                }
            }
            state.LastHealth = who.health;
            if (state.PendantCooldown > 0)
                state.PendantCooldown--;
            if (state.PendantCooldown == 0 && Has(who, "VialPendant") && who.health < who.maxHealth * EffectService.HandlerNumber(who, "VialPendant", "Threshold", 0.25))
                this.DrinkElixir(who);
        }
        catch (Exception ex)
        {
            this.log.Error($"An effect tick failed: {ex}");
        }
    }

    private static bool IsOurs(string buffId, string handler)
        => DataLoader.Buffs(Game1.content).TryGetValue(buffId, out BuffData? row) && row.CustomFields?.TryGetValue(GameContent.HandlerField, out string? name) == true && name == handler;

    private void UpdateGlow(Farmer who)
    {
        string id = GlowLightId + "_" + who.UniqueMultiplayerID;
        bool glow = Has(who, "Glow") || Has(who, "Censer");
        if (!glow)
        {
            Game1.currentLightSources.Remove(id);
            return;
        }
        float radius = (float)EffectService.HandlerNumber(who, "Glow", "Radius", 4) / 2f;
        Vector2 position = who.Position + new Vector2(32, 0);
        if (Game1.currentLightSources.TryGetValue(id, out LightSource? light))
            light.position.Value = position;
        else
            Game1.currentLightSources[id] = new LightSource(id, LightSource.lantern, position, Math.Max(2f, radius), LightSource.LightContext.None, who.UniqueMultiplayerID);
    }

    private void DrinkElixir(Farmer who)
    {
        Item? elixir = who.Items.FirstOrDefault(i => i?.QualifiedItemId == ElixirId);
        if (elixir is not SObject drink)
            return;
        this.screen.Value.PendantCooldown = 600; // ten seconds
        int heal = Math.Max(drink.healthRecoveredOnConsumption(), who.maxHealth / 3);
        who.health = Math.Min(who.maxHealth, who.health + heal);
        who.Stamina = Math.Min(who.MaxStamina, who.Stamina + drink.staminaRecoveredOnConsumption());
        who.Items.ReduceId(ElixirId, 1);
        Game1.playSound("gulp");
        Game1.addHUDMessage(new HUDMessage(ModServices.T("effect.pendant-drank"), HUDMessage.health_type));
    }

    private void OnOneSecond(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        Farmer who = Game1.player;
        try
        {
            this.ApplyWorn(who);
            this.PlanetaryGift(who);
            this.RebisGift(who);

            // Good Cheer: never tipsy.
            if (Has(who, "GoodCheer") && who.buffs.IsApplied("17"))
                who.buffs.Remove("17");

            // Cleanse: debuffs keep falling away while it lasts.
            if (Has(who, "Cleanse"))
                ClearDebuffs(who);

            this.Combat(who);
            this.Radiculas(who.currentLocation);
            this.Volatile(who);
        }
        catch (Exception ex)
        {
            this.log.Error($"An effect update failed: {ex}");
        }
    }

    private static void ClearDebuffs(Farmer who)
    {
        IDictionary<string, BuffData> rows = DataLoader.Buffs(Game1.content);
        foreach (string id in who.buffs.AppliedBuffs.Keys.Where(id => rows.TryGetValue(id, out BuffData? row) && row.IsDebuff).ToList())
            who.buffs.Remove(id);
    }

    /// <summary>Volatile: a buff applied while it lasts grows stronger and shorter.</summary>
    private void Volatile(Farmer who)
    {
        ScreenState state = this.screen.Value;
        bool active = Has(who, "Volatile");
        foreach (Buff buff in who.buffs.AppliedBuffs.Values)
        {
            if (!state.SeenBuffs.Add(buff.id) || !active || IsOurs(buff.id, "Volatile") || buff.millisecondsDuration <= 0)
                continue;
            double factor = EffectService.HandlerNumber(who, "Volatile", "Factor", 1.5);
            foreach (var field in typeof(BuffEffects).GetFields().Where(f => f.FieldType == typeof(Netcode.NetFloat)))
            {
                var net = (Netcode.NetFloat)field.GetValue(buff.effects)!;
                net.Value = (float)(net.Value * factor);
            }
            buff.millisecondsDuration = (int)(buff.millisecondsDuration / factor);
            who.buffs.Dirty = true;
        }
        state.SeenBuffs.IntersectWith(who.buffs.AppliedBuffs.Keys);
    }

    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        Farmer who = Game1.player;
        if (Has(who, "Vitality"))
            who.health = Math.Min(who.maxHealth, who.health + (int)EffectService.HandlerNumber(who, "Vitality", "PerTenMinutes", 2));
    }

    private void OnDayEnding(object? sender, DayEndingEventArgs e)
        => this.screen.Value.DreamTonight = Has(Game1.player, "LunarDream");

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        try
        {
            Farmer who = Game1.player;
            ScreenState state = this.screen.Value;
            state.AppliedByWear.Clear();
            state.Burning.Clear();
            state.MonsterHealth.Clear();

            // Lunar Dream: a hint from the dream pool (divination's when no Dream hints exist) on waking.
            if (state.DreamTonight)
            {
                state.DreamTonight = false;
                List<HintRow> pool = ModServices.World.AvailableHints("Dream");
                if (pool.Count == 0)
                    pool = ModServices.World.AvailableHints("Divination");
                if (pool.Count > 0)
                    Game1.drawObjectDialogue(ModServices.T("effect.dream", new { hint = ModServices.World.Reveal(who, pool[Game1.random.Next(pool.Count)]) }));
            }

            // A Gloam Lantern gathers one Shade each night it is carried.
            foreach (Item lantern in Shades.Lanterns(who))
                lantern.modData[Shades.ShadesKey] = Math.Min(Shades.Capacity, Shades.Held(lantern) + 1).ToString();

            if (Context.IsMainPlayer)
                this.CrumbleDiffusers();
        }
        catch (Exception ex)
        {
            this.log.Error($"The morning effect update failed: {ex}");
        }
    }

    // ── Worn effects: rings and trinkets (doc 15 §5) ────────────────────────────

    /// <summary>Effect IDs from worn items: rings by their <c>valleyopus_effect_*</c> tag, trinkets by their custom field.</summary>
    private static HashSet<string> WornEffects(Farmer who)
    {
        var effects = new HashSet<string>(StringComparer.Ordinal);
        void FromRing(StardewValley.Objects.Ring? ring)
        {
            if (ring is StardewValley.Objects.CombinedRing combined)
            {
                foreach (StardewValley.Objects.Ring inner in combined.combinedRings)
                    FromRing(inner);
                return;
            }
            foreach (string tag in ring?.GetContextTags() ?? Enumerable.Empty<string>())
                if (tag.StartsWith("valleyopus_effect_", StringComparison.Ordinal))
                    effects.Add(Ids.RowPrefix + tag["valleyopus_effect_".Length..]);
        }
        FromRing(who.leftRing.Value);
        FromRing(who.rightRing.Value);
        foreach (StardewValley.Objects.Trinkets.Trinket trinket in who.trinketItems)
        {
            if (trinket?.GetTrinketData()?.CustomFields?.TryGetValue(AlchemyTrinket.EffectField, out string? effect) == true)
                effects.Add(effect);
        }
        return effects;
    }

    /// <summary>Keeps worn effects applied as endless buffs and removes them when taken off.</summary>
    private void ApplyWorn(Farmer who)
    {
        ScreenState state = this.screen.Value;
        HashSet<string> worn = WornEffects(who);
        foreach (string id in state.AppliedByWear.Where(id => !worn.Contains(id)).ToList())
        {
            who.buffs.Remove(id);
            state.AppliedByWear.Remove(id);
        }
        foreach (string id in worn)
        {
            if (who.buffs.IsApplied(id) || !DataLoader.Buffs(Game1.content).ContainsKey(id))
                continue;
            who.applyBuff(new Buff(id) { millisecondsDuration = Buff.ENDLESS });
            state.AppliedByWear.Add(id);
        }
    }

    /// <summary>Planetary gift: the weekday's planet lends a stat (doc 15: Sun to Saturn), doubled by the Ring of the Seven.</summary>
    private void PlanetaryGift(Farmer who)
    {
        if (!Has(who, "Planetary"))
        {
            who.buffs.Remove(PlanetaryGiftBuff);
            return;
        }
        if (who.buffs.IsApplied(PlanetaryGiftBuff))
            return;
        bool strong = EffectService.HandlerParam(who, "Planetary", "Strong") == "true";
        int k = strong ? 2 : 1;
        var effects = new BuffEffects();
        string planet;
        switch (Game1.dayOfMonth % 7)
        {
            case 1: planet = "Moon"; effects.FishingLevel.Value = k; break;
            case 2: planet = "Mars"; effects.Attack.Value = 2 * k; break;
            case 3: planet = "Mercury"; effects.Speed.Value = 0.5f * k; break;
            case 4: planet = "Jupiter"; effects.LuckLevel.Value = k; break;
            case 5: planet = "Venus"; effects.ForagingLevel.Value = k; break;
            case 6: planet = "Saturn"; effects.Defense.Value = 2 * k; break;
            default: planet = "Sun"; effects.FarmingLevel.Value = k; break;
        }
        who.applyBuff(new Buff(PlanetaryGiftBuff, displayName: ModServices.T("effect.planet." + planet), duration: Buff.ENDLESS, effects: effects, iconTexture: Game1.buffsIcons, iconSheetIndex: 7));
    }

    /// <summary>The Rebis Amulet: attack by day, defence by night.</summary>
    private void RebisGift(Farmer who)
    {
        bool active = Has(who, "RebisAmulet");
        bool night = Game1.timeOfDay >= 1800;
        string mode = night ? "Night" : "Day";
        if (!active)
        {
            who.buffs.Remove(RebisBuff);
            return;
        }
        if (who.buffs.AppliedBuffs.TryGetValue(RebisBuff, out Buff? existing) && existing.source == mode)
            return;
        var effects = new BuffEffects();
        if (night)
            effects.Defense.Value = 3;
        else
            effects.AttackMultiplier.Value = 0.1f;
        who.applyBuff(new Buff(RebisBuff, source: mode, displayName: ModServices.T("effect.rebis." + mode), duration: Buff.ENDLESS, effects: effects, iconTexture: Game1.buffsIcons, iconSheetIndex: night ? 10 : 11));
    }

    // ── Combat: Kindled burns, the Censer, Ouroboros (doc 15) ───────────────────

    private void Combat(Farmer who)
    {
        ScreenState state = this.screen.Value;
        GameLocation location = who.currentLocation;
        bool kindled = Has(who, "Kindled");
        bool censer = Has(who, "Censer");
        foreach (Monster monster in location.characters.OfType<Monster>().ToList())
        {
            if (monster.Health <= 0)
                continue;
            if (kindled && state.MonsterHealth.TryGetValue(monster, out int before) && monster.Health < before && Game1.random.NextDouble() < Number(who, "Kindled", "Chance", 0.2))
                state.Burning[monster] = 3;
            if (state.Burning.TryGetValue(monster, out int left) && left > 0)
            {
                Hurt(location, monster, 4, who, Color.OrangeRed);
                state.Burning[monster] = left - 1;
            }
            if (censer && Vector2.Distance(monster.Tile, who.Tile) <= EffectService.HandlerNumber(who, "Censer", "Radius", 3))
                Hurt(location, monster, (int)EffectService.HandlerNumber(who, "Censer", "Damage", 6), who, Color.Gold);
            state.MonsterHealth[monster] = monster.Health;
        }
        foreach (Monster gone in state.MonsterHealth.Keys.Where(m => !location.characters.Contains(m)).ToList())
        {
            state.MonsterHealth.Remove(gone);
            state.Burning.Remove(gone);
        }
    }

    private static void Hurt(GameLocation location, Monster monster, int damage, Farmer who, Color color)
    {
        monster.takeDamage(damage, 0, 0, false, 0, who);
        location.debris.Add(new Debris(damage, new Vector2(monster.StandingPixel.X, monster.StandingPixel.Y - 32), color, 1f, monster));
        if (monster.Health <= 0)
            location.monsterDrop(monster, monster.StandingPixel.X, monster.StandingPixel.Y, who);
    }

    private void OnNpcListChanged(object? sender, NpcListChangedEventArgs e)
    {
        if (!e.IsCurrentLocation)
            return;
        Farmer who = Game1.player;
        if (!Has(who, "Ouroboros"))
            return;
        int kills = e.Removed.OfType<Monster>().Count(m => m.Health <= 0);
        if (kills > 0)
            who.Stamina = Math.Min(who.MaxStamina, who.Stamina + kills * (float)EffectService.HandlerNumber(who, "Ouroboros", "Energy", 1));
    }

    // ── Stones: Stonesense, Eruption, Midas (doc 15 §3a) ────────────────────────

    private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
    {
        try
        {
            foreach ((Vector2 tile, SObject obj) in e.Added)
            {
                if (obj.QualifiedItemId == DiffuserId && !obj.modData.ContainsKey(PlacedDayKey))
                    obj.modData[PlacedDayKey] = Game1.stats.DaysPlayed.ToString();
            }
            if (this.removingStones || !e.IsCurrentLocation)
                return;
            Farmer who = Game1.player;
            foreach ((Vector2 tile, SObject obj) in e.Removed)
            {
                if (!obj.IsBreakableStone())
                    continue;
                if (Roll(who, "Stonesense", 0.15))
                    Game1.createObjectDebris(OreFor(e.Location), (int)tile.X, (int)tile.Y, who.UniqueMultiplayerID, e.Location);
                if (Roll(who, "Eruption", 0.05))
                    this.Erupt(e.Location, tile, who);
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"A stone effect failed: {ex}");
        }
    }

    private static string OreFor(GameLocation location)
    {
        int level = location is MineShaft shaft ? shaft.mineLevel : 0;
        return level >= 121 ? "(O)386" : level >= 80 ? "(O)384" : level >= 40 ? "(O)380" : "(O)378";
    }

    private void Erupt(GameLocation location, Vector2 center, Farmer who)
    {
        this.removingStones = true;
        try
        {
            location.playSound("explosion");
            Game1.flashAlpha = 0.3f;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var tile = new Vector2(center.X + dx, center.Y + dy);
                    if (location.objects.TryGetValue(tile, out SObject? stone) && stone.IsBreakableStone())
                    {
                        location.OnStoneDestroyed(stone.ItemId, (int)tile.X, (int)tile.Y, who);
                        location.objects.Remove(tile);
                        Game1.createRadialDebris(location, 14, (int)tile.X, (int)tile.Y, 4, false);
                    }
                }
            }
        }
        finally
        {
            this.removingStones = false;
        }
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (!e.IsLocalPlayer || Game1.activeClickableMenu != null)
            return; // only fresh pickups, never transfers from a chest or a shop
        try
        {
            Farmer who = e.Player;
            foreach (Item item in e.Added)
            {
                bool mining = who.currentLocation is MineShaft or VolcanoDungeon || who.currentLocation?.Name == "Mountain";
                if (mining && NextOre.TryGetValue(item.QualifiedItemId, out string? better) && Roll(who, "Midas", 0.05))
                {
                    int index = who.Items.IndexOf(item);
                    if (index >= 0)
                        who.Items[index] = ItemRegistry.Create(better, item.Stack);
                    continue;
                }
                if (item is SObject { Category: SObject.junkCategory } && Has(who, "Ouroboros") && ModServices.Content.Registry.Settings.ResidueItem is string residue)
                {
                    int index = who.Items.IndexOf(item);
                    if (index >= 0)
                        who.Items[index] = ItemRegistry.Create(Ids.QualifyItem(residue), item.Stack);
                    continue;
                }
                if (item is SObject { bigCraftable.Value: false } && item.Quality is > 0 and < SObject.bestQuality && Roll(who, "Transmutation", 0.05))
                    item.Quality = item.Quality == SObject.highQuality ? SObject.bestQuality : item.Quality + 1;
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"A pickup effect failed: {ex}");
        }
    }

    // ── Patches: Savour, Good Cheer, Second Wind, Dissolution, Azoth Diffuser ──

    private static void EatPostfix(Farmer __instance, SObject o)
    {
        try
        {
            if (o.Category != SObject.CookingCategory || !Has(__instance, "Savour"))
                return;
            double bonus = Number(__instance, "Savour", "Bonus", 0.15);
            __instance.Stamina = Math.Min(__instance.MaxStamina, __instance.Stamina + (float)(o.staminaRecoveredOnConsumption() * bonus));
            __instance.health = Math.Min(__instance.maxHealth, __instance.health + (int)(o.healthRecoveredOnConsumption() * bonus));
        }
        catch (Exception ex)
        {
            Self.log.Error($"Savour failed: {ex}");
        }
    }

    private static bool inFriendship;

    private static void FriendshipPostfix(Farmer __instance, int amount, NPC n)
    {
        if (inFriendship || amount <= 0 || n is null || !Has(__instance, "GoodCheer"))
            return;
        int extra = (int)Math.Round(amount * Number(__instance, "GoodCheer", "Bonus", 0.05));
        if (extra <= 0)
            return;
        inFriendship = true;
        try
        {
            __instance.changeFriendship(extra, n);
        }
        finally
        {
            inFriendship = false;
        }
    }

    private static void TakeDamagePostfix(Farmer __instance)
    {
        if (__instance.health > 0 || !Has(__instance, "SecondWind"))
            return;
        __instance.health = Math.Max(1, __instance.maxHealth / 4);
        foreach (string id in __instance.buffs.AppliedBuffs.Keys.Where(id => IsOurs(id, "SecondWind")).ToList())
            __instance.buffs.Remove(id);
        Game1.playSound("yoba");
        Game1.addHUDMessage(new HUDMessage(ModServices.T("effect.second-wind"), HUDMessage.health_type));
    }

    /// <summary>Dissolution: stones, wood and weeds break in one hit for a number of swings.</summary>
    private static void ToolPrefix(Tool __instance, Farmer who)
    {
        savedToolLevel = -1;
        if (who is null || !Has(who, "Dissolution"))
            return;
        savedToolLevel = __instance.UpgradeLevel;
        __instance.UpgradeLevel = Math.Max(__instance.UpgradeLevel, 4);
        int swings = (who.modData.TryGetValue(SwingsKey, out string? raw) && int.TryParse(raw, out int n) ? n : 0) + 1;
        if (swings >= (int)EffectService.HandlerNumber(who, "Dissolution", "Swings", 40))
        {
            foreach (string id in who.buffs.AppliedBuffs.Keys.Where(id => IsOurs(id, "Dissolution")).ToList())
                who.buffs.Remove(id);
            swings = 0;
        }
        who.modData[SwingsKey] = swings.ToString();
    }

    private static void ToolFinalizer(Tool __instance)
    {
        if (savedToolLevel >= 0)
            __instance.UpgradeLevel = savedToolLevel;
        savedToolLevel = -1;
    }

    /// <summary>The Azoth Diffuser: any machine started within its radius runs 25% faster; diffusers never stack (doc 15).</summary>
    private static void PlaceInMachinePostfix(SObject __instance, bool __result, bool probe)
    {
        try
        {
            if (!__result || probe || __instance.Location is not GameLocation location || __instance.MinutesUntilReady <= 0)
                return;
            int radius = ModServices.Config.DiffuserRadius;
            bool near = location.objects.Values.Any(o => o.QualifiedItemId == DiffuserId && Vector2.Distance(o.TileLocation, __instance.TileLocation) <= radius);
            if (near)
                __instance.MinutesUntilReady = Math.Max(10, (int)(__instance.MinutesUntilReady * 0.75));
        }
        catch (Exception ex)
        {
            Self.log.Error($"The Azoth Diffuser failed: {ex}");
        }
    }

    private void CrumbleDiffusers()
    {
        int day = (int)Game1.stats.DaysPlayed;
        Utility.ForEachLocation(location =>
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs.Where(p => p.Value.QualifiedItemId == DiffuserId).ToList())
            {
                if (!obj.modData.TryGetValue(PlacedDayKey, out string? raw) || !int.TryParse(raw, out int placed) || day - placed < 7)
                    continue;
                location.objects.Remove(tile);
                if (ModServices.Content.Registry.Settings.ResidueItem is string residue)
                    Game1.createItemDebris(ItemRegistry.Create(Ids.QualifyItem(residue), 3), tile * 64f + new Vector2(32, 32), 0, location);
            }
            return true;
        });
    }

    /// <summary>A planted Radicula shrieks now and then, stunning monsters nearby.</summary>
    private void Radiculas(GameLocation location)
    {
        foreach (SObject plant in location.objects.Values.Where(o => o.QualifiedItemId == RadiculaId))
        {
            if (Game1.random.NextDouble() > 0.2)
                continue;
            List<Monster> near = location.characters.OfType<Monster>().Where(m => Vector2.Distance(m.Tile, plant.TileLocation) <= 5).ToList();
            if (near.Count == 0)
                continue;
            location.playSound("batScreech");
            plant.shakeTimer = 300;
            foreach (Monster monster in near)
                monster.stunTime.Value = 2000;
        }
    }

    // ── Apply-to-use products (doc 15 §3) ───────────────────────────────────────

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.ActiveObject is not SObject held || ModServices.Content is null)
            return;
        try
        {
            ProductUse? use = ModServices.Content.Registry.ProductForItem(held.QualifiedItemId)?.Use(UseKinds.Apply);
            if (use is null)
                return;
            Farmer who = Game1.player;
            GameLocation location = who.currentLocation;
            if (location.objects.ContainsKey(e.Cursor.GrabTile) && use.System != "Field")
                return; // let the object's own action run (machines take our items)
            if (use.System == "Field" && use.Ref == "Weedkiller")
            {
                int killed = 0;
                foreach ((Vector2 tile, SObject obj) in location.objects.Pairs.Where(p => p.Value.IsWeeds() && Vector2.Distance(p.Key, e.Cursor.GrabTile) <= 2.5f).ToList())
                {
                    location.objects.Remove(tile);
                    Game1.createRadialDebris(location, 12, (int)tile.X, (int)tile.Y, 4, false);
                    killed++;
                }
                if (killed == 0)
                    return;
                location.playSound("leafrustle");
            }
            else
            {
                foreach (string effect in use.Effects)
                    who.applyBuff(effect);
                location.playSound("bubbles");
            }
            who.reduceActiveItemByOne();
            ModServices.Helper.Input.Suppress(e.Button);
        }
        catch (Exception ex)
        {
            this.log.Error($"Using {held.QualifiedItemId} failed: {ex}");
        }
    }
}
