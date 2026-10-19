using ValleyOpus.Engine;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Packs;
using ValleyOpus.Engine.Registry;
using static ValleyOpus.Tests.TestData;

namespace ValleyOpus.Tests;

/// <summary>The validator (doc 12 §3, doc 13 §1a) and the data tests over the shipped packs (doc 11 §6).</summary>
public class ValidationTests
{
    [Fact]
    public void FixturePack_ReportsExactlyItsDeliberateErrors()
    {
        (ContentPipeline pipeline, PackReadResult read, _) = LoadFixtures();
        Assert.Empty(read.Findings);

        var expected = new[]
        {
            (Severity.Error, RegistryNames.Recipes, F + "ZStormVariant"),   // a weather Variant on an automatable recipe
            (Severity.Error, RegistryNames.Recipes, F + "ZCollide"),        // same inputs as Ignis + Terra
            (Severity.Error, RegistryNames.Recipes, F + "ZTagCollide"),     // fx_wet overlaps Mud
            (Severity.Warning, RegistryNames.TagWeights, "fx_glitter"),     // Aether off the source list
            (Severity.Warning, RegistryNames.Recipes, F + "Lumen+" + F + "Lumen"), // the pack-added Prime leaves one pairing undefined
        };
        Assert.Equal(
            expected.OrderBy(e => e.Item3).ToList(),
            pipeline.Findings.Select(f => (f.Severity, f.Registry, f.RowId)).OrderBy(e => e.RowId).ToList());

        Assert.False(pipeline.Registry.Recipes.Contains(F + "ZCollide"));
        Assert.True(pipeline.Registry.Recipes.Contains(F + "IgnisTerra")); // the earlier row keeps its place
        Assert.True(pipeline.Registry.Recipes.Contains(F + "StormGlass"));  // weather as a gate is fine
    }

    [Fact]
    public void ShippedPacks_HaveNoFindings()
    {
        string[] shipped = Directory.GetDirectories(ContentFolder)
            .Where(d => !Path.GetFileName(d).Contains("Test Fixtures") && !Path.GetFileName(d).Contains("Spikes"))
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToArray();
        Assert.NotEmpty(shipped);

        PackReadResult read = ContentPackReader.Read(shipped);
        Assert.Empty(read.Findings);
        ContentPipeline pipeline = ContentPipeline.Build(read.Registries, read.Items, catalogIsComplete: false);
        Assert.Empty(pipeline.Findings);
    }

    [Fact]
    public void EveryJsonFileInContentParses()
    {
        foreach (string file in Directory.GetFiles(ContentFolder, "*.json", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            var ex = Record.Exception(() => System.Text.Json.Nodes.JsonNode.Parse(text, documentOptions: new() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, AllowTrailingCommas = true }));
            Assert.True(ex is null, $"{file}: {ex?.Message}");
        }
    }

    [Fact]
    public void TagOverlap_UsesRealItems()
    {
        // category_fish and Tuna collide because Tuna is a fish (doc 13 §1a); fish_river and Tuna don't.
        (string, string, object)[] rows =
        {
            (RegistryNames.Operations, "Op", new { }),
            (RegistryNames.Apparatus, "Vat", new { Kind = "Engineered", NeighbourStates = new[] { new { Id = "S", Test = new { } } }, Operations = new[] { new { Operation = "Op", States = new[] { "S" } } } }),
            (RegistryNames.Recipes, "ByTuna", new { Inputs = new[] { new { Item = "(O)130" } }, Operation = "Op", ApparatusKinds = new[] { "Engineered" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)1" } }),
            (RegistryNames.Recipes, "ByFish", new { Inputs = new[] { new { Tag = "category_fish" } }, Operation = "Op", ApparatusKinds = new[] { "Engineered" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)2" } }),
            (RegistryNames.Recipes, "ByRiver", new { Inputs = new[] { new { Tag = "fish_river" } }, Operation = "Op", ApparatusKinds = new[] { "Engineered" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)3" } }),
        };
        var catalog = new ItemCatalog(VanillaLike.Append(ItemInfo.Create("(O)1", 1, 0)).Append(ItemInfo.Create("(O)2", 1, 0)).Append(ItemInfo.Create("(O)3", 1, 0)));
        ContentPipeline p = ContentPipeline.Build(Raw(rows), catalog, catalogIsComplete: true);
        Assert.Equal(new[] { "ByFish" }, p.Findings.Where(f => f.Severity == Severity.Error).Select(f => f.RowId));
    }

    [Fact]
    public void Apparatus_WithOverlappingStatesIsDisabled()
    {
        ContentPipeline p = ContentPipeline.Build(Raw(
            (RegistryNames.Operations, "Op", new { }),
            (RegistryNames.Apparatus, "Bad", new
            {
                Kind = "Engineered",
                NeighbourStates = new object[] { new { Id = "Any", Test = new { } }, new { Id = "Hot", Test = new { Heat = true } } },
                Operations = new[] { new { Operation = "Op", States = new[] { "Any" } } },
            })), new ItemCatalog(), catalogIsComplete: false);
        Assert.False(p.Registry.Apparatus.Contains("Bad"));
    }

    [Fact]
    public void Recipes_WithBrokenReferencesAreDisabledNeverThrown()
    {
        ContentPipeline p = ContentPipeline.Build(Raw(
            (RegistryNames.Operations, "Op", new { }),
            (RegistryNames.Recipes, "NoStates", new { Inputs = new[] { new { Item = "(O)1" } }, Operation = "Op", ApparatusKinds = new[] { "Engineered" }, Output = new { Id = "(O)1" } }),
            (RegistryNames.Recipes, "BadOp", new { Inputs = new[] { new { Item = "(O)1" } }, Operation = "Nope", ApparatusKinds = new[] { "Engineered" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)1" } }),
            (RegistryNames.Recipes, "BadCondition", new { Inputs = new[] { new { Item = "(O)1" } }, Operation = "Op", ApparatusKinds = new[] { "Engineered" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)1" }, Conditions = new[] { new { ConditionId = "Ghost" } } }),
            (RegistryNames.Recipes, "OurMissingItem", new { Inputs = new[] { new { Item = Ids.RowPrefix + "Nothing" } }, Operation = "Op", ApparatusKinds = new[] { "Engineered" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)1" } }),
            (RegistryNames.Recipes, "HandMadeOnVanilla", new { Inputs = new[] { new { Item = "(O)1" } }, Operation = "Op", ApparatusKinds = new[] { "Vanilla" }, NeighbourStates = new[] { "S" }, Output = new { Id = "(O)1" }, HandMadeOnly = true }),
            (RegistryNames.TagWeights, "bad_prime", new { Weights = new Dictionary<string, double> { ["Phlogiston"] = 1 } })),
            new ItemCatalog(), catalogIsComplete: false);

        Assert.Equal(0, p.Registry.Recipes.Count);
        Assert.Equal(5, p.Registry.Recipes.Disabled.Count);
        Assert.False(p.Registry.TagWeights.Contains("bad_prime"));
    }
}
