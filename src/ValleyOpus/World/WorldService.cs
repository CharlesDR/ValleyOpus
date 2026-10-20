using System.Text.Json;
using System.Text.Json.Nodes;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Engine.State;
using ValleyOpus.Framework;

namespace ValleyOpus.World;

/// <summary>
/// The <c>World</c> module (doc 14, doc 09 §3): unlock state and every unlock source (the intro event, room
/// restoration, delays, skills, friendship, storms, discoveries, contracts), what each unlock grants, workshop
/// bundles and rooms, Sorenson's contracts, and the hints players have been shown. The host decides; every player
/// reads the same copy and applies their own share of grants (recipes, starter items, letters) on their screen.
/// </summary>
internal sealed class WorldService
{
    public const string WorldKey = Ids.KeyPrefix + "World";
    public const string ClaimedKey = Ids.KeyPrefix + "Claimed";
    public const string UnlockFlagPrefix = Ids.RowPrefix + "Unlock_";
    public const string IntroEventId = Ids.RowPrefix + "Intro";
    public const string IntroUnlock = Ids.RowPrefix + "Intro";
    public const string EntranceHall = Ids.RowPrefix + "EntranceHall";

    private readonly IModHelper helper;
    private readonly SmapiLog log;
    private WorldData data = new();
    private string? cachedJson;
    private WorldData cachedView = new();

    public WorldService(IModHelper helper, SmapiLog log)
    {
        this.helper = helper;
        this.log = log;
        this.Familiars = new Familiars(helper, log, this);
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecond;
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => this.data = new WorldData();
        helper.Events.Multiplayer.ModMessageReceived += this.OnMessage;
        ModServices.Farm.EnvelopeLoaded += this.OnEnvelopeLoaded;
        ModServices.Farm.EnvelopeSaving += this.OnEnvelopeSaving;
    }

    public Familiars Familiars { get; }

    /// <summary>The world progress every screen reads: the host's own copy, or the host's mirror on a farmhand.</summary>
    public WorldData View
    {
        get
        {
            if (Context.IsMainPlayer)
                return this.data;
            if (Game1.MasterPlayer?.modData.TryGetValue(WorldKey, out string? json) != true || json is null)
                return this.cachedView;
            if (json != this.cachedJson)
            {
                this.cachedJson = json;
                this.cachedView = Read(json) ?? new WorldData();
            }
            return this.cachedView;
        }
    }

    private static WorldData? Read(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WorldData>(json, EnvelopeJson.Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ── Save data ───────────────────────────────────────────────────────────────

    private void OnEnvelopeLoaded(FarmEnvelope envelope)
    {
        this.data = envelope.Unlocks.Count > 0 ? envelope.Unlocks.Deserialize<WorldData>(EnvelopeJson.Options) ?? new WorldData() : new WorldData();
        // The mail flags are the truth content packs read; make sure every granted unlock still has its flag.
        foreach (string id in this.data.Granted.Keys)
            Game1.MasterPlayer.mailReceived.Add(Flag(id));
        this.Publish();
        this.log.Trace($"World loaded: {this.data.Granted.Count} unlocks, {this.data.BundlesDone.Count} bundles, {this.data.Contracts.Count} contracts, {this.data.Familiars.Count} familiars.");
    }

    private void OnEnvelopeSaving(FarmEnvelope envelope)
        => envelope.Unlocks = JsonSerializer.SerializeToNode(this.data, EnvelopeJson.Options)!.AsObject();

    /// <summary>Mirrors the host's progress into the host farmer's <c>modData</c>, which the game syncs to every player.</summary>
    public void Publish()
    {
        if (Context.IsMainPlayer)
            Game1.MasterPlayer.modData[WorldKey] = JsonSerializer.Serialize(this.data, EnvelopeJson.Options);
    }

    // ── Unlock queries ──────────────────────────────────────────────────────────

    public static string Flag(string unlockId) => UnlockFlagPrefix + Local(unlockId);

    public static string Local(string id) => id.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) ? id[Ids.RowPrefix.Length..] : id;

    public static string Full(string id) => id.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) ? id : Ids.RowPrefix + id;

    /// <summary>Whether the farm holds an unlock; read from the host's mail flag, so it's the same on every screen.</summary>
    public bool IsUnlocked(string unlockId) => Game1.MasterPlayer?.mailReceived.Contains(Flag(unlockId)) == true;

    /// <summary>Whether a workshop room is restored (its bundles done); the Entrance Hall counts once the intro is seen.</summary>
    public bool IsRestored(string roomId)
    {
        roomId = Full(roomId);
        if (roomId == EntranceHall)
            return this.IsUnlocked(IntroUnlock);
        WorkshopRoomRow? room = ModServices.Content.Registry.Rooms.Get(roomId);
        return room != null && room.Bundles.Count > 0 && room.Bundles.All(b => this.View.BundlesDone.Contains(b));
    }

    /// <summary>Whether a room's door opens: every room in AllOf restored, and any one in AnyOf (doc 14 §2).</summary>
    public bool IsOpen(string roomId)
    {
        roomId = Full(roomId);
        if (roomId == EntranceHall)
            return this.IsUnlocked(IntroUnlock);
        if (ModServices.Content.Registry.Rooms.Get(roomId) is not WorkshopRoomRow room || !this.IsUnlocked(IntroUnlock))
            return false;
        if (room.Opens.AllOf is { } all && !all.All(this.IsRestored))
            return false;
        if (room.Opens.AnyOf is { Count: > 0 } any && !any.Any(this.IsRestored))
            return false;
        return true;
    }

    private bool RequirementsHold(UnlockRow row)
    {
        foreach (string requirement in row.Requires)
        {
            if (ModServices.Content.Registry.Unlocks.Get(requirement) != null)
            {
                if (!this.IsUnlocked(requirement))
                    return false;
            }
            else if (!GameStateQuery.CheckConditions(requirement, Game1.getFarm(), Game1.MasterPlayer))
                return false;
        }
        return true;
    }

    // ── Unlock sources (host) ───────────────────────────────────────────────────

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        try
        {
            if (Context.IsMainPlayer)
            {
                this.Evaluate(morning: true);
                Machines.LeyTaps.OnDayStarted(this.log);
                Machines.Power.OnDayStarted();
            }
            this.ClaimLocal();
        }
        catch (Exception ex)
        {
            this.log.Error($"The morning world update failed: {ex}");
        }
    }

    private int ticks;

    private void OnOneSecond(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        try
        {
            // Sources that can fire mid-day (an event seen, a level reached, a discovery) are checked every few seconds.
            if (Context.IsMainPlayer && ++this.ticks % 3 == 0)
                this.Evaluate(morning: false);
            if (Context.IsPlayerFree)
                this.ClaimLocal();
        }
        catch (Exception ex)
        {
            this.log.Error($"The world update failed: {ex}");
        }
    }

    /// <summary>Checks every unlock not yet held and grants those whose source fired (host only).</summary>
    public void Evaluate(bool morning)
    {
        if (!Context.IsMainPlayer)
            return;
        int day = (int)Game1.stats.DaysPlayed;
        bool changed = true;
        int passes = 0;
        while (changed && passes++ < 8) // grants can satisfy other unlocks' requirements
        {
            changed = false;
            foreach (UnlockRow row in ModServices.Content.Registry.Unlocks.All)
            {
                if (this.IsUnlocked(row.Id) || !this.RequirementsHold(row))
                    continue;
                if (this.SourceFired(row, day, morning))
                {
                    this.Grant(row.Id, why: row.Source.Type);
                    changed = true;
                }
            }
        }
    }

    private bool SourceFired(UnlockRow row, int day, bool morning)
    {
        string reference = row.Source.Ref ?? "";
        switch (row.Source.Type)
        {
            case UnlockSources.Intro:
                return Game1.getAllFarmers().Any(f => f.eventsSeen.Contains(IntroEventId));

            case UnlockSources.WorkshopRoom:
                return this.IsRestored(reference);

            case UnlockSources.Delay:
                if (!this.data.Since.TryGetValue(row.Id, out int since))
                {
                    this.data.Since[row.Id] = since = day;
                    this.Publish();
                }
                int delay = int.TryParse(reference, out int days) ? days : 0;
                return delay == 0 || (morning && day - since >= delay);

            case UnlockSources.Skill:
            {
                string[] parts = reference.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length == 2 && int.TryParse(parts[1], out int level) && Game1.getOnlineFarmers().Any(f => SkillLevel(f, parts[0]) >= level);
            }

            case UnlockSources.Friendship:
            {
                string[] parts = reference.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length == 2 && int.TryParse(parts[1], out int hearts)
                    && Game1.getAllFarmers().Any(f => f.friendshipData.TryGetValue(parts[0], out Friendship? friendship) && friendship.Points >= hearts * 250);
            }

            case UnlockSources.Storm:
                if (!morning)
                    return false;
                if (this.data.StormSeen.Contains(row.Id))
                    return true; // the morning after the storm
                if (Game1.isLightning || Game1.netWorldState.Value.GetWeatherForLocation("Default").IsLightning)
                {
                    this.data.StormSeen.Add(row.Id);
                    this.Publish();
                }
                return false;

            case UnlockSources.Discoveries:
                return int.TryParse(reference, out int count) && ModServices.Farm.Codex.Data.Keys.Keys.Count(k => k.StartsWith(CodexKeys.RecipePrefix, StringComparison.Ordinal)) >= count;

            case UnlockSources.AnyOf:
                return reference.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(id => this.IsUnlocked(Full(id)));

            case UnlockSources.Discovery:
            {
                string? item = ModServices.Content.Registry.ResolveItemId(reference);
                return item != null && ModServices.Farm.Codex.Has(CodexKeys.Item(item));
            }

            default:
                return false; // Contract: granted by delivery
        }
    }

    /// <summary>A vanilla skill by name, or the alchemy skill by its ID.</summary>
    private static int SkillLevel(Farmer who, string skill)
        => skill switch
        {
            "Farming" => who.FarmingLevel,
            "Fishing" => who.FishingLevel,
            "Foraging" => who.ForagingLevel,
            "Mining" => who.MiningLevel,
            "Combat" => who.CombatLevel,
            _ => SpaceCore.Skills.GetSkillLevel(who, skill),
        };

    // ── Grants ──────────────────────────────────────────────────────────────────

    /// <summary>Grants an unlock (host): its flag, farm-wide grants, and a notice. Per-player grants follow on each screen.</summary>
    public bool Grant(string unlockId, string why)
    {
        unlockId = Full(unlockId);
        if (!Context.IsMainPlayer || ModServices.Content.Registry.Unlocks.Get(unlockId) is not UnlockRow row || this.IsUnlocked(unlockId))
            return false;
        int day = (int)Game1.stats.DaysPlayed;
        this.data.Granted[unlockId] = day;
        Game1.MasterPlayer.mailReceived.Add(Flag(unlockId));
        foreach (UnlockGrant grant in row.Grants)
        {
            switch (grant.Type)
            {
                case GrantTypes.Flag:
                    Game1.MasterPlayer.mailReceived.Add(grant.Ref);
                    break;
                case GrantTypes.Capacity when grant.Ref == "TableSlots":
                    ModServices.Farm.Codex.Data.TableCapacity = Math.Max(ModServices.Farm.Codex.Data.TableCapacity, 3);
                    break;
            }
        }
        Workshop.PlaceStarterEquipment(row.Grants.Where(g => g.Type == GrantTypes.Place).Select(g => g.Ref).ToList());
        this.Publish();
        this.log.Info($"Unlock {Local(unlockId)} granted ({why}) on day {day}.");
        if (row.NameKey != null && row.Kind != "Hidden" && row.Source.Type != UnlockSources.AnyOf)
            Game1.addHUDMessage(new HUDMessage(ModServices.T("world.unlocked", new { name = ModServices.T(row.NameKey) }), HUDMessage.achievement_type));
        this.ClaimLocal();
        return true;
    }

    /// <summary>
    /// Applies this screen's player's share of every granted unlock once: crafting recipes, starter items and
    /// letters. Runs on every screen, so farmhands and late joiners get theirs too (doc 17 §1).
    /// </summary>
    public void ClaimLocal()
    {
        Farmer who = Game1.player;
        if (who is null || Game1.MasterPlayer is null)
            return;
        HashSet<string> claimed = who.modData.TryGetValue(ClaimedKey, out string? raw) && raw.Length > 0
            ? new HashSet<string>(raw.Split('|'), StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        bool changed = false;
        var items = new List<Item>();
        foreach (UnlockRow row in ModServices.Content.Registry.Unlocks.All)
        {
            if (claimed.Contains(row.Id) || !this.IsUnlocked(row.Id))
                continue;
            foreach (UnlockGrant grant in row.Grants)
            {
                switch (grant.Type)
                {
                    case GrantTypes.CraftingRecipe:
                        string recipe = grant.Ref;
                        if (!who.craftingRecipes.ContainsKey(recipe))
                            who.craftingRecipes.Add(recipe, 0);
                        break;
                    case GrantTypes.Item:
                        if (ItemRegistry.Create(grant.Ref, Math.Max(1, grant.Count), allowNull: true) is Item item)
                            items.Add(item);
                        break;
                    case GrantTypes.Mail:
                        if (!who.mailReceived.Contains(grant.Ref) && !who.mailbox.Contains(grant.Ref))
                            who.mailbox.Add(grant.Ref);
                        break;
                }
            }
            claimed.Add(row.Id);
            changed = true;
        }
        if (!changed)
            return;
        who.modData[ClaimedKey] = string.Join("|", claimed);
        if (items.Count > 0)
            who.addItemsByMenuIfNecessary(items);
    }

    // ── Bundles (doc 14 §3) ─────────────────────────────────────────────────────

    public static CompiledSpec Compile(IngredientSpec spec)
        => new(spec, spec.Item is null ? null : ModServices.Content.Registry.ResolveItemId(spec.Item) ?? Ids.QualifyItem(spec.Item));

    /// <summary>The first open slot of a bundle this item fills, or -1 (counts must be met in one donation, like the CC).</summary>
    public int SlotFor(WorkshopBundleRow bundle, Item item)
    {
        WorldData view = this.View;
        if (view.BundlesDone.Contains(bundle.Id))
            return -1;
        view.Bundles.TryGetValue(bundle.Id, out Dictionary<int, string>? filled);
        for (int i = 0; i < bundle.Slots.Count; i++)
        {
            if (filled?.ContainsKey(i) == true)
                continue;
            IngredientSpec spec = bundle.Slots[i];
            if (item.Stack < Math.Max(1, spec.Count) || !Compile(spec).Matches(GameContent.ToInfo(item), item.Quality))
                continue;
            if (bundle.Distinct && filled?.ContainsValue(item.QualifiedItemId) == true)
                continue;
            return i;
        }
        return -1;
    }

    public int Filled(WorkshopBundleRow bundle) => this.View.Bundles.TryGetValue(bundle.Id, out var filled) ? filled.Count : 0;

    public bool IsSlotFilled(WorkshopBundleRow bundle, int slot, out string? itemId)
    {
        itemId = null;
        return this.View.Bundles.TryGetValue(bundle.Id, out var filled) && filled.TryGetValue(slot, out itemId);
    }

    /// <summary>Donates an item from a player's inventory to a bundle slot; pays the reward and restores the room when complete.</summary>
    public bool Donate(Farmer who, WorkshopBundleRow bundle, Item item)
    {
        int slot = this.SlotFor(bundle, item);
        if (slot < 0)
            return false;
        int count = Math.Max(1, bundle.Slots[slot].Count);
        string itemId = item.QualifiedItemId;
        item.Stack -= count;
        if (item.Stack <= 0)
            who.removeItemFromInventory(item);
        Game1.playSound("newArtifact");

        bool completes = this.Filled(bundle) + 1 >= bundle.RequiredCount;
        if (Context.IsMainPlayer)
            this.ApplyDonation(bundle, slot, itemId);
        else
            this.Send(new WorldRequest { Kind = "Donate", Id = bundle.Id, Slot = slot, ItemId = itemId });

        if (completes && bundle.Reward is { } reward && ItemRegistry.Create(reward.Item, reward.Count, allowNull: true) is Item gift)
        {
            who.addItemByMenuIfNecessary(gift);
            Game1.playSound("achievement");
            Game1.addHUDMessage(new HUDMessage(ModServices.T("world.bundle-done", new { name = BundleName(bundle) }), HUDMessage.achievement_type));
        }
        return true;
    }

    private void ApplyDonation(WorkshopBundleRow bundle, int slot, string itemId)
    {
        if (!this.data.Bundles.TryGetValue(bundle.Id, out Dictionary<int, string>? filled))
            this.data.Bundles[bundle.Id] = filled = new Dictionary<int, string>();
        filled[slot] = itemId;
        if (filled.Count >= bundle.RequiredCount && this.data.BundlesDone.Add(bundle.Id))
        {
            this.log.Info($"Bundle {Local(bundle.Id)} complete on day {Game1.stats.DaysPlayed}.");
            if (this.IsRestored(bundle.Room))
            {
                this.log.Info($"Room {Local(bundle.Room)} restored.");
                Game1.playSound("discoverMineral");
            }
        }
        this.Publish();
        this.Evaluate(morning: false);
    }

    public static string BundleName(WorkshopBundleRow bundle) => bundle.NameKey != null ? ModServices.T(bundle.NameKey) : Local(bundle.Id);

    public static string RoomName(string roomId)
        => ModServices.Content.Registry.Rooms.Get(Full(roomId))?.NameKey is string key ? ModServices.T(key) : Local(roomId);

    // ── Contracts (doc 15 §6) ───────────────────────────────────────────────────

    public const string SorensonUnlock = Ids.RowPrefix + "SorensonArrives";

    /// <summary>Contracts on offer: the first three not yet delivered, in pace order.</summary>
    public IReadOnlyList<ContractRow> OpenContracts()
    {
        string[] pace = { "Spring", "Summer", "Fall", "Winter" };
        return ModServices.Content.Registry.Contracts.All
            .Where(c => !this.View.Contracts.ContainsKey(c.Id))
            .OrderBy(c => Array.IndexOf(pace, c.Pace ?? "Summer"))
            .ThenBy(c => c.Id, StringComparer.Ordinal)
            .Take(3)
            .ToList();
    }

    /// <summary>Delivers a contract from a player's inventory: gold to them, the reward unlock to the farm.</summary>
    public bool Deliver(Farmer who, ContractRow contract)
    {
        string item = Ids.QualifyItem(contract.Item);
        int have = who.Items.Where(i => i?.QualifiedItemId == item).Sum(i => i.Stack);
        if (have < contract.Count)
            return false;
        int left = contract.Count;
        foreach (Item stack in who.Items.Where(i => i?.QualifiedItemId == item).ToList())
        {
            int take = Math.Min(left, stack.Stack);
            stack.Stack -= take;
            left -= take;
            if (stack.Stack <= 0)
                who.removeItemFromInventory(stack);
            if (left == 0)
                break;
        }
        who.Money += contract.Gold;
        Game1.playSound("purchase");
        if (Context.IsMainPlayer)
            this.ApplyContract(contract.Id);
        else
            this.Send(new WorldRequest { Kind = "Contract", Id = contract.Id });
        this.log.Info($"{who.Name} delivered contract {Local(contract.Id)} for {contract.Gold}g.");
        return true;
    }

    private void ApplyContract(string contractId)
    {
        if (ModServices.Content.Registry.Contracts.Get(contractId) is not ContractRow contract)
            return;
        this.data.Contracts[contractId] = (int)Game1.stats.DaysPlayed;
        if (contract.RewardUnlock != null)
            this.Grant(contract.RewardUnlock, "Contract");
        this.Publish();
    }

    // ── Hints (doc 07 §3, doc 16) ───────────────────────────────────────────────

    /// <summary>Whether a hint still points at something worth hinting: its target isn't discovered yet.</summary>
    public bool HintUseful(HintRow hint)
    {
        if (ModServices.Farm.Codex.Has(CodexKeys.Hint(hint.Id)))
            return false;
        if (hint.Requires != null && ModServices.Content.Registry.Unlocks.Get(hint.Requires) != null && !this.IsUnlocked(hint.Requires))
            return false;
        if (ModServices.Content.Registry.Recipes.Get(hint.Target) != null)
            return !ModServices.Farm.Codex.IsDiscovered(hint.Target);
        string? item = ModServices.Content.Registry.ResolveItemId(hint.Target);
        return item is null || !ModServices.Farm.Codex.Has(CodexKeys.Item(item));
    }

    /// <summary>Hints of a channel the farm hasn't seen, whose target is still undiscovered.</summary>
    public List<HintRow> AvailableHints(string channel)
        => ModServices.Content.Registry.Hints.All.Where(h => h.Channel == channel && this.HintUseful(h)).OrderBy(h => h.Id, StringComparer.Ordinal).ToList();

    /// <summary>Records a hint as shown (it joins the codex Notes page) and returns its text.</summary>
    public string Reveal(Farmer who, HintRow hint)
    {
        ModServices.Farm.Record(who, new[] { CodexKeys.Hint(hint.Id) });
        this.log.Trace($"Hint {Local(hint.Id)} shown to {who.Name} via {hint.Channel}.");
        return HintText(hint);
    }

    public static string HintText(HintRow hint) => hint.TextKey != null ? ModServices.T(hint.TextKey) : Local(hint.Target);

    public void CountDivination(int times)
    {
        if (Context.IsMainPlayer)
        {
            this.data.Divinations += times;
            this.Publish();
        }
        else
            this.Send(new WorldRequest { Kind = "Divination", Slot = times });
    }

    // ── Multiplayer ─────────────────────────────────────────────────────────────

    private void Send(WorldRequest request)
    {
        if (Context.IsMultiplayer)
            this.helper.Multiplayer.SendMessage(request, WorldMessages.Request, modIDs: new[] { ModServices.Manifest.UniqueID }, playerIDs: new[] { Game1.MasterPlayer.UniqueMultiplayerID });
    }

    public void SendFamiliar(string kind, string id, string? arg = null) => this.Send(new WorldRequest { Kind = kind, Id = id, ItemId = arg });

    private void OnMessage(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != ModServices.Manifest.UniqueID || e.Type != WorldMessages.Request || !Context.IsMainPlayer)
            return;
        try
        {
            WorldRequest request = e.ReadAs<WorldRequest>();
            switch (request.Kind)
            {
                case "Donate" when ModServices.Content.Registry.Bundles.Get(request.Id) is { } bundle && request.ItemId != null:
                    this.ApplyDonation(bundle, request.Slot, request.ItemId);
                    break;
                case "Contract":
                    this.ApplyContract(request.Id);
                    break;
                case "Divination":
                    this.CountDivination(request.Slot);
                    break;
                default:
                    this.Familiars.OnRequest(request, e.FromPlayerID);
                    break;
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"Couldn't apply a world request from player {e.FromPlayerID}: {ex}");
        }
    }

    // ── Dev commands ────────────────────────────────────────────────────────────

    /// <summary>Restores a room by filling its bundles (dev command); rewards aren't paid.</summary>
    public void DevRestore(string roomId)
    {
        if (ModServices.Content.Registry.Rooms.Get(Full(roomId)) is not WorkshopRoomRow room)
            return;
        foreach (string bundleId in room.Bundles)
        {
            if (ModServices.Content.Registry.Bundles.Get(bundleId) is not WorkshopBundleRow bundle)
                continue;
            for (int i = 0; i < bundle.Slots.Count; i++)
            {
                if (!this.data.Bundles.TryGetValue(bundle.Id, out var filled))
                    this.data.Bundles[bundle.Id] = filled = new Dictionary<int, string>();
                filled.TryAdd(i, "dev");
            }
            this.data.BundlesDone.Add(bundle.Id);
        }
        this.Publish();
        this.Evaluate(morning: false);
    }

    public void DevReset()
    {
        foreach (string id in this.data.Granted.Keys)
            Game1.MasterPlayer.mailReceived.Remove(Flag(id));
        this.data = new WorldData();
        Game1.player.modData.Remove(ClaimedKey);
        this.Publish();
    }

    public WorldData HostData => this.data;
}
