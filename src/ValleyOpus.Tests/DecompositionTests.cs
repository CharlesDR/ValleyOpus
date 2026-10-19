using ValleyOpus.Engine;
using ValleyOpus.Engine.Decomposition;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Randomness;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Tests;

/// <summary>Doc 03 "The computation" and doc 12 §2.</summary>
public class DecompositionTests
{
    private static (Decomposer Decomposer, ContentPipeline Pipeline) Build(object? settings = null, params (string, string, object)[] extra)
    {
        var rows = new List<(string, string, object)>
        {
            (RegistryNames.Primes, "Ignis", new { ItemId = "(O)P_Ignis", SortOrder = 10 }),
            (RegistryNames.Primes, "Aqua", new { ItemId = "(O)P_Aqua", SortOrder = 20 }),
            (RegistryNames.Primes, "Terra", new { ItemId = "(O)P_Terra", SortOrder = 30 }),
            (RegistryNames.Primes, "Sal", new { ItemId = "(O)P_Sal", SortOrder = 40 }),
            (RegistryNames.Primes, "Aether", new { ItemId = "(O)P_Aether", Group = "Rare", SortOrder = 80 }),
            (RegistryNames.TagWeights, "category_fish", new { Weights = new Dictionary<string, double> { ["Aqua"] = 3, ["Sal"] = 1 } }),
            (RegistryNames.TagWeights, "color_red", new { Weights = new Dictionary<string, double> { ["Ignis"] = 1 } }),
            (RegistryNames.TagWeights, "id_o_74", new { Weights = new Dictionary<string, double> { ["Aether"] = 4, ["Terra"] = 1 }, AetherSource = true }),
            (RegistryNames.TagWeights, "glitter", new { Weights = new Dictionary<string, double> { ["Aether"] = 9, ["Terra"] = 1 } }),
            (RegistryNames.CategoryDefaults, "-16", new { Weights = new Dictionary<string, double> { ["Terra"] = 1, ["Aether"] = 5 } }),
            (RegistryNames.CategoryDefaults, "Default", new { Weights = new Dictionary<string, double> { ["Terra"] = 1, ["Aqua"] = 1 } }),
            (RegistryNames.Settings, "Default", settings ?? new { K = 1.0, E = 0.5, QualityBonus = 0.1, Threshold = 0.1, ExchangeRatio = 0.8 }),
        };
        rows.AddRange(extra);
        var catalog = new ItemCatalog(new[]
        {
            ItemInfo.Create("(O)P_Ignis", 1, -16), ItemInfo.Create("(O)P_Aqua", 1, -16), ItemInfo.Create("(O)P_Terra", 1, -16),
            ItemInfo.Create("(O)P_Sal", 1, -16), ItemInfo.Create("(O)P_Aether", 1, -16),
        });
        ContentPipeline pipeline = ContentPipeline.Build(TestData.Raw(rows.ToArray()), catalog, catalogIsComplete: false);
        return (pipeline.Decomposer, pipeline);
    }

    [Fact]
    public void Profile_SumsTagWeightsAndNormalizes()
    {
        (Decomposer d, _) = Build();
        PrimeProfile profile = d.ProfileOf(ItemInfo.Create("(O)Tuna", 100, -4, "category_fish"));
        Assert.Equal(new[] { "Aqua", "Sal" }, profile.Weights.Select(w => w.Prime));
        Assert.Equal(0.75, profile.Weights[0].Weight, 6);
        Assert.Equal(0.25, profile.Weights[1].Weight, 6);
        Assert.Equal("tags", profile.Source);
    }

    [Fact]
    public void Profile_DropsPrimesUnderThresholdThenRenormalizes()
    {
        // 3 Aqua + 1 Sal + 0.3 Ignis: Ignis is 0.3/4.3 = 7% < 10%, so it's dropped.
        (Decomposer d, _) = Build(extra: (RegistryNames.TagWeights, "tiny_red", new { Weights = new Dictionary<string, double> { ["Ignis"] = 0.3 } }));
        PrimeProfile profile = d.ProfileOf(ItemInfo.Create("(O)RedFish", 100, -4, "category_fish", "tiny_red"));
        Assert.DoesNotContain(profile.Weights, w => w.Prime == "Ignis");
        Assert.Equal(1.0, profile.Weights.Sum(w => w.Weight), 6);
    }

    [Fact]
    public void Profile_FallsBackToCategoryThenGlobalDefault()
    {
        (Decomposer d, _) = Build();
        Assert.Equal("category default", d.ProfileOf(ItemInfo.Create("(O)Rock", 10, -16)).Source);
        PrimeProfile global = d.ProfileOf(ItemInfo.Create("(O)Odd", 10, -999));
        Assert.Equal("global default", global.Source);
        Assert.Equal(new[] { "Aqua", "Terra" }, global.Weights.Select(w => w.Prime)); // tie broken by Prime sort order
    }

    [Fact]
    public void Aether_OnlyFromSourceListRows()
    {
        (Decomposer d, ContentPipeline p) = Build();
        Assert.Contains(d.ProfileOf(ItemInfo.Create("(O)74", 2000, -2, "id_o_74")).Weights, w => w.Prime == "Aether");
        Assert.DoesNotContain(d.ProfileOf(ItemInfo.Create("(O)Shiny", 50, -2, "glitter")).Weights, w => w.Prime == "Aether");
        Assert.DoesNotContain(d.ProfileOf(ItemInfo.Create("(O)Rock", 10, -16)).Weights, w => w.Prime == "Aether");
        Assert.Contains(p.Findings, f => f.Registry == RegistryNames.TagWeights && f.RowId == "glitter");
        Assert.Contains(p.Findings, f => f.Registry == RegistryNames.CategoryDefaults && f.RowId == "-16");
    }

    [Theory]
    [InlineData(100, 0, 10)]   // floor(sqrt(100)) = 10
    [InlineData(100, 2, 12)]   // floor(10 x 1.2) = 12
    [InlineData(100, 4, 14)]   // iridium: floor(10 x 1.4)
    [InlineData(1, 0, 1)]      // never below 1 before the clamp
    [InlineData(0, 0, 1)]
    public void Units_SquareRootOfPriceWithQualityBonus(int price, int quality, int expected)
    {
        (Decomposer d, _) = Build(settings: new { K = 1.0, E = 0.5, QualityBonus = 0.1, Threshold = 0.1, ExchangeRatio = 1000.0 });
        DecompositionResult result = d.Decompose(ItemInfo.Create("(O)Tuna", price, -4, "category_fish"), quality, null);
        Assert.Equal(expected, result.UnclampedTotal);
    }

    [Fact]
    public void Split_LargestRemainderIsExactAndDeterministic()
    {
        (Decomposer d, _) = Build();
        var profile = new PrimeProfile(new[] { ("Ignis", 1 / 3.0), ("Aqua", 1 / 3.0), ("Terra", 1 / 3.0) }, "test");
        var split = d.Split(profile, 10);
        Assert.Equal(10, split.Sum(s => s.Units));
        // 3.33 each; the one spare unit goes to the lowest sort order on a tie.
        Assert.Equal(4, split.Single(s => s.Prime == "Ignis").Units);
        Assert.Equal(3, split.Single(s => s.Prime == "Aqua").Units);
        Assert.Equal(split, d.Split(profile, 10));
    }

    [Fact]
    public void EquivalentExchange_ClampsUnits()
    {
        // Primes are worth 1g each; ratio 0.8 of a 25g item allows 20g, so 5 units (sqrt 25) fit, but a ratio of 0.1 allows 2.
        (Decomposer d, _) = Build(settings: new { K = 1.0, E = 0.5, QualityBonus = 0.1, Threshold = 0.1, ExchangeRatio = 0.1 });
        DecompositionResult result = d.Decompose(ItemInfo.Create("(O)Tuna", 25, -4, "category_fish"), 0, null);
        Assert.True(result.Clamped);
        Assert.Equal(5, result.UnclampedTotal);
        Assert.Equal(2, result.Total);
    }

    [Fact]
    public void Concentration_KeepsTopPrimes()
    {
        (Decomposer d, _) = Build(extra: (RegistryNames.TagWeights, "mixed", new { Weights = new Dictionary<string, double> { ["Ignis"] = 5, ["Aqua"] = 3, ["Terra"] = 2 } }));
        DecompositionResult result = d.Decompose(ItemInfo.Create("(O)Mix", 100, -999, "mixed"), 0, new DecomposeModifiers { Concentration = 2 });
        Assert.Equal(new[] { "Ignis", "Aqua" }, result.Units.Select(u => u.Prime));
    }

    [Fact]
    public void Overrides_RefuseAndPin()
    {
        (Decomposer d, _) = Build(extra: new (string, string, object)[]
        {
            (RegistryNames.Overrides, "(O)Caput", new { Refuse = true }),
            (RegistryNames.Overrides, "(O)Calx", new { Profile = new Dictionary<string, double> { ["Sal"] = 1 }, Units = 3 }),
        });
        Assert.True(d.Decompose(ItemInfo.Create("(O)Caput", 10, -16), 0, null).Refused);
        DecompositionResult pinned = d.Decompose(ItemInfo.Create("(O)Calx", 100, -16), 0, null);
        Assert.Equal(new[] { ("Sal", 3) }, pinned.Units);
    }

    [Fact]
    public void Residue_UsesTheDeterministicSeed()
    {
        (Decomposer d, _) = Build();
        ItemInfo tuna = ItemInfo.Create("(O)Tuna", 100, -4, "category_fish");
        var rate = new DecomposeModifiers { ResidueRate = new ResidueAmount { Count = 2, Chance = 0.5 } };
        int[] first = Enumerable.Range(0, 20).Select(day => d.Decompose(tuna, 0, rate, SeededRandom.ForRoll(day, "Farm", 3, 4, "decompose")).ResidueCount).ToArray();
        int[] again = Enumerable.Range(0, 20).Select(day => d.Decompose(tuna, 0, rate, SeededRandom.ForRoll(day, "Farm", 3, 4, "decompose")).ResidueCount).ToArray();
        Assert.Equal(first, again);
        Assert.Contains(0, first);
        Assert.Contains(2, first);
    }
}
