#region
using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
#endregion

namespace XinZhao
{
    internal class Program
    {
        public static string ChampionName = "XinZhao";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        public static Items.Item Tiamat = new Items.Item(3077, 375);
        public static Items.Item Hydra = new Items.Item(3074, 375);

        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
                return;
            if (Player.IsDead)
                return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 480);

            CreateChampionMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            WelcomeMessage();
        }

        private static Obj_AI_Hero GetEnemy(float vDefaultRange = 0,
            TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Physical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
                vDefaultRange = Q.Range;

            if (!Config.Item("AssassinActive").GetValue<bool>())
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);

            var assassinRange = Config.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            Config.Item("Assassin" + enemy.ChampionName) != null &&
                            Config.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

            if (Config.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(100))
                return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
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

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawERange = Config.Item("DrawERange").GetValue<Circle>();
            if (drawERange.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawERange.Color, 1);

            var drawRRange = Config.Item("DrawRRange").GetValue<Circle>();
            if (drawRRange.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawRRange.Color, 1);

            var drawEMinRange = Config.Item("DrawEMinRange").GetValue<Circle>();
            if (drawEMinRange.Active)
            {
                var eMinRange = Config.Item("EMinRange").GetValue<Slider>().Value;
                Render.Circle.DrawCircle(Player.Position, eMinRange, drawEMinRange.Color, 1);
            }

            /* [ Draw Can Be Thrown Enemy ] */
            var drawThrownEnemy = Config.SubMenu("Drawings").Item("DrawThrown").GetValue<Circle>();
            if (drawThrownEnemy.Active)
            {
                foreach (var enemy in
                    from enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                enemy =>
                                    !enemy.IsDead && enemy.IsEnemy && Player.Distance(enemy) < R.Range && R.IsReady())
                    from buff in enemy.Buffs.Where(buff => !buff.Name.Contains("xenzhaointimidate"))
                    select enemy)
                {
                    Render.Circle.DrawCircle(enemy.Position, 90f, Color.White, 1);
                    Render.Circle.DrawCircle(enemy.Position, 95f, drawThrownEnemy.Color, 1);
                }
            }
        }

        public static void Combo()
        {
            var t = GetEnemy(E.Range, TargetSelector.DamageType.Magical);

            var useQ = Config.Item("ComboUseQ").GetValue<bool>();
            var useW = Config.Item("ComboUseW").GetValue<bool>();
            var useE = Config.Item("ComboUseE").GetValue<bool>();

            if (useQ && t.IsValidTarget(E.Range) && Q.IsReady())
                Q.Cast();

            if (useW && t.IsValidTarget(E.Range) && W.IsReady())
                W.Cast();

            if (useE && t.IsValidTarget(E.Range) && E.IsReady())
            {
                var eMinRange = Config.Item("EMinRange").GetValue<Slider>().Value;
                if (ObjectManager.Player.Distance(t) >= eMinRange)
                    E.CastOnUnit(t);
            }

            if (Player.Distance(t) <= E.Range)
                UseItems(t);

            if (Player.Distance(t) <= E.Range)
                UseItems(t, true);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) >= t.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, t);
                }
            }

            if (Tiamat.IsReady() && Player.Distance(t) <= Tiamat.Range)
                Tiamat.Cast();

            if (Hydra.IsReady() && Player.Distance(t) <= Hydra.Range)
                Tiamat.Cast();
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("LaneClearUseQ").GetValue<bool>();
            var useW = Config.Item("LaneClearUseW").GetValue<bool>();
            var useE = Config.Item("LaneClearUseE").GetValue<bool>();

            var allMinions = MinionManager.GetMinions(
                Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            if ((useQ || useW))
            {
                var minionsQ = MinionManager.GetMinions(Player.ServerPosition, 400);
                foreach (var vMinion in
                    from vMinion in minionsQ where vMinion.IsEnemy select vMinion)
                {
                    if (useQ && Q.IsReady())
                        Q.Cast();
                    if (useW && W.IsReady())
                        W.Cast();
                }
            }

            if (allMinions.Count >= 2)
            {
                if (Tiamat.IsReady())
                    Tiamat.Cast();

                if (Hydra.IsReady())
                    Hydra.Cast();
            }

            if (useE && E.IsReady())
            {

                var locE = E.GetCircularFarmLocation(allMinions);
                if (allMinions.Count == allMinions.Count(m => Player.Distance(m) < E.Range) && locE.MinionsHit >= 2 &&
                    locE.Position.IsValid())
                    E.Cast(locE.Position);
            }

        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("JungleFarmUseQ").GetValue<bool>();
            var useW = Config.Item("JungleFarmUseW").GetValue<bool>();
            var useE = Config.Item("JungleFarmUseE").GetValue<bool>();

            var mobs = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
                return;

            var mob = mobs[0];
            if (useQ && Q.IsReady() && mobs.Count >= 1)
                Q.Cast();

            if (useW && W.IsReady() && mobs.Count >= 1)
                W.Cast();

            if (useE && E.IsReady() && mobs.Count >= 2)
                E.CastOnUnit(mob);

            if (mobs.Count >= 2)
            {
                if (Tiamat.IsReady())
                    Tiamat.Cast();

                if (Hydra.IsReady())
                    Hydra.Cast();
            }

        }

        private static InventorySlot GetInventorySlot(int ID)
        {
            return
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    item =>
                        (item.Id == (ItemId) ID && item.Stacks >= 1) || (item.Id == (ItemId) ID && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget, bool useNonTargetedItems = false)
        {
            if (vTarget == null)
                return;
            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                into itemID
                where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null
                select itemID)
            {
                Items.UseItem(itemID, vTarget);
            }

            if (!useNonTargetedItems)
                return;

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items
                let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                into itemID
                where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null
                select itemID)
            {
                Items.UseItem(itemID);
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells)
                return;

            if (Player.Distance(vTarget) < R.Range) // && !vTarget.HasBuff("XinDamage"))
            {
                R.Cast();
            }
        }

        private static void CreateChampionMenu()
        {
            Config = new Menu("Bbyyy汉化-赵信", ChampionName, true);

            Config.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var TargetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);

            new AssassinManager();

            /* [ Combo ] */
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseQ", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseW", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseE", "使用 E").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("EMinRange", " E 最小范围").SetValue(new Slider(300, 200, 500)));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "连招!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            /* [ Lane Clear ] */
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearUseQ", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearUseW", "使用 W").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearUseE", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "最低清线蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "清线!").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            /* [ Jungling Farm ] */
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmUseQ", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmUseW", "使用 W").SetValue(false));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmUseE", "使用 E").SetValue(false));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmMana", "最低清线蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "清野!").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            /* [ Drawing ] */
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("DrawERange", "E 范围").SetValue(new Circle(false, Color.PowderBlue)));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("DrawEMinRange", "E 最小范围").SetValue(new Circle(false, Color.Aqua)));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("DrawRRange", "R 范围").SetValue(new Circle(false, Color.PowderBlue)));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawThrown", "能够挑起的敌人").SetValue(new Circle(false, Color.PowderBlue)));

            /* [  Extras -> Use Items ] */
            MenuExtras = new Menu("额外", "Extras");
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpells", "法术 中断").SetValue(true));

            /* [  Extras -> Use Items ] */
            var menuUseItems = new Menu("使用 物品", "menuUseItems");
            MenuExtras.AddSubMenu(menuUseItems);

            /* [ Extras -> Use Items -> Targeted Items ] */
            MenuTargetedItems = new Menu("攻击 物品", "menuTargetItems");
            menuUseItems.AddSubMenu(MenuTargetedItems);
            MenuTargetedItems.AddItem(new MenuItem("item3153", "破败").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "兰顿").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "比尔吉沃特弯刀").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3146", "科技枪").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "冰锤").SetValue(true));

            /* [ Extras -> Use Items -> AOE Items ] */
            MenuNonTargetedItems = new Menu("AOE 物品", "menuNonTargetedItems");
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "奥丁面纱").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "神圣之剑").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九头蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));

            new PotionManager();
            Config.AddToMainMenu();
        }

        private static void WelcomeMessage()
        {
            Game.PrintChat(
                String.Format(
                    "<font color='#70DBDB'>xQx |</font> <font color='#FFFFFF'>{0}</font> <font color='#70DBDB'>Loaded!汉化-Bbyyy</font>",
                    ChampionName));
        }

    }
}