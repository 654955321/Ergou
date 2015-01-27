using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    class Amumu : Common.Helper
    {
        public Amumu()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);
            Q.SetSkillshot(0.25f, 90, 2000, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 350, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 550, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("脚本插件", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    AddItem(ComboMenu, "Q", "使用 Q");
                    AddItem(ComboMenu, "QCol", "-> 惩戒碰撞检测");
                    AddItem(ComboMenu, "W", "使用 W");
                    AddItem(ComboMenu, "WMpA", "-> 如果蓝量超出", 20);
                    AddItem(ComboMenu, "E", "使用 E");
                    AddItem(ComboMenu, "R", "使用 R");
                    AddItem(ComboMenu, "RHpU", "-> 如果敌人血量在之下", 60);
                    AddItem(ComboMenu, "RCountA", "-> 如果敌人超出", 2, 1, 5);
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    AddItem(HarassMenu, "W", "使用 W");
                    AddItem(HarassMenu, "WMpA", "-> 如果蓝量超出", 20);
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
                    AddItem(ClearMenu, "WMpA", "-> 如果蓝量超出", 20);
                    AddItem(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    var KillStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddItem(KillStealMenu, "E", "使用 E");
                        AddItem(KillStealMenu, "R", "使用 R");
                        AddItem(KillStealMenu, "Ignite", "使用 点燃");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    var AntiGapMenu = new Menu("防突进者", "AntiGap");
                    {
                        AddItem(AntiGapMenu, "Q", "使用 Q");
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy))
                        {
                            foreach (var Spell in AntiGapcloser.Spells.Where(i => i.ChampionName == Obj.ChampionName)) AddItem(AntiGapMenu, Obj.ChampionName + "_" + Spell.Slot.ToString(), "-> Skill " + Spell.Slot.ToString() + " Of " + Obj.ChampionName);
                        }
                        MiscMenu.AddSubMenu(AntiGapMenu);
                    }
                    var InterruptMenu = new Menu("打断", "Interrupt");
                    {
                        AddItem(InterruptMenu, "Q", "使用 Q");
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy))
                        {
                            foreach (var Spell in Interrupter.Spells.Where(i => i.ChampionName == Obj.ChampionName)) AddItem(InterruptMenu, Obj.ChampionName + "_" + Spell.Slot.ToString(), "-> Skill " + Spell.Slot.ToString() + " Of " + Obj.ChampionName);
                        }
                        MiscMenu.AddSubMenu(InterruptMenu);
                    }
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    AddItem(DrawMenu, "Q", "Q 范围", false);
                    AddItem(DrawMenu, "W", "W 范围", false);
                    AddItem(DrawMenu, "E", "E 范围", false);
                    AddItem(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                MainMenu.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear) LaneJungClear();
            KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "W") && W.Level > 0) Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "E") && E.Level > 0) Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "R") && R.Level > 0) Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.IsDead || !GetValue<bool>("AntiGap", "Q") || !GetValue<bool>("AntiGap", gapcloser.Sender.ChampionName + "_" + gapcloser.Slot.ToString()) || !Q.IsReady() || !Orbwalk.InAutoAttackRange(gapcloser.Sender)) return;
            if (Q.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High, PacketCast)) return;
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Player.IsDead || !GetValue<bool>("Interrupt", "Q") || !GetValue<bool>("Interrupt", unit.ChampionName + "_" + spell.Slot.ToString()) || !Q.CanCast(unit)) return;
            if (Q.CastIfHitchanceEquals(unit, HitChance.High, PacketCast)) return;
        }

        private void NormalCombo(string Mode)
        {
            if (GetValue<bool>(Mode, "W") && W.IsReady() && Player.HasBuff("AuraofDespair") && W.GetTarget(200) == null && W.Cast(PacketCast)) return;
            if (Mode == "Combo")
            {
                if (GetValue<bool>(Mode, "R") && R.IsReady())
                {
                    var Target = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsValidTarget(R.Range));
                    if (((Target.Count > 1 && Target.Count(i => CanKill(i, R)) > 0) || Target.Count >= GetValue<Slider>(Mode, "RCountA").Value || (Target.Count > 1 && Target.Count(i => i.HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value) > 0)) && R.Cast(PacketCast)) return;
                }
                if (GetValue<bool>(Mode, "Q") && Q.IsReady())
                {
                    if (GetValue<bool>(Mode, "R") && R.IsReady())
                    {
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().FindAll(i => !(i is Obj_AI_Turret) && i.IsValidTarget(Q.Range) && Q.GetPrediction(i).Hitchance >= HitChance.High).OrderByDescending(i => i.CountEnemiesInRange(R.Range)))
                        {
                            var Sub = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsValidTarget(R.Range - 20, true, Obj.ServerPosition));
                            if (Sub.Count > 0 && ((Sub.Count > 1 && Sub.Count(i => CanKill(i, R)) > 0) || Sub.Count >= GetValue<Slider>(Mode, "RCountA").Value || (Sub.Count > 1 && Sub.Count(i => i.HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value) > 0)) && Q.CastIfHitchanceEquals(Obj, HitChance.High, PacketCast)) return;
                        }
                    }
                    var Target = Q.GetTarget();
                    if (Target != null && !Orbwalk.InAutoAttackRange(Target))
                    {
                        var State = Q.Cast(Target, PacketCast);
                        if (State == Spell.CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                        else if (State == Spell.CastStates.Collision && GetValue<bool>(Mode, "QCol"))
                        {
                            var Pred = Q.GetPrediction(Target);
                            if (Pred.CollisionObjects.FindAll(i => i.IsMinion).Count == 1 && CastSmite(Pred.CollisionObjects.First()) && Q.Cast(Pred.CastPosition, PacketCast)) return;
                        }
                    }
                }
            }
            if (GetValue<bool>(Mode, "E") && E.IsReady() && E.GetTarget() != null && E.Cast(PacketCast)) return;
            if (GetValue<bool>(Mode, "W") && W.IsReady())
            {
                if (Player.ManaPercentage() >= GetValue<Slider>(Mode, "WMpA").Value)
                {
                    if (W.GetTarget(60) != null)
                    {
                        if (!Player.HasBuff("AuraofDespair") && W.Cast(PacketCast)) return;
                    }
                    else if (Player.HasBuff("AuraofDespair") && W.Cast(PacketCast)) return;
                }
                else if (Player.HasBuff("AuraofDespair") && W.Cast(PacketCast)) return;
            }
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0)
            {
                if (GetValue<bool>("Clear", "W") && W.IsReady() && Player.HasBuff("AuraofDespair")) W.Cast(PacketCast);
                return;
            }
            if (GetValue<bool>("Clear", "E") && E.IsReady() && minionObj.Count(i => E.IsInRange(i)) > 0 && E.Cast(PacketCast)) return;
            if (GetValue<bool>("Clear", "W") && W.IsReady())
            {
                if (Player.ManaPercentage() >= GetValue<Slider>("Clear", "WMpA").Value)
                {
                    if (minionObj.Count(i => W.IsInRange(i, W.Range + 60)) > 1 || minionObj.Count(i => i.MaxHealth >= 1200 && W.IsInRange(i, W.Range + 60)) > 0)
                    {
                        if (!Player.HasBuff("AuraofDespair") && W.Cast(PacketCast)) return;
                    }
                    else if (Player.HasBuff("AuraofDespair") && W.Cast(PacketCast)) return;
                }
                else if (Player.HasBuff("AuraofDespair") && W.Cast(PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var Obj = minionObj.Find(i => CanKill(i, Q));
                if (Obj == null) Obj = minionObj.Find(i => !Orbwalk.InAutoAttackRange(i));
                if (Obj != null && Q.CastIfHitchanceEquals(Obj, HitChance.Medium, PacketCast)) return;
            }
            if (GetValue<bool>("SmiteMob", "Smite") && Smite.IsReady())
            {
                var Obj = minionObj.Find(i => i.Team == GameObjectTeam.Neutral && CanSmiteMob(i.Name));
                if (Obj != null && CastSmite(Obj)) return;
            }
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (Target != null && CastIgnite(Target)) return;
            }
            if (GetValue<bool>("KillSteal", "E") && E.IsReady())
            {
                var Target = E.GetTarget();
                if (Target != null && CanKill(Target, E) && E.Cast(PacketCast)) return;
            }
            if (GetValue<bool>("KillSteal", "R") && R.IsReady())
            {
                var Target = R.GetTarget();
                if (Target != null && CanKill(Target, R) && R.Cast(PacketCast)) return;
            }
        }
    }
}