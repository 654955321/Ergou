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
    internal class Kayle : Helper
    {
        private readonly Dictionary<int, RAntiItem> _rAntiDetected = new Dictionary<int, RAntiItem>();

        public Kayle()
        {
            Q = new Spell(SpellSlot.Q, 650, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 525);
            R = new Spell(SpellSlot.R, 900);

            var champMenu = new Menu("Plugin", Player.ChampionName + "_Plugin");
            {
                var comboMenu = new Menu("连招", "Combo");
                {
                    var healMenu = new Menu("治疗 (W)", "Heal");
                    {
                        foreach (var name in HeroManager.Allies.Select(MenuName))
                        {
                            AddBool(healMenu, name, name, false);
                            AddSlider(healMenu, name + "HpU", "-> 如果血量低于", 40);
                        }
                        comboMenu.AddSubMenu(healMenu);
                    }
                    var saveMenu = new Menu("保护 (R)", "Save");
                    {
                        foreach (var name in HeroManager.Allies.Select(MenuName))
                        {
                            AddBool(saveMenu, name, name, false);
                            AddSlider(saveMenu, name + "HpU", "-> 如果血量低于", 30);
                        }
                        comboMenu.AddSubMenu(saveMenu);
                    }
                    var antiMenu = new Menu("抵御大招 (R)", "Anti");
                    {
                        AddBool(antiMenu, "Fizz", "小鱼人 R", false);
                        AddBool(antiMenu, "Karthus", "死歌 R", false);
                        AddBool(antiMenu, "Vlad", "吸血鬼 R", false);
                        AddBool(antiMenu, "Zed", "劫 R", false);
                        comboMenu.AddSubMenu(antiMenu);
                    }
                    AddBool(comboMenu, "Q", "使用 Q");
                    AddBool(comboMenu, "W", "使用 W");
                    AddBool(comboMenu, "WSpeed", "-> 加速");
                    AddBool(comboMenu, "WHeal", "-> 治疗");
                    AddBool(comboMenu, "E", "使用 E");
                    AddBool(comboMenu, "EAoE", "-> 聚焦目标更多位置AOE输出");
                    AddBool(comboMenu, "R", "使用 R");
                    AddBool(comboMenu, "RSave", "-> 保护");
                    AddList(
                        comboMenu, "RAnti", "-> 抵御致命技能", new[] { "关闭", "保护自己", "保护队友", "同时" }, 3);
                    champMenu.AddSubMenu(comboMenu);
                }
                var harassMenu = new Menu("骚扰", "Harass");
                {
                    AddKeybind(harassMenu, "AutoQ", "自动 Q", "H", KeyBindType.Toggle);
                    AddSlider(harassMenu, "AutoQMpA", "-> 如果魔量高于", 50);
                    AddBool(harassMenu, "Q", "使用 Q");
                    AddBool(harassMenu, "E", "使用 E");
                    champMenu.AddSubMenu(harassMenu);
                }
                var clearMenu = new Menu("清线", "Clear");
                {
                    AddSmiteMob(clearMenu);
                    AddBool(clearMenu, "Q", "使用 Q");
                    AddBool(clearMenu, "E", "使用 E");
                    champMenu.AddSubMenu(clearMenu);
                }
                var lastHitMenu = new Menu("补刀", "LastHit");
                {
                    AddBool(lastHitMenu, "Q", "使用 Q");
                    champMenu.AddSubMenu(lastHitMenu);
                }
                var fleeMenu = new Menu("逃跑", "Flee");
                {
                    AddBool(fleeMenu, "Q", "使用 Q 减速敌人");
                    AddBool(fleeMenu, "W", "使用 W");
                    champMenu.AddSubMenu(fleeMenu);
                }
                var miscMenu = new Menu("杂项", "Misc");
                {
                    var killStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddBool(killStealMenu, "Q", "使用 Q");
                        AddBool(killStealMenu, "Ignite", "使用 点燃");
                        AddBool(killStealMenu, "Smite", "使用 惩戒");
                        miscMenu.AddSubMenu(killStealMenu);
                    }
                    champMenu.AddSubMenu(miscMenu);
                }
                var drawMenu = new Menu("显示", "Draw");
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
            AttackableUnit.OnDamage += OnDamage;
        }

        private bool HaveE
        {
            get { return Player.HasBuff("JudicatorRighteousFury"); }
        }

        private void OnUpdate(EventArgs args)
        {
            AntiDetect();
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
            if (GetValue<bool>("Draw", "R") && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
            }
        }

        private void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (Orbwalk.CurrentMode != Orbwalker.Mode.Combo)
            {
                return;
            }
            if (GetValue<bool>("Combo", "W") && GetValue<bool>("Combo", "WHeal") && W.IsReady())
            {
                var obj = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.TargetNetworkId);
                if (obj.IsValidTarget(W.Range, false) && obj.IsAlly && GetValue<bool>("Heal", MenuName(obj)) &&
                    obj.HealthPercent < GetValue<Slider>("Heal", MenuName(obj) + "HpU").Value && !obj.InFountain() &&
                    !obj.HasBuff("JudicatorIntervention") && !obj.HasBuff("UndyingRage") &&
                    W.CastOnUnit(obj, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("Combo", "R") && GetValue<bool>("Combo", "RSave") && R.IsReady())
            {
                var obj = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.TargetNetworkId);
                if (obj.IsValidTarget(R.Range, false) && obj.IsAlly && GetValue<bool>("Save", MenuName(obj)) &&
                    obj.HealthPercent < GetValue<Slider>("Save", MenuName(obj) + "HpU").Value && !obj.InFountain() &&
                    !obj.HasBuff("UndyingRage"))
                {
                    R.CastOnUnit(obj, PacketCast);
                }
            }
        }

        private void Fight(string mode)
        {
            if (mode == "Combo" && GetValue<bool>(mode, "E") && GetValue<bool>(mode, "EAoE") && HaveE)
            {
                var target =
                    HeroManager.Enemies.Where(i => Orbwalk.InAutoAttackRange(i))
                        .MaxOrDefault(i => i.CountEnemiesInRange(150));
                if (target != null)
                {
                    Orbwalk.ForcedTarget = target;
                }
            }
            else
            {
                Orbwalk.ForcedTarget = null;
            }
            if (GetValue<bool>(mode, "Q"))
            {
                var target = Q.GetTarget();
                if (target != null &&
                    ((Player.Distance(target) > Q.Range - 100 && !target.IsFacing(Player) && Player.IsFacing(target)) ||
                     target.HealthPercent > 60 || Player.CountEnemiesInRange(Q.Range) == 1) &&
                    Q.CastOnUnit(target, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>(mode, "E") && E.IsReady() && E.GetTarget() != null && E.Cast(PacketCast))
            {
                return;
            }
            if (mode != "Combo")
            {
                return;
            }
            if (GetValue<bool>(mode, "W") && GetValue<bool>(mode, "WSpeed") && W.IsReady())
            {
                var target = Q.GetTarget(200);
                if (target != null && !target.IsFacing(Player) && (!HaveE || !Orbwalk.InAutoAttackRange(target)) &&
                    (!GetValue<bool>(mode, "Q") || (Q.IsReady() && !Q.IsInRange(target))) && W.Cast(PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>(mode, "R") && GetValue<StringList>(mode, "RAnti").SelectedIndex > 0 && R.IsReady())
            {
                var obj =
                    HeroManager.Allies.Where(
                        i =>
                            i.IsValidTarget(R.Range, false) && _rAntiDetected.ContainsKey(i.NetworkId) &&
                            Game.Time > _rAntiDetected[i.NetworkId].StartTick && !i.HasBuff("UndyingRage"))
                        .MinOrDefault(i => i.Health);
                if (obj != null)
                {
                    R.CastOnUnit(obj, PacketCast);
                }
            }
        }

        private void Clear()
        {
            SmiteMob();
            var minionObj = MinionManager.GetMinions(
                Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (!minionObj.Any())
            {
                return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var obj = minionObj.Cast<Obj_AI_Minion>().FirstOrDefault(i => Q.IsKillable(i)) ??
                          minionObj.FirstOrDefault(i => i.MaxHealth >= 1200);
                if (obj != null && Q.CastOnUnit(obj, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("Clear", "E") && E.IsReady() &&
                (minionObj.Count > 1 || minionObj.Any(i => i.MaxHealth >= 1200)))
            {
                E.Cast(PacketCast);
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
            Q.CastOnUnit(obj, PacketCast);
        }

        private void Flee()
        {
            if (GetValue<bool>("Flee", "W") && W.IsReady() && W.Cast(PacketCast))
            {
                return;
            }
            if (GetValue<bool>("Flee", "Q"))
            {
                Q.CastOnBestTarget(0, PacketCast);
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
                if (target != null && Q.IsKillable(target))
                {
                    Q.CastOnUnit(target, PacketCast);
                }
            }
        }

        private void AntiDetect()
        {
            if (Player.IsDead || GetValue<StringList>("Combo", "RAnti").SelectedIndex == 0 || R.Level == 0)
            {
                return;
            }
            var key =
                HeroManager.Allies.FirstOrDefault(
                    i => _rAntiDetected.ContainsKey(i.NetworkId) && Game.Time > _rAntiDetected[i.NetworkId].EndTick);
            if (key != null)
            {
                _rAntiDetected.Remove(key.NetworkId);
            }
            foreach (var obj in
                HeroManager.Allies.Where(i => !i.IsDead && !_rAntiDetected.ContainsKey(i.NetworkId)))
            {
                if ((GetValue<StringList>("Combo", "RAnti").SelectedIndex == 1 && obj.IsMe) ||
                    (GetValue<StringList>("Combo", "RAnti").SelectedIndex == 2 && !obj.IsMe) ||
                    GetValue<StringList>("Combo", "RAnti").SelectedIndex == 3)
                {
                    foreach (var buff in obj.Buffs)
                    {
                        if ((buff.DisplayName == "ZedUltExecute" && GetValue<bool>("Anti", "Zed")) ||
                            (buff.DisplayName == "FizzChurnTheWatersCling" && GetValue<bool>("Anti", "Fizz")) ||
                            (buff.DisplayName == "VladimirHemoplagueDebuff" && GetValue<bool>("Anti", "Vlad")))
                        {
                            _rAntiDetected.Add(obj.NetworkId, new RAntiItem(buff));
                        }
                        else if (buff.DisplayName == "KarthusFallenOne" && GetValue<bool>("Anti", "Karthus") &&
                                 obj.Health <=
                                 ((Obj_AI_Hero) buff.Caster).GetSpellDamage(obj, SpellSlot.R) + obj.Health * 0.2f &&
                                 obj.CountEnemiesInRange(R.Range) > 0)
                        {
                            _rAntiDetected.Add(obj.NetworkId, new RAntiItem(buff));
                        }
                    }
                }
            }
        }

        private string MenuName(Obj_AI_Hero obj)
        {
            return obj.IsMe ? "Self" : obj.ChampionName;
        }

        private class RAntiItem
        {
            public readonly float EndTick;
            public readonly float StartTick;

            public RAntiItem(BuffInstance buff)
            {
                StartTick = Game.Time + (buff.EndTime - buff.StartTime) - (R.Level * 0.5f + 1);
                EndTick = Game.Time + (buff.EndTime - buff.StartTime);
            }
        }
    }
}