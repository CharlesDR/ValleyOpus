using System.Text;

namespace ValleyOpus.Engine.Randomness;

/// <summary>
/// A stable 64-bit hash (FNV-1a over UTF-8), identical on every machine, process and run. Never .NET's per-process
/// <c>string.GetHashCode</c> (doc 09 §3, doc 12 §8).
/// </summary>
public static class StableHash
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;
    private const byte Separator = 0x1F;

    public static ulong Of(params string[] parts)
    {
        ulong hash = OffsetBasis;
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0)
                hash = Step(hash, Separator);
            foreach (byte b in Encoding.UTF8.GetBytes(parts[i] ?? ""))
                hash = Step(hash, b);
        }
        return hash;
    }

    private static ulong Step(ulong hash, byte b) => (hash ^ b) * Prime;
}

/// <summary>A small deterministic generator (SplitMix64) seeded from a stable hash.</summary>
public sealed class SeededRandom
{
    private ulong state;

    public SeededRandom(ulong seed) => this.state = seed;

    /// <summary>The seed for a residue or yield roll: day, location, tile and recipe (doc 12 §8).</summary>
    public static SeededRandom ForRoll(int daysPlayed, string location, int tileX, int tileY, string recipeId)
        => new(StableHash.Of(daysPlayed.ToString(), location, tileX.ToString(), tileY.ToString(), recipeId));

    public ulong NextULong()
    {
        ulong z = this.state += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    /// <summary>A double in [0, 1).</summary>
    public double NextDouble() => (this.NextULong() >> 11) * (1.0 / (1UL << 53));

    public bool Chance(double chance) => chance >= 1 || (chance > 0 && this.NextDouble() < chance);
}
