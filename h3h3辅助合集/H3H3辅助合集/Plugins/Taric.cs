#region LICENSE

// Copyright 2014-2015 Support
// Taric.cs is part of Support.
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
// Filename: Support/Support/Taric.cs
// Created:  01/10/2014
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

    public class Taric : PluginBase
    {
        public Taric()
        {
            Q = new Spell(SpellSlot.Q, 750);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 200);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("ComboHealthQ").Value, Q.Range);
                if (Q.CastCheck(ally, "ComboQ", true, false))
                {
                    Q.Cast(ally);
                }

                if (W.CastCheck(Target, "ComboW"))
                {
                    W.Cast();
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                    E.Cast(Target);
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                    R.Cast();
                }
            }

            if (HarassMode)
            {
                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("HarassHealthQ").Value, Q.Range);
                if (Q.CastCheck(ally, "HarassQ", true, false))
                {
                    Q.Cast(ally);
                }

                if (E.CastCheck(Target, "HarassE"))
                {
                    E.Cast(Target);
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
                E.Cast(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
            {
                return;
            }

            if (E.CastCheck(unit, "InterruptE"))
            {
                E.Cast(unit);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboE", "使用 E", true);
            config.AddBool("ComboR", "使用 R", true);
            config.AddSlider("ComboHealthQ", "使用 Q 治疗", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassE", "使用 E", true);
            config.AddSlider("HarassHealthQ", "使用 Q 治疗", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "使用 E 阻止 突进", true);

            config.AddBool("InterruptE", "使用 E 打断 技能", true);
        }
    }
}