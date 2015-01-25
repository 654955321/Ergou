using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
namespace DZDraven_Reloaded
{
    class Cleanser
    {
        public static List<QSSSpell> qssSpells = new List<QSSSpell>();
        public static bool DeathMarkCreated;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static void CreateQSSSpellMenu()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(he => he.IsEnemy))
            {
                foreach (var spell in qssSpells.Where(spell => spell.ChampName == hero.ChampionName))
                {
                    DZDraven_Reloaded.Menu.SubMenu("QSSSpell")
                        .AddItem(new MenuItem("en" + spell.SpellBuff, spell.SpellName + " always ?").SetValue(spell.isEnabled));
                    DZDraven_Reloaded.Menu.SubMenu("QSSSpell")
                        .AddItem(new MenuItem("onlyK" + spell.SpellBuff, spell.SpellName + " if killed by it?").SetValue(spell.onlyKill));
                    // VayneHunterRework.Menu.SubMenu("QSSSpell").AddItem(new MenuItem("Spacer" + spell.SpellBuff, " "));
                }//
            }

        }

        public static void CreateTypeQSSMenu()
        {
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("stun", "Stuns").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("charm", "Charms").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("taunt", "Taunts").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("fear", "Fears").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("snare", "Snares").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("silence", "Silences").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("supression", "Supression").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("polymorph", "Polymorphs").SetValue(true));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("blind", "Blinds").SetValue(false));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("slow", "Slows").SetValue(false));
            DZDraven_Reloaded.Menu.SubMenu("QSST").AddItem(new MenuItem("poison", "Poisons").SetValue(false));
        }

        public static void CreateQSSSpellList()
        {
            /**Danger Level 5 Spells*/
            qssSpells.Add(new QSSSpell { ChampName = "Warwick", isEnabled = true, SpellBuff = "InfiniteDuress", SpellName = "Warwick R", onlyKill = false });
            qssSpells.Add(new QSSSpell { ChampName = "Zed", isEnabled = true, SpellBuff = "zedulttargetmark", SpellName = "Zed R", onlyKill = true });
            qssSpells.Add(new QSSSpell { ChampName = "Rammus", isEnabled = true, SpellBuff = "PuncturingTaunt", SpellName = "Rammus E", onlyKill = false });
            /** Danger Level 4 Spells*/
            qssSpells.Add(new QSSSpell { ChampName = "Skarner", isEnabled = true, SpellBuff = "SkarnerImpale", SpellName = "Skaner R", onlyKill = false });
            qssSpells.Add(new QSSSpell { ChampName = "Fizz", isEnabled = true, SpellBuff = "FizzMarinerDoom", SpellName = "Fizz R", onlyKill = false });
            qssSpells.Add(new QSSSpell { ChampName = "Galio", isEnabled = true, SpellBuff = "GalioIdolOfDurand", SpellName = "Galio R", onlyKill = false });
            qssSpells.Add(new QSSSpell { ChampName = "Malzahar", isEnabled = true, SpellBuff = "AlZaharNetherGrasp", SpellName = "Malz R", onlyKill = false });
            /** Danger Level 3 Spells*/
            qssSpells.Add(new QSSSpell { ChampName = "Zilean", isEnabled = false, SpellBuff = "timebombenemybuff", SpellName = "Zilean Q", onlyKill = true });
            qssSpells.Add(new QSSSpell { ChampName = "Vladimir", isEnabled = false, SpellBuff = "VladimirHemoplague", SpellName = "Vlad R", onlyKill = true });
            qssSpells.Add(new QSSSpell { ChampName = "Mordekaiser", isEnabled = true, SpellBuff = "MordekaiserChildrenOfTheGrave", SpellName = "Morde R", onlyKill = true });
            /** Danger Level 2 Spells*/
            qssSpells.Add(new QSSSpell { ChampName = "Poppy", isEnabled = true, SpellBuff = "PoppyDiplomaticImmunity", SpellName = "Poppy R", onlyKill = false });
        }

        internal static void cleanUselessSpells()
        {
            List<String> nameList = new List<String>();
            foreach (var h in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                nameList.Add(h.ChampionName);
            }
            foreach (var spell in qssSpells)
            {
                if (!nameList.Contains(spell.ChampName)) qssSpells.Remove(spell);
            }
        }
        internal static void cleanserBySpell()
        {
            var hasIT = Items.HasItem(3139) || Items.HasItem(3140) || Items.HasItem(3137);
            if (!DZDraven_Reloaded.isMenuEnabled("UseQSS") || !hasIT) return;
            List<CC> ccList = new List<CC>();
            foreach (var spell in qssSpells)
            {
                if (Player.HasBuff(spell.SpellBuff)) ccList.Add(new CC() { buffName = spell.SpellBuff, willKillMe = willSpellKillMe(spell) });
            }
            foreach (var cc in ccList)
            {
                if (DZDraven_Reloaded.isMenuEnabled("en" + cc.buffName))
                {
                    Console.WriteLine("Should Cleanse. " + cc.buffName + " cause it is a spell");
                    Cleanse();
                }
                if (DZDraven_Reloaded.isMenuEnabled("onlyK" + cc.buffName) && cc.willKillMe)
                {
                    Console.WriteLine("Should Cleanse. " + cc.buffName + " cause it will kill me");
                    Cleanse();
                }
            }
        }

        internal static void enableCheck()
        {
            foreach (var spell in qssSpells)
            {
                if (DZDraven_Reloaded.isMenuEnabled("en" + spell.SpellBuff))
                {
                    DZDraven_Reloaded.Menu.Item("onlyK" + spell.SpellBuff).SetValue(false);
                }
                if (DZDraven_Reloaded.isMenuEnabled("onlyK" + spell.SpellBuff))
                {
                    DZDraven_Reloaded.Menu.Item("en" + spell.SpellBuff).SetValue(false);
                }
            }
        }
        internal static void cleanserByBuffType()
        {
            var hasIT = Items.HasItem(3139) || Items.HasItem(3140) || Items.HasItem(3137);
            if (!DZDraven_Reloaded.isMenuEnabled("UseQSS") || !hasIT) return;
            int numBuffs = UnitBuffs(Player);
            //Console.WriteLine("Should Cleanse. "+numBuffs +" cause of the bufftype check");
            if (numBuffs >= 1) Cleanse();
        }

        static bool willSpellKillMe(QSSSpell spell)
        {
            SpellSlot Spells = SpellSlot.R;
            if (spell.SpellName.Contains(spell.ChampName + " R")) Spells = SpellSlot.R;
            if (spell.SpellName.Contains(spell.ChampName + " Q")) Spells = SpellSlot.Q;
            if (spell.SpellName.Contains(spell.ChampName + " W")) Spells = SpellSlot.W;
            if (spell.SpellName.Contains(spell.ChampName + " E")) Spells = SpellSlot.E;
            var TheDamage = getByChampName(spell.ChampName).GetDamageSpell(Player, Spells).CalculatedDamage;
            BuffInstance theBuff = null;
            foreach (var Buff in Player.Buffs)
            {
                if (Buff.Name == spell.SpellBuff)
                {
                    theBuff = Buff;
                }
            }
            var EndTime = theBuff.EndTime;
            var difference = EndTime - Environment.TickCount; //TODO Factor Player Regen
            if (TheDamage >= (Player.Health))
            {
                return true;
            }
            return false;
        }
        internal static void SaveMyAss()
        {
            if (DeathMarkCreated &&
                Player.HasBuff(getSpellByName("Zed R").SpellBuff, true) && getSpellByName("Zed R").onlyKill)
            {
                Cleanse();
            }
        }

        static int UnitBuffs(Obj_AI_Hero unit)
        {
            //Taken from 'Oracle Activator'. Thanks Kurisuu ^.^
            int cc = 0;
            if (DZDraven_Reloaded.Menu.Item("slow").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Slow))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("blind").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Blind))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("charm").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Charm))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("fear").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Fear))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("snare").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Snare))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("taunt").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Taunt))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("supression").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Suppression))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("stun").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Stun))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("polymorph").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Polymorph))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("silence").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Silence))
                    cc += 1;

            if (DZDraven_Reloaded.Menu.Item("poison").GetValue<bool>())
                if (unit.HasBuffOfType(BuffType.Poison))
                    cc += 1;

            return cc;
        }
        internal static void Cleanse()
        {
            if (Items.HasItem(3140)) DZDraven_Reloaded.UseItem(3140, Player); //QSS
            if (Items.HasItem(3139)) DZDraven_Reloaded.UseItem(3139, Player); //Mercurial
            if (Items.HasItem(3137)) DZDraven_Reloaded.UseItem(3137, Player); //Dervish Blade
        }
        public static void OnCreateObj(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Zed_Base_R_buf_tell.troy" && sender.IsEnemy)
            {
                DeathMarkCreated = true;
                SaveMyAss();
            }
        }
        public static void OnDeleteObj(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Zed_Base_R_buf_tell.troy" && sender.IsEnemy)
            {
                DeathMarkCreated = false;
            }
        }
        static QSSSpell getSpellByName(String Name)
        {
            return qssSpells.Find(spell => spell.SpellName == Name);
        }

        static Obj_AI_Hero getByChampName(String Name)
        {
            return ObjectManager.Get<Obj_AI_Hero>().First(h => h.ChampionName == Name);
        }

    }
    internal class QSSSpell
    {
        public String ChampName { get; set; }
        public String SpellName { get; set; }
        public String SpellBuff { get; set; }
        public bool isEnabled { get; set; }
        public bool onlyKill { get; set; }
    }
    internal class CC
    {
        public String buffName { get; set; }
        public bool willKillMe { get; set; }
    }
}
