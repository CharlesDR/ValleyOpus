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
