#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Ryze
{
    internal class Program
    {
        public const string ChampionName = "Ryze";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;

        //Menu
        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);

            //Create the menu
            Config = new Menu("花边汉化-Esk0r瑞兹", ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连 招", "Combo"));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "开 启!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("骚 扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "开 启!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("打 钱", "Farm"));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "使用 Q").SetValue(
                        new StringList(new[] { "控 线", "清 线", "两 者", "都 不" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "使用 W").SetValue(
                        new StringList(new[] { "控 线", "清 线", "两 者", "都 不" }, 3)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseEFarm", "使用 E").SetValue(
                        new StringList(new[] { "控 线", "清 线", "两 者", "都 不" }, 1)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "控 线!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClearActive", "清 线!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("清 野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "使用 W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "使用 E").SetValue(true));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "开 启!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示 连招 伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)(Player.GetSpellDamage(hero, SpellSlot.Q) * 2 + Player.GetSpellDamage(hero, SpellSlot.W) + Player.GetSpellDamage(hero, SpellSlot.E));
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Config.AddSubMenu(new Menu("范 围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("WRange", "W 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                args.Process = !(Q.IsReady() || W.IsReady() || E.IsReady() || Player.Distance(args.Target) >= 600);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var qCd = Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time;

            if (target != null)
            {
                if (Player.Distance(target) <= 600)
                {
                    if (Player.Distance(target) >= 575 && W.IsReady() && target.Path.Count() > 0 &&
                        target.Path[0].Distance(Player.ServerPosition) >
                        Player.Distance(target))
                    {
                        W.CastOnUnit(target);
                    }
                    else if (Q.IsReady())
                    {
                        Q.CastOnUnit(target);
                    }
                    else
                    {
                        if (qCd > 1.25f)
                        {
                            if (W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            else if (E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }
                    }
                }
                else if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
                {
                    Q.CastOnUnit(target);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null && Config.Item("UseQHarass").GetValue<bool>())
            {
                Q.CastOnUnit(target);
            }
        }

        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Config.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance(minion) * 1000 / 1400)) <
                         Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }
            }
            else if (useW && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(W.Range) &&
                        minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        W.CastOnUnit(minion);
                        return;
                    }
                }
            }
            else if (useE && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(E.Range) &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance(minion) * 1000 / 1000)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        E.CastOnUnit(minion);
                        return;
                    }
                }
            }

            if (laneClear)
            {
                foreach (var minion in allMinions)
                {
                    if (useQ)
                        Q.CastOnUnit(minion);

                    if (useW)
                        W.CastOnUnit(minion);

                    if (useE)
                        E.CastOnUnit(minion);
                }
            }
        }

        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                Q.CastOnUnit(mob);
                W.CastOnUnit(mob);
                E.CastOnUnit(mob);
            }
        }
    }
}
