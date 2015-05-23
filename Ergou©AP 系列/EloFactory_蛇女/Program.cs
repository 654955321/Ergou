#region

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace EloFactory_Cassiopeia
{
    internal class Program
    {
        public const string ChampionName = "Cassiopeia";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static Spell Ignite = new Spell(SpellSlot.Unknown, 600);

        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;

        public static SpellSlot FlashSlot;

        public static List<Obj_AI_Base> MinionCount;

        static int lastCastE = 0;
        static int lastCastQ = 0;
        static int lastCastW = 0;

        public static Items.Item HealthPotion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item CrystallineFlask = new Items.Item(2041, 0);
        public static Items.Item BiscuitofRejuvenation = new Items.Item(2010, 0);
        public static Items.Item TearoftheGoddess = new Items.Item(3070, 0);
        public static Items.Item TearoftheGoddessCrystalScar = new Items.Item(3073, 0);
        public static Items.Item ArchangelsStaff = new Items.Item(3003, 0);
        public static Items.Item ArchangelsStaffCrystalScar = new Items.Item(3007, 0);
        public static Items.Item Manamune = new Items.Item(3004, 0);
        public static Items.Item ManamuneCrystalScar = new Items.Item(3008, 0);

        public static Menu Config;

        private static Obj_AI_Hero Player;

        public static int[] abilitySequence;
        public static int qOff = 0, wOff = 0, eOff = 0, rOff = 0;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.ChampionName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 850f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 825f);

            Q.SetSkillshot(0.6f, 40f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.5f, 90f, 2500, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.2f, float.MaxValue);
            R.SetSkillshot(0.6f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            var ignite = Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "summonerdot");
            if (ignite != null)
                Ignite.Slot = ignite.Slot;

            FlashSlot = Player.GetSpellSlot("summonerflash");


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            abilitySequence = new int[] { 1, 3, 3, 2, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };

            List<Obj_AI_Hero> EnemyTeam = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).ToList();

            Config = new Menu("EloFactory_蛇女", ChampionName + " By LuNi", true);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));

            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.UseQCombo", "使用 Q 连招").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.UseWCombo", "使用 W 连招").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.UseECombo", "使用 E 连招").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.UseRCombo", "使用 R 连招").SetValue(true));
            Config.SubMenu("Combo").AddSubMenu(new Menu("优先使突进者中毒", "Swap Poison HitChance On Gapcloser Champ"));
            Config.SubMenu("Combo").SubMenu("Swap Poison HitChance On Gapcloser Champ").AddItem(new MenuItem("Cassiopeia.SwapHitChanceOnDashingChamp", "优先使突进者中毒进行连招").SetValue(true));
            foreach (Obj_AI_Hero Champion in EnemyTeam)
            {
                if (GapCloserChampions.Contains(Champion.ChampionName))
                {
                    Config.SubMenu("Combo").SubMenu("Swap Poison HitChance On Gapcloser Champ").AddItem(new MenuItem(Champion.ChampionName + "Swap", "优先中毒单位 " + Champion.ChampionName).SetValue(true));
                }         
            }
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.PoisonOrder", "连招施法顺序(模式)").SetValue(new StringList(new[] { "先W后Q", "先Q后W" })));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.PoisonStack", "中毒技能 (模式)").SetValue(new StringList(new[] { "对已中毒单位不QW", "不限制技能使用" })));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.PoisonStack1VS1", "不限制中毒技能使用当敌人只有一个时").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.StartECombo", "开始连招时只有对中毒单位才使用E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.AutoWOnStunTarget", "自动 W 减速目标").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.AA1", "走A 连招 (模式)").SetValue(new StringList(new[] { "最低限度 AA", "智能 走A", "禁用 走A" })));
            Config.SubMenu("Combo").AddSubMenu(new Menu("智能 E 延迟", "Inteligent E Delay"));
            Config.SubMenu("Combo").SubMenu("Inteligent E Delay").AddItem(new MenuItem("Cassiopeia.LegitE", "智能E|当目标在E范围内").SetValue(false));
            Config.SubMenu("Combo").SubMenu("Inteligent E Delay").AddItem(new MenuItem("Cassiopeia.EDelayCombo350", "智能E 连招时目标范围 < 350").SetValue(new Slider(4500, 0, 7000)));
            Config.SubMenu("Combo").SubMenu("Inteligent E Delay").AddItem(new MenuItem("Cassiopeia.EDelayCombo525", "智能E 连招时目标范围E延迟 < 525").SetValue(new Slider(2700, 0, 5000)));
            Config.SubMenu("Combo").SubMenu("Inteligent E Delay").AddItem(new MenuItem("Cassiopeia.EDelayComboERange", "智能E 连招目标范围 < E.范围").SetValue(new Slider(1300, 0, 3000)));
            Config.SubMenu("Combo").AddSubMenu(new Menu("抢人头 模式", "KS Mode"));
            Config.SubMenu("Combo").SubMenu("KS Mode").AddItem(new MenuItem("Cassiopeia.UseIgniteKS", "使用点燃抢人头").SetValue(true));
            Config.SubMenu("Combo").SubMenu("KS Mode").AddItem(new MenuItem("Cassiopeia.UseENPKS", "非中毒情况使用E抢人头").SetValue(true));
            Config.SubMenu("Combo").SubMenu("KS Mode").AddItem(new MenuItem("Cassiopeia.UseENPKSCount", "使用非中毒情况使用E抢人头 对方敌人最大数量").SetValue(new Slider(1, 1, 5)));
            Config.SubMenu("Combo").SubMenu("KS Mode").AddItem(new MenuItem("Cassiopeia.UseEPKS", "启用E抢人头").SetValue(true));
            Config.SubMenu("Combo").AddSubMenu(new Menu("协助使用 R", "Assisted R"));
            Config.SubMenu("Combo").SubMenu("Assisted R").AddItem(new MenuItem("Cassiopeia.AssistedRKey", "协助使用（半自动） R 键位!").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Combo").SubMenu("Assisted R").AddItem(new MenuItem("Cassiopeia.MoveOnCursorWhenAssistedRKey", "当按住键位时向鼠标方向移动").SetValue(true));
            Config.SubMenu("Combo").SubMenu("Assisted R").AddItem(new MenuItem("Cassiopeia.AssistedRFacing", "自动R当敌人突脸").SetValue(true));
            Config.SubMenu("Combo").SubMenu("Assisted R").AddItem(new MenuItem("Cassiopeia.AssistedRFacingCount", "自动R 敌人突脸人数").SetValue(new Slider(1, 1, 5)));
            Config.SubMenu("Combo").SubMenu("Assisted R").AddItem(new MenuItem("Cassiopeia.AssistedREnemies", "自动R当敌人攻击自己").SetValue(false));
            Config.SubMenu("Combo").SubMenu("Assisted R").AddItem(new MenuItem("Cassiopeia.AssistedREnemiesCount", "自动R 攻击自己的敌人数量").SetValue(new Slider(4, 1, 5)));
           
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddSubMenu(new Menu("当敌人在自己的走A范围内自动使用中毒技能", "Use Poison Spell On Enemy AA or Spell Cast"));
            Config.SubMenu("Harass").SubMenu("Use Poison Spell On Enemy AA or Spell Cast").AddItem(new MenuItem("Cassiopeia.AutoQWhenEnemyCastHarass", "自动使用 Q 骚扰").SetValue(true));
            Config.SubMenu("Harass").SubMenu("Use Poison Spell On Enemy AA or Spell Cast").AddItem(new MenuItem("Cassiopeia.QEnemyAttackMiniManaHarass", "自动使用 Q 骚扰 最低蓝量").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Harass").SubMenu("Use Poison Spell On Enemy AA or Spell Cast").AddItem(new MenuItem("Cassiopeia.AutoWWhenEnemyCastHarass", "自动使用 W 骚扰").SetValue(false));
            Config.SubMenu("Harass").SubMenu("Use Poison Spell On Enemy AA or Spell Cast").AddItem(new MenuItem("Cassiopeia.WEnemyAttackMiniManaHarass", "自动使用 W 骚扰 最低蓝量").SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.QMiniManaHarass", "使用 Q 最低蓝量").SetValue(new Slider(20, 0, 100)));      
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.UseWHarass", "使用 W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.WMiniManaHarass", "使用 W 最低蓝量").SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.EMiniManaHarass", "使用 E 最低蓝量").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.EDelayHarass", "骚扰时 E的延迟").SetValue(new Slider(0, 0, 2000)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("补刀", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.ToogleUseELastHit", "自动E补刀对中毒小兵 (开关)!").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.ToogleUseELastHitMode", "自动E补刀对中毒小兵 (模式)").SetValue(new StringList(new[] { "没有敌人的情况下", "总是" })));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.ToogleUseELastHitOption", "当按住连招键位时禁用技能补刀").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.UseELastHit", "使用E对中毒小兵补刀").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.UseELastHitNoPoisoned", "使用E对未中毒小兵补刀").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.EDelayLastHit", "补刀时 E的延迟").SetValue(new Slider(0, 0, 2000)));

            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseQLaneClear", "使用 Q 清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.QMiniManaLaneClear", "使用 Q 清线 最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.QLaneClearCount", "使用 Q 清线 小兵数量").SetValue(new Slider(2, 1, 6)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseWLaneClear", "使用 W 清线").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.WMiniManaLaneClear", "使用 W 清线 最低蓝量").SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.WLaneClearCount", "使用 W 清线 小兵数量").SetValue(new Slider(4, 1, 6)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseELaneClear", "使用 E 清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.EMiniManaLaneClear", "使用 E 清线 最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.EMiniManaLaneClearK", "清线仅使用E补刀 最低蓝量").SetValue(new Slider(70, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseEOnlyLastHitLaneClear", "仅使用E对中毒小兵补刀").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseELastHitLaneClearNoPoisoned", "使用E对未中毒小兵清线").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.EDelayLaneClear", "清线时 E的延迟").SetValue(new Slider(0, 0, 2000)));

            Config.AddSubMenu(new Menu("清野", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.UseQJungleClear", "使用 Q 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.QMiniManaJungleClear", "使用 Q 清野 最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.UseWJungleClear", "使用 W 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.WMiniManaJungleClear", "使用 W 清野 最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.UseEJungleClear", "使用 E 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.EMiniManaJungleClear", "使用 E 清野 最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.EDelayJungleClear", "清线时 E的延迟").SetValue(new Slider(0, 0, 2000)));

            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc").AddSubMenu(new Menu("自动R中断法术", "Auto R On Interruptable"));
            Config.SubMenu("Misc").SubMenu("Auto R On Interruptable").AddItem(new MenuItem("Cassiopeia.InterruptSpells", "自动R中断法术").SetValue(true));
            
            foreach (Obj_AI_Hero Champion in EnemyTeam)
            {
                Config.SubMenu("Misc").SubMenu("Auto R On Interruptable").AddItem(new MenuItem(Champion.ChampionName + "INT", "使用 R 中断法术对象 " + Champion.ChampionName).SetValue(true));
            }
            Config.SubMenu("Misc").AddSubMenu(new Menu("自动R防止突进", "Auto R On Gapclosers"));
            Config.SubMenu("Misc").SubMenu("Auto R On Gapclosers").AddItem(new MenuItem("Cassiopeia.AutoRGC", "自动R防止突进").SetValue(true));          
            foreach (Obj_AI_Hero Champion in EnemyTeam)
            {
                Config.SubMenu("Misc").SubMenu("Auto R On Gapclosers").AddItem(new MenuItem(Champion.ChampionName + "GC", "使用 R 防止突进目标 " + Champion.ChampionName).SetValue(true));
            }
            Config.SubMenu("Misc").SubMenu("Auto R On Gapclosers").AddSubMenu(new Menu("优先R防止突进设置", "Advanced R On GapCloser Option"));
            Config.SubMenu("Misc").SubMenu("Auto R On Gapclosers").SubMenu("Advanced R On GapCloser Option").AddItem(new MenuItem("Cassiopeia.AutoRGCIfKillable", "自动R 可击杀的突进者").SetValue(false));
            Config.SubMenu("Misc").SubMenu("Auto R On Gapclosers").SubMenu("Advanced R On GapCloser Option").AddItem(new MenuItem("Cassiopeia.AutoRGCEnCount", "自动R 突进者人数").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("Misc").SubMenu("Auto R On Gapclosers").SubMenu("Advanced R On GapCloser Option").AddItem(new MenuItem("Cassiopeia.AutoRGCMiniHp", "自动R 突进者血量").SetValue(new Slider(40, 0, 100)));
            Config.SubMenu("Misc").AddSubMenu(new Menu("女神堆叠设置", "Tear Stacking Menu"));
            Config.SubMenu("Misc").SubMenu("Tear Stacking Menu").AddItem(new MenuItem("Cassiopeia.StackTearInFountain", "在泉水自动堆叠女神").SetValue(true));
            Config.SubMenu("Misc").SubMenu("Tear Stacking Menu").AddItem(new MenuItem("Cassiopeia.AutoQTear", "自动使用Q堆叠 当可Q中目标的情况下").SetValue(false));
            Config.SubMenu("Misc").SubMenu("Tear Stacking Menu").AddItem(new MenuItem("Cassiopeia.AutoQTearMinMana", "自动使用Q堆叠 最低蓝量").SetValue(new Slider(90, 0, 100)));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoPotion", "自动吃药").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoLevelSpell", "自动加点").SetValue(true));


            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.QRange", "Q 范围").SetValue(new Circle(true, Color.Indigo)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.WRange", "W 范围").SetValue(new Circle(true, Color.Indigo)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.ERange", "E 范围").SetValue(new Circle(true, Color.Green)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.RRange", "R 范围").SetValue(new Circle(true, Color.Gold)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.DrawOrbwalkTarget", "显示 走砍目标").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;


        }

        private static string[] GapCloserChampions = new string[] 
        { "Ahri", "Bard", "Caitlyn", "Corki", "Gragas", "Graves", "Kalista", "Leona", "Lucian",
          "Rek'Sai", "Renekton", "Riven", "Sejuani", "Shen", "Shyvana", "Tryndamere", "Vayne", "Vi", "Aatrox",
          "Fizz", "Gnar", "Hecarim", "Kha'Zix", "LeBlanc", "Malphite", "Nidalee", "Tristana", "Zac", "Alistar",
          "Akali", "Azir", "Braum", "Diana", "Elise", "Fiora", "Irelia", "Jax", "Jayce", "Lee Sin", "Maokai", "MasterYi",
          "Nocturne", "Pantheon", "Poppy", "Quinn", "Rengar", "Thresh", "Vi", "MonkeyKing", "XinZhao", "Yasuo", "Zed", "Jarvan IV" };

        #region ToogleOrder Game_OnUpdate
        public static void Game_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("Cassiopeia.AutoLevelSpell").GetValue<bool>()) LevelUpSpells();

            if (Player.IsDead) return;

            if (Player.IsRecalling()) return;

            ManaManager();
            PotionManager();

            KillSteal();

            if (Config.Item("Cassiopeia.StackTearInFountain").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.InFountain() && Player.ManaPercent >= Config.Item("Cassiopeia.AutoQTearMinMana").GetValue<Slider>().Value &&
                (TearoftheGoddess.IsOwned(Player) || TearoftheGoddessCrystalScar.IsOwned(Player) || ArchangelsStaff.IsOwned(Player) || ArchangelsStaffCrystalScar.IsOwned(Player) || Manamune.IsOwned(Player) || ManamuneCrystalScar.IsOwned(Player)))
                Q.Cast(ObjectManager.Player, true, true);

            if (Config.Item("Cassiopeia.AutoWOnStunTarget").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget() && target.HasBuffOfType(BuffType.Stun) && Player.Distance(target) < W.Range)
                {
                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                }
            }

            if (Config.Item("Cassiopeia.AssistedRKey").GetValue<KeyBind>().Active)
            {
                if (Config.Item("Cassiopeia.MoveOnCursorWhenAssistedRKey").GetValue<bool>())
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    AssistedR();
                }

                if (!Config.Item("Cassiopeia.MoveOnCursorWhenAssistedRKey").GetValue<bool>())
                {
                    AssistedR();
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Orbwalking.Attack = true;
                LaneClear();
                JungleClear();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                Orbwalking.Attack = true;
                LastHit();
            }

            if (Config.Item("Cassiopeia.AutoQTear").GetValue<bool>() && Q.IsReady() && Player.ManaPercent >= Config.Item("Cassiopeia.AutoQTearMinMana").GetValue<Slider>().Value)
            {
                if (TearoftheGoddess.IsOwned(Player) || TearoftheGoddessCrystalScar.IsOwned(Player) || ArchangelsStaff.IsOwned(Player) || ArchangelsStaffCrystalScar.IsOwned(Player) || Manamune.IsOwned(Player) || ManamuneCrystalScar.IsOwned(Player))
                {
                    var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                    var allMinionsQ = MinionManager.GetMinions(Player.Position, Q.Range + 90f, MinionTypes.All, MinionTeam.Enemy);

                    if (Player.Distance(target) < Q.Range - 50)
                    {
                        Q.Cast(target, true, true);
                        return;
                    }
                    else if (allMinionsQ.Any() && Player.CountEnemiesInRange(1500) < 1)
                    {

                        var farmAll = Q.GetCircularFarmLocation(allMinionsQ, 90f);
                        if (farmAll.MinionsHit >= 2 || allMinionsQ.Count < 2)
                        {
                            Q.Cast(farmAll.Position, true);
                            return;
                        }

                    }

                }
            }

            if (Config.Item("Cassiopeia.HarassActive").GetValue<KeyBind>().Active || Config.Item("Cassiopeia.HarassActiveT").GetValue<KeyBind>().Active)
            {
                Orbwalking.Attack = true;
                Harass();
            }


            if ((!Config.Item("Cassiopeia.ToogleUseELastHitOption").GetValue<bool>() || (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Config.Item("Cassiopeia.ToogleUseELastHitOption").GetValue<bool>())) &&
                Config.Item("Cassiopeia.ToogleUseELastHit").GetValue<bool>() && E.IsReady() && (Config.Item("Cassiopeia.ToogleUseELastHitMode").GetValue<StringList>().SelectedIndex == 1 || (Config.Item("Cassiopeia.ToogleUseELastHitMode").GetValue<StringList>().SelectedIndex != 1 && Player.CountEnemiesInRange(1500) == 0)))
            {
                MinionCount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                foreach (var minion1 in MinionCount.Where(x => x.HasBuffOfType(BuffType.Poison)))
                {
                    if (GetEDamage(minion1) * 0.75 > minion1.Health)
                    {
                        E.Cast(minion1);
                        lastCastE = Environment.TickCount;
                    }
                }
            }

            switch (Config.Item("Cassiopeia.ToogleUseELastHitMode").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    {
                        if ((!Config.Item("Cassiopeia.ToogleUseELastHitOption").GetValue<bool>() || (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Config.Item("Cassiopeia.ToogleUseELastHitOption").GetValue<bool>())) && Player.CountEnemiesInRange(1500) == 0 && Config.Item("Cassiopeia.ToogleUseELastHit").GetValue<bool>() && E.IsReady())
                        {
                            MinionCount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                            foreach (var minion1 in MinionCount.Where(x => x.HasBuffOfType(BuffType.Poison)))
                            {
                                if (GetEDamage(minion1) * 0.75 > minion1.Health)
                                {
                                    E.Cast(minion1);
                                    lastCastE = Environment.TickCount;
                                }
                            }
                        }
                        break;
                    }

                case 1:
                    {
                        if ((!Config.Item("Cassiopeia.ToogleUseELastHitOption").GetValue<bool>() || (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Config.Item("Cassiopeia.ToogleUseELastHitOption").GetValue<bool>())) && Config.Item("Cassiopeia.ToogleUseELastHit").GetValue<bool>() && E.IsReady())
                        {
                            MinionCount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                            foreach (var minion1 in MinionCount.Where(x => x.HasBuffOfType(BuffType.Poison)))
                            {
                                if (GetEDamage(minion1) * 0.75 > minion1.Health)
                                {
                                    E.Cast(minion1);
                                    lastCastE = Environment.TickCount;
                                }
                            }
                        }
                        break;
                    }

            }

        }
        #endregion

        #region Interrupt OnProcessSpellCast
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {

            double InterruptOn = SpellToInterrupt(args.SData.Name);
            if (Config.Item(unit.BaseSkinName + "INT").GetValue<bool>() && Config.Item("Cassiopeia.InterruptSpells").GetValue<bool>() && unit.Team != ObjectManager.Player.Team && InterruptOn >= 0f && unit.IsValidTarget(R.Range))
            {
                if (R.IsReady() && Player.Mana > RMANA && Player.Distance(unit) < R.Range - 50 && unit.IsFacing(Player))
                {
                    R.CastIfHitchanceEquals(unit, HitChance.High, true);
                }
            }

            if (Config.Item("Cassiopeia.HarassActive").GetValue<KeyBind>().Active || Config.Item("Cassiopeia.HarassActiveT").GetValue<KeyBind>().Active)
            {
                if (Config.Item("Cassiopeia.AutoQWhenEnemyCastHarass").GetValue<bool>() && Player.ManaPercent >= Config.Item("Cassiopeia.QEnemyAttackMiniManaHarass").GetValue<Slider>().Value && (unit.IsValid<Obj_AI_Hero>() && !unit.IsValid<Obj_AI_Turret>()) && unit.IsEnemy && args.Target.IsMe && Q.IsReady() && Player.Distance(unit) <= Q.Range)
                {
                    Q.CastIfHitchanceEquals(unit, HitChance.High, true);
                }

                if (Config.Item("Cassiopeia.AutoWWhenEnemyCastHarass").GetValue<bool>() && Player.ManaPercent >= Config.Item("Cassiopeia.WEnemyAttackMiniManaHarass").GetValue<Slider>().Value && (unit.IsValid<Obj_AI_Hero>() && !unit.IsValid<Obj_AI_Turret>()) && unit.IsEnemy && args.Target.IsMe && W.IsReady() && Player.Distance(unit) <= W.Range)
                {
                    W.CastIfHitchanceEquals(unit, HitChance.High, true);
                }
            }


        }
        #endregion

        #region AntiGapCloser
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

            if (Config.Item(gapcloser.Sender.ChampionName + "GC").GetValue<bool>() && Config.Item("Cassiopeia.AutoRGCEnCount").GetValue<Slider>().Value <= Player.CountEnemiesInRange(1500) && Config.Item("Cassiopeia.AutoRGC").GetValue<bool>() && (getComboDamage(gapcloser.Sender) > gapcloser.Sender.Health || !Config.Item("Cassiopeia.AutoRGCIfKillable").GetValue<bool>()) && R.IsReady() && Player.Mana >= RMANA && gapcloser.Sender.IsValidTarget(R.Range) && gapcloser.Sender.IsFacing(Player) && Config.Item("Cassiopeia.AutoRGCMiniHp").GetValue<Slider>().Value > Player.HealthPercent)
            {
                R.Cast(gapcloser.Sender.ServerPosition, true);
            }

        }
        #endregion

        #region Combo
        public static void Combo()
        {

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                switch (Config.Item("Cassiopeia.AA1").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        {
                            var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                            if (t.IsValidTarget() && (!E.IsReady() || !IsPoisoned(targetE) && (!Q.IsReady() && !W.IsReady() && Player.Distance(t) < (Q.Range / 4)) || (Player.Mana < QMANA)))
                                Orbwalking.Attack = true;
                            else
                                Orbwalking.Attack = false;
                            break;
                        }

                    case 1:
                        {
                            var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                            if (t.IsValidTarget() && (ObjectManager.Player.GetAutoAttackDamage(t) > t.Health || (Player.Mana < QMANA || Player.Mana < WMANA || Player.Mana < EMANA) || (!IsPoisoned(t) && !Q.IsReady() && !W.IsReady() && Player.Distance(t) < Player.AttackRange)) && (!E.IsReady() || !IsPoisoned(targetE)))
                                Orbwalking.Attack = true;
                            else
                                Orbwalking.Attack = false;
                            break;
                        }

                    case 2:
                        {
                            var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                            if (t.IsValidTarget() && ObjectManager.Player.GetAutoAttackDamage(t) > t.Health || (Player.Mana < QMANA || Player.Mana < WMANA || Player.Mana < EMANA))
                                Orbwalking.Attack = true;
                            else
                                Orbwalking.Attack = false;
                            break;
                        }

                }

            }


            var useQ = Config.Item("Cassiopeia.UseQCombo").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWCombo").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseECombo").GetValue<bool>();
            var useR = Config.Item("Cassiopeia.UseRCombo").GetValue<bool>();
            var LegitE = Config.Item("Cassiopeia.LegitE").GetValue<bool>();


            if (target.IsValidTarget())
            {

                #region Sort R combo mode
                if (useR && R.IsReady() && Player.Mana >= RMANA && targetE.Distance(Player.Position) < R.Range)
                {
                    RLogic();
                }
                #endregion

                if (Config.Item("Cassiopeia.SwapHitChanceOnDashingChamp").GetValue<bool>() && Config.Item(target.ChampionName + "Swap").GetValue<bool>())
                {
                    if (Config.Item("Cassiopeia.StartECombo").GetValue<bool>() && !IsPoisoned(target) && (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare)))
                    {

                        switch (Config.Item("Cassiopeia.PoisonOrder").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                {
                                    #region Sort E combo mode
                                    if (useE && E.IsReady() && ((useQ && Q.IsReady()) || (useW && W.IsReady())) && Player.Mana >= EMANA + QMANA + WMANA && targetE.Distance(Player.Position) <= E.Range && targetE.Distance(Player.Position) > E.Range - 100)
                                    {
                                        E.Cast(targetE, true);
                                    }
                                    #endregion

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;

                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;
                                                    }

                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    #region Sort W combo mode
                                    else if (useW && W.IsReady() && Player.Mana >= WMANA && !Q.IsReady())
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }


                                    }
                                    #endregion

                                    break;
                                }

                            case 1:
                                {

                                    #region Sort E combo mode
                                    if (useE && E.IsReady() && ((useQ && Q.IsReady()) || (useW && W.IsReady())) && Player.Mana >= EMANA + QMANA + WMANA && targetE.Distance(Player.Position) <= E.Range && targetE.Distance(Player.Position) > E.Range - 100)
                                    {
                                        E.Cast(targetE, true);
                                    }
                                    #endregion

                                    #region Sort W combo mode
                                    if (useW && W.IsReady() && Player.Mana >= WMANA)
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }

                                    }
                                    #endregion

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && !W.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    break;
                                }

                        }
                    }
                    if (!Config.Item("Cassiopeia.StartECombo").GetValue<bool>() || (Config.Item("Cassiopeia.StartECombo").GetValue<bool>() && ((!target.HasBuffOfType(BuffType.Stun) && !target.HasBuffOfType(BuffType.Knockup) && !target.HasBuffOfType(BuffType.Snare) || IsPoisoned(target)))))
                    {
                        #region Sort E combo mode
                        if (useE && E.IsReady() && IsPoisoned(targetE) && Player.Mana >= EMANA && targetE.Distance(Player.Position) <= E.Range && !LegitE)
                        {
                            E.Cast(targetE, true);
                        }
                        #endregion

                        #region Sort E combo mode LegitE
                        if (useE && E.IsReady() && Player.Mana >= EMANA && LegitE)
                        {
                            if (targetE.Distance(Player.Position) <= 350 && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayCombo350").GetValue<Slider>().Value)
                            {
                                if (IsPoisoned(targetE))
                                {
                                    E.CastOnUnit(targetE, true);
                                    lastCastE = Environment.TickCount;
                                    return;
                                }
                            }

                            else if (targetE.Distance(Player.Position) <= 525 && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayCombo525").GetValue<Slider>().Value)
                            {
                                if (IsPoisoned(targetE))
                                {
                                    E.CastOnUnit(targetE, true);
                                    lastCastE = Environment.TickCount;
                                    return;
                                }
                            }

                            else if (targetE.Distance(Player.Position) <= E.Range && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayComboERange").GetValue<Slider>().Value)
                            {
                                if (IsPoisoned(targetE))
                                {
                                    E.CastOnUnit(targetE, true);
                                    lastCastE = Environment.TickCount;
                                    return;
                                }
                            }


                        }
                        #endregion

                        switch (Config.Item("Cassiopeia.PoisonOrder").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                {

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;

                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;
                                                    }

                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    #region Sort W combo mode
                                    else if (useW && W.IsReady() && Player.Mana >= WMANA && !Q.IsReady())
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }


                                    }
                                    #endregion

                                    break;
                                }

                            case 1:
                                {

                                    #region Sort W combo mode
                                    if (useW && W.IsReady() && Player.Mana >= WMANA)
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogicVeryHigh();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }

                                    }
                                    #endregion

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && !W.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogicVeryHigh();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogicVeryHigh();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    break;
                                }

                        }
                    }
                }

                if (!Config.Item("Cassiopeia.SwapHitChanceOnDashingChamp").GetValue<bool>() || !Config.Item(target.ChampionName + "Swap").GetValue<bool>())
                {

                    if (Config.Item("Cassiopeia.StartECombo").GetValue<bool>() && !IsPoisoned(target) && (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare)))
                    {

                        switch (Config.Item("Cassiopeia.PoisonOrder").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                {
                                    #region Sort E combo mode
                                    if (useE && E.IsReady() && ((useQ && Q.IsReady()) || (useW && W.IsReady())) && Player.Mana >= EMANA + QMANA + WMANA && targetE.Distance(Player.Position) <= E.Range && targetE.Distance(Player.Position) > E.Range - 100)
                                    {
                                        E.Cast(targetE, true);
                                    }
                                    #endregion

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;

                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;
                                                    }

                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    #region Sort W combo mode
                                    else if (useW && W.IsReady() && Player.Mana >= WMANA && !Q.IsReady())
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                    }
                                                    break;
                                                }
                                        }


                                    }
                                    #endregion

                                    break;
                                }

                            case 1:
                                {

                                    #region Sort E combo mode
                                    if (useE && E.IsReady() && ((useQ && Q.IsReady()) || (useW && W.IsReady())) && Player.Mana >= EMANA + QMANA + WMANA && targetE.Distance(Player.Position) <= E.Range && targetE.Distance(Player.Position) > E.Range - 100)
                                    {
                                        E.Cast(targetE, true);
                                    }
                                    #endregion

                                    #region Sort W combo mode
                                    if (useW && W.IsReady() && Player.Mana >= WMANA)
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                    }
                                                    break;
                                                }
                                        }

                                    }
                                    #endregion

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && !W.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    break;
                                }

                        }
                    }
                    if (!Config.Item("Cassiopeia.StartECombo").GetValue<bool>() || (Config.Item("Cassiopeia.StartECombo").GetValue<bool>() && ((!target.HasBuffOfType(BuffType.Stun) && !target.HasBuffOfType(BuffType.Knockup) && !target.HasBuffOfType(BuffType.Snare) || IsPoisoned(target)))))
                    {
                        #region Sort E combo mode
                        if (useE && E.IsReady() && IsPoisoned(targetE) && Player.Mana >= EMANA && targetE.Distance(Player.Position) <= E.Range && !LegitE)
                        {
                            E.Cast(targetE, true);
                        }
                        #endregion

                        #region Sort E combo mode LegitE
                        if (useE && E.IsReady() && Player.Mana >= EMANA && LegitE)
                        {
                            if (targetE.Distance(Player.Position) <= 350 && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayCombo350").GetValue<Slider>().Value)
                            {
                                if (IsPoisoned(targetE))
                                {
                                    E.CastOnUnit(targetE, true);
                                    lastCastE = Environment.TickCount;
                                    return;
                                }
                            }

                            else if (targetE.Distance(Player.Position) <= 525 && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayCombo525").GetValue<Slider>().Value)
                            {
                                if (IsPoisoned(targetE))
                                {
                                    E.CastOnUnit(targetE, true);
                                    lastCastE = Environment.TickCount;
                                    return;
                                }
                            }

                            else if (targetE.Distance(Player.Position) <= E.Range && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayComboERange").GetValue<Slider>().Value)
                            {
                                if (IsPoisoned(targetE))
                                {
                                    E.CastOnUnit(targetE, true);
                                    lastCastE = Environment.TickCount;
                                    return;
                                }
                            }


                        }
                        #endregion

                        switch (Config.Item("Cassiopeia.PoisonOrder").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                {

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;

                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;
                                                    }

                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    #region Sort W combo mode
                                    else if (useW && W.IsReady() && Player.Mana >= WMANA && !Q.IsReady())
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                    }
                                                    break;
                                                }
                                        }


                                    }
                                    #endregion

                                    break;
                                }

                            case 1:
                                {

                                    #region Sort W combo mode
                                    if (useW && W.IsReady() && Player.Mana >= WMANA)
                                    {

                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastQ) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < W.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastQ) >= 1000)
                                                    {
                                                        WLogic();
                                                        lastCastW = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < W.Range)
                                                    {
                                                        WLogic();
                                                    }
                                                    break;
                                                }
                                        }

                                    }
                                    #endregion

                                    #region Sort Q combo mode
                                    if (useQ && Q.IsReady() && !W.IsReady() && Player.Mana >= QMANA)
                                    {
                                        switch (Config.Item("Cassiopeia.PoisonStack").GetValue<StringList>().SelectedIndex)
                                        {

                                            case 0:
                                                {
                                                    if (Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() && Player.CountEnemiesInRange(1300) < 2 && Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && (target.HasBuffOfType(BuffType.Poison) || (Environment.TickCount - lastCastW) < 1000))
                                                    {
                                                        return;
                                                    }
                                                    else if ((!Config.Item("Cassiopeia.PoisonStack1VS1").GetValue<bool>() || Player.CountEnemiesInRange(1300) != 1) && Player.Distance(target) < Q.Range && !target.HasBuffOfType(BuffType.Poison) && (Environment.TickCount - lastCastW) >= 1000)
                                                    {
                                                        QLogic();
                                                        lastCastQ = Environment.TickCount;
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Player.Distance(target) < Q.Range)
                                                    {
                                                        QLogic();
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion

                                    break;
                                }

                        }
                    }
                }

            }

        }
        #endregion

        #region Harass
        public static void Harass()
        {

            var useQ = Config.Item("Cassiopeia.UseQHarass").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWHarass").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseEHarass").GetValue<bool>();

            var HavemanaQ = Player.ManaPercent >= Config.Item("Cassiopeia.QMiniManaHarass").GetValue<Slider>().Value;
            var HavemanaW = Player.ManaPercent >= Config.Item("Cassiopeia.WMiniManaHarass").GetValue<Slider>().Value;
            var HavemanaE = Player.ManaPercent >= Config.Item("Cassiopeia.EMiniManaHarass").GetValue<Slider>().Value;


            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (useQ && HavemanaQ)
            {
                if (Player.Distance(target) <= Q.Range)
                {
                    Q.Cast(target, true, true);
                }
            }

            if (useE && HavemanaE && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayHarass").GetValue<Slider>().Value)
            {
                if (Player.Distance(target) <= E.Range)
                {
                    if (IsPoisoned(target))
                    {
                        E.CastOnUnit(target, true);
                        lastCastE = Environment.TickCount;
                    }
                }
            }

            if (useW && HavemanaW)
            {
                if (Player.Distance(target) <= W.Range)
                {
                    W.Cast(target, true, true);
                }
            }


        }
        #endregion

        #region LastHit
        public static void LastHit()
        {
            var useE = Config.Item("Cassiopeia.UseELastHit").GetValue<bool>();

            if (useE && E.IsReady() && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayLastHit").GetValue<Slider>().Value)
            {
                MinionCount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                foreach (var minion1 in MinionCount.Where(x => Config.Item("Cassiopeia.UseELastHitNoPoisoned").GetValue<bool>() || x.HasBuffOfType(BuffType.Poison)))
                {
                    var buffEndTime = Config.Item("Cassiopeia.UseELastHitNoPoisoned").GetValue<bool>() ? float.MaxValue : GetPoisonBuffTimer(minion1);
                    if (buffEndTime > Game.Time + E.Delay)
                    {

                        if (GetEDamage(minion1) * 0.62 > minion1.Health)
                        {
                            Orbwalking.Attack = false;
                            E.Cast(minion1);
                            lastCastE = Environment.TickCount;
                        }

                    }
                }
            }

        }
        #endregion

        #region LaneClear
        public static void LaneClear()
        {

            var useQ = Config.Item("Cassiopeia.UseQLaneClear").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWLaneClear").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseELaneClear").GetValue<bool>();

            var HavemanaQ = Player.ManaPercent >= Config.Item("Cassiopeia.QMiniManaLaneClear").GetValue<Slider>().Value;
            var HavemanaW = Player.ManaPercent >= Config.Item("Cassiopeia.WMiniManaLaneClear").GetValue<Slider>().Value;
            var HavemanaE = Player.ManaPercent >= Config.Item("Cassiopeia.EMiniManaLaneClear").GetValue<Slider>().Value;
            var HavemanaEK = Player.ManaPercent >= Config.Item("Cassiopeia.EMiniManaLaneClearK").GetValue<Slider>().Value;

            var CountQ = Config.Item("Cassiopeia.QLaneClearCount").GetValue<Slider>().Value;
            var CountW = Config.Item("Cassiopeia.WLaneClearCount").GetValue<Slider>().Value;

            if (Q.IsReady() && useQ && HavemanaQ)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All, MinionTeam.Enemy);

                if (allMinionsQ.Any())
                {
                    var farmAll = Q.GetCircularFarmLocation(allMinionsQ, 150f);
                    if (farmAll.MinionsHit >= CountQ)
                    {
                        Q.Cast(farmAll.Position, true);
                        return;
                    }
                }
            }

            if (W.IsReady() && useW && HavemanaW)
            {
                var allMinionsW = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All);

                if (allMinionsW.Any())
                {
                    var farmAll = W.GetCircularFarmLocation(allMinionsW, W.Width * 0.8f);
                    if (farmAll.MinionsHit >= CountW)
                    {
                        W.Cast(farmAll.Position, true);
                        return;
                    }
                }
            }

            if (E.IsReady() && useE && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayLaneClear").GetValue<Slider>().Value)
            {

                MinionCount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);

                foreach (var minion in MinionCount.Where(x => Config.Item("Cassiopeia.UseELastHitLaneClearNoPoisoned").GetValue<bool>() || x.HasBuffOfType(BuffType.Poison)))
                {
                    var buffEndTime = Config.Item("Cassiopeia.UseELastHitLaneClearNoPoisoned").GetValue<bool>() ? float.MaxValue : GetPoisonBuffTimer(minion);
                    if (buffEndTime > Game.Time + E.Delay)
                    {
                        if (Config.Item("Cassiopeia.UseEOnlyLastHitLaneClear").GetValue<bool>() || HavemanaE)
                        {

                            if (GetEDamage(minion) * 0.82 > minion.Health)
                            {
                                Orbwalking.Attack = false;
                                E.Cast(minion);
                                lastCastE = Environment.TickCount;
                            }
                        }
                        if (!Config.Item("Cassiopeia.UseEOnlyLastHitLaneClear").GetValue<bool>() && HavemanaEK)
                        {

                            if (GetEDamage(minion) * 1.50 < minion.Health)
                            {
                                Orbwalking.Attack = false;
                                E.Cast(minion);
                                lastCastE = Environment.TickCount;
                            }

                        }
                    }
                }
            }
            
        }
        #endregion

        #region JungleClear
        public static void JungleClear()
        {

            var useQ = Config.Item("Cassiopeia.UseQJungleClear").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWJungleClear").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseEJungleClear").GetValue<bool>();

            var HavemanaQ = Player.ManaPercent >= Config.Item("Cassiopeia.QMiniManaJungleClear").GetValue<Slider>().Value;
            var HavemanaW = Player.ManaPercent >= Config.Item("Cassiopeia.WMiniManaJungleClear").GetValue<Slider>().Value;
            var HavemanaE = Player.ManaPercent >= Config.Item("Cassiopeia.EMiniManaJungleClear").GetValue<Slider>().Value;

            var MinionN = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (!MinionN.IsValidTarget() || MinionN == null)
            {
                LaneClear();
                return;
            }

            if (useQ && Q.IsReady() && HavemanaQ)
            {
                var allMonsterQ = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                var farmAll = Q.GetCircularFarmLocation(allMonsterQ, 150f);
                if (farmAll.MinionsHit >= 1)
                {
                    Q.Cast(farmAll.Position, true);
                    return;
                }

            }

            if (useW && W.IsReady() && HavemanaW)
            {
                if (Player.Distance(MinionN) <= W.Range)
                {
                    W.CastIfHitchanceEquals(MinionN, HitChance.High);
                }

            }

            if (useE && E.IsReady() && HavemanaE && (Environment.TickCount - lastCastE) >= Config.Item("Cassiopeia.EDelayJungleClear").GetValue<Slider>().Value)
            {
                if (Player.Distance(MinionN) <= E.Range && IsPoisoned(MinionN))
                {
                    E.CastOnUnit(MinionN);
                    lastCastE = Environment.TickCount;
                }

            }

        }
        #endregion

        #region KillSteal
        public static void KillSteal()
        {

            var UseIgniteKS = Config.Item("Cassiopeia.UseIgniteKS").GetValue<bool>();
            var UseENPKS = Config.Item("Cassiopeia.UseENPKS").GetValue<bool>();
            var UseEPKS = Config.Item("Cassiopeia.UseEPKS").GetValue<bool>();

            var UseENPKSCount = Config.Item("Cassiopeia.EDelayCombo350").GetValue<Slider>().Value;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => !target.IsMe && target.Team != ObjectManager.Player.Team))
            {

                if (!target.HasBuff("SionPassiveZombie") && !target.HasBuffOfType(BuffType.Invulnerability) && !target.HasBuffOfType(BuffType.SpellImmunity))
                {

                    if (UseEPKS && E.IsReady() && Player.Mana >= EMANA && target.Health < E.GetDamage(target) && Player.Distance(target) <= E.Range && !target.IsDead && target.IsValidTarget() && IsPoisoned(target))
                    {
                        E.CastOnUnit(target, true);
                        return;
                    }

                    if (UseEPKS && E.IsReady() && Player.Mana >= EMANA * 2 && target.Health < E.GetDamage(target) * 2 && Player.Distance(target) <= E.Range && !target.IsDead && target.IsValidTarget() && IsPoisoned(target))
                    {
                        E.CastOnUnit(target, true);
                        return;
                    }

                    if (UseENPKS && E.IsReady() && Player.Mana >= EMANA && target.Health < E.GetDamage(target) && Player.Distance(target) <= E.Range && !target.IsDead && target.IsValidTarget() && !IsPoisoned(target) && Player.CountEnemiesInRange(1300) <= UseENPKSCount)
                    {
                        E.CastOnUnit(target, true);
                        return;
                    }

                    if (UseIgniteKS && Ignite.Slot != SpellSlot.Unknown && target.Health < Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) && Player.Distance(target) <= Ignite.Range && !target.IsDead && target.IsValidTarget())
                    {
                        Ignite.Cast(target, true);
                        return;
                    }

                    if (UseEPKS && UseIgniteKS && E.IsReady() && Ignite.Slot != SpellSlot.Unknown && Player.Mana >= EMANA && target.Health < E.GetDamage(target) + Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) && Player.Distance(target) <= Ignite.Range && !target.IsDead && target.IsValidTarget() && IsPoisoned(target))
                    {
                        E.CastOnUnit(target, true);
                        return;
                    }

                    if (UseEPKS && UseIgniteKS && E.IsReady() && Ignite.Slot != SpellSlot.Unknown && Player.Mana >= EMANA * 2 && target.Health < (E.GetDamage(target) * 2) + Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) && Player.Distance(target) <= Ignite.Range && !target.IsDead && target.IsValidTarget() && IsPoisoned(target))
                    {
                        E.CastOnUnit(target, true);
                        return;
                    }

                    if (UseENPKS && UseIgniteKS && E.IsReady() && Ignite.Slot != SpellSlot.Unknown && Player.Mana >= EMANA && target.Health < E.GetDamage(target) + Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) && Player.Distance(target) <= Ignite.Range && !target.IsDead && target.IsValidTarget() && !IsPoisoned(target) && Player.CountEnemiesInRange(1300) <= UseENPKSCount)
                    {
                        E.CastOnUnit(target, true);
                        return;
                    }

                }

            }
        }
        #endregion

        #region PlayerDamage
        public static float getComboDamage(Obj_AI_Hero target)
        {
            float damage = 0f;
            if (Config.Item("Cassiopeia.UseQCombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA)
                {
                    damage += Q.GetDamage(target) * 1.5f;
                }
            }
            if (Config.Item("Cassiopeia.UseWCombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA + WMANA)
                {
                    damage += W.GetDamage(target);
                }
            }
            if (Config.Item("Cassiopeia.UseECombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA + WMANA + (EMANA * 4))
                {
                    damage += E.GetDamage(target) * 4f;
                }
            }
            if (R.IsReady() && Player.Mana >= QMANA + WMANA + (EMANA * 4) + RMANA)
            {
                damage += R.GetDamage(target);
            }
            if (Ignite.Slot != SpellSlot.Unknown)
            {
                damage += (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            return damage;
        }


        public static float getComboDamageNoUlt(Obj_AI_Hero target)
        {
            float damage = 0f;
            if (Config.Item("Cassiopeia.UseQCombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA)
                {
                    damage += Q.GetDamage(target) * 1.5f;
                }
            }
            if (Config.Item("Cassiopeia.UseWCombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA + WMANA)
                {
                    damage += W.GetDamage(target);
                }
            }
            if (Config.Item("Cassiopeia.UseECombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA + WMANA + (EMANA * 4))
                {
                    damage += E.GetDamage(target) * 4f;
                }
            }
            if (Ignite.Slot != SpellSlot.Unknown)
            {
                damage += (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            return damage;
        }


        public static float getMiniComboDamage(Obj_AI_Hero target)
        {
            float damage = 0f;
            if (Config.Item("Cassiopeia.UseQCombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA)
                {
                    damage += Q.GetDamage(target) * 1f;
                }
            }
            if (Config.Item("Cassiopeia.UseWCombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA + WMANA)
                {
                    damage += W.GetDamage(target);
                }
            }
            if (Config.Item("Cassiopeia.UseECombo").GetValue<bool>())
            {
                if (Player.Mana >= QMANA + WMANA + (EMANA * 2))
                {
                    damage += E.GetDamage(target) * 2f;
                }
            }
            if (Ignite.Slot != SpellSlot.Unknown)
            {
                damage += (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            return damage;
        }
        #endregion

        #region Interrupt Spell List
        public static double SpellToInterrupt(string SpellName)
        {
            if (SpellName == "KatarinaR")
                return 0;
            if (SpellName == "AlZaharNetherGrasp")
                return 0;
            if (SpellName == "GalioIdolOfDurand")
                return 0;
            if (SpellName == "LuxMaliceCannon")
                return 0;
            if (SpellName == "MissFortuneBulletTime")
                return 0;
            if (SpellName == "CaitlynPiltoverPeacemaker")
                return 0;
            if (SpellName == "EzrealTrueshotBarrage")
                return 0;
            if (SpellName == "InfiniteDuress")
                return 0;
            if (SpellName == "VelkozR")
                return 0;
            if (SpellName == "XerathLocusOfPower2")
                return 0;
            if (SpellName == "Crowstorm")
                return 0;
            if (SpellName == "ReapTheWhirlwind")
                return 0;
            if (SpellName == "FallenOne")
                return 0;
            if (SpellName == "KennenShurikenStorm")
                return 0;
            if (SpellName == "LucianR")
                return 0;
            if (SpellName == "SoulShackles")
                return 0;
            if (SpellName == "AbsoluteZero")
                return 0;
            if (SpellName == "SkarnerImpale")
                return 0;
            if (SpellName == "MonkeyKingSpinToWin")
                return 0;
            if (SpellName == "ZacR")
                return 0;
            if (SpellName == "UrgotSwap2")
                return 0;
            return -1;
        }
        # endregion

        #region ManaManager
        public static void ManaManager()
        {

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            RMANA = R.Instance.ManaCost;

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }

        }
        #endregion

        #region PotionManager
        public static void PotionManager()
        {
            if (Player.Level == 1 && Player.CountEnemiesInRange(1000) == 1 && Player.Health >= Player.MaxHealth * 0.35) return;
            if (Player.Level == 1 && Player.CountEnemiesInRange(1000) == 2 && Player.Health >= Player.MaxHealth * 0.50) return;

            if (Config.Item("Cassiopeia.AutoPotion").GetValue<bool>() && !Player.InFountain() && !Player.IsRecalling() && !Player.IsDead)
            {
                #region BiscuitofRejuvenation
                if (BiscuitofRejuvenation.IsReady() && !Player.HasBuff("ItemMiniRegenPotion") && !Player.HasBuff("ItemCrystalFlask"))
                {

                    if (Player.MaxHealth > Player.Health + 170 && Player.MaxMana > Player.Mana + 10 && Player.CountEnemiesInRange(1000) > 0 &&
                        Player.Health < Player.MaxHealth * 0.75)
                    {
                        BiscuitofRejuvenation.Cast();
                    }

                    else if (Player.MaxHealth > Player.Health + 170 && Player.MaxMana > Player.Mana + 10 && Player.CountEnemiesInRange(1000) == 0 &&
                        Player.Health < Player.MaxHealth * 0.6)
                    {
                        BiscuitofRejuvenation.Cast();
                    }

                }
                #endregion

                #region HealthPotion
                else if (HealthPotion.IsReady() && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("ItemCrystalFlask"))
                {

                    if (Player.MaxHealth > Player.Health + 150 && Player.CountEnemiesInRange(1000) > 0 &&
                        Player.Health < Player.MaxHealth * 0.75)
                    {
                        HealthPotion.Cast();
                    }

                    else if (Player.MaxHealth > Player.Health + 150 && Player.CountEnemiesInRange(1000) == 0 &&
                        Player.Health < Player.MaxHealth * 0.6)
                    {
                        HealthPotion.Cast();
                    }

                }
                #endregion

                #region CrystallineFlask
                else if (CrystallineFlask.IsReady() && !Player.HasBuff("ItemCrystalFlask") && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("FlaskOfCrystalWater") && !Player.HasBuff("ItemMiniRegenPotion"))
                {

                    if (Player.MaxHealth > Player.Health + 120 && Player.MaxMana > Player.Mana + 60 && Player.CountEnemiesInRange(1000) > 0 &&
                        (Player.Health < Player.MaxHealth * 0.85 || Player.Mana < Player.MaxMana * 0.65))
                    {
                        CrystallineFlask.Cast();
                    }

                    else if (Player.MaxHealth > Player.Health + 120 && Player.MaxMana > Player.Mana + 60 && Player.CountEnemiesInRange(1000) == 0 &&
                        (Player.Health < Player.MaxHealth * 0.7 || Player.Mana < Player.MaxMana * 0.5))
                    {
                        CrystallineFlask.Cast();
                    }

                }
                #endregion

                #region ManaPotion
                else if (ManaPotion.IsReady() && !Player.HasBuff("FlaskOfCrystalWater") && !Player.HasBuff("ItemCrystalFlask"))
                {

                    if (Player.MaxMana > Player.Mana + 100 && Player.CountEnemiesInRange(1000) > 0 &&
                        Player.Mana < Player.MaxMana * 0.7)
                    {
                        ManaPotion.Cast();
                    }

                    else if (Player.MaxMana > Player.Mana + 100 && Player.CountEnemiesInRange(1000) == 0 &&
                        Player.Mana < Player.MaxMana * 0.4)
                    {
                        ManaPotion.Cast();
                    }

                }
                #endregion
            }
        }
        #endregion

        #region DrawingRange
        public static void Drawing_OnDraw(EventArgs args)
        {

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item("Cassiopeia." + spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && (spell.Slot != SpellSlot.R || R.Level > 0))
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            if (Config.Item("Cassiopeia.DrawOrbwalkTarget").GetValue<bool>())
            {
                var orbT = Orbwalker.GetTarget();
                if (orbT.IsValidTarget())
                    Render.Circle.DrawCircle(orbT.Position, 100, System.Drawing.Color.Pink);
            }
        }
        #endregion

        #region Up Spell
        private static void LevelUpSpells()
        {
            int qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            int wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            int eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            int rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = new int[] { 0, 0, 0, 0 };
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[abilitySequence[i] - 1] = level[abilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);

            }
        }
        #endregion

        #region CheckPoison
        private static bool IsPoisoned(Obj_AI_Base unit)
        {
            return
                unit.Buffs.Where(buff => buff.IsActive && buff.Type == BuffType.Poison)
                    .Any(buff => buff.EndTime >= (Game.Time + 0.35 + 700 / 1900));
        }
        #endregion

        #region CheckBuffTimer
        private static float GetPoisonBuffTimer(Obj_AI_Base target)
        {
            var buffEndTimer = target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Poison)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
            return buffEndTimer;
        }
        #endregion

        #region GetEDamage
        private static double GetEDamage(Obj_AI_Base Etarget)
        {
            var SpellDamage = new DamageSpell { Slot = SpellSlot.E, DamageType = LeagueSharp.Common.Damage.DamageType.Magical, Damage = (source, target, level) => new double[] { 45, 85, 120, 155, 190 }[level] + (0.55 * source.FlatMagicDamageMod) };
            var rawDamage = SpellDamage.Damage(Player, Etarget, Math.Max(1, Math.Min(Player.Spellbook.GetSpell(SpellSlot.Q).Level - 1, 6)));

            return CalcMagicDamage(Player, Etarget, rawDamage);
        }

        private static double CalcMagicDamage(Obj_AI_Base source, Obj_AI_Base target, double amount)
        {
            var magicResist = (target.SpellBlock * source.PercentMagicPenetrationMod) - source.FlatMagicPenetrationMod;

            double k;
            if (magicResist < 0)
            {
                k = 2 - 100 / (100 - magicResist);
            }
            else
            {
                k = 100 / (100 + magicResist);
            }

            k = k * (1 - target.PercentMagicReduction) * (1 + target.PercentMagicDamageMod);

            return k * amount;
        }
        #endregion

        #region QLogic
        public static void QLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Player.CountEnemiesInRange(1300) > 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    if (target.CountAlliesInRange(Q.Width) >= 1)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                {
                                    Q.Cast(target, true, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.Cast(target, true, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.MoveSpeed > target.MoveSpeed)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.MoveSpeed <= target.MoveSpeed)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent <= target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.MoveSpeed > target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                        {
                                            Q.Cast(target, true, true);
                                            return;
                                        }
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            Q.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    if (Player.MoveSpeed <= target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            Q.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent > target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.MoveSpeed > target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                        {
                                            Q.Cast(target, true, true);
                                            return;
                                        }
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            Q.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    if (Player.MoveSpeed <= target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            Q.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (target.CountAlliesInRange(Q.Width) == 0)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent <= target.HealthPercent)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent > target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                    {
                                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    if (target.CountAlliesInRange(Q.Width) >= 1)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                {
                                    Q.Cast(target, true, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.Cast(target, true, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.MoveSpeed > target.MoveSpeed)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.MoveSpeed <= target.MoveSpeed)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.MoveSpeed > target.MoveSpeed)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                }
                                if (Player.MoveSpeed <= target.MoveSpeed)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.Cast(target, true, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (target.CountAlliesInRange(Q.Width) == 0)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent <= target.HealthPercent)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent > target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                    {
                                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }

            if (Player.CountEnemiesInRange(1300) == 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                    return;
                }
                return;
            }
            return;
        }
        #endregion

        #region WLogic
        public static void WLogic()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            if (Player.CountEnemiesInRange(1300) > 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    if (target.CountAlliesInRange(W.Width) >= 1)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                {
                                    W.Cast(target, true, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.Cast(target, true, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.MoveSpeed > target.MoveSpeed)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.MoveSpeed <= target.MoveSpeed)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent <= target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.MoveSpeed > target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                        {
                                            W.Cast(target, true, true);
                                            return;
                                        }
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            W.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    if (Player.MoveSpeed <= target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            W.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent > target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.MoveSpeed > target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                        {
                                            W.Cast(target, true, true);
                                            return;
                                        }
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            W.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    if (Player.MoveSpeed <= target.MoveSpeed)
                                    {
                                        if (Player.Distance(target) <= E.Range)
                                        {
                                            W.Cast(target, true, true);
                                            return;
                                        }
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (target.CountAlliesInRange(W.Width) == 0)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent <= target.HealthPercent)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent > target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                    {
                                        W.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    if (target.CountAlliesInRange(W.Width) >= 1)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                {
                                    W.Cast(target, true, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.Cast(target, true, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.MoveSpeed > target.MoveSpeed)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.MoveSpeed <= target.MoveSpeed)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.MoveSpeed > target.MoveSpeed)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                }
                                if (Player.MoveSpeed <= target.MoveSpeed)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.Cast(target, true, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.Cast(target, true, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (target.CountAlliesInRange(W.Width) == 0)
                    {
                        if (getComboDamageNoUlt(target) > target.Health)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (Player.HealthPercent <= target.HealthPercent)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent > target.HealthPercent)
                            {
                                if (Player.HealthPercent >= 50)
                                {
                                    if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                    {
                                        W.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                if (Player.HealthPercent < 50)
                                {
                                    if (Player.Distance(target) <= E.Range)
                                    {
                                        W.CastIfHitchanceEquals(target, HitChance.High, true);
                                        return;
                                    }
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }

            if (Player.CountEnemiesInRange(1300) == 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    W.CastIfHitchanceEquals(target, HitChance.High, true);
                    return;
                }
                return;
            }
            return;
        }
        #endregion

        #region RLogic
        public static void RLogic()
        {
            var target = TargetSelector.GetTarget(R.Range - 100, TargetSelector.DamageType.Magical);

            if (Player.CountEnemiesInRange(1300) > 1)
            {
                if (target.IsFacing(Player))
                {
                    List<Obj_AI_Hero> targets = HeroManager.Enemies.Where(o => R.WillHit(o, target.Position) && o.Distance(Player.Position) < 500).ToList<Obj_AI_Hero>();
                    if (getMiniComboDamage(target) < target.Health)
                    {
                        

                        if (targets.Count > 1)
                        {
                            R.Cast(target.Position, true);
                            return;
                        }
                        return;
                    }

                    if (targets.Count > 2)
                    {
                        R.Cast(target.Position, true);
                        return;
                    }
                    return;

                }
                if (target.CountAlliesInRange(R.Width) == 0)
                {
                    if (Player.HealthPercent <= target.HealthPercent)
                    {
                        if (Player.HealthPercent < 50)
                        {
                            if (target.IsFacing(Player))
                            {
                                R.CastIfHitchanceEquals(target, HitChance.High, true);
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }
            if (Player.CountEnemiesInRange(1300) == 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    if (Player.HealthPercent <= target.HealthPercent)
                    {
                        if (Player.HealthPercent < 50)
                        {
                            if (target.IsFacing(Player))
                            {
                                R.CastIfHitchanceEquals(target, HitChance.High, true);
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    if (getComboDamage(target) > target.Health)
                    {
                        if (CanEscapeWithFlash(target.ServerPosition.To2D()))
                        {
                            if (target.IsFacing(Player))
                            {
                                if (getMiniComboDamage(target) < target.Health)
                                {
                                    R.CastIfHitchanceEquals(target, HitChance.High, true);
                                    return;
                                }
                            }
                            return;
                        }
                        if (getComboDamageNoUlt(target) < target.Health)
                        {
                            if (target.IsFacing(Player))
                            {
                                R.CastIfHitchanceEquals(target, HitChance.High, true);
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }
            return;
        }
        #endregion

        #region QLogic VeryHigh
        public static void QLogicVeryHigh()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Player.CountEnemiesInRange(1300) > 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    if (getComboDamageNoUlt(target) > target.Health)
                    {
                        if (Player.HealthPercent >= 50)
                        {
                            if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            if (Player.Distance(target) <= E.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        if (Player.HealthPercent < 50)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (getComboDamageNoUlt(target) < target.Health)
                    {
                        if (Player.HealthPercent <= target.HealthPercent)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        if (Player.HealthPercent > target.HealthPercent)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    if (getComboDamageNoUlt(target) > target.Health)
                    {
                        if (Player.HealthPercent >= 50)
                        {
                            if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            if (Player.Distance(target) <= E.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                        }
                        if (Player.HealthPercent < 50)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (getComboDamageNoUlt(target) < target.Health)
                    {
                        if (Player.HealthPercent <= target.HealthPercent)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        if (Player.HealthPercent > target.HealthPercent)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < Q.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }

            if (Player.CountEnemiesInRange(1300) == 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                    return;
                }
                return;
            }
            return;
        }
        #endregion

        #region WLogic VeryHigh
        public static void WLogicVeryHigh()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            if (Player.CountEnemiesInRange(1300) > 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    if (getComboDamageNoUlt(target) > target.Health)
                    {
                        if (Player.HealthPercent >= 50)
                        {
                            if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            if (Player.Distance(target) <= E.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        if (Player.HealthPercent < 50)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (getComboDamageNoUlt(target) < target.Health)
                    {
                        if (Player.HealthPercent <= target.HealthPercent)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        if (Player.HealthPercent > target.HealthPercent)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    if (getComboDamageNoUlt(target) > target.Health)
                    {
                        if (Player.HealthPercent >= 50)
                        {
                            if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            if (Player.Distance(target) <= E.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                        }
                        if (Player.HealthPercent < 50)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (getComboDamageNoUlt(target) < target.Health)
                    {
                        if (Player.HealthPercent <= target.HealthPercent)
                        {
                            if (Player.Distance(target) <= E.Range)
                            {
                                W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                return;
                            }
                            return;
                        }
                        if (Player.HealthPercent > target.HealthPercent)
                        {
                            if (Player.HealthPercent >= 50)
                            {
                                if (Player.Distance(target) > E.Range && Player.Distance(target) < W.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            if (Player.HealthPercent < 50)
                            {
                                if (Player.Distance(target) <= E.Range)
                                {
                                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }

            if (Player.CountEnemiesInRange(1300) == 1)
            {
                if (Player.CountAlliesInRange(1300) >= 1 + 1)
                {
                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                    return;
                }
                if (Player.CountAlliesInRange(1300) == 0 + 1)
                {
                    W.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
                    return;
                }
                return;
            }
            return;
        }
        #endregion

        #region Assisted R
        public static void AssistedR()
        {
            var RList = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && Prediction.GetPrediction(x, R.Delay).UnitPosition.Distance(Player.Position) < R.Range).ToList();
            if (R.IsReady() && Player.Mana >= RMANA && RList.Any())
            {
                Obj_AI_Hero RPos = RList.FirstOrDefault();
                PredictionOutput RPred = R.GetPrediction(RPos, true, R.Range);

                List<Obj_AI_Hero> RHitCount = HeroManager.Enemies.Where(x => R.WillHit(x.Position, RPred.CastPosition)).ToList();
                int IsFacing = RHitCount.Where(x => x.IsFacing(Player)).Count();
                int RHitCountList = RHitCount.Count();

                if ((Config.Item("Cassiopeia.AssistedRFacing").GetValue<bool>() && IsFacing >= Config.Item("Cassiopeia.AssistedRFacingCount").GetValue<Slider>().Value) || (Config.Item("Cassiopeia.AssistedREnemies").GetValue<bool>() && RHitCountList >= Config.Item("Cassiopeia.AssistedREnemiesCount").GetValue<Slider>().Value))
                {
                    R.Cast(RPred.CastPosition, true);
                }
            }
        }
        #endregion

        public static bool CanEscapeWithFlash(Vector2 pos)
        {
            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsEnemy && turret.Health > 0))
            {
                if (pos.Distance(turret.Position.To2D()) < (1800 + Player.BoundingRadius))
                    return true;
            }
            return false;
        }

    }


}
