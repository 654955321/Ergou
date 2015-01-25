/*
___________             __   .__                    _____                                              .____                     _________.__        
\_   _____/_ __   ____ |  | _|__| ____    ____     /  _  \__  _  __ ____   __________   _____   ____   |    |    ____   ____    /   _____/|__| ____  
 |    __)|  |  \_/ ___\|  |/ /  |/    \  / ___\   /  /_\  \ \/ \/ // __ \ /  ___/  _ \ /     \_/ __ \  |    |  _/ __ \_/ __ \   \_____  \ |  |/    \ 
 |     \ |  |  /\  \___|    <|  |   |  \/ /_/  > /    |    \     /\  ___/ \___ (  <_> )  Y Y  \  ___/  |    |__\  ___/\  ___/   /        \|  |   |  \
 \___  / |____/  \___  >__|_ \__|___|  /\___  /  \____|__  /\/\_/  \___  >____  >____/|__|_|  /\___  > |_______ \___  >\___  > /_______  /|__|___|  /
     \/              \/     \/       \//_____/           \/            \/     \/            \/     \/          \/   \/     \/          \/         \/ 
*/
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Linq;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSin
{
    class Program
    {
        #region Params
        public static string ChampName = "LeeSin";
        public static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        public static Spell Q,W, E, R;
        public static Spellbook SBook;
        public static Items.Item Dfg;
        public static Vector2 JumpPos;
        public static Vector3 mouse = Game.CursorPos;
        public static SpellSlot smiteSlot;
        public static SpellSlot flashSlot;
        public static Menu Menu;
        public static bool CastQAgain;
        public static bool CastWardAgain = true;
        public static bool reCheckWard = true;
        public static bool wardJumped = false;
        public static Obj_AI_Base minionerimo;
        public static bool checkSmite = false;
        public static bool delayW = false;
        public static Vector2 insecLinePos;
        public static float TimeOffset;
        public static Vector3 lastWardPos;
        public static float lastPlaced;
        public static int passiveStacks;
        public static float passiveTimer;
        public static bool waitforjungle;
        public static bool waitingForQ2 = false;
        public static bool q2Done = false;
        public static float q2Timer;
        public static int clickCount;
        public static Vector3 insecClickPos;
        public static float resetTime;
        public static bool clicksecEnabled;
        public static float doubleClickReset;
        public static Vector3 lastClickPos;
        public static bool lastClickBool;


        public static bool textRendered;
        private static readonly string[] epics =
        {
            "SRU_Baron", "SRU_Dragon"
        };
        private static readonly string[] buffs =
        {
            "SRU_Red", "SRU_Blue"
        };
        private static readonly string[] buffandepics =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron"
        };
        private static readonly string[] bigjungleminions =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Gromp", "SRU_Krug"
        };
        private static readonly string[] spells =
        {
            "BlindMonkQOne", "BlindMonkWOne", "BlindMonkEOne", "blindmonkwtwo", "blindmonkqtwo", "blindmonketwo", "BlindMonkRKick"
        };
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN || !paramBool("clickInsec"))
                return;
            var asec = ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy && a.Distance(Game.CursorPos) < 200 && a.IsValid && !a.IsDead);
            if (asec.Any())
                return;
            if (!lastClickBool || clickCount == 0)
            {
                clickCount++;
                lastClickPos = Game.CursorPos;
                lastClickBool = true;
                doubleClickReset = Environment.TickCount + 600;
                return;
            }
            if (lastClickBool && lastClickPos.Distance(Game.CursorPos) < 200)
            {
                clickCount++;
                lastClickBool = false;
            }
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && passiveStacks > 0)
            {
                passiveStacks = passiveStacks - 1;
            }
            
        }

        public static SpellSlot IgniteSlot;
        #endregion

        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_GeneralParticleEmitter))
                return;
            if (sender.Name.Contains("blindMonk_Q_resonatingStrike") && waitingForQ2)
            {
                waitingForQ2 = false;
                q2Done = true;
                q2Timer = Environment.TickCount + 800;
            }
        }

        #region OnLoad

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampName) return;
            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            flashSlot = Player.GetSpellSlot("summonerflash");

            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 430);
            R = new Spell(SpellSlot.R, 375);
            
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed,true,SkillshotType.SkillshotLine);
            
            //Base menu
            Menu = new Menu("花边汉化-FA瞎子", ChampName, true);
            //Orbwalker and menu
            Menu.AddSubMenu(new Menu("走 砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);
            //Combo menu
            Menu.AddSubMenu(new Menu("连 招", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useQ2", "使用 2段Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useW", "连招时丨摸眼").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("dsjk", "摸眼|如果范围: "));
            Menu.SubMenu("Combo").AddItem(new MenuItem("wMode", "> 平A范围|| > Q范围").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useR", "使用 R").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ksR", "R 击杀").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("starCombo", "开始 连招").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("random2ejwej", "W->Q->R->Q2"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("aaStacks", "等待 被动").SetValue(false));

            var harassMenu = new Menu("骚 扰", "Harass");
            harassMenu.AddItem(new MenuItem("q1H", "使用 Q1").SetValue(true));
            harassMenu.AddItem(new MenuItem("q2H", "使用 Q2").SetValue(true));
            harassMenu.AddItem(new MenuItem("wH", "摸眼|W逃跑{受伤}").SetValue(true));
            harassMenu.AddItem(new MenuItem("eH", "使用 E1").SetValue(false));
            Menu.AddSubMenu(harassMenu);

            //Jung/Wave Clear
            var waveclearMenu = new Menu("清线|清野", "wjClear");
            waveclearMenu.AddItem(new MenuItem("sjasjsdsjs", "清 线"));
            waveclearMenu.AddItem(new MenuItem("useQClear", "使用 Q").SetValue(true));
            waveclearMenu.AddItem(new MenuItem("useEClear", "使用 E").SetValue(true));
            waveclearMenu.AddItem(new MenuItem("sjasjjs", "清 野"));
            waveclearMenu.AddItem(new MenuItem("jungActive", "开启 按键").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            waveclearMenu.AddItem(new MenuItem("Qjng", "使用 Q").SetValue(true));
            waveclearMenu.AddItem(new MenuItem("Wjng", "使用 W").SetValue(true));
            waveclearMenu.AddItem(new MenuItem("Ejng", "使用 E").SetValue(true));
            Menu.AddSubMenu(waveclearMenu);

            //InsecMenu
            var insecMenu = new Menu("Insec", "Insec");
            insecMenu.AddItem(new MenuItem("InsecEnabled", "使用回旋踢").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            insecMenu.AddItem(new MenuItem("rnshsasdhjk", "Insec 模式:"));
            insecMenu.AddItem(new MenuItem("insecMode", "左键点击 [on] TS [off]").SetValue(true));
            insecMenu.AddItem(new MenuItem("insecOrbwalk", "使用 走砍").SetValue(true));
            insecMenu.AddItem(new MenuItem("flashInsec", "闪现 R").SetValue(false));
            insecMenu.AddItem(new MenuItem("waitForQBuff", "等待Q丨技能CD").SetValue(false));
            insecMenu.AddItem(new MenuItem("22222222222222", "(更多更快伤害)"));
            insecMenu.AddItem(new MenuItem("clickInsec", "点击 R").SetValue(true));
            var lM = insecMenu.AddSubMenu(new Menu("点击 指令 (解说)", "clickInstruct"));
            lM.AddItem(new MenuItem("1223342334", "首先 选择目标"));
            lM.AddItem(new MenuItem("122334233", "然后. 就会对你的目标强暴"));
            insecMenu.AddItem(new MenuItem("insec2champs", "R向队友").SetValue(true));
            insecMenu.AddItem(new MenuItem("bonusRangeA", "R向队友范围").SetValue(new Slider(0, 0, 1000)));
            insecMenu.AddItem(new MenuItem("insec2tower", "R向塔底").SetValue(true));
            insecMenu.AddItem(new MenuItem("bonusRangeT", "R向塔底范围").SetValue(new Slider(0, 0, 1000)));
            insecMenu.AddItem(new MenuItem("insec2orig", "R向原始位置").SetValue(true));
            insecMenu.AddItem(new MenuItem("22222222222", "花边汉化-FA瞎子"));
            insecMenu.AddItem(new MenuItem("instaFlashInsec1", "手动使用R"));
            insecMenu.AddItem(new MenuItem("instaFlashInsec2", "使用闪现R"));
            insecMenu.AddItem(new MenuItem("instaFlashInsec", "开 启").SetValue(new KeyBind("P".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.AddSubMenu(insecMenu);

            var autoSmiteSettings = new Menu("惩戒 设置", "Auto Smite Settings");
            autoSmiteSettings.AddItem(new MenuItem("smiteEnabled", "开 启").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            var itemSelMenu = autoSmiteSettings.AddSubMenu(new Menu("选择 惩戒 目标", "sst"));
            itemSelMenu.AddItem(new MenuItem("SRU_Red", "红 Buff").SetValue(true));
            itemSelMenu.AddItem(new MenuItem("SRU_Blue", "蓝 Buff").SetValue(true));
            itemSelMenu.AddItem(new MenuItem("SRU_Dragon", "小龙").SetValue(true));
            itemSelMenu.AddItem(new MenuItem("SRU_Baron", "大龙").SetValue(true));
            autoSmiteSettings.AddItem(new MenuItem("qqSmite", "Q->惩戒->Q").SetValue(true));
            autoSmiteSettings.AddItem(new MenuItem("normSmite", "正常 惩戒").SetValue(true));
            autoSmiteSettings.AddItem(new MenuItem("drawSmite", "显示 惩戒 范围").SetValue(true));
            Menu.AddSubMenu(autoSmiteSettings);

            //SaveMe Menu
            var SaveMeMenu = new Menu("惩戒 节省 设置", "Smite Save Settings");
            SaveMeMenu.AddItem(new MenuItem("smiteSave", "启 用").SetValue(true));
            SaveMeMenu.AddItem(new MenuItem("hpPercentSM", "W惩戒 on x%").SetValue(new Slider(10, 1)));
            SaveMeMenu.AddItem(new MenuItem("param1", "禁用惩戒|血量设定")); // TBC
            SaveMeMenu.AddItem(new MenuItem("dBuffs", "Buff").SetValue(true));// TBC
            SaveMeMenu.AddItem(new MenuItem("hpBuffs", "HP %").SetValue(new Slider(30, 1)));// TBC
            SaveMeMenu.AddItem(new MenuItem("dEpics", "野怪").SetValue(true));// TBC
            SaveMeMenu.AddItem(new MenuItem("hpEpics", "HP %").SetValue(new Slider(10, 1)));// TBC
            Menu.AddSubMenu(SaveMeMenu);
            //Wardjump menu
            var wardjumpMenu = new Menu("摸 眼", "Wardjump");
            wardjumpMenu.AddItem(
                new MenuItem("wjump", "摸 眼 key").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            wardjumpMenu.AddItem(new MenuItem("m2m", "移动到鼠标位置再摸眼").SetValue(true));
            wardjumpMenu.AddItem(new MenuItem("j2m", "到小兵").SetValue(true));
            wardjumpMenu.AddItem(new MenuItem("j2c", "到敌人").SetValue(true));
            Menu.AddSubMenu(wardjumpMenu);

            var drawMenu = new Menu("范 围", "Drawing");
            drawMenu.AddItem(new MenuItem("DrawEnabled", "开 启").SetValue(false));
            drawMenu.AddItem(new MenuItem("drawST", "显示 惩戒 文字").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawOutLineST", "显示 轮廓").SetValue(true));
            drawMenu.AddItem(new MenuItem("insecDraw", "回旋踢 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("WJDraw", "显示 瞬眼 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawE", "显示 E 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            var miscMenu = new Menu("杂 项", "Misc");
            miscMenu.AddItem(new MenuItem("NFE", "使用 封包?").SetValue(true));
            miscMenu.AddItem(new MenuItem("QHC", "Q 命中率").SetValue(new StringList(new []{"慢", "正常", "快", "超级 快"}, 1)));
            miscMenu.AddItem(new MenuItem("IGNks", "使用 点燃?").SetValue(true));
            miscMenu.AddItem(new MenuItem("qSmite", "智能 Q!").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += GameObject_OnCreate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnWndProc += Game_OnWndProc;

            PrintMessage("FA鑽夋偿椹嫍鐬庢眽鍖朆y鑺辫竟!");
        }

        #endregion

        #region Harass
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range + 200, TargetSelector.DamageType.Physical);
            var q = paramBool("q1H");
            var q2 = paramBool("q2H");
            var e = paramBool("eH");
            var w = paramBool("wH");

            if (q && Q.IsReady() && Q.Instance.Name == "BlindMonkQOne" && target.IsValidTarget(Q.Range) && q) CastQ1(target);
            if (q2 && Q.IsReady() &&
               (target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)) && q2)
            {
                if (CastQAgain || !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player))) Q.Cast();
            }
            if (e && E.IsReady() && target.IsValidTarget(E.Range) && E.Instance.Name == "BlindMonkEOne" && e) E.Cast();
            if (w && Player.Distance(target) < 50 && !(target.HasBuff("BlindMonkQOne", true) && !target.HasBuff("blindmonkqonechaos", true)) && (E.Instance.Name == "blindmonketwo" || !E.IsReady() && e) && (Q.Instance.Name == "blindmonkqtwo" || !Q.IsReady() && q))
            {
                var min =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(a => a.IsAlly && a.Distance(Player) <= W.Range)
                        .OrderByDescending(a => a.Distance(target))
                        .FirstOrDefault();
                W.CastOnUnit(min);
            }

        }
        #endregion

        #region Insec
        public static bool isNullInsecPos = true;
        public static Vector3 insecPos;

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (spells.Contains(args.SData.Name))
            {
                passiveStacks = 2;
                passiveTimer = Environment.TickCount + 3000;
            }
            if (args.SData.Name == "BlindMonkQOne")
            {
                CastQAgain = false;
                Utility.DelayAction.Add(2900, () =>
                {
                    CastQAgain = true;
                });
            }
            if (Menu.Item("instaFlashInsec").GetValue<KeyBind>().Active && args.SData.Name == "BlindMonkRKick")
            {
                Player.Spellbook.CastSpell(flashSlot, getInsecPos((Obj_AI_Hero) (args.Target)));
            }
            if (args.SData.Name == "summonerflash" && InsecComboStep != InsecComboStepSelect.NONE)
            {
                Obj_AI_Hero target = paramBool("insecMode")
                   ? TargetSelector.GetSelectedTarget()
                   : TargetSelector.GetTarget(Q.Range + 200, TargetSelector.DamageType.Physical);
                InsecComboStep = InsecComboStepSelect.PRESSR;
                Utility.DelayAction.Add(80, () => R.CastOnUnit(target, true));
            }
            if (args.SData.Name == "blindmonkqtwo")
            {
                waitingForQ2 = true;
                Utility.DelayAction.Add(3000, () =>
                {
                    waitingForQ2 = false;
                });
            }
            if (args.SData.Name == "BlindMonkRKick")
                InsecComboStep = InsecComboStepSelect.NONE;
        }

        public static Vector3 getInsecPos(Obj_AI_Hero target)
        {

            if (clicksecEnabled && paramBool("clickInsec"))
            {
                insecLinePos = Drawing.WorldToScreen(insecClickPos);
                return V2E(insecClickPos, target.Position, target.Distance(insecClickPos) + 230).To3D();
            }
            if (isNullInsecPos)
            {
                isNullInsecPos = false;
                insecPos = Player.Position;
            }
            var turrets = (from tower in ObjectManager.Get<Obj_Turret>()
                           where tower.IsAlly && !tower.IsDead && target.Distance(tower.Position) < 1500 + Menu.Item("bonusRangeT").GetValue<Slider>().Value && tower.Health > 0
                select tower).ToList();
            if (GetAllyHeroes(target, 2000 + Menu.Item("bonusRangeA").GetValue<Slider>().Value).Count > 0 && paramBool("insec2champs"))
            {
                Vector3 insecPosition = InterceptionPoint(GetAllyInsec(GetAllyHeroes(target, 2000 + Menu.Item("bonusRangeA").GetValue<Slider>().Value)));
                insecLinePos = Drawing.WorldToScreen(insecPosition);
                return V2E(insecPosition, target.Position, target.Distance(insecPosition) + 230).To3D();

            } 
            if(turrets.Any() && paramBool("insec2tower"))
            {
                insecLinePos = Drawing.WorldToScreen(turrets[0].Position);
                return V2E(turrets[0].Position, target.Position, target.Distance(turrets[0].Position) + 230).To3D();
            }
            if (paramBool("insec2orig"))
            {
                insecLinePos = Drawing.WorldToScreen(insecPos);
                return V2E(insecPos, target.Position, target.Distance(insecPos) + 230).To3D();
            }
            return new Vector3();
        }
        enum InsecComboStepSelect { NONE, QGAPCLOSE, WGAPCLOSE, PRESSR };
        static InsecComboStepSelect InsecComboStep;
        static void InsecCombo(Obj_AI_Hero target)
        {
            if (target != null && target.IsVisible)
            {
                if (Player.Distance(getInsecPos(target)) < 200)
                {
                    InsecComboStep = InsecComboStepSelect.PRESSR;
                }
                else if (InsecComboStep == InsecComboStepSelect.NONE &&
                         getInsecPos(target).Distance(Player.Position) < 600)
                    InsecComboStep = InsecComboStepSelect.WGAPCLOSE;
                else if (InsecComboStep == InsecComboStepSelect.NONE && target.Distance(Player) < Q.Range)
                    InsecComboStep = InsecComboStepSelect.QGAPCLOSE;

                switch (InsecComboStep)
                {
                    case InsecComboStepSelect.QGAPCLOSE:
                        if (!(target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)) &&
                            Q.Instance.Name == "BlindMonkQOne")
                        {
                            CastQ1(target);
                        }
                        else if ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)))
                        {
                            Q.Cast();
                            InsecComboStep = InsecComboStepSelect.WGAPCLOSE;
                        }
                        else
                        {
                            if (Q.Instance.Name == "blindmonkqtwo" && returnQBuff().Distance(target) <= 600)
                            {
                                Q.Cast();
                            }
                        }
                        break;
                    case InsecComboStepSelect.WGAPCLOSE:
                        if (W.IsReady() && W.Instance.Name == "BlindMonkWOne" &&
                            (paramBool("waitForQBuff") && (Q.Instance.Name == "BlindMonkQOne" || (!Q.IsReady() || Q.Instance.Name == "blindmonkqtwo") && q2Done)) || !paramBool("waitForQBuff"))
                        {
                            WardJump(getInsecPos(target), false, false, true);
                            wardJumped = true;
                        }
                        else if (Player.Spellbook.CanUseSpell(flashSlot) == SpellState.Ready &&
                                 paramBool("flashInsec") && !wardJumped && Player.Distance(insecPos) < 400 ||
                                 Player.Spellbook.CanUseSpell(flashSlot) == SpellState.Ready &&
                                 paramBool("flashInsec") && !wardJumped && Player.Distance(insecPos) < 400 &&
                                 FindBestWardItem() == null)
                        {
                            Player.Spellbook.CastSpell(flashSlot, getInsecPos(target));
                            Utility.DelayAction.Add(50, () => R.CastOnUnit(target, true));
                        }
                        break;
                    case InsecComboStepSelect.PRESSR:
                        R.CastOnUnit(target, true);
                        break;
                }
            }
        }
        static Vector3 InterceptionPoint(List<Obj_AI_Hero> heroes)
        {
            Vector3 result = new Vector3();
            foreach (Obj_AI_Hero hero in heroes)
            result += hero.Position;
            result.X /= heroes.Count;
            result.Y /= heroes.Count;
            return result;
        }
        static List<Obj_AI_Hero> GetAllyInsec(List<Obj_AI_Hero> heroes)
        {
            byte alliesAround = 0;
            Obj_AI_Hero tempObject = new Obj_AI_Hero();
            foreach (Obj_AI_Hero hero in heroes)
            {
                int localTemp = GetAllyHeroes(hero, 500 + Menu.Item("bonusRangeA").GetValue<Slider>().Value).Count;
                if (localTemp > alliesAround)
                {
                    tempObject = hero;
                    alliesAround = (byte)localTemp;
                }
            }
            return GetAllyHeroes(tempObject, 500 + Menu.Item("bonusRangeA").GetValue<Slider>().Value);
        }
        private static List<Obj_AI_Hero> GetAllyHeroes(Obj_AI_Hero position, int range)
        {
            List<Obj_AI_Hero> temp = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                if (hero.IsAlly && !hero.IsMe && hero.Distance(position) < range)
                    temp.Add(hero);
            return temp;
        }

        static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }
        #endregion

        #region SmiteSaver
        public static void SaveMe()
        {
            if ((Player.Health / Player.MaxHealth * 100) > Menu.Item("hpPercentSM").GetValue<Slider>().Value || Player.Spellbook.CanUseSpell(smiteSlot) != SpellState.Ready) return;
            var epicSafe = false;
            var buffSafe = false;
            foreach (
                var minion in
                    MinionManager.GetMinions(Player.Position, 1100f, MinionTypes.All, MinionTeam.Neutral,
                        MinionOrderTypes.None))
            {
                foreach (var minionName in epics)
                {
                    if (minion.BaseSkinName == minionName && hpLowerParam(minion, "hpEpics") && paramBool("dEpics"))
                    {
                        epicSafe = true;
                        break;
                    }
                }
                foreach (var minionName in buffs)
                {
                    if (minion.BaseSkinName == minionName && hpLowerParam(minion, "hpBuffs") && paramBool("dBuffs"))
                    {
                        buffSafe = true;
                        break;
                    }
                }
            }

            if(epicSafe || buffSafe) return;

            foreach (var minion in MinionManager.GetMinions(Player.Position, 700f, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth))
            {
                if (!W.IsReady() && !Player.HasBuff("BlindMonkIronWill") || smiteSlot == SpellSlot.Unknown ||
                    smiteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(smiteSlot) != SpellState.Ready) break;
                if (minion.Name.ToLower().Contains("ward")) return;
                if (W.Instance.Name != "blindmonkwtwo")
                {
                    W.Cast();
                    W.Cast();
                }
                if (Player.HasBuff("BlindMonkIronWill"))
                {
                    Player.Spellbook.CastSpell(smiteSlot, minion);
                }
            }
        }
        #endregion

        #region Tick Tasks
        static void Game_OnGameUpdate(EventArgs args)
        {
            smiteSlot = Player.GetSpellSlot(smitetype());

            if (doubleClickReset <= Environment.TickCount && clickCount != 0)
            {
                doubleClickReset = float.MaxValue;
                clickCount = 0;
            }

            if (clickCount >= 2 && paramBool("clickInsec"))
            {
                resetTime = Environment.TickCount + 3000;
                clicksecEnabled = true;
                insecClickPos = Game.CursorPos;
                clickCount = 0;
            }

            if (passiveTimer <= Environment.TickCount) passiveStacks = 0;

            if (resetTime <= Environment.TickCount && !Menu.Item("InsecEnabled").GetValue<KeyBind>().Active && clicksecEnabled) clicksecEnabled = false;

            if (q2Timer <= Environment.TickCount) q2Done = false;

            if(Player.IsDead) return;

            if (Menu.Item("jungActive").GetValue<KeyBind>().Active) JungleClear();

            if ((paramBool("insecMode")
                ? TargetSelector.GetSelectedTarget()
                : TargetSelector.GetTarget(Q.Range + 200, TargetSelector.DamageType.Physical)) == null)
            {
                InsecComboStep = InsecComboStepSelect.NONE;
            }

            if (Menu.Item("smiteEnabled").GetValue<KeyBind>().Active) smiter();

            if (Menu.Item("starCombo").GetValue<KeyBind>().Active) wardCombo();

            if (paramBool("smiteSave")) SaveMe();

            if (paramBool("IGNks"))
            {
                Obj_AI_Hero NewTarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);

                if (NewTarget != null && IgniteSlot != SpellSlot.Unknown
                    && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready
                    && ObjectManager.Player.GetSummonerSpellDamage(NewTarget, Damage.SummonerSpell.Ignite) > NewTarget.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, NewTarget);
                }
            }

            if (Menu.Item("InsecEnabled").GetValue<KeyBind>().Active)
            {
                if (paramBool("insecOrbwalk"))
                {
                    Orbwalk(Game.CursorPos);
                }
                Obj_AI_Hero newTarget = paramBool("insecMode")
                    ? TargetSelector.GetSelectedTarget()
                    : TargetSelector.GetTarget(Q.Range + 200, TargetSelector.DamageType.Physical);  

                 if(newTarget != null) InsecCombo(newTarget);
            }
            else
            {
                isNullInsecPos = true;
                wardJumped = false;
            }

            if(Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo) InsecComboStep = InsecComboStepSelect.NONE;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    StarCombo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    AllClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }

            if(Menu.Item("wjump").GetValue<KeyBind>().Active)
                wardjumpToMouse();
            }
#endregion

        #region Draw
        static void Drawing_OnDraw(EventArgs args)
        {
            Obj_AI_Hero newTarget = paramBool("insecMode")
                   ? TargetSelector.GetSelectedTarget()
                   : TargetSelector.GetTarget(Q.Range + 200, TargetSelector.DamageType.Physical);
            if (clicksecEnabled)
            {
                Utility.DrawCircle(insecClickPos, 100, System.Drawing.Color.White);
            }
            if (Menu.Item("instaFlashInsec").GetValue<KeyBind>().Active) Drawing.DrawText(960, 340, System.Drawing.Color.Red, "FLASH INSEC ENABLED");
            if (newTarget != null && newTarget.IsVisible && Player.Distance(newTarget) < 3000 && paramBool("insecDraw"))
            {
                Vector2 targetPos = Drawing.WorldToScreen(newTarget.Position);
                Drawing.DrawLine(insecLinePos.X, insecLinePos.Y, targetPos.X, targetPos.Y, 3, System.Drawing.Color.White);
                Utility.DrawCircle(getInsecPos(newTarget), 100, System.Drawing.Color.White);
            }
            if (!paramBool("DrawEnabled")) return;
            foreach (var t in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(t.HasBuff("BlindMonkQOne", true) || t.HasBuff("blindmonkqonechaos", true)) Drawing.DrawCircle(t.Position, 200, System.Drawing.Color.Red);
            }
            if (Menu.Item("smiteEnabled").GetValue<KeyBind>().Active && paramBool("drawSmite"))
            {
                Utility.DrawCircle(Player.Position, 700, System.Drawing.Color.White);
            }
            if (Menu.Item("wjump").GetValue<KeyBind>().Active && paramBool("WJDraw"))
            {   
                Utility.DrawCircle(JumpPos.To3D(), 20, System.Drawing.Color.Red);
                Utility.DrawCircle(Player.Position, 600, System.Drawing.Color.Red);
            }
            if (paramBool("drawQ")) Utility.DrawCircle(Player.Position, Q.Range - 80, Q.IsReady() ? System.Drawing.Color.LightSkyBlue : System.Drawing.Color.Tomato);
            if (paramBool("drawW")) Utility.DrawCircle(Player.Position, W.Range - 80, W.IsReady() ? System.Drawing.Color.LightSkyBlue : System.Drawing.Color.Tomato);
            if (paramBool("drawE")) Utility.DrawCircle(Player.Position, E.Range - 80, E.IsReady() ? System.Drawing.Color.LightSkyBlue : System.Drawing.Color.Tomato);
            if (paramBool("drawR")) Utility.DrawCircle(Player.Position, R.Range - 80, R.IsReady() ? System.Drawing.Color.LightSkyBlue : System.Drawing.Color.Tomato);

        }
        #endregion

        #region Autosmite
        public static void smiter()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().Where(a => buffandepics.Contains(a.BaseSkinName) && a.Distance(Player) <= 1300).FirstOrDefault() ;
            if (minion != null)
            {
                if (Menu.Item(minion.BaseSkinName).GetValue<bool>())
                    {
                        minionerimo = minion;
                        if (SmiteDmg() > minion.Health && minion.IsValidTarget(780) && paramBool("normSmite")) Player.Spellbook.CastSpell(smiteSlot, minion);
                        if (minion.Distance(Player) < 100 && checkSmite)
                        {
                            checkSmite = false;
                            Player.Spellbook.CastSpell(smiteSlot, minion);
                        }
                        if (!Q.IsReady() || !paramBool("qqSmite")) return;
                        if (Q2Damage(minion, ((float) SmiteDmg() + Q.GetDamage(minion)), true) + SmiteDmg() >
                            minion.Health &&
                            !(minion.HasBuff("BlindMonkQOne", true) || minion.HasBuff("blindmonkqonechaos", true)))
                        {
                            Q.Cast(minion, true);
                        }
                        if ((Q2Damage(minion, (float) SmiteDmg(), true) + SmiteDmg()) > minion.Health &&
                            (minion.HasBuff("BlindMonkQOne", true) || minion.HasBuff("blindmonkqonechaos", true)))
                        {
                            Q.CastOnUnit(Player, true);
                            checkSmite = true;
                        }
                        if ((minion.HasBuff("BlindMonkQOne", true) || minion.HasBuff("blindmonkqonechaos", true)) &&
                            CastQAgain ||
                            (minion.HasBuff("BlindMonkQOne", true) || minion.HasBuff("blindmonkqonechaos", true)) &&
                            Q2Damage(minion, 0, true) > minion.Health)
                        {
                            Q.CastOnUnit(Player, true);
                        }
                    }
            }
        }

        #endregion

        #region WaveClear

        public static void JungleClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (minion == null)
                return;
            var passiveIsActive = passiveStacks > 0;
            useClearItems(minion);
            if (Q.IsReady() && paramBool("Qjng"))
                {
                    if ((minion.HasBuff("BlindMonkQOne", true) || minion.HasBuff("blindmonkqonechaos", true)) &&
                        (CastQAgain) || Q.GetDamage(minion, 1) > minion.Health)
                    {
                        Q.Cast(packets());
                    }
                }
                if (passiveIsActive || waitforjungle)
                {
                    return;
                }
                if (paramBool("Qjng") && Q2Damage(minion, Q.Instance.Name == "BlindMonkQOne" ? minion.Health - Q.GetDamage(minion) : minion.Health, true) > minion.Health && Q.IsReady())
                {
                    if (Q.Instance.Name == "BlindMonkQOne")
                    {
                        Q.Cast(minion, packets());
                        waiter();
                        return;
                    }
                    Q.Cast(packets());
                    waiter();
                    return;
                    
                }
                if (paramBool("Wjng") && W.IsReady() && minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 200))
                {
                    if (W.Instance.Name == "BlindMonkWOne")
                    {
                        W.Cast(packets());
                        waiter();
                        return;
                    }
                    W.Cast(packets());
                    waiter();
                    return;
                }
                if (paramBool("Qjng") && Q.IsReady() && minion.IsValidTarget(Q.Range))
                {
                    if (Q.Instance.Name == "BlindMonkQOne")
                    {
                        Q.Cast(minion, packets());
                        waiter();
                        return;
                    }
                    if ((minion.HasBuff("BlindMonkQOne", true) || minion.HasBuff("blindmonkqonechaos", true)))
                    {
                        Q.Cast(packets());
                        waiter();
                        return;
                    }
                }
                if (paramBool("Ejng") && E.IsReady() && minion.IsValidTarget(E.Range))
                {
                    E.Cast(packets());
                    waiter();
                    return;
                }
        }

        public static void waiter()
        {
            waitforjungle = true;
            Utility.DelayAction.Add(300, () => waitforjungle = false);
        }
        public static void AllClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range).FirstOrDefault();
            useClearItems(minion);
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;
                if (Menu.Item("useQClear").GetValue<bool>() && Q.IsReady())
                {
                    if (Q.Instance.Name == "BlindMonkQOne")
                    {
                            Q.Cast(minion, true);
                    }
                    else if ((minion.HasBuff("BlindMonkQOne", true) ||
                             minion.HasBuff("blindmonkqonechaos", true)) && ( Q.IsKillable(minion, 1)) ||
                             Player.Distance(minion) > 500) Q.Cast();
                }
                if (Menu.Item("useEClear").GetValue<bool>() && E.IsReady())
                {
                    if (E.Instance.Name == "BlindMonkEOne" && minion.IsValidTarget(E.Range) && !delayW)
                    {
                            E.Cast();
                            delayW = true;
                            Utility.DelayAction.Add(300, () => delayW = false);
                    }
                    else if (minion.HasBuff("BlindMonkEOne", true) && (Player.Distance(minion) > 450))
                    {
                        E.Cast();
                    }
                }
        }
        #endregion

        #region Wardjump
        public static void wardjumpToMouse()
        {
            WardJump(Game.CursorPos, paramBool("m2m"), false, false, paramBool("j2m"), paramBool("j2c"));
        }
        private static void WardJump(Vector3 pos, bool m2m = true, bool maxRange = false, bool reqinMaxRange = false, bool minions = true, bool champions = true)
        {
            var basePos = Player.Position.To2D();
            var newPos = (pos.To2D() - Player.Position.To2D());

            if (JumpPos == new Vector2())
            {
                if (reqinMaxRange) JumpPos = pos.To2D();
                else if (maxRange || Player.Distance(pos) > 590) JumpPos = basePos + (newPos.Normalized() * (590));
                else JumpPos = basePos + (newPos.Normalized()*(Player.Distance(pos)));
            }
            if (JumpPos != new Vector2() && reCheckWard)
            {
                reCheckWard = false;
                Utility.DelayAction.Add(20, () =>
                {
                    if (JumpPos != new Vector2())
                    {
                        JumpPos = new Vector2();
                        reCheckWard = true;
                    }
                });
            }
            if (m2m) Orbwalk(pos);
            if (!W.IsReady() || W.Instance.Name == "blindmonkwtwo" || reqinMaxRange && Player.Distance(pos) > W.Range) return;
            if (minions || champions)
            {
                if (champions)
                {
                    var champs =
                        (from champ in ObjectManager.Get<Obj_AI_Hero>()
                            where champ.IsAlly && champ.Distance(Player) < W.Range && champ.Distance(pos) < 200 && !champ.IsMe
                            select champ).ToList();
                    if (champs.Count > 0)
                    {
                        W.CastOnUnit(champs[0], true);
                        return;
                    }
                }
                if (minions)
                {
                    var minion2 =
                        (from minion in ObjectManager.Get<Obj_AI_Minion>()
                            where
                                minion.IsAlly && minion.Distance(Player) < W.Range && minion.Distance(pos) < 200 &&
                                !minion.Name.ToLower().Contains("ward")
                            select minion).ToList();
                    if (minion2.Count > 0)
                    {
                        W.CastOnUnit(minion2[0], true);
                        return;
                    }
                }
            }
            var isWard = false;
            foreach (var ward in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (ward.IsAlly && ward.Name.ToLower().Contains("ward") && ward.Distance(JumpPos) < 200)
                {
                    isWard = true;
                    W.CastOnUnit(ward, true);
                }
            }
            if (!isWard && CastWardAgain)
            {
                var ward = FindBestWardItem();
                if (ward == null) return;
                Player.Spellbook.CastSpell(ward.SpellSlot, JumpPos.To3D());
                CastWardAgain = false;
                lastWardPos = JumpPos.To3D();
                lastPlaced = Environment.TickCount;
                Utility.DelayAction.Add(500, () => CastWardAgain = true);
            }
        }

        //Thanks to xSallice the gumbo
        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;

            SpellDataInst sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return slot;
        }
        
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (Environment.TickCount < lastPlaced + 300)
            {
                var ward = (Obj_AI_Minion)sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(lastWardPos) < 500 && E.IsReady())
                {
                    W.Cast(ward);
                }
            }
        }
    #endregion

        #region Combo
        public static void wardCombo()
        {
            var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);

            Orbwalking.Orbwalk(target ?? null, Game.CursorPos, Menu.Item("ExtraWindup").GetValue<Slider>().Value, Menu.Item("HoldPosRadius").GetValue<Slider>().Value);

            if (target == null) return;
            useItems(target);
            if ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)))
            {
                if (CastQAgain || target.HasBuffOfType(BuffType.Knockup) && !Player.IsValidTarget(300) && !R.IsReady() || !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && !R.IsReady())
                {
                    Q.Cast();
                }
            }
            if (target.Distance(Player) > R.Range && target.Distance(Player) < R.Range + 580 && (target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)))
            {
                WardJump(target.Position, false);
            }
            if (E.IsReady() && E.Instance.Name == "BlindMonkEOne" && target.IsValidTarget(E.Range))
                E.Cast();

            if (E.IsReady() && E.Instance.Name != "BlindMonkEOne" &&
                !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                E.Cast();

            if (Q.IsReady() && Q.Instance.Name == "BlindMonkQOne")
                CastQ1(target);

            if (R.IsReady() && Q.IsReady() &&
                ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true))))
                R.CastOnUnit(target, packets());
        }
        public static void StarCombo()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);
            if (target == null) return;
            if ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)) && paramBool("useQ2"))
            {
                if (CastQAgain || target.HasBuffOfType(BuffType.Knockup) && !Player.IsValidTarget(300) && !R.IsReady() || !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) || Q.GetDamage(target, 1) > target.Health || returnQBuff().Distance(target) < Player.Distance(target) && !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                {
                    Q.Cast();
                }
            }
            useItems(target);
            if (R.GetDamage(target) >= target.Health && paramBool("ksR") && target.IsValidTarget()) R.Cast(target, packets());
            if (paramBool("aaStacks") && passiveStacks > 0 && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 100)) return;
            if (paramBool("useW"))
            {
                if (paramBool("wMode") && target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player))
                {
                    WardJump(target.Position, false, true);
                }
                if (!paramBool("wMode") && target.Distance(Player) > Q.Range)
                {
                    WardJump(target.Position, false, true);
                }
            }
            if (E.IsReady() && E.Instance.Name == "BlindMonkEOne" && target.IsValidTarget(E.Range) && paramBool("useE"))
            {
                E.Cast();
            }

            if (E.IsReady() && E.Instance.Name != "BlindMonkEOne" &&
                !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && paramBool("useE"))
            {
                E.Cast();
            }

            if (Q.IsReady() && Q.Instance.Name == "BlindMonkQOne" && paramBool("useQ"))
            {
                CastQ1(target);
            }

            if (R.IsReady() && Q.IsReady() &&
                ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true))) && paramBool("useR"))
                R.CastOnUnit(target, packets());
        }
        public static void CastQ1(Obj_AI_Hero target)
        {
            var Qpred = Q.GetPrediction(target);
            if ((Qpred.CollisionObjects.Where(a => a.IsValidTarget() && a.IsMinion).ToList().Count) == 1 && smiteSlot.IsReady() && paramBool("qSmite") && Qpred.CollisionObjects[0].IsValidTarget(780))
            {
                Player.Spellbook.CastSpell(smiteSlot, Qpred.CollisionObjects[0]);
                Utility.DelayAction.Add(Game.Ping/2, () => Q.Cast(Qpred.CastPosition, packets()));
            }
            else if(Qpred.CollisionObjects.Count == 0)
            {
                var minChance = GetHitChance(Menu.Item("QHC").GetValue<StringList>());
                Q.CastIfHitchanceEquals(target, minChance, true);
            }
        }

#endregion

        #region Utility
        //Start Credits to Kurisu
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        public static string smitetype()
        {
            if (SmiteBlue.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(a => Items.HasItem(a)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        //End credits

        public static float Q2Damage(Obj_AI_Base target, float subHP = 0, bool monster = false)
        {
            var damage = (50 + (Q.Level * 30)) + (0.09 * Player.FlatPhysicalDamageMod) + ((target.MaxHealth - (target.Health - subHP)) * 0.08);
            if (monster && damage > 400) return (float)Damage.CalcDamage(Player, target, Damage.DamageType.Physical, 400);
            return (float)Damage.CalcDamage(Player, target, Damage.DamageType.Physical, damage);
        }

        public static void PrintMessage(string msg) // Credits to ChewyMoon, and his Brain.exe
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>FALeeSin:</b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }
        public static void Orbwalk(Vector3 pos, Obj_AI_Hero target = null)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, pos);
        }
        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }
        public static bool packets()
        {
            return Menu.Item("NFE").GetValue<bool>();
        }

        public static Obj_AI_Base returnQBuff()
        {
            foreach (var unit in ObjectManager.Get<Obj_AI_Base>().Where(a => a.IsValidTarget(1300)))
            {
                if (unit.HasBuff("BlindMonkQOne", true) || unit.HasBuff("blindmonkqonechaos", true)) return unit;
            }
            return null;
        }

        public static void useItems(Obj_AI_Hero enemy)
        {
            if (Items.CanUseItem(3142) && Player.Distance(enemy) <= 600)
                Items.UseItem(3142);
            if (Items.CanUseItem(3144) && Player.Distance(enemy) <= 450)
                Items.UseItem(3144, enemy);
            if (Items.CanUseItem(3153) && Player.Distance(enemy) <= 450)
                Items.UseItem(3153, enemy);
            if (Items.CanUseItem(3077) && Utility.CountEnemiesInRange(350) >= 1)
                Items.UseItem(3077);
            if (Items.CanUseItem(3074) && Utility.CountEnemiesInRange(350) >= 1)
                Items.UseItem(3074);
            if (Items.CanUseItem(3143) && Utility.CountEnemiesInRange(450) >= 1)
                Items.UseItem(3143);
        }


        public static double SmiteDmg()
        {
            int[] dmg =
            {
                20*Player.Level + 370, 30*Player.Level + 330, 40*+Player.Level + 240, 50*Player.Level + 100
            };
            return Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready ? dmg.Max() : 0;
        }

        public static void useClearItems(Obj_AI_Base enemy)
        {
            if (Items.CanUseItem(3077) && Player.Distance(enemy) < 350)
                Items.UseItem(3077);
            if (Items.CanUseItem(3074) && Player.Distance(enemy) < 350)
                Items.UseItem(3074);
        }
        public static bool paramBool(String paramName)
        {
            return Menu.Item(paramName).GetValue<bool>();
        }

        public static HitChance GetHitChance(StringList stringList)
        {
            switch (stringList.SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        public static bool hpLowerParam(Obj_AI_Base obj, String paramName)
        {
            return ((obj.Health / obj.MaxHealth) * 100) <= Menu.Item(paramName).GetValue<Slider>().Value;
        }
        #endregion
    }
}