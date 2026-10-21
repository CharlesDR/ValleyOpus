using System.Text.Json;
using System.Text.Json.Nodes;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.State;

/// <summary>
/// All host save data in one versioned record, <c>CharlesDR.ValleyOpus/Farm</c> (doc 17 §5a). Sections other
/// than the codex are filled by the batches that build their systems; unknown sections are kept as JSON so an
/// older build never drops what a newer one wrote.
/// </summary>
public sealed class FarmEnvelope
{
    /// <summary>The schema this build writes. Version 1 is the slice's envelope (doc 17 §5g).</summary>
    public const int CurrentSchemaVersion = 1;

    public const string SaveKey = Ids.KeyPrefix + "Farm";

    public static string BackupKey(int version) => Ids.KeyPrefix + "Backup_v" + version;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public string ModVersion { get; set; } = "";

    /// <summary>The schema the one-time object <c>modData</c> sweep last brought every object to (doc 17 §5b).</summary>
    public int ObjectsMigratedTo { get; set; } = CurrentSchemaVersion;

    public CodexData Codex { get; set; } = new();
    public JsonObject Unlocks { get; set; } = new();
    public JsonObject Familiars { get; set; } = new();
    public Dictionary<string, PlayerRecord> Players { get; set; } = new();
    public JsonObject SliceStats { get; set; } = new();

    /// <summary>
    /// Our buildings, stripped out of the farm on every save so a removed mod can't break the save (S3 fix, Charles
    /// option C, doc 17 §6). Restored right after saving and on load.
    /// </summary>
    public List<StrippedBuilding> StrippedBuildings { get; set; } = new();

    /// <summary>The day (DaysPlayed) the strip was written; a save loaded on a later day was played without the mod.</summary>
    public int StrippedAtDay { get; set; }
}

/// <summary>One stripped building: where it stood and its vanilla XML, as the game serializes it.</summary>
public sealed class StrippedBuilding
{
    public string Location { get; set; } = "Farm";
    public string BuildingType { get; set; } = "";
    public int TileX { get; set; }
    public int TileY { get; set; }
    public string Xml { get; set; } = "";
}

/// <summary>Per-player records kept by the host, keyed by the player's multiplayer ID (doc 17 data shape).</summary>
public sealed class PlayerRecord
{
    public List<string> XpKeys { get; set; } = new();
    public int SatchelCapacity { get; set; }
}

/// <summary>Reads and writes the envelope as plain JSON.</summary>
public static class EnvelopeJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public static string Write(FarmEnvelope envelope) => JsonSerializer.Serialize(envelope, Options);

    public static JsonObject ToNode(FarmEnvelope envelope) => JsonSerializer.SerializeToNode(envelope, Options)!.AsObject();

    public static FarmEnvelope Read(JsonObject node) => node.Deserialize<FarmEnvelope>(Options) ?? new FarmEnvelope();
}
