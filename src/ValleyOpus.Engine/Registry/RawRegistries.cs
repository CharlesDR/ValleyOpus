using System.Text.Json.Nodes;
using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Registry;

/// <summary>
/// Registry rows as JSON, in the order the packs added them. In game these come from our Content Patcher-filled
/// assets; in tests and tools from <see cref="Packs.ContentPackReader"/>. Content Patcher applies packs in load order,
/// and an edit to an existing key replaces the value in place, so a row's position is its rank: on a collision the
/// later row loses (doc 09 §4b, doc 13 §1a).
/// </summary>
public sealed class RawRegistries
{
    private readonly Dictionary<string, List<KeyValuePair<string, JsonObject?>>> registries = new(StringComparer.Ordinal);

    public void Set(string registry, string key, JsonObject? row)
    {
        List<KeyValuePair<string, JsonObject?>> rows = this.GetList(registry);
        int index = rows.FindIndex(p => p.Key == key);
        if (index >= 0)
            rows[index] = new(key, row);
        else
            rows.Add(new(key, row));
    }

    public void Remove(string registry, string key) => this.GetList(registry).RemoveAll(p => p.Key == key);

    public IReadOnlyList<KeyValuePair<string, JsonObject?>> Rows(string registry)
        => this.registries.TryGetValue(registry, out var rows) ? rows : Array.Empty<KeyValuePair<string, JsonObject?>>();

    public IEnumerable<string> Names => this.registries.Keys;

    private List<KeyValuePair<string, JsonObject?>> GetList(string registry)
    {
        if (!this.registries.TryGetValue(registry, out var rows))
            this.registries[registry] = rows = new();
        return rows;
    }

    /// <summary>Whether the name is one of our registries, given a full asset name or a bare registry name.</summary>
    public static bool TryGetRegistryName(string assetName, out string registry)
    {
        string name = assetName.Replace('\\', '/');
        if (name.StartsWith(Ids.KeyPrefix, StringComparison.OrdinalIgnoreCase))
            name = name.Substring(Ids.KeyPrefix.Length);
        registry = RegistryNames.All.FirstOrDefault(r => r.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? "";
        return registry != "";
    }
}
