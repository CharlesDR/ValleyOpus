using System.Text.Json.Nodes;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Packs;
using ValleyOpus.Engine.Registry;
using static ValleyOpus.Tests.TestData;

namespace ValleyOpus.Tests;

/// <summary>The registry loader (format upgrades, never crash) and the content-pack reader (doc 09 §4, doc 17 §5f).</summary>
public class RegistryAndPackTests
{
    [Fact]
    public void Loader_FillsIdsFromKeysAndKeepsPackOrder()
    {
        ContentRegistry registry = RegistryLoader.Load(Raw(
            (RegistryNames.Primes, "B", new { SortOrder = 2 }),
            (RegistryNames.Primes, "A", new { SortOrder = 1 })));
        Assert.Equal(new[] { "B", "A" }, registry.Primes.All.Select(p => p.Id));
        Assert.True(registry.Primes.RankOf("B") < registry.Primes.RankOf("A"));
    }

    [Fact]
    public void Loader_UpgradesOlderFormatsInMemory()
    {
        var formats = new RowFormats(
            new Dictionary<string, int> { [RegistryNames.Primes] = 2 },
            new[] { new RowUpgradeStep(RegistryNames.Primes, 1, row => { row["SortOrder"] = row["Order"]!.GetValue<int>() * 10; row.Remove("Order"); return row; }) });
        ContentRegistry registry = RegistryLoader.Load(Raw(
            (RegistryNames.Primes, "Old", new { Order = 3 }),
            (RegistryNames.Primes, "New", new { FormatVersion = 2, SortOrder = 7 }),
            (RegistryNames.Primes, "Future", new { FormatVersion = 3 })), formats);

        Assert.Equal(30, registry.Primes.Get("Old")!.SortOrder);
        Assert.Equal(2, registry.Primes.Get("Old")!.FormatVersion);
        Assert.Equal(7, registry.Primes.Get("New")!.SortOrder);
        Assert.False(registry.Primes.Contains("Future"));
        Assert.Contains(registry.Findings, f => f.RowId == "Future" && f.Severity == Severity.Error);
    }

    [Fact]
    public void Loader_DisablesUnreadableRowsAndSkipsDeletedOnes()
    {
        var raw = new RawRegistries();
        raw.Set(RegistryNames.Primes, "Bad", new JsonObject { ["SortOrder"] = "not a number" });
        raw.Set(RegistryNames.Primes, "Good", Json(new { SortOrder = 1 }));
        raw.Set(RegistryNames.Primes, "Gone", Json(new { SortOrder = 2 }));
        raw.Set(RegistryNames.Primes, "Gone", null); // a later pack deleted it
        ContentRegistry registry = RegistryLoader.Load(raw);
        Assert.Equal(new[] { "Good" }, registry.Primes.All.Select(p => p.Id));
        Assert.Single(registry.Findings);
    }

    [Fact]
    public void RawRegistries_LaterEditReplacesInPlace()
    {
        var raw = new RawRegistries();
        raw.Set("Recipes", "A", Json(new { V = 1 }));
        raw.Set("Recipes", "B", Json(new { V = 2 }));
        raw.Set("Recipes", "A", Json(new { V = 3 }));
        Assert.Equal(new[] { "A", "B" }, raw.Rows("Recipes").Select(r => r.Key));
        Assert.Equal(3, raw.Rows("Recipes")[0].Value!["V"]!.GetValue<int>());
        Assert.True(RawRegistries.TryGetRegistryName("CharlesDR.ValleyOpus/Decomposition/TagWeights", out string name));
        Assert.Equal(RegistryNames.TagWeights, name);
    }

    [Fact]
    public void PackReader_ResolvesModIdAndI18n()
    {
        PackReadResult read = ContentPackReader.Read(FixturePack);
        var prime = read.Registries.Rows(RegistryNames.Primes).First();
        Assert.Equal(F + "Ignis", prime.Key);
        Assert.Equal("Fixture Ignis", prime.Value!["NameKey"]!.GetValue<string>());
        Assert.NotNull(read.Items.Get("(O)" + F + "Ignis"));
    }

    [Fact]
    public void PackReader_ReportsDisallowedTokensAndReadsHasMod()
    {
        string dir = Path.Combine(Path.GetTempPath(), "vo-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "manifest.json"), "{ \"UniqueID\": \"Test.Pack\" }");
            File.WriteAllText(Path.Combine(dir, "content.json"), """
            {
              "Format": "2.4.0",
              "Changes": [
                { "Action": "EditData", "Target": "CharlesDR.ValleyOpus/Primes", "Entries": { "{{ModId}}_A": { "Color": "{{Season}}" } } },
                { "Action": "EditData", "Target": "CharlesDR.ValleyOpus/Primes", "When": { "HasMod": "FlashShifter.StardewValleyExpandedCP" }, "Entries": { "SveOnly": {} } }
              ]
            }
            """);
            PackReadResult without = ContentPackReader.Read(new[] { dir });
            Assert.Contains(without.Findings, f => f.Message.Contains("Season"));
            Assert.Equal(new[] { "Test.Pack_A" }, without.Registries.Rows(RegistryNames.Primes).Select(r => r.Key));

            PackReadResult with = ContentPackReader.Read(new[] { dir }, new HashSet<string> { "FlashShifter.StardewValleyExpandedCP" });
            Assert.Contains("SveOnly", with.Registries.Rows(RegistryNames.Primes).Select(r => r.Key));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void PackReader_WarnsAboutFieldsOnlyWhereItReadsRows()
    {
        string dir = Path.Combine(Path.GetTempPath(), "vo-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "manifest.json"), "{ \"UniqueID\": \"Test.Pack\" }");
            File.WriteAllText(Path.Combine(dir, "content.json"), """
            {
              "Format": "2.4.0",
              "Changes": [
                { "Action": "EditData", "Target": "Data/Buffs", "Fields": { "X": { "IconSpriteIndex": 3 } } },
                { "Action": "EditData", "Target": "CharlesDR.ValleyOpus/Primes", "Fields": { "Y": { "Color": "#FFFFFF" } } }
              ]
            }
            """);
            PackReadResult read = ContentPackReader.Read(new[] { dir });
            Finding only = Assert.Single(read.Findings);
            Assert.Equal("content.json change #1", only.RowId);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Ids_QualifyAndPrefix()
    {
        Assert.Equal("(O)74", Ids.QualifyItem("74"));
        Assert.Equal("(BC)12", Ids.QualifyItem("(BC)12"));
        Assert.Equal("CharlesDR.ValleyOpus/Recipes", RegistryNames.AssetName(RegistryNames.Recipes));
    }
}
