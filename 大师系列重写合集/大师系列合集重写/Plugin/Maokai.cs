using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    class Maokai : Common.Helper
    {
        public Maokai()
        {
            Q = new Spell(SpellSlot.Q, 630);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 1115);
            R = new Spell(SpellSlot.R, 478);
            Q.SetSkillshot(0.3333f, 110, 1100, false, SkillshotType.SkillshotLine);
            W.SetTargetted(0.5f, 1000);
            E.SetSkillshot(0.25f, 225, 1750, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 478, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("脚本插件", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    AddItem(ComboMenu, "Q", "使用 Q");
                    AddItem(ComboMenu, "W", "使用 W");
                    AddItem(ComboMenu, "E", "使用 E");
                    AddItem(ComboMenu, "R", "使用 R");
                    AddItem(ComboMenu, "RHpU", "-> 如果敌人血量在之下", 60);
                    AddItem(ComboMenu, "RCountA", "-> 如果敌人超出", 2, 1, 5);
                    AddItem(ComboMenu, "RKill", "-> 如果能击杀将取消");
                    AddItem(ComboMenu, "RMpU", "-> 如果蓝量在之下将取消", 20);
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    AddItem(HarassMenu, "AutoQ", "使用 Q", "H", KeyBindType.Toggle);
                    AddItem(HarassMenu, "AutoQMpA", "-> 如果蓝量低于", 50);
                    AddItem(HarassMenu, "Q", "使用 Q");
                    AddItem(HarassMenu, "W", "使用 W");
                    AddItem(HarassMenu, "WHpA", "-> 如果血量低于", 20);
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
                    AddItem(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var FleeMenu = new Menu("逃跑", "Flee");
                {
                    AddItem(FleeMenu, "W", "使用 W");
                    AddItem(FleeMenu, "Q", "使用 Q 减慢敌人");
                    ChampMenu.AddSubMenu(FleeMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    var KillStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddItem(KillStealMenu, "Q", "使用 Q");
                        AddItem(KillStealMenu, "W", "使用 W");
                        AddItem(KillStealMenu, "Ignite", "使用 点燃");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    var AntiGapMenu = new Menu("防突进者", "AntiGap");
                    {
                        AddItem(AntiGapMenu, "Q", "使用 Q");
                        AddItem(AntiGapMenu, "QSlow", "-> 如果不能减速 (技能施放)");
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
                    AddItem(MiscMenu, "Gank", "Gank", "Z");
                    AddItem(MiscMenu, "WTower", "如果敌人在塔下使用W");
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
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee) Flee();
            if (GetValue<KeyBind>("Misc", "Gank").Active) NormalCombo("Gank");
            if (GetValue<KeyBind>("Harass", "AutoQ").Active) AutoQ();
            KillSteal();
            if (GetValue<bool>("Misc", "WTower")) AutoWUnderTower();
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
            if (Player.IsDead || !GetValue<bool>("AntiGap", "Q") || !GetValue<bool>("AntiGap", gapcloser.Sender.ChampionName + "_" + gapcloser.Slot.ToString()) || !Q.CanCast(gapcloser.Sender)) return;
            if (Player.Distance(gapcloser.Sender, true) <= Math.Pow(100, 2) && Q.Cast(gapcloser.Sender.ServerPosition, PacketCast))
            {
                return;
            }
            else if (GetValue<bool>("AntiGap", "QSlow") && gapcloser.SkillType == GapcloserType.Skillshot && Player.Distance(gapcloser.End) > 100 && Q.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High, PacketCast)) return;
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Player.IsDead || !GetValue<bool>("Interrupt", "Q") || !GetValue<bool>("Interrupt", unit.ChampionName + "_" + spell.Slot.ToString()) || !Q.IsReady()) return;
            if (Player.Distance(unit, true) > Math.Pow(100, 2) && W.CanCast(unit) && Player.Mana >= Q.Instance.ManaCost + W.Instance.ManaCost && W.CastOnUnit(unit, PacketCast)) return;
            if (Player.Distance(unit, true) <= Math.Pow(100, 2) && Q.Cast(unit.ServerPosition, PacketCast)) return;
        }

        private void NormalCombo(string Mode)
        {
            if (Mode == "Combo" && GetValue<bool>(Mode, "R") && R.IsReady())
            {
                var Target = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsValidTarget(R.Range));
                if (!Player.HasBuff("MaokaiDrain"))
                {
                    var RCount = GetValue<Slider>(Mode, "RCountA").Value;
                    if (Player.ManaPercentage() >= GetValue<Slider>(Mode, "RMpU").Value && ((RCount > 1 && (Target.Count >= RCount || (Target.Count > 1 && Target.Count(i => i.HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value) > 0) || (Player.CountEnemiesInRange(R.Range + 100) == 1 && R.GetTarget() != null && R.GetTarget().HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value))) || (RCount == 1 && R.GetTarget() != null && R.GetTarget().HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value)) && R.Cast(PacketCast)) return;
                }
                else if (((GetValue<bool>(Mode, "RKill") && ((Player.CountEnemiesInRange(R.Range + 50) == 1 && R.GetTarget() != null && CanKill(Target.First(), R, GetRDmg(Target.First()))) || (Target.Count > 1 && Target.Count(i => CanKill(i, R, GetRDmg(i))) > 0))) || Player.ManaPercentage() < GetValue<Slider>(Mode, "RMpU").Value) && R.Cast(PacketCast)) return;
            }
            if (Mode == "Gank")
            {
                var Target = W.GetTarget(100);
                CustomOrbwalk(Target);
                if (Target != null && W.IsReady())
                {
                    if (E.IsReady())
                    {
                        E.Speed = 1750 - Player.Distance(Target.ServerPosition);
                        if (E.CastIfWillHit(Target, -1, PacketCast))
                        {
                            E.Speed = 1750;
                            return;
                        }
                    }
                    if (W.CastOnUnit(Target, PacketCast))
                    {
                        Utility.DelayAction.Add((int)(W.Delay * 1000 + Player.Distance(Target.ServerPosition) / W.Speed - 100), () => Q.Cast(Target.ServerPosition, PacketCast));
                        return;
                    }
                }
            }
            else
            {
                if (GetValue<bool>(Mode, "E") && E.IsReady())
                {
                    var Target = E.GetTarget();
                    if (Target != null)
                    {
                        E.Speed = 1750 - Player.Distance(Target.ServerPosition);
                        if (E.CastIfWillHit(Target, -1, PacketCast))
                        {
                            E.Speed = 1750;
                            return;
                        }
                    }
                }
                if (GetValue<bool>(Mode, "W") && (Mode == "Combo" || (Mode == "Harass" && Player.HealthPercentage() >= GetValue<Slider>(Mode, "WHpA").Value)) && W.CastOnBestTarget(0, PacketCast) == Spell.CastStates.SuccessfullyCasted) return;
                if (GetValue<bool>(Mode, "Q") && Q.CastOnBestTarget(0, PacketCast) == Spell.CastStates.SuccessfullyCasted) return;
            }
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0) return;
            if (GetValue<bool>("Clear", "E") && E.IsReady() && (minionObj.Count > 2 || minionObj.Any(i => i.MaxHealth >= 1200)))
            {
                var Pos = E.GetCircularFarmLocation(minionObj);
                if (Pos.MinionsHit > 0 && Pos.Position.IsValid() && E.Cast(Pos.Position, PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var Pos = Q.GetLineFarmLocation(minionObj.FindAll(i => Q.IsInRange(i)));
                if (Pos.MinionsHit > 0 && Pos.Position.IsValid() && Q.Cast(Pos.Position, PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "W") && W.IsReady())
            {
                var Obj = minionObj.Find(i => W.IsInRange(i) && i.MaxHealth >= 1200);
                if (Obj == null && minionObj.Count(i => Player.Distance(i, true) <= Math.Pow(Orbwalk.GetAutoAttackRange(Player, i) + 40, 2)) == 0) Obj = minionObj.FindAll(i => W.IsInRange(i)).MinOrDefault(i => i.Health);
                if (Obj != null && W.CastOnUnit(Obj, PacketCast)) return;
            }
            if (GetValue<bool>("SmiteMob", "Smite") && Smite.IsReady())
            {
                var Obj = minionObj.Find(i => i.Team == GameObjectTeam.Neutral && CanSmiteMob(i.Name));
                if (Obj != null && CastSmite(Obj)) return;
            }
        }

        private void Flee()
        {
            if (GetValue<bool>("Flee", "W") && W.IsReady())
            {
                var Obj = ObjectManager.Get<Obj_AI_Base>().FindAll(i => !(i is Obj_AI_Turret) && i.IsValidTarget(W.Range + i.BoundingRadius) && i.Distance(Game.CursorPos) < 200).MinOrDefault(i => i.Distance(Game.CursorPos));
                if (Obj != null && W.CastOnUnit(Obj, PacketCast)) return;
            }
            if (GetValue<bool>("Flee", "Q") && Q.CastOnBestTarget(0, PacketCast) == Spell.CastStates.SuccessfullyCasted) return;
        }

        private void AutoQ()
        {
            if (Player.ManaPercentage() < GetValue<Slider>("Harass", "AutoQMpA").Value || Q.CastOnBestTarget(0, PacketCast) == Spell.CastStates.SuccessfullyCasted) return;
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
            if (GetValue<bool>("KillSteal", "W") && W.IsReady())
            {
                var Target = W.GetTarget();
                if (Target != null && CanKill(Target, W) && W.CastOnUnit(Target, PacketCast)) return;
            }
        }

        private void AutoWUnderTower()
        {
            if (!W.IsReady()) return;
            var Target = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsValidTarget(W.Range)).MinOrDefault(i => i.Distance(Player, true));
            var Tower = ObjectManager.Get<Obj_AI_Turret>().Find(i => i.IsAlly && !i.IsDead && i.Distance(Player, true) <= Math.Pow(950, 2));
            if (Target != null && Tower != null && Target.Distance(Tower, true) <= Math.Pow(950, 2) && W.CastOnUnit(Target, PacketCast)) return;
        }

        private double GetRDmg(Obj_AI_Base Target)
        {
            return Player.CalcDamage(Target, Damage.DamageType.Magical, new double[] { 100, 150, 200 }[R.Level - 1] + 0.5 * Player.FlatMagicDamageMod + R.Instance.Ammo);
        }
    }
}