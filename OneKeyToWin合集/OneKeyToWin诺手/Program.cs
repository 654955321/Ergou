#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Darius
{
    internal class Program
    {
        private const string ChampionName = "Darius";
        private static Orbwalking.Orbwalker Orbwalker;
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell Q, W, E, R;
        private static Menu _config;

        public static SpellSlot IgniteSlot;
        public static Items.Item Hydra;
        public static Items.Item Tiamat;
        public static Items.Item Randuin;
        public static int QMANA;
        public static int WMANA;
        public static int EMANA;
        public static int RMANA;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 145);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);
            
            E.SetSkillshot(0.1f, 50f * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Tiamat = new Items.Item(3077, 375);
            Hydra = new Items.Item(3074, 375);
            Randuin = new Items.Item(3143, 500);
            _config = new Menu("花边汉化-一键诺手", "Darius", true);

            _config.AddSubMenu(new Menu("走 砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            _config.AddSubMenu(new Menu("连 招", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseICombo", "使用 物品").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("Killsteal", "抢人头").SetValue(true));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "开 启!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("骚 扰", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassActive", "开 启!").SetValue(new KeyBind(88, KeyBindType.Press)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActiveT", "骚 扰 (自动)!").SetValue(new KeyBind("Y".ToCharArray()[0],
                        KeyBindType.Toggle)));

            _config.AddSubMenu(new Menu("范 围", "Drawings"));
            _config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            _config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += afterAttack;
        }

        private static void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (_config.Item("ComboActive").GetValue<KeyBind>().Active && _config.Item("UseWCombo").GetValue<bool>() &&
                unit.IsMe && (target is Obj_AI_Hero))
                W.Cast();  
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SpellList == null) return;

            foreach (var spell in SpellList)
            {
                var menuItem = _config.Item(spell.Slot + "Range").GetValue<Circle>();

                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (R.IsReady() && _config.Item("UseRCombo").GetValue<bool>())
            {
                CastR();
            }
            var targetRanduin = TargetSelector.GetTarget(Randuin.Range, TargetSelector.DamageType.Physical); 
            if (targetRanduin.IsValidTarget() && targetRanduin.Path[0].Distance(ObjectManager.Player.ServerPosition) > ObjectManager.Player.Distance(targetRanduin.ServerPosition) && ObjectManager.Player.Distance(targetRanduin.ServerPosition) > 220)
            {
                Items.UseItem(3143);
            }

            if (E.IsReady() && ObjectManager.Player.Mana > RMANA + EMANA && Orbwalker.ActiveMode.ToString() == "Combo")
            {
                var target =  TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical); 
                if (target.IsValidTarget())
                {
                    if ( target.Path.Count() > 0 && target.Path[0].Distance(ObjectManager.Player.ServerPosition) > ObjectManager.Player.Distance(target.ServerPosition) && ObjectManager.Player.Distance(target) > 220)
                        E.Cast(target, true, true);
                }
            }
            if (CountEnemies(ObjectManager.Player, 385) > 0)
            {
                Items.UseItem(Items.HasItem(3077) ? 3077 : 3074);
            }
            if (Q.IsReady() && CountEnemies(ObjectManager.Player, Q.Range) > 0 && ObjectManager.Player.Mana > RMANA + QMANA && Orbwalker.ActiveMode.ToString() == "Combo")
                Q.Cast();


            if ((_config.Item("HarassActive").GetValue<KeyBind>().Active) ||
                (_config.Item("HarassActiveT").GetValue<KeyBind>().Active))
                ExecuteHarass();
            if (Q.IsReady() && ObjectManager.Player.Mana > RMANA + QMANA + RMANA + WMANA && Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                   MinionTypes.All);
                foreach (var minion in allMinionsQ)
                    if (ObjectManager.Player.Distance(minion.ServerPosition) > 300 && minion.Health <  ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) * 0.6)
                        Q.Cast();
            }
        }

        private static void CastR()
        {
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(R.Range)))
            {
                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R, 1) - 5 > target.Health)
                {
                    R.CastOnUnit(target, true);
                }
                else
                {
                    foreach (var buff in target.Buffs)
                    {
                        if (buff.Name == "dariushemo")
                        {
                            if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R, 1) * (1 + buff.Count / 5) - 1 > target.Health)
                            {
                                R.CastOnUnit(target, true);
                            }
                            else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.4 && ObjectManager.Player.GetSpellDamage(target, SpellSlot.R, 1) * 1.2 * ((1 + buff.Count / 5) - 1) > target.Health)
                            {
                                R.CastOnUnit(target, true);
                            }
                            
                        }
                    }
                }
            }
            
        }

        private static void ExecuteHarass()
        {
            if (!_config.Item("UseQHarass").GetValue<bool>() || !Q.IsReady() || ObjectManager.Player.Mana < RMANA + WMANA + EMANA + QMANA) return;

            var c =
                (from hero in ObjectManager.Get<Obj_AI_Hero>()
                    where hero.IsValidTarget()
                    select ObjectManager.Player.Distance(hero)).Count(dist => dist > 270 && dist <= Q.Range);

            if (c > 0)
                Q.Cast();
        }
        public static void ManaMenager()
        {
            QMANA = 40;
            WMANA = 25 + 5 * W.Level;
            EMANA = 45;
            if (!R.IsReady())
                RMANA = 25;
            else
                RMANA = 100;
        }
        private static int CountEnemies(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                            hero.IsValidTarget() && hero.Team != ObjectManager.Player.Team &&
                            hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }
    }
}