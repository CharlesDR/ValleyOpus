namespace ValleyOpus.Engine.Model;

/// <summary>
/// A game item as the engine sees it: the adapter's <c>Catalog</c> snapshots every item into one of these
/// (doc 09 §3), and tests build them by hand.
/// </summary>
public sealed record ItemInfo(string QualifiedId, int Price, int Category, IReadOnlySet<string> Tags)
{
    public static ItemInfo Create(string id, int price, int category, params string[] tags)
        => new(Ids.QualifyItem(id), price, category, new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase));

    public bool HasTag(string tag) => this.Tags.Contains(tag);
}

/// <summary>One item instance in a tray or a load: the item, its vanilla quality (0, 1, 2 or 4) and a count.</summary>
public sealed record ItemStack(ItemInfo Item, int Quality = 0, int Count = 1);

/// <summary>Looks items up by qualified ID; the adapter's catalog in game, a dictionary in tests.</summary>
public interface IItemCatalog
{
    ItemInfo? Get(string qualifiedId);
    IEnumerable<ItemInfo> All { get; }
}

public sealed class ItemCatalog : IItemCatalog
{
    private readonly Dictionary<string, ItemInfo> items = new(StringComparer.OrdinalIgnoreCase);

    public ItemCatalog(IEnumerable<ItemInfo>? items = null)
    {
        foreach (ItemInfo item in items ?? Array.Empty<ItemInfo>())
            this.Add(item);
    }

    public void Add(ItemInfo item) => this.items[item.QualifiedId] = item;

    public ItemInfo? Get(string qualifiedId)
        => this.items.TryGetValue(Ids.QualifyItem(qualifiedId), out ItemInfo? item) ? item : null;

    public IEnumerable<ItemInfo> All => this.items.Values;
}
