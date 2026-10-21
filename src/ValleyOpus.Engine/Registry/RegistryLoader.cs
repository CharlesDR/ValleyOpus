using System.Text.Json;
using System.Text.Json.Nodes;
using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Registry;

/// <summary>One in-memory upgrade of a registry row from <see cref="FromVersion"/> to the next format (doc 17 §5f).</summary>
public sealed record RowUpgradeStep(string Registry, int FromVersion, Func<JsonObject, JsonObject> Apply);

/// <summary>The current pack format per registry and the steps that bring older rows up to it.</summary>
public sealed class RowFormats
{
    /// <summary>Format 1 is the first; no upgrade steps exist yet.</summary>
    public static readonly RowFormats Current = new(new Dictionary<string, int>(), Array.Empty<RowUpgradeStep>());

    private readonly IReadOnlyDictionary<string, int> versions;

    public RowFormats(IReadOnlyDictionary<string, int> versions, IReadOnlyList<RowUpgradeStep> steps)
    {
        this.versions = versions;
        this.Steps = steps;
    }

    public IReadOnlyList<RowUpgradeStep> Steps { get; }

    public int VersionOf(string registry) => this.versions.TryGetValue(registry, out int v) ? v : 1;
}

/// <summary>
/// Reads raw registry JSON into typed rows: upgrades older formats in memory, fills each row's ID from its key,
/// and disables rows that can't be read, with a reason, instead of crashing (doc 09 §4b).
/// </summary>
public static class RegistryLoader
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static ContentRegistry Load(RawRegistries raw, RowFormats? formats = null)
    {
        formats ??= RowFormats.Current;
        var findings = new List<Finding>();
        var registry = new ContentRegistry(findings);

        Read(raw, formats, RegistryNames.Primes, registry.Primes, findings);
        Read(raw, formats, RegistryNames.Substances, registry.Substances, findings);
        Read(raw, formats, RegistryNames.Recipes, registry.Recipes, findings);
        Read(raw, formats, RegistryNames.Conditions, registry.Conditions, findings);
        Read(raw, formats, RegistryNames.Operations, registry.Operations, findings);
        Read(raw, formats, RegistryNames.Apparatus, registry.Apparatus, findings);
        Read(raw, formats, RegistryNames.SecretPools, registry.SecretPools, findings);
        Read(raw, formats, RegistryNames.TagWeights, registry.TagWeights, findings);
        Read(raw, formats, RegistryNames.CategoryDefaults, registry.CategoryDefaults, findings);
        Read(raw, formats, RegistryNames.Overrides, registry.Overrides, findings);
        Read(raw, formats, RegistryNames.Products, registry.Products, findings);
        Read(raw, formats, RegistryNames.Effects, registry.Effects, findings);
        Read(raw, formats, RegistryNames.XpSources, registry.XpSources, findings);
        Read(raw, formats, RegistryNames.CodexPages, registry.CodexPages, findings);
        Read(raw, formats, RegistryNames.CodexLore, registry.Lore, findings);
        Read(raw, formats, RegistryNames.Milestones, registry.Milestones, findings);

        var settings = new RowSet<DecompositionSettingsRow>(RegistryNames.Settings);
        Read(raw, formats, RegistryNames.Settings, settings, findings);
        if (settings.Count > 1)
            findings.Add(new(Severity.Warning, RegistryNames.Settings, settings.All[^1].Id, "more than one settings row; the last one is used"));
        registry.Settings = settings.Count > 0 ? settings.All[^1] : new DecompositionSettingsRow { Id = "Default" };

        return registry;
    }

    private static void Read<T>(RawRegistries raw, RowFormats formats, string name, RowSet<T> into, List<Finding> findings) where T : IRow
    {
        int current = formats.VersionOf(name);
        foreach ((string key, JsonObject? json) in raw.Rows(name))
        {
            if (json is null)
                continue; // a pack deleted the row (CP sets the value to null)

            JsonObject row = JsonUtil.Clone(json);
            int version = row.TryGetPropertyValue(nameof(IRow.FormatVersion), out JsonNode? v) && v is JsonValue value && value.TryGetValue(out int parsed) ? parsed : 1;
            if (version > current)
            {
                findings.Add(new(Severity.Error, name, key, $"format {version} is newer than this ValleyOpus understands ({current})"));
                into.Disable(key, "newer format");
                continue;
            }

            try
            {
                for (int from = version; from < current; from++)
                {
                    RowUpgradeStep? step = formats.Steps.FirstOrDefault(s => s.Registry == name && s.FromVersion == from);
                    if (step is null)
                        throw new InvalidOperationException($"no upgrade step from format {from}");
                    row = step.Apply(row);
                }
                row[nameof(IRow.FormatVersion)] = current;

                T? parsedRow = row.Deserialize<T>(JsonOptions);
                if (parsedRow is null)
                    throw new JsonException("empty row");
                parsedRow.Id = key;
                if (into.Contains(key))
                    continue; // RawRegistries already keeps one row per key
                into.Add(parsedRow);
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or NotSupportedException or FormatException)
            {
                findings.Add(new(Severity.Error, name, key, "can't be read: " + ex.Message));
                into.Disable(key, "unreadable");
            }
        }
    }
}
