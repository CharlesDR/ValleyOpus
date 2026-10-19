using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Engine.Skill;

/// <summary>
/// The alchemy skill's level curve and XP rules (doc 05). Build 1 caps the skill at level 4; XP keeps banking past
/// it, so the levels arrive when later builds raise the cap (doc 11).
/// </summary>
public static class SkillMath
{
    /// <summary>Cumulative XP for levels 1–4 (vanilla's first four steps).</summary>
    public static readonly IReadOnlyList<int> Curve = new[] { 100, 380, 770, 1300 };

    public const int MaxLevel = 4;

    public static int LevelFor(int xp)
    {
        int level = 0;
        foreach (int need in Curve)
            if (xp >= need)
                level++;
        return Math.Min(level, MaxLevel);
    }

    /// <summary>Satchel type slots: 24, plus 12 at level 2 (doc 03b, doc 05).</summary>
    public static int SatchelCapacity(int level) => level >= 2 ? 36 : 24;

    /// <summary>The Lens assays a whole chest from level 4 (doc 05).</summary>
    public static bool LensAssaysChests(int level) => level >= 4;
}

/// <summary>
/// One XP grant; <see cref="OnceKey"/> is stored per player so it never pays twice. Automated output pays a
/// fraction (a quarter of 2 per item), so the amount can be fractional; the adapter banks the remainder.
/// </summary>
public sealed record XpGrant(double Amount, string? OnceKey);

/// <summary>Computes XP for an event from the <c>XpSources</c> rows (doc 05 data shape).</summary>
public sealed class XpCalculator
{
    private readonly ContentRegistry registry;

    public XpCalculator(ContentRegistry registry) => this.registry = registry;

    /// <param name="xpEvent">One of <see cref="XpEvents"/>.</param>
    /// <param name="subject">The recipe, effect, item or key the event is about (for <c>OncePer</c>).</param>
    /// <param name="tier">The recipe's tier, for <c>TierScale</c>.</param>
    /// <param name="count">How many items (a hand collection of a stack).</param>
    /// <param name="hasKey">Whether the player already holds a once-only key.</param>
    public XpGrant? For(string xpEvent, string subject, int tier, int count, Func<string, bool> hasKey)
    {
        XpSourceRow? row = this.registry.XpSources.All.FirstOrDefault(r => r.Event == xpEvent);
        if (row is null || row.Amount <= 0)
            return null;

        string? onceKey = row.OncePer is null ? null : $"xp:{xpEvent}:{subject}";
        if (onceKey != null && hasKey(onceKey))
            return null;

        double amount = row.Amount * (row.TierScale ? Math.Max(1, tier) : 1) * Math.Max(1, count) * row.Scale;
        if (row.CapPerAction > 0)
            amount = Math.Min(amount, row.CapPerAction * row.Scale);
        return amount > 0 ? new XpGrant(amount, onceKey) : null;
    }
}
