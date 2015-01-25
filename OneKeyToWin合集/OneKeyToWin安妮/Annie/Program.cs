using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using SharpDX;

namespace Annie
{
    class Program
    {
        public const string ChampionName = "Annie";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        //ManaMenager
        public static int QMANA;
        public static int WMANA;
        public static int EMANA;
        public static int RMANA;
        public static bool Farm = false;
        public static bool HaveStun = false;
        //AutoPotion
        public static Items.Item Potion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item Youmuu = new Items.Item(3142, 0);
        //Menu
        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600f);
            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.60f, 50f * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.20f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu("花边汉化-一键安妮", ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("noti", "显示 通知").SetValue(true));
            Config.AddItem(new MenuItem("pots", "使用 pots").SetValue(true));
            Config.AddItem(new MenuItem("autoR", "自动 R").SetValue(true));
            Config.AddItem(new MenuItem("useR", "半自动 R 按键").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            //Add the events we are going to use:
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Game.PrintChat("<font color=\"#ff00d8\">J</font>inx full automatic SI ver 1.5 Loaded<font color=\"#000000\">by sebastiank1</font> - <font color=\"#00BFFF\">涓ㄦ眽鍖朆y 鑺辫竟</font>");
        }

        static void Orbwalking_BeforeAttack(LeagueSharp.Common.Orbwalking.BeforeAttackEventArgs args)
        {
            if (((Obj_AI_Base)Orbwalker.GetTarget()).IsMinion) args.Process = false;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (ObjectManager.Player.HasBuff("Recall"))
                return;
            HaveStun = GetPassiveStacks();

            if ((Q.IsReady() || W.IsReady()) && Orbwalker.ActiveMode.ToString() == "Combo")
            {
                var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget() && ObjectManager.Player.GetAutoAttackDamage(t) * 2 > t.Health)
                    Orbwalking.Attack = true;
                else
                    Orbwalking.Attack = false;
            }
            else
                Orbwalking.Attack = true;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var targetR = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (HaveStun && R.IsReady() && Orbwalker.ActiveMode.ToString() == "Combo" && targetR.IsValidTarget())
                R.Cast(targetR, true, true);
            else if (HaveStun && W.IsReady() && target.IsValidTarget() && CountEnemies(target, R.Width) > 1)
                W.Cast(target, true, true);
            else if ( Q.IsReady() && target.IsValidTarget())
                Q.Cast(target, true);

            if (W.IsReady() && !Q.IsReady() && target.IsValidTarget())
                W.Cast(target, true, true);
            if (!W.IsReady() && !Q.IsReady() && targetR.IsValidTarget())
                R.Cast(targetR, true, true);

            if (E.IsReady() && !HaveStun)
                E.Cast();

        }

        
        private static int CountEnemies(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                            hero.IsValidTarget() && hero.Team != ObjectManager.Player.Team &&
                            hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }
        private static int CountAlliesNearTarget(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                            hero.Team == ObjectManager.Player.Team &&
                            hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }
        public static bool GetPassiveStacks()
        {
            var buffs = Player.Buffs.Where(buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
            if (buffs.Any())
            {
                var buff = buffs.First();
                if (buff.Name.ToLower() == "pyromania_particle")
                    return true;
                else
                    return false;
            }
            return false;
        }
        public static void ManaMenager()
        {
            QMANA = 10;
            WMANA = 40 + 10 * W.Level;
            EMANA = 50;
            if (!R.IsReady())
                RMANA = WMANA - ObjectManager.Player.Level * 2;
            else
                RMANA = 100;

            if (Farm)
                RMANA = RMANA + (CountEnemies(ObjectManager.Player, 2500) * 20);

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }
    }
}
