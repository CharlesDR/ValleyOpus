using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Registry;

/// <summary>The enabled rows of one registry, in pack order, with disabled rows set aside with their reason.</summary>
public sealed class RowSet<T> where T : IRow
{
    private readonly List<T> rows = new();
    private readonly Dictionary<string, T> byId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> rank = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> disabled = new(StringComparer.Ordinal);

    public RowSet(string registry) => this.Registry = registry;

    public string Registry { get; }

    /// <summary>Enabled rows in pack order.</summary>
    public IReadOnlyList<T> All => this.rows;

    /// <summary>Disabled row IDs and the reason.</summary>
    public IReadOnlyDictionary<string, string> Disabled => this.disabled;

    public int Count => this.rows.Count;

    public bool TryGet(string id, out T row) => this.byId.TryGetValue(id, out row!);

    public T? Get(string id) => this.byId.TryGetValue(id, out T? row) ? row : default;

    public bool Contains(string id) => this.byId.ContainsKey(id);

    /// <summary>The row's position in pack order; later rows lose collisions.</summary>
    public int RankOf(string id) => this.rank.TryGetValue(id, out int r) ? r : int.MaxValue;

    internal void Add(T row)
    {
        this.rank[row.Id] = this.rank.Count;
        this.rows.Add(row);
        this.byId[row.Id] = row;
    }

    internal void Disable(string id, string reason)
    {
        if (this.byId.Remove(id))
            this.rows.RemoveAll(r => r.Id == id);
        this.disabled.TryAdd(id, reason);
    }
}
