using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugins
{
    class Garen : Common.Helper
    {
        public Garen()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 400);
            Q.SetTargetted(0.2333f, float.MaxValue);
            R.SetTargetted(0.13f, 900);

            var ChampMenu = new Menu("脚本插件", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    var KillableMenu = new Menu("能击杀 (R)", "Killable");
                    {
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy)) AddItem(KillableMenu, Obj.ChampionName, Obj.ChampionName);
                        ComboMenu.AddSubMenu(KillableMenu);
                    }
                    AddItem(ComboMenu, "Q", "使用 Q");
                    AddItem(ComboMenu, "W", "使用 W");
                    AddItem(ComboMenu, "WHpU", "-> 如果血量低下", 60);
                    AddItem(ComboMenu, "E", "使用 E");
                    AddItem(ComboMenu, "R", "如果能击杀使用 R ");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    AddItem(HarassMenu, "Q", "使用 Q");
                    AddItem(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    AddItem(ClearMenu, "Q", "使用 Q");
                    AddItem(ClearMenu, "QMode", "-> 模式", new[] { "普攻重置", "能击杀" });
                    AddItem(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var FleeMenu = new Menu("逃跑", "Flee");
                {
                    AddItem(FleeMenu, "Q", "使用 Q");
                    ChampMenu.AddSubMenu(FleeMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    var KillStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddItem(KillStealMenu, "R", "使用 R");
                        AddItem(KillStealMenu, "Ignite", "使用 点燃");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    AddItem(MiscMenu, "QLastHit", "使用Q进行补刀");
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    AddItem(DrawMenu, "E", "E 范围", false);
                    AddItem(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                MainMenu.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalk.BeforeAttack += BeforeAttack;
            Orbwalk.AfterAttack += AfterAttack;
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
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit)
            {
                LastHit();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && GetValue<bool>("Flee", "Q") && Q.Cast(PacketCast)) return;
            KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (GetValue<bool>("Draw", "E") && E.Level > 0) Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "R") && R.Level > 0) Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void BeforeAttack(Orbwalk.BeforeAttackEventArgs Args)
        {
            if (!Q.IsReady()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Harass && GetValue<bool>("Harass", "Q") && Args.Target is Obj_AI_Hero && Q.Cast(PacketCast))
            {
                return;
            }
            else if (((Orbwalk.CurrentMode == Orbwalk.Mode.LastHit && GetValue<bool>("Misc", "QLastHit")) || (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && GetValue<bool>("Clear", "Q") && GetValue<StringList>("Clear", "QMode").SelectedIndex == 1)) && Args.Target is Obj_AI_Minion && CanKill((Obj_AI_Minion)Args.Target, Q) && Q.Cast(PacketCast)) return;
        }

        private void AfterAttack(AttackableUnit Target)
        {
            if (!Q.IsReady()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo && GetValue<bool>("Combo", "Q") && Target is Obj_AI_Hero && Q.Cast(PacketCast) && Player.IssueOrder(GameObjectOrder.AttackUnit, Target))
            {
                return;
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && GetValue<bool>("Clear", "Q") && GetValue<StringList>("Clear", "QMode").SelectedIndex == 0 && Target is Obj_AI_Minion && Q.Cast(PacketCast) && Player.IssueOrder(GameObjectOrder.AttackUnit, Target)) return;
        }

        private void NormalCombo(string Mode)
        {
            if (Mode == "Combo")
            {
                if (GetValue<bool>(Mode, "R") && R.IsReady())
                {
                    var Target = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsValidTarget(R.Range) && GetValue<bool>("Killable", i.ChampionName) && CanKill(i, R)).MinOrDefault(i => i.Health);
                    if (Target != null && R.CastOnUnit(Target, PacketCast)) return;
                }
                if (GetValue<bool>(Mode, "Q"))
                {
                    var Target = R.GetTarget(300);
                }
                if (GetValue<bool>(Mode, "W") && W.IsReady() && R.GetTarget(200) != null && Player.HealthPercentage() < GetValue<Slider>(Mode, "WHpU").Value && W.Cast(PacketCast)) return;
            }
            if (GetValue<bool>(Mode, "E") && E.IsReady() && E.GetTarget() != null && !Player.HasBuff("GarenE") && !Player.HasBuff("GarenQ") && E.Cast(PacketCast)) return;
            //if (GetValue<bool>("Clear", "Q"))
            //{
            //    switch (GetValue<StringList>("Clear", "QMode").SelectedIndex)
            //    {
            //        case 0:
            //            if (Q.IsReady() && minionObj.Count(i => Player.Distance(i, true) <= Math.Pow(Orbwalk.GetAutoAttackRange(Player, i) + 40, 2)) == 0 && Q.Cast(PacketCast)) return;
            //            break;
            //        case 1:
            //            if ((Q.IsReady() || Player.HasBuff("GarenQ")) && !Player.HasBuff("GarenE"))
            //            {
            //                var Obj = minionObj.Find(i => Orbwalk.InAutoAttackRange(i) && CanKill(i, Q));
            //                if (Obj != null && Player.IssueOrder(GameObjectOrder.AttackUnit, Obj)) return;
            //            }
            //            break;
            //    }
            //}
            //if (ItemBool(Mode, "Q") && Q.IsReady() && Player.Distance3D(targetObj) <= ((Mode == "Harass") ? Orbwalk.GetAutoAttackRange(Player, targetObj) + 20 : 800) && (Mode == "Harass" || (Mode == "Combo" && !Orbwalk.InAutoAttackRange(targetObj))))
            //{
            //    if (Mode == "Harass")
            //    {
            //        Orbwalk.SetAttack(false);
            //        Player.IssueOrder(GameObjectOrder.AttackUnit, targetObj);
            //        Orbwalk.SetAttack(true);
            //    }
            //    else Q.Cast(PacketCast());
            //}
            //if (ItemBool(Mode, "E") && E.CanCast(targetObj) && !Player.HasBuff("GarenE") && !Player.HasBuff("GarenQBuff")) E.Cast(PacketCast());
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0) return;
            if (GetValue<bool>("Clear", "E") && E.IsReady() && minionObj.Count(i => E.IsInRange(i)) > 0 && !Player.HasBuff("GarenE") && !Player.HasBuff("GarenQ") && E.Cast(PacketCast)) return;
            if (GetValue<bool>("Clear", "Q"))
            {
                switch (GetValue<StringList>("Clear", "QMode").SelectedIndex)
                {
                    case 0:
                        if (Q.IsReady() && minionObj.Count(i => Player.Distance(i, true) <= Math.Pow(Orbwalk.GetAutoAttackRange(Player, i) + 40, 2)) == 0 && Q.Cast(PacketCast)) return;
                        break;
                    case 1:
                        if ((Q.IsReady() || Player.HasBuff("GarenQ")) && !Player.HasBuff("GarenE"))
                        {
                            var Obj = minionObj.Find(i => Orbwalk.InAutoAttackRange(i) && CanKill(i, Q));
                            if (Obj != null && Player.IssueOrder(GameObjectOrder.AttackUnit, Obj)) return;
                        }
                        break;
                }
            }
        }

        private void LastHit()
        {
            if (!GetValue<bool>("Misc", "QLastHit") || !Q.IsReady() || !Player.HasBuff("GarenQ") || Player.HasBuff("GarenE")) return;
            var minionObj = MinionManager.GetMinions(Orbwalk.GetAutoAttackRange() + 50, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FindAll(i => CanKill(i, Q));
            if (minionObj.Count == 0 || Player.IssueOrder(GameObjectOrder.AttackUnit, minionObj.First())) return;
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (Target != null && CastIgnite(Target)) return;
            }
            if (GetValue<bool>("KillSteal", "R") && R.IsReady())
            {
                var Target = R.GetTarget();
                if (Target != null && CanKill(Target, R) && R.CastOnUnit(Target, PacketCast)) return;
            }
        }
    }
}