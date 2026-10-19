namespace ValleyOpus.Engine.Routes;

/// <summary>A cardinal direction on the tile grid.</summary>
public enum Facing { North, East, South, West }

/// <summary>
/// Directional chaining's geometry (doc 13 §7): every machine takes in from its intake side and puts out from its
/// spout side. In build 1 the spout is East and the intake West; a chest to the South is the buffer a machine
/// fills when nothing downstream takes its output, and the one it pulls from when its tray is empty.
/// </summary>
public static class ChainGeometry
{
    public const Facing Spout = Facing.East;
    public const Facing Intake = Facing.West;
    public const Facing Buffer = Facing.South;

    public static (int X, int Y) Offset(Facing facing) => facing switch
    {
        Facing.North => (0, -1),
        Facing.East => (1, 0),
        Facing.South => (0, 1),
        _ => (-1, 0),
    };

    public static (int X, int Y) Step((int X, int Y) tile, Facing facing)
    {
        (int dx, int dy) = Offset(facing);
        return (tile.X + dx, tile.Y + dy);
    }

    /// <summary>The cardinal neighbours in a fixed order (N, E, S, W), so heat and power pick the same consumers every time.</summary>
    public static IEnumerable<(int X, int Y)> Cardinals((int X, int Y) tile)
    {
        foreach (Facing f in new[] { Facing.North, Facing.East, Facing.South, Facing.West })
            yield return Step(tile, f);
    }

    /// <summary>Tiles within a square range (Chebyshev distance), excluding the centre.</summary>
    public static IEnumerable<(int X, int Y)> Within((int X, int Y) tile, int range)
    {
        for (int dy = -range; dy <= range; dy++)
            for (int dx = -range; dx <= range; dx++)
                if (dx != 0 || dy != 0)
                    yield return (tile.X + dx, tile.Y + dy);
    }

    /// <summary>
    /// The heat sources a tile receives heat from: each source heats at most <paramref name="maxConsumers"/>
    /// cardinal neighbours that ask for heat, chosen in N, E, S, W order (doc 13 §2).
    /// </summary>
    public static bool ReceivesHeat((int X, int Y) consumer, IEnumerable<(int X, int Y)> sources, Func<(int X, int Y), bool> wantsHeat, int maxConsumers)
    {
        foreach ((int X, int Y) source in sources)
        {
            List<(int X, int Y)> served = Cardinals(source).Where(wantsHeat).Take(maxConsumers).ToList();
            if (served.Contains(consumer))
                return true;
        }
        return false;
    }
}
