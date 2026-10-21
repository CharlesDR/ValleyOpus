using ValleyOpus.Engine.Codex;

namespace ValleyOpus.Engine.Net;

/// <summary>The multiplayer message types (doc 17 §3); the adapter sends them with SMAPI's multiplayer API.</summary>
public static class MessageTypes
{
    public const string CodexRequest = "CodexRequest";
    public const string CodexDelta = "CodexDelta";
    public const string CodexSnapshot = "CodexSnapshot";
    public const string VersionHello = "VersionHello";
}

/// <summary>A farmhand's discovery, sent to the host, which records it and broadcasts a delta. Counts are added.</summary>
public sealed class CodexRequest
{
    public List<string> Keys { get; set; } = new();
    public Dictionary<string, int> Counts { get; set; } = new();
    public int Day { get; set; }
}

/// <summary>The host's change set: keys newly recorded and the new counts.</summary>
public sealed class CodexDelta
{
    public Dictionary<string, CodexEntry> Keys { get; set; } = new();
    public Dictionary<string, int> Counts { get; set; } = new();
}

/// <summary>The whole codex, sent to a farmhand when it connects.</summary>
public sealed class CodexSnapshot
{
    public CodexData Codex { get; set; } = new();
}

/// <summary>Each side's mod version and envelope schema; the host ignores requests from a mismatched version (D12).</summary>
public sealed class VersionHello
{
    public string ModVersion { get; set; } = "";
    public int Schema { get; set; }
}

/// <summary>Applies the host side of codex sync, free of the game so it can be tested.</summary>
public static class CodexSync
{
    /// <summary>The host records a farmhand's request; returns the delta to broadcast (empty when nothing changed).</summary>
    public static CodexDelta ApplyRequest(FarmCodex codex, CodexRequest request, long player)
    {
        var delta = new CodexDelta();
        foreach (string key in request.Keys)
            if (codex.Record(key, request.Day, player))
                delta.Keys[key] = codex.Data.Keys[key];
        foreach ((string key, int add) in request.Counts)
        {
            if (add <= 0)
                continue;
            delta.Counts[key] = codex.Data.Counts[key] = codex.Data.Counts.GetValueOrDefault(key) + add;
        }
        return delta;
    }

    /// <summary>A farmhand applies the host's delta to its mirror; returns the keys it didn't have yet (for notices).</summary>
    public static List<string> ApplyDelta(FarmCodex mirror, CodexDelta delta)
    {
        var fresh = new List<string>();
        foreach ((string key, CodexEntry entry) in delta.Keys)
            if (mirror.Record(key, entry.Day, entry.Player))
                fresh.Add(key);
        foreach ((string key, int count) in delta.Counts)
            mirror.Data.Counts[key] = Math.Max(mirror.Data.Counts.GetValueOrDefault(key), count);
        return fresh;
    }
}
