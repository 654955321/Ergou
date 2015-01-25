#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Karma
{
    internal class Program
    {
        private const string ChampionName = "Karma";

        private static Orbwalking.Orbwalker _orbwalker;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;

        private static Menu _config;

        private static bool MantraIsActive
        {
            get { return ObjectManager.Player.HasBuff("KarmaMantra"); }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName)
            {
                return;
            }

            _q = new Spell(SpellSlot.Q, 1050f);
            _w = new Spell(SpellSlot.W, 700f);
            _e = new Spell(SpellSlot.E, 800f);
            _r = new Spell(SpellSlot.R);

            _q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
            _w.SetTargetted(0.25f, 2200f);
            _e.SetTargetted(0.25f, float.MaxValue);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _config = new Menu("花边汉化-Esk0r卡尔马", ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            _config.AddSubMenu(new Menu("走 砍", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            _config.AddSubMenu(new Menu("连 招", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            _config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "开 启!").SetValue(
                        new KeyBind(_config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("骚 扰", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(false));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "使用 R").SetValue(true));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "开 启!").SetValue(
                        new KeyBind(_config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("杂 项", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEDefense", "使用E 防御").SetValue(true));

            _config.AddSubMenu(new Menu("范 围", "Drawings"));
            _config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("WRange", "W 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("WRootRange", "W 长度 范围").SetValue(new Circle(true, Color.MintCream)));

            _config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));

            _config.AddToMainMenu();
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsValidTarget(300f))
            {
                _e.Cast(ObjectManager.Player);
                _q.Cast(gapcloser.Sender);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = _config.Item("WRootRange").GetValue<Circle>();
            if (menuItem.Active)
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget() && h.HasBuff("KarmaSpiritBind")))
                {
                    var distance = (1 - Math.Min(Math.Max(850 - ObjectManager.Player.Distance(enemy), 0), 450) / 450);
                
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position, 850, Color.FromArgb((int)(50 * distance), menuItem.Color), -420,
                        true);
                        Render.Circle.DrawCircle(
                        ObjectManager.Player.Position, 850, Color.FromArgb((int)(255 * distance), menuItem.Color), 10);
                
                    break;
                }
            }

            foreach (var spell in SpellList)
            {
                menuItem = _config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            _q.Width = MantraIsActive ? 80f : 60f; // Mantra increases the q line width
            _q.Range = MantraIsActive ? 1250f : 1050f;
            if (_config.Item("UseEDefense").GetValue<bool>())
            {
                foreach (var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                hero.IsValidTarget(_e.Range, false) && hero.IsAlly &&
                                ObjectManager.Get<Obj_AI_Hero>().Count(h => h.IsValidTarget() && h.Distance(hero) < 400) >
                                1))
                {
                    _e.Cast(hero);
                }
            }

            if (!_config.Item("ComboActive").GetValue<KeyBind>().Active &&
                !_config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                return;
            }

            if (_config.Item("HarassActive").GetValue<KeyBind>().Active &&
                ObjectManager.Player.Mana - ObjectManager.Player.Spellbook.Spells.First(s => s.Slot == SpellSlot.W).ManaCost -
                ObjectManager.Player.Spellbook.Spells.First(s => s.Slot == SpellSlot.E).ManaCost < 0)
            {
                return;
            }

            
            var qTarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);

            var qActive =
                _config.Item("UseQ" + (_config.Item("ComboActive").GetValue<KeyBind>().Active ? "Combo" : "Harass"))
                    .GetValue<bool>();
            var wActive =
                _config.Item("UseW" + (_config.Item("ComboActive").GetValue<KeyBind>().Active ? "Combo" : "Harass"))
                    .GetValue<bool>();
            var rActive =
                _config.Item("UseR" + (_config.Item("ComboActive").GetValue<KeyBind>().Active ? "Combo" : "Harass"))
                    .GetValue<bool>();

            if (wActive && wTarget != null && _w.IsReady())
            {
                if ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) /
                    (qTarget.Health / qTarget.MaxHealth) < 1)
                {
                    if (rActive)
                    {
                        _r.Cast();
                    }

                    if (!rActive || !_r.IsReady())
                    {
                        _w.Cast(wTarget);
                    }
                }
            }

            if (qActive && qTarget != null && _q.IsReady())
            {
                if (rActive)
                {
                    _r.Cast();
                }

                if (!rActive || !_r.IsReady())
                {
                    var qPrediction = _q.GetPrediction(qTarget);
                    if (qPrediction.Hitchance >= HitChance.High)
                    {
                        _q.Cast(qPrediction.CastPosition);
                    }
                    else if (qPrediction.Hitchance == HitChance.Collision)
                    {
                        var minionsHit = qPrediction.CollisionObjects;
                        var closest =
                            minionsHit.Where(m => m.NetworkId != ObjectManager.Player.NetworkId)
                                .OrderBy(m => m.Distance(ObjectManager.Player))
                                .FirstOrDefault();

                        if (closest != null && closest.Distance(qPrediction.UnitPosition) < 200)
                        {
                            _q.Cast(qPrediction.CastPosition);
                        }
                    }
                }
            }

            if (wActive && wTarget != null)
            {
                _w.Cast(wTarget);
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (unit.IsValidTarget(1000f) && spell.DangerLevel == InterruptableDangerLevel.High && _e.IsReady())
            {
                _r.Cast();

                if (!_r.IsReady())
                {
                    _e.Cast(ObjectManager.Player);
                }
            }
        }
    }
}
