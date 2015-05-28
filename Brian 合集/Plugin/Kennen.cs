using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrianSharp.Common;
using LeagueSharp;
using LeagueSharp.Common;
using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    internal class Kennen : Helper
    {
        public Kennen()
        {
            Q = new Spell(SpellSlot.Q, 1050, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 550, TargetSelector.DamageType.Magical);
            Q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);

            var champMenu = new Menu("Plugin", Player.ChampionName + "_Plugin");
            {
                var comboMenu = new Menu("连招", "Combo");
                {
                    AddBool(comboMenu, "Q", "使用 Q");
                    AddBool(comboMenu, "W", "使用 W");
                    AddBool(comboMenu, "R", "使用 R");
                    AddSlider(comboMenu, "RHpU", "-> 如果敌人血量低于", 60);
                    AddSlider(comboMenu, "RCountA", "-> 如果敌人数量大于", 2, 1, 5);
                    AddBool(comboMenu, "RItem", "-> 使用中亚当R启用");
                    AddSlider(comboMenu, "RItemHpU", "--> 如果血量低于", 60);
                    champMenu.AddSubMenu(comboMenu);
                }
                var harassMenu = new Menu("骚扰", "Harass");
                {
                    AddKeybind(harassMenu, "AutoQ", "自动 Q", "H", KeyBindType.Toggle);
                    AddSlider(harassMenu, "AutoQMpA", "-> 如果魔量高于", 50);
                    AddBool(harassMenu, "Q", "使用 Q");
                    AddBool(harassMenu, "W", "使用 W");
                    AddSlider(harassMenu, "WMpA", "-> 如果魔量高于", 50);
                    champMenu.AddSubMenu(harassMenu);
                }
                var clearMenu = new Menu("清线", "Clear");
                {
                    AddBool(clearMenu, "Q", "使用 Q");
                    AddBool(clearMenu, "W", "使用 W");
                    AddSlider(clearMenu, "WHitA", "-> 如果小兵数量大于", 2, 1, 5);
                    champMenu.AddSubMenu(clearMenu);
                }
                var lastHitMenu = new Menu("补刀", "LastHit");
                {
                    AddBool(lastHitMenu, "Q", "使用 Q");
                    champMenu.AddSubMenu(lastHitMenu);
                }
                var fleeMenu = new Menu("逃跑", "Flee");
                {
                    AddBool(fleeMenu, "E", "使用 E");
                    AddBool(fleeMenu, "W", "使用 W 眩晕敌人");
                    champMenu.AddSubMenu(fleeMenu);
                }
                var miscMenu = new Menu("杂项", "Misc");
                {
                    var killStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddBool(killStealMenu, "Q", "使用 Q");
                        AddBool(killStealMenu, "W", "使用 W");
                        AddBool(killStealMenu, "R", "使用 R");
                        AddBool(killStealMenu, "Ignite", "使用 点燃");
                        miscMenu.AddSubMenu(killStealMenu);
                    }
                    var interruptMenu = new Menu("中断技能", "Interrupt");
                    {
                        AddBool(interruptMenu, "W", "使用 W");
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
                var drawMenu = new Menu("范围", "Draw");
                {
                    AddBool(drawMenu, "Q", "Q 范围", false);
                    AddBool(drawMenu, "W", "W 范围", false);
                    AddBool(drawMenu, "R", "R 范围", false);
                    champMenu.AddSubMenu(drawMenu);
                }
                MainMenu.AddSubMenu(champMenu);
            }
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private bool HaveR
        {
            get { return Player.HasBuff("KennenShurikenStorm"); }
        }

        private List<Obj_AI_Hero> GetRTarget
        {
            get
            {
                return
                    HeroManager.Enemies.Where(
                        i =>
                            i.IsValidTarget() &&
                            Player.Distance(Prediction.GetPrediction(i, 0.25f).UnitPosition) <= R.Range).ToList();
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
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
            if (GetValue<bool>("Draw", "R") && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
            }
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Player.IsDead || !GetValue<bool>("Interrupt", "W") ||
                !GetValue<bool>("Interrupt", unit.ChampionName + "_" + spell.Slot) || !W.CanCast(unit) ||
                !HaveW(unit, true))
            {
                return;
            }
            W.Cast(PacketCast);
        }

        private void Fight(string mode)
        {
            if (GetValue<bool>(mode, "Q") && Q.CastOnBestTarget(0, PacketCast).IsCasted())
            {
                return;
            }
            if (GetValue<bool>(mode, "W") && W.IsReady() &&
                HeroManager.Enemies.Any(i => i.IsValidTarget(W.Range) && HaveW(i)) &&
                (mode == "Combo" || Player.ManaPercent >= GetValue<Slider>(mode, "WMpA").Value))
            {
                if (HaveR)
                {
                    var obj = HeroManager.Enemies.Where(i => i.IsValidTarget(W.Range) && HaveW(i)).ToList();
                    if ((obj.Count(i => HaveW(i, true)) > 1 || obj.Any(i => W.IsKillable(i, 1)) || obj.Count > 2 ||
                         (obj.Count(i => HaveW(i, true)) == 1 && obj.Any(i => !HaveW(i, true)))) && W.Cast(PacketCast))
                    {
                        return;
                    }
                }
                else if (W.Cast(PacketCast))
                {
                    return;
                }
            }
            if (mode == "Combo" && GetValue<bool>(mode, "R"))
            {
                if (R.IsReady())
                {
                    var obj = GetRTarget;
                    if ((obj.Count > 1 && obj.Any(i => CanKill(i, GetRDmg(i)))) ||
                        (obj.Count > 1 && obj.Any(i => i.HealthPercent < GetValue<Slider>(mode, "RHpU").Value)) ||
                        obj.Count >= GetValue<Slider>(mode, "RCountA").Value)
                    {
                        R.Cast(PacketCast);
                    }
                }
                else if (HaveR && GetValue<bool>(mode, "RItem") &&
                         Player.HealthPercent < GetValue<Slider>(mode, "RItemHpU").Value && GetRTarget.Count > 0 &&
                         Zhonya.IsReady())
                {
                    Zhonya.Cast();
                }
            }
        }

        private void Clear()
        {
            var minionObj = MinionManager.GetMinions(
                Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (!minionObj.Any())
            {
                return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var list = minionObj.Where(i => Q.GetPrediction(i).Hitchance >= HitChance.Medium).ToList();
                var obj = list.Cast<Obj_AI_Minion>().FirstOrDefault(i => Q.IsKillable(i)) ??
                          list.MaxOrDefault(i => i.Distance(Player));
                if (obj != null && Q.CastIfHitchanceEquals(obj, HitChance.Medium, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("Clear", "W") && W.IsReady() &&
                minionObj.Count(i => W.IsInRange(i) && HaveW(i)) >= GetValue<Slider>("Clear", "WHitA").Value)
            {
                W.Cast(PacketCast);
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
                    .Where(i => Q.GetPrediction(i).Hitchance >= HitChance.High)
                    .FirstOrDefault(i => Q.IsKillable(i));
            if (obj == null)
            {
                return;
            }
            Q.CastIfHitchanceEquals(obj, HitChance.High, PacketCast);
        }

        private void Flee()
        {
            if (GetValue<bool>("Flee", "E") && E.IsReady() && !Player.HasBuff("KennenLightningRush") &&
                E.Cast(PacketCast))
            {
                return;
            }
            if (GetValue<bool>("Flee", "W") && W.IsReady() &&
                HeroManager.Enemies.Any(i => i.IsValidTarget(W.Range) && HaveW(i, true)))
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
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var target = Q.GetTarget();
                if (target != null && Q.IsKillable(target) &&
                    Q.CastIfHitchanceEquals(target, HitChance.High, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "W") && W.IsReady())
            {
                var target = W.GetTarget(0, HeroManager.Enemies.Where(i => !HaveW(i)));
                if (target != null && W.IsKillable(target, 1) && W.Cast(PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "R") && R.IsReady())
            {
                var target = GetRTarget.FirstOrDefault(i => CanKill(i, GetRDmg(i)));
                if (target != null)
                {
                    R.Cast(PacketCast);
                }
            }
        }

        private double GetRDmg(Obj_AI_Hero target)
        {
            return Player.CalcDamage(
                target, Damage.DamageType.Magical,
                (new[] { 80, 145, 210 }[R.Level - 1] + 0.4 * Player.FlatMagicDamageMod) * 3);
        }

        private bool HaveW(Obj_AI_Base target, bool onlyStun = false)
        {
            return target.HasBuff("KennenMarkOfStorm") &&
                   (!onlyStun || target.Buffs.First(i => i.DisplayName == "KennenMarkOfStorm").Count == 2);
        }
    }
}