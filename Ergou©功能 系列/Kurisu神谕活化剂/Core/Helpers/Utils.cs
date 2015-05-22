#region LICENSE

// Copyright 2014 - 2014 SpellDetector
// Utils.cs is part of SpellDetector.
// SpellDetector is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// SpellDetector is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with SpellDetector. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Oracle.Core.Helpers
{
    public class Utils
    {
        public static void Load()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Oracle.IncomeDamage >= 1)
                Utility.DelayAction.Add(Game.Ping + 50, () => Oracle.IncomeDamage = 0);
            if (Oracle.MinionDamage >= 1)
                Utility.DelayAction.Add(Game.Ping + 50, () => Oracle.MinionDamage = 0);
            if (Oracle.Danger)
                Utility.DelayAction.Add(Game.Ping + 130, () => Oracle.Danger = false);
            if (Oracle.Dangercc)
                Utility.DelayAction.Add(Game.Ping + 130, () => Oracle.Dangercc = false);
            if (Oracle.DangerUlt)
                Utility.DelayAction.Add(Game.Ping + 130, () => Oracle.DangerUlt = false);
            if (Oracle.Spell)
                Utility.DelayAction.Add(Game.Ping + 130, () => Oracle.Spell = false);
        }
    }

    internal class GameBuff
    {
        public string ChampionName { get; set; }
        public string BuffName { get; set; }
        public SpellSlot Slot { get; set; }
        public string SpellName { get; set; }
        public int Delay { get; set; }

        public static readonly List<GameBuff> EvadeBuffs = new List<GameBuff>();
        public static readonly List<GameBuff> CleanseBuffs = new List<GameBuff>();


        static GameBuff()
        {
            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Braum",
                BuffName = "braummark",
                SpellName = "braumq",
                Slot = SpellSlot.Q,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Zed",
                BuffName = "zedulttargetmark",
                SpellName = "zedult",
                Slot = SpellSlot.R,
                Delay = 1800
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Fizz",
                BuffName = "fizzmarinerdoombomb",
                SpellName = "fizzmarinerdoom",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Leblanc",
                BuffName = "leblancsoulshackle",
                SpellName = "leblancsoulshackle",
                Slot = SpellSlot.E,
                Delay = 500
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "LeeSin",
                BuffName = "blindmonkqonechaos",
                SpellName = "blindmonkqone",
                Slot = SpellSlot.Q,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Leblanc",
                BuffName = "leblancsoulshacklem",
                SpellName = "leblancsoulshacklem",
                Slot = SpellSlot.R,
                Delay = 500
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Nasus",
                BuffName = "NasusW",
                SpellName = "NasusW",
                Slot = SpellSlot.W,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Mordekaiser",
                BuffName = "mordekaiserchildrenofthegrave",
                SpellName = "mordekaiserchildrenofthegrave",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Poppy",
                BuffName = "poppydiplomaticimmunity",
                SpellName = "poppydiplomaticimmunity",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Skarner",
                BuffName = "skarnerimpale",
                SpellName = "skarnerimpale",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Urgot",
                BuffName = "urgotswap2",
                SpellName = "urgotswap2",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Vladimir",
                BuffName = "vladimirhemoplague",
                SpellName = "vladimirhemoplague",
                Slot = SpellSlot.R,
                Delay = 2000
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Morgana",
                BuffName = "soulshackles",
                SpellName = "soulshackles",
                Slot = SpellSlot.R,
                Delay = 1000
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Karthus",
                BuffName = "fallenonetarget",
                SpellName = "fallenone",
                Slot = SpellSlot.R,
                Delay = 2500
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Morgana",
                BuffName = "soulshackles",
                SpellName = "soulshackles",
                Slot = SpellSlot.R,
                Delay = 2500
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Vladimir",
                BuffName = "vladimirhemoplague",
                SpellName = "vladimirhemoplague",
                Slot = SpellSlot.R,
                Delay = 4500
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Zed",
                BuffName = "zedulttargetmark",
                SpellName = "zedult",
                Slot = SpellSlot.R,
                Delay = 2800
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Caitlyn",
                BuffName = "caitlynaceinthehole",
                SpellName = "caitlynaceinthehole",
                Slot = SpellSlot.R,
                Delay = 1000
            });
        }
    }

    public class SpellList<T> : List<T>
    {
        public event EventHandler OnAdd;
        public event EventHandler OnRemove;

        public new void Add(T item)
        {
            if (OnAdd != null)
            {
                OnAdd(this, null); // TODO: return item
            }

            base.Add(item);
        }

        public new void Remove(T item)
        {
            if (OnRemove != null)
            {
                OnRemove(this, null); // TODO: return item
            }

            base.Remove(item);
        }

        public new void RemoveAll(Predicate<T> match)
        {
            if (OnRemove != null)
            {
                OnRemove(this, null); // TODO: return items
            }

            base.RemoveAll(match);
        }
    }
}