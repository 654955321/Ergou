using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace LookaJayce
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Orbwalking.Orbwalker Orbwalker;
        private static HpBarIndicator hpi = new HpBarIndicator();
        private static Spell Qcharge, Qcannon, Wcannon, Ecannon, Qhammer, Whammer, Ehammer, Rswitch;
        private static SpellDataInst Qdata, Edata;
        private static Items.Item Muramana;
        private static bool isHammer;
        private static Menu Menu;

        //Cooldowns
        public static float[] QcannonTrueCD = { 8, 8, 8, 8, 8 };
        public static float[] WcannonTrueCD = { 14, 12, 10, 8, 6 };
        public static float[] EcannonTrueCD = { 16, 16, 16, 16, 16 };
        public static float[] QhammerTrueCD = { 16, 14, 12, 10, 8 };
        public static float[] WhammerTrueCD = { 10, 10, 10, 10, 10 };
        public static float[] EhammerTrueCD = { 14, 12, 12, 11, 10 };

        private static float QcannonCD, WcannonCD, EcannonCD;
        private static float QhammerCD, WhammerCD, EhammerCD;
        private static float QcannonCDrem, WcannonCDrem, EcannonCDrem;
        private static float QhammerCDrem, WhammerCDrem, EhammerCDrem;

        private static Obj_AI_Base mob;
        private static string[] MinionNames = 
        {
            "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", 
            "SRU_Red", "SRU_Krug", "SRU_Dragon", "Sru_Crab", "SRU_Baron"
        };

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Jayce")
            {
                Notifications.AddNotification("Not Using Jayce :(", 3000);
                return;
            }

            Qcharge = new Spell(SpellSlot.Q, 1600f);
            Qcannon = new Spell(SpellSlot.Q, 1050f);
            Wcannon = new Spell(SpellSlot.W, 0f);
            Ecannon = new Spell(SpellSlot.E, 650f);
            Qhammer = new Spell(SpellSlot.Q, 600f);
            Whammer = new Spell(SpellSlot.W, 285f);
            Ehammer = new Spell(SpellSlot.E, 270f);
            Rswitch = new Spell(SpellSlot.R, 0f);

            Qcannon.SetSkillshot(0.250f, 70f, 1450f, true, SkillshotType.SkillshotLine);
            Qcharge.SetSkillshot(0.250f, 70f, 2350f, true, SkillshotType.SkillshotLine);

            Qdata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q);
            Edata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E);

            Muramana = new Items.Item(3042, int.MaxValue);

            Menu = new Menu("Looka杰斯", "Jayce", true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = Menu.AddSubMenu(new Menu("目标选择", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            //Keys Menu
            Menu keys = new Menu("键位", "Keys");
            keys.AddItem(new MenuItem("QuickScope", "鼠标方向EQ", true)).SetValue(new KeyBind('T', KeyBindType.Press));
            keys.AddItem(new MenuItem("ToggleHarass", "自动骚扰", true)).SetValue(new KeyBind('N', KeyBindType.Toggle));
            keys.AddItem(new MenuItem("StackTear", "堆叠女神之泪", true)).SetValue(new KeyBind('J', KeyBindType.Toggle));
            keys.AddItem(new MenuItem("Flee", "战术撤退（逃跑）", true)).SetValue(new KeyBind('A', KeyBindType.Press));
            keys.AddItem(new MenuItem("Insec", "连招", true)).SetValue(new KeyBind('G', KeyBindType.Press));
            keys.AddItem(new MenuItem("Exploit", "利用bug", true)).SetValue(new KeyBind('Y', KeyBindType.Press));
            Menu.AddSubMenu(keys);

            //Combo Menu
            Menu combo = new Menu("连招", "Combo");
            combo.AddItem(new MenuItem("SwitchForm", "开关形式", true)).SetValue(true);
            combo.AddItem(new MenuItem("UseBug", "使用 Bug", true)).SetValue(true);
            Menu.AddSubMenu(combo);

            //Misc Menu
            Menu misc = new Menu("杂项", "Misc");
            misc.AddItem(new MenuItem("FlashInsec", "闪现连招", true).SetValue(true));
            misc.AddItem(new MenuItem("KillSteal", "抢人头模式", true).SetValue(true));
            keys.AddItem(new MenuItem("JungleSteal", "打野模式", true).SetValue(true));
            misc.AddItem(new MenuItem("ForceGate", "强制 E?", true).SetValue(false));
            misc.AddItem(new MenuItem("GateDistance", "E 施放距离", true)).SetValue(new Slider(10, 5, 60));
            misc.AddItem(new MenuItem("AntiGapcloser", "防止突进", true).SetValue(true));
            misc.AddItem(new MenuItem("Interrupt", "中断法术", true).SetValue(true));
            Menu.AddSubMenu(misc);

            //Drawings Menu
            Menu drawMenu = new Menu("显示", "Drawings");
            drawMenu.AddItem(new MenuItem("DisableDrawing", "禁用所以范围", true).SetValue(false));
            drawMenu.AddItem(new MenuItem("Qcharge", "EQ 范围", true).SetValue(true));
            drawMenu.AddItem(new MenuItem("Qcannon", "Q 炮范围", true).SetValue(true));
            drawMenu.AddItem(new MenuItem("Ecannon", "E 炮范围", true).SetValue(false));
            drawMenu.AddItem(new MenuItem("Qhammer", "Q 锤范围", true).SetValue(true));
            drawMenu.AddItem(new MenuItem("Ehammer", "E 锤范围", true).SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawDmg", "显示 连招伤害", true).SetValue(true));
            drawMenu.AddItem(new MenuItem("Drawcds", "显示 冷却时间", true).SetValue(true));
            Menu.AddSubMenu(drawMenu);

            //Camps Menu
            Menu campsMenu = new Menu("EQ抢野怪", "Camps");
            campsMenu.AddItem(new MenuItem("SRU_Baron", "男爵").SetValue(true));
            campsMenu.AddItem(new MenuItem("SRU_Dragon", "小龙").SetValue(true));
            campsMenu.AddItem(new MenuItem("SRU_Blue", "蓝buff").SetValue(true));
            campsMenu.AddItem(new MenuItem("SRU_Red", "红buff").SetValue(true));
            campsMenu.AddItem(new MenuItem("SRU_Gromp", "石头人").SetValue(false));
            campsMenu.AddItem(new MenuItem("SRU_Murkwolf", "三狼").SetValue(false));
            campsMenu.AddItem(new MenuItem("SRU_Krug", "四小鸡").SetValue(false));
            campsMenu.AddItem(new MenuItem("SRU_Razorbeak", "蛤蟆").SetValue(false));
            campsMenu.AddItem(new MenuItem("Sru_Crab", "螃蟹").SetValue(false));
            Menu.AddSubMenu(campsMenu);

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += InterrupterOnPossibleToInterrupt;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Notifications.AddNotification("LookaJayce 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847 !", 3000);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            ProcessCDs();
            isHammer = !Qdata.Name.Contains("jayceshockblast"); //Update Form

            if (Menu.Item("KillSteal", true).GetValue<bool>())
            {
                KillSteal();
            }

            if (Menu.Item("JungleSteal", true).GetValue<bool>())
            {
                JungleSteal();
            }

            if (Menu.Item("QuickScope", true).GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (isHammer && Rswitch.IsReady()) Rswitch.Cast();
                if (!isHammer && Ecannon.IsReady() && Qcannon.IsReady())
                {
                    var gateVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 10 * Menu.Item("GateDistance", true).GetValue<Slider>().Value;
                    Qcannon.Cast(Game.CursorPos);
                    Ecannon.Cast(gateVector);
                }
            }

            if (Menu.Item("Flee", true).GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (!isHammer && Ecannon.IsReady())
                {
                    Ecannon.Cast(Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 100);
                }
                if (Rswitch.IsReady()) Rswitch.Cast();
            }

            if (Menu.Item("StackTear", true).GetValue<KeyBind>().Active)
            {
                if (Whammer.IsReady()) Whammer.Cast();
                if (!isHammer && Ecannon.IsReady())
                {
                    Ecannon.Cast(Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 100);
                }
                if (Rswitch.IsReady()) Rswitch.Cast();
            }

            if (Menu.Item("Insec", true).GetValue<KeyBind>().Active)
            {
                Insec(TargetSelector.GetTarget(Qcharge.Range, TargetSelector.DamageType.Physical));
            }

            if (Menu.Item("Exploit", true).GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                castBug(TargetSelector.GetTarget(Ecannon.Range, TargetSelector.DamageType.Physical));
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    //LaneClear();
                    break;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Qcharge.Range, TargetSelector.DamageType.Physical);
            var QcannonPred = Qcannon.GetPrediction(target);

            if (!isHammer)
            {
                CastQE(target);

                if (Qcannon.GetPrediction(target).Hitchance >= HitChance.High && Qcannon.IsReady())
                {
                    Qcannon.Cast(target);
                }
                
                if (Player.Distance(target.Position) <= 490 && Wcannon.IsReady())
                {
                    //Activate muramana
                    //Activate ghostblade
                    Wcannon.Cast();
                }

                //No abilities left and in range to hammer Q?
                if (Menu.Item("SwitchForm", true).GetValue<bool>() && Player.Distance(target.Position) <= Qhammer.Range && QhammerCD <= 0.5 &&!Qcannon.IsReady() && !Wcannon.IsReady() && Rswitch.IsReady())
                {
                    Rswitch.Cast();
                }
            }

            if (isHammer)
            {
                if (Player.Distance(target.Position) <= Qhammer.Range && Qhammer.IsReady())
                {
                    Qhammer.Cast(target);
                }

                if (Player.Distance(target.Position) <= Whammer.Range && Whammer.IsReady())
                {
                    Whammer.Cast();
                }
                
                //No abilities left?
                if (Menu.Item("SwitchForm", true).GetValue<bool>() && !Qhammer.IsReady() && !Whammer.IsReady() && Rswitch.IsReady())
                {
                    Rswitch.Cast();
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Qcharge.Range, TargetSelector.DamageType.Physical);

            if (!isHammer)
            {
                CastQE(target);

                //Try Q
                if (Qcannon.GetPrediction(target).Hitchance >= HitChance.High && Qcannon.IsReady())
                {
                    Qcannon.Cast(target);
                }
            }
        }

        private static void KillSteal()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Qcharge.Range) && x.IsEnemy && !x.IsDead))
            {
                //Try QE
                if ((Player.GetSpellDamage(target, SpellSlot.Q) * 1.4 - 20) > target.Health)
                {
                    if (isHammer && Rswitch.IsReady()) Rswitch.Cast();
                    if (!isHammer)
                    {
                        CastQE(target);
                    }
                }
                
                //Try Qcannon
                if ((Player.GetSpellDamage(target, SpellSlot.Q) - 20) > target.Health && Qcannon.GetPrediction(target).Hitchance >= HitChance.High && Qcannon.IsReady())
                {
                    if (isHammer && Rswitch.IsReady()) Rswitch.Cast();
                    if (!isHammer)
                    {
                        Qcannon.Cast(target);
                    }
                }

                if (Player.Distance(target.ServerPosition) <= Qhammer.Range && Qhammer.IsReady())
                {
                    //try hammer QE
                    if ((Player.GetSpellDamage(target, SpellSlot.Q, 1) + Player.GetSpellDamage(target, SpellSlot.E) - 20) > target.Health && Ehammer.IsReady())
                    {
                        if (!isHammer && Rswitch.IsReady()) Rswitch.Cast();
                        if (isHammer)
                        {
                            Qhammer.Cast(target);
                            Ehammer.Cast(target);
                        }
                    }
                    //try hammer Q
                    else if ((Player.GetSpellDamage(target, SpellSlot.Q, 1) - 20) > target.Health)
                    {
                        if (!isHammer && Rswitch.IsReady()) Rswitch.Cast();
                        if (isHammer)
                        {
                            Qhammer.Cast(target);
                        }
                    }
                }
                //try hammer E
                if (Player.Distance(target.ServerPosition) <= Ehammer.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && Ehammer.IsReady())
                {
                    if (!isHammer && Rswitch.IsReady()) Rswitch.Cast();
                    if (isHammer)
                    {
                        Ehammer.Cast(target);
                    }
                }
            }
        }

        private static void JungleSteal()
        {
            mob = GetNearest(ObjectManager.Player.ServerPosition);
            if (mob != null && Menu.Item(mob.BaseSkinName).GetValue<bool>())
            {
                var healthPred = HealthPrediction.GetHealthPrediction(mob, (int)Qcharge.Delay);

                if ((Player.GetSpellDamage(mob, SpellSlot.Q) * 1.4 - 20) > healthPred)
                {
                    CastQE(mob);
                }
            }
        }

        private static void Insec(Obj_AI_Hero target)
        {
            if (EhammerCD == 0)
            {
                if (!isHammer && Rswitch.IsReady() && Player.Distance(target.Position) <= Qhammer.Range)
                {
                    Rswitch.Cast();
                }

                Vector3 insecPos = Game.CursorPos.Extend(target.Position, Game.CursorPos.Distance(target.Position) + 200);
                Player.IssueOrder(GameObjectOrder.MoveTo, insecPos);

                if (Menu.Item("FlashInsec", true).GetValue<bool>() && Player.Distance(insecPos) <= 400 && Player.Distance(insecPos) >= 150)
                {
                    Player.Spellbook.CastSpell(Player.GetSpellSlot("SummonerFlash"), insecPos);
                }
                if (Player.Distance(insecPos) < 130)
                {
                    Ehammer.Cast(target);
                    return;
                }
                if (Player.Distance(target.Position) <= Qhammer.Range && Qhammer.IsReady())
                {
                    Qhammer.Cast(target);
                }
            }
        }

        

        //HELPER METHODS

        private static Vector3 GateVector(Obj_AI_Base target)
        {
            return Player.Position + Vector3.Normalize(target.ServerPosition - Player.Position) * 10 * Menu.Item("GateDistance", true).GetValue<Slider>().Value;
        }

        private static void CastQE(Obj_AI_Base target)
        {
            if ((Qdata.ManaCost + Edata.ManaCost) <= Player.Mana && QcannonCD == 0 && EcannonCD == 0)
            {
                if (Player.Distance(target.Position) <= Ecannon.Range && Menu.Item("UseBug", true).GetValue<bool>())
                {
                    castBug(target);
                }

                else
                {
                    var QchargePred = Qcharge.GetPrediction(target);

                    //Try QE
                    if (QchargePred.Hitchance >= HitChance.High)
                    {
                        Ecannon.Cast(GateVector(target));
                        Qcharge.Cast(target);
                    }
                    //Try QE with minion collision
                    else if (QchargePred.Hitchance == HitChance.Collision)
                    {
                        var minion = QchargePred.CollisionObjects.OrderBy(unit => unit.Distance(Player.ServerPosition)).First();
                        if (minion.Distance(QchargePred.UnitPosition) < (180 - minion.BoundingRadius / 2) &&
                            minion.Distance(target.ServerPosition) < (180 - minion.BoundingRadius / 2))
                        {
                            Ecannon.Cast(GateVector(minion));
                            Qcharge.Cast(minion);
                            //if (Qcharge.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                        }
                    }
                }
            }
        }

        private static void castBug(Obj_AI_Base target)
        {
            if (Qcannon.GetPrediction(target).Hitchance >= HitChance.High)
            {
                Qcannon.Cast(target);
                int time = (int)((1000 * Player.Distance(target.Position) / Qcannon.Speed) + Qcannon.Delay * 1000 + Game.Ping / 2f) - 160;
                Utility.DelayAction.Add(time, () => Ecannon.Cast(target.Position));
            }
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            var damage = 0d;
            if (Qcannon.IsReady() && Ecannon.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.4;

            else if (Qcannon.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (Qhammer.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q, 1);

            if (Whammer.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (Ehammer.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            return (float)damage;
        }

        private static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")) && !MinionNames.Any(name => minion.Name.Contains("Spawn")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }

        //EVENTS
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_SpellMissile)) return;
            if (Menu.Item("ForceGate", true).GetValue<bool>() && Ecannon.IsReady() && ((Obj_SpellMissile)sender).SpellCaster.Name == Player.Name && ((Obj_SpellMissile)sender).SData.Name == "JayceShockBlastMis")
            {
                var v2 = Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * 300;
                var bom = new Vector2(v2.X, v2.Y);
                Ecannon.Cast(Player.ServerPosition.To2D() + bom);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("DisableDrawing", true).GetValue<bool>()) return;

            if (Menu.Item("Qcharge", true).GetValue<bool>() && !isHammer)
                Render.Circle.DrawCircle(Player.Position, Qcharge.Range, (Qcharge.IsReady() && Ecannon.IsReady()) ? Color.Green : Color.Red);

            if (Menu.Item("Qcannon", true).GetValue<bool>() && !isHammer)
                Render.Circle.DrawCircle(Player.Position, Qcannon.Range, Qcannon.IsReady() ? Color.Green : Color.Red);

            if (Menu.Item("Qhammer", true).GetValue<bool>() && isHammer)
                Render.Circle.DrawCircle(Player.Position, Qhammer.Range, Qhammer.IsReady() ? Color.Green : Color.Red);

            if (Menu.Item("Ecannon", true).GetValue<bool>() && !isHammer)
                Render.Circle.DrawCircle(Player.Position, Ecannon.Range, Ecannon.IsReady() ? Color.Green : Color.Red);

            if (Menu.Item("Ehammer", true).GetValue<bool>() && isHammer)
                Render.Circle.DrawCircle(Player.Position, Ehammer.Range, Ehammer.IsReady() ? Color.Green : Color.Red);


            if (Menu.Item("Drawcds", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                if (isHammer)
                {
                    if (QcannonCD == 0)
                        Drawing.DrawText(wts[0] - 80, wts[1] + 10, Color.White, "Q Ready");
                    else
                        Drawing.DrawText(wts[0] - 80, wts[1] + 10, Color.Orange, "Q: " + QcannonCD.ToString("0.0"));
                    if (WcannonCD == 0)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 30, Color.White, "W Ready");
                    else
                        Drawing.DrawText(wts[0] - 40, wts[1] + 30, Color.Orange, "W: " + WcannonCD.ToString("0.0"));
                    if (EcannonCD == 0)
                        Drawing.DrawText(wts[0], wts[1] + 10, Color.White, "E Ready");
                    else
                        Drawing.DrawText(wts[0], wts[1] + 10, Color.Orange, "E: " + EcannonCD.ToString("0.0"));
                }
                else
                {
                    if (QhammerCD == 0)
                        Drawing.DrawText(wts[0] - 80, wts[1] + 10, Color.White, "Q Ready");
                    else
                        Drawing.DrawText(wts[0] - 80, wts[1] + 10, Color.Orange, "Q: " + QhammerCD.ToString("0.0"));
                    if (WhammerCD == 0)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 30, Color.White, "W Ready");
                    else
                        Drawing.DrawText(wts[0] - 40, wts[1] + 30, Color.Orange, "W: " + WhammerCD.ToString("0.0"));
                    if (EhammerCD == 0)
                        Drawing.DrawText(wts[0], wts[1] + 10, Color.White, "E Ready");
                    else
                        Drawing.DrawText(wts[0], wts[1] + 10, Color.Orange, "E: " + EhammerCD.ToString("0.0"));
                }
            }

            if (Menu.Item("Insec", true).GetValue<KeyBind>().Active)
            {
                var target = TargetSelector.GetTarget(Qcharge.Range, TargetSelector.DamageType.Physical);
                Vector3 insecPos = Game.CursorPos.Extend(target.Position, Game.CursorPos.Distance(target.Position) + 250);
                var wtsx = Drawing.WorldToScreen(Game.CursorPos);
                var wts = Drawing.WorldToScreen(target.Position);
                Drawing.DrawLine(wts[0], wts[1], wtsx[0], wtsx[1], 5f, System.Drawing.Color.Red);
                Render.Circle.DrawCircle(insecPos, 110, System.Drawing.Color.Blue, 5);
            }

            if (Menu.Item("DrawDmg", true).GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => !x.IsDead && x.IsEnemy && x.IsVisible))
                {
                    hpi.unit = enemy;
                    hpi.drawDmg(ComboDamage(enemy), Color.Yellow);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.Distance(gapcloser.Sender.Position) <= Ehammer.Range && Ehammer.IsReady())
            {
                if (!isHammer && Rswitch.IsReady()) Rswitch.Cast();
                if (isHammer && Ehammer.IsReady()) Ehammer.Cast(gapcloser.Sender);
            }
        }

        private static void InterrupterOnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Player.Distance(sender.Position) <= Qhammer.Range && Ehammer.IsReady())
            {
                if (!isHammer && Rswitch.IsReady()) Rswitch.Cast();
                if (Player.Distance(sender.Position) <= Ehammer.Range)
                {
                    Ehammer.Cast(sender);
                    return;
                }
                if (Player.Distance(sender.Position) <= Qhammer.Range && Qhammer.IsReady())
                {
                    Qhammer.Cast(sender);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe) GetCooldowns(spell);
        }

        private static void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (isHammer)
            {
                if (spell.SData.Name == "JayceToTheSkies")
                    QhammerCDrem = Game.Time + CalculateCd(QhammerTrueCD[Qhammer.Level - 1]);
                if (spell.SData.Name == "JayceStaticField")
                    WhammerCDrem = Game.Time + CalculateCd(WhammerTrueCD[Whammer.Level - 1]);
                if (spell.SData.Name == "JayceThunderingBlow")
                    EhammerCDrem = Game.Time + CalculateCd(EhammerTrueCD[Ehammer.Level - 1]);
            }
            else
            {
                if (spell.SData.Name == "jayceshockblast")
                    QcannonCDrem = Game.Time + CalculateCd(QcannonTrueCD[Qcannon.Level - 1]);
                if (spell.SData.Name == "jaycehypercharge")
                    WcannonCDrem = Game.Time + CalculateCd(WcannonTrueCD[Wcannon.Level - 1]);
                if (spell.SData.Name == "jayceaccelerationgate")
                    EcannonCDrem = Game.Time + CalculateCd(EcannonTrueCD[Ecannon.Level - 1]);
            }
        }

        private static float CalculateCd(float time)
        {
            return time + (time * Player.PercentCooldownMod);
        }

        private static void ProcessCDs()
        {
            QhammerCD = ((QhammerCDrem - Game.Time) > 0) ? (QhammerCDrem - Game.Time) : 0;
            WhammerCD = ((WhammerCDrem - Game.Time) > 0) ? (WhammerCDrem - Game.Time) : 0;
            EhammerCD = ((EhammerCDrem - Game.Time) > 0) ? (EhammerCDrem - Game.Time) : 0;
            QcannonCD = ((QcannonCDrem - Game.Time) > 0) ? (QcannonCDrem - Game.Time) : 0;
            WcannonCD = ((WcannonCDrem - Game.Time) > 0) ? (WcannonCDrem - Game.Time) : 0;
            EcannonCD = ((EcannonCDrem - Game.Time) > 0) ? (EcannonCDrem - Game.Time) : 0;
        }
    }
}
