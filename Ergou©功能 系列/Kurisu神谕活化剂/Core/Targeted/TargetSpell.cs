#region LICENSE

// Copyright 2014 - 2014 SpellDetector
// TargetSpell.cs is part of SpellDetector.
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
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Oracle.Core.Targeted
{
    public class TargetSpell
    {
        public Obj_AI_Hero Caster { get; set; }
        public Obj_AI_Base Target { get; set; }
        public TargetSpellData Spell { get; set; }
        public int StartTick { get; set; }
        public Vector2 StartPosition { get; set; }

        public int EndTick
        {
            get { return (int) (StartTick + Spell.Delay + 1000*(StartPosition.Distance(EndPosition)/Spell.Speed)); }
        }

        public Vector2 EndPosition
        {
            get { return Target.ServerPosition.To2D(); }
        }

        public Vector2 Direction
        {
            get { return (EndPosition - StartPosition).Normalized(); }
        }

        public double Damage
        {
            get { return Caster.GetSpellDamage(Target, Spell.Name); }
        }

        public bool IsKillable
        {
            get { return Damage >= Target.Health; }
        }

        public CcType CrowdControl
        {
            get { return Spell.CcType; }
        }

        public SpellType Type
        {
            get { return Spell.Type; }
        }

        public bool IsActive
        {
            get { return Environment.TickCount <= EndTick; }
        }

        public bool HasMissile
        {
            get { return Spell.Speed == float.MaxValue || Spell.Speed == 0; }
        }
    }

    public class TargetSpellData
    {
        public string ChampionName { get; set; }
        public SpellSlot Spellslot { get; set; }
        public SpellType Type { get; set; }
        public CcType CcType { get; set; }
        public string Name { get; set; }
        public float Range { get; set; }
        public int Delay { get; set; }
        public int Speed { get; set; }
        public bool Dangerous { get; set; }
        public bool Stealth { get; set; }
        public bool Wait { get; set; }

        public TargetSpellData(string champion, string name, SpellSlot slot, SpellType type, CcType cc, float range,
            int delay, int speed, bool dangerous = false, bool stealth = false, bool wait = false)
        {
            ChampionName = champion;
            Name = name;
            Spellslot = slot;
            Type = type;
            CcType = cc;
            Range = range;
            Speed = speed;
            Delay = delay;
            Dangerous = dangerous;
            Stealth = stealth;
            Wait = wait;
        }
    }

    public enum SpellType
    {
        Skillshot,
        Targeted,
        Self,
        AutoAttack
    }

    public enum SpellSlot
    {
        Unknown = -1,
        Q = 0,
        W = 1,
        E = 2,
        R = 3,
        Summoner1 = 4,
        Summoner2 = 5,
        Recall = 13,
    }

    public enum CcType
    {
        No,
        Stun,
        Silence,
        Taunt,
        Polymorph,
        Slow,
        Snare,
        Fear,
        Charm,
        Suppression,
        Blind,
        Flee,
        Knockup,
        Knockback,
        Pull
    }
}