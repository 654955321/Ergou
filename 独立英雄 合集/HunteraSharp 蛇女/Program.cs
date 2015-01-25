using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Text.RegularExpressions;
using Color = System.Drawing.Color;

namespace Cassiopeia
{
    class Program
    {
        static readonly Obj_AI_Hero player = ObjectManager.Player;
        static Orbwalking.Orbwalker Orbwalker;
        static Spell Q;
        static Spell W;
        static Spell E;
        static Spell R;
        static int dontUseQW = -1;
        static float dontUseQW2 = 0;
        static float castWafter = 0;
        static float castWafter2 = 0;
        static Obj_AI_Hero selectedTarget = null;
        static int legitEdelay = 0;
        static int wallCastTick = 0;
        static Vector2 yasuoWallPos;

        static Boolean debug = false;

        static Menu menu = new Menu("HunteraSharp 蛇女", "Cassiopeia", true);

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            if (player.ChampionName != "Cassiopeia") return;

            Game.PrintChat("HunteraSharp 铔囧コ 鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍锛丵Q缇361630847");

            Q = new Spell(SpellSlot.Q, 850f);
            Q.SetSkillshot(0.6f, 40f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 850f);
            W.SetSkillshot(0.5f, 90f, 2500, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 700f);
            E.SetTargetted(0.2f, float.MaxValue);
            
            R = new Spell(SpellSlot.R, 800f);
            R.SetSkillshot(0.6f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            menu.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalking"));

            menu.AddSubMenu(new Menu("骚扰", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("qHarass", "使用 Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("wHarass", "使用 W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("eHarass", "使用 E").SetValue(true));

            menu.AddSubMenu(new Menu("清线", "LineClear"));
            menu.SubMenu("LineClear").AddItem(new MenuItem("lineclearQ", "使用 Q").SetValue(true));
            menu.SubMenu("LineClear").AddItem(new MenuItem("lineclearW", "使用 W").SetValue(false));
            menu.SubMenu("LineClear").AddItem(new MenuItem("onlylasthitE", "Only last hit E").SetValue(true));

            menu.AddSubMenu(new Menu("清野", "JungleClear"));
            menu.SubMenu("JungleClear").AddItem(new MenuItem("jungleclearQ", "使用 Q").SetValue(true));
            menu.SubMenu("JungleClear").AddItem(new MenuItem("jungleclearW", "使用 W").SetValue(false));

            menu.AddSubMenu(new Menu("R设置", "Ultimate"));
            menu.SubMenu("Ultimate").AddItem(new MenuItem("comboR", "使用 R 连招").SetValue(true));

            menu.SubMenu("Ultimate").SubMenu("AntiGapcloser").AddItem(new MenuItem("gapcloserR", "使用R防止突进").SetValue(true));
            menu.SubMenu("Ultimate").SubMenu("Interruptspells").AddItem(new MenuItem("interruptR", "使用R中断法术").SetValue(true));

            Boolean yasuoHere = false;
            List<Obj_AI_Hero> enemiesChamps = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).ToList();
            foreach (Obj_AI_Hero enemyChamp in enemiesChamps)
            {
                menu.SubMenu("Ultimate").SubMenu("AntiGapcloser").AddItem(new MenuItem(enemyChamp.ChampionName + "gapcloser", "Anti Gapcloser " + enemyChamp.ChampionName).SetValue(true));
                menu.SubMenu("Ultimate").SubMenu("Interruptspells").AddItem(new MenuItem(enemyChamp.ChampionName + "interrupt", "Interrupt spells of " + enemyChamp.ChampionName).SetValue(true));
                if (enemyChamp.ChampionName == "Yasuo")
                    yasuoHere = true;
            }

            menu.AddSubMenu(new Menu("杂项", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("castedalay", "使用 E 延迟").SetValue(new Slider(0, 0, 2000)));
            menu.SubMenu("Misc").AddItem(new MenuItem("castWPoisoned", "使用W（目标未中毒）").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("focusSelectedTarget", "攻击选择目标").SetValue(false));
            menu.SubMenu("Misc").AddItem(new MenuItem("useAAcombo", "连招 使用 走砍").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("PacketCast", "使用 封包").SetValue(true));

            menu.AddSubMenu(new Menu("范围", "Drawings"));
            menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "范围 Q").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "范围 E").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "范围 R").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));

            menu.AddToMainMenu();
            
            Game.OnGameUpdate += OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Game.OnWndProc += GameOnOnWndProc;
            Drawing.OnDraw += DrawingOnOnDraw;
            if (yasuoHere)
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.IsEnemy && sender is Obj_AI_Hero && args.SData.Name == "YasuoWMovingWall" && player.Distance(sender.Position) < 2000)
            {
                wallCastTick = Environment.TickCount + 4000;
                yasuoWallPos = sender.ServerPosition.To2D();
            }
        }

        static Boolean checkYasuoWall(Vector3 enemyPos)
        {
            if (wallCastTick > Environment.TickCount)
            {
                GameObject wall = null;
                foreach (var gameObject in ObjectManager.Get<GameObject>())
                {
                    if (gameObject.IsValid && Regex.IsMatch(gameObject.Name, "_w_windwall", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        wall = gameObject;
                        break;
                    }
                }

                if (wall == null)
                    return false;

                int wallWidth = (300 + 50 * Convert.ToInt32(wall.Name.Substring(wall.Name.Length - 6, 1)));

                var wallDirection = (wall.Position.To2D() - yasuoWallPos).Normalized().Perpendicular();
                var wallStart = wall.Position.To2D() + wallWidth / 2 * wallDirection;
                var wallEnd = wallStart - wallWidth * wallDirection;

                Vector2 Direction = (wallEnd - wallStart).Normalized();
                Vector2 Perpendicular = Direction.Perpendicular();
                Geometry.Polygon wallPolygon = new Geometry.Polygon();

                int widthWall = 75;
                wallPolygon.Add(wallStart + widthWall * Perpendicular);
                wallPolygon.Add(wallStart - widthWall * Perpendicular);
                wallPolygon.Add(wallEnd - widthWall * Perpendicular);
                wallPolygon.Add(wallEnd + widthWall * Perpendicular);

                int polygonCounts = wallPolygon.Points.Count;
                for (var i = 0; i < polygonCounts; i++)
                {
                    var inter = wallPolygon.Points[i].Intersection(wallPolygon.Points[i != polygonCounts - 1 ? i + 1 : 0], player.ServerPosition.To2D(), enemyPos.To2D());
                    if (inter.Intersects)
                        return true;
                }
            }

            return false;
        }

        static void OnGameUpdate(EventArgs args)
        {
            try
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        EFarm();
                        Combo(true);
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        LineClear();
                        JungleClear();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        EFarm();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (debug)
            {
                Obj_AI_Hero enemyR = GetEnemyList().Where(x => x.IsValidTarget(R.Range)).OrderBy(x => x.Health / getRDmg(x)).FirstOrDefault();
                if (enemyR != null)
                {
                    PredictionOutput castPred = R.GetPrediction(enemyR, true, R.Range);
                    int enemiesHit = GetEnemyList().Where(x => R.WillHit(x.Position, castPred.CastPosition)).Count();
                    Drawing.DrawText(10, 10, Color.Yellow, enemiesHit.ToString());
                }
            }

            var menuItemQ = menu.Item("QRange").GetValue<Circle>();
            if (menuItemQ.Active)
                Render.Circle.DrawCircle(player.Position, Q.Range, menuItemQ.Color);

            var menuItemE = menu.Item("ERange").GetValue<Circle>();
            if (menuItemE.Active)
                Render.Circle.DrawCircle(player.Position, E.Range, menuItemE.Color);

            var menuItemR = menu.Item("RRange").GetValue<Circle>();
            if (menuItemR.Active && R.IsReady() && R.Level > 0)
                Render.Circle.DrawCircle(player.Position, R.Range, menuItemR.Color);


            if (menu.Item("focusSelectedTarget").GetValue<bool>())
            {
                if (selectedTarget.IsValidTarget())
                {
                    Render.Circle.DrawCircle(selectedTarget.Position, 150, Color.Red, 7, true);
                }
            }
        }

        private static void GameOnOnWndProc(WndEventArgs args)
        {
            if (!menu.Item("focusSelectedTarget").GetValue<bool>()) return;

            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
                return;

            selectedTarget = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000)
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Cast.Header)
            {
                var decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (decodedPacket.SourceNetworkId == player.NetworkId && decodedPacket.Slot == SpellSlot.R)
                {
                    Vector3 vecCast = new Vector3(decodedPacket.ToX, decodedPacket.ToY, 0);
                    var query = GetEnemyList().Where(x => R.WillHit(x, vecCast));

                    if (query.Count() == 0)
                    {
                        args.Process = false;
                    }
                }
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("gapcloserR").GetValue<bool>()) return;
            
            if (gapcloser.Sender.IsValidTarget(R.Range) && R.IsReady() && menu.Item(gapcloser.Sender.ChampionName + "gapcloser").GetValue<bool>())
                R.Cast(gapcloser.Sender.ServerPosition, menu.Item("PacketCast").GetValue<bool>());
        }

        static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("interruptR").GetValue<bool>()) return;
            if (!(unit is Obj_AI_Hero)) return;

            Obj_AI_Hero sender = unit as Obj_AI_Hero;

            if (unit.IsValidTarget(R.Range) && spell.DangerLevel >= InterruptableDangerLevel.High && menu.Item(sender + "interrupt").GetValue<bool>())
                R.CastIfHitchanceEquals(unit, unit.IsMoving ? HitChance.High : HitChance.Medium, menu.Item("PacketCast").GetValue<bool>());
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Boolean useAAcombo = menu.Item("useAAcombo").GetValue<bool>();

                args.Process = useAAcombo;

                if (useAAcombo)
                {
                    if (args.Target is Obj_AI_Base)
                    {
                        var target = args.Target as Obj_AI_Base;
                        if (E.IsReady() && target.HasBuffOfType(BuffType.Poison) && target.IsValidTarget(E.Range))
                            args.Process = false;
                    }
                }
            }
        }
        
        private static void EFarm()
        {
            List<Obj_AI_Base> MinionList = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            foreach (var minion in MinionList.Where(x => x.HasBuffOfType(BuffType.Poison)))
            {
                var buffEndTime = GetPoisonBuffEndTime(minion);
                if (buffEndTime > Game.Time + E.Delay)
                {
                    if (getEDmg(minion) >= minion.Health * 1.1)
                        E.CastOnUnit(minion, menu.Item("PacketCast").GetValue<bool>());
                }
            }
        }

        private static void LineClear()
        {
            List<Obj_AI_Base> mobs = MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).ToList();

            if (!mobs.Any())
                return;

            Boolean packetCast = menu.Item("PacketCast").GetValue<bool>();

            if (E.IsReady())
            {
                Obj_AI_Base mob = mobs.Where(x => x.HasBuffOfType(BuffType.Poison) && x.IsValidTarget(E.Range) && GetPoisonBuffEndTime(x) > (Game.Time + E.Delay) && (!menu.Item("onlylasthitE").GetValue<bool>() || getEDmg(x) >= x.Health * 1.1)).FirstOrDefault();
                if (mob != null)
                    E.CastOnUnit(mob, packetCast);
            }

            if (Q.IsReady() && menu.Item("lineclearQ").GetValue<bool>())
            {
                MinionManager.FarmLocation Qunpoisoned = Q.GetCircularFarmLocation(mobs.Where(x => !x.HasBuffOfType(BuffType.Poison)).ToList(), Q.Width * 0.95f);
                if (Qunpoisoned.MinionsHit > 0)
                {
                    Q.Cast(Qunpoisoned.Position, packetCast);
                    castWafter2 = Game.Time + Q.Delay;
                    return;
                }
            }

            if (W.IsReady() && menu.Item("lineclearW").GetValue<bool>() && castWafter2 < Game.Time)
            {
                MinionManager.FarmLocation Qunpoisoned = Q.GetCircularFarmLocation(mobs.Where(x => !x.HasBuffOfType(BuffType.Poison)).ToList(), W.Width);
                if (Qunpoisoned.MinionsHit > 0)
                    W.Cast(Qunpoisoned.Position, packetCast);
            }
        }

        private static void JungleClear()
        {
            List<Obj_AI_Base> mobs = MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).ToList();

            if (!mobs.Any())
                return;

            Boolean packetCast = menu.Item("PacketCast").GetValue<bool>();
            
            MinionManager.FarmLocation mf = Q.GetCircularFarmLocation(mobs, Q.Width * 0.90f);

            if (Q.IsReady() && menu.Item("jungleclearQ").GetValue<bool>())
            {
                if (mf.MinionsHit > 1)
                    Q.Cast(mf.Position, packetCast);
                else
                    Q.Cast(mobs.FirstOrDefault(), packetCast);

                castWafter2 = Game.Time + Q.Delay;
            }

            if (W.IsReady() && menu.Item("jungleclearW").GetValue<bool>() && castWafter2 < Game.Time)
            {
                MinionManager.FarmLocation Qunpoisoned = Q.GetCircularFarmLocation(mobs.Where(x => !x.HasBuffOfType(BuffType.Poison)).ToList(), W.Width);
                if (Qunpoisoned.MinionsHit > 0)
                    W.Cast(Qunpoisoned.Position, packetCast);
            }

            if (E.IsReady())
            {
                Obj_AI_Base mob = mobs.Where(x => x.HasBuffOfType(BuffType.Poison) && x.IsValidTarget(E.Range) && GetPoisonBuffEndTime(x) > (Game.Time + E.Delay)).FirstOrDefault();
                if (mob != null)
                    E.CastOnUnit(mob, packetCast);
            }
            
        }

        private static void Combo(Boolean harass = false)
        {
            Boolean checkTarget = true;

            if (menu.Item("focusSelectedTarget").GetValue<bool>())
            {
                if (selectedTarget.IsValidTarget())
                {
                    checkTarget = false;

                    if (dontUseQW2 > Game.ClockTime)
                        if (selectedTarget.NetworkId == dontUseQW)
                            checkTarget = true;
                }
            }

            try
            {
                if (checkTarget)
                    Orbwalker.ForceTarget(GetEnemyList().Where(x => x.IsValidTarget(Orbwalking.GetRealAutoAttackRange(x))).OrderBy(x => x.Health / getEDmg(x)).FirstOrDefault());
                else
                    Orbwalker.ForceTarget(selectedTarget);
            }
            catch (Exception ex) { }

            Boolean packetCast = menu.Item("PacketCast").GetValue<bool>();

            if (R.IsReady() && !harass)
            {
                if (menu.Item("comboR").GetValue<bool>())
                {
                    Obj_AI_Hero enemyR = GetEnemyList().Where(x => x.IsValidTarget(550f) && !checkYasuoWall(x.Position)).OrderBy(x => x.Health / getRDmg(x)).FirstOrDefault();
                    if (enemyR != null)
                    {
                        PredictionOutput castPred = R.GetPrediction(enemyR, true, R.Range);

                        List<Obj_AI_Hero> enemiesHit = GetEnemyList().Where(x => R.WillHit(x.Position, castPred.CastPosition) && !checkYasuoWall(x.Position)).ToList();
                        int facingEnemies = enemiesHit.Where(x => x.IsFacing(player)).Count();
                        int countList2 = GetEnemyList().Where(x => x.Distance(enemyR.Position) < 250 && !checkYasuoWall(x.Position)).Count();
                        int countList = enemiesHit.Count();

                        if ((countList >= 2 && facingEnemies >= 1) || countList >= 3 || countList2 >= 3 || (countList == 1 && player.Level < 11))
                        {
                            Boolean ult = true;

                            if (countList == 1)
                            {
                                ult = false;
                                if (player.Level < 11)
                                {
                                    int multipleE = (facingEnemies == 1) ? 4 : 2;
                                    int multipleQ = (facingEnemies == 1) ? 2 : 1;
                                    double procHealth = (getEDmg(enemyR) * multipleE + getQDmg(enemyR) * multipleQ + getRDmg(enemyR)) / enemyR.Health;
                                    if (procHealth > 1 && procHealth < 1.5 && (enemyR.HasBuffOfType(BuffType.Poison) || Q.IsReady() || W.IsReady()) && (E.IsReady() || player.Spellbook.GetSpell(E.Slot).CooldownExpires < 1))
                                        ult = true;
                                }
                            }

                            if (ult) R.Cast(castPred.CastPosition, packetCast);
                        }
                    }
                }
            }


            if (E.IsReady() && Environment.TickCount > legitEdelay && (!harass || menu.Item("eHarass").GetValue<bool>()))
            {
                Obj_AI_Hero mainTarget = null;

                if (checkTarget)
                {
                    List<Obj_AI_Hero> Eenemies = GetEnemyList();

                    Obj_AI_Hero eTarget = null;
                    Obj_AI_Hero eTarget2 = null;

                    if (Eenemies.Count() > 0)
                    {
                        double minCast1 = -1;
                        double minCast2 = -1;

                        foreach (Obj_AI_Hero enemyto in Eenemies)
                        {
                            if (!enemyto.IsValidTarget(E.Range)) continue;
                            if (checkYasuoWall(enemyto.ServerPosition)) continue;

                            Boolean buffedEnemy = false;
                            if (enemyto.HasBuffOfType(BuffType.Poison))
                            {
                                var buffEndTime = GetPoisonBuffEndTime(enemyto);
                                if (buffEndTime > Game.Time + E.Delay)
                                    buffedEnemy = true;
                            }

                            if (buffedEnemy)
                            {
                                double casts = enemyto.Health / getEDmg(enemyto);
                                if (minCast1 == -1 || minCast1 > casts)
                                {
                                    minCast1 = casts;
                                    eTarget = enemyto;
                                }
                            }
                            else if (getEDmg(enemyto) > enemyto.Health * 1.03)
                            {
                                float dist = player.Distance(enemyto.Position);
                                if (minCast2 == -1 || minCast2 < dist)
                                {
                                    minCast2 = dist;
                                    eTarget2 = enemyto;
                                }
                            }
                        }
                    }

                    mainTarget = (eTarget != null) ? eTarget : eTarget2;
                }
                else
                {
                    if (player.Distance(selectedTarget.Position) <= E.Range && ((selectedTarget.HasBuffOfType(BuffType.Poison) && GetPoisonBuffEndTime(selectedTarget) > Game.Time + E.Delay) || getEDmg(selectedTarget) > selectedTarget.Health * 1.03))
                        mainTarget = selectedTarget;
                }

                if (mainTarget != null)
                {
                    Orbwalker.ForceTarget(mainTarget);

                    if (E.Cast(mainTarget, packetCast) == Spell.CastStates.SuccessfullyCasted)
                    {
                        int castEdelay = menu.Item("castedalay").GetValue<Slider>().Value;
                        if (castEdelay > 0)
                        {
                            Random rand = new Random();
                            legitEdelay = Environment.TickCount + rand.Next(castEdelay);
                        }

                        if (getEDmg(mainTarget) > mainTarget.Health * 1.1)
                        {
                            dontUseQW = mainTarget.NetworkId;
                            dontUseQW2 = Game.ClockTime + 0.6f;
                        }
                    }
                }
            }

            Obj_AI_Hero enemy = (!checkTarget) ? selectedTarget : getTarget(Q.Range);
            
            if (enemy != null)
            {
                if (Q.IsReady() && (!harass || menu.Item("qHarass").GetValue<bool>()))
                {
                    if (Q.CastIfHitchanceEquals(enemy, HitChance.High, packetCast))
                    {
                        castWafter = Game.Time + Q.Delay;
                        return;
                    }
                }

                if (!Q.IsReady() && castWafter < Game.Time && W.IsReady() && (!enemy.HasBuffOfType(BuffType.Poison) || !menu.Item("castWPoisoned").GetValue<bool>()) && (!harass || menu.Item("wHarass").GetValue<bool>()))
                {
                    W.CastIfHitchanceEquals(enemy, HitChance.High, packetCast);
                    return;
                }
            }
        }

        private static Obj_AI_Hero getTarget(float range)
        {
            Obj_AI_Hero target = null;
            
            if (Q.IsReady() || W.IsReady())
            {
                Boolean targetWall = false;
                double minCasts = -1;
                foreach (var eTarget in GetEnemyList())
                {
                    if (eTarget.IsValidTarget(range))
                    {
                        if (dontUseQW2 > Game.ClockTime)
                            if (eTarget.NetworkId == dontUseQW)
                                continue;

                        PredictionOutput QPred = Q.GetPrediction(eTarget, true, Q.Range);
                        if (QPred.AoeTargetsHitCount > 0)
                            if (player.Distance(QPred.CastPosition) > Q.Range)
                                continue;

                        Boolean wallHit = checkYasuoWall(eTarget.ServerPosition);
                        if (wallHit)
                        {
                            if (!Q.IsReady() || (target != null && !targetWall))
                                continue;
                        }

                        double casts = eTarget.Health / getEDmg(eTarget);
                        if (target == null || minCasts > casts)
                        {
                            target = eTarget;
                            minCasts = casts;
                        }

                        targetWall = wallHit;
                    }
                }
            }

            return target;
        }

        private static float GetPoisonBuffEndTime(Obj_AI_Base target)
        {
            var buffEndTime = target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Poison)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
            return buffEndTime;
        }

        private static List<Obj_AI_Hero> GetEnemyList()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValid && !x.IsInvulnerable).ToList();
        }

        private static double getQDmg(Obj_AI_Base target)
        {
            return player.CalcDamage(target, Damage.DamageType.Magical, (SpellManaReq(Q, 30, 16) + 0.18 * player.FlatMagicDamageMod));
        }

        private static double getEDmg(Obj_AI_Base target)
        {
            return player.CalcDamage(target, Damage.DamageType.Magical, (SpellManaReq(E, 55, 25) + 0.55 * player.FlatMagicDamageMod));
        }

        private static double getRDmg(Obj_AI_Base target)
        {
            return player.CalcDamage(target, Damage.DamageType.Magical, (SpellManaReq(R, 150, 100) + 0.5 * player.FlatMagicDamageMod));
        }

        static float SpellManaReq(Spell spell, float firstLevel, float step)
        {
            return (firstLevel + ((spell.Level - 1) * step));
        }

    }
}