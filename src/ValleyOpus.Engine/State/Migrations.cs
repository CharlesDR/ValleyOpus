using System.Text.Json.Nodes;

namespace ValleyOpus.Engine.State;

/// <summary>One schema step from <see cref="From"/> to <c>From + 1</c>: a pure function over plain JSON (doc 17 §5b).</summary>
public sealed record MigrationStep(int From, string Description, Func<JsonObject, int> Apply)
{
    public int To => this.From + 1;
}

public enum LoadMode
{
    /// <summary>A new save, or no envelope yet.</summary>
    Fresh,

    /// <summary>Already at the current schema.</summary>
    Current,

    /// <summary>Migrated from an older schema; keep the old envelope as a backup until the next save.</summary>
    Migrated,

    /// <summary>Made by a newer build: read what we understand, never write (doc 17 §5d, D13).</summary>
    ReadOnly,

    /// <summary>A step is missing or failed; nothing was changed and the mod treats the save as read-only.</summary>
    Failed,
}

/// <param name="Envelope">The envelope to use.</param>
/// <param name="Mode">How it was loaded.</param>
/// <param name="FromVersion">The saved schema version.</param>
/// <param name="Backup">The untouched envelope to keep as <c>Backup_v&lt;FromVersion&gt;</c> when migrated.</param>
/// <param name="Log">One line per step: from, to, records changed (doc 17 §5b).</param>
public sealed record LoadResult(FarmEnvelope Envelope, LoadMode Mode, int FromVersion, JsonObject? Backup, IReadOnlyList<string> Log)
{
    public bool CanWrite => this.Mode is LoadMode.Fresh or LoadMode.Current or LoadMode.Migrated;
}

/// <summary>Applies every migration step from the save's version to the current one, in order (doc 17 §5b).</summary>
public sealed class MigrationRunner
{
    /// <summary>The steps this build ships. Empty: schema 1 is the first.</summary>
    public static readonly IReadOnlyList<MigrationStep> Steps = Array.Empty<MigrationStep>();

    private readonly IReadOnlyList<MigrationStep> steps;
    private readonly int current;

    public MigrationRunner(IReadOnlyList<MigrationStep>? steps = null, int currentVersion = FarmEnvelope.CurrentSchemaVersion)
    {
        this.steps = steps ?? Steps;
        this.current = currentVersion;
    }

    public LoadResult Load(JsonObject? saved, string modVersion)
    {
        if (saved is null)
            return new LoadResult(new FarmEnvelope { ModVersion = modVersion, SchemaVersion = this.current }, LoadMode.Fresh, this.current, null, Array.Empty<string>());

        int version = saved.TryGetPropertyValue(nameof(FarmEnvelope.SchemaVersion), out JsonNode? v) && v is JsonValue value && value.TryGetValue(out int parsed) ? parsed : 0;

        if (version > this.current)
            return new LoadResult(SafeRead(saved), LoadMode.ReadOnly, version, null, new[] { $"save schema {version} is newer than this build's {this.current}; read-only" });

        if (version == this.current)
            return new LoadResult(EnvelopeJson.Read(saved), LoadMode.Current, version, null, Array.Empty<string>());

        JsonObject backup = JsonUtil.Clone(saved);
        JsonObject working = JsonUtil.Clone(saved);
        var log = new List<string>();
        for (int from = version; from < this.current; from++)
        {
            MigrationStep? step = this.steps.FirstOrDefault(s => s.From == from);
            if (step is null)
                return new LoadResult(SafeRead(saved), LoadMode.Failed, version, null, log.Append($"no migration from schema {from}").ToList());
            try
            {
                int changed = step.Apply(working);
                working[nameof(FarmEnvelope.SchemaVersion)] = step.To;
                log.Add($"schema {step.From} -> {step.To}: {step.Description}; {changed} records changed");
            }
            catch (Exception ex)
            {
                return new LoadResult(SafeRead(saved), LoadMode.Failed, version, null, log.Append($"migration {from} failed: {ex.Message}").ToList());
            }
        }

        FarmEnvelope envelope = EnvelopeJson.Read(working);
        envelope.ModVersion = modVersion;
        return new LoadResult(envelope, LoadMode.Migrated, version, backup, log);
    }

    /// <summary>Reads what this build understands of an envelope it can't migrate; an unreadable one gives an empty envelope.</summary>
    private static FarmEnvelope SafeRead(JsonObject saved)
    {
        try
        {
            return EnvelopeJson.Read(saved);
        }
        catch (Exception)
        {
            return new FarmEnvelope();
        }
    }
}
