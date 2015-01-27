using System;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    class Lucian : Common.Helper
    {
        private bool QCasted = false, WCasted = false, ECasted = false;
        private Obj_AI_Hero RTarget = null;
        private Vector3 REndPos = default(Vector3);
        private bool RKillable = false;

        public Lucian()
        {
            Q = new Spell(SpellSlot.Q, 630);
            Q2 = new Spell(SpellSlot.Q, 1300);
            W = new Spell(SpellSlot.W, 1080);
            E = new Spell(SpellSlot.E, 445);
            R = new Spell(SpellSlot.R, 1460);
            Q.SetTargetted(0.5f, float.MaxValue);
            Q2.SetSkillshot(0.5f, 65, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0, 80, 500, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0, 60, 500, true, SkillshotType.SkillshotLine);

            var ChampMenu = new Menu("脚本插件", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    AddItem(ComboMenu, "P", "使用被动");
                    AddItem(ComboMenu, "PSave", "-> 总是储存", false);
                    AddItem(ComboMenu, "Q", "Use Q");
                    AddItem(ComboMenu, "QExtend", "-> 扩大");
                    AddItem(ComboMenu, "W", "Use W");
                    AddItem(ComboMenu, "WPred", "-> 预判", false);
                    AddItem(ComboMenu, "E", "Use E");
                    AddItem(ComboMenu, "EGap", "-> 突击者");
                    AddItem(ComboMenu, "EDelay", "-> 停止使用Q/W 将使用E (毫秒)", 500, 0, 1000);
                    AddItem(ComboMenu, "EMode", "-> 模式", new[] { "安全位置", "鼠标", "追击" });
                    AddItem(ComboMenu, "EModeKey", "--> 键位开关", "Z", KeyBindType.Toggle).ValueChanged += ComboEModeChanged;
                    AddItem(ComboMenu, "EModeDraw", "--> 范围文本", false);
                    AddItem(ComboMenu, "R", "如果能击杀使用R");
                    AddItem(ComboMenu, "RItem", "-> 使用幽梦之灵追加伤害");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    AddItem(HarassMenu, "AutoQ", "使用 Q 扩大", "H", KeyBindType.Toggle);
                    AddItem(HarassMenu, "AutoQMpA", "-> 如果蓝量低于", 50);
                    AddItem(HarassMenu, "P", "使用被动");
                    AddItem(HarassMenu, "PSave", "-> 总是储存", false);
                    AddItem(HarassMenu, "Q", "使用 Q");
                    AddItem(HarassMenu, "W", "使用 W");
                    AddItem(HarassMenu, "E", "使用 E");
                    AddItem(HarassMenu, "EHpA", "-> 如果血量低于", 20);
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    AddItem(ClearMenu, "Q", "使用 Q");
                    AddItem(ClearMenu, "W", "使用 W");
                    AddItem(ClearMenu, "E", "使用 E");
                    AddItem(ClearMenu, "EDelay", "-> 停止使用Q/W 将使用E (ms)", 500, 0, 1000);
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var FleeMenu = new Menu("逃跑", "Flee");
                {
                    AddItem(FleeMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(FleeMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    var KillStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddItem(KillStealMenu, "RStop", "如果能击杀停止使用R");
                        AddItem(KillStealMenu, "Q", "使用 Q");
                        AddItem(KillStealMenu, "W", "使用 W");
                        AddItem(KillStealMenu, "Ignite", "使用 点燃");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    AddItem(MiscMenu, "LockR", "R将目标锁定");
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
            Obj_AI_Hero.OnProcessSpellCast += OnProcessSpellCast;
            Spellbook.OnCastSpell += OnCastSpell;
            Orbwalk.AfterAttack += AfterAttack;
        }

        private void ComboEModeChanged(object sender, OnValueChangeEventArgs e)
        {
            var Mode = GetValue<StringList>("Combo", "EMode").SelectedIndex;
            GetItem("Combo", "EMode").SetValue(new StringList(GetValue<StringList>("Combo", "EMode").SList, Mode == 2 ? 0 : Mode += 1));
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling()) return;
            KillSteal();
            if (Player.IsChannelingImportantSpell())
            {
                if (GetValue<bool>("Misc", "LockR")) LockROnTarget();
                return;
            }
            else
            {
                RTarget = null;
                REndPos = default(Vector3);
                RKillable = false;
            }
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && GetValue<bool>("Flee", "E") && E.IsReady() && E.Cast(Player.ServerPosition.Extend(Game.CursorPos, E.Range), PacketCast)) return;
            if (GetValue<KeyBind>("Harass", "AutoQ").Active) AutoQ();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (GetValue<bool>("Combo", "E") && GetValue<bool>("Combo", "EModeDraw"))
            {
                var Pos = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(Pos.X, Pos.Y, Color.Orange, GetValue<StringList>("Combo", "EMode").SelectedValue);
            }
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "W") && W.Level > 0) Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "E") && E.Level > 0) Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "R") && R.Level > 0) Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name == "LucianQ")
            {
                QCasted = true;
                Utility.DelayAction.Add(500, () => QCasted = false);
            }
            if (args.SData.Name == "LucianW")
            {
                WCasted = true;
                Utility.DelayAction.Add(500, () => WCasted = false);
            }
            if (args.SData.Name == "LucianE")
            {
                ECasted = true;
                Utility.DelayAction.Add(500, () => ECasted = false);
            }
            if (args.SData.Name == "LucianR" && !RKillable) REndPos = (Player.ServerPosition - (Player.ServerPosition.To2D() + R.Range * Player.Direction.To2D().Perpendicular()).To3D()).Normalized();
        }

        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe) return;
            if (args.Slot == SpellSlot.W && Player.IsDashing()) args.Process = false;
        }

        private void AfterAttack(AttackableUnit Target)
        {
            if (!E.IsReady() || !Target.IsValidTarget()) return;
            if ((Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && GetValue<bool>("Clear", "E") && !HavePassive() && Target is Obj_AI_Minion) || ((Orbwalk.CurrentMode == Orbwalk.Mode.Combo || (Orbwalk.CurrentMode == Orbwalk.Mode.Harass && Player.HealthPercentage() >= GetValue<Slider>("Harass", "EHpA").Value)) && GetValue<bool>(Orbwalk.CurrentMode.ToString(), "E") && !HavePassive(Orbwalk.CurrentMode.ToString()) && Target is Obj_AI_Hero))
            {
                if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear || Orbwalk.CurrentMode == Orbwalk.Mode.Harass || (Orbwalk.CurrentMode == Orbwalk.Mode.Combo && GetValue<StringList>("Combo", "EMode").SelectedIndex == 0))
                {
                    var Pos = Geometry.CircleCircleIntersection(Player.ServerPosition.To2D(), ((Obj_AI_Base)Target).ServerPosition.To2D(), E.Range, Orbwalk.GetAutoAttackRange(Player, Target));
                    if (Pos.Count() > 0)
                    {
                        if (E.Cast(Pos.MinOrDefault(i => i.Distance(Game.CursorPos)), PacketCast)) return;
                    }
                    else if (E.Cast(Player.ServerPosition.Extend(((Obj_AI_Base)Target).ServerPosition, -E.Range), PacketCast)) return;
                }
                else if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo)
                {
                    switch (GetValue<StringList>("Combo", "EMode").SelectedIndex)
                    {
                        case 1:
                            if (E.Cast(Player.ServerPosition.Extend(Game.CursorPos, E.Range), PacketCast)) return;
                            break;
                        case 2:
                            if (E.Cast(((Obj_AI_Base)Target).ServerPosition, PacketCast)) return;
                            break;
                    }
                }
            }
        }

        private void NormalCombo(string Mode)
        {
            if (Mode == "Combo" && GetValue<bool>(Mode, "R") && R.IsReady())
            {
                var Target = R.GetTarget();
                if (Target != null && CanKill(Target, R, GetRDmg(Target)))
                {
                    if (Player.Distance(Target, true) > Math.Pow(550, 2) || (!Orbwalk.InAutoAttackRange(Target) && Player.Distance(Target, true) <= Math.Pow(550, 2) && (!GetValue<bool>(Mode, "Q") || (GetValue<bool>(Mode, "Q") && !Q.IsReady())) && (!GetValue<bool>(Mode, "W") || (GetValue<bool>(Mode, "W") && !W.IsReady())) && (!GetValue<bool>(Mode, "E") || (GetValue<bool>(Mode, "E") && !E.IsReady()))))
                    {
                        if (R.CastIfHitchanceEquals(Target, HitChance.Medium, PacketCast))
                        {
                            RTarget = Target;
                            REndPos = (Player.ServerPosition - Target.ServerPosition).Normalized();
                            RKillable = true;
                            if (GetValue<bool>(Mode, "RItem") && Youmuu.IsReady()) Utility.DelayAction.Add(10, () => Youmuu.Cast());
                            return;
                        }
                    }
                }
            }
            if (Mode == "Combo" && GetValue<bool>(Mode, "E") && GetValue<bool>(Mode, "EGap") && E.IsReady())
            {
                var Target = E.GetTarget(Orbwalk.GetAutoAttackRange());
                if (Target != null && !Orbwalk.InAutoAttackRange(Target) && Target.Distance(Player.ServerPosition.Extend(Game.CursorPos, E.Range)) + 20 <= Orbwalk.GetAutoAttackRange(Player, Target) && E.Cast(Player.ServerPosition.Extend(Game.CursorPos, E.Range), PacketCast)) return;
            }
            if (HavePassive(Mode) && GetValue<bool>(Mode, "PSave")) return;
            if (!GetValue<bool>(Mode, "E") || (GetValue<bool>(Mode, "E") && !E.IsReady()))
            {
                if (Mode == "Combo" && GetValue<bool>(Mode, "E") && E.IsReady(GetValue<Slider>(Mode, "EDelay").Value)) return;
                if (GetValue<bool>(Mode, "Q") && Q.IsReady())
                {
                    var Target = Q.GetTarget();
                    if (Target == null) Target = Q2.GetTarget();
                    if (Target != null)
                    {
                        if (((Orbwalk.InAutoAttackRange(Target) && !HavePassive(Mode)) || (Player.Distance(Target, true) > Math.Pow(Orbwalk.GetAutoAttackRange(Player, Target) + 20, 2) && Q.IsInRange(Target))) && Q.CastOnUnit(Target, PacketCast) && Player.IssueOrder(GameObjectOrder.AttackUnit, Target))
                        {
                            return;
                        }
                        else if ((Mode == "Harass" || (Mode == "Combo" && GetValue<bool>(Mode, "QExtend"))) && !Q.IsInRange(Target) && CastExtendQ(Target)) return;
                    }
                }
                if ((!GetValue<bool>(Mode, "Q") || (GetValue<bool>(Mode, "Q") && !Q.IsReady())) && GetValue<bool>(Mode, "W") && W.IsReady() && !Player.IsDashing())
                {
                    var Target = W.GetTarget();
                    if (Target != null && ((Orbwalk.InAutoAttackRange(Target) && !HavePassive(Mode)) || (Player.Distance(Target, true) > Math.Pow(Orbwalk.GetAutoAttackRange(Player, Target) + 20, 2))))
                    {
                        if ((Mode == "Harass" || (Mode == "Combo" && GetValue<bool>(Mode, "WPred"))) && W.CastIfHitchanceEquals(Target, HitChance.Medium, PacketCast))
                        {
                            return;
                        }
                        else if (Mode == "Combo" && !GetValue<bool>(Mode, "WPred") && W.Cast(W.GetPrediction(Target).CastPosition, PacketCast)) return;
                    }
                }
            }
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0) return;
            if (!GetValue<bool>("Clear", "E") || (GetValue<bool>("Clear", "E") && !E.IsReady()))
            {
                if (GetValue<bool>("Clear", "E") && E.IsReady(GetValue<Slider>("Clear", "EDelay").Value)) return;
                if (GetValue<bool>("Clear", "W") && W.IsReady() && !HavePassive() && !Player.IsDashing())
                {
                    var Pos = W.GetCircularFarmLocation(minionObj.FindAll(i => W.IsInRange(i)));
                    if (Pos.MinionsHit > 1)
                    {
                        if (Pos.Position.IsValid() && W.Cast(Pos.Position, PacketCast)) return;
                    }
                    else
                    {
                        var Obj = minionObj.Find(i => i.MaxHealth >= 1200);
                        if (Obj != null && W.IsInRange(Obj) && W.CastIfHitchanceEquals(Obj, HitChance.Medium, PacketCast)) return;
                    }
                }
                if ((!GetValue<bool>("Clear", "W") || (GetValue<bool>("Clear", "W") && !W.IsReady())) && GetValue<bool>("Clear", "Q") && Q.IsReady() && !HavePassive())
                {
                    var Pos = Q2.GetLineFarmLocation(minionObj);
                    if (Pos.MinionsHit > 0 && Pos.Position.IsValid())
                    {
                        var Obj = minionObj.Find(i => Q.IsInRange(i) && Q2.WillHit(i, Pos.Position.To3D().Extend(Player.ServerPosition, -Q2.Range), (int)(i.BoundingRadius / 4)));
                        if (Obj != null && Q.CastOnUnit(Obj, PacketCast) && Player.IssueOrder(GameObjectOrder.AttackUnit, Obj)) return;
                    }
                }
            }
        }

        private void AutoQ()
        {
            if (!Q.IsReady() || Player.ManaPercentage() < GetValue<Slider>("Harass", "AutoQMpA").Value) return;
            var Target = Q2.GetTarget();
            if (Target != null && !Q.IsInRange(Target) && CastExtendQ(Target)) return;
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (Target != null && CastIgnite(Target)) return;
            }
            if (Player.IsDashing() || (!GetValue<bool>("KillSteal", "RStop") && Player.IsChannelingImportantSpell())) return;
            var CancelR = GetValue<bool>("KillSteal", "RStop") && Player.IsChannelingImportantSpell();
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var Target = Q.GetTarget();
                if (Target == null) Target = Q2.GetTarget();
                if (Target != null && CanKill(Target, Q))
                {
                    if (Q.IsInRange(Target))
                    {
                        if ((!CancelR || (CancelR && R.Cast(PacketCast))) && Q.CastOnUnit(Target, PacketCast)) return;
                    }
                    else if (CastExtendQ(Target, CancelR)) return;
                }
            }
            if (GetValue<bool>("KillSteal", "W") && W.IsReady() && !Player.IsDashing())
            {
                var Target = W.GetTarget();
                if (Target != null && CanKill(Target, W) && (!CancelR || (CancelR && R.Cast(PacketCast))) && W.CastIfHitchanceEquals(Target, HitChance.Medium, PacketCast)) return;
            }
        }

        private void LockROnTarget()
        {
            var Target = RTarget.IsValidTarget() ? RTarget : R.GetTarget();
            if (Target == null || REndPos == default(Vector3)) return;
            var Pos = R.GetPrediction(Target).CastPosition;
            var FullPoint = new Vector2(Pos.X + REndPos.X * R.Range * 0.98f, Pos.Y + REndPos.Y * R.Range * 0.98f).To3D();
            //var MidPoint = new Vector2((FullPoint.X * 2 - Pos.X) / Pos.Distance(FullPoint) * R.Range * 0.98f, (FullPoint.Y * 2 - Pos.Y) / Pos.Distance(FullPoint) * R.Range * 0.98f).To3D();
            var ClosestPoint = Player.ServerPosition.To2D().Closest(new List<Vector3> { Pos, FullPoint }.To2D()).To3D();
            if (ClosestPoint.IsValid() && !ClosestPoint.IsWall() && Pos.Distance(ClosestPoint) > E.Range)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, ClosestPoint);
            }
            else if (FullPoint.IsValid() && !FullPoint.IsWall() && Pos.Distance(FullPoint) < R.Range && Pos.Distance(FullPoint) > 100)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, FullPoint);
            }
            //else if (MidPoint.IsValid() && !MidPoint.IsWall()) Player.IssueOrder(GameObjectOrder.MoveTo, MidPoint);
        }

        private bool CastExtendQ(Obj_AI_Hero Target, bool CancelR = false)
        {
            var Obj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly).Find(i => Q2.WillHit(Target, i.ServerPosition.Extend(Player.ServerPosition, -Q2.Range), (int)(Target.BoundingRadius / 4)));
            if (Obj != null && (!CancelR || (CancelR && R.Cast(PacketCast))) && Q.CastOnUnit(Obj, PacketCast)) return true;
            return false;
        }

        private bool HavePassive(string Mode = "Clear")
        {
            if (Mode != "Clear" && !GetValue<bool>(Mode, "P")) return false;
            if (QCasted || WCasted || ECasted || Player.HasBuff("LucianPassiveBuff")) return true;
            return false;
        }

        private double GetRDmg(Obj_AI_Hero Target)
        {
            var Shot = (int)(7.5 + new double[] { 7.5, 9, 10.5 }[R.Level - 1] * 1 / Player.AttackDelay);
            var MaxShot = new int[] { 26, 30, 33 }[R.Level - 1];
            return Player.CalcDamage(Target, Damage.DamageType.Physical, (new double[] { 40, 50, 60 }[R.Level - 1] + 0.25 * Player.FlatPhysicalDamageMod + 0.1 * Player.FlatMagicDamageMod) * (Shot > MaxShot ? MaxShot : Shot));
        }
    }
}