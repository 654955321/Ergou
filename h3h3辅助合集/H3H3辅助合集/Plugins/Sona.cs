#region LICENSE

// Copyright 2014-2015 Support
// Sona.cs is part of Support.
// 
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.
// 
// Filename: Support/Support/Sona.cs
// Created:  15/10/2014
// Date:     20/01/2015/11:20
// Author:   h3h3

#endregion

namespace Support.Plugins
{
    #region

    using System;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Support.Util;
    using ActiveGapcloser = Support.Util.ActiveGapcloser;

    #endregion

    public class Sona : PluginBase
    {
        public Sona()
        {
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 1000);

            R.SetSkillshot(0.5f, 125, float.MaxValue, false, SkillshotType.SkillshotLine);
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "ComboQ"))
                    {
                        Q.Cast();
                    }

                    //if (Target.IsValidTarget(AttackRange) &&
                    //    (Player.HasBuff("sonaqprocattacker") || Player.HasBuff("sonaqprocattacker")))
                    //{
                    //    Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                    //}

                    var allyW = Helpers.AllyBelowHp(ConfigValue<Slider>("ComboHealthW").Value, W.Range);
                    if (W.CastCheck(allyW, "ComboW", true, false))
                    {
                        W.Cast();
                    }

                    if (E.IsReady() && Helpers.AllyInRange(E.Range).Count > 0 && ConfigValue<bool>("ComboE"))
                    {
                        E.Cast();
                    }

                    if (R.CastCheck(Target, "ComboR"))
                    {
                        R.CastIfWillHit(Target, ConfigValue<Slider>("ComboCountR").Value, true);
                    }
                }

                if (HarassMode)
                {
                    if (Q.CastCheck(Target, "HarassQ"))
                    {
                        Q.Cast();
                    }

                    var allyW = Helpers.AllyBelowHp(ConfigValue<Slider>("HarassHealthW").Value, W.Range);
                    if (W.CastCheck(allyW, "HarassW", true, false))
                    {
                        W.Cast();
                    }

                    if (E.IsReady() && Helpers.AllyInRange(E.Range).Count > 0 && ConfigValue<bool>("HarassE"))
                    {
                        E.Cast();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                R.Cast(Target, true);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
            {
                return;
            }

            if (R.CastCheck(unit, "InterruptR"))
            {
                R.Cast(Target, true);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboE", "使用 E", true);
            config.AddBool("ComboR", "使用 R", true);
            config.AddSlider("ComboCountR", "大招范围内人数", 3, 1, 5);
            config.AddSlider("ComboHealthW", "使用 W 治疗", 80, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassW", "使用 W", true);
            config.AddBool("HarassE", "使用 E", true);
            config.AddSlider("HarassHealthW", "使用 W 治疗", 60, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserR", "使用 R 阻止 突进", false);

            config.AddBool("InterruptR", "使用 R 打断 法术", true);
        }
    }
}