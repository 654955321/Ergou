using LeagueSharp;
using LeagueSharp.Common;
namespace Prince_Urgot
{
    internal class SpellClass
    {
        internal static Spell Q, Q2, W, E, R;
        public SpellClass()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            Q2 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.10f, 100f, 1600f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.10f, 100f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.283f, 0f, 1750f, false, SkillshotType.SkillshotCircle);
        }
    }
}