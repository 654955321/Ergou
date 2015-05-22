using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Viktor
{
    internal class Program
    {
        private static  readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private const int ECastRange = 550;

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Viktor") return;
            Utils.ClearConsole();

            #region Spells
            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 700);

            Q.SetTargetted(0.25f, 2000);
            W.SetSkillshot(0.25f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.0f, 90, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
            #endregion

            #region Menu
            Menu = new Menu(" Apollo's 维克托", "Viktor", true);

            TargetSelector.AddToMenu(Menu.SubMenu("目标选择"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.AddSubMenu(new Menu("走砍", "Orbwalking")));

            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "使用 Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseWC", "使用 W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "使用 E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "使用 R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("kek", ""));
            Menu.SubMenu("Combo").AddItem(new MenuItem("PredEC", "最小值 命中率 E").SetValue(new StringList((new[] {"低", "中", "高", "很高"}), 2)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("HitRC", "最小值 R命中人数").SetValue(new Slider(3, 1, 5)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseIgniteC", "使用点燃").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("KeyC", "连招 (" + Utils.KeyToText(Menu.Item("Orbwalk").GetValue<KeyBind>().Key) + ")", true)).DontSave();

            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "使用 Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseEH", "使用 E").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("kek", ""));
            Menu.SubMenu("Harass").AddItem(new MenuItem("PredEH", "最小值 命中率 E").SetValue(new StringList((new[] { "低", "中", "高", "很高"}), 3)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("ManaH", "技能骚扰蓝量%").SetValue(new Slider(30)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("KeyH", "骚扰 (" + Utils.KeyToText(Menu.Item("Farm").GetValue<KeyBind>().Key) + ")", true)).DontSave();

            Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "使用 Q").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseEL", "使用 E").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("kek", ""));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("HitEL", "清线 E|小兵数量").SetValue(new Slider(3, 1, 10)));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("ManaL", "技能清线|蓝量%").SetValue(new Slider(30)));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("KeyL", "清线 (" + Utils.KeyToText(Menu.Item("LaneClear").GetValue<KeyBind>().Key) + ")", true)).DontSave();

            Menu.SubMenu("Misc").AddItem(new MenuItem("UseQinAA", "只有在平A范围才使用Q").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AutoW", "自动 W").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AutoFollowR", "R自动跟随目标").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("KsE", "使用E抢人头").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("GapcloserW", "使用W防止突进").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("InterrupterW", "使用W中断法术").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("InterrupterR", "使用R中断法术").SetValue(false));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q 范围").SetValue(new Circle(true, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "W 范围").SetValue(new Circle(false, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E 范围").SetValue(new Circle(true, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "R 范围").SetValue(new Circle(false, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("CDDraw", "显示 CD").SetValue(new Circle(false, Color.DarkRed)));
            MenuItem drawComboDamageMenu = new MenuItem("DmgDraw", "显示组合连招伤害", true).SetValue(true);
            MenuItem drawFill = new MenuItem("DmgFillDraw", "显示组合连招填充伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
            Menu.SubMenu("Drawings").AddItem(drawComboDamageMenu);
            Menu.SubMenu("Drawings").AddItem(drawFill);
            DamageIndicator.DamageToUnit = ComboDmg;
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

            Menu.AddToMainMenu();
            #endregion

            UpdateChecker.Init("Apollo16", "Viktor");
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            ShowNotification("Apollo's " + ObjectManager.Player.ChampionName + " Loaded", NotificationColor, 7000);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling() || args == null)
                return;

            if (E.IsReady() && Menu.Item("KsE").GetValue<bool>())
            {
                foreach (var t in HeroManager.Enemies.Where(h => h.IsValidTarget(ECastRange + E.Range) && h.Health < Player.GetSpellDamage(h, SpellSlot.E)))
                {
                    CastE(t, HitChance.VeryHigh);
                }
            }

            if (R.Instance.Name != "ViktorChaosStorm" && Menu.Item("AutoFollowR").GetValue<bool>())
            {
                var stormT = TargetSelector.GetTarget(Player, 1100, TargetSelector.DamageType.Magical);
                if (stormT != null)
                    Utility.DelayAction.Add(200, () => R.Cast(stormT.ServerPosition));
            }

            AutoW();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                {
                    Combo();
                    break;
                }
                case Orbwalking.OrbwalkingMode.Mixed:
                {
                    Harass();
                    break;
                }
                case Orbwalking.OrbwalkingMode.LaneClear:
                {
                    LaneClear();
                    break;
                }
            }
        }

        private static void Combo()
        {
            if (IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() && Menu.Item("UseIgniteC").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (t != null)
                    if (t.Health < ComboDmg(t))
                        Player.Spellbook.CastSpell(IgniteSlot, t);
            }
            if (Q.IsReady() && Menu.Item("UseQC").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                {
                    if (Orbwalking.InAutoAttackRange(t))
                        Q.Cast(t);
                    else if (!Menu.Item("UseQinAA").GetValue<bool>())
                        Q.Cast(t);
                }
            }
            if (W.IsReady() && Menu.Item("UseWC").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                {
                    if (t.Path.Count() < 2)
                    {
                        if (t.HasBuffOfType(BuffType.Slow))
                        {
                            if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                    return;
                        }
                        if (t.CountEnemiesInRange(250) > 2)
                        {
                            if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                    return;
                        }
                        if (Player.Position.Distance(t.ServerPosition) < Player.Position.Distance(t.Position))
                        {
                            if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                return;
                        }
                        else
                        {
                            if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                }
            }
            if (E.IsReady() && Menu.Item("UseEC").GetValue<bool>())
            {
                var hitchance = (HitChance)(Menu.Item("PredEC").GetValue<StringList>().SelectedIndex + 3);
                var t = TargetSelector.GetTarget(ECastRange + E.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    CastE(t, hitchance);
            }
            if (R.IsReady() && Menu.Item("UseRC").GetValue<bool>() && R.Instance.Name == "ViktorChaosStorm")
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                {
                    if (t.Health < ComboDmg(t) && t.HealthPercent > 5)
                        Utility.DelayAction.Add(100, () => R.Cast(t, false, true));
                }
                foreach (var unit in HeroManager.Enemies.Where(h => h.IsValidTarget(R.Range)))
                {
                    R.CastIfWillHit(unit, Menu.Item("HitRC").GetValue<Slider>().Value);
                }
            }
        }

        private static void Harass()
        {
            if (Player.ManaPercentage() < Menu.Item("ManaH").GetValue<Slider>().Value)
                return;

            if (Q.IsReady() && Menu.Item("UseQH").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                {
                    if (Orbwalking.InAutoAttackRange(t))
                        Q.Cast(t);
                    else if (!Menu.Item("UseQinAA").GetValue<bool>())
                        Q.Cast(t);
                }
            }
            if (E.IsReady() && Menu.Item("UseEH").GetValue<bool>())
            {
                var hitchance = (HitChance) (Menu.Item("PredEH").GetValue<StringList>().SelectedIndex + 3);
                var t = TargetSelector.GetTarget(ECastRange + E.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    CastE(t, hitchance);
            }
        }

        private static void LaneClear()
        {
            if (Player.ManaPercentage() < Menu.Item("ManaL").GetValue<Slider>().Value)
                return;

            if (Q.IsReady() && Menu.Item("UseQL").GetValue<bool>())
            {
                var minionQ =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (minionQ != null)
                    Q.Cast(minionQ);
            }
            if (E.IsReady() && Menu.Item("UseEL").GetValue<bool>())
            {
                var minionJ =
                    MinionManager.GetMinions(ECastRange + E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (minionJ != null)
                {
                    if (minionJ.Distance(Player) > ECastRange)
                        E.Cast(Player.ServerPosition.Extend(minionJ.ServerPosition, ECastRange), minionJ.ServerPosition);
                    else
                        E.Cast(minionJ.ServerPosition.Extend(Player.ServerPosition, 50), minionJ.ServerPosition);
                }
                else
                {
                    foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, ECastRange))
                    {
                        var farmLocation =
                            MinionManager.GetBestLineFarmLocation(
                                MinionManager.GetMinions(minion.ServerPosition, E.Range)
                                    .Select(m => m.ServerPosition.To2D())
                                    .ToList(), E.Width, E.Speed);
                        if (farmLocation.MinionsHit >= Menu.Item("HitEL").GetValue<Slider>().Value)
                            E.Cast(minion.ServerPosition, farmLocation.Position.To3D());
                    }
                }
            }
        }

        private static void CastE(Obj_AI_Base t, HitChance hitchance)
        {
            if (Player.ServerPosition.Distance(t.ServerPosition) < ECastRange)
            {
                E.UpdateSourcePosition(t.ServerPosition, t.ServerPosition);
                var pred = E.GetPrediction(t, true);
                if (pred.Hitchance >= hitchance)
                    E.Cast(t.ServerPosition, pred.CastPosition);
            }
            else if (Player.ServerPosition.Distance(t.ServerPosition) < ECastRange + E.Range)
            {
                var castStartPos = Player.ServerPosition.Extend(t.ServerPosition, ECastRange);
                E.UpdateSourcePosition(castStartPos, castStartPos);
                var pred = E.GetPrediction(t, true);
                if (pred.Hitchance >= hitchance)
                    E.Cast(castStartPos, pred.CastPosition);
            }
        }

        private static void AutoW()
        {
            if (!W.IsReady() || !Menu.Item("AutoW").GetValue<bool>())
                return;

            var tPanth = HeroManager.Enemies.Find(h => h.IsValidTarget(W.Range) && h.HasBuff("Pantheon_GrandSkyfall_Jump", true));
            if (tPanth != null)
            {
                if (W.Cast(tPanth) == Spell.CastStates.SuccessfullyCasted)
                    return;
            }

            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget(W.Range)))
            {
                if (enemy.HasBuff("rocketgrab2"))
                {
                    var t = HeroManager.Allies.Find(h => h.BaseSkinName.ToLower() == "blitzcrank" && h.Distance(Player) < W.Range);
                    if (t != null)
                    {
                        if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                            return;
                    }
                }
                if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.IsRecalling())
                {
                    if (W.Cast(enemy) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                if (W.GetPrediction(enemy).Hitchance == HitChance.Immobile)
                {
                    if (W.Cast(enemy) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead || args == null)
                return;

            Spell[] spells = {Q, W, E, R};
            foreach (var spell in spells)
            {
                var menuItem = Menu.Item("Draw" + spell.Slot).GetValue<Circle>();
                var drawCd = Menu.Item("CDDraw").GetValue<Circle>();
                if (spell.Slot == SpellSlot.E && menuItem.Active && spell.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, ECastRange + E.Range,
                        (drawCd.Active && !spell.IsReady()) ? drawCd.Color : menuItem.Color);
                }
                else if (menuItem.Active && spell.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range,
                        (drawCd.Active && !spell.IsReady()) ? drawCd.Color : menuItem.Color);
                }
            }
        }

        private static float ComboDmg(Obj_AI_Base enemy)
        {
            var qaaDmg = new Double[] { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };
            var damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (Player.HasBuff("viktorpowertransferreturn") || Q.IsReady())
            {
                damage += Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    qaaDmg[Player.Level >= 18 ? 18 - 1 : Player.Level - 1] +
                    (Player.TotalMagicalDamage * .5) + Player.TotalAttackDamage());
            }

            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (Player.Buffs.Any(h => h.Name.Contains("E") && h.Name.Contains("Aug") && h.Name.Contains("Viktor")))
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * .4;
            }

            if (R.IsReady() && R.Instance.Name == "ViktorChaosStorm")
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                damage += Player.GetSpellDamage(enemy, SpellSlot.R, 1);
                damage += 4 * Player.GetSpellDamage(enemy, SpellSlot.R, 2);
            }

            if (R.IsReady() && R.Instance.Name != "ViktorChaosStorm")
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R, 2);
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)damage;
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("GapcloserW").GetValue<bool>() && Player.Distance(gapcloser.Sender) < Orbwalking.GetRealAutoAttackRange(Player) && W.IsReady())
            {
                W.Cast(gapcloser.End);
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                var useW = Menu.Item("InterrupterW").GetValue<bool>();
                var useR = Menu.Item("InterrupterR").GetValue<bool>();

                if (useW && W.IsReady() && unit.IsValidTarget(W.Range) &&
                    (Game.Time + 1.5 + W.Delay) >= args.EndTime)
                {
                    if (W.Cast(unit) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                else if (useR && unit.IsValidTarget(R.Range) && R.Instance.Name == "ViktorChaosStorm")
                {
                    E.Cast(unit);
                }
            }
        }

        public static readonly Color NotificationColor = Color.FromArgb(136, 207, 240);

        public static Notification ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            var notif = new Notification(message).SetTextColor(color);
            Notifications.AddNotification(notif);

            if (dispose)
            {
                Utility.DelayAction.Add(duration, () => notif.Dispose());
            }

            return notif;
        }
    }
}
