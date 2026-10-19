using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Resolver;

/// <summary>A recipe with its specs resolved, ready to match.</summary>
public sealed record CompiledRecipe(RecipeRow Row, IReadOnlyList<CompiledSpec> Specs)
{
    public string Id => this.Row.Id;

    /// <summary>Whether this apparatus, in this neighbour state, offers the recipe (doc 13 data shape).</summary>
    public bool RunsOn(ApparatusRow apparatus, string? state)
    {
        if (state is null || !this.Row.NeighbourStates.Contains(state))
            return false;
        if (!this.Row.ApparatusKinds.Contains(apparatus.Kind) && !this.Row.ApparatusIds.Contains(apparatus.Id))
            return false;
        return apparatus.Operations.Any(o => o.Operation == this.Row.Operation && o.States.Contains(state));
    }
}

/// <summary>
/// The runnable-recipe index: recipes keyed by the item IDs, tags and categories their inputs name, so a staging
/// call is one lookup (doc 13 §6.4). Rebuilt on registry reloads; discovery is checked at lookup, not baked in.
/// </summary>
public sealed class RecipeIndex
{
    private readonly Dictionary<string, List<CompiledRecipe>> byItem = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<CompiledRecipe>> byTag = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, List<CompiledRecipe>> byCategory = new();
    private readonly Dictionary<string, CompiledRecipe> byId = new(StringComparer.Ordinal);

    public RecipeIndex(ContentRegistry registry)
    {
        foreach (RecipeRow row in registry.Recipes.All)
        {
            var specs = row.Inputs.Select(s => new CompiledSpec(s, s.Item is null ? null : registry.ResolveItemId(s.Item))).ToList();
            var recipe = new CompiledRecipe(row, specs);
            this.byId[row.Id] = recipe;
            foreach (CompiledSpec spec in specs)
            {
                if (spec.ItemId != null)
                    AddTo(this.byItem, spec.ItemId, recipe);
                else if (spec.Spec.Tag != null)
                    AddTo(this.byTag, spec.Spec.Tag, recipe);
                else if (spec.Spec.Category is int category)
                    AddTo(this.byCategory, category, recipe);
            }
        }
    }

    public IEnumerable<CompiledRecipe> All => this.byId.Values;

    public CompiledRecipe? Get(string recipeId) => this.byId.TryGetValue(recipeId, out CompiledRecipe? r) ? r : null;

    /// <summary>Recipes with at least one input this item could fill, in pack order, without duplicates.</summary>
    public IEnumerable<CompiledRecipe> Using(ItemInfo item)
    {
        var seen = new HashSet<string>();
        IEnumerable<CompiledRecipe> found = Lookup(this.byItem, item.QualifiedId);
        foreach (string tag in item.Tags)
            found = found.Concat(Lookup(this.byTag, tag));
        found = found.Concat(Lookup(this.byCategory, item.Category));
        return found.Where(r => seen.Add(r.Id));
    }

    private static IEnumerable<CompiledRecipe> Lookup<TKey>(Dictionary<TKey, List<CompiledRecipe>> map, TKey key) where TKey : notnull
        => map.TryGetValue(key, out List<CompiledRecipe>? list) ? list : Enumerable.Empty<CompiledRecipe>();

    private static void AddTo<TKey>(Dictionary<TKey, List<CompiledRecipe>> map, TKey key, CompiledRecipe recipe) where TKey : notnull
    {
        if (!map.TryGetValue(key, out List<CompiledRecipe>? list))
            map[key] = list = new();
        if (!list.Contains(recipe))
            list.Add(recipe);
    }
}
