using ValleyOpus.Engine.Decomposition;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;
using ValleyOpus.Engine.Resolver;
using ValleyOpus.Engine.Validation;

namespace ValleyOpus.Engine;

/// <summary>
/// Load, validate, index (doc 09 §4b): the one path from raw registry rows to a ready resolver and decomposer,
/// used by the game adapter, the tests and the tools alike.
/// </summary>
public sealed class ContentPipeline
{
    private ContentPipeline(ContentRegistry registry, RecipeIndex index, RecipeResolver resolver, Decomposer decomposer)
    {
        this.Registry = registry;
        this.Index = index;
        this.Resolver = resolver;
        this.Decomposer = decomposer;
    }

    public ContentRegistry Registry { get; }
    public RecipeIndex Index { get; }
    public RecipeResolver Resolver { get; }
    public Decomposer Decomposer { get; }

    /// <summary>Every loader and validator finding, in order.</summary>
    public IReadOnlyList<Finding> Findings => this.Registry.Findings;

    public static ContentPipeline Build(RawRegistries raw, IItemCatalog catalog, bool catalogIsComplete, RowFormats? formats = null)
    {
        ContentRegistry registry = RegistryLoader.Load(raw, formats);
        Validator.Validate(registry, new ValidationOptions(catalog, catalogIsComplete));
        var index = new RecipeIndex(registry);
        return new ContentPipeline(registry, index, new RecipeResolver(registry, index), new Decomposer(registry, catalog));
    }
}
