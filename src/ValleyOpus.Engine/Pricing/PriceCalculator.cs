using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Pricing;

/// <summary>
/// Derived prices (doc 15 §6): <c>Price = round(Markup × Σ input prices + Flat)</c> from the reference recipe, else
/// the cheapest route; a fixed price where a product row pins one. Recomputed whenever the registries reload, and
/// written into <c>Data/Objects</c> by the data compiler (doc 09 §4c).
/// </summary>
public sealed class PriceCalculator
{
    private readonly ContentRegistry registry;
    private readonly IItemCatalog catalog;
    private readonly Dictionary<string, int> prices = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> visiting = new(StringComparer.OrdinalIgnoreCase);

    public PriceCalculator(ContentRegistry registry, IItemCatalog catalog)
    {
        this.registry = registry;
        this.catalog = catalog;
    }

    /// <summary>The markup band for a tier when a row sets none (placeholders tuned in the sweep).</summary>
    public static double DefaultMarkup(int tier) => tier switch
    {
        <= 1 => 1.2,
        2 => 1.4,
        _ => 1.0,
    };

    /// <summary>The price of every Item-class substance and every product, by qualified item ID.</summary>
    public IReadOnlyDictionary<string, int> ComputeAll()
    {
        foreach (SubstanceRow substance in this.registry.Substances.All)
            if (substance.ItemId != null)
                this.PriceOf(Ids.QualifyItem(substance.ItemId));
        foreach (ProductRow product in this.registry.Products.All)
        {
            string? item = this.registry.ResolveItemId(product.Id);
            if (item != null)
                this.PriceOf(item);
        }
        return this.prices;
    }

    /// <summary>The price of one item: derived or fixed for our items, the catalog's for anything else.</summary>
    public int PriceOf(string qualifiedItemId)
    {
        if (this.prices.TryGetValue(qualifiedItemId, out int known))
            return known;

        SubstanceRow? substance = this.registry.SubstanceForItem(qualifiedItemId);
        ProductRow? product = this.registry.ProductForItem(qualifiedItemId);
        if (substance is null && product is null)
            return this.catalog.Get(qualifiedItemId)?.Price ?? 0;

        if (product?.Price.Mode == PriceMode.Fixed)
            return this.prices[qualifiedItemId] = Math.Max(0, product.Price.Value);

        if (!this.visiting.Add(qualifiedItemId))
            return this.catalog.Get(qualifiedItemId)?.Price ?? 0; // a cycle: fall back to the authored price
        try
        {
            string key = substance?.Id ?? qualifiedItemId;
            List<RecipeRow> routes = this.registry.Recipes.All
                .Where(r => r.Output.Id.Equals(key, StringComparison.OrdinalIgnoreCase) || (this.registry.ResolveItemId(r.Output.Id)?.Equals(qualifiedItemId, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            if (product?.Price.ReferenceRecipe is string reference)
                routes = routes.Where(r => r.Id == reference).ToList();
            if (routes.Count == 0)
                return this.prices[qualifiedItemId] = this.catalog.Get(qualifiedItemId)?.Price ?? 0;

            double markup = product?.Price.Markup ?? DefaultMarkup(substance?.Tier ?? 1);
            int flat = product?.Price.Flat ?? 0;
            int best = routes
                .Select(r => (int)Math.Round(markup * this.InputCost(r) / Math.Max(1, r.Output.Count) + flat, MidpointRounding.AwayFromZero))
                .Min();
            return this.prices[qualifiedItemId] = Math.Max(1, best);
        }
        finally
        {
            this.visiting.Remove(qualifiedItemId);
        }
    }

    /// <summary>Σ input prices of one recipe; a tag or category input costs its cheapest matching item.</summary>
    public double InputCost(RecipeRow recipe)
    {
        double total = 0;
        foreach (IngredientSpec spec in recipe.Inputs)
        {
            double each;
            if (spec.Item != null)
            {
                string? item = this.registry.ResolveItemId(spec.Item);
                each = item is null ? 0 : this.PriceOf(item);
            }
            else
            {
                each = this.catalog.All
                    .Where(i => spec.Tag != null ? i.HasTag(spec.Tag) : i.Category == spec.Category)
                    .Select(i => (double)i.Price)
                    .DefaultIfEmpty(0)
                    .Min();
            }
            total += each * spec.Count;
        }
        return total;
    }
}
