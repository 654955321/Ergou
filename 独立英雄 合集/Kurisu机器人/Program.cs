using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuBlitz
{
    //   _____       _    _____           _ 
    //  |   __|___ _| |  |  |  |___ ___ _| |
    //  |  |  | . | . |  |     | .'|   | . |
    //  |_____|___|___|  |__|__|__,|_|_|___|
    //  Copyright © Kurisu Solutions 2014          

    internal class Program
    {
        private static Spell q;
        private static Spell e;
        private static Spell r;

        private static Menu _menu;
        private static Obj_AI_Hero _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static  Obj_AI_Hero _player = ObjectManager.Player;

        static void Main(string[] args)
        {
            Console.WriteLine("Blitzcrank injected...");
            CustomEvents.Game.OnGameLoad += BlitzOnLoad;
        }

        private static void BlitzOnLoad(EventArgs args)
        {
            if (_player.ChampionName != "Blitzcrank")
            {
                return;
            }

            // Set spells      
            q = new Spell(SpellSlot.Q, 1050);
            q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            e = new Spell(SpellSlot.E, _player.AttackRange);
            r = new Spell(SpellSlot.R, 550f);

            // Load Menu
            BlitzMenu();

            // Load Drawings
            Drawing.OnDraw += BlitzOnDraw;

            // OnUpdate
            Game.OnGameUpdate += BlitzOnUpdate;

            // Interrupter
            Interrupter.OnPossibleToInterrupt += BlitzOnInterrupt;

        }

        private static void BlitzOnInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_menu.Item("interrupt").GetValue<bool>())
            {
                if (unit.Distance(_player.Position) < q.Range)
                    q.Cast(unit);
                else if (unit.Distance(_player.Position) < r.Range)
                    r.Cast();
            }
        }

        private static void BlitzOnDraw(EventArgs args)
        {
            if (!_player.IsDead)
            {
                var rcircle = _menu.SubMenu("drawings").Item("drawR").GetValue<Circle>();
                var qcircle = _menu.SubMenu("drawings").Item("drawQ").GetValue<Circle>();

                if (qcircle.Active)
                    Render.Circle.DrawCircle(_player.Position, q.Range, qcircle.Color);

                if (rcircle.Active)
                    Render.Circle.DrawCircle(_player.Position, r.Range, qcircle.Color);
            }

            if (_target.IsValidTarget(q.Range * 2))
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red, 10);
        }


        private static void BlitzOnUpdate(EventArgs args)
        {

            _target = TargetSelector.GetSelectedTarget()
                ?? TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

            // do KS
            GodKS(q);
            GodKS(r);
            GodKS(e);

            var actualHealthSetting = _menu.Item("hneeded").GetValue<Slider>().Value;
            var actualHealthPercent = (int)(_player.Health / _player.MaxHealth * 100);

            if (actualHealthPercent < actualHealthSetting)
            {
                return;
            }

            // use the god hand
            GodHand(_target);

            // powerfist that hoe
            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(_player.AttackRange)))
            {
                if (_menu.Item("useE").GetValue<bool>() && !q.IsReady())
                    e.CastOnUnit(_player);
            }

        }

        private static void GodHand(Obj_AI_Base target)
        {
            if (TargetSelector.GetSelectedTarget() != null && _target.Distance(_player.Position) > 1000)
                return;

            if (target.IsValidTarget() && q.IsReady())
            {
                var prediction = q.GetPrediction(target);
                if (_menu.Item("combokey").GetValue<KeyBind>().Active)
                {
                    if ((target.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                        && (target.Distance(_player.Position) < _menu.Item("dneeded2").GetValue<Slider>().Value))
                    {
                        if (_menu.Item("dograb" + target.SkinName).GetValue<StringList>().SelectedIndex == 0)
                            return;

                        if (prediction.Hitchance >= HitChance.High &&
                            _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 2)
                        {
                            q.Cast(prediction.CastPosition);
                        }

                        else if (prediction.Hitchance >= HitChance.Medium &&
                                 _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 1)
                        {
                            q.Cast(prediction.CastPosition);
                        }

                        else if (prediction.Hitchance >= HitChance.Low &&
                                 _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 0)
                        {
                            q.Cast(prediction.CastPosition);
                        }
                    }
                }
            }

            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e => e.IsValidTarget(q.Range) &&
                                _menu.Item("dograb" + e.SkinName).GetValue<StringList>().SelectedIndex == 2))
            {
                if (e.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                {
                    var prediction = q.GetPrediction(e);
                    if (prediction.Hitchance == HitChance.Immobile &&
                        _menu.Item("immobile").GetValue<bool>())
                    {
                        q.Cast(prediction.CastPosition);
                    }

                    if (prediction.Hitchance == HitChance.Dashing &&
                        _menu.Item("dashing").GetValue<bool>())
                    {
                        q.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        private static void GodKS(Spell spell)
        {
            if (_menu.Item("killsteal" + spell.Slot).GetValue<bool>() && spell.IsReady())
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(e => e.IsValidTarget(spell.Range)))
                {
                    var ksDmg = _player.GetSpellDamage(enemy, spell.Slot);
                    if (ksDmg > enemy.Health)
                    {
                        if (spell.Slot.ToString() == "Q")
                        {
                            var po = spell.GetPrediction(enemy);
                            if (po.Hitchance >= HitChance.Medium)
                                spell.Cast(po.CastPosition);
                        }

                        else
                        {
                            spell.CastOnUnit(_player);
                        }
                    }
                }
            }
        }

        private static void BlitzMenu()
        {
            _menu = new Menu("花边汉化-Kurisu机器人", "blitz", true);

            var blitzOrb = new Menu("走 砍", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(blitzOrb);
            _menu.AddSubMenu(blitzOrb);

            var blitzTS = new Menu("目标 选择", "tselect");
            TargetSelector.AddToMenu(blitzTS);
            _menu.AddSubMenu(blitzTS);

            var menuD = new Menu("范 围", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "Q 范围")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawR", "R 范围")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            _menu.AddSubMenu(menuD);

            var menuG = new Menu("神 Q", "autograb");
            menuG.AddItem(new MenuItem("hitchance", "命中率"))
                .SetValue(new StringList(new[] { "低", "中", "高" }, 2));
            menuG.AddItem(new MenuItem("dneeded", "使用 Q丨最小距离")).SetValue(new Slider(255, 0, (int)q.Range));
            menuG.AddItem(new MenuItem("dneeded2", "使用 Q丨最大距离")).SetValue(new Slider((int)q.Range, 0, (int)q.Range));
            menuG.AddItem(new MenuItem("dashing", "自动Q丨移动的目标")).SetValue(true);
            menuG.AddItem(new MenuItem("immobile", "自动Q丨停止的目标")).SetValue(true);
            menuG.AddItem(new MenuItem("hneeded", "禁用Q|生命值低于 %")).SetValue(new Slider(0));
            menuG.AddItem(new MenuItem("sep", "花边汉化-Kurisu机器人"));

            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.Team != _player.Team))
            {
                menuG.AddItem(new MenuItem("dograb" + e.SkinName, e.SkinName))
                    .SetValue(new StringList(new[] { "禁用 Q ", "正常 Q ", "自动 Q" }, 1));
            }

            _menu.AddSubMenu(menuG);

            var menuK = new Menu("抢 人头", "blitzks");
            menuK.AddItem(new MenuItem("killstealQ", "使用 Q")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealE", "使用 E")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealR", "使用 R")).SetValue(false);
            _menu.AddSubMenu(menuK);

            _menu.AddItem(new MenuItem("interrupt", "打断 法术")).SetValue(true);
            _menu.AddItem(new MenuItem("useE", "Q过来 自动E")).SetValue(true);
            _menu.AddItem(new MenuItem("combokey", "连招 按键")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddToMainMenu();
        }
    }
}
