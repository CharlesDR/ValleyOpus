using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Skill;
using ValleyOpus.Framework;
using ValleyOpus.UI;

namespace ValleyOpus.Skill;

/// <summary>
/// The <c>Skill</c> module (doc 05): the SpaceCore skill with ten levels and the six professions of the
/// <c>Professions</c> rows (two at level 5, four at 10), and every XP source with its once-only keys. Keys live in the farmer's own <c>modData</c>, so they travel with a
/// farmhand (doc 17 §1).
/// </summary>
internal sealed class AlchemySkill
{
    public const string SkillId = Ids.RowPrefix + "Alchemy";
    public const string XpKeysKey = Ids.KeyPrefix + "XpKeys";

    private readonly SmapiLog log;
    private readonly Dictionary<long, double> fractions = new();

    public AlchemySkill(SmapiLog log) => this.log = log;

    private static readonly Dictionary<string, int> ProfessionIds = new(StringComparer.Ordinal);

    public static void Register() => SpaceCore.Skills.RegisterSkill(new Definition());

    /// <summary>Adds the professions from the <c>Professions</c> rows once content can load (after launch).</summary>
    public static void RegisterProfessions(IEnumerable<ProfessionRow> rows, SmapiLog log)
    {
        if (SpaceCore.Skills.GetSkill(SkillId) is not Definition skill || skill.Professions.Count > 0)
            return;
        var byId = new Dictionary<string, Profession>(StringComparer.Ordinal);
        foreach (ProfessionRow row in rows.OrderBy(r => r.Level).ThenBy(r => r.Id, StringComparer.Ordinal))
        {
            string local = row.Id.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) ? row.Id[Ids.RowPrefix.Length..] : row.Id;
            var profession = new Profession(skill, local, row);
            skill.Professions.Add(profession);
            byId[row.Id] = profession;
            ProfessionIds[local] = profession.GetVanillaId();
        }
        foreach (IGrouping<string, ProfessionRow> pair in rows.Where(r => r.Level == 10 && r.Parent != null).GroupBy(r => r.Parent!))
        {
            List<ProfessionRow> two = pair.OrderBy(r => r.Id, StringComparer.Ordinal).ToList();
            if (two.Count == 2 && byId.TryGetValue(pair.Key, out Profession? parent))
                skill.ProfessionsForLevels.Add(new SpaceCore.Skills.Skill.ProfessionPair(10, byId[two[0].Id], byId[two[1].Id], parent));
        }
        List<ProfessionRow> five = rows.Where(r => r.Level == 5).OrderBy(r => r.Id, StringComparer.Ordinal).ToList();
        if (five.Count == 2)
            skill.ProfessionsForLevels.Insert(0, new SpaceCore.Skills.Skill.ProfessionPair(5, byId[five[0].Id], byId[five[1].Id]));
        log.Trace($"Registered {skill.Professions.Count} alchemy professions in {skill.ProfessionsForLevels.Count} pairs.");
    }

    /// <summary>Whether a player has a profession by its local ID (e.g. <c>Mechanist</c>).</summary>
    public static bool HasProfession(Farmer who, string localId)
        => ProfessionIds.TryGetValue(localId, out int id) && who.professions.Contains(id);

    // Cells on the base pack's UI sheet (sos.3 art).
    private const int SkillCell = 4;
    private const int SkillsPageCell = 5;

    private static readonly Dictionary<string, int> ProfessionCells = new(StringComparer.Ordinal)
    {
        ["Mechanist"] = 6, ["Mystic"] = 7, ["Engineer"] = 8, ["Artificer"] = 9, ["Hermeticist"] = 10, ["Theurgist"] = 11,
    };

    /// <summary>Cuts the skill and profession icons from the UI sheet once the graphics device exists.</summary>
    public static void CreateIcons(SmapiLog log)
    {
        if (SpaceCore.Skills.GetSkill(SkillId) is not SpaceCore.Skills.Skill skill)
            return;
        try
        {
            Texture2D sheet = Game1.content.Load<Texture2D>(MenuKit.UiTexture);
            skill.Icon = Cut(sheet, MenuKit.UiCell(SkillCell));
            skill.SkillsPageIcon = Cut(sheet, MenuKit.UiCell(SkillsPageCell, 10));
            foreach (SpaceCore.Skills.Skill.Profession profession in skill.Professions)
                profession.Icon = ProfessionCells.TryGetValue(profession.Id, out int cell) ? Cut(sheet, MenuKit.UiCell(cell)) : skill.Icon;
        }
        catch (Exception ex)
        {
            log.Warn($"Couldn't load the Alchemy skill icons; the skill page will show none. {ex.Message}");
        }
    }

    private static Texture2D Cut(Texture2D sheet, Rectangle area)
    {
        var pixels = new Color[area.Width * area.Height];
        sheet.GetData(0, area, pixels, 0, pixels.Length);
        var icon = new Texture2D(Game1.graphics.GraphicsDevice, area.Width, area.Height);
        icon.SetData(pixels);
        return icon;
    }

    public int Level(Farmer who) => SpaceCore.Skills.GetSkillLevel(who, SkillId);

    public bool HasKey(Farmer who, string key) => Keys(who).Contains(key);

    /// <summary>Grants XP for an event; returns the whole XP added.</summary>
    public int Grant(Farmer who, string xpEvent, string subject, int tier = 1, int count = 1)
    {
        try
        {
            HashSet<string> keys = Keys(who);
            XpGrant? grant = ModServices.Content.Xp.For(xpEvent, subject, tier, count, keys.Contains);
            if (grant is null)
                return 0;
            if (grant.OnceKey != null)
            {
                keys.Add(grant.OnceKey);
                who.modData[XpKeysKey] = string.Join("|", keys);
            }

            double total = grant.Amount + this.fractions.GetValueOrDefault(who.UniqueMultiplayerID);
            int whole = (int)Math.Floor(total);
            this.fractions[who.UniqueMultiplayerID] = total - whole;
            if (whole > 0)
                SpaceCore.Skills.AddExperience(who, SkillId, whole);
            this.log.Trace($"XP: {who.Name} +{whole} for {xpEvent} ({subject}).");
            return whole;
        }
        catch (Exception ex)
        {
            this.log.Error($"Couldn't grant {xpEvent} XP to {who.Name}: {ex}");
            return 0;
        }
    }

    private static HashSet<string> Keys(Farmer who)
        => who.modData.TryGetValue(XpKeysKey, out string? raw) && raw.Length > 0
            ? new HashSet<string>(raw.Split('|'), StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

    private sealed class Definition : SpaceCore.Skills.Skill
    {
        public Definition()
            : base(SkillId)
        {
            this.ExperienceCurve = SkillMath.Curve.ToArray();
            this.ExperienceBarColor = new Color(122, 74, 168);
        }

        public override string GetName() => ModServices.T("skill.name");

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            string key = "skill.level" + level;
            return ModServices.HasT(key) ? new List<string> { ModServices.T(key) } : new List<string>();
        }

        public override string GetSkillPageHoverText(int level) => ModServices.T("skill.hover", new { level });
    }

    private sealed class Profession : SpaceCore.Skills.Skill.Profession
    {
        private readonly ProfessionRow row;

        public Profession(SpaceCore.Skills.Skill skill, string id, ProfessionRow row)
            : base(skill, id)
        {
            this.row = row;
        }

        public override string GetName() => this.row.NameKey != null ? ModServices.T(this.row.NameKey) : this.Id;

        public override string GetDescription() => this.row.DescKey != null ? ModServices.T(this.row.DescKey) : "";
    }
}
