using StardewValley;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using SObject = StardewValley.Object;

namespace ValleyOpus.Machines;

/// <summary>
/// The Ley Tap (doc 13 world tap, doc 14 §7): placed on a ley node's tile (or next to it), it draws that node's
/// Primes each morning with no input. Output waits in the tap like any machine output; extras follow one by one.
/// </summary>
internal static class LeyTaps
{
    public const string TapId = Ids.RowPrefix + "LeyTap";

    /// <summary>The node a tap at this tile draws from: the node's tile or any of its eight neighbours.</summary>
    public static LeyNodeRow? NodeAt(GameLocation location, int x, int y)
        => ModServices.Content.Registry.LeyNodes.All.FirstOrDefault(n =>
            string.Equals(n.Location, location.Name, StringComparison.OrdinalIgnoreCase) && Math.Abs(n.Tile.X - x) <= 1 && Math.Abs(n.Tile.Y - y) <= 1);

    public static void OnDayStarted(SmapiLog log)
    {
        int taps = 0;
        Utility.ForEachLocation(location =>
        {
            foreach (SObject tap in location.objects.Values.Where(o => ModServices.Content.ApparatusOf(o)?.Id == TapId).ToList())
            {
                try
                {
                    if (Draw(location, tap))
                        taps++;
                }
                catch (Exception ex)
                {
                    log.Error($"Ley Tap at {location.NameOrUniqueName} {tap.TileLocation} failed: {ex}");
                }
            }
            return true;
        });
        if (taps > 0)
            log.Trace($"Ley Taps drew from their nodes: {taps}.");
    }

    private static bool Draw(GameLocation location, SObject tap)
    {
        if (tap.heldObject.Value != null || MachineService.HasHeldOutput(tap))
            return false;
        if (NodeAt(location, (int)tap.TileLocation.X, (int)tap.TileLocation.Y) is not LeyNodeRow node || node.Profile.Count == 0)
            return false;

        // Split the node's units across its Primes by weight, largest share first; leftovers go to the heaviest.
        double total = node.Profile.Values.Sum();
        var shares = node.Profile.OrderByDescending(p => p.Value).ThenBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => (Prime: p.Key, Count: (int)Math.Floor(node.Units * p.Value / total))).ToList();
        int left = node.Units - shares.Sum(s => s.Count);
        if (left > 0)
            shares[0] = (shares[0].Prime, shares[0].Count + left);

        var outputs = shares.Where(s => s.Count > 0)
            .Select(s => (ItemId: ModServices.Content.Registry.ResolveItemId(s.Prime), s.Count))
            .Where(s => s.ItemId != null)
            .Select(s => (s.ItemId!, s.Count))
            .ToList();
        if (outputs.Count == 0)
            return false;

        Engine.Machines.MachineData state = MachineService.State(tap);
        state.Recipe = TapId;
        state.Extras = outputs.Skip(1).ToList();
        MachineService.Save(tap, state);
        tap.heldObject.Value = (SObject)ItemRegistry.Create(outputs[0].Item1, outputs[0].Item2);
        tap.readyForHarvest.Value = true;
        tap.MinutesUntilReady = 0;
        return true;
    }

    public static string Describe(SObject tap)
        => tap.Location is GameLocation location && NodeAt(location, (int)tap.TileLocation.X, (int)tap.TileLocation.Y) is LeyNodeRow node
            ? ModServices.T("tooltip.ley-node", new { name = node.NameKey != null ? ModServices.T(node.NameKey) : node.Id })
            : ModServices.T("tooltip.no-ley-node");
}
