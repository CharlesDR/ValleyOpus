using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Quality;
using ValleyOpus.Engine.Randomness;
using ValleyOpus.Engine.Resolver;

namespace ValleyOpus.Tests;

/// <summary>Small rules: input matching, neighbour states, conditions, quality and the stable hash.</summary>
public class RuleTests
{
    private static CompiledSpec Item(string id, int count = 1) => new(new IngredientSpec { Item = id, Count = count }, Ids.QualifyItem(id));
    private static CompiledSpec Tag(string tag, int count = 1) => new(new IngredientSpec { Tag = tag, Count = count }, null);

    [Fact]
    public void InputMatcher_TagAndItemSpecsCompeteForTheSameItem()
    {
        ItemInfo tuna = ItemInfo.Create("Tuna", 100, -4, "category_fish");
        ItemInfo sardine = ItemInfo.Create("Sardine", 40, -4, "category_fish");
        // Tuna can fill either slot; the matcher must give Tuna to the Tuna slot and Sardine to the fish slot.
        var specs = new[] { Tag("category_fish"), Item("Tuna") };
        Assert.True(InputMatcher.MatchesExactly(specs, new[] { new ItemStack(tuna), new ItemStack(sardine) }));
        Assert.False(InputMatcher.MatchesExactly(specs, new[] { new ItemStack(sardine), new ItemStack(sardine) }));
        Assert.True(InputMatcher.CanStillComplete(specs, new[] { new ItemStack(sardine) }));
        Assert.False(InputMatcher.CanStillComplete(specs, new[] { new ItemStack(sardine, Count: 2) }));
    }

    [Fact]
    public void InputMatcher_RespectsMinQuality()
    {
        ItemInfo tuna = ItemInfo.Create("Tuna", 100, -4);
        var specs = new[] { new CompiledSpec(new IngredientSpec { Item = "Tuna", MinQuality = 2 }, "(O)Tuna") };
        Assert.False(InputMatcher.MatchesExactly(specs, new[] { new ItemStack(tuna, Quality: 1) }));
        Assert.True(InputMatcher.MatchesExactly(specs, new[] { new ItemStack(tuna, Quality: 4) }));
    }

    private static ApparatusRow Vat() => new()
    {
        Id = "Vat",
        NeighbourStates = new()
        {
            new() { Id = "NoPower", Test = new() { None = true } },
            new() { Id = "Vapor", Test = new() { Power = new() { "Vapor" } } },
            new() { Id = "Fulgur", Test = new() { Power = new() { "Fulgur" } } },
            new() { Id = "Crossed", Test = new() { Power = new() { "Vapor", "Fulgur" } } },
        },
    };

    [Fact]
    public void NeighbourStates_ResolveExactlyOne()
    {
        ApparatusRow vat = Vat();
        Assert.Equal("NoPower", NeighbourStates.Resolve(vat, new HashSet<string>(), false));
        Assert.Equal("Vapor", NeighbourStates.Resolve(vat, new HashSet<string> { "Vapor" }, true));
        Assert.Equal("Crossed", NeighbourStates.Resolve(vat, new HashSet<string> { "Fulgur", "Vapor" }, false));
        Assert.Empty(NeighbourStates.Overlaps(vat));
    }

    [Fact]
    public void NeighbourStates_FindOverlaps()
    {
        ApparatusRow bad = Vat();
        bad.NeighbourStates.Add(new() { Id = "Hot", Test = new() { Heat = true } });
        Assert.Contains(NeighbourStates.Overlaps(bad), p => p.B == "Hot");
    }

    [Fact]
    public void Conditions_AdjacencyRangeAndDirections()
    {
        var all = new ConditionRow { Kind = ConditionKind.Adjacency, Adjacency = new() { Tag = "flower_item", Directions = "All", Range = 2 } };
        var cardinal = new ConditionRow { Kind = ConditionKind.Adjacency, Adjacency = new() { Tag = "flower_item", Directions = "Cardinal", Range = 1 } };
        var diagonal = new ConditionSnapshot { Neighbours = new[] { NeighbourFact.Of(1, 1, "flower_item") } };
        var far = new ConditionSnapshot { Neighbours = new[] { NeighbourFact.Of(3, 0, "flower_item") } };
        Assert.True(ConditionEvaluator.Holds(all, diagonal));
        Assert.False(ConditionEvaluator.Holds(cardinal, diagonal));
        Assert.False(ConditionEvaluator.Holds(all, far));
    }

    [Fact]
    public void Conditions_GsqComesFromTheSnapshotOnly()
    {
        var monday = new ConditionRow { Kind = ConditionKind.Gsq, Query = "DAY_OF_WEEK Monday" };
        Assert.True(ConditionEvaluator.Holds(monday, new ConditionSnapshot { Queries = new Dictionary<string, bool> { ["DAY_OF_WEEK Monday"] = true } }));
        Assert.False(ConditionEvaluator.Holds(monday, ConditionSnapshot.Empty));
        Assert.False(monday.IsPlayerControlled);
    }

    [Fact]
    public void Conditions_CatalystAndPower()
    {
        var catalyst = new ConditionRow { Kind = ConditionKind.Catalyst, Catalyst = new() { Item = "SalFixum", Count = 2 } };
        Assert.True(ConditionEvaluator.Holds(catalyst, new ConditionSnapshot { Catalysts = new Dictionary<string, int> { ["(O)SalFixum"] = 2 } }));
        Assert.False(ConditionEvaluator.Holds(catalyst, new ConditionSnapshot { Catalysts = new Dictionary<string, int> { ["(O)SalFixum"] = 1 } }));
        var power = new ConditionRow { Kind = ConditionKind.Power, Power = "Vapor" };
        Assert.True(ConditionEvaluator.Holds(power, new ConditionSnapshot { ChargedPower = new HashSet<string> { "Vapor" } }));
    }

    private static ApparatusRow Kind(string kind, int? cap = null) => new() { Id = "A", Kind = kind, QualityCap = cap };

    [Theory]
    [InlineData(QualityMode.InheritMin, 0, new[] { 2, 4 }, 2)]
    [InlineData(QualityMode.InheritAverage, 0, new[] { 1, 2 }, 1)]   // index average 1.5 rounds down to silver
    [InlineData(QualityMode.Fixed, 2, new[] { 0 }, 2)]
    [InlineData(QualityMode.Advance, 1, new[] { 2 }, 4)]             // gold + 1 step = iridium: no stage 3
    public void Quality_Modes(QualityMode mode, int steps, int[] inputs, int expected)
        => Assert.Equal(expected, QualityCalculator.Compute(new QualityRule { Mode = mode, Steps = steps }, inputs, Kind(ApparatusKinds.RitualAltar)));

    [Fact]
    public void Quality_ModifiersAddThenOneClamp()
    {
        var rule = new QualityRule { Mode = QualityMode.InheritMin };
        Assert.Equal(1, QualityCalculator.Compute(rule, new[] { 0 }, Kind(ApparatusKinds.Engineered), new[] { 1, 1, 1 }));       // engineered cap: silver
        Assert.Equal(2, QualityCalculator.Compute(rule, new[] { 0 }, Kind(ApparatusKinds.Engineered), new[] { 3 }, capRaise: 1)); // a perk raises it to gold
        Assert.Equal(2, QualityCalculator.Compute(rule, new[] { 4 }, Kind(ApparatusKinds.MinorArcane)));
        Assert.Equal(0, QualityCalculator.Compute(rule, new[] { 0 }, Kind(ApparatusKinds.MinorArcane), new[] { 2, -2 }));
        Assert.Equal(1, QualityCalculator.Compute(new QualityRule { Mode = QualityMode.InheritMin, Floor = 1 }, new[] { 0 }, Kind(ApparatusKinds.MinorArcane), new[] { -3 }));
        Assert.Equal(1, QualityCalculator.Compute(new QualityRule { Mode = QualityMode.Fixed, Steps = 4, Cap = 1 }, Array.Empty<int>(), Kind(ApparatusKinds.RitualAltar)));
        Assert.Equal(0, QualityCalculator.Compute(new QualityRule { Mode = QualityMode.Fixed, Steps = 4 }, Array.Empty<int>(), Kind(ApparatusKinds.Table)));
        Assert.Equal(4, QualityCalculator.Compute(rule, new[] { 4 }, Kind(ApparatusKinds.Engineered, cap: 4), new[] { 5 }, capRaise: 3)); // never past iridium
    }

    [Fact]
    public void StableHash_IsFixedAcrossRuns()
    {
        // FNV-1a 64 of "a" is a published test vector.
        Assert.Equal(0xaf63dc4c8601ec8cUL, StableHash.Of("a"));
        Assert.NotEqual(StableHash.Of("ab", "c"), StableHash.Of("a", "bc"));
        Assert.Equal(new SeededRandom(42).NextULong(), new SeededRandom(42).NextULong());
        Assert.InRange(new SeededRandom(7).NextDouble(), 0, 1);
    }
}
