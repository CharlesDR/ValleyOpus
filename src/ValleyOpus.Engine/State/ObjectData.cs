using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.State;

/// <summary>A schema step for one object's <c>modData</c> entries, run lazily on first read and by the day-start sweep (doc 17 §5b).</summary>
public sealed record ObjectMigrationStep(int From, Func<IDictionary<string, string>, bool> Apply);

/// <summary>
/// The one accessor for our keys in an object's <c>modData</c>: every key has the <c>CharlesDR.ValleyOpus/</c>
/// prefix, and each object carries its own schema entry (doc 17 §5a). The adapter wraps the game's dictionary.
/// </summary>
public static class ObjectData
{
    public const int CurrentSchemaVersion = 1;
    public const string SchemaKey = Ids.KeyPrefix + "Schema";

    public static readonly IReadOnlyList<ObjectMigrationStep> Steps = Array.Empty<ObjectMigrationStep>();

    public static string Key(string name) => Ids.KeyPrefix + name;

    /// <summary>The object's schema; an object with any of our keys but no schema entry predates schemas and counts as 1.</summary>
    public static int SchemaOf(IDictionary<string, string> data)
    {
        if (data.TryGetValue(SchemaKey, out string? raw) && int.TryParse(raw, out int version))
            return version;
        return 1;
    }

    /// <summary>Brings the object's entries to the current schema. Returns whether anything changed; a newer object is left alone.</summary>
    public static bool Migrate(IDictionary<string, string> data, IReadOnlyList<ObjectMigrationStep>? steps = null, int current = CurrentSchemaVersion)
    {
        steps ??= Steps;
        if (!data.Keys.Any(k => k.StartsWith(Ids.KeyPrefix, StringComparison.Ordinal)))
            return false;

        int version = SchemaOf(data);
        if (version > current)
            return false;

        bool changed = !data.ContainsKey(SchemaKey);
        for (int from = version; from < current; from++)
        {
            ObjectMigrationStep? step = steps.FirstOrDefault(s => s.From == from);
            if (step is null)
                break;
            changed |= step.Apply(data);
            version = from + 1;
        }
        data[SchemaKey] = version.ToString();
        return changed;
    }
}
