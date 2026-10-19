using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using ValleyOpus.Engine;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Framework;

/// <summary>
/// Provides our registries as empty assets that Content Patcher packs fill (doc 09 §4a), and reads them back
/// into the engine with a catalog of every game item (doc 09 §4b). The full adapter <c>Content</c> and
/// <c>Catalog</c> modules grow from this in batch B2.
/// </summary>
internal sealed class GameContent
{
    private readonly IModHelper helper;
    private readonly SmapiLog log;

    public GameContent(IModHelper helper, SmapiLog log)
    {
        this.helper = helper;
        this.log = log;
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
    }

    /// <summary>Our registries load at exclusive priority as empty dictionaries; every pack, ours included, edits them.</summary>
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (RawRegistries.TryGetRegistryName(e.NameWithoutLocale.BaseName, out _) && e.NameWithoutLocale.StartsWith(Ids.KeyPrefix))
            e.LoadFrom(() => new Dictionary<string, JObject>(), AssetLoadPriority.Exclusive);
    }

    /// <summary>Reads every registry, validates it against the game's items and returns the pipeline.</summary>
    public ContentPipeline Load()
    {
        var raw = new RawRegistries();
        foreach (string registry in RegistryNames.All)
        {
            var rows = Game1.content.Load<Dictionary<string, JObject>>(RegistryNames.AssetName(registry));
            foreach ((string key, JObject? row) in rows)
                raw.Set(registry, key, row is null ? null : JsonNode.Parse(row.ToString()) as JsonObject);
        }

        ContentPipeline pipeline = ContentPipeline.Build(raw, BuildCatalog(), catalogIsComplete: true);
        foreach (Finding finding in pipeline.Findings)
        {
            if (finding.Severity == Severity.Error)
                this.log.Warn(finding.ToString());
            else
                this.log.Trace(finding.ToString());
        }
        return pipeline;
    }

    /// <summary>Snapshots every object and big craftable: qualified ID, price, category and context tags.</summary>
    public static ItemCatalog BuildCatalog()
    {
        var catalog = new ItemCatalog();
        foreach (string id in Game1.objectData.Keys.Select(k => "(O)" + k).Concat(Game1.bigCraftableData.Keys.Select(k => "(BC)" + k)))
        {
            if (ItemRegistry.Create(id, allowNull: true) is not StardewValley.Object item)
                continue;
            catalog.Add(new ItemInfo(item.QualifiedItemId, item.Price, item.Category, new HashSet<string>(item.GetContextTags(), StringComparer.OrdinalIgnoreCase)));
        }
        return catalog;
    }
}
