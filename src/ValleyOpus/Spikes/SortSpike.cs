using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using ValleyOpus.Engine.Randomness;

namespace ValleyOpus.Spikes;

/// <summary>
/// S11: what vanilla Organize keys on (doc 09 §11, doc 11 "Inventory sort order"). Four schemes of the same six
/// placeholder items; in each, only one field (category, item ID, internal name or display name) carries the
/// wanted order and the others run backwards, so the result names the field the sort uses.
/// </summary>
internal sealed class SortSpike : ISpike
{
    private const string Prefix = "CharlesDR.ValleyOpus_Spike_S11";

    /// <summary>The wanted order: Primes, then Tier 1, then Tier 2, then endgame Elements; alphabetical within groups.</summary>
    private static readonly string[] Expected = { "P a", "P b", "T1 a", "T1 b", "T2 a", "E a" };

    private static readonly string[] Schemes = { "cat", "id", "name", "disp" };

    public string Id => "S11";

    public string Summary => "Inventory sort: which field vanilla Organize keys on. Args: (none) | chest";

    public void Run(SpikeLog log, string[] args)
    {
        if (args.Length > 0 && args[0] == "chest")
        {
            this.PlaceChest(log);
            return;
        }

        log.Note($"language: {LocalizedContentManager.CurrentLanguageCode}");
        int passing = 0;
        foreach (string scheme in Schemes)
        {
            List<Item> items = Shuffled(ItemsFor(scheme), scheme);
            if (items.Count != Expected.Length)
            {
                log.Fail($"{scheme}: found {items.Count} of {Expected.Length} placeholder items; is the [CP] ValleyOpus - Spikes pack installed?");
                continue;
            }

            ItemGrabMenu.organizeItemsInList(items);
            string[] got = items.Select(Label).ToArray();
            string detail = string.Join(" | ", items.Select(i => $"{Label(i)} [id {i.ItemId.Substring(Prefix.Length)}, name {i.Name.Substring(Prefix.Length)}, shown '{i.DisplayName}', cat {i.Category}]"));
            if (got.SequenceEqual(Expected))
            {
                passing++;
                log.Pass($"{scheme}: Organize gives the wanted order when {Describe(scheme)} carries it. {detail}");
            }
            else if (got.SequenceEqual(Expected.Reverse()))
                log.Note($"{scheme}: Organize gives the exact reverse, so {Describe(scheme)} is the key but sorts descending. {detail}");
            else
                log.Note($"{scheme}: not this field. {detail}");
        }

        if (passing == 0)
            log.Fail("no single field gave the wanted order; see the lines above for what Organize did");
        log.Look("now run 'valleyopus_spike S11 chest', press each installed sort mod's sort button on that chest, and note the order");
    }

    private void PlaceChest(SpikeLog log)
    {
        GameLocation location = Game1.currentLocation;
        Vector2 tile = SpikeRunner.FacingTile();
        if (location.isObjectAtTile((int)tile.X, (int)tile.Y))
        {
            log.Error("the tile you face is occupied; face an empty tile");
            return;
        }

        var chest = new Chest(playerChest: true, tileLocation: tile);
        foreach (Item item in Schemes.SelectMany(s => Shuffled(ItemsFor(s), s)))
            chest.addItem(item);
        location.objects.Add(tile, chest);
        log.Look($"placed a chest at {tile} with all {chest.Items.Count} placeholder items, shuffled. Wanted order inside each scheme: {string.Join(", ", Expected)}");
    }

    private static List<Item> ItemsFor(string scheme)
        => Game1.objectData.Keys
            .Where(k => k.StartsWith($"{Prefix}{scheme}_", StringComparison.Ordinal))
            .Select(k => ItemRegistry.Create("(O)" + k))
            .ToList();

    /// <summary>A fixed shuffle, so every run starts from the same order.</summary>
    private static List<Item> Shuffled(List<Item> items, string seed)
        => items.OrderBy(i => StableHash.Of("S11", seed, i.ItemId)).ToList();

    /// <summary>The group label is the end of the display name in English ("Cat P a" -> "P a").</summary>
    private static string Label(Item item)
    {
        string[] words = item.DisplayName.Split(' ');
        return words.Length >= 2 ? words[^2] + " " + words[^1] : item.DisplayName;
    }

    private static string Describe(string scheme) => scheme switch
    {
        "cat" => "the category number",
        "id" => "the item ID",
        "name" => "the internal name",
        "disp" => "the display name",
        _ => scheme,
    };
}
