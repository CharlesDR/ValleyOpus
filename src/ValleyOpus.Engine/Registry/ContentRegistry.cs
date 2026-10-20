using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Registry;

/// <summary>Every registry row the packs loaded, after format upgrades and validation (doc 09 §3 <c>Registry</c>).</summary>
public sealed class ContentRegistry
{
    private readonly List<Finding> findings;

    internal ContentRegistry(List<Finding> findings) => this.findings = findings;

    public RowSet<PrimeRow> Primes { get; } = new(RegistryNames.Primes);
    public RowSet<SubstanceRow> Substances { get; } = new(RegistryNames.Substances);
    public RowSet<RecipeRow> Recipes { get; } = new(RegistryNames.Recipes);
    public RowSet<ConditionRow> Conditions { get; } = new(RegistryNames.Conditions);
    public RowSet<OperationRow> Operations { get; } = new(RegistryNames.Operations);
    public RowSet<ApparatusRow> Apparatus { get; } = new(RegistryNames.Apparatus);
    public RowSet<SecretPoolRow> SecretPools { get; } = new(RegistryNames.SecretPools);
    public RowSet<TagWeightRow> TagWeights { get; } = new(RegistryNames.TagWeights);
    public RowSet<CategoryDefaultRow> CategoryDefaults { get; } = new(RegistryNames.CategoryDefaults);
    public RowSet<DecompositionOverrideRow> Overrides { get; } = new(RegistryNames.Overrides);
    public RowSet<ProductRow> Products { get; } = new(RegistryNames.Products);
    public RowSet<EffectRow> Effects { get; } = new(RegistryNames.Effects);
    public RowSet<XpSourceRow> XpSources { get; } = new(RegistryNames.XpSources);
    public RowSet<CodexPageRow> CodexPages { get; } = new(RegistryNames.CodexPages);
    public RowSet<LoreRow> Lore { get; } = new(RegistryNames.CodexLore);
    public RowSet<MilestoneRow> Milestones { get; } = new(RegistryNames.Milestones);
    public RowSet<UnlockRow> Unlocks { get; } = new(RegistryNames.Unlocks);
    public RowSet<WorkshopRoomRow> Rooms { get; } = new(RegistryNames.WorkshopRooms);
    public RowSet<WorkshopBundleRow> Bundles { get; } = new(RegistryNames.WorkshopBundles);
    public RowSet<LeyNodeRow> LeyNodes { get; } = new(RegistryNames.LeyNodes);
    public RowSet<HintRow> Hints { get; } = new(RegistryNames.Hints);
    public RowSet<ContractRow> Contracts { get; } = new(RegistryNames.Contracts);
    public RowSet<FamiliarRow> Familiars { get; } = new(RegistryNames.Familiars);
    public RowSet<ProfessionRow> Professions { get; } = new(RegistryNames.Professions);
    public DecompositionSettingsRow Settings { get; internal set; } = new() { Id = "Default" };

    /// <summary>Loader and validator findings so far.</summary>
    public IReadOnlyList<Finding> Findings => this.findings;

    /// <summary>
    /// The qualified item ID an ingredient or output names: a Prime or substance ID resolves to its item, anything
    /// else is taken as an item ID. Null for a substance with no item (Power, ProcessState, Intermediate).
    /// </summary>
    public string? ResolveItemId(string id)
    {
        if (this.Primes.TryGet(id, out PrimeRow prime))
            return prime.ItemId is null ? null : Ids.QualifyItem(prime.ItemId);
        if (this.Substances.TryGet(id, out SubstanceRow substance))
            return substance.ItemId is null ? null : Ids.QualifyItem(substance.ItemId);
        return Ids.QualifyItem(id);
    }

    /// <summary>The Prime whose item this is, if any.</summary>
    public PrimeRow? PrimeForItem(string qualifiedItemId)
        => this.Primes.All.FirstOrDefault(p => p.ItemId != null && Ids.QualifyItem(p.ItemId).Equals(qualifiedItemId, StringComparison.OrdinalIgnoreCase));

    /// <summary>The substance whose item this is, if any.</summary>
    public SubstanceRow? SubstanceForItem(string qualifiedItemId)
        => this.Substances.All.FirstOrDefault(s => s.ItemId != null && Ids.QualifyItem(s.ItemId).Equals(qualifiedItemId, StringComparison.OrdinalIgnoreCase));

    /// <summary>The apparatus placed by this item, if any.</summary>
    public ApparatusRow? ApparatusForItem(string qualifiedItemId)
        => this.Apparatus.All.FirstOrDefault(a => a.ItemId != null && a.ItemId.Equals(qualifiedItemId, StringComparison.OrdinalIgnoreCase));

    /// <summary>The product row for an item: keyed by its substance ID or its qualified item ID.</summary>
    public ProductRow? ProductForItem(string qualifiedItemId)
    {
        SubstanceRow? substance = this.SubstanceForItem(qualifiedItemId);
        if (substance != null && this.Products.TryGet(substance.Id, out ProductRow row))
            return row;
        return this.Products.TryGet(qualifiedItemId, out ProductRow byItem) ? byItem : null;
    }

    /// <summary>Apparatus rows of a kind.</summary>
    public IEnumerable<ApparatusRow> ApparatusOfKind(string kind) => this.Apparatus.All.Where(a => a.Kind == kind);

    internal void Report(Finding finding) => this.findings.Add(finding);

    internal void Disable<T>(RowSet<T> set, string id, string reason) where T : IRow
    {
        if (!set.Contains(id))
            return;
        set.Disable(id, reason);
        this.findings.Add(new Finding(Severity.Error, set.Registry, id, reason));
    }

    internal void Warn(string registry, string id, string message)
        => this.findings.Add(new Finding(Severity.Warning, registry, id, message));
}
