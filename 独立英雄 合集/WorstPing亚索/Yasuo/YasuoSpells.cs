using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Yasuo
{
    public class YasuoSpells
    {
        /// <summary>
        ///     Steel Tempest Spell
        /// </summary>
        public static USpell Q = new USpell
        {
            QStage0 = new Spell(SpellSlot.Q, 475f),
            QStage1 = new Spell(SpellSlot.Q, 900f),
            DashingRange = 375f
        };

        /// <summary>
        ///     Wind Wall Spell
        /// </summary>
        public static Spell W = new Spell(SpellSlot.W, 400f);

        /// <summary>
        ///     Sweeping Blade Spell
        /// </summary>
        public static Spell E = new Spell(SpellSlot.E, 475f);

        /// <summary>
        ///     Last Breath Spell
        /// </summary>
        public static Spell R = new Spell(SpellSlot.R, 1200f);

        /// <summary>
        ///     Ravenous Hydra Item
        /// </summary>
        public static ItemData.Item RavenousHydra = ItemData.Ravenous_Hydra_Melee_Only;

        /// <summary>
        ///     Tiamat Item
        /// </summary>
        public static ItemData.Item Tiamat = ItemData.Tiamat_Melee_Only;

        /// <summary>
        ///     Blade of the Ruined King Item
        /// </summary>
        public static ItemData.Item BladeoftheRuinedKing = ItemData.Blade_of_the_Ruined_King;

        /// <summary>
        ///     Bilgewater Cutlass Item
        /// </summary>
        public static ItemData.Item BilgewaterCutlass = ItemData.Bilgewater_Cutlass;

        /// <summary>
        ///     Yasuo Skills Loader
        /// </summary>
        static YasuoSpells()
        {
            Q.QStage0.SetSkillshot(0.36f, 350f, 20000f, false, SkillshotType.SkillshotLine);
            Q.QStage1.SetSkillshot(0.36f, 120f, 1200f, true, SkillshotType.SkillshotLine);
        }

        public struct USpell
        {
            public float DashingRange;
            public Spell QStage0;
            public Spell QStage1;

            public bool IsReady()
            {
                return QStage0.IsReady() | QStage1.IsReady();
            }
        }
    }
}