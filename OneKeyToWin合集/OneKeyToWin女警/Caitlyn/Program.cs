using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
namespace Caitlyn
{
    class Program
    {
        public const string ChampionName = "Caitlyn";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell Qc;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        //ManaMenager
        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;
        public static bool Farm = false;
        public static double WCastTime = 0;
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
            Q = new Spell(SpellSlot.Q, 1230);
            Qc = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 980);
            R = new Spell(SpellSlot.R, 3000);
            R1 = new Spell(SpellSlot.R, 3000f);

            Q.SetSkillshot(0.625f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.625f, 90f, 2200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.5f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.7f, 200f, 1500f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(R1);
            //Create the menu
            Config = new Menu("花边汉化-一键女警", ChampionName, true);

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
            Config.AddItem(new MenuItem("Botrk", "使用 破败").SetValue(true));
            Config.AddItem(new MenuItem("opsE", "连招时 使用W").SetValue(true));
            Config.AddItem(new MenuItem("useE", "进攻 E 按键").SetValue(new KeyBind('t', KeyBindType.Press)));
            Config.AddItem(new MenuItem("autoR", "自动 R").SetValue(true));
            Config.AddItem(new MenuItem("useR", "半自动 R 按键").SetValue(new KeyBind('t', KeyBindType.Press)));
			//32 == space
            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.PrintChat("<font color=\"#7e62cc\">C</font>aitlyn full automatic SI ver 1.2 >Loaded<font color=\"#000000\">by sebastiank1</font> - <font color=\"#00BFFF\">涓ㄦ眽鍖朆y 鑺辫竟</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            ManaMenager();
            if (Orbwalker.ActiveMode.ToString() == "Mixed" || Orbwalker.ActiveMode.ToString() == "LaneClear" || Orbwalker.ActiveMode.ToString() == "LastHit")
                Farm = true;
            else
                Farm = false;

            if (ObjectManager.Player.Mana > RMANA + WMANA  && W.IsReady())
            {
                
                var t = TargetSelector.GetTarget(W.Range + 300, TargetSelector.DamageType.Physical);
                foreach (var Object in ObjectManager.Get<Obj_AI_Base>().Where(Obj => Obj.Distance(Player.ServerPosition) < W.Range && Obj.Team != Player.Team && Obj.HasBuff("teleport_target", true)))
                {
                    W.Cast(Object.Position, true);
                }
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(W.Range) && W.IsReady()))
                {
                    if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.HasBuff("Recall"))
                        W.Cast(enemy, true);
                    else if (enemy.HasBuffOfType(BuffType.Slow) && t.Path.Count() > 1)
                        W.CastIfHitchanceEquals(enemy, HitChance.VeryHigh, true);
                }
            }

            if (E.IsReady())
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                var t2 = TargetSelector.GetTarget(E.Range + 350, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var eDmg = E.GetDamage(t);
                    var qDmg = Q.GetDamage(t);
                    if (GetRealDistance(t) > bonusRange() && qDmg + eDmg > t.Health && qDmg * 0.8 < t.Health && ObjectManager.Player.Mana > EMANA + QMANA && Q.IsReady())
                        E.Cast(t, true);
                    else if (Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.Mana > RMANA + EMANA && GetRealDistance(t) < 400 && ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.8)
                        E.Cast(t, true);
                    else if ( ObjectManager.Player.Mana > RMANA + EMANA && GetRealDistance(t) < 500 && ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.3)
                        E.Cast(t, true);
                    else if (Orbwalker.ActiveMode.ToString() == "Combo"
                        && ObjectManager.Player.Mana > RMANA + EMANA
                        && t2.IsValidTarget()
                        && ObjectManager.Player.GetAutoAttackDamage(t) * 2 > t.Health
                        && GetRealDistance(t2) > GetRealRange(t2) + 100
                        && !ObjectManager.Player.UnderTurret(true)
                        && t2.CountEnemiesInRange(600) < 3
                        && (Game.Time - WCastTime > 1))
                    {
                        var position = ObjectManager.Player.ServerPosition - (t2.ServerPosition - ObjectManager.Player.ServerPosition);
                        E.Cast(position, true);
                    }                         
                }
                if (Config.Item("useE").GetValue<KeyBind>().Active)
                {
                    var position = ObjectManager.Player.ServerPosition - (Game.CursorPos - ObjectManager.Player.ServerPosition);
                    E.Cast(position,true);
                }
            }

            if (Q.IsReady())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Q.GetDamage(t);
                    if (GetRealDistance(t) > bonusRange() && qDmg  > t.Health)
                        Q.Cast(t, true);
                    else if (Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.Mana > RMANA + QMANA + EMANA && ObjectManager.Player.CountEnemiesInRange(GetRealRange(t)) == 0)
                        Q.CastIfHitchanceEquals(t, HitChance.High, true);
                    else if ((Farm && ObjectManager.Player.Mana > RMANA + EMANA + QMANA + QMANA) && ObjectManager.Player.CountEnemiesInRange(bonusRange()) == 0 )
                    {
                        if (ObjectManager.Player.Mana > ObjectManager.Player.MaxMana * 0.9)
                            Q.CastIfHitchanceEquals(t, HitChance.High, true);
                        else if (t.Path.Count() > 1)
                            Qc.CastIfHitchanceEquals(t, HitChance.VeryHigh, true);
                    }
                    else if ((Orbwalker.ActiveMode.ToString() == "Combo" || Farm) && ObjectManager.Player.Mana > RMANA + QMANA && ObjectManager.Player.CountEnemiesInRange(GetRealRange(t)) == 0)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                             enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                             enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Slow) || enemy.HasBuff("Recall"))
                                Q.CastIfHitchanceEquals(t, HitChance.High, true);
                        }
                    }
                }
            }

            if (R.IsReady() && Config.Item("autoR").GetValue<bool>())
            {
                bool cast = false;
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(500 * R.Level + 1500)))
                {
                    if (target.IsValidTarget() && (Game.Time - WCastTime > 1) &&
                        !target.HasBuffOfType(BuffType.PhysicalImmunity) && !target.HasBuffOfType(BuffType.SpellImmunity) && !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        float predictedHealth = HealthPrediction.GetHealthPrediction(target, (int)(R.Delay + (Player.Distance(target.ServerPosition) / R.Speed) * 1000));
                        var Rdmg = R.GetDamage(target);
                        if (Rdmg > predictedHealth && GetRealDistance(target) > bonusRange() + 300 + target.BoundingRadius && target.CountAlliesInRange(500) == 0 && ObjectManager.Player.CountEnemiesInRange(GetRealRange(target)) == 0)
                        {
                            cast = true;
                            PredictionOutput output = R.GetPrediction(target);
                            Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
                            direction.Normalize();
                            List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
                            foreach (var enemy in enemies)
                            {
                                if (enemy.SkinName == target.SkinName || !cast)
                                    continue;
                                PredictionOutput prediction = R.GetPrediction(enemy);
                                Vector3 predictedPosition = prediction.CastPosition;
                                Vector3 v = output.CastPosition - Player.ServerPosition;
                                Vector3 w = predictedPosition - Player.ServerPosition;
                                double c1 = Vector3.Dot(w, v);
                                double c2 = Vector3.Dot(v, v);
                                double b = c1 / c2;
                                Vector3 pb = Player.ServerPosition + ((float)b * v);
                                float length = Vector3.Distance(predictedPosition, pb);
                                if (length < ( 400 + enemy.BoundingRadius) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                                    cast = false;
                            }
                            if (cast && target.IsValidTarget() && target.CountEnemiesInRange(500) == 1)
                                    R.Cast(target, true);
                        }
                    }
                }
            }
            
            PotionMenager();
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        { 
            if (Config.Item("AGC").GetValue<bool>() && E.IsReady() && ObjectManager.Player.Mana > RMANA + EMANA ) 
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                    E.Cast(Target, true);
                return;
            }
            return;
        }

        private static void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
  
            
        }

        static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            
        }

        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            double ShouldUse = ShouldUseE(args.SData.Name);

            if (Config.Item("opsE").GetValue<bool>() && unit.Team != ObjectManager.Player.Team && ShouldUse >= 0f && unit.IsValidTarget(E.Range))
                E.Cast(unit.ServerPosition, true);
            if (unit.IsMe && (args.SData.Name == "CaitlynPiltoverPeacemaker" || args.SData.Name == "CaitlynEntrapment"))
            {
                WCastTime = Game.Time;
            }
        }

        public static double ShouldUseE(string SpellName)
        {
            if (SpellName == "ThreshQ")
                return 0;
            if (SpellName == "KatarinaR")
                return 0;
            if (SpellName == "AlZaharNetherGrasp")
                return 0;
            if (SpellName == "GalioIdolOfDurand")
                return 0;
            if (SpellName == "LuxMaliceCannon")
                return 0;
            if (SpellName == "MissFortuneBulletTime")
                return 0;
            if (SpellName == "RocketGrabMissile")
                return 0;
            if (SpellName == "CaitlynPiltoverPeacemaker")
                return 0;
            if (SpellName == "EzrealTrueshotBarrage")
                return 0;
            if (SpellName == "InfiniteDuress")
                return 0;
            if (SpellName == "VelkozR")
                return 0;
            return -1;
        }

        private static float GetRealRange(GameObject target)
        {
            return 680f + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }

        public static float bonusRange()
        {
            return 680f + ObjectManager.Player.BoundingRadius;
        }
        private static float GetRealDistance(GameObject target)
        {
            return ObjectManager.Player.ServerPosition.Distance(target.Position) + ObjectManager.Player.BoundingRadius +
                   target.BoundingRadius;
        }

        public static void ManaMenager()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.Level * 2;
            else
                RMANA = R.Instance.ManaCost;

            RMANA = RMANA + (ObjectManager.Player.CountEnemiesInRange(2500) * 20);

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        public static void PotionMenager()
        {
            if (Config.Item("pots").GetValue<bool>() && Potion.IsReady() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
            {
                if (ObjectManager.Player.CountEnemiesInRange(700) > 0 && ObjectManager.Player.Health + 200 < ObjectManager.Player.MaxHealth)
                    Potion.Cast();
                else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6)
                    Potion.Cast();
            }
            if (Config.Item("pots").GetValue<bool>() && ManaPotion.IsReady() && !ObjectManager.Player.InFountain())
            {
                if (ObjectManager.Player.CountEnemiesInRange(1000) > 0 && ObjectManager.Player.Mana < RMANA + WMANA + EMANA)
                    ManaPotion.Cast();
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("noti").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(500 * R.Level + 1500, TargetSelector.DamageType.Physical);
                float predictedHealth = HealthPrediction.GetHealthPrediction(t, (int)(R.Delay + (Player.Distance(t.ServerPosition) / R.Speed) * 1000));
                if (t.IsValidTarget() && R.IsReady())
                {
                    var rDamage = R.GetDamage(t);
                    if (rDamage > predictedHealth)
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    if (Config.Item("useR").GetValue<KeyBind>().Active)
                    {
                        R.Cast(t, true);
                    }
                }
                var tw = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (tw.IsValidTarget())
                {
                    var qDmg = W.GetDamage(tw);
                    if (qDmg > tw.Health)
                    {
                        Utility.DrawCircle(ObjectManager.Player.ServerPosition, Q.Range, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "W can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    }
                }
            }
        }
    }
}
