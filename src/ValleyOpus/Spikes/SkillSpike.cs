using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ValleyOpus.Spikes;

/// <summary>
/// S14 (doc 05, doc 11): a SpaceCore skill registers, levels up, and keeps banking XP past level 4. The desk
/// check of SpaceCore's source says a skill's level is the highest step of its experience curve reached and XP
/// itself is never capped, so a four-step curve gives "levels 1–4, banking past 4" with no code of ours.
/// </summary>
internal sealed class SkillSpike : ISpike
{
    public const string SkillId = "CharlesDR.ValleyOpus_Spike";

    /// <summary>Vanilla's first four level thresholds (doc 05: vanilla-speed skill).</summary>
    private static readonly int[] Curve = { 100, 380, 770, 1300 };

    private readonly IModHelper helper;

    public SkillSpike(IModHelper helper) => this.helper = helper;

    public string Id => "S14";

    public string Summary => "SpaceCore skill: registration, level-up, XP banking past level 4. Args: (none) | xp <amount>";

    /// <summary>SpaceCore wants skills registered from Entry.</summary>
    public static void Register() => SpaceCore.Skills.RegisterSkill(new SpikeSkill());

    /// <summary>Gives the skill a plain icon once the graphics device exists, so the skills page can draw it.</summary>
    public static void CreateIcons()
    {
        if (SpaceCore.Skills.GetSkill(SkillId) is not SpaceCore.Skills.Skill skill)
            return;
        var icon = new Texture2D(Game1.graphics.GraphicsDevice, 16, 16);
        icon.SetData(Enumerable.Repeat(new Color(122, 74, 168), 16 * 16).ToArray());
        skill.Icon = icon;
        skill.SkillsPageIcon = icon;
    }

    public void Run(SpikeLog log, string[] args)
    {
        Farmer player = Game1.player;
        if (args.Length >= 2 && args[0] == "xp" && int.TryParse(args[1], out int amount))
        {
            SpaceCore.Skills.AddExperience(player, SkillId, amount);
            log.Note($"added {amount} XP: now {SpaceCore.Skills.GetExperienceFor(player, SkillId)} XP, level {SpaceCore.Skills.GetSkillLevel(player, SkillId)}");
            return;
        }

        int before = SpaceCore.Skills.GetExperienceFor(player, SkillId);
        SpaceCore.Skills.AddExperience(player, SkillId, Curve[^1] + 500);
        int xp = SpaceCore.Skills.GetExperienceFor(player, SkillId);
        int level = SpaceCore.Skills.GetSkillLevel(player, SkillId);
        string state = $"XP {before} -> {xp}, level {level}";
        if (level == Curve.Length && xp > Curve[^1])
            log.Pass($"the skill reached level {Curve.Length} and kept banking XP past it ({state})");
        else
            log.Fail($"expected level {Curve.Length} with XP past {Curve[^1]} ({state})");

        log.Look("sleep: SpaceCore should show level-up screens for levels 1 to 4 and none past 4. Open the skills page: the Spike skill shows level 4");
        log.Note($"Walk of Life installed: {this.helper.ModRegistry.IsLoaded("DaLion.Professions")}. Mastery: SpaceCore's skill code has no Mastery hook, so custom-skill XP shouldn't count toward 1.6 Mastery; check the Mastery bar didn't move");
    }

    private sealed class SpikeSkill : SpaceCore.Skills.Skill
    {
        public SpikeSkill()
            : base(SkillId)
        {
            this.ExperienceCurve = Curve;
            this.ExperienceBarColor = new Color(122, 74, 168);
        }

        public override string GetName() => "Spike Alchemy";
    }
}
