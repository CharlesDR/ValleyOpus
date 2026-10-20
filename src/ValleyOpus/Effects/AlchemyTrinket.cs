using StardewValley;
using StardewValley.Objects.Trinkets;
using ValleyOpus.Engine.Model;

namespace ValleyOpus.Effects;

/// <summary>
/// The trinket effect class every ValleyOpus trinket names in <c>Data/Trinkets</c> (doc 15 §5). The effect itself is
/// the row's <c>CharlesDR.ValleyOpus/Effect</c> custom field, kept applied by <see cref="Handlers"/> while worn, so
/// this class only has to exist and do nothing the game would double.
/// </summary>
public sealed class AlchemyTrinket : TrinketEffect
{
    public const string EffectField = Ids.KeyPrefix + "Effect";

    public AlchemyTrinket(Trinket trinket)
        : base(trinket) { }

    public override void Apply(Farmer farmer) { }

    public override void Unapply(Farmer farmer)
    {
        if (this.Trinket.GetTrinketData()?.CustomFields?.TryGetValue(EffectField, out string? effect) == true)
            farmer.buffs.Remove(effect);
    }
}
