using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color=System.Drawing.Color;

namespace Princess_LeBlanc
{
    class Program
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Menu LeBlancConfig;
        public static Menu TargetSelectorMenu;
        public static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;
        private static bool PacketCast;

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");
        private static readonly Items.Item Dfg = new Items.Item(ItemData.Deathfire_Grasp.Id, ItemData.Deathfire_Grasp.Range);
        private static readonly Items.Item Zho = new Items.Item(ItemData.Zhonyas_Hourglass.Id);
        private static readonly Items.Item Hp = new Items.Item(ItemData.Health_Potion.Id);
        private static readonly Items.Item Mp = new Items.Item(ItemData.Mana_Potion.Id);
        private static readonly Items.Item Flask = new Items.Item(ItemData.Crystalline_Flask.Id);

        private static class SecondW
        {
            public static GameObject Object { get; set; }
            public static Vector3 Pos { get; set; }
            public static double ExpireTime { get; set; }
        }
        private static class Clone
        {
            public static GameObject Object { get; set; }
            public static Vector3 Pos { get; set; }
            public static double ExpireTime { get; set; }
        }

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Leblanc")
            {
                return;
            }

            Spells();
            Menu();
            Drawing.OnDraw += OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            Game.OnGameUpdate += OnGameUpdate;

            Game.PrintChat("<b><font color =\"#FFFFFF\">Princess LeBlanc</font></b><font color =\"#FFFFFF\"> by </font><b><font color=\"#FF66FF\">Leia</font></b><font color =\"#FFFFFF\"> loaded!</font>");
        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.HasBuff("Recall"))
                return;

            PacketCast = LeBlancConfig.Item("UsePacket").GetValue<bool>();

            WLogic();
            switch (Orbwalker.ActiveMode)
            {
                    case Orbwalking.OrbwalkingMode.Combo:
                {
                    var t = GetEnemy(1200);
                    GapClose(t);
                    break;
                }
                    case Orbwalking.OrbwalkingMode.LaneClear:
                {
                    LaneClear();
                    break;
                }
                    case Orbwalking.OrbwalkingMode.Mixed:
                {
                    Harass();
                    break;
                }
            }
            Flee();
            UseItems();
            Clones();
            if (LeBlancConfig.Item("haraKey").GetValue<KeyBind>().Active)
                Harass();
        }

        private static void Menu()
        {
            LeBlancConfig = new Menu("Princess 汉化者: Feeeez" + ObjectManager.Player.ChampionName, "Princess" + ObjectManager.Player.ChampionName, true);

            LeBlancConfig.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(LeBlancConfig.SubMenu("Orbwalking"));

            TargetSelectorMenu = new Menu("目标选择", "Common_TargetSelector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            LeBlancConfig.AddSubMenu(TargetSelectorMenu);

            new AssassinManager();

            LeBlancConfig.AddSubMenu(new Menu("连招", "Combo"));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(LeBlancConfig.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useR", "使用 R").SetValue(true));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("preE", "最低命中率 E").SetValue(new StringList((new[] { "低", "正常", "高", "非常高" }), 2)));
            LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("preR", "Minimum HitChance R(E)").SetValue(new StringList((new[] { "低", "正常", "高", "非常高" }), 2)));

            LeBlancConfig.AddSubMenu(new Menu("清线", "laneclear"));
            LeBlancConfig.SubMenu("laneclear").AddItem(new MenuItem("laneClearActive", "清线").SetValue(new KeyBind(LeBlancConfig.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));
            LeBlancConfig.SubMenu("laneclear").AddItem(new MenuItem("LaneClearQ", "使用 Q").SetValue(true));
            LeBlancConfig.SubMenu("laneclear").AddItem(new MenuItem("LaneClearW", "使用 W").SetValue(true));
            LeBlancConfig.SubMenu("laneclear").AddItem(new MenuItem("LaneClear2W", "使用第二段 W").SetValue(true));
            LeBlancConfig.SubMenu("laneclear").AddItem(new MenuItem("LaneClearWHit", "小兵数量 W").SetValue(new Slider(2, 0, 5)));
            LeBlancConfig.SubMenu("laneclear").AddItem(new MenuItem("LaneClearManaPercent", "最低蓝量百分比").SetValue(new Slider(30)));

            LeBlancConfig.AddSubMenu(new Menu("骚扰", "harass"));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind(LeBlancConfig.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("use2W", "使用第二段 W").SetValue(true));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("HarassManaPercent", "最低蓝量百分比").SetValue(new Slider(30)));
            LeBlancConfig.SubMenu("harass").AddItem(new MenuItem("haraKey", "骚扰切换").SetValue(new KeyBind('Y', KeyBindType.Toggle)));

            LeBlancConfig.AddSubMenu(new Menu("物品", "items"));
            LeBlancConfig.SubMenu("items").AddItem(new MenuItem("useIgnite", "使用 点燃").SetValue(true));
            LeBlancConfig.SubMenu("items").AddItem(new MenuItem("useDfg", "使用 冥火").SetValue(true));
            LeBlancConfig.SubMenu("items").AddItem(new MenuItem("useZho", "使用 中亚").SetValue(true));
            LeBlancConfig.SubMenu("items").AddItem(new MenuItem("minZho", "自动中亚最低血量%").SetValue(new Slider(5)));
            LeBlancConfig.SubMenu("items").AddSubMenu(new Menu("红药", "hp"));
            LeBlancConfig.SubMenu("items").SubMenu("hp").AddItem(new MenuItem("useHp", "使用红药").SetValue(true));
            LeBlancConfig.SubMenu("items").SubMenu("hp").AddItem(new MenuItem("useFlask", "使用 水晶瓶").SetValue(true));
            LeBlancConfig.SubMenu("items").SubMenu("hp").AddItem(new MenuItem("minHp", "最低血量使用%").SetValue(new Slider(30)));
            LeBlancConfig.SubMenu("items").AddSubMenu(new Menu("蓝药", "mp"));
            LeBlancConfig.SubMenu("items").SubMenu("mp").AddItem(new MenuItem("useMp", "使用蓝药").SetValue(true));
            LeBlancConfig.SubMenu("items").SubMenu("mp").AddItem(new MenuItem("useFlask", "使用 水晶瓶").SetValue(true));
            LeBlancConfig.SubMenu("items").SubMenu("mp").AddItem(new MenuItem("minMp", "最低蓝量使用%").SetValue(new Slider(30)));

            LeBlancConfig.AddSubMenu(new Menu("逃跑", "Flee"));
            LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeK", "键位").SetValue(new KeyBind('A', KeyBindType.Press)));
            LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeW", "使用 W").SetValue(true));
            LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeE", "使用 E").SetValue(true));
            LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeR", "使用 R").SetValue(true));
            LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeSecondW", "如果跟随鼠标使用第二段W").SetValue(true));

            LeBlancConfig.AddSubMenu(new Menu("显示范围", "Draw"));
            LeBlancConfig.SubMenu("Draw").AddItem(new MenuItem("drawQ", "显示 Q").SetValue(new Circle(true, Color.AntiqueWhite)));
            LeBlancConfig.SubMenu("Draw").AddItem(new MenuItem("drawW", "显示 W").SetValue(new Circle(true, Color.AntiqueWhite)));
            LeBlancConfig.SubMenu("Draw").AddItem(new MenuItem("drawE", "显示 E").SetValue(new Circle(true, Color.AntiqueWhite)));

            MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示连招伤害", true).SetValue(true);
            MenuItem drawFill = new MenuItem("Draw_Fill", "显示连招伤害填补", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
            LeBlancConfig.SubMenu("Draw").AddItem(drawComboDamageMenu);
            LeBlancConfig.SubMenu("Draw").AddItem(drawFill);
            DamageIndicator.DamageToUnit = ComboDamage;
            DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
            drawComboDamageMenu.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            drawFill.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                    DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                };

            LeBlancConfig.AddSubMenu(new Menu("额外选项", "Misc"));
            LeBlancConfig.SubMenu("Misc").AddSubMenu(new Menu("第二段 W 逻辑", "backW"));
            LeBlancConfig.SubMenu("Misc").SubMenu("backW").AddItem(new MenuItem("SWcountEnemy", "第二段W落点敌人数").SetValue(new Slider(0, 0, 5)));
            LeBlancConfig.SubMenu("Misc").SubMenu("backW").AddItem(new MenuItem("SWpos", "第二段W如果悬停鼠标位置").SetValue(true));
            LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "打断 E").SetValue(true));
            LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("Gapclose", "反突进 E").SetValue(true));
            LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("UsePacket", "使用封包").SetValue(false));
            LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("Clone", "克隆逻辑").SetValue(true));

            LeBlancConfig.AddToMainMenu();
        }

        #region Combo

        private static void GapClose(Obj_AI_Base t)
        {
            var dmg = ComboDamage(t) - Player.GetSpellDamage(t, SpellSlot.W) > t.Health;
            var useW = LeBlancConfig.SubMenu("Combo").Item("useW").GetValue<bool>();

            if (!dmg || Q.IsInRange(t))
            {
                Combo(t);
                return;
            }

            W.Cast(t.ServerPosition, PacketCast);
            Combo(t);
        }
        private static void Combo(Obj_AI_Base t)
        {
            var useQ = LeBlancConfig.SubMenu("Combo").Item("useQ").GetValue<bool>();
            var useW = LeBlancConfig.SubMenu("Combo").Item("useW").GetValue<bool>();
            var useE = LeBlancConfig.SubMenu("Combo").Item("useE").GetValue<bool>();
            var useR = LeBlancConfig.SubMenu("Combo").Item("useR").GetValue<bool>();
            var useDfg = LeBlancConfig.SubMenu("items").Item("useDfg").GetValue<bool>();
            var useIgnite = LeBlancConfig.SubMenu("items").Item("useIgnite").GetValue<bool>();
            var hitE = (HitChance)(LeBlancConfig.SubMenu("Combo").Item("preE").GetValue<StringList>().SelectedIndex + 3);
            var hitR = (HitChance)(LeBlancConfig.SubMenu("Combo").Item("preE").GetValue<StringList>().SelectedIndex + 3);
            if (t.Health < ComboDamage(t))
            {
                CastDfg(t, useDfg);
                CastIgnite(t, useIgnite);
            }
            CastE(t, hitE, useE);
            CastQ(t, useQ);
            CastW(t, useW);

            switch (Player.Spellbook.GetSpell(SpellSlot.R).Name)
            {
                case "LeblancChaosOrbM":
                {
                    if (W.Level <= Q.Level || (!Q.IsReady() && !W.IsReady()))
                    {
                        CastRq(t, useR);
                    }
                    break;
                }

                case "LeblancSlideM":
                {
                    if (W.Level > Q.Level || (!Q.IsReady() && !W.IsReady()))
                    {
                        CastRw(t, useR);
                    }
                    break;
                }

                case "LeblancSoulShackleM":
                {
                    if (Player.MoveSpeed - 10 < t.MoveSpeed || (!Q.IsReady() && !W.IsReady() && !E.IsReady()))
                    {
                        CastRe(t, hitR, useR);
                    }
                    break;
                }
            }
        }
        #endregion Combo
        #region LaneClear

        private static void LaneClear()
        {
            var useQ = LeBlancConfig.Item("LaneClearQ").GetValue<bool>();
            var useW = LeBlancConfig.Item("LaneClearW").GetValue<bool>();
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var minionsJung = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
            var farmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(W.Range).Select(m => m.ServerPosition.To2D()).ToList(), W.Width, W.Range);
            var mana = ObjectManager.Player.ManaPercentage() > LeBlancConfig.Item("LaneClearManaPercent").GetValue<Slider>().Value;
            var minionHit = farmLocation.MinionsHit >= LeBlancConfig.Item("LaneClearWHit").GetValue<Slider>().Value;
            if (!mana)
            {
                return;
            }
            foreach (var minion in minions)
            {
                if (minion != null && minion.IsValidTarget() && minion.Health <= Q.GetDamage(minion))
                {
                    CastQ(minion, useQ);
                }
            }
            foreach (var minion in minionsJung)
            {
                if (minion != null && minion.IsValidTarget())
                {
                    CastQ(minion, useQ);
                }
            }

            if (minionHit && useW && W.IsReady() && W.Instance.Name == "LeblancSlide")
            {
                W.Cast(farmLocation.Position, PacketCast);
            }
            if (farmLocation.MinionsHit > 0 && useW && W.IsReady() && W.Instance.Name == "LeblancSlide")
            {
                W.Cast(farmLocation.Position, PacketCast);
            }

            CastSecondW(LeBlancConfig.Item("LaneClear2W").GetValue<bool>());
        }
        #endregion LaneClear
        #region Harass
        private static void Harass()
        {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                var mana = ObjectManager.Player.ManaPercentage() > LeBlancConfig.Item("HarassManaPercent").GetValue<Slider>().Value;
                var useQ = LeBlancConfig.Item("useQ").GetValue<bool>();
                var useW = LeBlancConfig.Item("useW").GetValue<bool>();
                var useE = LeBlancConfig.Item("useE").GetValue<bool>();

                if (!mana) { return; }

                CastE(target, HitChance.High, useE);
                CastQ(target, useQ);
                CastW(target, useW);
                CastSecondW(LeBlancConfig.Item("use2W").GetValue<bool>());
        }
        #endregion Harass
        #region Flee
        private static void Flee()
        {
            if (!LeBlancConfig.Item("FleeK").GetValue<KeyBind>().Active) { return; }

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var useW = LeBlancConfig.Item("FleeW").GetValue<bool>();
            var useE = LeBlancConfig.Item("FleeE").GetValue<bool>();
            var useR = LeBlancConfig.Item("FleeR").GetValue<bool>();
            var wposex = SecondW.Pos.Extend(Game.CursorPos, 100);
            var fleepos = wposex.Distance(SecondW.Pos) > Game.CursorPos.Distance(SecondW.Pos);
            var useSecondW = LeBlancConfig.Item("FleeSecondW").GetValue<bool>();

            if (W.IsReady() && useW && W.Instance.Name == "LeblancSlide")
            {
                W.Cast(Game.CursorPos, PacketCast);
            }
            if (R.IsReady() && useR && R.Instance.Name == "LeblancSlideM")
            {
                R.Cast(Game.CursorPos, PacketCast);
            }
            CastE(target, HitChance.Medium, useE);

            if (fleepos)
            {
                CastSecondW(useSecondW);
            }

        }

        #endregion Flee
        #region Items
        private static void CastIgnite(Obj_AI_Base t, bool useIgnite)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(600) || !IgniteSlot.IsReady() || !useIgnite || IgniteSlot == SpellSlot.Unknown)
                return;

            Player.Spellbook.CastSpell(IgniteSlot, t);
        }
        private static void CastDfg(Obj_AI_Base t, bool useDfg)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(Dfg.Range) || !Dfg.IsReady() || !useDfg)
                return;

            Dfg.Cast(t);
        }
        private static void CastZho(bool useZho)
        {
            if (!Zho.IsReady() || !useZho)
                return;

            Zho.Cast();
        }
        private static void CastHp(bool useHp)
        {
            if (!Hp.IsReady() || !useHp)
                return;

            Hp.Cast();
        }
        private static void CastMp(bool useMp)
        {
            if (!Mp.IsReady() || !useMp)
                return;

            Mp.Cast();
        }
        private static void CastFlask(bool useFlask)
        {
            if (!Flask.IsReady() || !useFlask)
                return;

            Flask.Cast();
        }
        private static void UseItems()
        {
            var useHp = LeBlancConfig.SubMenu("items").SubMenu("hp").Item("useHp").GetValue<bool>();
            var useHFlask = LeBlancConfig.SubMenu("items").SubMenu("hp").Item("useFlask").GetValue<bool>();
            var minHp = ObjectManager.Player.HealthPercentage() < LeBlancConfig.SubMenu("items").SubMenu("hp").Item("minHp").GetValue<Slider>().Value;
            var useMp = LeBlancConfig.SubMenu("items").SubMenu("mp").Item("useMp").GetValue<bool>();
            var useMFlask = LeBlancConfig.SubMenu("items").SubMenu("mp").Item("useFlask").GetValue<bool>();
            var minMp = ObjectManager.Player.ManaPercentage() < LeBlancConfig.SubMenu("items").SubMenu("mp").Item("minmp").GetValue<Slider>().Value;
            var useFlask = useHFlask || useMFlask;
            var useZho = LeBlancConfig.SubMenu("items").Item("useZho").GetValue<bool>();
            var minZho = LeBlancConfig.SubMenu("items").Item("minZho").GetValue<Slider>().Value > ObjectManager.Player.HealthPercentage();

            if (ObjectManager.Player.HasBuff("Recall"))
                return;

            if (minHp)
            {
                CastHp(useHp);
                CastFlask(useFlask);
            }
            if (minMp)
            {
                CastMp(useMp);
                CastFlask(useFlask);
            }
            if (minZho)
            {
                CastZho(useZho);
            }
        }
        #endregion Items
        #region Misc
        private static void WLogic()
        {
            var countW = Utility.CountEnemiesInRange(SecondW.Pos, 200) > LeBlancConfig.SubMenu("Misc").SubMenu("backW").Item("SWcountEnemy").GetValue<Slider>().Value;
            var wposex = SecondW.Pos.Extend(Game.CursorPos, 100);
            var fleepos = wposex.Distance(SecondW.Pos) > Game.CursorPos.Distance(SecondW.Pos);

            if (countW) { return; }
            if (fleepos)
            {
                CastSecondW(LeBlancConfig.SubMenu("Misc").SubMenu("backW").Item("SWpos").GetValue<bool>());
            }
        }

        private static void Clones()
        {
            if (Game.Time > Clone.ExpireTime)
                return;

            var mirror = Clone.Object as Obj_AI_Base;
            var nextenemy =
                ObjectManager.Get<Obj_AI_Base>().OrderBy(h => h.Distance(Player.ServerPosition, true)).FirstOrDefault(h => h.IsEnemy);
            var pos = Player.ServerPosition.Extend(nextenemy.ServerPosition, 200);

            if (mirror != null && LeBlancConfig.SubMenu("Misc").Item("Clone").GetValue<bool>())
            {
                mirror.IssueOrder(GameObjectOrder.MovePet, pos);
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!LeBlancConfig.Item("Interrupt").GetValue<bool>() || spell.DangerLevel != InterruptableDangerLevel.High)
                return;

            CastE(unit, HitChance.Medium ,true);
            CastRe(unit, HitChance.Medium, true);
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!LeBlancConfig.Item("Interrupt").GetValue<bool>())
                return;

            CastE(gapcloser.Sender, HitChance.Medium, true);
            CastRe(gapcloser.Sender, HitChance.Medium, true);
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!(sender.Name.Contains("LeBlanc_Base_W_return_indicator.troy")))
            {
                return;
            }
            SecondW.Pos = new Vector3(0, 0, 0);
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("LeBlanc_Base_W_return_indicator.troy") && !sender.IsMe)
            {
                    SecondW.Object = sender;
                    SecondW.Pos = sender.Position;
                    SecondW.ExpireTime = Game.Time + 4;
            }
            if (sender.Name.Contains("LeBlanc_MirrorImagePoff.troy") && sender.IsMe)
            {
                Clone.Object = sender;
                Clone.Pos = sender.Position;
                Clone.ExpireTime = Game.Time + 8;
            }
        }
        #endregion Misc
        #region Spells
        private static void Spells()
        {
            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 970);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.5f, 220, 1500, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.366f, 70, 1600, true, SkillshotType.SkillshotLine);

            var name = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name;

            switch (name)
            {
                case "LeblancChaosOrbM":
                    {
                        R = new Spell(SpellSlot.R, Q.Range);
                        break;
                    }
                case "LeblancSlideM":
                    {
                        R = new Spell(SpellSlot.R, W.Range);
                        R.SetSkillshot(0.5f, 220, 1500, false, SkillshotType.SkillshotCircle);
                        break;
                    }
                case "LeblancSoulShackleM":
                    {
                        R = new Spell(SpellSlot.R, E.Range);
                        R.SetSkillshot(0.366f, 70, 1600, true, SkillshotType.SkillshotLine);
                        break;
                    }
            }
        }

        private static void CastQ(Obj_AI_Base t, bool useQ)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(Q.Range) || !Q.IsReady() || !useQ)
                return;

            Q.CastOnUnit(t, PacketCast);
        }
        private static void CastW(Obj_AI_Base t, bool useW)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(W.Range) || !W.IsReady() || !useW || W.Instance.Name != "LeblancSlide")
                return;

            W.Cast(t.ServerPosition, PacketCast);
        }
        private static void CastSecondW(bool useSecondW)
        {
            if (!W.IsReady() || !useSecondW || W.Instance.Name == "LeblancSlide")
                return;

            W.Cast(PacketCast);
        }
        private static void CastE(Obj_AI_Base t, HitChance hitE, bool useE)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(E.Range) || !E.IsReady() || !useE)
                return;

            E.CastIfHitchanceEquals(t, hitE, PacketCast);
        }
        private static void CastRq(Obj_AI_Base t, bool useR)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(Q.Range) || !R.IsReady() || !useR || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name != "LeblancChaosOrbM")
                return;

            R.CastOnUnit(t, PacketCast);
        }
        private static void CastRw(Obj_AI_Base t, bool useR)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(W.Range) || !R.IsReady() || !useR || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name != "LeblancSlideM")
                return;

            R.Cast(t.ServerPosition, PacketCast);
        }
        private static void CastRe(Obj_AI_Base t, HitChance hitR, bool useR)
        {
            if (t.IsInvulnerable || !t.IsValidTarget(E.Range) || !R.IsReady() || !useR || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name != "LeblancSoulShackleM")
                return;

            R.CastIfHitchanceEquals(t, hitR, PacketCast);
        }
        #endregion Spells
        #region Drawing
        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
                return;

            var q = LeBlancConfig.Item("drawQ").GetValue<Circle>();
            var w = LeBlancConfig.Item("drawW").GetValue<Circle>();
            var e = LeBlancConfig.Item("drawE").GetValue<Circle>();
            var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
            var timer = (SecondW.ExpireTime - Game.Time > 0) ? (SecondW.ExpireTime - Game.Time) : 0;

            if (SecondW.Pos != new Vector3(0, 0, 0))
            {
                Drawing.DrawText(wts.X - 35, wts.Y + 10, Color.White, "Second W: " + timer.ToString("0.0"));
            }

            if (SecondW.Pos != new Vector3(0, 0, 0))
            {
                Render.Circle.DrawCircle(SecondW.Pos, 100, Color.Red, 50);
            }

            if (q.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, q.Color);
            if (w.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, w.Color);
            if (e.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, e.Color);

            if (LeBlancConfig.Item("haraKey").GetValue<KeyBind>().Active)
                Drawing.DrawText(Player.HPBarPosition.X + 40, Player.HPBarPosition.Y - 15, Color.Red, "Harass On");
        }
        #endregion Drawing
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var ap = ObjectManager.Player.BaseAbilityDamage + ObjectManager.Player.FlatMagicDamageMod;
            var dmg = 0d;

            if (Q.IsReady())
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.Q);

                if ((W.IsReady() && W.Instance.Name == "LeblancSlide") || E.IsReady() || R.IsReady())
                    dmg += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (R.IsReady())
            {
                var name = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name;
                var maxDmg = new[] { 200, 400, 600 }[R.Level] + 1.3 * ap;

                switch (name)
                {
                    case "LeblancChaosOrbM":
                        {
                            var qDmg = new[] { 100, 200, 300 }[R.Level] + 0.65 * ap;

                            dmg += Player.CalcDamage(enemy, Damage.DamageType.Magical, qDmg);

                            if ((W.IsReady() && W.Instance.Name == "LeblancSlide") || E.IsReady() || Q.IsReady())
                            {
                                dmg += Player.CalcDamage(enemy, Damage.DamageType.Magical, maxDmg);
                            }
                            break;
                        }
                    case "LeblancSlideM":
                        {
                            var wDmg = new[] { 150, 300, 450 }[R.Level] + 0.975 * ap;
                            dmg += Player.CalcDamage(enemy, Damage.DamageType.Magical, wDmg);
                            break;
                        }
                    case "LeblancSoulShackleM":
                        {
                            var eDmg = new[] { 100, 200, 300 }[R.Level] + 0.65 * ap;

                            dmg += Player.CalcDamage(enemy, Damage.DamageType.Magical, eDmg);

                            if (Player.CalcDamage(enemy, Damage.DamageType.Magical, eDmg) > maxDmg)
                            {
                                dmg += Player.CalcDamage(enemy, Damage.DamageType.Magical, maxDmg);
                            }
                            break;
                        }
                }
            }

            if (enemy.HasBuff("LeblancChaosOrbM", true))
            {
                var qDmg = new[] { 100, 200, 300 }[R.Level] + 0.65 * ap;

                dmg += Player.CalcDamage(enemy, Damage.DamageType.Magical, qDmg);
            }

            if (enemy.HasBuff("LeblancChaosOrb", true))
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (Dfg.IsReady())
                dmg += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }
            dmg += Player.GetAutoAttackDamage(enemy, true) * 1;

            return (float)dmg * (Dfg.IsReady() ? 1.2f : 1);
        }
        private static Obj_AI_Hero GetEnemy(float vDefaultRange = 0, TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Magical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
                vDefaultRange = E.Range;

            if (!Program.LeBlancConfig.Item("AssassinActive").GetValue<bool>())
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);

            var assassinRange = Program.LeBlancConfig.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy = ObjectManager.Get<Obj_AI_Hero>()
                .Where(
                    enemy =>
                        enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                        Program.LeBlancConfig.Item("Assassin" + enemy.ChampionName) != null &&
                        Program.LeBlancConfig.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                        ObjectManager.Player.Distance(enemy) < assassinRange);

            if (Program.LeBlancConfig.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }

    }
}
