using System.Text.Json.Nodes;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Net;
using ValleyOpus.Engine.State;
using ValleyOpus.Framework;

namespace ValleyOpus.State;

/// <summary>What SMAPI saves for us: the envelope as JSON text, so the engine's System.Text.Json model round-trips exactly.</summary>
internal sealed class EnvelopeBox
{
    public string Json { get; set; } = "";
}

/// <summary>
/// The <c>FarmState</c>, <c>Saves</c> and <c>Net</c> modules (doc 09 §3, doc 17): the host's envelope and farm codex,
/// discovery recording with XP and celebrations, multiplayer codex sync, and the save-time Lab strip (S3).
/// </summary>
internal sealed class FarmState
{
    /// <summary>SMAPI prefixes save-data keys with our mod ID, so this is stored as the doc's <c>CharlesDR.ValleyOpus/Farm</c>.</summary>
    private const string SaveDataKey = "Farm";

    private readonly IModHelper helper;
    private readonly SmapiLog log;
    private readonly PerScreen<ToastQueue> toasts = new(() => new ToastQueue());
    private readonly HashSet<long> mismatchedPeers = new();
    private LoadResult? loaded;

    public FarmState(IModHelper helper, SmapiLog log)
    {
        this.helper = helper;
        this.log = log;
        this.Strip = new BuildingStrip(log);
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.Saving += this.OnSaving;
        helper.Events.GameLoop.Saved += this.OnSaved;
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => this.Reset();
        helper.Events.GameLoop.UpdateTicked += (_, _) => this.toasts.Value.Tick();
        helper.Events.Multiplayer.ModMessageReceived += this.OnMessage;
        helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
    }

    public FarmEnvelope Envelope { get; private set; } = new();

    /// <summary>Raised on the host after the envelope loads, so other modules read their sections.</summary>
    public event Action<FarmEnvelope>? EnvelopeLoaded;

    /// <summary>Raised on the host just before the envelope is written, so other modules store their sections.</summary>
    public event Action<FarmEnvelope>? EnvelopeSaving;

    public FarmCodex Codex { get; private set; } = new();

    public BuildingStrip Strip { get; }

    /// <summary>Whether this save may be written (a newer build's save loads read-only, D13).</summary>
    public bool CanWrite => this.loaded?.CanWrite ?? true;

    private string GameId => Game1.uniqueIDForThisGame.ToString();

    private void Reset()
    {
        this.Envelope = new FarmEnvelope();
        this.Codex = NewCodex(null);
        this.loaded = null;
        this.mismatchedPeers.Clear();
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.Reset();
        if (!Context.IsMainPlayer)
        {
            this.Send(new VersionHello { ModVersion = ModServices.Manifest.Version.ToString(), Schema = FarmEnvelope.CurrentSchemaVersion }, MessageTypes.VersionHello, Game1.MasterPlayer.UniqueMultiplayerID);
            return;
        }

        EnvelopeBox? box = this.helper.Data.ReadSaveData<EnvelopeBox>(SaveDataKey);
        JsonObject? saved = string.IsNullOrWhiteSpace(box?.Json) ? null : JsonNode.Parse(box.Json) as JsonObject;
        this.loaded = new MigrationRunner().Load(saved, ModServices.Manifest.Version.ToString());
        foreach (string line in this.loaded.Log)
            this.log.Info(line);
        if (this.loaded.Mode == LoadMode.ReadOnly)
            this.log.Warn("This save was made by a newer ValleyOpus. It loads read-only: nothing ValleyOpus records will be saved (D13).");
        else if (this.loaded.Mode == LoadMode.Failed)
            this.log.Error("ValleyOpus couldn't migrate this save's data; it loads read-only so nothing is lost.");

        this.Envelope = this.loaded.Envelope;
        this.Codex = NewCodex(this.Envelope.Codex);
        this.Codex.ApplySecretPicks(ModServices.Content.Registry, this.GameId);
        this.Strip.AfterLoad(this.Envelope);
        this.EnvelopeLoaded?.Invoke(this.Envelope);
        this.log.Trace($"Farm codex loaded: {this.Codex.Data.Keys.Count} keys.");
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        if (!Context.IsMainPlayer)
            return;
        // The strip runs even on a read-only save: it keeps a removed mod from breaking the save (S3).
        this.Strip.BeforeSave(this.Envelope);
        if (!this.CanWrite)
            return;
        this.Envelope.ModVersion = ModServices.Manifest.Version.ToString();
        this.Envelope.SchemaVersion = FarmEnvelope.CurrentSchemaVersion;
        this.Envelope.Codex = this.Codex.Data;
        this.EnvelopeSaving?.Invoke(this.Envelope);
        this.helper.Data.WriteSaveData(SaveDataKey, new EnvelopeBox { Json = EnvelopeJson.Write(this.Envelope) });
        if (this.loaded?.Backup is JsonObject backup)
            this.helper.Data.WriteSaveData("Backup_v" + this.loaded.FromVersion, new EnvelopeBox { Json = backup.ToJsonString() });
    }

    private void OnSaved(object? sender, SavedEventArgs e)
    {
        if (Context.IsMainPlayer)
            this.Strip.AfterSave(this.Envelope);
    }

    /// <summary>A codex whose story-gated recipes read the farm's unlocks (doc 14 §1).</summary>
    private static FarmCodex NewCodex(CodexData? data)
        => new(data) { IsUnlocked = id => ModServices.World?.IsUnlocked(id) ?? true };

    // ── Discovery ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Records codex keys for a player and returns the ones that are new on this screen. The host writes and
    /// broadcasts; a farmhand writes its mirror and asks the host (doc 17 §3). Counts are added the same way.
    /// </summary>
    public List<string> Record(Farmer who, IEnumerable<string> keys, IReadOnlyDictionary<string, int>? counts = null)
    {
        int day = (int)Game1.stats.DaysPlayed;
        var fresh = keys.Where(k => this.Codex.Record(k, day, who.UniqueMultiplayerID)).ToList();
        var countDelta = new Dictionary<string, int>();
        foreach ((string key, int add) in counts ?? new Dictionary<string, int>())
        {
            this.Codex.Data.Counts[key] = this.Codex.Data.Counts.GetValueOrDefault(key) + add;
            countDelta[key] = this.Codex.Data.Counts[key];
        }

        if (fresh.Count == 0 && countDelta.Count == 0)
            return fresh;
        if (Context.IsMainPlayer)
        {
            if (Context.IsMultiplayer)
            {
                var delta = new CodexDelta { Counts = countDelta };
                foreach (string key in fresh)
                    delta.Keys[key] = this.Codex.Data.Keys[key];
                this.Send(delta, MessageTypes.CodexDelta, null);
            }
        }
        else
            this.Send(new CodexRequest { Keys = fresh, Counts = counts?.ToDictionary(p => p.Key, p => p.Value) ?? new(), Day = day }, MessageTypes.CodexRequest, Game1.MasterPlayer.UniqueMultiplayerID);
        return fresh;
    }

    /// <summary>A toast for this screen; toasts within two seconds merge into one (doc 16 §7).</summary>
    public void Toast(string text, Item? icon = null)
    {
        if (ModServices.Config.Toasts == "None")
            return;
        this.toasts.Value.Add(text, icon);
    }

    /// <summary>Holds an item up with a message (the clock pauses in single player, S5); a toast when someone else is connected.</summary>
    public void Celebrate(Item? item, string message, Item? icon = null)
    {
        if (item != null && !Context.IsMultiplayer && Game1.activeClickableMenu is null)
            Game1.player.holdUpItemThenMessage(item, showMessage: true);
        this.Toast(message, item ?? icon);
    }

    /// <summary>Checks milestones after a discovery and celebrates each new one.</summary>
    public void CheckMilestones(Farmer who)
    {
        foreach (MilestoneRow milestone in ModServices.Content.Grid.NewMilestones(this.Codex))
        {
            this.Record(who, new[] { CodexKeys.Milestone(milestone.Id) });
            string text = milestone.TextKey != null ? ModServices.T(milestone.TextKey) : milestone.Id;
            if (milestone.Celebration is "HoldUp" or "Flourish")
                this.Celebrate(null, text);
            else
                this.Toast(text);
            this.log.Info($"Milestone {milestone.Id} reached by {who.Name}.");
        }
    }

    // ── Multiplayer ─────────────────────────────────────────────────────────────

    private void Send<T>(T message, string type, long? to)
    {
        if (!Context.IsMultiplayer)
            return;
        this.helper.Multiplayer.SendMessage(message, type, modIDs: new[] { ModServices.Manifest.UniqueID }, playerIDs: to is long id ? new[] { id } : null);
    }

    private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
    {
        if (!Context.IsMainPlayer)
            return;
        IMultiplayerPeerMod? mod = e.Peer.GetMod(ModServices.Manifest.UniqueID);
        if (mod is null)
            return;
        if (!mod.Version.Equals(ModServices.Manifest.Version))
        {
            this.mismatchedPeers.Add(e.Peer.PlayerID);
            this.log.Warn($"A farmhand joined with ValleyOpus {mod.Version}; the host runs {ModServices.Manifest.Version}. Their discoveries won't be recorded until versions match (D12).");
        }
        this.Send(new CodexSnapshot { Codex = this.Codex.Data }, MessageTypes.CodexSnapshot, e.Peer.PlayerID);
    }

    private void OnMessage(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != ModServices.Manifest.UniqueID)
            return;
        try
        {
            switch (e.Type)
            {
                case MessageTypes.CodexRequest when Context.IsMainPlayer:
                    if (this.mismatchedPeers.Contains(e.FromPlayerID))
                        return;
                    CodexDelta delta = CodexSync.ApplyRequest(this.Codex, e.ReadAs<CodexRequest>(), e.FromPlayerID);
                    if (delta.Keys.Count > 0 || delta.Counts.Count > 0)
                        this.Send(delta, MessageTypes.CodexDelta, null);
                    this.NoticeOthers(delta, e.FromPlayerID);
                    break;

                case MessageTypes.CodexDelta when !Context.IsMainPlayer:
                    CodexDelta received = e.ReadAs<CodexDelta>();
                    CodexSync.ApplyDelta(this.Codex, received);
                    this.NoticeOthers(received, received.Keys.Values.FirstOrDefault()?.Player ?? 0);
                    break;

                case MessageTypes.CodexSnapshot when !Context.IsMainPlayer:
                    this.Codex = NewCodex(e.ReadAs<CodexSnapshot>().Codex);
                    this.Codex.ApplySecretPicks(ModServices.Content.Registry, this.GameId);
                    break;

                case MessageTypes.VersionHello when !Context.IsMainPlayer:
                    VersionHello hello = e.ReadAs<VersionHello>();
                    if (hello.ModVersion != ModServices.Manifest.Version.ToString())
                        Game1.addHUDMessage(new HUDMessage(ModServices.T("notice.version-mismatch", new { host = hello.ModVersion }), HUDMessage.error_type));
                    break;

                case MessageTypes.VersionHello when Context.IsMainPlayer:
                    this.Send(new VersionHello { ModVersion = ModServices.Manifest.Version.ToString(), Schema = FarmEnvelope.CurrentSchemaVersion }, MessageTypes.VersionHello, e.FromPlayerID);
                    break;
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"Couldn't read a {e.Type} message from player {e.FromPlayerID}: {ex}");
        }
    }

    /// <summary>A quieter notice for players other than the discoverer (doc 11 multiplayer).</summary>
    private void NoticeOthers(CodexDelta delta, long discoverer)
    {
        if (discoverer == Game1.player.UniqueMultiplayerID)
            return;
        int recipes = delta.Keys.Keys.Count(k => k.StartsWith(CodexKeys.RecipePrefix, StringComparison.Ordinal));
        if (recipes > 0)
            Game1.addHUDMessage(new HUDMessage(ModServices.T("notice.shared-discovery", new { name = Game1.getFarmer(discoverer)?.Name ?? "?", count = recipes })) { noIcon = true });
    }

    /// <summary>Merges toasts that arrive within two seconds (120 ticks); a toast shows its item's icon (doc 16 §8).</summary>
    private sealed class ToastQueue
    {
        private readonly List<(string Text, Item? Icon)> pending = new();
        private int firstTick;

        public void Add(string text, Item? icon)
        {
            if (this.pending.Count == 0)
                this.firstTick = Game1.ticks;
            this.pending.Add((text, icon));
        }

        public void Tick()
        {
            if (this.pending.Count == 0 || Game1.ticks - this.firstTick < 120)
                return;
            string text = this.pending.Count == 1
                ? this.pending[0].Text
                : ModServices.T("toast.merged", new { count = this.pending.Count, list = string.Join(", ", this.pending.Take(3).Select(p => p.Text)) });
            Item? icon = this.pending.Select(p => p.Icon).FirstOrDefault(i => i != null);
            Game1.addHUDMessage(icon != null
                ? new HUDMessage(text) { messageSubject = icon.getOne() }
                : new HUDMessage(text, HUDMessage.newQuest_type));
            this.pending.Clear();
        }
    }
}
