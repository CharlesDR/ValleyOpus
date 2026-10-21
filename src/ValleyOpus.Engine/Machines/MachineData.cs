using System.Globalization;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.State;

namespace ValleyOpus.Engine.Machines;

/// <summary>
/// One apparatus's state, read from and written to its object <c>modData</c> (doc 13 data shape). Plain strings
/// only, so it syncs in multiplayer and is inert without the mod. The tray's items live in a per-machine global
/// inventory named by <see cref="TrayId"/>, which keeps item fidelity (flavour, quality, modData).
/// </summary>
public sealed class MachineData
{
    public const string TrayKey = Ids.KeyPrefix + "Tray";
    public const string RecipeMemoryKey = Ids.KeyPrefix + "RecipeMemory";
    public const string PinnedKey = Ids.KeyPrefix + "Pinned";
    public const string OwnerKey = Ids.KeyPrefix + "Owner";
    public const string RecipeKey = Ids.KeyPrefix + "Recipe";
    public const string LockedKey = Ids.KeyPrefix + "Locked";
    public const string KindledKey = Ids.KeyPrefix + "Kindled";
    public const string FuelKey = Ids.KeyPrefix + "Fuel";
    public const string ExtrasKey = Ids.KeyPrefix + "Extras";
    public const string HandLoadedKey = Ids.KeyPrefix + "HandLoaded";
    public const string BonusKey = Ids.KeyPrefix + "Bonus";

    /// <summary>The global inventory that holds this machine's tray, e.g. <c>CharlesDR.ValleyOpus_Tray_3f2a…</c>.</summary>
    public string? TrayId { get; set; }

    /// <summary>The last recipe set by hand, or <see cref="Ids.DecomposeRecipe"/>; automated loaders only ever load it.</summary>
    public string? RecipeMemory { get; set; }
    public bool Pinned { get; set; }

    /// <summary>The multiplayer ID of the player who last set it by hand; automated XP and perks go to them.</summary>
    public long Owner { get; set; }

    /// <summary>The recipe running now (or <see cref="Ids.DecomposeRecipe"/>).</summary>
    public string? Recipe { get; set; }

    /// <summary>The running output is a first, undiscovered make: held back until collected by hand (doc 13 §6.5).</summary>
    public bool Locked { get; set; }

    /// <summary>The run was loaded by hand or the picker (full XP), not by chaining or Automate.</summary>
    public bool HandLoaded { get; set; }

    /// <summary>A Bonus condition that applied to the running recipe, for the <c>effect:</c> key.</summary>
    public string? Bonus { get; set; }
    public bool Kindled { get; set; }
    public int Fuel { get; set; }

    /// <summary>More outputs of the current run beyond the held object: extra Primes and residue.</summary>
    public List<(string ItemId, int Count)> Extras { get; set; } = new();

    public static MachineData Read(IDictionary<string, string> data)
    {
        var state = new MachineData
        {
            TrayId = Get(data, TrayKey),
            RecipeMemory = Get(data, RecipeMemoryKey),
            Pinned = Get(data, PinnedKey) == "1",
            Owner = long.TryParse(Get(data, OwnerKey), NumberStyles.Integer, CultureInfo.InvariantCulture, out long owner) ? owner : 0,
            Recipe = Get(data, RecipeKey),
            Locked = Get(data, LockedKey) == "1",
            HandLoaded = Get(data, HandLoadedKey) == "1",
            Bonus = Get(data, BonusKey),
            Kindled = Get(data, KindledKey) == "1",
            Fuel = int.TryParse(Get(data, FuelKey), NumberStyles.Integer, CultureInfo.InvariantCulture, out int fuel) ? fuel : 0,
            Extras = ParseExtras(Get(data, ExtrasKey)),
        };
        return state;
    }

    public void Write(IDictionary<string, string> data)
    {
        Set(data, TrayKey, this.TrayId);
        Set(data, RecipeMemoryKey, this.RecipeMemory);
        Set(data, PinnedKey, this.Pinned ? "1" : null);
        Set(data, OwnerKey, this.Owner != 0 ? this.Owner.ToString(CultureInfo.InvariantCulture) : null);
        Set(data, RecipeKey, this.Recipe);
        Set(data, LockedKey, this.Locked ? "1" : null);
        Set(data, HandLoadedKey, this.HandLoaded ? "1" : null);
        Set(data, BonusKey, this.Bonus);
        Set(data, KindledKey, this.Kindled ? "1" : null);
        Set(data, FuelKey, this.Fuel > 0 ? this.Fuel.ToString(CultureInfo.InvariantCulture) : null);
        Set(data, ExtrasKey, this.Extras.Count > 0 ? FormatExtras(this.Extras) : null);
        data[ObjectData.SchemaKey] = ObjectData.CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Clears what belongs to one run, keeping memory, pin, owner, kindling, fuel and the tray ID.</summary>
    public void EndRun()
    {
        this.Recipe = null;
        this.Locked = false;
        this.HandLoaded = false;
        this.Bonus = null;
        this.Extras.Clear();
    }

    /// <summary>Takes the next extra output, one item ID and count at a time, for automated extraction.</summary>
    public (string ItemId, int Count)? TakeNextExtra()
    {
        if (this.Extras.Count == 0)
            return null;
        var next = this.Extras[0];
        this.Extras.RemoveAt(0);
        return next;
    }

    public static string FormatExtras(IEnumerable<(string ItemId, int Count)> extras)
        => string.Join(";", extras.Where(e => e.Count > 0).Select(e => e.ItemId + "=" + e.Count.ToString(CultureInfo.InvariantCulture)));

    public static List<(string ItemId, int Count)> ParseExtras(string? raw)
    {
        var list = new List<(string, int)>();
        if (string.IsNullOrWhiteSpace(raw))
            return list;
        foreach (string part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = part.LastIndexOf('=');
            if (eq <= 0 || !int.TryParse(part[(eq + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count <= 0)
                continue;
            list.Add((part[..eq], count));
        }
        return list;
    }

    private static string? Get(IDictionary<string, string> data, string key) => data.TryGetValue(key, out string? value) && value != "" ? value : null;

    private static void Set(IDictionary<string, string> data, string key, string? value)
    {
        if (value is null)
            data.Remove(key);
        else
            data[key] = value;
    }
}
