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
        public static SpellSlot ignite;

        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;

        public static List<Obj_AI_Base> MinionCount;

        static int lastCastE = 0;

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


        static readonly Render.Text text = new Render.Text(0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");

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
            ignite = Player.GetSpellSlot("summonerdot");

            if (Player.ChampionName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 850f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 825f);

            Q.SetSkillshot(0.6f, 40f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.5f, 90f, 2000, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.2f, float.MaxValue);
            R.SetSkillshot(0.6f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            abilitySequence = new int[] { 1, 3, 3, 2, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };

            Config = new Menu("EloFactory_蛇女"," By LuNi", true);

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
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.UseIgnite", "智能使用点燃").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.PoisonOrder", "连招施法顺序设置").SetValue(new StringList(new[] { "先W后Q", "先Q后W" }, 1)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.DontStackQ", "禁用Q在目标离开E的范围后").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.DontStackW", "禁用W在目标离开E的范围后").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.AutoQWhenEnemyCast", "使用Q在敌人攻击时").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.AutoWWhenEnemyCast", "使用W在敌人攻击时").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.UseRComboDamage", "连招使用R + R能击杀目标").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.RCount", "使用R最小敌人数量").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.AA1", "连招走A设置").SetValue(new StringList(new[] { "禁用 走A", "智能 走A" }, 1)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.LegitE", "智能E|当目标在E范围内").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.EDelayCombo350", "智能E|连招时目标范围 < 350").SetValue(new Slider(4500, 0, 7000)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.EDelayCombo525", "智能E|连招时目标范围E延迟 < 525").SetValue(new Slider(2700, 0, 5000)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cassiopeia.EDelayComboERange", "智能E|连招目标范围 < E.范围").SetValue(new Slider(1300, 0, 3000)));

            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.UseQHarass", "使用 Q 骚扰").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.QMiniManaHarass", "骚扰最低蓝量").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.UseWHarass", "使用 W 骚扰").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.WMiniManaHarass", "MW骚扰最低蓝量").SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.UseEHarass", "使用 E 骚扰").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.EMiniManaHarass", "E骚扰最低蓝量").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.EDelayHarass", "E骚扰|延迟设置").SetValue(new Slider(0, 0, 2000)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Cassiopeia.HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("补刀", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.ToogleUseELastHit", "自动E补刀|目标中毒!").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.ToogleUseELastHitMode", "自动E补刀(模式)").SetValue(new StringList(new[] { "没有敌人", "总是" }, 1)));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.ToogleUseELastHitOption", "当连招时禁用自动E补刀").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.UseELastHit", "使用E补刀|目标中毒").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.UseELastHitNoPoisoned", "使用E补刀|目标未中毒").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Cassiopeia.EDelayLastHit", "E补刀|延迟设置").SetValue(new Slider(0, 0, 2000)));

            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseQLaneClear", "使用 Q 清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.QMiniManaLaneClear", "使用 Q 清线|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.QLaneClearCount", "使用 Q 清线|小兵数量").SetValue(new Slider(2, 1, 6)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseWLaneClear", "使用 W 清线").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.WMiniManaLaneClear", "使用 W 清线|最低蓝量").SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.WLaneClearCount", "使用 W 清线|小兵数量").SetValue(new Slider(4, 1, 6)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseELaneClear", "使用 E 清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.EMiniManaLaneClear", "使用 E 清线|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.EMiniManaLaneClearK", "使用 E 清线|连续E补刀最低蓝量").SetValue(new Slider(70, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseEOnlyLastHitLaneClear", "仅使用E补刀").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.UseELastHitLaneClearNoPoisoned", "使用E补刀|目标未中毒").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Cassiopeia.EDelayLaneClear", "E清线|延迟设置").SetValue(new Slider(0, 0, 2000)));

            Config.AddSubMenu(new Menu("清野", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.UseQJungleClear", "使用 Q 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.QMiniManaJungleClear", "使用 Q 清野|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.UseWJungleClear", "使用 W 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.WMiniManaJungleClear", "使用 W 清野|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.UseEJungleClear", "使用 E 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.EMiniManaJungleClear", "使用 E 清野|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Cassiopeia.EDelayJungleClear", "E清清野|延迟设置").SetValue(new Slider(0, 0, 2000)));

            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.InterruptSpells", "使用R 中断法术").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoWGC", "目标突进 自动W").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoRGC", "目标突进 自动R").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoPotion", "自动吃药").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoLevelSpell", "自动加点").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.StackTearInFountain", "在泉水自动使用Q堆叠女神层数").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoQTear", "当敌人进入范围自动使用Q堆叠被动层数").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Cassiopeia.AutoQTearMinMana", "Q堆叠层数|最低蓝量").SetValue(new Slider(90, 0, 100)));

            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.QRange", "Q 范围").SetValue(new Circle(true, Color.Indigo)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.WRange", "W 范围").SetValue(new Circle(true, Color.Indigo)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.ERange", "E 范围").SetValue(new Circle(true, Color.Green)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.RRange", "R 范围").SetValue(new Circle(true, Color.Gold)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.DrawOrbwalkTarget", "显示 走砍 目标").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Cassiopeia.DrawDmg", "显示 技能 伤害")).SetValue(true);

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Notifications.AddNotification("Cassiopeia By LuNi 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847 !", 10000);

        }
        //
        #region ToogleOrder Game_OnUpdate
        public static void Game_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("Cassiopeia.AutoLevelSpell").GetValue<bool>()) LevelUpSpells();

            if (Player.IsDead) return;

            if (Player.IsRecalling()) return;

            ManaManager();
            PotionManager();

            KillSteal();

            if (Config.Item("Cassiopeia.StackTearInFountain").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.InFountain() && Player.ManaPercentage() >= Config.Item("Cassiopeia.AutoQTearMinMana").GetValue<Slider>().Value &&
                (TearoftheGoddess.IsOwned(Player) || TearoftheGoddessCrystalScar.IsOwned(Player) || ArchangelsStaff.IsOwned(Player) || ArchangelsStaffCrystalScar.IsOwned(Player) || Manamune.IsOwned(Player) || ManamuneCrystalScar.IsOwned(Player)))
                Q.Cast(ObjectManager.Player, true, true);

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
            
            if (Config.Item("Cassiopeia.AutoQTear").GetValue<bool>() && Q.IsReady() && Player.ManaPercentage() >= Config.Item("Cassiopeia.AutoQTearMinMana").GetValue<Slider>().Value)
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

        }
        #endregion
        //
        #region Interrupt OnProcessSpellCast
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {

            if (Config.Item("Cassiopeia.AutoQWhenEnemyCast").GetValue<bool>() && (unit.IsValid<Obj_AI_Hero>() && !unit.IsValid<Obj_AI_Turret>()) && unit.IsEnemy && args.Target.IsMe && Q.IsReady() && Player.Distance(unit) <= Q.Range)
            {
                Q.CastIfHitchanceEquals(unit, HitChance.High, true);
            }

            if (Config.Item("Cassiopeia.AutoWWhenEnemyCast").GetValue<bool>() && (unit.IsValid<Obj_AI_Hero>() && !unit.IsValid<Obj_AI_Turret>()) && unit.IsEnemy && args.Target.IsMe && W.IsReady() && Player.Distance(unit) <= W.Range)
            {
                W.CastIfHitchanceEquals(unit, HitChance.High, true);
            }

            double InterruptOn = SpellToInterrupt(args.SData.Name);
            if (Config.Item("Cassiopeia.InterruptSpells").GetValue<bool>() && unit.Team != ObjectManager.Player.Team && InterruptOn >= 0f && unit.IsValidTarget(R.Range))
            {

                if (R.IsReady() && Player.Mana > RMANA && Player.Distance(unit) < R.Range - 50)
                {
                    R.CastIfHitchanceEquals(unit, HitChance.High, true);
                }

            }
        }
        #endregion
        //
        #region AntiGapCloser
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("Cassiopeia.AutoWGC").GetValue<bool>()) return;

            if (R.IsReady() && Player.Mana >= RMANA && Player.Distance(gapcloser.Sender) <= R.Range - 20)
            {
                R.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High, true);
            }

            if (W.IsReady() && Player.Mana >= WMANA && Player.Distance(gapcloser.Sender) <= Q.Range)
            {
                W.Cast(Player.Position, true);
            }
        }
        #endregion
        //
        #region Combo
        public static void Combo()
        {

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {

                if (Config.Item("Cassiopeia.AA1").GetValue<StringList>().SelectedIndex == 1)
                {
                    var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget() && (!E.IsReady() || !IsPoisoned(targetE) && (!Q.IsReady() && !W.IsReady() && Player.Distance(t) < (Q.Range / 4)) || (Player.Mana < QMANA)))
                        Orbwalking.Attack = true;
                    else
                        Orbwalking.Attack = false;
                }

                if (Config.Item("Cassiopeia.AA1").GetValue<StringList>().SelectedIndex != 1)
                {
                    var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget() && (ObjectManager.Player.GetAutoAttackDamage(t) > t.Health || (Player.Mana < QMANA || Player.Mana < WMANA || Player.Mana < EMANA) || (!IsPoisoned(t) && !Q.IsReady() && !W.IsReady() && Player.Distance(t) < Player.AttackRange)) && (!E.IsReady() || !IsPoisoned(targetE)))
                        Orbwalking.Attack = true;
                    else
                        Orbwalking.Attack = false;

                }

            }


            var useQ = Config.Item("Cassiopeia.UseQCombo").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWCombo").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseECombo").GetValue<bool>();
            var useR = Config.Item("Cassiopeia.UseRCombo").GetValue<bool>();
            var useIgnite = Config.Item("Cassiopeia.UseIgnite").GetValue<bool>();
            var LegitE = Config.Item("Cassiopeia.LegitE").GetValue<bool>();


            if (target.IsValidTarget())
            {


                #region Sort Ignite combo mode
                if (useIgnite)
                {
                    if (getComboDamage(target) > target.Health && target.Distance(Player.Position) < 500)
                    {
                        Player.Spellbook.CastSpell(ignite);
                    }
                }
                #endregion

                #region Sort R combo mode
                if (useR && R.IsReady() && Player.Mana >= RMANA)
                {

                    List<Obj_AI_Hero> targets;

                    targets = HeroManager.Enemies.Where(o => R.WillHit(o, target.Position) && o.Distance(Player.Position) < 500).ToList<Obj_AI_Hero>();

                    if (targets.Count >= Config.Item("Cassiopeia.RCount").GetValue<Slider>().Value)
                    {
                        R.Cast(target.Position, true);
                    }



                    if (Config.Item("Cassiopeia.UseRComboDamage").GetValue<bool>() && Player.Distance(target) < 500 && getComboDamage(target) > target.Health && getComboDamageNoUlt(target) < target.Health && target.Health > R.GetDamage(target) && Player.CountEnemiesInRange(1500) < 2 && (Player.CountAlliesInRange(1500) < 2 || (Player.HealthPercent < target.HealthPercent && Player.HealthPercent < 20)))
                    {
                        R.Cast(target.Position, true);
                    }

                }
                #endregion

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


                if (Config.Item("Cassiopeia.PoisonOrder").GetValue<StringList>().SelectedIndex != 1)
                {

                    #region Sort W combo mode
                    if (useW && W.IsReady() && Player.Mana >= WMANA)
                    {

                        if (Player.Distance(target) < W.Range && Config.Item("Cassiopeia.DontStackW").GetValue<bool>() && !IsPoisoned(target))
                        {
                            W.CastIfHitchanceEquals(target, HitChance.High, true);
                        }
                        else if (Player.Distance(target) < W.Range && !Config.Item("Cassiopeia.DontStackW").GetValue<bool>())
                        {
                            W.CastIfHitchanceEquals(target, HitChance.High, true);
                        }

                    }
                    #endregion

                    #region Sort Q combo mode
                    if (useQ && Q.IsReady() && Player.Mana >= QMANA && !W.IsReady())
                    {

                        if (target.Distance(Player) < Q.Range && Config.Item("Cassiopeia.DontStackQ").GetValue<bool>() && !IsPoisoned(target))
                        {
                            Q.CastIfHitchanceEquals(target, HitChance.High, true);
                        }
                        else if (target.Distance(Player) < Q.Range && !Config.Item("Cassiopeia.DontStackQ").GetValue<bool>())
                        {
                            Q.CastIfHitchanceEquals(target, HitChance.High, true);
                        }

                    }
                    #endregion

                }



                if (Config.Item("Cassiopeia.PoisonOrder").GetValue<StringList>().SelectedIndex == 1)
                {

                    #region Sort Q combo mode
                    if (useQ && Q.IsReady() && Player.Mana >= QMANA)
                    {

                        if (target.Distance(Player) < Q.Range && Config.Item("Cassiopeia.DontStackQ").GetValue<bool>() && !IsPoisoned(target))
                        {
                            Q.CastIfHitchanceEquals(target, HitChance.High, true);
                        }
                        else if (target.Distance(Player) < Q.Range && !Config.Item("Cassiopeia.DontStackQ").GetValue<bool>())
                        {
                            Q.CastIfHitchanceEquals(target, HitChance.High, true);
                        }

                    }
                    #endregion

                    #region Sort W combo mode
                    else if (useW && W.IsReady() && Player.Mana >= WMANA && !Q.IsReady())
                    {

                        if (Player.Distance(target) < W.Range && Config.Item("Cassiopeia.DontStackW").GetValue<bool>() && !IsPoisoned(target))
                        {
                            W.CastIfHitchanceEquals(target, HitChance.High, true);
                        }
                        else if (Player.Distance(target) < W.Range && !Config.Item("Cassiopeia.DontStackW").GetValue<bool>())
                        {
                            W.CastIfHitchanceEquals(target, HitChance.High, true);
                        }

                    }
                    #endregion

                }

            }

        }
        #endregion
        //
        #region Harass
        public static void Harass()
        {

            var useQ = Config.Item("Cassiopeia.UseQHarass").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWHarass").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseEHarass").GetValue<bool>();

            var HavemanaQ = Player.ManaPercentage() >= Config.Item("Cassiopeia.QMiniManaHarass").GetValue<Slider>().Value;
            var HavemanaW = Player.ManaPercentage() >= Config.Item("Cassiopeia.WMiniManaHarass").GetValue<Slider>().Value;
            var HavemanaE = Player.ManaPercentage() >= Config.Item("Cassiopeia.EMiniManaHarass").GetValue<Slider>().Value;


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
        //
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
                            E.Cast(minion1);
                            lastCastE = Environment.TickCount;
                        }

                    }
                }
            }

        }
        #endregion
        //
        #region LaneClear
        public static void LaneClear()
        {

            var useQ = Config.Item("Cassiopeia.UseQLaneClear").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWLaneClear").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseELaneClear").GetValue<bool>();

            var HavemanaQ = Player.ManaPercentage() >= Config.Item("Cassiopeia.QMiniManaLaneClear").GetValue<Slider>().Value;
            var HavemanaW = Player.ManaPercentage() >= Config.Item("Cassiopeia.WMiniManaLaneClear").GetValue<Slider>().Value;
            var HavemanaE = Player.ManaPercentage() >= Config.Item("Cassiopeia.EMiniManaLaneClear").GetValue<Slider>().Value;
            var HavemanaEK = Player.ManaPercentage() >= Config.Item("Cassiopeia.EMiniManaLaneClearK").GetValue<Slider>().Value;

            var CountQ = Config.Item("Cassiopeia.QLaneClearCount").GetValue<Slider>().Value;
            var CountW = Config.Item("Cassiopeia.WLaneClearCount").GetValue<Slider>().Value;

            if (Q.IsReady() && useQ && HavemanaQ)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.Position, Q.Range + 90f, MinionTypes.All, MinionTeam.Enemy);

                if (allMinionsQ.Any())
                {
                    var farmAll = Q.GetCircularFarmLocation(allMinionsQ, 90f);
                    if (farmAll.MinionsHit >= CountQ)
                    {
                        Q.Cast(farmAll.Position, true);
                        return;
                    }
                }
            }

            if (W.IsReady() && useW && HavemanaW)
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range + W.Width, MinionTypes.All).ToList();

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
                                E.Cast(minion);
                                lastCastE = Environment.TickCount;
                            }
                        }
                        if (!Config.Item("Cassiopeia.UseEOnlyLastHitLaneClear").GetValue<bool>() && HavemanaEK)
                        {

                            if (GetEDamage(minion) * 1.50 < minion.Health)
                            {
                                E.Cast(minion);
                                lastCastE = Environment.TickCount;
                            }

                        }
                    }
                }
            }

        }
        #endregion
        //
        #region JungleClear
        public static void JungleClear()
        {

            var useQ = Config.Item("Cassiopeia.UseQJungleClear").GetValue<bool>();
            var useW = Config.Item("Cassiopeia.UseWJungleClear").GetValue<bool>();
            var useE = Config.Item("Cassiopeia.UseEJungleClear").GetValue<bool>();

            var HavemanaQ = Player.ManaPercentage() >= Config.Item("Cassiopeia.QMiniManaJungleClear").GetValue<Slider>().Value;
            var HavemanaW = Player.ManaPercentage() >= Config.Item("Cassiopeia.WMiniManaJungleClear").GetValue<Slider>().Value;
            var HavemanaE = Player.ManaPercentage() >= Config.Item("Cassiopeia.EMiniManaJungleClear").GetValue<Slider>().Value;

            var MinionN = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (!MinionN.IsValidTarget() || MinionN == null)
            {
                LaneClear();
                return;
            }

            if (useQ && Q.IsReady() && HavemanaQ)
            {
                if (Player.Distance(MinionN) <= Q.Range)
                {
                    Q.CastIfHitchanceEquals(MinionN, HitChance.High);
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
        //
        #region KillSteal
        public static void KillSteal()
        {

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => !target.IsMe && target.Team != ObjectManager.Player.Team))
            {

                if (E.IsReady() && Player.Mana >= EMANA && target.Health < E.GetDamage(target) && ObjectManager.Player.Distance(target) <= E.Range && !target.IsDead && target.IsValidTarget() && IsPoisoned(target))
                {
                    E.Cast(target, true);
                    lastCastE = Environment.TickCount;
                    return;
                }

                if (Q.IsReady() && Player.Mana >= QMANA && target.Health < Q.GetDamage(target) && ObjectManager.Player.Distance(target) <= Q.Range && !target.IsDead && target.IsValidTarget())
                {
                    Q.Cast(target, true, true);
                    return;
                }

                if (W.IsReady() && Player.Mana >= WMANA && target.Health < W.GetDamage(target) && ObjectManager.Player.Distance(target) <= W.Range && !target.IsDead && target.IsValidTarget())
                {
                    W.Cast(target, true, true);
                    return;
                }

                if (Q.IsReady() && E.IsReady() && Player.Mana >= QMANA + EMANA && target.Health < Q.GetDamage(target) + E.GetDamage(target) && ObjectManager.Player.Distance(target) <= Q.Range && !target.IsDead && target.IsValidTarget())
                {
                    Q.Cast(target, true, true);
                    return;
                }

                if (W.IsReady() && E.IsReady() && Player.Mana >= WMANA + EMANA && target.Health < W.GetDamage(target) + E.GetDamage(target) && ObjectManager.Player.Distance(target) <= W.Range && !target.IsDead && target.IsValidTarget())
                {
                    W.Cast(target, true, true);
                    return;
                }

                if (E.IsReady() && Player.Mana >= EMANA && target.Health < E.GetDamage(target) && Player.Distance(target) <= E.Range && (Player.CountEnemiesInRange(E.Range) < 2 || Player.HealthPercent < 20 || Player.CountAlliesInRange(1500) > 0) && !target.IsDead && target.IsValidTarget())
                {
                    E.Cast(target, true);
                    lastCastE = Environment.TickCount;
                    return;
                }

            }
        }
        #endregion
        //
        #region PlayerDamage
        public static float getComboDamage(Obj_AI_Hero target)
        {
            float damage = 0f;
            if (Config.Item("Cassiopeia.UseQCombo").GetValue<bool>())
            {
                if (Q.IsReady())
                {
                    damage += Q.GetDamage(target) * 1.5f;
                }
            }
            if (Config.Item("Cassiopeia.UseWCombo").GetValue<bool>())
            {
                if (W.IsReady())
                {
                    damage += W.GetDamage(target);
                }
            }
            if (Config.Item("Cassiopeia.UseECombo").GetValue<bool>())
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target) * 4f;
                }
            }
            if (Config.Item("Cassiopeia.UseRCombo").GetValue<bool>())
            {
                if (R.IsReady())
                {
                    damage += R.GetDamage(target);
                }
            }
            if (Config.Item("Cassiopeia.UseIgnite").GetValue<bool>())
            {
                if (ignite.IsReady())
                {
                    damage += (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }
            }
            return damage;
        }


        public static float getComboDamageNoUlt(Obj_AI_Hero target)
        {
            float damage = 0f;
            if (Config.Item("Cassiopeia.UseQCombo").GetValue<bool>())
            {
                if (Q.IsReady())
                {
                    damage += Q.GetDamage(target) * 1.5f;
                }
            }
            if (Config.Item("Cassiopeia.UseWCombo").GetValue<bool>())
            {
                if (W.IsReady())
                {
                    damage += W.GetDamage(target);
                }
            }
            if (Config.Item("Cassiopeia.UseECombo").GetValue<bool>())
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target) * 4f;
                }
            }
            if (Config.Item("Cassiopeia.UseIgnite").GetValue<bool>())
            {
                if (ignite.IsReady())
                {
                    damage += (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }
            }
            return damage;
        }
        #endregion
        //
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
        //
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
        //
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
        //
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

            if (Config.Item("Cassiopeia.DrawDmg").GetValue<bool>())
            {
                DrawHPBarDamage();
            }


        }


        static void DrawHPBarDamage()
        {
            const int XOffset = 10;
            const int YOffset = 20;
            const int Width = 103;
            const int Height = 8;
            foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
            {
                var barPos = unit.HPBarPosition;
                float damage = getComboDamage(unit);
                float percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                float yPos = barPos.Y + YOffset;
                float xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                float xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    text.X = (int)barPos.X + XOffset;
                    text.Y = (int)barPos.Y + YOffset - 13;
                    text.text = ((int)(unit.Health - damage)).ToString();
                    text.OnEndScene();
                }
                Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 2, System.Drawing.Color.Yellow);
            }
        }
        #endregion
        //
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
        //
        #region CheckPoison
        private static bool IsPoisoned(Obj_AI_Base unit)
        {
            return
                unit.Buffs.Where(buff => buff.IsActive && buff.Type == BuffType.Poison)
                    .Any(buff => buff.EndTime >= (Game.Time + 0.35 + 700 / 1900));
        }
        #endregion
        //
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
        //
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
    }
        #endregion

}
