using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Packs;

/// <summary>What reading one or more content packs produced.</summary>
public sealed class PackReadResult
{
    public RawRegistries Registries { get; } = new();

    /// <summary>Items the packs author in <c>Data/Objects</c> and <c>Data/BigCraftables</c>, for the validator's catalog.</summary>
    public ItemCatalog Items { get; } = new();

    /// <summary>Problems reading the packs: unreadable JSON, unsupported actions or tokens, missing translations.</summary>
    public List<Finding> Findings { get; } = new();
}

/// <summary>
/// A small reader for our Content Patcher packs, so CI validates the same rows the game loads (doc 09 §4d). It
/// understands <c>EditData</c> with <c>Entries</c>, <c>Include</c>, and the two tokens base-pack rows may use,
/// <c>{{ModId}}</c> and <c>{{i18n:…}}</c>. <c>When</c> is only understood for <c>HasMod</c>, which integration
/// packs use; anything else is reported, never guessed.
/// </summary>
public static class ContentPackReader
{
    private static readonly Regex Token = new(@"\{\{\s*([^{}]+?)\s*\}\}", RegexOptions.Compiled);

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Reads packs in load order; later packs edit after earlier ones.</summary>
    /// <param name="packFolders">Folders holding a <c>manifest.json</c> and a <c>content.json</c>.</param>
    /// <param name="installedMods">Mod IDs that count as installed for <c>HasMod</c> conditions.</param>
    public static PackReadResult Read(IEnumerable<string> packFolders, IReadOnlySet<string>? installedMods = null)
    {
        var result = new PackReadResult();
        installedMods ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string folder in packFolders)
            ReadPack(folder, installedMods, result);
        return result;
    }

    public static PackReadResult Read(params string[] packFolders) => Read(packFolders, null);

    private static void ReadPack(string folder, IReadOnlySet<string> installedMods, PackReadResult result)
    {
        string packName = Path.GetFileName(Path.TrimEndingDirectorySeparator(folder));
        JsonObject? manifest = ParseFile(Path.Combine(folder, "manifest.json"), packName, result);
        string modId = manifest?["UniqueID"]?.GetValue<string>() ?? "";
        if (modId == "")
            result.Findings.Add(new(Severity.Error, packName, "manifest.json", "no UniqueID"));

        var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string i18nPath = Path.Combine(folder, "i18n", "default.json");
        if (File.Exists(i18nPath) && ParseFile(i18nPath, packName, result) is { } i18n)
            foreach ((string key, JsonNode? value) in i18n)
                if (value is JsonValue v && v.TryGetValue(out string? text))
                    translations[key] = text;

        var context = new PackContext(folder, packName, modId, translations, installedMods, result);
        JsonObject? content = ParseFile(Path.Combine(folder, "content.json"), packName, result);
        if (content != null)
            ReadChanges(content, "content.json", context, depth: 0);
    }

    private sealed record PackContext(string Folder, string PackName, string ModId, IReadOnlyDictionary<string, string> Translations, IReadOnlySet<string> InstalledMods, PackReadResult Result);

    private static void ReadChanges(JsonObject file, string fileName, PackContext context, int depth)
    {
        if (depth > 10)
        {
            context.Result.Findings.Add(new(Severity.Error, context.PackName, fileName, "Include nested too deeply"));
            return;
        }
        if (file["Changes"] is not JsonArray changes)
            return;

        int index = 0;
        foreach (JsonNode? node in changes)
        {
            string where = $"{fileName} change #{index++}";
            if (node is not JsonObject change)
                continue;
            if (!WhenHolds(change, where, context))
                continue;

            string action = change["Action"]?.GetValue<string>() ?? "";
            switch (action)
            {
                case "Include":
                    string? from = change["FromFile"]?.GetValue<string>();
                    foreach (string path in (from ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        JsonObject? included = ParseFile(Path.Combine(context.Folder, path), context.PackName, context.Result);
                        if (included != null)
                            ReadChanges(included, path, context, depth + 1);
                    }
                    break;

                case "EditData":
                    ReadEditData(change, where, context);
                    break;

                case "Load":
                case "EditImage":
                case "EditMap":
                    break; // assets the validator doesn't read

                default:
                    context.Result.Findings.Add(new(Severity.Warning, context.PackName, where, $"action '{action}' isn't read by the CI reader"));
                    break;
            }
        }
    }

    private static bool WhenHolds(JsonObject change, string where, PackContext context)
    {
        if (change["When"] is not JsonObject when)
            return true;
        foreach ((string key, JsonNode? value) in when)
        {
            if (!key.Equals("HasMod", StringComparison.OrdinalIgnoreCase))
            {
                context.Result.Findings.Add(new(Severity.Error, context.PackName, where, $"'When: {key}' isn't allowed in registry rows; only HasMod, in integration packs (doc 09 §4d)"));
                return false;
            }
            string mods = value is JsonArray array ? string.Join(",", array.Select(a => a?.GetValue<string>())) : value?.GetValue<string>() ?? "";
            if (!mods.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).All(context.InstalledMods.Contains))
                return false;
        }
        return true;
    }

    private static void ReadEditData(JsonObject change, string where, PackContext context)
    {
        foreach (string unsupported in new[] { "Fields", "TargetField", "MoveEntries", "TextOperations" })
            if (change.ContainsKey(unsupported))
                context.Result.Findings.Add(new(Severity.Warning, context.PackName, where, $"'{unsupported}' isn't read by the CI reader"));

        string targets = Resolve(change["Target"]?.GetValue<string>() ?? "", where, context);
        if (change["Entries"] is not JsonObject entries)
            return;

        foreach (string target in targets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            bool isRegistry = RawRegistries.TryGetRegistryName(target, out string registry) && target.StartsWith(Ids.KeyPrefix, StringComparison.OrdinalIgnoreCase);
            bool isObjects = target.Equals("Data/Objects", StringComparison.OrdinalIgnoreCase);
            bool isBigCraftables = target.Equals("Data/BigCraftables", StringComparison.OrdinalIgnoreCase);
            if (!isRegistry && !isObjects && !isBigCraftables)
                continue;

            foreach ((string rawKey, JsonNode? rawValue) in entries)
            {
                string key = Resolve(rawKey, where, context);
                JsonNode? value = rawValue is null ? null : ResolveTree(JsonUtil.Clone(rawValue), where, context);
                if (isRegistry)
                {
                    if (value is not null and not JsonObject)
                        context.Result.Findings.Add(new(Severity.Error, registry, key, "row isn't a JSON object"));
                    else
                        context.Result.Registries.Set(registry, key, value as JsonObject);
                }
                else if (value is JsonObject item)
                {
                    context.Result.Items.Add(ToItemInfo((isObjects ? "(O)" : "(BC)") + key, item));
                }
            }
        }
    }

    private static ItemInfo ToItemInfo(string qualifiedId, JsonObject row)
    {
        int price = row["Price"] is JsonValue p && p.TryGetValue(out int pv) ? pv : 0;
        int category = row["Category"] is JsonValue c && c.TryGetValue(out int cv) ? cv : 0;
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (row["ContextTags"] is JsonArray array)
            foreach (JsonNode? tag in array)
                if (tag is JsonValue t && t.TryGetValue(out string? text))
                    tags.Add(text);
        return new ItemInfo(qualifiedId, price, category, tags);
    }

    private static JsonNode? ResolveTree(JsonNode node, string where, PackContext context)
    {
        switch (node)
        {
            case JsonObject obj:
                var resolved = new JsonObject();
                foreach ((string key, JsonNode? value) in obj.ToList())
                {
                    obj.Remove(key);
                    resolved[Resolve(key, where, context)] = value is null ? null : ResolveTree(value, where, context);
                }
                return resolved;

            case JsonArray array:
                var list = new JsonArray();
                foreach (JsonNode? item in array.ToList())
                {
                    array.Remove(item);
                    list.Add(item is null ? null : ResolveTree(item, where, context));
                }
                return list;

            case JsonValue value when value.TryGetValue(out string? text):
                return JsonValue.Create(Resolve(text, where, context));

            default:
                return node;
        }
    }

    private static string Resolve(string text, string where, PackContext context)
        => Token.Replace(text, match =>
        {
            string token = match.Groups[1].Value.Trim();
            if (token.Equals("ModId", StringComparison.OrdinalIgnoreCase))
                return context.ModId;
            if (token.StartsWith("i18n", StringComparison.OrdinalIgnoreCase) && token.Contains(':'))
            {
                string key = token.Substring(token.IndexOf(':') + 1).Split('|')[0].Trim();
                if (context.Translations.TryGetValue(key, out string? value))
                    return value;
                context.Result.Findings.Add(new(Severity.Warning, context.PackName, where, $"no translation for '{key}'"));
                return $"(no translation:{key})";
            }
            context.Result.Findings.Add(new(Severity.Error, context.PackName, where, $"token '{{{{{token}}}}}' isn't allowed in registry rows; only ModId and i18n (doc 09 §4d)"));
            return match.Value;
        });

    private static JsonObject? ParseFile(string path, string packName, PackReadResult result)
    {
        if (!File.Exists(path))
        {
            result.Findings.Add(new(Severity.Error, packName, Path.GetFileName(path), "file not found"));
            return null;
        }
        try
        {
            return JsonNode.Parse(File.ReadAllText(path), documentOptions: DocumentOptions) as JsonObject;
        }
        catch (JsonException ex)
        {
            result.Findings.Add(new(Severity.Error, packName, Path.GetFileName(path), "invalid JSON: " + ex.Message));
            return null;
        }
    }
}
