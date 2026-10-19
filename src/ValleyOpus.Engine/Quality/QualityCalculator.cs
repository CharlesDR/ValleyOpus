using ValleyOpus.Engine.Model;

namespace ValleyOpus.Engine.Quality;

/// <summary>
/// Vanilla quality as stages (R1): 0 base, 1 silver, 2 gold, 4 iridium. Vanilla has no quality 3, so steps move
/// along the stage list and gold + 1 step is iridium (doc 12 §5).
/// </summary>
public static class QualityStages
{
    public static readonly IReadOnlyList<int> Values = new[] { 0, 1, 2, 4 };

    public const int Max = 4;

    /// <summary>The stage index of a vanilla quality; an unknown value rounds down.</summary>
    public static int IndexOf(int quality)
    {
        for (int i = Values.Count - 1; i >= 0; i--)
            if (quality >= Values[i])
                return i;
        return 0;
    }

    public static int FromIndex(int index) => Values[Math.Clamp(index, 0, Values.Count - 1)];

    /// <summary>The default cap of an apparatus kind (doc 13 §3); unknown kinds are uncapped and the validator warns.</summary>
    public static int DefaultCap(string kind) => kind switch
    {
        ApparatusKinds.Table => 0,
        ApparatusKinds.Engineered => 1,
        ApparatusKinds.MinorArcane => 2,
        ApparatusKinds.Specialist => 2,
        ApparatusKinds.RitualAltar => 4,
        ApparatusKinds.RitualCircle => 4,
        _ => 4,
    };

    public static bool HasDefaultCap(string kind) => kind is ApparatusKinds.Table or ApparatusKinds.Engineered or ApparatusKinds.MinorArcane
        or ApparatusKinds.Specialist or ApparatusKinds.RitualAltar or ApparatusKinds.RitualCircle;
}

/// <summary>Computes an output's quality: base mode, then every modifier added, then one clamp (doc 12 §5).</summary>
public static class QualityCalculator
{
    /// <param name="rule">The recipe's quality rule.</param>
    /// <param name="inputQualities">Vanilla qualities of the inputs (Primes are always 0).</param>
    /// <param name="apparatus">The apparatus running it; sets the cap unless the recipe sets a lower one.</param>
    /// <param name="modifierSteps">Every modifier in steps: met Bonus conditions, catalysts, the Lab tier, perks.</param>
    /// <param name="capRaise">Steps a perk raises the cap by, where doc 05 allows it (never past iridium).</param>
    public static int Compute(QualityRule rule, IEnumerable<int> inputQualities, ApparatusRow apparatus, IEnumerable<int>? modifierSteps = null, int capRaise = 0)
    {
        List<int> inputs = inputQualities.Select(QualityStages.IndexOf).ToList();
        int baseIndex = rule.Mode switch
        {
            QualityMode.Fixed => QualityStages.IndexOf(rule.Steps),
            QualityMode.InheritMin => inputs.Count == 0 ? 0 : inputs.Min(),
            QualityMode.InheritAverage => inputs.Count == 0 ? 0 : (int)Math.Floor(inputs.Average()),
            QualityMode.Advance => (inputs.Count == 0 ? 0 : inputs.Min()) + rule.Steps,
            _ => 0,
        };

        int index = baseIndex + (modifierSteps?.Sum() ?? 0);

        int apparatusCap = apparatus.QualityCap ?? QualityStages.DefaultCap(apparatus.Kind);
        int capIndex = QualityStages.IndexOf(apparatusCap) + Math.Max(0, capRaise);
        if (rule.Cap is int recipeCap)
            capIndex = Math.Min(capIndex, QualityStages.IndexOf(recipeCap));
        int floorIndex = Math.Min(QualityStages.IndexOf(rule.Floor), capIndex);

        return QualityStages.FromIndex(Math.Clamp(index, floorIndex, capIndex));
    }
}
