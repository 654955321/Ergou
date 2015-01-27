using System;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace BrianSharp.Common
{
    class Orbwalker
    {
        private static Menu Config;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static Obj_AI_Hero ForcedTarget = null;
        private static Obj_AI_Minion PrevMinion = null;
        public enum Mode
        {
            Combo,
            Harass,
            LaneClear,
            LastHit,
            Flee,
            None
        }
        private static bool Attack = true;
        private static bool Move = true;
        private static bool DisableNextAttack;
        private const float ClearWaitTime = 2;
        private static int LastAttack;
        private static int LastMove;
        private static int WindUp;
        private static int LastRealAttack;
        private static AttackableUnit LastTarget;
        private static Spell MovePrediction;
        private static readonly Random RandomPos = new Random(DateTime.Now.Millisecond);
        private static readonly Dictionary<string, string[]> NoInterruptSpells = new Dictionary<string, string[]>() { { "Varus", new[] { "VarusQ" } }, { "Lucian", new[] { "LucianR" } } };
        public class BeforeAttackEventArgs
        {
            public AttackableUnit Target;
            private bool Value = true;
            public bool Process
            {
                get { return Value; }
                set
                {
                    DisableNextAttack = !value;
                    Value = value;
                }
            }
        }
        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs Args);
        public delegate void OnAttackEvenH(AttackableUnit Target);
        public delegate void AfterAttackEvenH(AttackableUnit Target);
        public delegate void OnTargetChangeH(AttackableUnit OldTarget, AttackableUnit NewTarget);
        public delegate void OnNonKillableMinionH(AttackableUnit Minion);
        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnAttackEvenH OnAttack;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnTargetChangeH OnTargetChange;
        public static event OnNonKillableMinionH OnNonKillableMinion;

        public static void AddToMainMenu(Menu MainMenu)
        {
            Config = MainMenu;
            var OWMenu = new Menu("Orbwalker", "OW");
            {
                var DrawMenu = new Menu("Draw", "Draw");
                {
                    DrawMenu.AddItem(new MenuItem("OW_Draw_AARange", "Player AA Circle").SetValue(new Circle(false, Color.FloralWhite)));
                    DrawMenu.AddItem(new MenuItem("OW_Draw_AARangeEnemy", "Enemy AA Circle").SetValue(new Circle(false, Color.Pink)));
                    DrawMenu.AddItem(new MenuItem("OW_Draw_HoldZone", "Hold Zone").SetValue(new Circle(false, Color.FloralWhite)));
                    DrawMenu.AddItem(new MenuItem("OW_Draw_HpBar", "Minion Hp Bar").SetValue(new Circle(false, Color.Black)));
                    DrawMenu.AddItem(new MenuItem("OW_Draw_HpBarThickness", "-> Line Thickness").SetValue(new Slider(1, 1, 3)));
                    DrawMenu.AddItem(new MenuItem("OW_Draw_LastHit", "Minion Last Hit").SetValue(new Circle(false, Color.Lime)));
                    DrawMenu.AddItem(new MenuItem("OW_Draw_NearKill", "Minion Near Kill").SetValue(new Circle(false, Color.Gold)));
                    OWMenu.AddSubMenu(DrawMenu);
                }
                var MiscMenu = new Menu("Misc", "Misc");
                {
                    MiscMenu.AddItem(new MenuItem("OW_Misc_HoldZone", "Hold Zone").SetValue(new Slider(50, 0, 150)));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_FarmDelay", "Farm Delay").SetValue(new Slider(0, 0, 300)));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_MoveDelay", "Movement Delay").SetValue(new Slider(0, 0, 150)));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_ExtraWindUp", "Extra WindUp Time").SetValue(new Slider(80, 0, 200)));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_AutoWindUp", "-> Auto WindUp").SetValue(true));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_PriorityUnit", "Priority Unit").SetValue(new StringList(new[] { "Minion", "Hero" })));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_MeleeMagnet", "Melee Movement Magnet").SetValue(true));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_AllMovementDisabled", "Disable All Movement").SetValue(false));
                    MiscMenu.AddItem(new MenuItem("OW_Misc_AllAttackDisabled", "Disable All Attack").SetValue(false));
                    OWMenu.AddSubMenu(MiscMenu);
                }
                var ModeMenu = new Menu("Mode", "Mode");
                {
                    var ComboMenu = new Menu("Combo", "OW_Combo");
                    {
                        ComboMenu.AddItem(new MenuItem("OW_Combo_Key", "Key").SetValue(new KeyBind(32, KeyBindType.Press)));
                        ComboMenu.AddItem(new MenuItem("OW_Combo_Move", "Movement").SetValue(true));
                        ComboMenu.AddItem(new MenuItem("OW_Combo_Attack", "Attack").SetValue(true));
                        ModeMenu.AddSubMenu(ComboMenu);
                    }
                    var HarassMenu = new Menu("Harass", "OW_Harass");
                    {
                        HarassMenu.AddItem(new MenuItem("OW_Harass_Key", "Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                        HarassMenu.AddItem(new MenuItem("OW_Harass_Move", "Movement").SetValue(true));
                        HarassMenu.AddItem(new MenuItem("OW_Harass_Attack", "Attack").SetValue(true));
                        HarassMenu.AddItem(new MenuItem("OW_Harass_LastHit", "Last Hit Minion").SetValue(true));
                        ModeMenu.AddSubMenu(HarassMenu);
                    }
                    var ClearMenu = new Menu("Lane/Jungle Clear", "OW_Clear");
                    {
                        ClearMenu.AddItem(new MenuItem("OW_Clear_Key", "Key").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
                        ClearMenu.AddItem(new MenuItem("OW_Clear_Move", "Movement").SetValue(true));
                        ClearMenu.AddItem(new MenuItem("OW_Clear_Attack", "Attack").SetValue(true));
                        ModeMenu.AddSubMenu(ClearMenu);
                    }
                    var LastHitMenu = new Menu("Last Hit", "OW_LastHit");
                    {
                        LastHitMenu.AddItem(new MenuItem("OW_LastHit_Key", "Key").SetValue(new KeyBind(17, KeyBindType.Press)));
                        LastHitMenu.AddItem(new MenuItem("OW_LastHit_Move", "Movement").SetValue(true));
                        LastHitMenu.AddItem(new MenuItem("OW_LastHit_Attack", "Attack").SetValue(true));
                        ModeMenu.AddSubMenu(LastHitMenu);
                    }
                    var FleeMenu = new Menu("Flee", "OW_Flee");
                    {
                        FleeMenu.AddItem(new MenuItem("OW_Flee_Key", "Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                        ModeMenu.AddSubMenu(FleeMenu);
                    }
                    OWMenu.AddSubMenu(ModeMenu);
                }
                OWMenu.AddItem(new MenuItem("OW_Info", "Credits: xSLx"));
                Config.AddSubMenu(OWMenu);
            }
            MovePrediction = new Spell(SpellSlot.Unknown, GetAutoAttackRange());
            MovePrediction.SetTargetted(Player.BasicAttack.SpellCastTime, Player.BasicAttack.MissileSpeed);
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += OnProcessSpellCast;
            GameObject.OnCreate += OnCreateObjMissile;
            Spellbook.OnStopCast += OnStopCast;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            CheckAutoWindUp();
            if (Player.IsDead || CurrentMode == Mode.None || MenuGUI.IsChatOpen || Player.IsRecalling()) return;
            if (Player.IsChannelingImportantSpell() && (!NoInterruptSpells.ContainsKey(Player.ChampionName) || !NoInterruptSpells[Player.ChampionName].Contains(Player.LastCastedSpellName()))) return;
            Orbwalk(CurrentMode == Mode.Flee ? null : GetPossibleTarget());
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("OW_Draw_AARange").GetValue<Circle>().Active) Render.Circle.DrawCircle(Player.Position, GetAutoAttackRange(), Config.Item("OW_Draw_AARange").GetValue<Circle>().Color);
            if (Config.Item("OW_Draw_AARangeEnemy").GetValue<Circle>().Active)
            {
                foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(1300))) Render.Circle.DrawCircle(Obj.Position, GetAutoAttackRange(Obj, Player), Config.Item("OW_Draw_AARangeEnemy").GetValue<Circle>().Color);
            }
            if (Config.Item("OW_Draw_HoldZone").GetValue<Circle>().Active) Render.Circle.DrawCircle(Player.Position, Config.Item("OW_Misc_HoldZone").GetValue<Slider>().Value, Config.Item("OW_Draw_HoldZone").GetValue<Circle>().Color);
            if (Config.Item("OW_Draw_HpBar").GetValue<Circle>().Active || Config.Item("OW_Draw_LastHit").GetValue<Circle>().Active || Config.Item("OW_Draw_NearKill").GetValue<Circle>().Active)
            {
                foreach (var Obj in MinionManager.GetMinions(GetAutoAttackRange() + 500, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth))
                {
                    if (Config.Item("OW_Draw_HpBar").GetValue<Circle>().Active)
                    {
                        var KillHit = Math.Ceiling(Obj.MaxHealth / Player.GetAutoAttackDamage(Obj, true));
                        var HpBarWidth = Obj.IsMelee() ? 75 : 80;
                        if (Obj.HasBuff("turretshield", true)) HpBarWidth = 70;
                        for (var i = 1; i < KillHit; i++)
                        {
                            var PosX = Obj.HPBarPosition.X + 45 + (float)(HpBarWidth / KillHit) * i;
                            Drawing.DrawLine(new Vector2(PosX, Obj.HPBarPosition.Y + 18), new Vector2(PosX, Obj.HPBarPosition.Y + 23), Config.Item("OW_Draw_HpBarThickness").GetValue<Slider>().Value, Config.Item("OW_Draw_HpBar").GetValue<Circle>().Color);
                        }
                    }
                    if (Config.Item("OW_Draw_LastHit").GetValue<Circle>().Active && Obj.Health <= Player.GetAutoAttackDamage(Obj, true))
                    {
                        Render.Circle.DrawCircle(Obj.Position, Obj.BoundingRadius, Config.Item("OW_Draw_LastHit").GetValue<Circle>().Color);
                    }
                    else if (Config.Item("OW_Draw_NearKill").GetValue<Circle>().Active && Obj.Health <= Player.GetAutoAttackDamage(Obj, true) * 2) Render.Circle.DrawCircle(Obj.Position, Obj.BoundingRadius, Config.Item("OW_Draw_NearKill").GetValue<Circle>().Color);
                }
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (Orbwalking.IsAutoAttackReset(args.SData.Name)) Utility.DelayAction.Add(250, ResetAutoAttack);
            if (!args.SData.IsAutoAttack() || args.SData.Name.ToLower() == "lucianpassiveattack") return;
            if (args.Target is AttackableUnit)
            {
                LastAttack = Environment.TickCount - Game.Ping / 2;
                var Target = (AttackableUnit)args.Target;
                if (Target.IsValid)
                {
                    FireOnTargetSwitch(Target);
                    LastTarget = Target;
                }
                if (sender.IsMelee()) Utility.DelayAction.Add((int)(sender.AttackCastDelay * 1000 + 40), () => FireAfterAttack(LastTarget));
                FireOnAttack(LastTarget);
            }
        }

        private static void OnCreateObjMissile(GameObject sender, EventArgs args)
        {
            if (sender is Obj_LampBulb) return;
            if (!sender.IsValid<Obj_SpellMissile>()) return;
            var missile = (Obj_SpellMissile)sender;
            if (!missile.SData.IsAutoAttack() || missile.SData.Name.ToLower() == "lucianpassiveattack") return;
            if (missile.SpellCaster.IsMe)
            {
                FireAfterAttack(LastTarget);
                LastRealAttack = Environment.TickCount;
            }
        }

        private static void OnStopCast(Spellbook sender, SpellbookStopCastEventArgs args)
        {
            if (!sender.Owner.IsMe) return;
            if (args.DestroyMissile && args.StopAnimation) ResetAutoAttack();
        }

        private static void MoveTo(Vector3 Pos)
        {
            if (Environment.TickCount - LastMove < Config.Item("OW_Misc_MoveDelay").GetValue<Slider>().Value) return;
            LastMove = Environment.TickCount;
            if (Player.Distance(Pos) < Config.Item("OW_Misc_HoldZone").GetValue<Slider>().Value)
            {
                if (Player.Path.Count() > 1) Player.IssueOrder(GameObjectOrder.HoldPosition, Player.ServerPosition);
                return;
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Extend(Pos, (RandomPos.NextFloat(0.6f, 1) + 0.2f) * 300));
        }

        public static void Orbwalk(AttackableUnit Target)
        {
            if (Target.IsValidTarget() && (CanAttack() || HaveCancled()) && IsAllowedToAttack())
            {
                DisableNextAttack = false;
                FireBeforeAttack(Target);
                if (!DisableNextAttack)
                {
                    if (CurrentMode != Mode.Harass || !((Obj_AI_Base)Target).IsMinion || Config.Item("OW_Harass_LastHit").GetValue<bool>())
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        if (LastTarget != null && LastTarget.IsValid && LastTarget != Target) LastAttack = Environment.TickCount + Game.Ping / 2;
                        LastTarget = Target;
                        return;
                    }
                }
            }
            if (!CanMove() || !IsAllowedToMove()) return;
            if (Player.IsMelee() && Player.AttackRange <= 200 && InAutoAttackRange(Target) && Target is Obj_AI_Hero && Config.Item("OW_Misc_MeleeMagnet").GetValue<bool>() && ((Obj_AI_Hero)Target).Distance(Game.CursorPos) < 300)
            {
                MovePrediction.Delay = Player.BasicAttack.SpellCastTime;
                MovePrediction.Speed = Player.BasicAttack.MissileSpeed;
                MoveTo(MovePrediction.GetPrediction((Obj_AI_Hero)Target).UnitPosition);
            }
            else MoveTo(Game.CursorPos);
        }

        private static void ResetAutoAttack()
        {
            LastAttack = 0;
        }

        private static bool IsAllowedToAttack()
        {
            if (!Attack || Config.Item("OW_Misc_AllAttackDisabled").GetValue<bool>()) return false;
            if (CurrentMode == Mode.Combo && !Config.Item("OW_Combo_Attack").GetValue<bool>()) return false;
            if (CurrentMode == Mode.Harass && !Config.Item("OW_Harass_Attack").GetValue<bool>()) return false;
            if (CurrentMode == Mode.LaneClear && !Config.Item("OW_Clear_Attack").GetValue<bool>()) return false;
            return CurrentMode != Mode.LastHit || Config.Item("OW_LastHit_Attack").GetValue<bool>();
        }

        private static bool IsAllowedToMove()
        {
            if (!Move || Config.Item("OW_Misc_AllMovementDisabled").GetValue<bool>()) return false;
            if (CurrentMode == Mode.Combo && !Config.Item("OW_Combo_Move").GetValue<bool>()) return false;
            if (CurrentMode == Mode.Harass && !Config.Item("OW_Harass_Move").GetValue<bool>()) return false;
            if (CurrentMode == Mode.LaneClear && !Config.Item("OW_Clear_Move").GetValue<bool>()) return false;
            return CurrentMode != Mode.LastHit || Config.Item("OW_LastHit_Move").GetValue<bool>();
        }

        private static void CheckAutoWindUp()
        {
            if (!Config.Item("OW_Misc_AutoWindUp").GetValue<bool>())
            {
                WindUp = GetCurrentWindupTime();
                return;
            }
            var Sub = 0;
            if (Game.Ping >= 100)
            {
                Sub = Game.Ping / 100 * 5;
            }
            else if (Game.Ping > 40 && Game.Ping < 100)
            {
                Sub = Game.Ping / 100 * 10;
            }
            else if (Game.Ping <= 40) Sub = 20;
            var windUp = Game.Ping + Sub;
            if (windUp < 40) windUp = 40;
            Config.Item("OW_Misc_ExtraWindUp").SetValue(windUp < 200 ? new Slider(windUp, 0, 200) : new Slider(200, 0, 200));
            WindUp = windUp;
            Config.Item("OW_Misc_AutoWindUp").SetValue(false);
        }

        private static int GetCurrentWindupTime()
        {
            return Config.Item("OW_Misc_ExtraWindUp").GetValue<Slider>().Value;
        }

        public static float GetAutoAttackRange(Obj_AI_Base Source = null, AttackableUnit Target = null)
        {
            if (Source == null) Source = Player;
            return Source.AttackRange + Source.BoundingRadius + (Target.IsValidTarget() ? Target.BoundingRadius : 0);
        }

        public static bool InAutoAttackRange(AttackableUnit Target)
        {
            if (Target == null) return false;
            if (Player.ChampionName == "Azir" && Target.IsValidTarget(1000) && !(Target is Obj_AI_Turret || Target is Obj_BarracksDampener || Target is Obj_HQ) && ObjectManager.Get<Obj_AI_Minion>().Any(i => i.Name == "AzirSoldier" && i.IsAlly && i.BoundingRadius < 66 && i.AttackSpeedMod > 1 && i.Distance(Target) <= 400)) return true;
            return Target.IsValidTarget(GetAutoAttackRange(Player, Target));
        }

        private static double GetAzirWDamage(AttackableUnit Target)
        {
            var Solider = ObjectManager.Get<Obj_AI_Minion>().Count(i => i.Name == "AzirSoldier" && i.IsAlly && i.BoundingRadius < 66 && i.AttackSpeedMod > 1 && i.Distance(Target) <= 400);
            if (Solider > 0)
            {
                var Dmg = Player.CalcDamage((Obj_AI_Base)Target, Damage.DamageType.Magical, 45 + (Player.Level < 12 ? 5 : 10) + Player.FlatMagicDamageMod * 0.6);
                return Dmg + (Solider == 2 ? Dmg * 0.25 : 0);
            }
            return Player.GetAutoAttackDamage((Obj_AI_Base)Target, true);
        }

        private static bool CanAttack()
        {
            if (LastAttack <= Environment.TickCount) return Environment.TickCount + Game.Ping / 2 + 25 >= LastAttack + Player.AttackDelay * 1000 && Attack;
            return false;
        }

        private static bool HaveCancled()
        {
            if (LastAttack - Environment.TickCount > Player.AttackCastDelay * 1000 + 25) return LastRealAttack < LastAttack;
            return false;
        }

        private static bool CanMove()
        {
            if (LastAttack <= Environment.TickCount) return Environment.TickCount + Game.Ping / 2 >= LastAttack + Player.AttackCastDelay * 1000 + WindUp && Move;
            return false;
        }

        private static int GetCurrentFarmDelay
        {
            get { return Config.Item("OW_Misc_FarmDelay").GetValue<Slider>().Value; }
        }

        public static Mode CurrentMode
        {
            get
            {
                if (Config.Item("OW_Combo_Key").GetValue<KeyBind>().Active) return Mode.Combo;
                if (Config.Item("OW_Harass_Key").GetValue<KeyBind>().Active) return Mode.Harass;
                if (Config.Item("OW_Clear_Key").GetValue<KeyBind>().Active) return Mode.LaneClear;
                if (Config.Item("OW_LastHit_Key").GetValue<KeyBind>().Active) return Mode.LastHit;
                return Config.Item("OW_Flee_Key").GetValue<KeyBind>().Active ? Mode.Flee : Mode.None;
            }
        }

        //public static void SetAttack(bool Value)
        //{
        //    Attack = Value;
        //}

        //public static void SetMovement(bool Value)
        //{
        //    Move = Value;
        //}

        private static bool ShouldWait()
        {
            return ObjectManager.Get<Obj_AI_Minion>().Any(i => InAutoAttackRange(i) && i.Team != GameObjectTeam.Neutral && HealthPrediction.LaneClearHealthPrediction(i, (int)(Player.AttackDelay * 1000 * ClearWaitTime), GetCurrentFarmDelay) <= (Player.ChampionName == "Azir" ? GetAzirWDamage(i) : Player.GetAutoAttackDamage(i, true)));
        }

        private static AttackableUnit GetPossibleTarget()
        {
            AttackableUnit Target = null;
            if (Config.Item("OW_Misc_PriorityUnit").GetValue<StringList>().SelectedIndex == 1 && (CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear))
            {
                Target = GetBestHeroTarget();
                if (Target.IsValidTarget()) return Target;
            }
            if (CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear || CurrentMode == Mode.LastHit)
            {
                foreach (var Obj in ObjectManager.Get<Obj_AI_Minion>().Where(i => InAutoAttackRange(i) && i.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(i, true)))
                {
                    var Time = (int)(Player.AttackCastDelay * 1000 - 100 + Game.Ping / 2 + 1000 * Player.Distance((AttackableUnit)Obj) / (Player.IsMelee() ? float.MaxValue : Player.BasicAttack.MissileSpeed));
                    var HpPred = HealthPrediction.GetHealthPrediction(Obj, Time, GetCurrentFarmDelay);
                    if (HpPred <= 0) FireOnNonKillableMinion(Obj);
                    if (HpPred > 0 && HpPred <= (Player.ChampionName == "Azir" ? GetAzirWDamage(Obj) : Player.GetAutoAttackDamage(Obj, true))) return Obj;
                }
            }
            if (InAutoAttackRange(ForcedTarget)) return ForcedTarget;
            if (CurrentMode == Mode.LaneClear)
            {
                foreach (var Obj in ObjectManager.Get<Obj_AI_Turret>().Where(i => InAutoAttackRange(i))) return Obj;
                foreach (var Obj in ObjectManager.Get<Obj_BarracksDampener>().Where(i => InAutoAttackRange(i))) return Obj;
                foreach (var Obj in ObjectManager.Get<Obj_HQ>().Where(i => InAutoAttackRange(i))) return Obj;
            }
            if (CurrentMode != Mode.LastHit)
            {
                Target = GetBestHeroTarget();
                if (Target.IsValidTarget()) return Target;
            }
            if (CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear)
            {
                Target = ObjectManager.Get<Obj_AI_Minion>().Where(i => InAutoAttackRange(i) && i.Team == GameObjectTeam.Neutral).MaxOrDefault(i => i.MaxHealth);
                if (Target != null) return Target;
            }
            if (CurrentMode == Mode.LaneClear && !ShouldWait())
            {
                if (InAutoAttackRange(PrevMinion))
                {
                    var HpPred = HealthPrediction.LaneClearHealthPrediction(PrevMinion, (int)(Player.AttackDelay * 1000 * ClearWaitTime), GetCurrentFarmDelay);
                    if (HpPred >= 2 * (Player.ChampionName == "Azir" ? GetAzirWDamage(PrevMinion) : Player.GetAutoAttackDamage(PrevMinion, true)) || Math.Abs(HpPred - PrevMinion.Health) < float.Epsilon) return PrevMinion;
                }
                Target = (from Obj in ObjectManager.Get<Obj_AI_Minion>().Where(i => InAutoAttackRange(i))
                          let HpPred = HealthPrediction.LaneClearHealthPrediction(Obj, (int)(Player.AttackDelay * 1000 * ClearWaitTime), GetCurrentFarmDelay)
                          where HpPred >= 2 * (Player.ChampionName == "Azir" ? GetAzirWDamage(Obj) : Player.GetAutoAttackDamage(Obj, true)) || Math.Abs(HpPred - Obj.Health) < float.Epsilon
                          select Obj).MaxOrDefault(i => i.Health);
                if (Target != null) PrevMinion = (Obj_AI_Minion)Target;
            }
            return Target;
        }

        private static Obj_AI_Hero GetBestHeroTarget()
        {
            Obj_AI_Hero KillableObj = null;
            var HitsToKill = double.MaxValue;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => InAutoAttackRange(i)))
            {
                var KillHits = Obj.Health / (Player.ChampionName == "Azir" ? GetAzirWDamage(Obj) : Player.GetAutoAttackDamage(Obj, true));
                if (KillableObj != null && (!(KillHits < HitsToKill) || Obj.HasBuffOfType(BuffType.Invulnerability))) continue;
                KillableObj = Obj;
                HitsToKill = KillHits;
            }
            if (Player.ChampionName == "Azir")
            {
                if (HitsToKill <= 4) return KillableObj;
                Obj_AI_Hero BestObj = null;
                foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => InAutoAttackRange(i) && (BestObj == null || GetAzirWDamage(i) > GetAzirWDamage(BestObj)))) BestObj = Obj;
                if (BestObj != null) return BestObj;
            }
            return HitsToKill <= 3 ? KillableObj : TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
        }

        private static void FireBeforeAttack(AttackableUnit Target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs { Target = Target });
            }
            else DisableNextAttack = false;
        }

        private static void FireOnAttack(AttackableUnit Target)
        {
            if (OnAttack != null) OnAttack(Target);
        }

        private static void FireAfterAttack(AttackableUnit Target)
        {
            if (AfterAttack != null) AfterAttack(Target);
        }

        private static void FireOnTargetSwitch(AttackableUnit NewTarget)
        {
            if (OnTargetChange != null && (!LastTarget.IsValidTarget() || LastTarget != NewTarget)) OnTargetChange(LastTarget, NewTarget);
        }

        private static void FireOnNonKillableMinion(AttackableUnit Minion)
        {
            if (OnNonKillableMinion != null) OnNonKillableMinion(Minion);
        }
    }
}