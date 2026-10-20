using System.Diagnostics;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using ValleyOpus.Engine;
using ValleyOpus.Engine.Codex;
using Stager = ValleyOpus.Engine.Machines.Stager;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Pricing;
using ValleyOpus.Engine.Registry;
using ValleyOpus.Engine.Skill;
using SObject = StardewValley.Object;

namespace ValleyOpus.Framework;

/// <summary>
/// The <c>Content</c> and <c>Catalog</c> modules (doc 09 §3, §4): provides our registries as empty assets that
/// Content Patcher packs fill, reads them back into the engine with a catalog of every game item, and runs the
/// data compiler that adds computed fields to the vanilla rows our items need (§4c).
/// </summary>
internal sealed class GameContent
{
    /// <summary>The <c>OutputMethod</c> every apparatus rule names (doc 13 §6).</summary>
    public const string OutputMethodName = "ValleyOpus.Machines.MachineService, ValleyOpus: OutputMethod";

    /// <summary>Custom field on <c>Data/Buffs</c> rows naming a custom effect's handler (doc 15 §3).</summary>
    public const string HandlerField = Ids.KeyPrefix + "Handler";

    private static readonly string[] CompiledAssets = { "Data/Objects", "Data/Machines", "Data/Buffs" };

    private readonly IModHelper helper;
    private readonly SmapiLog log;
    private ContentPipeline? pipeline;

    public GameContent(IModHelper helper, SmapiLog log)
    {
        this.helper = helper;
        this.log = log;
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
    }

    /// <summary>Raised after the registries reload, so services can drop caches.</summary>
    public event Action? Reloaded;

    /// <summary>The current pipeline, loaded on first use after launch or invalidation.</summary>
    public ContentPipeline Pipeline => this.pipeline ??= this.Load();

    public ContentRegistry Registry => this.Pipeline.Registry;

    public Stager Stager { get; private set; } = null!;
    public XpCalculator Xp { get; private set; } = null!;
    public CodexGrid Grid { get; private set; } = null!;
    public IReadOnlyDictionary<string, int> Prices { get; private set; } = new Dictionary<string, int>();

    /// <summary>Our registries load at exclusive priority as empty dictionaries; every pack, ours included, edits them.</summary>
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (RawRegistries.TryGetRegistryName(e.NameWithoutLocale.BaseName, out _) && e.NameWithoutLocale.StartsWith(Ids.KeyPrefix))
        {
            e.LoadFrom(() => new Dictionary<string, JObject>(), AssetLoadPriority.Exclusive);
            return;
        }
        if (e.NameWithoutLocale.IsEquivalentTo(ModServices.StringsAsset))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
            return;
        }

        // The data compiler edits early, so other packs can still override what it writes (doc 09 §4c).
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            e.Edit(asset => this.CompileObjects(asset.AsDictionary<string, ObjectData>().Data), AssetEditPriority.Early);
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
            e.Edit(asset => this.CompileMachines(asset.AsDictionary<string, MachineData>().Data), AssetEditPriority.Early);
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Buffs"))
            e.Edit(asset => this.CompileBuffs(asset.AsDictionary<string, BuffData>().Data), AssetEditPriority.Early);
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (!e.NamesWithoutLocale.Any(n => n.StartsWith(Ids.KeyPrefix) && RawRegistries.TryGetRegistryName(n.BaseName, out _)))
            return;
        this.pipeline = null;
        foreach (string asset in CompiledAssets)
            this.helper.GameContent.InvalidateCache(asset);
        this.Reloaded?.Invoke();
    }

    /// <summary>Reads every registry, validates it against the game's items and returns the pipeline.</summary>
    public ContentPipeline Load()
    {
        var timer = Stopwatch.StartNew();
        RawRegistries raw = ReadRaw();
        ItemCatalog catalog = BuildCatalog();
        ContentPipeline loaded = ContentPipeline.Build(raw, catalog, catalogIsComplete: true);
        foreach (Finding finding in loaded.Findings)
        {
            if (finding.Severity == Severity.Error)
                this.log.Warn(finding.ToString());
            else
                this.log.Trace(finding.ToString());
        }

        this.pipeline = loaded;
        this.Stager = new Stager(loaded);
        this.Xp = new XpCalculator(loaded.Registry);
        this.Grid = new CodexGrid(loaded.Registry);
        this.Prices = new PriceCalculator(loaded.Registry, catalog).ComputeAll();
        this.log.Trace($"Loaded registries in {timer.ElapsedMilliseconds} ms: {loaded.Registry.Recipes.Count} recipes, {loaded.Registry.Apparatus.Count} apparatus, {loaded.Findings.Count} finding(s).");
        return loaded;
    }

    public static RawRegistries ReadRaw()
    {
        var raw = new RawRegistries();
        foreach (string registry in RegistryNames.All)
        {
            var rows = Game1.content.Load<Dictionary<string, JObject>>(RegistryNames.AssetName(registry));
            foreach ((string key, JObject? row) in rows)
                raw.Set(registry, key, row is null ? null : JsonNode.Parse(row.ToString()) as JsonObject);
        }
        return raw;
    }

    /// <summary>Snapshots every object and big craftable: qualified ID, price, category and context tags.</summary>
    public static ItemCatalog BuildCatalog()
    {
        var catalog = new ItemCatalog();
        foreach (string id in Game1.objectData.Keys.Select(k => "(O)" + k).Concat(Game1.bigCraftableData.Keys.Select(k => "(BC)" + k)))
        {
            if (ItemRegistry.Create(id, allowNull: true) is not SObject item)
                continue;
            catalog.Add(ToInfo(item));
        }
        return catalog;
    }

    /// <summary>One item instance as the engine sees it; built from the instance so flavoured items keep their price and tags.</summary>
    public static ItemInfo ToInfo(Item item)
        => new(item.QualifiedItemId, item is SObject o ? o.Price : item.salePrice(), item.Category, new HashSet<string>(item.GetContextTags(), StringComparer.OrdinalIgnoreCase));

    /// <summary>Derived prices and our tags, into our authored <c>Data/Objects</c> rows.</summary>
    private void CompileObjects(IDictionary<string, ObjectData> data)
    {
        try
        {
            // The game's catalog can't be read while Data/Objects is loading, so prices come from the asset itself.
            var catalog = new ItemCatalog(data.Select(p => new ItemInfo("(O)" + p.Key, p.Value.Price, p.Value.Category,
                new HashSet<string>(p.Value.ContextTags ?? new List<string>(), StringComparer.OrdinalIgnoreCase))));
            ContentRegistry registry = RegistryLoader.Load(ReadRaw());
            IReadOnlyDictionary<string, int> prices = new PriceCalculator(registry, catalog).ComputeAll();
            int priced = 0;
            foreach ((string qualified, int price) in prices)
            {
                string key = qualified.StartsWith("(O)") ? qualified[3..] : qualified;
                if (!data.TryGetValue(key, out ObjectData? row))
                    continue;
                row.Price = price;
                priced++;
            }
            this.log.Trace($"Data compiler: priced {priced} items.");
        }
        catch (Exception ex)
        {
            this.log.Error($"The data compiler couldn't price our items; they keep their authored prices. {ex}");
        }
    }

    /// <summary>One <c>ItemPlacedInMachine</c> rule per apparatus, calling our <c>OutputMethod</c> (doc 09 §4c, doc 13).</summary>
    private void CompileMachines(IDictionary<string, MachineData> data)
    {
        try
        {
            ContentRegistry registry = RegistryLoader.Load(ReadRaw());
            foreach (ApparatusRow apparatus in registry.Apparatus.All)
            {
                if (apparatus.ItemId is null || apparatus.Kind == ApparatusKinds.Table || data.ContainsKey(apparatus.ItemId))
                    continue;
                data[apparatus.ItemId] = new MachineData
                {
                    OutputRules = new List<MachineOutputRule>
                    {
                        new()
                        {
                            Id = Ids.RowPrefix + "Resolver",
                            Triggers = new List<MachineOutputTriggerRule> { new() { Id = "Placed", Trigger = MachineOutputTrigger.ItemPlacedInMachine } },
                            OutputItem = new List<MachineItemOutput> { new() { Id = "Resolver", OutputMethod = OutputMethodName } },
                            MinutesUntilReady = 120,
                        },
                    },
                    // ReadyTimeModifiers stay empty but open, so other mods' speed edits apply on top (S19).
                };
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"The data compiler couldn't write machine rules; our apparatus won't run. {ex}");
        }
    }

    /// <summary>One <c>Data/Buffs</c> row per effect; custom ones name their handler (doc 15 §3).</summary>
    private void CompileBuffs(IDictionary<string, BuffData> data)
    {
        try
        {
            ContentRegistry registry = RegistryLoader.Load(ReadRaw());
            foreach (EffectRow effect in registry.Effects.All)
            {
                if (data.ContainsKey(effect.Id))
                    continue;
                var row = new BuffData
                {
                    DisplayName = effect.NameKey != null ? ModServices.T(effect.NameKey) : effect.Id,
                    Description = effect.DescKey != null ? ModServices.T(effect.DescKey) : null,
                    Duration = effect.Duration * 1000 * 7 / 10, // game minutes to milliseconds (one game minute = 0.7 s)
                    IconTexture = "TileSheets\\BuffsIcons",
                    IconSpriteIndex = 7,
                    CustomFields = new Dictionary<string, string>(),
                };
                if (effect.Kind == EffectKind.Custom && effect.Handler != null)
                {
                    row.CustomFields[HandlerField] = effect.Handler;
                    foreach ((string key, string value) in effect.Params)
                        row.CustomFields[Ids.KeyPrefix + key] = value;
                }
                else if (effect.Stats.Count > 0)
                {
                    row.Effects = new BuffAttributesData();
                    foreach ((string stat, double value) in effect.Stats)
                        typeof(BuffAttributesData).GetField(stat)?.SetValue(row.Effects, (float)value);
                }
                data[effect.Id] = row;
            }
        }
        catch (Exception ex)
        {
            this.log.Error($"The data compiler couldn't write buff rows. {ex}");
        }
    }

    /// <summary>The item for an apparatus row's ID, or the apparatus row for a placed object.</summary>
    public ApparatusRow? ApparatusOf(SObject? obj)
    {
        ApparatusRow? row = obj is { bigCraftable.Value: true } ? this.Registry.ApparatusForItem(obj.QualifiedItemId) : null;
        // An altar standing on a Circle Sigil is a Ritual Circle (doc 13 §5): same machine, the circle's recipes.
        if (row?.Kind == ApparatusKinds.RitualAltar && obj!.Location is GameLocation location && OnCircleSigil(location, obj.TileLocation)
            && this.Registry.Apparatus.Get(CircleApparatusId) is ApparatusRow circle)
            return circle;
        return row;
    }

    public const string CircleApparatusId = Ids.RowPrefix + "RitualCircle";
    public const string CircleSigilId = "(F)" + Ids.RowPrefix + "CircleSigil";

    public static bool OnCircleSigil(GameLocation location, Microsoft.Xna.Framework.Vector2 tile)
        => location.furniture.Any(f => f.QualifiedItemId == CircleSigilId && f.GetBoundingBox().Contains((int)tile.X * 64 + 32, (int)tile.Y * 64 + 32));

    /// <summary>The display name of a substance, Prime or item ID; Power substances (never items) are named in our i18n.</summary>
    public string NameOf(string id)
    {
        string? item = this.Registry.ResolveItemId(id);
        if (item != null && ItemRegistry.GetData(item) is { } data)
            return data.DisplayName;
        string local = id.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) ? id[Ids.RowPrefix.Length..] : id;
        Translation name = ModServices.Helper.Translation.Get("substance." + local);
        return name.HasValue() ? name.ToString() : local;
    }
}
