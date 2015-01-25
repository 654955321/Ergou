using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace VeigarEndboss
{
    class Program
    {
        private static readonly string champName = "Veigar";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        private static Spell Q, W, E, R;
        public static List<Spell> SpellList = new List<Spell>();

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        private static Menu menu;
        private static Orbwalking.Orbwalker OW;
        public static Items.Item Dfg = new Items.Item(3128, 750);

        public static void Main(string[] args)
        {
            // Register load event
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            // Validate champion
            if (Player.ChampionName != champName)
                return;

            // Initialize spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 650);

            SpellList.AddRange(new[] { Q, W, E, R });
            
            Q.SetTargetted(0.25f, 1500);
            W.SetSkillshot(1.25f, 225, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, 1400);

            // Setup menu
            SetuptMenu();

            // Initialize classes
            BalefulStrike.Initialize(Q, OW);
            DarkMatter.Initialize(W);

            // Register additional events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Auto stack Q
            BalefulStrike.AutoFarmMinions = menu.SubMenu("misc").Item("miscStackQ").GetValue<bool>() && !menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active;
            // Auto W on stunned
            DarkMatter.AutoCastStunned = menu.SubMenu("misc").Item("miscAutoW").GetValue<bool>();

            // Combo
            if (menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo();
            // Harass
            if (menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active)
                OnHarass();
            // WaveClear
            if (menu.SubMenu("waveClear").Item("waveActive").GetValue<KeyBind>().Active)
                OnWaveClear();
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (W.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.W);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.R);

            if (Items.CanUseItem(3128))
                fComboDamage += Player.GetItemDamage(vTarget, Damage.DamageItems.Dfg);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }


        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            var useQ = menu.SubMenu("combo").Item("comboUseQ").GetValue<bool>();
            var useW = menu.SubMenu("combo").Item("comboUseW").GetValue<bool>();
            var useE = menu.SubMenu("combo").Item("comboUseE").GetValue<bool>();
            var useR = menu.SubMenu("combo").Item("comboUseR").GetValue<bool>();

            var comboResult = GetComboDamage(target);
            var eResult = EventHorizon.GetCastPosition(target);

            if (target.Health < comboResult)
            { 
                if (IgniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                    Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, target);
                }

                if (Dfg.IsReady() && target.Health < Player.GetItemDamage(target, Damage.DamageItems.Dfg))
                {
                    Dfg.Cast(target);
                }

                if (E.IsReady() && useE && eResult.Valid)
                    E.Cast(eResult.CastPosition);

                if (W.IsReady() && useW && W.IsInRange(target.ServerPosition))
                    W.Cast(target.Position);

                if (Q.IsReady() && useQ && Q.IsInRange(target.ServerPosition))
                    Q.CastOnUnit(target);

                if (R.IsReady() && useR && R.IsInRange(target.ServerPosition))
                    R.CastOnUnit(target);
            }
            else
            {
                if (Q.IsReady() && useQ && Q.IsInRange(target.ServerPosition))
                    Q.CastOnUnit(target);

                if (W.IsReady() && useW && W.IsInRange(target.ServerPosition))
                    W.Cast(target.Position);

                if (E.IsReady() && useE && eResult.Valid)
                    E.Cast(eResult.CastPosition);
            }
        }
        
        private static void OnHarass()
        {
            // Mana check
            if (Player.Mana / Player.MaxMana * 100 < menu.SubMenu("harass").Item("harassMana").GetValue<Slider>().Value)
                return;

            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            // Q
            if (menu.SubMenu("harass").Item("harassUseQ").GetValue<bool>() && Q.IsReady() && Q.IsInRange(target.ServerPosition))
            {
                Q.CastOnUnit(target);
            }

            // W
            if (menu.SubMenu("harass").Item("harassUseW").GetValue<bool>() && W.IsReady())
            {
                W.Cast(target);
            }
        }

        private static void OnWaveClear()
        {
            if (menu.SubMenu("waveClear").Item("waveUseW").GetValue<bool>() && W.IsReady())
            {
                var farmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(Player.Position, W.Range).Select(minion => minion.ServerPosition.To2D()).ToList(), W.Width, W.Range);

                if (farmLocation.MinionsHit >= menu.SubMenu("waveClear").Item("waveNumW").GetValue<Slider>().Value && Player.Distance(farmLocation.Position) <= W.Range)
                    W.Cast(farmLocation.Position);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in SpellList)
            {
                var circleEntry = menu.SubMenu("drawings").Item("drawRange" + spell.Slot.ToString()).GetValue<Circle>();
                if (circleEntry.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, circleEntry.Color);
            }
        }

        private static void SetuptMenu()
        {
            // Create menu
            menu = new Menu("Bbyyy汉化-小法" + champName, "hells" + champName, true);

            // Target selector
            Menu targetSelector = new Menu("目标 选择", "ts");
            TargetSelector.AddToMenu(targetSelector);
            menu.AddSubMenu(targetSelector);

            // Orbwalker
            Menu orbwalker = new Menu("走砍", "orbwalker");
            OW = new Orbwalking.Orbwalker(orbwalker);
            menu.AddSubMenu(orbwalker);

            // Combo
            Menu combo = new Menu("连招", "combo");
            combo.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("comboUseIgnite", "使用 引燃").SetValue(true));
            combo.AddItem(new MenuItem("comboActive", "连招 启动").SetValue(new KeyBind(32, KeyBindType.Press)));
            menu.AddSubMenu(combo);

            // Harass
            Menu harass = new Menu("骚扰", "harass");
            harass.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseW", "使用 W").SetValue(true));
            harass.AddItem(new MenuItem("harassMana", "骚扰最低蓝量").SetValue(new Slider(30)));
            harass.AddItem(new MenuItem("harassActive", "骚扰 启动").SetValue(new KeyBind('C', KeyBindType.Press)));
            menu.AddSubMenu(harass);

            // WaveClear
            Menu waveClear = new Menu("清线", "waveClear");
            waveClear.AddItem(new MenuItem("waveUseW", "使用 W").SetValue(true));
            waveClear.AddItem(new MenuItem("waveNumW", "W 小兵数").SetValue(new Slider(3, 1, 10)));
            waveClear.AddItem(new MenuItem("waveActive", "清线 启动").SetValue(new KeyBind('V', KeyBindType.Press)));
            menu.AddSubMenu(waveClear);

            // Misc
            Menu misc = new Menu("杂项", "misc");
            misc.AddItem(new MenuItem("miscStackQ", "自动 堆叠 Q").SetValue(true));
            misc.AddItem(new MenuItem("miscAutoW", "自动 W 眩晕").SetValue(true));
            menu.AddSubMenu(misc);

            // Drawings
            Menu drawings = new Menu("范围", "drawings");
            drawings.AddItem(new MenuItem("drawRangeQ", "Q 范围").SetValue(new Circle(true, Color.FromArgb(150, Color.IndianRed))));
            drawings.AddItem(new MenuItem("drawRangeW", "W 范围").SetValue(new Circle(true, Color.FromArgb(150, Color.MediumPurple))));
            drawings.AddItem(new MenuItem("drawRangeE", "E 范围").SetValue(new Circle(false, Color.FromArgb(150, Color.DarkRed))));
            drawings.AddItem(new MenuItem("drawRangeR", "R 范围").SetValue(new Circle(false, Color.FromArgb(150, Color.Red))));
            menu.AddSubMenu(drawings);

            // Finalize menu
            menu.AddToMainMenu();
        }
    }
}
