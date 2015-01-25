using System;
using System.Linq;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace VayneHunterRework
{
    class VayneHunterRework
    {
        public static Orbwalking.Orbwalker COrbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static String charName = "Vayne";
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Vector3 AfterCond = Vector3.Zero;
        public static AttackableUnit current; // for tower farming
        public static AttackableUnit last; // for tower farming
        private static float LastMoveC;
        private static bool aLInit;
        private static int[] QWE =  { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
        private static int[] QEW =  { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
        private static int[] WQE =  { 1, 3, 2, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
        private static int[] WEQ =  { 1, 3, 2, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1 };
        private static int[] EQW =  { 1, 3, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
        private static int[] EWQ =  { 1, 3, 2, 3, 3, 4, 3, 2, 3, 2, 4, 2, 2, 1, 1, 4, 1, 1 };

        private static StringList Orders = new StringList(new [] {"QWE","QEW","WQE","WEQ","EQW","EWQ"},2);

        public VayneHunterRework()
        {
            CustomEvents.Game.OnGameLoad +=Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != charName) return;
            Cleanser.CreateQSSSpellList();

            #region Menu
            Menu = new Menu("花边汉化-Vn猎人重做", "VHRework", true);
            var orbMenu = new Menu("走 砍", "orbwalker");
            COrbwalker = new Orbwalking.Orbwalker(orbMenu);
            Menu.AddSubMenu(orbMenu);
            var tsMenu = new Menu("目标 选择", "TargetSel");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            Menu.AddSubMenu(new Menu("连 招", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "使用 Q")).SetValue(true);
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "使用 E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "使用 R").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("QManaC", "停止 Q 丨MP<= %").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("EManaC", "停止 E 丨MP<= %").SetValue(new Slider(20, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("NEnUlt", "开启大招丨敌人数").SetValue(new Slider(2, 1, 5)));

            Menu.AddSubMenu(new Menu("骚 扰", "Harrass"));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "使用 Q")).SetValue(true);
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "使用 E").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("QManaH", "停止 Q 丨Mp<= %").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("EManaH", "停止 E 丨Mp<= %").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("打 钱", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLH", "使用 Q丨最后一下")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLC", "使用 Q丨清线")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLH", "停止 Q 最后一下丨Mp<= %").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLC", "停止 Q 清线丨Mp<= %").SetValue(new Slider(35, 1, 100)));

            var MiscSubMenu = new Menu("杂 项", "Misc");

            var MiscTSubMenu = new Menu("杂项-翻滚", "MiscT");
            {
                MiscTSubMenu.AddItem(new MenuItem("SmartQ", "先用QE连招").SetValue(false));
                MiscTSubMenu.AddItem(new MenuItem("NoQEn", "不向 敌方滚动").SetValue(true));
                MiscTSubMenu.AddItem(new MenuItem("NoAAStealth", "不隐身一段时间再平A").SetValue(false));
                MiscTSubMenu
                    .AddItem(
                        new MenuItem("WallTumble", "向墙边滚动").SetValue(new KeyBind("Y".ToCharArray()[0],
                            KeyBindType.Press)));
            }
            var MiscCSubMenu = new Menu("杂项 - 钉墙", "MiscC");
            {
                MiscCSubMenu.AddItem(new MenuItem("ENext", "下个 自动E").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                MiscCSubMenu.AddItem(new MenuItem("PushDistance", "E 推动 距离").SetValue(new Slider(425, 400, 500)));
                MiscCSubMenu.AddItem(new MenuItem("CondemnTurret", "尝试往塔下E").SetValue(false));
                MiscCSubMenu.AddItem(new MenuItem("CondemnFlag", "钉墙 to J4 标志").SetValue(true));
                MiscCSubMenu.AddItem(new MenuItem("AutoE", "自动 E").SetValue(false));
                MiscCSubMenu.AddItem(new MenuItem("AutoEKS", "智能 E 击杀").SetValue(true));
                MiscCSubMenu.AddItem(new MenuItem("NoEEnT", "No E Under enemy turret").SetValue(true));
            }
            var MiscGSubMenu = new Menu("杂项 - General", "MiscG");
            {

                MiscGSubMenu.AddItem(new MenuItem("Packets", "封 包").SetValue(true));
                MiscGSubMenu.AddItem(new MenuItem("AntiGP", "阻止 突进")).SetValue(true);
                MiscGSubMenu.AddItem(new MenuItem("Interrupt", "打断 法术").SetValue(true));
                MiscGSubMenu
                    .AddItem(new MenuItem("SpecialFocus", "Focus targets with 2 W marks").SetValue(false));
                MiscGSubMenu
                    .AddItem(
                        new MenuItem("ThreshLantern", "Grab Thresh Lantern").SetValue(new KeyBind("S".ToCharArray()[0],
                            KeyBindType.Press)));
                MiscGSubMenu.AddItem(new MenuItem("UseIgn", "使用 点燃")).SetValue(true);
            }
            MiscSubMenu.AddSubMenu(MiscTSubMenu);
            MiscSubMenu.AddSubMenu(MiscCSubMenu);
            MiscSubMenu.AddSubMenu(MiscGSubMenu);
            Menu.AddSubMenu(MiscSubMenu);

            Menu.AddSubMenu(new Menu("草丛 探测", "BushReveal"));
            //Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushReveal", "Bush Revealer").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushRevealer", "Trinket bush on condemn").SetValue(true));

            Menu.AddSubMenu(new Menu("物 品", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkC", "破败 连招").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkH", "破败 骚扰").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuC", "幽梦 连招").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuH", "幽梦 骚扰").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeC", "弯刀 连招").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeH", "弯刀 骚扰").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "使用破败丨自己血量").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "使用破败丨敌人血量").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("水银腰带", "QSSMenu"));
            Menu.SubMenu("QSSMenu").AddItem(new MenuItem("UseQSS", "使用 水银").SetValue(true));
            Menu.SubMenu("QSSMenu").AddItem(new MenuItem("QSSMinBuffs", "身上有多少个Debuff丨使用水银").SetValue(new Slider(2,1,5)));

            Menu.AddSubMenu(new Menu("解除状态", "QSST"));
            Cleanser.CreateTypeQSSMenu();
            Menu.AddSubMenu(new Menu("解除效果", "QSSSpell"));
            Cleanser.CreateQSSSpellMenu();
            Menu.AddSubMenu(new Menu("不E", "NoCondemn"));
            CreateNoCondemnMenu();

            Menu.AddSubMenu(new Menu("自动 嗑药", "AutoPot"));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APH", "磕 红").SetValue(true));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APM", "磕 蓝").SetValue(true));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APH_Slider", "Hp<= %丨使用药水").SetValue(new Slider(35,1)));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APM_Slider", "Mp<= %丨使用药水").SetValue(new Slider(35, 1)));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APHeal", "使用 治疗").SetValue(true));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APHeal_Slider", "Hp<= %丨使用治疗").SetValue(new Slider(35, 1)));

            Menu.AddSubMenu(new Menu("自动 加点", "AutoLevel"));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALSeq", "顺 序").SetValue(Orders));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALAct", "主 动").SetValue(false));

            Menu.AddSubMenu(new Menu("范 围", "Draw"));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawE", "E 范围").SetValue(new Circle(true,Color.MediumPurple)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawCond", "假如眩晕丨显示E位置").SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawDrake", "显示 Drake 点").SetValue(new Circle(true, Color.WhiteSmoke)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawMid", "显示 中点").SetValue(new Circle(true, Color.WhiteSmoke)));

            Menu.AddToMainMenu();
            #endregion

            Game.PrintChat("<font color='#FF0000'>VayneHunter</font> <font color='#FFFFFF'>Rework loaded!</font>");
            Game.PrintChat("By <font color='#FF0000'>DZ191</font><font color='#FFFFFF'>Special Thanks to: Kurisuu & KonoeChan</font>涓ㄦ眽鍖朆y 鑺辫竟");
            Game.PrintChat("If you like my assemblies feel free to donate me (link on the forum :) )");

           //Cleanser.cleanUselessSpells();
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E,550f);
            R = new Spell(SpellSlot.R);
            E.SetTargetted(0.25f,1600f);
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;
            Game.OnGameUpdate += Game_OnGameUpdate;

           // Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += Cleanser.OnCreateObj;
            GameObject.OnDelete += Cleanser.OnDeleteObj;
            Menu.Item("ALAct").ValueChanged += AutoLevel_ValueChanged;

            #region AutoLeveler

            if (isMenuEnabled("ALAct") && !aLInit)
            {
                var AutoLevel_I =
                    new AutoLevel(
                        getSequence(
                            Menu.Item("ALSeq").GetValue<StringList>().SList[
                                Menu.Item("ALSeq").GetValue<StringList>().SelectedIndex]));
                aLInit = true;
            }
        }

        private void AutoLevel_ValueChanged(object sender, OnValueChangeEventArgs ev)
        {
            if (isMenuEnabled("ALAct") && !aLInit)
            {
                var AutoLevel_I =
                    new AutoLevel(
                        getSequence(
                            Menu.Item("ALSeq").GetValue<StringList>().SList[
                                Menu.Item("ALSeq").GetValue<StringList>().SelectedIndex]));
                aLInit = true;
            }

            AutoLevel.Enabled(ev.GetNewValue<bool>());
        }

            #endregion


        private void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                 AfterAA(target);
            }
        }

        #region Drawing
        void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var DrawE = Menu.Item("DrawE").GetValue<Circle>();
            var DrawCond = Menu.Item("DrawCond").GetValue<Circle>();
            var DrawDrake = Menu.Item("DrawDrake").GetValue<Circle>();
            var DrawMid = Menu.Item("DrawMid").GetValue<Circle>();
            Vector2 MidWallQPos = new Vector2(6707.485f, 8802.744f);
            Vector2 DrakeWallQPos = new Vector2(11514, 4462);
            if (DrawDrake.Active && Player.Distance(DrakeWallQPos) < 1500f && isSummonersRift()) Utility.DrawCircle(new Vector3(12052, 4826, 0f), 75f, DrawDrake.Color);
            if (DrawMid.Active && Player.Distance(MidWallQPos) < 1500f && isSummonersRift()) Utility.DrawCircle(new Vector3(6958, 8944, 0f), 75f, DrawMid.Color);
            if (DrawE.Active) Utility.DrawCircle(Player.Position, E.Range, DrawE.Color);
            if (DrawCond.Active) DrawPostCondemn();

        }
        #endregion

        void AfterAA(AttackableUnit target)
        {
            if (!(target is Obj_AI_Hero)) return;
            var tar = (Obj_AI_Hero)target;

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    if (isMenuEnabled("UseQC")) SmartQCheck(tar);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isMenuEnabled("UseQH")) SmartQCheck(tar);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:

                default:
                    break;
            }

            ENextAuto(tar);
            UseItems(tar);
        }


        void Game_OnGameUpdate(EventArgs args)
        {
            //Cleanser.enableCheck();
            if (Player.IsDead) return;
            Obj_AI_Hero tar;

            if (isMenuEnabled("AutoE") && CondemnCheck(Player.Position, out tar)) { CastE(tar,true);}
            if (Menu.Item("WallTumble").GetValue<KeyBind>().Active) WallTumble();
            if (Menu.Item("ThreshLantern").GetValue<KeyBind>().Active) takeLantern();
            QFarmCheck();
            FocusTarget();
            NoAAStealth();
            EKs();

            AutoPot();

            //Cleanser
            Cleanser.cleanserBySpell();
            Cleanser.cleanserByBuffType();

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Obj_AI_Hero tar2;
                    if (isMenuEnabled("UseEC") && CondemnCheck(Player.ServerPosition, out tar2)) { CastE(tar2);}
                    useIgnite();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Obj_AI_Hero tar3;
                    if (isMenuEnabled("UseEH") && CondemnCheck(Player.ServerPosition, out tar3)) { CastE(tar3); }
                    break;
            }  

            
        }

        #region Miscellaneous

        private void ENextAuto(Obj_AI_Hero tar)
        {
            if (!E.IsReady() || !tar.IsValid || !Menu.Item("ENext").GetValue<KeyBind>().Active) return;
            CastE(tar, true);
            Menu.Item("ENext").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle, false));
        }

        void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var GPSender = (Obj_AI_Hero)gapcloser.Sender;
            if (!isMenuEnabled("AntiGP") || !E.IsReady() || !GPSender.IsValidTarget()) return;
            CastE(GPSender, true);

        }

        void Interrupter_OnPossibleToInterrupt(AttackableUnit unit, InterruptableSpell spell)
        {
            var Sender = (Obj_AI_Hero)unit;
            if (!isMenuEnabled("Interrupt") || !E.IsReady() || !Sender.IsValidTarget()) return;
            CastE(Sender,true);
        }

        bool CondemnCheck(Vector3 Position, out Obj_AI_Hero target)
        {
            if (isUnderEnTurret(Player.Position) && isMenuEnabled("NoEEnT"))
            {
                target = null;
                return false;
            }
            foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && !isMenuEnabled("nC"+hero.ChampionName) && hero.Distance(Position)<=E.Range))
            {
                var EPred = E.GetPrediction(En);
                int pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                var FinalPosition = EPred.UnitPosition.To2D().Extend(Position.To2D(), -pushDist).To3D();
                for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                {
                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Position.To2D(), -i).To3D();
                    var OrTurret = isMenuEnabled("CondemnTurret") && isUnderTurret(FinalPosition);
                    var OrFlag = isMenuEnabled("CondemnFlag") && isJ4FlagThere(loc3, En);
                    var OrFountain = isMenuEnabled("CondemnTurret") && isFountain(FinalPosition);
                    AfterCond = loc3;
                    if (isWall(loc3) || OrTurret || OrFlag || OrFountain)
                    {
                        if(isMenuEnabled("BushRevealer"))CheckAndWard(Position,loc3,En);
                        target = En;
                        return true; 
                    }
                }
            }
            target = null;
            return false;
            
        }

        void QFarmCheck()
        {
            if (!Q.IsReady()) return;
            var PosAfterQ = Player.Position.To2D().Extend(Game.CursorPos.To2D(), 300);
            var minList =
                MinionManager.GetMinions(Player.Position, 550f).Where(min =>
                    HealthPrediction.GetHealthPrediction(min,(int)(Q.Delay + min.Distance(PosAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000)+(Game.Ping/2) <= (Q.GetDamage(min)+Player.GetAutoAttackDamage(min))
                    && HealthPrediction.GetHealthPrediction(min, (int)(Q.Delay + min.Distance(PosAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000) + (Game.Ping / 2) > Player.GetAutoAttackDamage(min)); //Player.GetAutoAttackDamage(min)
           
            if (!minList.Any()) return;
            CastQ(Vector3.Zero,minList.First());
        }

        
        void NoAAStealth()
        {
            var mb = (isMenuEnabled("NoAAStealth") && Player.HasBuff("vaynetumblefade", true))?false:true;
            COrbwalker.SetAttack(mb);
        }

        void FocusTarget()
        {
            if (!isMenuEnabled("SpecialFocus")) return;
            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null))))
            {
                if (has2WStacks(hero))
                {
                    COrbwalker.ForceTarget(hero);
                    Hud.SelectedUnit = hero;
                    return;
                }       
             }
         }
            
        


        int[] getSequence(String Order)
        {
            switch (Order)
            {
                case "QWE":
                    return QWE;
                case "QEW":
                    return QEW;
                case "WQE":
                    return WQE;
                case "EQW":
                    return EQW;
                case "WEQ":
                    return WEQ;
                case "EWQ":
                    return EWQ;
                default:
                    return null;
            }
        }

        private static void CreateNoCondemnMenu()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                Menu.SubMenu("NoCondemn").AddItem(new MenuItem("nC" + hero.ChampionName, hero.ChampionName).SetValue(false));
            }
        }

        void CheckAndWard(Vector3 sPos, Vector3 EndPosition, Obj_AI_Hero target)
        {
            if (isGrass(EndPosition))
            {
                var WardSlot = FindBestWardItem();
                if (WardSlot == null) return;
                for (int i = 1; i < Vector3.Distance(sPos, EndPosition); i += (int)target.BoundingRadius)
                {
                    var v = sPos.To2D().Extend(EndPosition.To2D(), i).To3D();
                    if (isGrass(v))
                    {
                        //WardSlot.UseItem(v);
                        Player.Spellbook.CastSpell(WardSlot.SpellSlot, v);
                        return;
                    }
                }
            }
        }

        void DrawPostCondemn()
        {
            var DrawCond = Menu.Item("DrawCond").GetValue<Circle>();
            foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && !isMenuEnabled("nC" + hero.ChampionName) && hero.Distance(Player.Position) <= E.Range))
            {
                var EPred = E.GetPrediction(En);
                int pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                {
                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();
                    if (isWall(loc3)) Utility.DrawCircle(loc3, 100f, DrawCond.Color);

                }
            }
        }

        void takeLantern()
        {
            /**
            foreach (GameObject obj in ObjectManager.Get<GameObject>())
            {
                if (obj.Name.Contains("ThreshLantern") && obj.Position.Distance(ObjectManager.Player.ServerPosition) <= 500 && obj.IsAlly)
                {
                    var InteractPKT = new PKT_InteractReq
                    {
                        NetworkId = Player.NetworkId,
                        TargetNetworkId = obj.NetworkId
                    };
                    //Credits to Trees
                    Game.SendPacket(InteractPKT.Encode(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                    return;
                }
            }
             * */
        }

        void WallTumble()
        {
            if (!isSummonersRift()) return;
            Vector2 MidWallQPos = new Vector2(6707.485f, 8802.744f);
            Vector2 DrakeWallQPos = new Vector2(11514, 4462);
            if (Player.Distance(MidWallQPos) >= Player.Distance(DrakeWallQPos))
            {

                if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                    Player.Position.Y > 4872)
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                    Q.Cast(DrakeWallQPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6908 || Player.Position.X > 6978 || Player.Position.Y < 8917 ||
                    Player.Position.Y > 8989)
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                    Q.Cast(MidWallQPos, true);
                }
            }
        }

        void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }
            LastMoveC = Environment.TickCount;
            Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        #endregion

        #region Q Region
        void SmartQCheck(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !target.IsValidTarget()) return;
            if (!isMenuEnabled("SmartQ") || !E.IsReady())
            {
                CastQ(Game.CursorPos,target);
            }
            else
            {
                for (int I = 0; I <= 360; I += 65)
                {
                    var F1 = new Vector2(Player.Position.X + (float)(300 * Math.Cos(I * (Math.PI / 180))), Player.Position.Y + (float)(300 * Math.Sin(I * (Math.PI / 180)))).To3D();
                   // var FinalPos = Player.Position.To2D().Extend(F1, 300).To3D();
                    Obj_AI_Hero targ;
                    if (CondemnCheck(F1, out targ))
                    {
                        CastTumble(F1,target);
                        CastE(target);
                        return;
                    }
                }
                CastQ(Game.CursorPos, target);
            }
        }

        void CastQ(Vector3 Pos,Obj_AI_Base target,bool customPos=false)
        {
           if (!Q.IsReady() || !target.IsValidTarget()) return;
           
            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var ManaC = Menu.Item("QManaC").GetValue<Slider>().Value;
                    var EnMin = Menu.Item("NEnUlt").GetValue<Slider>().Value;
                    var EnemiesList =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(h => h.IsValid && !h.IsDead && h.Distance(Player.Position) <= 900 && h.IsEnemy).ToList();
                    if (getPerValue(true) >= ManaC && isMenuEnabled("UseQC"))
                    {
                        if(isMenuEnabled("UseRC") && R.IsReady() && EnemiesList.Count >= EnMin)R.CastOnUnit(Player);
                        if(!customPos){CastTumble(target);}else{CastTumble(Pos,target);}
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var ManaH = Menu.Item("QManaH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaH && isMenuEnabled("UseQH")){ if (!customPos){ CastTumble(target);} else{ CastTumble(Pos, target);}}
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    var ManaLH = Menu.Item("QManaLH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaLH && isMenuEnabled("UseQLH")) { if (!customPos) { CastTumble(target); } else { CastTumble(Pos, target); } }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var ManaLC = Menu.Item("QManaLC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaLC && isMenuEnabled("UseQLC")){ if (!customPos){CastTumble(target); }else{ CastTumble(Pos, target);}}
                    break;
                default:
                    break;
            }
        }
        Vector3 getQVectorMelee()
        {
            if (isMenuEnabled("SmartQ") || !isMenuEnabled("SpecialQMelee"))
            {
                return Player.Position.Extend(Game.CursorPos, Q.Range);
            }

            var Position = Game.CursorPos;
            //Standard Q end position
            var Extended = Player.Position.Extend(Position, Q.Range);
            //Check for Melee enemies in that range
            var HeroesThere =
                ObjectManager.Get<Obj_AI_Hero>().Where(h => h.Distance(Extended) <= 375f && h.IsValidTarget() && h.IsMelee()).OrderBy(h => h.Distance(Player)).ToList();
            //If the count is 0 return the normal position
            if (HeroesThere.Count == 0)
            {
                return Extended;
            }
            //Find the closest hero
            var theHero = HeroesThere.First();
            //Extend the V3 of the hero radius to my player. Not used atm.
            var HeroRadius = theHero.Position.Extend(Player.Position, theHero.AttackRange);
            //Intersection.. Not used atm.
            var Intersection = Geometry.Intersection(theHero.Position.To2D(), HeroRadius.To2D(), Player.Position.To2D(), Extended.To2D());
            //Start angle
            double Angle = 0;
            //Step angle
            var step = 10;
            //The new extended variable
            var newExtended = Extended;
            //While the heroes near the new position are > 0 or the angle <= 180
            while (
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.Distance(newExtended) <= 375f && h.IsValidTarget() && h.IsMelee())
                    .ToList()
                    .Count > 0 || Angle <= 45)
            {
                //Augment the angle by step
                Angle += step;
                //Second angle
                var Angle2 = -Angle;
                //Find the new extended position
                newExtended = new Vector3(
                    Extended.X * (float)Math.Cos(Geometry.DegreeToRadian(Angle)),
                    Extended.Y * (float)Math.Sin(Geometry.DegreeToRadian(Angle)), Extended.Z);

                //Move in one direction first. Then check:
                //Are enemies still there?
                //If they are then try the other direction
                //If not the while cycle will just end.
                if (
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.Distance(newExtended) <= 375f && h.IsValidTarget() && h.IsMelee())
                        .ToList()
                        .Count > 0)
                {
                    newExtended = new Vector3(
                    Extended.X * (float)Math.Cos(Geometry.DegreeToRadian(Angle2)),
                    Extended.Y * (float)Math.Sin(Geometry.DegreeToRadian(Angle2)), Extended.Z);
                }
            }
            //return the end position
            return newExtended;
        }

        void CastTumble(Obj_AI_Base target)
        {
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550*550 && distanceAfterTumble > 100*100)
            {
                if (getEnemiesInRange(posAfterTumble, 500f) >= 3 && getAlliesInRange(posAfterTumble, 410f) < 3 && isMenuEnabled("NoQEn")) return;
                Q.Cast(Game.CursorPos, isMenuEnabled("Packets"));
            }
        }
        void CastTumble(Vector3 Pos,Obj_AI_Base target)
        {
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Pos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550*550 && distanceAfterTumble > 100*100)
            {
                if (getEnemiesInRange(posAfterTumble, 500f) >= 3 && getAlliesInRange(posAfterTumble, 410f) < 3 && isMenuEnabled("NoQEn")) return;
                Q.Cast(Pos, isMenuEnabled("Packets"));
            }
        }
        #endregion

        #region E Region
        void CastE(Obj_AI_Hero target, bool isForGapcloser = false)
        {
            if (!E.IsReady() || !target.IsValidTarget()) return;
            if (isForGapcloser)
            {
                E.Cast(target, isMenuEnabled("Packets"));
                AfterCond = Vector3.Zero;
                return;
            }
            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var ManaC = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if (isMenuEnabled("UseEC") && getPerValue(true) >= ManaC)
                    {
                        E.Cast(target, isMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var ManaH = Menu.Item("EManaH").GetValue<Slider>().Value;
                    if (isMenuEnabled("UseEH") && getPerValue(true) >= ManaH)
                    {
                        E.Cast(target, isMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                default:
                    break;
            }
        }

        void EKs()
        {
            if (Q.IsReady() || Player.CanAttack || !isMenuEnabled("AutoEKS"))
                return;
            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.Distance(Player) > E.Range - 120 && h.IsValidTarget())
                )
            {
                if (has2WStacks(hero))
                {
                    if (Player.GetSpellDamage(hero, SpellSlot.W) + Player.GetSpellDamage(hero, SpellSlot.E) >=
                        HealthPrediction.GetHealthPrediction(hero, (int) (Player.Distance(hero) / E.Speed) * 1000))
                    {
                        E.Cast(hero, isMenuEnabled("Packets"));
                        return;
                    }
                }
            }
        }
        #endregion
        
        #region Items
        void UseItems(Obj_AI_Hero tar)
        {
            var ownH = getPerValue(false);
            if ((Menu.Item("BotrkC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar,false))))
            {
                UseItem(3153, tar);
            }
            if ((Menu.Item("BotrkH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
               ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }
            if (Menu.Item("YoumuuC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3142);
            }
            if (Menu.Item("YoumuuH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3142);
            }
            if (Menu.Item("BilgeC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3144,tar);
            }
            if (Menu.Item("BilgeH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3144, tar);
            }
        }

        void useIgnite()
        {
            //Ignite
            var dmg = 50 + 20 * Player.Level;
            var tg = TargetSelector.GetSelectedTarget();
            var ign = Player.GetSpellSlot("summonerdot");
            if (isMenuEnabled("UseIgn") && tg.IsValidTarget() && dmg > tg.Health)
            {
                if (ign != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(ign) == SpellState.Ready)
                {
                    Player.Spellbook.CastSpell(ign, tg);
                }
            }
        }

        private void AutoPot()
        {
            if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop())
                return;

            //Health Pots
            if (isMenuEnabled("APH") && getPerValue(false) <= Menu.Item("APH_Slider").GetValue<Slider>().Value && !Player.HasBuff("RegenerationPotion", true))
            {
                UseItem(2003);
            }
            //Mana Pots
            if (isMenuEnabled("APM") && getPerValue(true) <= Menu.Item("APM_Slider").GetValue<Slider>().Value && !Player.HasBuff("FlaskOfCrystalWater", true))
            {
                UseItem(2004);
            }
            //Summoner Heal
            if (isMenuEnabled("APHeal") && getPerValue(false) <= Menu.Item("APHeal_Slider").GetValue<Slider>().Value)
            {
                var heal = Player.GetSpellSlot("summonerheal");
                if (heal != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(heal) == SpellState.Ready)
                {
                    Player.Spellbook.CastSpell(heal);
                }
            }
        }




        #endregion

        #region Utility Methods

        bool isSummonersRift()
        {
          return true;
        }

        bool has2WStacks(Obj_AI_Hero target)
        {
            return target.Buffs.Any(bu => bu.Name == "vaynesilvereddebuff" && bu.Count == 2);
        }
        int getEnemiesInRange(Vector3 point, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsEnemy && !h.IsDead && h.IsValid && h.Distance(point) <= range).ToList().Count;
        }
        int getAlliesInRange(Vector3 point, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsAlly && !h.IsDead && h.IsValid && h.Distance(point) <= range).ToList().Count;
        }

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }

        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            SpellDataInst sdi = GetItemSpell(slot);
            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)return slot;
            return null;
        }
        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        bool isWall(Vector3 Pos)
        {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(Pos);
            return (cFlags == CollisionFlags.Wall);
        }
        bool isUnderTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsAlly && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        bool isUnderEnTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        public static bool isMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }
        float getPerValue(bool mana)
        {
            if (mana) return (Player.Mana / Player.MaxMana) * 100;
            return (Player.Health / Player.MaxHealth) * 100;
        }
        float getPerValueTarget(Obj_AI_Hero target, bool mana)
        {
            if (mana) return (target.Mana / target.MaxMana) * 100;
            return (target.Health / target.MaxHealth) * 100;
        }
        bool isGrass(Vector3 Pos)
        {
            return NavMesh.IsWallOfGrass(Pos,65);
            //return false; 
        }

        bool isJ4FlagThere(Vector3 Position,Obj_AI_Hero target)
        {
            return ObjectManager.Get<Obj_AI_Base>().Any(m => m.Distance(Position) <= target.BoundingRadius && m.Name == "Beacon");
        }

        bool isFountain(Vector3 Position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>()
                    .Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly)
                    .Any(
                        spawnPoint =>
                            Vector2.Distance(Position.To2D(), spawnPoint.Position.To2D()) <
                            fountainRange);
        }

        #endregion

    }
}
