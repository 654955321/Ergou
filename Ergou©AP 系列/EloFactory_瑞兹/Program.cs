#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace EloFactory_Ryze
{
    internal class Program
    {
        public const string ChampionName = "Ryze";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;


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
        public static int Muramana = 3042;

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

            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 580f);
            E = new Spell(SpellSlot.E, 580f);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 50f, 1800f, true, SkillshotType.SkillshotLine);
            E.SetTargetted(0.20f, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            abilitySequence = new int[] { 1, 2, 1, 3, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };

            Config = new Menu("EloFactory_瑞兹（new）"," By LuNi", true);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));

            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.UseQCombo", "连招 使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.UseWCombo", "连招 使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.UseECombo", "连招 使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.UseRCombo", "连招 使用 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.AutoMuramana", "自动使用魔切").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.AutoMuramanaMiniMana", "自动使用魔切|最小法力值").SetValue(new Slider(10, 0, 100)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Ryze.AA", "连招中走A设置").SetValue(new StringList(new[] { "禁用 走A", "智能 走A" }, 1)));

            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.UseQHarass", "使用 Q 骚扰").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.QMiniManaHarass", "Q骚扰最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.UseWHarass", "使用 W 骚扰").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.WMiniManaHarass", "W骚扰最低蓝量").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.UseEHarass", "使用 E 骚扰").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.EMiniManaHarass", "E骚扰最低蓝量").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Harass").AddItem(new MenuItem("Ryze.HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("补刀", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.UseQLastHit", "使用 Q 补刀").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.QMiniManaLastHit", "使用 Q 补刀|最低蓝量").SetValue(new Slider(35, 0, 100)));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.UseWLastHit", "使用 W 补刀").SetValue(false));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.WMiniManaLastHit", "使用 W 补刀|最低蓝量").SetValue(new Slider(65, 0, 100)));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.SafeWLastHit", "当敌人接近时禁用W补刀").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.UseELastHit", "使用 E 补刀").SetValue(false));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.EMiniManaLastHit", "使用 E 补刀|最低蓝量").SetValue(new Slider(35, 0, 100)));
            Config.SubMenu("LastHit").AddItem(new MenuItem("Ryze.NoPassiveProcLastHit", "当被动已经被激活禁用技能补刀").SetValue(true));
            
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.UseQLaneClear", "使用 Q 清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.QMiniManaLaneClear", "使用 Q 清线|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.UseWLaneClear", "使用 W 清线").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.WMiniManaLaneClear", "使用 W 清线|最低蓝量").SetValue(new Slider(65, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.SafeWLaneClear", "当敌人接近时禁用W清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.UseELaneClear", "使用 E 清线").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.EMiniManaLaneClear", "使用 E 清线|最低蓝量").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("Ryze.NoPassiveProcLaneClear", "当被动已经被激活禁用技能清线").SetValue(false));
            
            Config.AddSubMenu(new Menu("清野", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Ryze.UseQJungleClear", "使用 Q 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Ryze.UseWJungleClear", "使用 W 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Ryze.UseEJungleClear", "使用 E 清野").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("Ryze.SafeJungleClear", "当敌人接近时禁用技能清野").SetValue(true));

            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ryze.AutoQEGC", "对突进者使用Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ryze.AutoWEGC", "对突进者使用W").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ryze.AutoPotion", "使用自动吃药").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ryze.AutoLevelSpell", "使用自动加点").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ryze.StackTearInFountain", "在泉水时自动使用Q堆叠被动").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ryze.AutoQTearMinMana", "技能堆叠被动|最低蓝量").SetValue(new Slider(90, 0, 100)));

            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.Indigo)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(true, Color.Green)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "E 范围").SetValue(new Circle(true, Color.Green)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawOrbwalkTarget", "显示 走砍 目标").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Notifications.AddNotification("Ryze By LuNi 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847 !!", 10000);

        }

        #region ToogleOrder Game_OnUpdate
        public static void Game_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("Ryze.AutoLevelSpell").GetValue<bool>()) LevelUpSpells();

            if (Player.IsDead) return;

            if (Player.IsRecalling()) return;

            ManaManager();
            PotionManager();

            KillSteal();

            if (Config.Item("Ryze.StackTearInFountain").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.InFountain() && Player.ManaPercentage() >= Config.Item("Ryze.AutoQTearMinMana").GetValue<Slider>().Value &&
                (TearoftheGoddess.IsOwned(Player) || TearoftheGoddessCrystalScar.IsOwned(Player) || ArchangelsStaff.IsOwned(Player) || ArchangelsStaffCrystalScar.IsOwned(Player) || Manamune.IsOwned(Player) || ManamuneCrystalScar.IsOwned(Player)))
                Q.Cast(ObjectManager.Player, true, true);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Orbwalking.Attack = true;
                JungleClear();
                LaneClear();

            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                Orbwalking.Attack = true;
                LastHit();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Orbwalking.Attack = true;
                LastHit();
            }

            if (Config.Item("Ryze.HarassActive").GetValue<KeyBind>().Active || Config.Item("Ryze.HarassActiveT").GetValue<KeyBind>().Active)
            {
                Orbwalking.Attack = true;
                Harass();
            }

        }
        #endregion

        #region AntiGapCloser
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

            if (Config.Item("Ryze.AutoWEGC").GetValue<bool>() && W.IsReady() && Player.Mana >= WMANA && Player.Distance(gapcloser.Sender) <= W.Range)
            {
                W.Cast(gapcloser.Sender, true);
            }

            if (Config.Item("Ryze.AutoQEGC").GetValue<bool>() && Q.IsReady() && Player.Mana >= QMANA + WMANA && Player.Distance(gapcloser.Sender) < Q.Range)
            {
                Q.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High, true);
            }

        }
        #endregion

        #region Combo
        public static void Combo()
        {



            var useQ = Program.Config.Item("Ryze.UseQCombo").GetValue<bool>();
            var useW = Program.Config.Item("Ryze.UseWCombo").GetValue<bool>();
            var useE = Program.Config.Item("Ryze.UseECombo").GetValue<bool>();
            var useR = Program.Config.Item("Ryze.UseRCombo").GetValue<bool>();

            

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {

                if (Config.Item("Ryze.AA").GetValue<StringList>().SelectedIndex == 1)
                {
                    var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget() && ((ObjectManager.Player.GetAutoAttackDamage(t) > t.Health) || (Player.Distance(t) <= W.Range - 150 && Player.MoveSpeed <= t.MoveSpeed && !W.IsReady() && !E.IsReady()) || (Player.Distance(t) <= W.Range - 75 && Player.MoveSpeed > t.MoveSpeed && !W.IsReady() && !E.IsReady())))
                        Orbwalking.Attack = true;
                    else
                        Orbwalking.Attack = false;
                }

                if (Config.Item("Ryze.AA").GetValue<StringList>().SelectedIndex != 1)
                {
                    var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget() && ((Player.GetAutoAttackDamage(t) > t.Health) || (!Q.IsReady() && !W.IsReady() && !E.IsReady() && Player.Distance(t) < W.Range * 0.65) || (Player.Mana < QMANA || Player.Mana < WMANA || Player.Mana < EMANA)))
                        Orbwalking.Attack = true;
                    else
                        Orbwalking.Attack = false;

                }

            }

            foreach (var target1 in ObjectManager.Get<Obj_AI_Hero>().Where(target1 => !target1.IsMe && target1.Team != ObjectManager.Player.Team))
            {
                if (useQ && Q.IsReady() && (!W.IsReady() || !useW) && (!E.IsReady() || !useE) && Player.Mana >= QMANA && !target1.IsDead && target1.IsValidTarget())
                {
                    if (Player.Distance(target1) < Q.Range)
                    {
                        Q.CastIfHitchanceEquals(target1, HitChance.High, true);
                    }
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {

                #region Sort R combo mode
                if (useR && R.IsReady())
                {
                    RUsage();
                }
                #endregion

                #region Sort E combo mode
                if (useE && E.IsReady() && Player.Mana > EMANA)
                {

                    if(Player.Distance(target) <= E.Range)
                    {
                        E.Cast(target, true);
                    }

                }
                #endregion

                #region Sort W combo mode
                if (useW && W.IsReady() && Player.Mana >= WMANA)
                {

                    if (Player.Distance(target) <= W.Range)
                    {
                        W.Cast(target, true);
                    }          

                }
                #endregion

                #region Sort Q combo mode
                if (useQ && Q.IsReady() && Player.Mana >= QMANA)
                {
                    if (Player.Distance(target) < Q.Range)
                    {
                        Q.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                }
                #endregion
            }

        }
        #endregion

        #region Harass
        public static void Harass()
        {

            var useQ = Program.Config.Item("Ryze.UseQHarass").GetValue<bool>();
            var useW = Program.Config.Item("Ryze.UseWHarass").GetValue<bool>();
            var useE = Program.Config.Item("Ryze.UseEHarass").GetValue<bool>();

            var MinManaQ = Config.Item("Ryze.QMiniManaHarass").GetValue<Slider>().Value;
            var MinManaW = Config.Item("Ryze.WMiniManaHarass").GetValue<Slider>().Value;
            var MinManaE = Config.Item("Ryze.EMiniManaHarass").GetValue<Slider>().Value;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);


            #region Sort E Harass mode
            if (useE && E.IsReady() && Player.Mana > EMANA && Player.ManaPercentage() > MinManaE)
            {

                if (Player.Distance(target) <= E.Range)
                {
                    E.Cast(target, true);
                }

            }
            #endregion

            #region Sort W Harass mode
            if (useW && W.IsReady() && Player.Mana >= WMANA && Player.ManaPercentage() > MinManaW)
            {

                if (Player.Distance(target) <= W.Range)
                {
                    W.Cast(target, true);
                }

            }
            #endregion

            #region Sort Q Harass mode
            if (useQ && Q.IsReady() && Player.Mana >= QMANA && Player.ManaPercentage() > MinManaQ)
            {
                if (Player.Distance(target) < Q.Range)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                }
            }
            #endregion



        }
        #endregion

        #region LastHit
        public static void LastHit()
        {

            if (GetPassiveBuff == 4 && Config.Item("Ryze.NoPassiveProcLastHit").GetValue<bool>()) return;


            var useQ = Program.Config.Item("Ryze.UseQLastHit").GetValue<bool>();
            var useW = Program.Config.Item("Ryze.UseWLastHit").GetValue<bool>();
            var useE = Program.Config.Item("Ryze.UseELastHit").GetValue<bool>();

            var MinManaQ = Config.Item("Ryze.QMiniManaLastHit").GetValue<Slider>().Value;
            var MinManaW = Config.Item("Ryze.WMiniManaLastHit").GetValue<Slider>().Value;
            var MinManaE = Config.Item("Ryze.EMiniManaLastHit").GetValue<Slider>().Value;

            var allMinionsQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            var MinionQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (useW && W.IsReady())
            {

                if (Player.GetAutoAttackDamage(MinionQ) < MinionQ.Health && Player.ManaPercentage() > MinManaW && Player.Distance(MinionQ) <= W.Range && MinionQ.Health < W.GetDamage(MinionQ) && MinionQ.Health < W.GetDamage(MinionQ) * 0.8)
                {
                    if (Config.Item("Ryze.SafeWLastHit").GetValue<bool>() && Player.CountEnemiesInRange(1500) > 0)
                    {
                        return;
                    }
                    else
                    W.Cast(MinionQ, true);
                }

            }

            if (useE && E.IsReady())
            {

                if (Player.GetAutoAttackDamage(MinionQ) < MinionQ.Health && Player.ManaPercentage() > MinManaE && Player.Distance(MinionQ) <= E.Range && MinionQ.Health < E.GetDamage(MinionQ) * 0.8)
                {
                    E.Cast(MinionQ, true);
                }

            }

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinionsQ)
                {
                    if (Player.GetAutoAttackDamage(MinionQ) < MinionQ.Health && Player.ManaPercentage() > MinManaQ && minion.Health < Q.GetDamage(minion) * 0.8)
                    {
                        Q.CastIfHitchanceEquals(minion, HitChance.High, true);
                    }
                }
            }

        }
        #endregion

        #region LaneClear
        public static void LaneClear()
        {

            if (GetPassiveBuff == 4 && Config.Item("Ryze.NoPassiveProcLaneClear").GetValue<bool>()) return;

            var useQ = Program.Config.Item("Ryze.UseQLaneClear").GetValue<bool>();
            var useW = Program.Config.Item("Ryze.UseWLaneClear").GetValue<bool>();
            var useE = Program.Config.Item("Ryze.UseELaneClear").GetValue<bool>();

            var MinManaQ = Config.Item("Ryze.QMiniManaLaneClear").GetValue<Slider>().Value;
            var MinManaW = Config.Item("Ryze.WMiniManaLaneClear").GetValue<Slider>().Value;
            var MinManaE = Config.Item("Ryze.EMiniManaLaneClear").GetValue<Slider>().Value;

            var allMinionsQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            var MinionQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (useW && W.IsReady())
            {

                if (Player.GetAutoAttackDamage(MinionQ) < MinionQ.Health && Player.ManaPercentage() > MinManaW && Player.Distance(MinionQ) <= W.Range && MinionQ.Health < W.GetDamage(MinionQ) * 0.8)
                {
                    if (Config.Item("Ryze.SafeWLaneClear").GetValue<bool>() && Player.CountEnemiesInRange(1500) > 0)
                    {
                        return;
                    }
                    else
                    W.Cast(MinionQ, true);
                }

            }

            if (useE && E.IsReady())
            {

                if (Player.GetAutoAttackDamage(MinionQ) < MinionQ.Health && Player.ManaPercentage() > MinManaE && Player.Distance(MinionQ) <= E.Range)
                {
                    E.Cast(MinionQ, true);
                }

            }

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinionsQ)
                {
                    if (Player.GetAutoAttackDamage(MinionQ) < MinionQ.Health && Player.ManaPercentage() > MinManaQ && minion.Health < Q.GetDamage(minion) * 0.8)
                    {
                        Q.CastIfHitchanceEquals(minion, HitChance.High, true);
                    }
                }
            }





        }
        #endregion

        #region JungleClear
        public static void JungleClear()
        {

            if (Config.Item("Ryze.SafeJungleClear").GetValue<bool>() && Player.CountEnemiesInRange(1500) > 0) return;

            var useQ = Program.Config.Item("Ryze.UseQJungleClear").GetValue<bool>();
            var useW = Program.Config.Item("Ryze.UseWJungleClear").GetValue<bool>();
            var useE = Program.Config.Item("Ryze.UseEJungleClear").GetValue<bool>();

            var MinionN = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (!MinionN.IsValidTarget() || MinionN == null)
            {
                LaneClear();
                return;
            }

            if (useE && E.IsReady() && Player.Distance(MinionN) <= E.Range && Player.Mana > EMANA)
            {
                E.Cast(MinionN, true);
            }

            if (useW && W.IsReady() && Player.Distance(MinionN) <= W.Range && Player.Mana > WMANA)
            {
                W.Cast(MinionN, true);
            }

            if (useQ && Q.IsReady() && Player.Distance(MinionN) < Q.Range && Player.Mana > QMANA)
            {
                Q.CastIfHitchanceEquals(MinionN, HitChance.High, true);
            }

        }
        #endregion

        #region KillSteal
        public static void KillSteal()
        {

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => !target.IsMe && target.Team != ObjectManager.Player.Team))
            {

                if (Q.IsReady() && Player.Mana >= QMANA && target.Health < Q.GetDamage(target) && Player.Distance(target) < Q.Range && !target.IsDead && target.IsValidTarget())
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High, true);
                    return;
                }

                if (E.IsReady() && Player.Mana >= EMANA && target.Health < E.GetDamage(target) && Player.Distance(target) <= E.Range && !target.IsDead && target.IsValidTarget())
                {
                    E.Cast(target, true);
                    return;
                }

                if (W.IsReady() && Player.Mana >= WMANA && target.Health < W.GetDamage(target) && Player.Distance(target) <= W.Range && !target.IsDead && target.IsValidTarget())
                {
                    W.Cast(target, true);
                    return;
                }

                if (E.IsReady() && W.IsReady() && Player.Mana >= EMANA + WMANA && target.Health <= E.GetDamage(target) + W.GetDamage(target) && Player.Distance(target) <= E.Range && !target.IsDead && target.IsValidTarget())
                {
                    E.Cast(target, true);
                    return;
                }


            }

        }
        #endregion

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

            if (Config.Item("Ryze.AutoPotion").GetValue<bool>() && !Player.InFountain() && !Player.IsRecalling() && !Player.IsDead)
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
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && (spell.Slot != SpellSlot.R || R.Level > 0))
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            if (Config.Item("DrawOrbwalkTarget").GetValue<bool>())
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

        #region PassiveBuff
        private static int GetPassiveBuff
        {
            get
            {
                var data = ObjectManager.Player.Buffs.FirstOrDefault(b => b.DisplayName == "RyzePassiveStack");
                return data != null ? data.Count : 0;
            }
        }
        #endregion

        #region R Usage

        public static void RUsage()
        {

            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);


            if (Player.CountEnemiesInRange(1200) > 1)
            {

                if (Player.CountAlliesInRange(1200) > 1)
                {

                    if (Player.HealthPercent <= target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }
                        return;
                    }

                    if (Player.HealthPercent > target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }
                        return;
                    }
                    return;
                }

                if (Player.CountAlliesInRange(1200) < 2)
                {

                    if (Player.HealthPercent <= target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }
                        return;
                    }

                    if (Player.HealthPercent > target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }







            if (Player.CountEnemiesInRange(1200) < 2)
            {

                if (Player.CountAlliesInRange(1200) > 1)
                {

                    if (Player.HealthPercent <= target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }
                        return;
                    }

                    if (Player.HealthPercent > target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            return;
                        }
                        return;
                    }
                    return;
                }

                if (Player.CountAlliesInRange(1200) < 2)
                {

                    if (Player.HealthPercent <= target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            R.Cast(Player, true);
                            return;
                        }
                        return;
                    }

                    if (Player.HealthPercent > target.HealthPercent)
                    {

                        if (GetPassiveBuff == 4 || Player.HasBuff("RyzePassiveCharged"))
                        {
                            return;
                        }

                        if (GetPassiveBuff < 4 || !Player.HasBuff("RyzePassiveCharged"))
                        {
                            return;
                        }
                        return;
                    }
                    return;
                }
                return;
            }




        }


        #endregion

        #region BeforeAA
        static void Orbwalking_BeforeAttack(LeagueSharp.Common.Orbwalking.BeforeAttackEventArgs args)
        {
            
            if (Config.Item("Ryze.AutoMuramana").GetValue<bool>())
            {
                int Muramanaitem = Items.HasItem(Muramana) ? 3042 : 3043;
                if (args.Target.IsValid<Obj_AI_Hero>() && args.Target.IsEnemy && Items.HasItem(Muramanaitem) && Items.CanUseItem(Muramanaitem) && ObjectManager.Player.ManaPercentage() > Config.Item("Ryze.AutoMuramanaMiniMana").GetValue<Slider>().Value)
                {
                    if (!ObjectManager.Player.HasBuff("Muramana"))
                        Items.UseItem(Muramanaitem);
                }
                else if (ObjectManager.Player.HasBuff("Muramana") && Items.HasItem(Muramanaitem) && Items.CanUseItem(Muramanaitem))
                    Items.UseItem(Muramanaitem);
            }



        }
        #endregion
    }
}
