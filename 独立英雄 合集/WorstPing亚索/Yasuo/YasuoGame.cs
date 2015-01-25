using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Yasuo.Evade;

namespace Yasuo
{
    public class YasuoGame
    {
        private int _sweepingBladeDelay;
        public Vector2 DashingEnd;
        public int LastSweepingBladeTick;
        private readonly YasuoMenu menu;
        private readonly Obj_AI_Hero player;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="player">Player Instance</param>
        /// <param name="menu">Menu Instance</param>
        public YasuoGame(Obj_AI_Hero @player, YasuoMenu @menu)
        {
            this.player = player;
            this.menu = menu;
        }

        /// <summary>
        ///     On Game Update
        /// </summary>
        /// <param name="args">System EventArgs</param>
        public void OnGameUpdate(EventArgs args)
        {
            Yasuo.EvadeDetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());
            Evader();

            switch (menu.GetOrbwalker().ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    if (menu.GetValue<KeyBind>(YasuoMenu.FleeKeyName).Active)
                    {
                        Flee();
                    }
                    break;
            }
            Killsteal();
        }

        /// <summary>
        ///     Combo Function
        /// </summary>
        // ReSharper disable once FunctionComplexityOverflow
        private void Combo()
        {
            var packets = menu.GetValue<bool>(YasuoMenu.MiscPacketsName); // => Packets
            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical); // => Target

            var useE = menu.GetValue<bool>(YasuoMenu.ComboEName); // => Use E

            // => E Function
            if (useE && YasuoSpells.E.IsReady())
            {
                var eMode = menu.GetValue<StringList>(YasuoMenu.ComboGapcloserModeName).SelectedIndex;
                // => Gapcloser Mode
                var efMode = menu.GetValue<bool>(YasuoMenu.ComboGapcloserFModeName); // => Gapcloser Follow Mode

                // => Mouse mode
                if (eMode == 0)
                {
                    var dashData = player.GetDashData(Game.CursorPos);
                    if (dashData != null && dashData.Value.ObjAiBase.IsValidTarget())
                    {
                        if (Environment.TickCount - _sweepingBladeDelay > 300)
                        {
                            if (dashData.Value.Vector3.Distance(Game.CursorPos) <
                                Yasuo.Player.Distance(Game.CursorPos) - 100 &&
                                dashData.Value.Vector3.Distance(Game.CursorPos) <
                                Yasuo.Menu.GetValue<Slider>(YasuoMenu.ComboERangeName).Value)
                            {
                                YasuoSpells.E.Cast(dashData.Value.ObjAiBase, packets);
                                DashingEnd = player.GetDashingEnd(dashData.Value.ObjAiBase);
                                _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                            }
                        }
                    }
                }
                else
                {
                    // => Target mode
                    if (!target.IsValidTarget())
                    {
                        return;
                    }


                    if (player.Distance(target.ServerPosition) < player.GetAutoAttackRange() && efMode ||
                        player.Distance(target.ServerPosition) > player.GetAutoAttackRange())
                    {
                        var dashData = player.GetDashData(target.ServerPosition, target);
                        if (dashData != null && dashData.Value.ObjAiBase.IsValidTarget())
                        {
                            if (Environment.TickCount - _sweepingBladeDelay > 300)
                            {
                                if (dashData.Value.Vector3.Distance(target.ServerPosition) <
                                    Yasuo.Player.Distance(target.ServerPosition) - 100 &&
                                    dashData.Value.Vector3.Distance(target.ServerPosition) <
                                    Yasuo.Menu.GetValue<Slider>(YasuoMenu.ComboERangeName).Value)
                                {
                                    YasuoSpells.E.Cast(dashData.Value.ObjAiBase, packets);
                                    DashingEnd = player.GetDashingEnd(dashData.Value.ObjAiBase);
                                    _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                                }
                            }
                        }
                    }
                }
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = menu.GetValue<bool>(YasuoMenu.ComboQName); // => Use Q
            var use3Q = menu.GetValue<bool>(YasuoMenu.Combo3QName); // => Use 3rd Q
            var useR = menu.GetValue<bool>(YasuoMenu.ComboRName); // => Use R

            // => Q Function
            if (useQ && !player.HasWhirlwind() && YasuoSpells.Q.QStage0.IsReady())
            {
                // => Dashing
                if (player.IsDashing())
                {
                    // => Prediction Distance
                    if (player.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f &&
                        DashingEnd.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f)
                    {
                        YasuoSpells.Q.QStage0.Cast(packets);
                    }
                }
                else if (Environment.TickCount - LastSweepingBladeTick > 420)
                {
                    // => Not dashing
                    var targetPosition =
                        Prediction.GetPrediction(target, YasuoSpells.Q.QStage0.Delay, 0f, target.MoveSpeed).UnitPosition;
                    // => Prediction
                    var castPosition =
                        Prediction.GetPrediction(target, YasuoSpells.Q.QStage0.Delay, 0f, YasuoSpells.Q.QStage0.Speed)
                            .CastPosition; // => Prediction

                    // => Prediction Distance
                    if (player.Distance(targetPosition) < YasuoSpells.Q.QStage0.Range)
                    {
                        YasuoSpells.Q.QStage0.Cast(castPosition, packets);
                    }
                }
            }

            // => 3Q Function
            if (use3Q && player.HasWhirlwind() && YasuoSpells.Q.QStage1.IsReady())
            {
                // => Dashing
                if (player.IsDashing())
                {
                    // => Prediction Distance
                    if (player.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f &&
                        DashingEnd.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f)
                    {
                        YasuoSpells.Q.QStage1.Cast(packets);
                    }
                }
                else if (Environment.TickCount - LastSweepingBladeTick > 420)
                {
                    // => Prediction
                    var castPosition =
                        Prediction.GetPrediction(target, YasuoSpells.Q.QStage1.Delay, 0f, YasuoSpells.Q.QStage1.Speed)
                            .CastPosition; // => Prediction

                    // => Prediction Distance
                    if (player.Distance(target.ServerPosition) < YasuoSpells.Q.QStage1.Range)
                    {
                        YasuoSpells.Q.QStage1.Cast(castPosition, packets);
                    }
                }
            }

            // => R Function
            if (useR && YasuoSpells.R.IsReady())
            {
                var rMode = menu.GetValue<StringList>(YasuoMenu.ComboRModeName).SelectedIndex; // => R Mode
                var rMPercent = menu.GetValue<Slider>(YasuoMenu.ComboRPercentName).Value; // => R Min Enemies Health %
                var rSPercent = menu.GetValue<Slider>(YasuoMenu.ComboRPercent2Name).Value; // => R Min Enemy Health %
                var rSelfKnockup = menu.GetValue<bool>(YasuoMenu.ComboRSelfName); // => R only self knockedup enemies
                var rMin = menu.GetValue<Slider>(YasuoMenu.ComboRMinName).Value; // => R Min Enemies to use
                var rKnockupTime = menu.GetValue<Slider>(YasuoMenu.ComboRAirTimeName).Value; // => R Min. Airtime
                var usedR = false;

                if (rMode == 0 || rMode == 2)
                {
                    var targets =
                        ObjectManager.Get<Obj_AI_Hero>().FindAll(t => t.IsValidTarget() && t.IsKnockedup(rSelfKnockup));
                    if (targets.Count() >= rMin)
                    {
                        var totalPercent = targets.Sum(t => t.Health / t.MaxHealth * 100) / targets.Count();
                        if (totalPercent <= rMPercent)
                        {
                            var lowestAirtime = targets.OrderBy(t => Game.Time - t.KnockupTimeLeft()).FirstOrDefault();
                            var formula = (float) rKnockupTime / 100;
                            if (lowestAirtime != null && lowestAirtime.KnockupTimeLeft() <= formula)
                            {
                                YasuoSpells.R.Cast(packets);
                                usedR = true;
                            }
                        }
                    }
                }
                if ((rMode == 1 || rMode == 2) && !usedR)
                {
                    if (target.IsKnockedup(rSelfKnockup))
                    {
                        var totalPercent = target.Health / target.MaxHealth * 100;
                        if (totalPercent <= rSPercent)
                        {
                            var formula = (float) rKnockupTime / 1000;
                            if (target.KnockupTimeLeft() <= formula)
                            {
                                YasuoSpells.R.Cast(packets);
                            }
                        }
                    }
                }
            }

            // => Items
            if (YasuoSpells.Tiamat.GetItem().IsOwned() && menu.GetValue<bool>(YasuoMenu.ComboItemsTiamatName))
            {
                // => Tiamat
                var item = YasuoSpells.Tiamat.GetItem();
                var range = YasuoSpells.Tiamat.Range;

                if (item.IsReady())
                {
                    if (player.Distance(target) < range)
                    {
                        item.Cast();
                    }
                }
            }
            else if (YasuoSpells.RavenousHydra.GetItem().IsOwned() && menu.GetValue<bool>(YasuoMenu.ComboItemsHydraName))
            {
                // => Hydra
                var item = YasuoSpells.RavenousHydra.GetItem();
                var range = YasuoSpells.RavenousHydra.Range;

                if (item.IsReady())
                {
                    if (player.Distance(target) < range)
                    {
                        item.Cast();
                    }
                }
            }

            if (YasuoSpells.BilgewaterCutlass.GetItem().IsOwned() &&
                menu.GetValue<bool>(YasuoMenu.ComboItemsBilgewaterName))
            {
                // => Bilgewater
                var item = YasuoSpells.BilgewaterCutlass.GetItem();
                var range = YasuoSpells.BilgewaterCutlass.Range;

                if (item.IsReady())
                {
                    if (player.Distance(target) < range)
                    {
                        item.Cast(target);
                    }
                }
            }
            else if (YasuoSpells.BladeoftheRuinedKing.GetItem().IsOwned() &&
                     menu.GetValue<bool>(YasuoMenu.ComboItemsBotRkName))
            {
                // => BotRK
                var item = YasuoSpells.BladeoftheRuinedKing.GetItem();
                var range = YasuoSpells.BladeoftheRuinedKing.Range;

                if (item.IsReady())
                {
                    if (player.Distance(target) < range)
                    {
                        item.Cast(target);
                    }
                }
            }
        }

        /// <summary>
        ///     Lane Clear Function
        /// </summary>
        private void LaneClear()
        {
            if (LastHit(true))
            {
                return;
            }

            var useQ = menu.GetValue<bool>(YasuoMenu.FarmingLaneClearQName);
            var use3Q = menu.GetValue<bool>(YasuoMenu.FarmingLaneClear3QName);

            if (!useQ && !use3Q)
            {
                return;
            }

            var minions = ObjectManager.Get<Obj_AI_Minion>().FindAll(m => m.IsValidTarget(1200f)).ToList();
            var packets = menu.GetValue<bool>(YasuoMenu.MiscPacketsName); // => Packets

            if (useQ && YasuoSpells.Q.IsReady() && !player.HasWhirlwind())
            {
                var qMinions = minions.FindAll(m => m.IsValidTarget(YasuoSpells.Q.QStage0.Range));
                if (qMinions.Any())
                {
                    var qOnAa = menu.GetValue<bool>(YasuoMenu.FarmingLaneClearQaaName);
                    if (qOnAa || !ShouldWait())
                    {
                        var qMinion = qMinions.OrderBy(m => m.Health).FirstOrDefault();
                        if (qMinion != null && Environment.TickCount - LastSweepingBladeTick > 420 &&
                            !player.IsDashing())
                        {
                            if (player.Distance(qMinion.ServerPosition) < YasuoSpells.Q.QStage0.Range)
                            {
                                YasuoSpells.Q.QStage0.Cast(qMinion, packets);
                                return;
                            }
                        }
                    }
                }
            }

            if (use3Q && YasuoSpells.Q.IsReady() && player.HasWhirlwind())
            {
                var qMinions = minions.FindAll(m => m.IsValidTarget(YasuoSpells.Q.QStage1.Range));
                if (qMinions.Any())
                {
                    var qMinion = qMinions.OrderBy(m => m.Health).LastOrDefault();
                    if (qMinion != null && Environment.TickCount - LastSweepingBladeTick > 420 && !player.IsDashing())
                    {
                        if (player.Distance(qMinion.ServerPosition) < YasuoSpells.Q.QStage0.Range)
                        {
                            YasuoSpells.Q.QStage1.Cast(qMinion, packets);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Last Hit Function
        /// </summary>
        /// <param name="laneclear"></param>
        /// <returns></returns>
        private bool LastHit(bool laneclear = false)
        {
            var useQ = (laneclear)
                ? menu.GetValue<bool>(YasuoMenu.FarmingLaneClearQName)
                : menu.GetValue<bool>(YasuoMenu.FarmingLastHitQName);
            var use3Q = (laneclear)
                ? menu.GetValue<bool>(YasuoMenu.FarmingLaneClear3QName)
                : menu.GetValue<bool>(YasuoMenu.FarmingLastHit3QName);
            var useE = (laneclear)
                ? menu.GetValue<bool>(YasuoMenu.FarmingLaneClearEName)
                : menu.GetValue<bool>(YasuoMenu.FarmingLastHitEName);

            if (!useQ && !use3Q && !useE)
            {
                return false;
            }

            // Preload minions
            var minions = ObjectManager.Get<Obj_AI_Minion>().FindAll(m => m.IsValidTarget(1200f)).ToList();
            var packets = menu.GetValue<bool>(YasuoMenu.MiscPacketsName); // => Packets

            // => E Function
            if (useE && YasuoSpells.E.IsReady())
            {
                var eMinions =
                    minions.FindAll(
                        m => m.IsValidTarget(YasuoSpells.E.Range) && m.Health < player.GetSweepingBladeDamage(m));
                if (eMinions.Any())
                {
                    var eOnAa = (laneclear)
                        ? menu.GetValue<bool>(YasuoMenu.FarmingLaneClearEaaName)
                        : menu.GetValue<bool>(YasuoMenu.FarmingLastHitEaaName);
                    if (eOnAa || !ShouldWait())
                    {
                        var eMinion = eMinions.OrderBy(m => m.Health).FirstOrDefault();
                        if (eMinion != null)
                        {
                            var underTower = player.GetDashingEnd(eMinion).To3D().UnderTurret(true);
                            var shouldDive = (laneclear)
                                ? menu.GetValue<bool>(YasuoMenu.FarmingLaneClearTurretName)
                                : menu.GetValue<bool>(YasuoMenu.FarmingLastHitTurretName);
                            if (underTower && shouldDive)
                            {
                                // TODO: Smarter function
                                YasuoSpells.E.Cast(eMinion, packets);
                                _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                                return true;
                            }

                            if (!underTower)
                            {
                                YasuoSpells.E.Cast(eMinion, packets);
                                _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                                return true;
                            }
                        }
                    }
                }
            }

            // => Q Function
            if (useQ && YasuoSpells.Q.IsReady() && !player.HasWhirlwind())
            {
                var qMinions =
                    minions.FindAll(
                        m => m.IsValidTarget(YasuoSpells.Q.QStage0.Range) && m.Health < player.GetSteelTempestDamage(m));
                if (qMinions.Any())
                {
                    var qOnAa = (laneclear)
                        ? menu.GetValue<bool>(YasuoMenu.FarmingLaneClearQaaName)
                        : menu.GetValue<bool>(YasuoMenu.FarmingLastHitQaaName);
                    if (qOnAa || !ShouldWait())
                    {
                        var qMinion = qMinions.OrderBy(m => m.Health).FirstOrDefault();
                        if (qMinion != null && Environment.TickCount - LastSweepingBladeTick > 420 &&
                            !player.IsDashing())
                        {
                            if (player.Distance(qMinion.ServerPosition) < YasuoSpells.Q.QStage0.Range - 20f)
                            {
                                YasuoSpells.Q.QStage0.Cast(qMinion, packets);
                                return true;
                            }
                        }
                    }
                }
            }

            if (use3Q && YasuoSpells.Q.IsReady() && player.HasWhirlwind())
            {
                var qMinions =
                    minions.FindAll(
                        m => m.IsValidTarget(YasuoSpells.Q.QStage1.Range) && m.Health < player.GetSteelTempestDamage(m));
                if (qMinions.Any())
                {
                    var qMinion = qMinions.OrderBy(m => m.Health).FirstOrDefault();
                    if (qMinion != null && Environment.TickCount - LastSweepingBladeTick > 420 && !player.IsDashing())
                    {
                        if (player.Distance(qMinion.ServerPosition) < YasuoSpells.Q.QStage0.Range - 20f)
                        {
                            YasuoSpells.Q.QStage1.Cast(qMinion, packets);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Flee Function
        /// </summary>
        private void Flee()
        {
            if (!menu.GetValue<bool>(YasuoMenu.FleeEnableName))
            {
                return;
            }

            if (YasuoSpells.E.IsReady())
            {
                var dashData = player.GetDashData(Game.CursorPos);
                if (dashData != null && dashData.Value.ObjAiBase.IsValidTarget())
                {
                    if (Environment.TickCount - _sweepingBladeDelay > 300)
                    {
                        if (dashData.Value.Vector3.Distance(Game.CursorPos) <
                            Yasuo.Player.Distance(Game.CursorPos) - 100 &&
                            dashData.Value.Vector3.Distance(Game.CursorPos) < 250)
                        {
                            YasuoSpells.E.Cast(dashData.Value.ObjAiBase, menu.GetValue<bool>(YasuoMenu.MiscPacketsName));
                            DashingEnd = player.GetDashingEnd(dashData.Value.ObjAiBase);
                            _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                        }
                    }
                }
            }

            player.MoveTo(Game.CursorPos, Yasuo.Player.BoundingRadius * 2f);
        }

        /// <summary>
        ///     Killsteal Function
        /// </summary>
        private void Killsteal()
        {
            var useks = menu.GetValue<bool>(YasuoMenu.KillstealEnabledName);
            if (!useks)
            {
                return;
            }

            var packets = menu.GetValue<bool>(YasuoMenu.MiscPacketsName); // => Packets
            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical); // => Target

            if (target == null)
            {
                return;
            }

            var useE = menu.GetValue<bool>(YasuoMenu.KillstealEName); // => Use E
            var useQ = menu.GetValue<bool>(YasuoMenu.KillstealQName); // => Use Q
            var use3Q = menu.GetValue<bool>(YasuoMenu.Killsteal3QName); // => Use 3rd Q

            // => E Function
            if (useE && YasuoSpells.E.IsReady() && player.GetSweepingBladeDamage(target) > target.Health)
            {
                var underTower = player.GetDashingEnd(target).To3D().UnderTurret(true);
                var shouldDive = menu.GetValue<bool>(YasuoMenu.KillstealEIntoTowerName);
                if (underTower && shouldDive)
                {
                    // TODO: Smarter function
                    YasuoSpells.E.Cast(target, packets);
                    _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                }

                if (!underTower)
                {
                    YasuoSpells.E.Cast(target, packets);
                    _sweepingBladeDelay = LastSweepingBladeTick = Environment.TickCount;
                }
            }

            // => Q Function
            if (useQ && !player.HasWhirlwind() && YasuoSpells.Q.QStage0.IsReady() &&
                player.GetSteelTempestDamage(target) > target.Health)
            {
                // => Dashing
                if (player.IsDashing())
                {
                    // => Prediction Distance
                    if (player.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f &&
                        DashingEnd.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f)
                    {
                        YasuoSpells.Q.QStage0.Cast(packets);
                    }
                }
                else if (Environment.TickCount - LastSweepingBladeTick > 420)
                {
                    // => Not dashing
                    var targetPosition =
                        Prediction.GetPrediction(target, YasuoSpells.Q.QStage0.Delay, 0f, target.MoveSpeed).UnitPosition;
                    // => Prediction
                    var castPosition =
                        Prediction.GetPrediction(target, YasuoSpells.Q.QStage0.Delay, 0f, YasuoSpells.Q.QStage0.Speed)
                            .CastPosition; // => Prediction

                    // => Prediction Distance
                    if (player.Distance(targetPosition) < YasuoSpells.Q.QStage0.Range)
                    {
                        YasuoSpells.Q.QStage0.Cast(castPosition, packets);
                    }
                }
            }

            // => 3Q Function
            if (use3Q && player.HasWhirlwind() && YasuoSpells.Q.QStage1.IsReady() &&
                player.GetSteelTempestDamage(target) > target.Health)
            {
                // => Dashing
                if (player.IsDashing())
                {
                    // => Prediction Distance
                    if (player.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f &&
                        DashingEnd.Distance(target.ServerPosition) < YasuoSpells.Q.DashingRange - 50f)
                    {
                        YasuoSpells.Q.QStage1.Cast(packets);
                    }
                }
                else if (Environment.TickCount - LastSweepingBladeTick > 420)
                {
                    // => Prediction
                    var castPosition =
                        Prediction.GetPrediction(target, YasuoSpells.Q.QStage1.Delay, 0f, YasuoSpells.Q.QStage1.Speed)
                            .CastPosition; // => Prediction

                    // => Prediction Distance
                    if (player.Distance(target.ServerPosition) < YasuoSpells.Q.QStage1.Range)
                    {
                        YasuoSpells.Q.QStage1.Cast(castPosition, packets);
                    }
                }
            }
        }

        /// <summary>
        ///     Should wait function
        /// </summary>
        /// <returns>Should wait for a minion</returns>
        private bool ShouldWait()
        {
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Any(
                        minion =>
                            minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
                            player.Distance(minion.ServerPosition) < player.GetAutoAttackRange() &&
                            HealthPrediction.LaneClearHealthPrediction(
                                minion, (int) ((player.AttackDelay * 1000) * 2f), 0) <=
                            player.GetAutoAttackDamage(minion));
        }

        private static void Evader()
        {
            foreach (var skillshot in Yasuo.EvadeDetectedSkillshots)
            {
                var flag = false;
                if (YasuoSpells.E.IsReady())
                {
                    if (Yasuo.Menu.GetValue<bool>(YasuoMenu.EvadeUseLoc))
                    {
                        flag = Evade(skillshot);
                    }
                }

                if (flag)
                {
                    continue;
                }

                if (YasuoSpells.W.IsReady())
                {
                    if (Yasuo.Menu.GetValue<bool>(YasuoMenu.AutoWindWallUseLoc))
                    {
                        Windwall(skillshot);
                    }
                }
            }
        }

        private static bool Evade(Skillshot skillshot)
        {
            if (YasuoSpells.E.IsReady())
            {
                // => If anything, execute the windwall.
                var flag = true;

                foreach (var ss in Yasuo.MenuDashesList.Where(ss => ss.SpellName == skillshot.SpellData.SpellName))
                {
                    flag =
                        Yasuo.Menu.GetValue<bool>(
                            ((ss.IsWindwall) ? YasuoMenu.AutoWindWallLoc : YasuoMenu.EvadeLoc) + "." + ss.ChampionName +
                            "." + ss.Slot);
                    break;
                }

                if (flag)
                {
                    // => Will skillshot collide with Yasuo?
                    if (Yasuo.Player.Position.To2D().ProjectOn(skillshot.Start, skillshot.End).IsOnSegment)
                    {
                        var minions =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(m => m.IsValidTarget() && Yasuo.Player.IsDashable(m));
                        var safeMinions =
                            minions.Where(
                                m =>
                                    !Yasuo.Player.GetDashingEnd(m).ProjectOn(skillshot.Start, skillshot.End).IsOnSegment &&
                                    !Yasuo.Player.GetDashingEnd(m).To3D().UnderTurret(true));

                        var objAiMinions = safeMinions as Obj_AI_Minion[] ?? safeMinions.ToArray();
                        if (objAiMinions.Any())
                        {
                            var minion = objAiMinions.OrderBy(m => m.Distance(Yasuo.Player.Position)).FirstOrDefault();
                            if (minion != null)
                            {
                                YasuoSpells.E.Cast(minion, Yasuo.Menu.GetValue<bool>(YasuoMenu.MiscPacketsName));

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static void Windwall(Skillshot skillshot)
        {
            if (YasuoSpells.W.IsReady() && skillshot.SpellData.Type != SkillShotType.SkillshotCircle ||
                skillshot.SpellData.Type != SkillShotType.SkillshotRing)
            {
                var isAboutToHitRange = Yasuo.Menu.GetValue<Slider>(YasuoMenu.AutoWindWallDelayLoc).Value;

                // => If anything, execute the windwall.
                var flag =
                    Yasuo.MenuWallsList.Where(ss => ss.SpellName == skillshot.SpellData.SpellName)
                        .Select(
                            ss =>
                                Yasuo.Menu.GetValue<bool>(
                                    ((ss.IsWindwall) ? YasuoMenu.AutoWindWallLoc : YasuoMenu.EvadeLoc) + "." +
                                    ss.ChampionName + "." + ss.Slot))
                        .FirstOrDefault();

                if (flag)
                {
                    if (skillshot.IsAboutToHit(isAboutToHitRange, Yasuo.Player))
                    {
                        var cast = Yasuo.Player.ServerPosition +
                                   Vector3.Normalize(skillshot.MissilePosition.To3D() - Yasuo.Player.ServerPosition) *
                                   10;
                        YasuoSpells.W.Cast(cast, Yasuo.Menu.GetValue<bool>(YasuoMenu.MiscPacketsName));
                    }
                }
            }
        }
    }
}