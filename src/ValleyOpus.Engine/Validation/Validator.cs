using ValleyOpus.Engine.Conditions;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Quality;
using ValleyOpus.Engine.Registry;
using ValleyOpus.Engine.Resolver;

namespace ValleyOpus.Engine.Validation;

/// <summary>Options for one validation pass.</summary>
/// <param name="Catalog">Every known item: the game's full list in game, pack-authored items plus fixtures in CI.</param>
/// <param name="CatalogIsComplete">True in game: an item reference the catalog doesn't know is broken. In CI only our own IDs can be checked.</param>
public sealed record ValidationOptions(IItemCatalog Catalog, bool CatalogIsComplete);

/// <summary>
/// The load-time validator (doc 09 §3 <c>Validation</c>, doc 12 §3, doc 13 §1a). It disables every row that
/// breaks a rule, with a reason, and warns about the rest; ambiguity never reaches play.
/// </summary>
public static class Validator
{
    public static IReadOnlyList<Finding> Validate(ContentRegistry registry, ValidationOptions options)
    {
        int before = registry.Findings.Count;

        CheckDecompositionRows(registry);
        CheckApparatus(registry);
        CheckSubstances(registry);
        CheckRecipes(registry, options);
        CheckUniqueness(registry, options);
        CheckTierOneGrid(registry);
        CheckSettings(registry);
        CheckProducts(registry, options);
        CheckSkillAndCodex(registry);

        return registry.Findings.Skip(before).ToList();
    }

    private static void CheckDecompositionRows(ContentRegistry registry)
    {
        foreach (PrimeRow prime in registry.Primes.All)
            if (string.IsNullOrWhiteSpace(prime.ItemId))
                registry.Warn(RegistryNames.Primes, prime.Id, "no ItemId: the Prime can't be carried or valued");

        foreach (TagWeightRow row in registry.TagWeights.All.ToList())
        {
            string? unknown = row.Weights.Keys.FirstOrDefault(p => !registry.Primes.Contains(p));
            if (unknown != null)
            {
                registry.Disable(registry.TagWeights, row.Id, $"unknown Prime '{unknown}'");
                continue;
            }
            foreach (string rare in row.Weights.Keys.Where(p => IsRare(registry, p) && !row.AetherSource))
                registry.Warn(RegistryNames.TagWeights, row.Id, $"weight for {rare} ignored: the row isn't on the Aether source list (AetherSource)");
        }

        foreach (CategoryDefaultRow row in registry.CategoryDefaults.All.ToList())
        {
            string? unknown = row.Weights.Keys.FirstOrDefault(p => !registry.Primes.Contains(p));
            if (unknown != null)
            {
                registry.Disable(registry.CategoryDefaults, row.Id, $"unknown Prime '{unknown}'");
                continue;
            }
            foreach (string rare in row.Weights.Keys.Where(p => IsRare(registry, p)))
                registry.Warn(RegistryNames.CategoryDefaults, row.Id, $"weight for {rare} ignored: category defaults never carry Aether");
        }

        foreach (DecompositionOverrideRow row in registry.Overrides.All.ToList())
        {
            string? unknown = row.Profile?.Keys.FirstOrDefault(p => !registry.Primes.Contains(p));
            if (unknown != null)
                registry.Disable(registry.Overrides, row.Id, $"unknown Prime '{unknown}'");
        }
    }

    private static void CheckApparatus(ContentRegistry registry)
    {
        foreach (ApparatusRow apparatus in registry.Apparatus.All.ToList())
        {
            if (apparatus.NeighbourStates.Count == 0)
            {
                registry.Disable(registry.Apparatus, apparatus.Id, "no NeighbourStates: every apparatus names its states (doc 13 §1a)");
                continue;
            }
            string? duplicate = apparatus.NeighbourStates.GroupBy(s => s.Id).FirstOrDefault(g => g.Count() > 1)?.Key;
            if (duplicate != null)
            {
                registry.Disable(registry.Apparatus, apparatus.Id, $"neighbour state '{duplicate}' is listed twice");
                continue;
            }
            var overlaps = NeighbourStates.Overlaps(apparatus).ToList();
            if (overlaps.Count > 0)
            {
                registry.Disable(registry.Apparatus, apparatus.Id, $"neighbour states '{overlaps[0].A}' and '{overlaps[0].B}' can both match one arrangement");
                continue;
            }
            var stateIds = apparatus.NeighbourStates.Select(s => s.Id).ToHashSet();
            ApparatusOperation? badOp = apparatus.Operations.FirstOrDefault(o => !registry.Operations.Contains(o.Operation));
            if (badOp != null)
            {
                registry.Disable(registry.Apparatus, apparatus.Id, $"unknown operation '{badOp.Operation}'");
                continue;
            }
            string? badState = apparatus.Operations.SelectMany(o => o.States).FirstOrDefault(s => !stateIds.Contains(s));
            if (badState != null)
            {
                registry.Disable(registry.Apparatus, apparatus.Id, $"an operation names unknown state '{badState}'");
                continue;
            }
            if (apparatus.QualityCap is null && !QualityStages.HasDefaultCap(apparatus.Kind))
                registry.Warn(RegistryNames.Apparatus, apparatus.Id, $"kind '{apparatus.Kind}' has no default quality cap; set QualityCap");
        }
    }

    private static void CheckSubstances(ContentRegistry registry)
    {
        foreach (SubstanceRow substance in registry.Substances.All.ToList())
        {
            if (substance.Class == SubstanceClass.Item && string.IsNullOrWhiteSpace(substance.ItemId))
                registry.Disable(registry.Substances, substance.Id, "Item-class substance with no ItemId");
            else if (substance.Class != SubstanceClass.Item && substance.ItemId != null)
                registry.Warn(RegistryNames.Substances, substance.Id, $"{substance.Class} substance has an ItemId; it is never an item");
            if (substance.Polarity != null && !registry.Substances.Contains(substance.Polarity))
                registry.Warn(RegistryNames.Substances, substance.Id, $"Polarity twin '{substance.Polarity}' is unknown");
        }
    }

    private static void CheckRecipes(ContentRegistry registry, ValidationOptions options)
    {
        foreach (RecipeRow recipe in registry.Recipes.All.ToList())
        {
            string? problem = RecipeProblem(registry, options, recipe);
            if (problem != null)
                registry.Disable(registry.Recipes, recipe.Id, problem);
            else if (recipe.Quality.Cap is int cap && !QualityStages.Values.Contains(cap))
                registry.Warn(RegistryNames.Recipes, recipe.Id, $"quality cap {cap} isn't a vanilla quality (0, 1, 2, 4)");
        }
    }

    private static string? RecipeProblem(ContentRegistry registry, ValidationOptions options, RecipeRow recipe)
    {
        if (!registry.Operations.Contains(recipe.Operation))
            return $"unknown operation '{recipe.Operation}'";
        if (recipe.Inputs.Count == 0)
            return "no inputs";
        foreach (IngredientSpec spec in recipe.Inputs)
        {
            int kinds = (spec.Item != null ? 1 : 0) + (spec.Tag != null ? 1 : 0) + (spec.Category != null ? 1 : 0);
            if (kinds != 1)
                return $"input '{spec}' must name exactly one of Item, Tag or Category";
            if (spec.Count < 1)
                return $"input '{spec}' has a count below 1";
            if (spec.Item != null)
            {
                string? itemId = registry.ResolveItemId(spec.Item);
                if (itemId is null)
                    return $"input '{spec.Item}' is a substance that is never an item";
                if (!registry.Primes.Contains(spec.Item) && !registry.Substances.Contains(spec.Item) && !KnownItem(options, itemId))
                    return $"input '{spec.Item}' doesn't resolve to an item";
            }
        }

        if (recipe.ApparatusKinds.Count == 0 && recipe.ApparatusIds.Count == 0)
            return "no ApparatusKinds or ApparatusIds";
        if (recipe.HandMadeOnly && recipe.ApparatusKinds.Contains(ApparatusKinds.Vanilla))
            return "hand-made-only recipe on a vanilla machine, which Automate drives natively";

        if (recipe.NeighbourStates.Count == 0)
            return "no NeighbourStates: a recipe that doesn't care lists every state (doc 13 §1a)";
        List<ApparatusRow> apparatus = ListedApparatus(registry, recipe).ToList();
        if (apparatus.Count > 0)
        {
            string? unknownState = recipe.NeighbourStates.FirstOrDefault(s => !apparatus.Any(a => a.NeighbourStates.Any(n => n.Id == s)));
            if (unknownState != null)
                return $"neighbour state '{unknownState}' isn't a state of any listed apparatus";
        }

        foreach (RecipeConditionRef reference in recipe.Conditions)
        {
            if (!registry.Conditions.TryGet(reference.ConditionId, out ConditionRow condition))
                return $"unknown condition '{reference.ConditionId}'";
            if (reference.Use == ConditionUse.Variant && !condition.IsPlayerControlled && IsAutomatable(registry, recipe))
                return $"Variant on '{condition.Id}', which the player doesn't control: automated output never switches with the environment (doc 13 §1a)";
        }

        string? outputItem = registry.ResolveItemId(recipe.Output.Id);
        if (!registry.Substances.Contains(recipe.Output.Id) && !registry.Primes.Contains(recipe.Output.Id) && (outputItem is null || !KnownItem(options, outputItem)))
            return $"output '{recipe.Output.Id}' doesn't resolve";

        if (recipe.SecretPool != null && !registry.SecretPools.Contains(recipe.SecretPool))
            return $"unknown secret pool '{recipe.SecretPool}'";

        return null;
    }

    /// <summary>
    /// No two recipes may share an apparatus kind, a neighbour state and an input set, where tags overlap if any
    /// real item satisfies both; the later pack's row is disabled (doc 12 §3, doc 13 §1a).
    /// </summary>
    private static void CheckUniqueness(ContentRegistry registry, ValidationOptions options)
    {
        var index = new RecipeIndex(registry);
        List<CompiledRecipe> recipes = index.All.OrderBy(r => registry.Recipes.RankOf(r.Id)).ToList();
        var disabled = new HashSet<string>();

        for (int i = 0; i < recipes.Count; i++)
        {
            if (disabled.Contains(recipes[i].Id))
                continue;
            for (int j = i + 1; j < recipes.Count; j++)
            {
                CompiledRecipe a = recipes[i], b = recipes[j];
                if (disabled.Contains(b.Id))
                    continue;
                if (!ShareApparatus(a.Row, b.Row) || !a.Row.NeighbourStates.Intersect(b.Row.NeighbourStates).Any())
                    continue;
                if (a.Row.InputCount != b.Row.InputCount || !InputsOverlap(a.Specs, b.Specs, options))
                    continue;

                Exclusivity exclusive = ConditionsExclusive(registry, a.Row, b.Row);
                if (exclusive == Exclusivity.ByPlayer)
                    continue;
                if (exclusive == Exclusivity.ByEnvironment && !(IsAutomatable(registry, a.Row) && IsAutomatable(registry, b.Row)))
                    continue;

                string why = exclusive == Exclusivity.ByEnvironment
                    ? $"same inputs as '{a.Id}' apart from a condition the player doesn't control, so automated output would switch with the environment"
                    : $"collides with '{a.Id}': same apparatus, neighbour state and inputs";
                registry.Disable(registry.Recipes, b.Id, why);
                disabled.Add(b.Id);
            }
        }
    }

    private static void CheckTierOneGrid(ContentRegistry registry)
    {
        List<PrimeRow> primes = registry.Primes.All.Where(p => p.ItemId != null).OrderBy(p => p.SortOrder).ToList();
        if (primes.Count == 0)
            return;

        var pairs = new HashSet<string>();
        foreach (RecipeRow recipe in registry.Recipes.All)
        {
            if (recipe.InputCount != 2 || recipe.Inputs.Any(s => s.Item is null || !registry.Primes.Contains(s.Item)))
                continue;
            List<string> ids = recipe.Inputs.SelectMany(s => Enumerable.Repeat(s.Item!, s.Count)).OrderBy(x => x, StringComparer.Ordinal).ToList();
            pairs.Add(ids[0] + "+" + ids[1]);
        }

        for (int i = 0; i < primes.Count; i++)
            for (int j = i; j < primes.Count; j++)
            {
                var ids = new[] { primes[i].Id, primes[j].Id }.OrderBy(x => x, StringComparer.Ordinal).ToArray();
                if (!pairs.Contains(ids[0] + "+" + ids[1]))
                    registry.Warn(RegistryNames.Recipes, ids[0] + "+" + ids[1], "missing Tier 1 cell: this pairing is an undefined combination");
            }
    }

    private static void CheckProducts(ContentRegistry registry, ValidationOptions options)
    {
        foreach (SubstanceRow substance in registry.Substances.All)
            if (substance.Class == SubstanceClass.Item && substance.ItemId != null && options.Catalog.Get(Ids.QualifyItem(substance.ItemId)) is null
                && (options.CatalogIsComplete || substance.ItemId.Contains(Ids.RowPrefix, StringComparison.OrdinalIgnoreCase)))
                registry.Warn(RegistryNames.Substances, substance.Id, $"no authored Data/Objects row for '{substance.ItemId}' (doc 09 §4c)");

        foreach (ProductRow product in registry.Products.All.ToList())
        {
            string? item = registry.ResolveItemId(product.Id);
            if (item is null || (!registry.Substances.Contains(product.Id) && !KnownItem(options, item)))
            {
                registry.Disable(registry.Products, product.Id, "the key is neither a substance nor a known item");
                continue;
            }
            string? effect = product.Uses.SelectMany(u => u.Effects).FirstOrDefault(e => !registry.Effects.Contains(e));
            if (effect != null)
            {
                registry.Disable(registry.Products, product.Id, $"unknown effect '{effect}'");
                continue;
            }
            if (product.Price.Mode == PriceMode.Fixed && product.Price.Value <= 0)
                registry.Warn(RegistryNames.Products, product.Id, "fixed price of 0 or less");
            if (product.Price.ReferenceRecipe is string reference && !registry.Recipes.Contains(reference))
                registry.Warn(RegistryNames.Products, product.Id, $"reference recipe '{reference}' is unknown; the cheapest route is used");
        }

        foreach (EffectRow effect in registry.Effects.All.ToList())
            if (effect.Kind == EffectKind.Custom && string.IsNullOrWhiteSpace(effect.Handler))
                registry.Disable(registry.Effects, effect.Id, "custom effect with no Handler");
    }

    private static void CheckSkillAndCodex(ContentRegistry registry)
    {
        foreach (XpSourceRow row in registry.XpSources.All.ToList())
            if (!XpEvents.All.Contains(row.Event))
                registry.Disable(registry.XpSources, row.Id, $"unknown XP event '{row.Event}'");

        foreach (MilestoneRow row in registry.Milestones.All.ToList())
        {
            string kind = row.Trigger.Split(':')[0];
            if (kind is not ("FirstOfTier" or "TierComplete" or "RowComplete" or "Process"))
                registry.Disable(registry.Milestones, row.Id, $"unknown trigger '{row.Trigger}'");
        }
    }

    private static void CheckSettings(ContentRegistry registry)
    {
        DecompositionSettingsRow s = registry.Settings;
        if (s.E <= 0 || s.K <= 0)
            registry.Warn(RegistryNames.Settings, s.Id, "k and e must be positive");
        if (s.ExchangeRatio <= 0 || s.ExchangeRatio > 1)
            registry.Warn(RegistryNames.Settings, s.Id, "ExchangeRatio should be in (0, 1]: decomposition never creates value");
        if (s.Threshold < 0 || s.Threshold >= 1)
            registry.Warn(RegistryNames.Settings, s.Id, "Threshold should be in [0, 1)");
    }

    private enum Exclusivity { None, ByPlayer, ByEnvironment }

    /// <summary>Whether two recipes can never both run: one requires a condition the other requires the negation of.</summary>
    private static Exclusivity ConditionsExclusive(ContentRegistry registry, RecipeRow a, RecipeRow b)
    {
        Exclusivity result = Exclusivity.None;
        foreach (RecipeConditionRef ca in a.Conditions.Where(c => c.Use != ConditionUse.Bonus && !c.Soft))
            foreach (RecipeConditionRef cb in b.Conditions.Where(c => c.Use != ConditionUse.Bonus && !c.Soft))
            {
                if (ca.ConditionId != cb.ConditionId || ca.Negate == cb.Negate)
                    continue;
                bool player = registry.Conditions.TryGet(ca.ConditionId, out ConditionRow row) && row.IsPlayerControlled;
                if (player)
                    return Exclusivity.ByPlayer;
                result = Exclusivity.ByEnvironment;
            }
        return result;
    }

    private static bool ShareApparatus(RecipeRow a, RecipeRow b)
        => a.ApparatusKinds.Intersect(b.ApparatusKinds).Any() || a.ApparatusIds.Intersect(b.ApparatusIds).Any();

    /// <summary>
    /// Whether one multiset of real items could satisfy both recipes: a perfect matching between their input slots
    /// where each pair of slots can be filled by the same item.
    /// </summary>
    internal static bool InputsOverlap(IReadOnlyList<CompiledSpec> a, IReadOnlyList<CompiledSpec> b, ValidationOptions options)
    {
        List<CompiledSpec> slotsA = a.SelectMany(s => Enumerable.Repeat(s, s.Spec.Count)).ToList();
        List<CompiledSpec> slotsB = b.SelectMany(s => Enumerable.Repeat(s, s.Spec.Count)).ToList();
        if (slotsA.Count != slotsB.Count)
            return false;

        var cache = new Dictionary<(CompiledSpec, CompiledSpec), bool>();
        bool Joint(CompiledSpec x, CompiledSpec y)
        {
            if (!cache.TryGetValue((x, y), out bool joint))
                cache[(x, y)] = joint = SpecsOverlap(x, y, options.Catalog);
            return joint;
        }

        int[] owner = Enumerable.Repeat(-1, slotsB.Count).ToArray();
        bool Assign(int u, bool[] seen)
        {
            for (int v = 0; v < slotsB.Count; v++)
            {
                if (seen[v] || !Joint(slotsA[u], slotsB[v]))
                    continue;
                seen[v] = true;
                if (owner[v] < 0 || Assign(owner[v], seen))
                {
                    owner[v] = u;
                    return true;
                }
            }
            return false;
        }

        for (int u = 0; u < slotsA.Count; u++)
            if (!Assign(u, new bool[slotsB.Count]))
                return false;
        return true;
    }

    /// <summary>Whether some single item could fill both specs (quality never limits it: an item can have any quality).</summary>
    internal static bool SpecsOverlap(CompiledSpec x, CompiledSpec y, IItemCatalog catalog)
    {
        if (x.ItemId != null && y.ItemId != null)
            return x.ItemId.Equals(y.ItemId, StringComparison.OrdinalIgnoreCase);
        if (x.Spec.Tag != null && x.Spec.Tag.Equals(y.Spec.Tag, StringComparison.OrdinalIgnoreCase))
            return true;
        if (x.Spec.Category is int cx && y.Spec.Category is int cy)
            return cx == cy;

        if (x.ItemId != null || y.ItemId != null)
        {
            (CompiledSpec exact, CompiledSpec other) = x.ItemId != null ? (x, y) : (y, x);
            ItemInfo? item = catalog.Get(exact.ItemId!);
            return item != null && other.Matches(item, int.MaxValue);
        }

        return catalog.All.Any(item => x.Matches(item, int.MaxValue) && y.Matches(item, int.MaxValue));
    }

    private static IEnumerable<ApparatusRow> ListedApparatus(ContentRegistry registry, RecipeRow recipe)
        => registry.Apparatus.All.Where(a => recipe.ApparatusKinds.Contains(a.Kind) || recipe.ApparatusIds.Contains(a.Id));

    /// <summary>Automatable: not hand-made-only, and some listed apparatus is auto-triggered and open to automation (doc 13 §1a).</summary>
    private static bool IsAutomatable(ContentRegistry registry, RecipeRow recipe)
        => !recipe.HandMadeOnly
            && (recipe.ApparatusKinds.Contains(ApparatusKinds.Vanilla)
                || ListedApparatus(registry, recipe).Any(a => a.Trigger == TriggerMode.Auto && a.Automation != AutomationMode.None));

    private static bool KnownItem(ValidationOptions options, string qualifiedId)
    {
        if (options.Catalog.Get(qualifiedId) != null)
            return true;
        // In CI only our own IDs can be checked; vanilla and other mods' items are checked in game.
        return !options.CatalogIsComplete && !qualifiedId.Contains(Ids.RowPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRare(ContentRegistry registry, string prime)
        => registry.Primes.TryGet(prime, out PrimeRow row) && row.Group == PrimeGroups.Rare;
}
