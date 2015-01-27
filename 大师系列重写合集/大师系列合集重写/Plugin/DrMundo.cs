using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    class DrMundo : Common.Helper
    {
        public DrMundo()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 325);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            Q.SetSkillshot(0.25f, 60, 2000, true, SkillshotType.SkillshotLine);

            var ChampMenu = new Menu("脚本插件", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    AddItem(ComboMenu, "Q", "使用 Q");
                    AddItem(ComboMenu, "QCol", "-> 惩戒碰撞检测");
                    AddItem(ComboMenu, "W", "使用 W");
                    AddItem(ComboMenu, "WHpA", "-> 如果血量超出", 20);
                    AddItem(ComboMenu, "E", "使用 E");
                    AddItem(ComboMenu, "R", "使用 R");
                    AddItem(ComboMenu, "RHpU", "-> 如果血量超出", 50);
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    AddItem(HarassMenu, "AutoQ", "使用 Q", "H", KeyBindType.Toggle);
                    AddItem(HarassMenu, "AutoQHpA", "-> 如果血量超出", 30);
                    AddItem(HarassMenu, "Q", "使用 Q");
                    AddItem(HarassMenu, "W", "使用 W");
                    AddItem(HarassMenu, "WHpA", "-> 如果血量超出", 20);
                    AddItem(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    var SmiteMob = new Menu("如果能惩戒击杀野怪", "SmiteMob");
                    {   
					    AddItem(SmiteMob, "Baron", "大龙");
                        AddItem(SmiteMob, "Dragon", "小龙");
                        AddItem(SmiteMob, "Red", "红BUFF");
                        AddItem(SmiteMob, "Blue", "蓝BUFF");
                        AddItem(SmiteMob, "Krug", "石头怪");
                        AddItem(SmiteMob, "Gromp", "大蛤蟆");
                        AddItem(SmiteMob, "Raptor", "啄木鸟4兄弟");
                        AddItem(SmiteMob, "Wolf", "幽灵狼3兄弟");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    AddItem(ClearMenu, "Q", "使用 Q");
                    AddItem(ClearMenu, "W", "使用 W");
                    AddItem(ClearMenu, "WHpA", "-> 如果血量超出", 20);
                    AddItem(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    var KillStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddItem(KillStealMenu, "Q", "使用 Q");
                        AddItem(KillStealMenu, "Ignite", "使用 点燃");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    AddItem(MiscMenu, "QLastHit", "使用Q进行补刀");
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    AddItem(DrawMenu, "Q", "Q 范围", false);
                    AddItem(DrawMenu, "W", "W 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                MainMenu.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalk.BeforeAttack += BeforeAttack;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit) LastHit();
            if (GetValue<KeyBind>("Harass", "AutoQ").Active) AutoQ();
            KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "W") && W.Level > 0) Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
        }

        private void BeforeAttack(Orbwalk.BeforeAttackEventArgs Args)
        {
            if ((Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass) && GetValue<bool>(Orbwalk.CurrentMode.ToString(), "E") && Args.Target is Obj_AI_Hero && E.Cast(PacketCast))
            {
                return;
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && GetValue<bool>("Clear", "E") && Args.Target is Obj_AI_Minion && E.Cast(PacketCast)) return;
        }

        private void NormalCombo(string Mode)
        {
            if (GetValue<bool>(Mode, "W") && W.IsReady() && Player.HasBuff("BurningAgony") && W.GetTarget(175) == null && W.Cast(PacketCast)) return;
            if (GetValue<bool>(Mode, "Q") && Q.IsReady())
            {
                var State = Q.CastOnBestTarget(0, PacketCast);
                if (State == Spell.CastStates.SuccessfullyCasted)
                {
                    return;
                }
                else if (Mode == "Combo" && State == Spell.CastStates.Collision && GetValue<bool>(Mode, "QCol"))
                {
                    var Pred = Q.GetPrediction(Q.GetTarget());
                    if (Pred.CollisionObjects.FindAll(i => i.IsMinion).Count == 1 && CastSmite(Pred.CollisionObjects.First()) && Q.Cast(Pred.CastPosition, PacketCast)) return;
                }
            }
            if (GetValue<bool>(Mode, "W") && W.IsReady())
            {
                if (Player.HealthPercentage() >= GetValue<Slider>(Mode, "WHpA").Value)
                {
                    if (W.GetTarget(60) != null)
                    {
                        if (!Player.HasBuff("BurningAgony") && W.Cast(PacketCast)) return;
                    }
                    else if (Player.HasBuff("BurningAgony") && W.Cast(PacketCast)) return;
                }
                else if (Player.HasBuff("BurningAgony") && W.Cast(PacketCast)) return;
            }
            if (Mode == "Combo" && GetValue<bool>(Mode, "R") && Player.HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value && !Player.InFountain() && Q.GetTarget() != null && R.Cast(PacketCast)) return;
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0)
            {
                if (GetValue<bool>("Clear", "W") && W.IsReady() && Player.HasBuff("BurningAgony")) W.Cast(PacketCast);
                return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var Obj = minionObj.Find(i => CanKill(i, Q));
                if (Obj == null) Obj = minionObj.Find(i => i.MaxHealth >= 1200);
                if (Obj != null && Q.CastIfHitchanceEquals(Obj, HitChance.Medium, PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "W") && W.IsReady())
            {
                if (Player.HealthPercentage() >= GetValue<Slider>("Clear", "WHpA").Value)
                {
                    if (minionObj.Count(i => W.IsInRange(i, W.Range + 60)) > 1 || minionObj.Count(i => i.MaxHealth >= 1200 && W.IsInRange(i, W.Range + 60)) > 0)
                    {
                        if (!Player.HasBuff("BurningAgony") && W.Cast(PacketCast)) return;
                    }
                    else if (Player.HasBuff("BurningAgony") && W.Cast(PacketCast)) return;
                }
                else if (Player.HasBuff("BurningAgony") && W.Cast(PacketCast)) return;
            }
            if (GetValue<bool>("SmiteMob", "Smite") && Smite.IsReady())
            {
                var Obj = minionObj.Find(i => i.Team == GameObjectTeam.Neutral && CanSmiteMob(i.Name));
                if (Obj != null && CastSmite(Obj)) return;
            }
        }

        private void LastHit()
        {
            if (!GetValue<bool>("Misc", "QLastHit") || !Q.IsReady()) return;
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FindAll(i => CanKill(i, Q));
            if (minionObj.Count == 0 || Q.CastIfHitchanceEquals(minionObj.First(), HitChance.High, PacketCast)) return;
        }

        private void AutoQ()
        {
            if (Player.HealthPercentage() < GetValue<Slider>("Harass", "AutoQHpA").Value || Q.CastOnBestTarget(0, PacketCast) == Spell.CastStates.SuccessfullyCasted) return;
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (Target != null && CastIgnite(Target)) return;
            }
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var Target = Q.GetTarget();
                if (Target != null && CanKill(Target, Q) && Q.CastIfHitchanceEquals(Target, HitChance.High, PacketCast)) return;
            }
        }
    }
}