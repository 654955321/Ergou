﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrianSharp.Common;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    internal class JarvanIV : Helper
    {
        private const int RWidth = 325;
        private bool _rCasted;

        public JarvanIV()
        {
            Q = new Spell(SpellSlot.Q, 770);
            Q2 = new Spell(SpellSlot.Q, 880);
            W = new Spell(SpellSlot.W, 520);
            E = new Spell(SpellSlot.E, 860, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 650);
            Q.SetSkillshot(0.6f, 70, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 180, 1450, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 175, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var champMenu = new Menu("Plugin", Player.ChampionName + "_Plugin");
            {
                var comboMenu = new Menu("连招", "Combo");
                {
                    AddBool(comboMenu, "Q", "使用 Q");
                    AddSlider(comboMenu, "QFlagRange", "-> 范围设置", 500, 100, 880);
                    AddBool(comboMenu, "W", "使用 W");
                    AddSlider(comboMenu, "WHpU", "-> 自己血量低于", 40);
                    AddSlider(comboMenu, "WCountA", "-> 敌人数量大于", 2, 1, 5);
                    AddBool(comboMenu, "E", "使用 E");
                    AddBool(comboMenu, "EQ", "-> 保留E为EQ二连");
                    AddBool(comboMenu, "R", "使用 R");
                    AddSlider(comboMenu, "RHpU", "-> 敌人血量低于", 40);
                    AddSlider(comboMenu, "RCountA", "-> 敌人数量大于", 2, 1, 5);
                    champMenu.AddSubMenu(comboMenu);
                }
                var harassMenu = new Menu("骚扰", "Harass");
                {
                    AddKeybind(harassMenu, "AutoQ", "自动 Q", "H", KeyBindType.Toggle);
                    AddSlider(harassMenu, "AutoQMpA", "-> 蓝量百分比", 50);
                    AddBool(harassMenu, "Q", "使用 Q");
                    champMenu.AddSubMenu(harassMenu);
                }
                var clearMenu = new Menu("清线", "Clear");
                {
                    AddSmiteMob(clearMenu);
                    AddBool(clearMenu, "Q", "使用 Q");
                    AddBool(clearMenu, "W", "使用 W");
                    AddSlider(clearMenu, "WHpU", "-> 自己血量低于", 40);
                    AddBool(clearMenu, "E", "使用 E");
                    AddBool(clearMenu, "Item", "使用 提亚马特/九头蛇");
                    champMenu.AddSubMenu(clearMenu);
                }
                var lastHitMenu = new Menu("补刀", "LastHit");
                {
                    AddBool(lastHitMenu, "Q", "使用 Q");
                    champMenu.AddSubMenu(lastHitMenu);
                }
                var fleeMenu = new Menu("逃跑", "Flee");
                {
                    AddBool(fleeMenu, "EQ", "使用 EQ");
                    AddBool(fleeMenu, "W", "使用 W 减速敌人");
                    champMenu.AddSubMenu(fleeMenu);
                }
                var miscMenu = new Menu("杂项", "Misc");
                {
                    var killStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddBool(killStealMenu, "Q", "使用 Q");
                        AddBool(killStealMenu, "E", "使用 E");
                        AddBool(killStealMenu, "R", "使用 R");
                        AddBool(killStealMenu, "Ignite", "使用 点燃");
                        AddBool(killStealMenu, "Smite", "使用 惩戒");
                        miscMenu.AddSubMenu(killStealMenu);
                    }
                    var interruptMenu = new Menu("中断法术", "Interrupt");
                    {
                        AddBool(interruptMenu, "EQ", "使用 EQ");
                        foreach (var spell in
                            Interrupter.Spells.Where(
                                i => HeroManager.Enemies.Any(a => i.ChampionName == a.ChampionName)))
                        {
                            AddBool(
                                interruptMenu, spell.ChampionName + "_" + spell.Slot,
                                "-> Skill " + spell.Slot + " Of " + spell.ChampionName);
                        }
                        miscMenu.AddSubMenu(interruptMenu);
                    }
                    champMenu.AddSubMenu(miscMenu);
                }
                var drawMenu = new Menu("显示", "Draw");
                {
                    AddBool(drawMenu, "Q", "Q 范围", false);
                    AddBool(drawMenu, "W", "W 范围", false);
                    AddBool(drawMenu, "E", "E 范围", false);
                    AddBool(drawMenu, "R", "R 范围", false);
                    champMenu.AddSubMenu(drawMenu);
                }
                MainMenu.AddSubMenu(champMenu);
            }
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private IEnumerable<Obj_AI_Minion> Flag
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            i =>
                                i.IsValidTarget(Q2.Range, false) && i.IsAlly && i.Name == "Beacon" &&
                                Player.Distance(i) > 1);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
            {
                return;
            }
            if (R.IsReady() && _rCasted && Player.CountEnemiesInRange(RWidth) == 0 && R.Cast(PacketCast))
            {
                return;
            }
            switch (Orbwalk.CurrentMode)
            {
                case Orbwalker.Mode.Combo:
                    Fight("Combo");
                    break;
                case Orbwalker.Mode.Harass:
                    Fight("Harass");
                    break;
                case Orbwalker.Mode.Clear:
                    Clear();
                    break;
                case Orbwalker.Mode.LastHit:
                    LastHit();
                    break;
                case Orbwalker.Mode.Flee:
                    Flee();
                    break;
            }
            if (GetValue<bool>("SmiteMob", "Auto") && Orbwalk.CurrentMode != Orbwalker.Mode.Clear)
            {
                SmiteMob();
            }
            AutoQ();
            KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            }
            if (GetValue<bool>("Draw", "W") && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            }
            if (GetValue<bool>("Draw", "E") && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            }
            if (GetValue<bool>("Draw", "R") && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
            }
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Player.IsDead || !GetValue<bool>("Interrupt", "EQ") ||
                !GetValue<bool>("Interrupt", unit.ChampionName + "_" + spell.Slot) || !Q.IsReady())
            {
                return;
            }
            if (E.CanCast(unit) && Player.Mana >= Q.Instance.ManaCost + E.Instance.ManaCost)
            {
                var predE = E.GetPrediction(unit);
                if (predE.Hitchance >= HitChance.High &&
                    E.Cast(predE.CastPosition.Extend(Player.ServerPosition, -E.Width), PacketCast) &&
                    Q.Cast(predE.CastPosition, PacketCast))
                {
                    return;
                }
            }
            foreach (var flag in
                Flag.Where(i => unit.Distance(i) <= 60 || Q2.WillHit(unit, i.ServerPosition)))
            {
                Q.Cast(flag.ServerPosition, PacketCast);
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            if (args.SData.Name == "JarvanIVCataclysm")
            {
                _rCasted = true;
                Utility.DelayAction.Add(3500, () => _rCasted = false);
            }
        }

        private void Fight(string mode)
        {
            if (mode == "Combo" && GetValue<bool>(mode, "E") && E.IsReady())
            {
                if (GetValue<bool>(mode, "EQ") &&
                    (Player.Mana < E.Instance.ManaCost + Q.Instance.ManaCost || (!Q.IsReady() && Q.IsReady(4000))))
                {
                    return;
                }
                var target = E.GetTarget(E.Width / 2);
                if (target != null)
                {
                    var predE = E.GetPrediction(target);
                    if (predE.Hitchance >= HitChance.High &&
                        E.Cast(predE.CastPosition.Extend(Player.ServerPosition, -E.Width), PacketCast))
                    {
                        if (GetValue<bool>(mode, "Q") && Q.IsReady())
                        {
                            Q.Cast(predE.CastPosition, PacketCast);
                        }
                        return;
                    }
                }
            }
            if (GetValue<bool>(mode, "Q") && Q.IsReady())
            {
                if (mode == "Combo")
                {
                    if (GetValue<bool>(mode, "E") && Player.Mana >= E.Instance.ManaCost + Q.Instance.ManaCost &&
                        E.IsReady(2000))
                    {
                        return;
                    }
                    var target = Q2.GetTarget();
                    if (GetValue<bool>(mode, "E") && target != null &&
                        Flag.Where(
                            i =>
                                Player.Distance(i) <= GetValue<Slider>(mode, "QFlagRange").Value &&
                                (target.Distance(i) <= 60 || Q2.WillHit(target, i.ServerPosition)))
                            .Any(i => Q.Cast(i.ServerPosition, PacketCast)))
                    {
                        return;
                    }
                }
                if (Q.CastOnBestTarget(0, PacketCast).IsCasted())
                {
                    return;
                }
            }
            if (mode != "Combo")
            {
                return;
            }
            if (GetValue<bool>(mode, "R") && R.IsReady() && !_rCasted)
            {
                var obj = (from i in HeroManager.Enemies.Where(i => i.IsValidTarget(R.Range))
                    let enemy = GetRTarget(i.ServerPosition)
                    where
                        (enemy.Count > 1 && R.IsKillable(i)) ||
                        (enemy.Count > 1 && enemy.Any(a => a.HealthPercent < GetValue<Slider>(mode, "RHpU").Value)) ||
                        enemy.Count >= GetValue<Slider>(mode, "RCountA").Value
                    select i).MaxOrDefault(i => GetRTarget(i.ServerPosition).Count);
                if (obj != null && R.CastOnUnit(obj, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>(mode, "W") && W.IsReady() &&
                Player.CountEnemiesInRange(W.Range) >= GetValue<Slider>(mode, "WCountA").Value &&
                Player.HealthPercent < GetValue<Slider>(mode, "WHpU").Value)
            {
                W.Cast(PacketCast);
            }
        }

        private void Clear()
        {
            SmiteMob();
            if (GetValue<bool>("Clear", "E") && E.IsReady())
            {
                var minionObj = MinionManager.GetMinions(
                    E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
                if (minionObj.Any())
                {
                    var pos = E.GetCircularFarmLocation(minionObj);
                    if (pos.MinionsHit > 1)
                    {
                        if (E.Cast(pos.Position, PacketCast))
                        {
                            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
                            {
                                Q.Cast(pos.Position, PacketCast);
                            }
                            return;
                        }
                    }
                    else
                    {
                        var obj = minionObj.FirstOrDefault(i => i.MaxHealth >= 1200);
                        if (obj != null && E.CastIfHitchanceEquals(obj, HitChance.Medium, PacketCast))
                        {
                            return;
                        }
                    }
                }
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var minionObj = MinionManager.GetMinions(
                    Q2.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
                if (minionObj.Any() &&
                    (!GetValue<bool>("Clear", "E") ||
                     (!E.IsReady() || (E.IsReady() && E.GetCircularFarmLocation(minionObj).MinionsHit == 1))))
                {
                    if (GetValue<bool>("Clear", "E") &&
                        Flag.Where(
                            i =>
                                minionObj.Count(a => a.Distance(i) <= 60) > 1 ||
                                minionObj.Count(a => Q2.WillHit(a, i.ServerPosition)) > 1)
                            .Any(i => Q.Cast(i.ServerPosition, PacketCast)))
                    {
                        return;
                    }
                    var pos = Q.GetLineFarmLocation(minionObj.Where(i => Q.IsInRange(i)).ToList());
                    if (pos.MinionsHit > 0 && Q.Cast(pos.Position, PacketCast))
                    {
                        return;
                    }
                }
            }
            if (GetValue<bool>("Clear", "W") && W.IsReady() &&
                Player.HealthPercent < GetValue<Slider>("Clear", "WHpU").Value &&
                MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly).Any() && W.Cast(PacketCast))
            {
                return;
            }
            if (GetValue<bool>("Clear", "Item") && (Hydra.IsReady() || Tiamat.IsReady()))
            {
                var minionObj = MinionManager.GetMinions(
                    (Hydra.IsReady() ? Hydra : Tiamat).Range, MinionTypes.All, MinionTeam.NotAlly);
                if (minionObj.Count > 2 ||
                    minionObj.Any(
                        i => i.MaxHealth >= 1200 && i.Distance(Player) < (Hydra.IsReady() ? Hydra : Tiamat).Range - 80))
                {
                    if (Tiamat.IsReady())
                    {
                        Tiamat.Cast();
                    }
                    if (Hydra.IsReady())
                    {
                        Hydra.Cast();
                    }
                }
            }
        }

        private void LastHit()
        {
            if (!GetValue<bool>("LastHit", "Q") || !Q.IsReady())
            {
                return;
            }
            var obj =
                MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth)
                    .Cast<Obj_AI_Minion>()
                    .FirstOrDefault(i => Q.IsKillable(i));
            if (obj == null)
            {
                return;
            }
            Q.CastIfHitchanceEquals(obj, HitChance.High, PacketCast);
        }

        private void Flee()
        {
            if (GetValue<bool>("Flee", "EQ") && Q.IsReady() && E.IsReady() &&
                Player.Mana >= Q.Instance.ManaCost + E.Instance.ManaCost && E.Cast(Game.CursorPos, PacketCast) &&
                Q.Cast(Game.CursorPos, PacketCast))
            {
                return;
            }
            if (GetValue<bool>("Flee", "W") && W.IsReady() && !Q.IsReady() && W.GetTarget() != null)
            {
                W.Cast(PacketCast);
            }
        }

        private void AutoQ()
        {
            if (!GetValue<KeyBind>("Harass", "AutoQ").Active ||
                Player.ManaPercent < GetValue<Slider>("Harass", "AutoQMpA").Value)
            {
                return;
            }
            Q.CastOnBestTarget(0, PacketCast);
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (target != null && CastIgnite(target))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "Smite") &&
                (CurrentSmiteType == SmiteType.Blue || CurrentSmiteType == SmiteType.Red))
            {
                var target = TargetSelector.GetTarget(760, TargetSelector.DamageType.True);
                if (target != null && CastSmite(target))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var target = Q.GetTarget();
                if (target != null && Q.IsKillable(target) &&
                    Q.CastIfHitchanceEquals(target, HitChance.High, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "E") && E.IsReady())
            {
                var target = E.GetTarget(E.Width / 2);
                if (target != null && E.IsKillable(target) &&
                    E.CastIfHitchanceEquals(target, HitChance.High, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "R") && R.IsReady())
            {
                var target = R.GetTarget();
                if (target != null && R.IsKillable(target))
                {
                    R.CastOnUnit(target, PacketCast);
                }
            }
        }

        private List<Obj_AI_Hero> GetRTarget(Vector3 pos)
        {
            return
                HeroManager.Enemies.Where(
                    i => i.IsValidTarget() && Prediction.GetPrediction(i, 0.25f).UnitPosition.Distance(pos) <= RWidth)
                    .ToList();
        }
    }
}