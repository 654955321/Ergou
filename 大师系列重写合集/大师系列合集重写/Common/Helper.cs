using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

namespace BrianSharp.Common
{
    class Helper : Program
    {
        #region Menu
        public static MenuItem AddItem(Menu SubMenu, string Item, string Display, string Key, KeyBindType Type = KeyBindType.Press, bool State = false)
        {
            return SubMenu.AddItem(new MenuItem("_" + SubMenu.Name + "_" + Item, Display, true).SetValue(new KeyBind(Key.ToCharArray()[0], Type, State)));
        }

        public static MenuItem AddItem(Menu SubMenu, string Item, string Display, bool State = true)
        {
            return SubMenu.AddItem(new MenuItem("_" + SubMenu.Name + "_" + Item, Display, true).SetValue(State));
        }

        public static MenuItem AddItem(Menu SubMenu, string Item, string Display, int Cur, int Min = 1, int Max = 100)
        {
            return SubMenu.AddItem(new MenuItem("_" + SubMenu.Name + "_" + Item, Display, true).SetValue(new Slider(Cur, Min, Max)));
        }

        public static MenuItem AddItem(Menu SubMenu, string Item, string Display, string[] Text, int DefaultIndex = 0)
        {
            return SubMenu.AddItem(new MenuItem("_" + SubMenu.Name + "_" + Item, Display, true).SetValue(new StringList(Text, DefaultIndex)));
        }

        public static bool ItemActive(string Item)
        {
            return MainMenu.Item("_OW_" + Item + "_Key", true).GetValue<KeyBind>().Active;
        }

        public static T GetValue<T>(string SubMenu, string Item)
        {
            return MainMenu.Item("_" + SubMenu + "_" + Item, true).GetValue<T>();
        }

        public static MenuItem GetItem(string SubMenu, string Item)
        {
            return MainMenu.Item("_" + SubMenu + "_" + Item, true);
        }
        #endregion

        public static bool PacketCast
        {
            get { return GetValue<bool>("Misc", "UsePacket"); }
        }

        public static void CustomOrbwalk(Obj_AI_Base Target)
        {
            Orbwalker.Orbwalk(Orbwalker.InAutoAttackRange(Target) ? Target : null);
        }

        public static bool CanKill(Obj_AI_Base Target, Spell Skill, double Health, double SubDmg)
        {
            return Skill.GetHealthPrediction(Target) - Health + 5 <= SubDmg;
        }

        public static bool CanKill(Obj_AI_Base Target, Spell Skill, double SubDmg)
        {
            return CanKill(Target, Skill, 0, SubDmg);
        }

        public static bool CanKill(Obj_AI_Base Target, Spell Skill, int Stage = 0, double SubDmg = 0)
        {
            return Skill.GetHealthPrediction(Target) + 5 <= (SubDmg > 0 ? SubDmg : Skill.GetDamage(Target, Stage));
        }

        public static bool CastFlash(Vector3 Pos)
        {
            if (!Flash.IsReady() || !Pos.IsValid()) return false;
            return Player.Spellbook.CastSpell(Flash, Pos);
        }

        public static bool CastSmite(Obj_AI_Base Target, bool Killable = true)
        {
            if (!Smite.IsReady() || !Target.IsValidTarget(760) || (Killable && Target.Health > Player.GetSummonerSpellDamage(Target, Damage.SummonerSpell.Smite))) return false;
            return Player.Spellbook.CastSpell(Smite, Target);
        }

        public static bool CastIgnite(Obj_AI_Hero Target)
        {
            if (!Ignite.IsReady() || !Target.IsValidTarget(600) || Target.Health + 5 > Player.GetSummonerSpellDamage(Target, Damage.SummonerSpell.Ignite)) return false;
            return Player.Spellbook.CastSpell(Ignite, Target);
        }

        public static InventorySlot GetWardSlot()
        {
            InventorySlot Ward = null;
            int[] WardPink = { 3362, 2043 };
            int[] WardGreen = { 3340, 3361, 2049, 2045, 2044 };
            if (GetValue<bool>("Flee", "PinkWard") && WardPink.Any(i => Items.CanUseItem(i))) Ward = Player.InventoryItems.Find(i => i.Id == (ItemId)WardPink.Find(a => Items.CanUseItem(a)));
            if (WardGreen.Any(i => Items.CanUseItem(i))) Ward = Player.InventoryItems.Find(i => i.Id == (ItemId)WardGreen.Find(a => Items.CanUseItem(a)));
            return Ward;
        }

        public static float GetWardRange()
        {
            int[] TricketWard = { 3340, 3361, 3362 };
            return 600 * ((Player.HasMastery(MasteryData.Scout) && GetWardSlot() != null && TricketWard.Contains((int)GetWardSlot().Id)) ? 1.15f : 1);
        }

        public static bool CanSmiteMob(string Name)
        {
            if (GetValue<bool>("SmiteMob", "Baron") && Name.StartsWith("SRU_Baron")) return true;
            if (GetValue<bool>("SmiteMob", "Dragon") && Name.StartsWith("SRU_Dragon")) return true;
            if (Name.Contains("Mini")) return false;
            if (GetValue<bool>("SmiteMob", "Red") && Name.StartsWith("SRU_Red")) return true;
            if (GetValue<bool>("SmiteMob", "Blue") && Name.StartsWith("SRU_Blue")) return true;
            if (GetValue<bool>("SmiteMob", "Krug") && Name.StartsWith("SRU_Krug")) return true;
            if (GetValue<bool>("SmiteMob", "Gromp") && Name.StartsWith("SRU_Gromp")) return true;
            if (GetValue<bool>("SmiteMob", "Raptor") && Name.StartsWith("SRU_Razorbeak")) return true;
            if (GetValue<bool>("SmiteMob", "Wolf") && Name.StartsWith("SRU_Murkwolf")) return true;
            return false;
        }
    }
}