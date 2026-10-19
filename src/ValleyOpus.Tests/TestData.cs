using System.Text.Json;
using System.Text.Json.Nodes;
using ValleyOpus.Engine;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Packs;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Tests;

/// <summary>Paths and loaders shared by the tests.</summary>
internal static class TestData
{
    public const string FixtureModId = "CharlesDR.ValleyOpus.TestFixtures";
    public const string F = FixtureModId + "_";

    public static string RepoRoot { get; } = FindRepoRoot();

    public static string ContentFolder => Path.Combine(RepoRoot, "content");

    public static string FixturePack => Path.Combine(ContentFolder, "[CP] ValleyOpus - Test Fixtures");

    public static string BasePack => Path.Combine(ContentFolder, "[CP] ValleyOpus");

    /// <summary>A few vanilla-shaped items the fixture rows don't author, for tag-overlap and decomposition checks.</summary>
    public static IEnumerable<ItemInfo> VanillaLike => new[]
    {
        ItemInfo.Create("(O)130", 100, -4, "category_fish", "fish_ocean"),          // Tuna
        ItemInfo.Create("(O)131", 40, -4, "category_fish", "fish_ocean"),           // Sardine
        ItemInfo.Create("(O)74", 2000, -2, "fx_aether_source", "fx_glitter"),       // Prismatic Shard
        ItemInfo.Create("(O)390", 2, -16),                                          // Stone
    };

    /// <summary>The fixture pack, loaded, validated and indexed, with its authored items plus <see cref="VanillaLike"/>.</summary>
    public static (ContentPipeline Pipeline, PackReadResult Read, ItemCatalog Catalog) LoadFixtures()
    {
        PackReadResult read = ContentPackReader.Read(FixturePack);
        var catalog = new ItemCatalog(read.Items.All.Concat(VanillaLike));
        ContentPipeline pipeline = ContentPipeline.Build(read.Registries, catalog, catalogIsComplete: false);
        return (pipeline, read, catalog);
    }

    public static ItemInfo Item(ItemCatalog catalog, string name)
        => catalog.Get("(O)" + F + name) ?? throw new InvalidOperationException("no fixture item " + name);

    /// <summary>Raw registries from anonymous objects, for hand-built cases.</summary>
    public static RawRegistries Raw(params (string Registry, string Key, object Row)[] rows)
    {
        var raw = new RawRegistries();
        foreach ((string registry, string key, object row) in rows)
            raw.Set(registry, key, JsonSerializer.SerializeToNode(row)!.AsObject());
        return raw;
    }

    public static JsonObject Json(object value) => JsonSerializer.SerializeToNode(value)!.AsObject();

    private static string FindRepoRoot()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "ValleyOpus.sln")))
                return dir.FullName;
        throw new InvalidOperationException("can't find the repository root from " + AppContext.BaseDirectory);
    }
}
