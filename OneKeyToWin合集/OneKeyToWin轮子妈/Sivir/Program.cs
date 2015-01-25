using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
namespace Sivir
{
    class Program
    {
        public const string ChampionName = "Sivir";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell Qc;
        public static Spell R;

        public static int QMANA;
        public static int WMANA;
        public static int RMANA;
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
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 1250f);
            Qc = new Spell(SpellSlot.Q, 1250f);
            W = new Spell(SpellSlot.W, float.MaxValue);
            E = new Spell(SpellSlot.E, float.MaxValue);

            R = new Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.25f, 90f, 1300f, true, SkillshotType.SkillshotLine);
            SpellList.Add(Q);
            SpellList.Add(W);

            SpellList.Add(R);

            //Create the menu
            Config = new Menu("花边汉化-一键轮子妈", ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("pots", "使用 pots").SetValue(true));
            Config.AddItem(new MenuItem("autoE", "自动 E").SetValue(true));
            Config.AddItem(new MenuItem("autoR", "自动 R").SetValue(true));
            Config.AddItem(new MenuItem("Edmg", "E 抵挡伤害丨hp<= %").SetValue(new Slider(0, 100, 0)));

            

            //Add the events we are going to use:
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttackEvenH;
            Game.PrintChat("<font color=\"#9c3232\">S</font>ivir full automatic AI ver 1.1 Loaded<font color=\"#000000\">by sebastiank1</font> - <font color=\"#00BFFF\">涓ㄦ眽鍖朆y 鑺辫竟</font>");

        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var dmg = sender.GetSpellDamage(ObjectManager.Player, args.SData.Name);
            double HpLeft = ObjectManager.Player.Health - dmg;
            double HpPercentage = (dmg * 100) / ObjectManager.Player.Health;

            if (sender.IsValid<Obj_AI_Hero>() && HpPercentage >= Config.Item("Edmg").GetValue<Slider>().Value && sender.MaxMana > 10  && sender.IsEnemy && args.Target.IsMe && !args.SData.IsAutoAttack() && Config.Item("autoE").GetValue<bool>() && E.IsReady())
            {
                E.Cast();
                //Game.PrintChat("" + HpPercentage);
            }

        }

        private static void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target)
        {
            var t = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
            if (W.IsReady() && unit.IsMe)
            {
                if (Orbwalker.ActiveMode.ToString() == "Combo" && target is Obj_AI_Hero && ObjectManager.Player.Mana > RMANA + WMANA)
                    W.Cast();
                else if (target is Obj_AI_Hero && ObjectManager.Player.Mana > RMANA + WMANA + QMANA)
                    W.Cast();
                else if (Orbwalker.ActiveMode.ToString() == "LaneClear" && ObjectManager.Player.Mana > RMANA + WMANA + QMANA + WMANA && farmW())
                    W.Cast();
                else if (Orbwalker.ActiveMode.ToString() == "LaneClear" && ObjectManager.Player.Mana > RMANA + WMANA + QMANA + WMANA && t.IsValidTarget())
                    W.Cast();
                if (Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.GetAutoAttackDamage(t) * 3 > target.Health && !Q.IsReady() && !R.IsReady())
                    W.Cast();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            ManaMenager();
            PotionMenager();
            if (Q.IsReady())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = W.GetDamage(t);
                    if (qDmg * 2 > t.Health)
                        Q.Cast(t, true);
                    else if (Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.Mana > RMANA + QMANA)
                        Q.CastIfHitchanceEquals(t, HitChance.VeryHigh, true);
                    else if (((Orbwalker.ActiveMode.ToString() == "Mixed" || Orbwalker.ActiveMode.ToString() == "LaneClear") ))
                        if (ObjectManager.Player.Mana > RMANA + WMANA + QMANA + QMANA && t.Path.Count() > 1)
                            Qc.CastIfHitchanceEquals(t, HitChance.VeryHigh, true);
                        else if (ObjectManager.Player.Mana > ObjectManager.Player.MaxMana * 0.9 )
                             Q.CastIfHitchanceEquals(t, HitChance.High, true);
                    else if (ObjectManager.Player.Mana > RMANA + QMANA)
                    {
                            if (t.HasBuffOfType(BuffType.Stun) || t.HasBuffOfType(BuffType.Snare) ||
                                t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Fear) ||
                                t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Slow)
                                || t.HasBuffOfType(BuffType.Suppression) || t.IsStunned || t.HasBuff("Recall"))
                                Q.CastIfHitchanceEquals(t, HitChance.High, true);
                    }
                }
                
            }
            if (R.IsReady() && Config.Item("autoR").GetValue<bool>())
            {
                if (CountEnemies(ObjectManager.Player, 700f) > 2)
                    R.Cast();
            }
        }
        public static bool farmW()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1300, MinionTypes.All);
            int num=0;
            foreach (var minion in allMinionsQ)
            {
                num++;
            }
            if (num > 4)
                return true;
            else
                return false;
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

        private static float GetSlowEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Slow)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        public static void ManaMenager()
        {
            QMANA = 60 + 10 * Q.Level;
            WMANA = 60;
            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.Level * 3;
            else
                RMANA = 100;
            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.3)
            {
                QMANA = 0;
                WMANA = 0;
                RMANA = 0;
            }

        }
        public static void PotionMenager()
        {
            if (Config.Item("pots").GetValue<bool>() && Potion.IsReady() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
            {
                if (CountEnemies(ObjectManager.Player, 600) > 0 && ObjectManager.Player.Health + 200 < ObjectManager.Player.MaxHealth)
                    Potion.Cast();
                else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6)
                    Potion.Cast();
            }
            if (Config.Item("pots").GetValue<bool>() && ManaPotion.IsReady() && !ObjectManager.Player.InFountain())
            {
                if (CountEnemies(ObjectManager.Player, 1000) > 0 && ObjectManager.Player.Mana < RMANA + WMANA + QMANA)
                    ManaPotion.Cast();
            }
        }

    }

}