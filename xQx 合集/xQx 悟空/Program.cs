#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Wukong
{
    internal class Program
    {
        public const string ChampionName = "MonkeyKing";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        private static readonly Items.Item Tiamat = new Items.Item(3077, 450);
        private static readonly SpellSlot SmiteSlot = Player.GetSpellSlot("SummonerSmite");
        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        private const float SmiteRange = 700f;

        //Menu
        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;
        private static int DelayTick { get; set; }


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "MonkeyKing") return;
            if (Player.IsDead) return;
            
            DelayTick = 0;

            Q = new Spell(SpellSlot.Q, 420f);
            E = new Spell(SpellSlot.E, 640f);
            R = new Spell(SpellSlot.R, 355f);

            E.SetTargetted(0.5f, 2000f);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu("Bbyyy汉化-悟空", "MonkeyKing", true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);



            var menuCombo = new Menu("连招", "Combo");
            // Combo
            Config.AddSubMenu(menuCombo);
            menuCombo.AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
      
            menuCombo.AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            menuCombo.AddItem(new MenuItem("UseEComboTurret", "禁止塔下使用 E").SetValue(true));
            menuCombo.AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            menuCombo.AddItem(new MenuItem("UseRComboEnemyCount", "最低敌人数量 : ").SetValue(new Slider(1, 5, 0)));
            menuCombo.AddItem(
                new MenuItem("ComboActive", "连招!").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            // Harass
            Config.AddSubMenu(new Menu("Harass", "骚扰"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarassTurret", "禁止塔下使用 E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "骚扰最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "骚扰").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));


            // Lane Clear
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "清线最低蓝量: ").SetValue(new Slider(50, 100, 0)));

            var menuLaneClearItems = new Menu("使用 物品", "menuLaneClearItems");
            Config.SubMenu("LaneClear").AddSubMenu(menuLaneClearItems);
            menuLaneClearItems.AddItem(new MenuItem("LaneClearUseTiamat", "提亚马特").SetValue(true));

            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearActive", "清线").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            // Jungling Farm
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "使用 E").SetValue(false));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("AutoSmite", "自动 惩戒")
                .SetValue<KeyBind>(new KeyBind('N', KeyBindType.Toggle)));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmMana", "清野最低蓝量: ")
                .SetValue(new Slider(50, 100, 0)));

            var menuJungleFarmItems = new Menu("使用 物品", "menuJungleFarmItems");
            Config.SubMenu("JungleFarm").AddSubMenu(menuJungleFarmItems);
            menuLaneClearItems.AddItem(new MenuItem("JungleFarmUseTiamat", "提亚马特").SetValue(true));

            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmActive", "清野").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            // Extras -> Use Items 
            MenuExtras = new Menu("杂项", "Extras");
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpells", "中断技能").SetValue(true));
            MenuExtras.AddItem(new MenuItem("AutoLevelUp", "自动升级").SetValue(true));

            Menu menuUseItems = new Menu("使用 物品", "menuUseItems");
            Config.SubMenu("Extras").AddSubMenu(menuUseItems);
            // Extras -> Use Items -> Targeted Items
            MenuTargetedItems = new Menu("攻击 物品", "menuTargetItems");
            menuUseItems.AddSubMenu(MenuTargetedItems);

            MenuTargetedItems.AddItem(new MenuItem("item3153", "破败").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "兰顿").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "比尔吉沃特弯刀").SetValue(true));

            MenuTargetedItems.AddItem(new MenuItem("item3146", "科技枪").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "冰锤 ").SetValue(true));

            // Extras -> Use Items -> AOE Items
            MenuNonTargetedItems = new Menu("AOE 物品", "menuNonTargetedItems");
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "奥丁面纱").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "神圣之剑").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九头蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3077", "提亚马特 ").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));

            // Drawing
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(false,
                System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(false,
                System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(false,
                System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("SmiteRange", "惩戒 范围").SetValue(new Circle(false,
                System.Drawing.Color.FromArgb(255, 255, 255, 255))));

           // new PotionManager();
            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            Game.PrintChat(String.Format("<font color='#70DBDB'>xQx | </font> <font color='#FFFFFF'>" +
                                         "{0}</font> <font color='#70DBDB'> Loaded!汉化-Bbyyy!</font>", ChampionName));
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && spell.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            var drawSmite = Config.Item("SmiteRange").GetValue<Circle>();
            if (Config.Item("AutoSmite").GetValue<KeyBind>().Active && drawSmite.Active)
            {
                Render.Circle.DrawCircle(Player.Position, SmiteRange, drawSmite.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(100)) return;

            if (DelayTick - Environment.TickCount <= 250)
            {
                UseSummoners();
                DelayTick = Environment.TickCount;
            }

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("HarassMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("LaneClearMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    JungleFarm();
            }
        }

        private static void Combo()
        {
            // var q1Target = selectedTarget ?? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            // var e1Target = selectedTarget ?? TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);



            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();

            if (qTarget != null & Q.IsReady() && useQ)
            {
                Q.CastOnUnit(Player);
            }

            if (eTarget != null && E.IsReady() && useE)
            {
                if (Config.Item("UseEComboTurret").GetValue<bool>())
                {
                    if (!Utility.UnderTurret(eTarget, true))
                        E.CastOnUnit(eTarget);
                }
                else
                    E.CastOnUnit(eTarget);
            }

            if (eTarget != null)
                UseItems(eTarget);

            if (qTarget != null && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                Player.GetSummonerSpellDamage(rTarget, Damage.SummonerSpell.Ignite) > rTarget.Health)
            {
                Player.Spellbook.CastSpell(IgniteSlot, rTarget);
            }

            if (R.IsReady() && useR)
            {
                if (ObjectManager.Player.CountEnemysInRange((int) Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)) >=
                    Config.Item("UserRComboEnemyCount").GetValue<Slider>().Value) 
                    R.Cast();
            }
        }

        private static void Harass()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();


            if (qTarget != null && Q.IsReady() && useQ)
            {
                Q.CastOnUnit(Player);
            }

            if (eTarget != null && E.IsReady() && useE)
            {
                if (Config.Item("UseEHarassTurret").GetValue<bool>())
                {
                    if (!Utility.UnderTurret(eTarget, true))
                        E.CastOnUnit(eTarget);
                }
                else
                    E.CastOnUnit(eTarget);
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (useQ && Q.IsReady() && mobs.Count >= 1)
                Q.Cast();

            if (useE && E.IsReady() && mobs.Count >= 2)
                E.CastOnUnit(mob);

            if (Tiamat.IsReady() && Config.Item("JungleFarmUseTiamat").GetValue<bool>())
            {
                if (mobs.Count >= 2)
                    Tiamat.Cast(Player);
            }
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                var minionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

                foreach (var vMinion in from vMinion in minionsQ
                    let vMinionEDamage = Player.GetSpellDamage(vMinion, SpellSlot.Q)
                    where vMinion.Health <= vMinionEDamage && vMinion.Health > Player.GetAutoAttackDamage(vMinion)
                    select vMinion) 
                {
                    Q.CastOnUnit(Player);
                }
            }

            if (Tiamat.IsReady() && Config.Item("LaneClearUseTiamat").GetValue<bool>())
            {
                var allMinions = MinionManager.GetMinions(Player.ServerPosition,
                    Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));
                var locTiamat = E.GetCircularFarmLocation(allMinions);
                if (locTiamat.MinionsHit >= 3)
                    Tiamat.Cast(Player);
                //Items.UseItem(itemTiamat, locTiamat.Position);
            }

            if (useE && E.IsReady())
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var locE = E.GetCircularFarmLocation(allMinionsE);
                if (allMinionsE.Count == allMinionsE.Count(m => Player.Distance(m) < E.Range) && locE.MinionsHit >= 2 &&
                    locE.Position.IsValid()) 
                    E.Cast(locE.Position);
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.E);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.R);

            if (Items.CanUseItem(3128))
                fComboDamage += Player.GetItemDamage(vTarget, Damage.DamageItems.Botrk);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells) return;

            if (Player.Distance(vTarget) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
            {
                R.Cast();
            }
        }

        private static InventorySlot GetInventorySlot(int id)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(
                item => (item.Id == (ItemId)id && item.Stacks >= 1) || (item.Id == (ItemId)id && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget)
        {
            if (vTarget == null) return;

            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                                   let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                                       into itemId
                                       where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                                       select itemId)
            {
                Items.UseItem(itemID, vTarget);
            }

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items
                                   let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                                       into itemId
                                       where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                                       select itemId)
            {
                Items.UseItem(itemID);
            }
        }
        private static void UseSummoners()
        {
            if (SmiteSlot == SpellSlot.Unknown)
                return;

            if (!Config.Item("AutoSmite").GetValue<KeyBind>().Active) return;

            string[] monsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
            var firstOrDefault = Player.Spellbook.Spells.FirstOrDefault(
                spell => spell.Name.Contains("mite"));
            if (firstOrDefault == null) return;

            var vMonsters = MinionManager.GetMinions(Player.ServerPosition, firstOrDefault.SData.CastRange[0],
                MinionTypes.All, MinionTeam.NotAlly);
            foreach (
                var vMonster in
                    vMonsters.Where(
                        vMonster =>
                            vMonster != null && !vMonster.IsDead && !Player.IsDead && !Player.IsStunned &&
                            SmiteSlot != SpellSlot.Unknown &&
                            Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                        .Where(
                            vMonster =>
                                (vMonster.Health < Player.GetSummonerSpellDamage(vMonster, Damage.SummonerSpell.Smite)) &&
                                (monsterNames.Any(name => vMonster.BaseSkinName.StartsWith(name))))) 
            {
                Player.Spellbook.CastSpell(SmiteSlot, vMonster);
            }
        }

    }
}
