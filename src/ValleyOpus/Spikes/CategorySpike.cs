using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace ValleyOpus.Spikes;

/// <summary>
/// S16: which categories trigger which selling professions (Artisan for sellables, Blacksmith for bars,
/// Gemologist for gems), and whether vanilla dyeing reads our colour tags (doc 15, doc 09 §14).
/// </summary>
internal sealed class CategorySpike : ISpike
{
    private const string Prefix = "(O)CharlesDR.ValleyOpus_Spike_S16_";

    // Vanilla profession IDs: Artisan 4, Blacksmith 20, Gemologist 23.
    private static readonly (string Item, int Profession, string ProfessionName, double Expected)[] Cases =
    {
        ("Artisan", 4, "Artisan", 1.4),
        ("Bar", 20, "Blacksmith", 1.5),
        ("Gem", 23, "Gemologist", 1.3),
        ("Mineral", 23, "Gemologist", 1.3),
        ("Basic", 4, "Artisan", 1.0),
        ("Monster", 4, "Artisan", 1.0),
    };

    public string Id => "S16";

    public string Summary => "Categories and the selling professions they trigger; dye tags";

    public void Run(SpikeLog log, string[] args)
    {
        Farmer player = Game1.player;
        foreach ((string name, int profession, string professionName, double expected) in Cases)
        {
            if (ItemRegistry.Create(Prefix + name, allowNull: true) is not SObject item)
            {
                log.Fail($"{name}: placeholder missing; is the spike pack installed?");
                continue;
            }

            bool had = player.professions.Contains(profession);
            int without, with;
            try
            {
                if (had)
                    player.professions.Remove(profession);
                without = item.sellToStorePrice(player.UniqueMultiplayerID);
                player.professions.Add(profession);
                with = item.sellToStorePrice(player.UniqueMultiplayerID);
            }
            finally
            {
                player.professions.Remove(profession);
                if (had)
                    player.professions.Add(profession);
            }

            double ratio = without == 0 ? 0 : (double)with / without;
            string line = $"category {item.Category} ('{item.getCategoryName()}'): {without}g, {with}g with {professionName} (x{ratio:0.00}, expected x{expected:0.00})";
            if (Math.Abs(ratio - expected) < 0.05)
                log.Pass(line);
            else
                log.Fail(line);
        }

        if (ItemRegistry.Create(Prefix + "Dye", allowNull: true) is SObject dye)
        {
            var color = TailoringMenu.GetDyeColor(dye);
            log.Note($"dye item tags: {string.Join(", ", dye.GetContextTags().Where(t => t.StartsWith("color_") || t.StartsWith("dye_")))}");
            if (color.HasValue)
                log.Pass($"the sewing machine's dye check reads color_blue as {color.Value}");
            else
                log.Fail("the sewing machine's dye check found no colour on an item tagged color_blue");
            log.Look("put the Spike Dye item in one of Emily's dye pots: does it count as a blue dye?");
        }
    }
}
