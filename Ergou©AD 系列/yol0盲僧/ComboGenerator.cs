using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace yol0LeeSin
{
    class ComboGenerator
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static double GetQ2Damage(Obj_AI_Hero target, double dmg)
        {
            var hpafter = target.Health - dmg;
            var qDmg = ((Program._Q.Level * 30) + 20) + (Player.FlatPhysicalDamageMod * 0.9) + (0.08 * (target.MaxHealth - hpafter));
            return Damage.CalcDamage(Player, target, Damage.DamageType.Physical, qDmg);
        }

        private static bool GetComboOption(string option)
        {
            return Program._menu.SubMenu("Combo").Item(option).GetValue<bool>();
        }

        private static bool IsQOne { get { return Program._Q.Instance.Name == "BlindMonkQOne"; } }
        private static bool IsQTwo { get { return Program._Q.Instance.Name == "blindmonkqtwo"; } }
        private static bool IsWOne { get { return Program._W.Instance.Name == "BlindMonkWOne"; } }
        private static bool IsWTwo { get { return Program._W.Instance.Name == "blindmonkwtwo"; } }
        private static bool IsEOne { get { return Program._E.Instance.Name == "BlindMonkEOne"; } }
        private static bool IsETwo { get { return Program._E.Instance.Name == "blindmonketwo"; } }

        private static bool UseQ { get { return GetComboOption("useQ"); } }
        private static bool UseQ2 { get { return GetComboOption("useQ2"); } }
        private static bool UseE { get { return GetComboOption("useE"); } }
        private static bool UseE2 { get { return GetComboOption("useE2"); } }
        private static bool UseR { get { return GetComboOption("useR"); } }
        private static bool UseI { get { return Program._I.Slot != SpellSlot.Unknown && GetComboOption("useI"); } }
        
        public static List<Spell> GetKillCombo(Obj_AI_Hero target)
        {
            var qDmg = IsQOne && Program._Q.IsReady()? Player.GetSpellDamage(target, SpellSlot.Q) : 0.0;
            var eDmg = IsEOne && Program._E.IsReady()? Player.GetSpellDamage(target, SpellSlot.E) : 0.0;
            var rDmg = Program._R.IsReady() ? Player.GetSpellDamage(target, SpellSlot.R) : 0.0;
            var iDmg = Program._I.Slot != SpellSlot.Unknown && Program._I.IsReady() ? Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) : 0.0;
            var aDmg = Player.GetAutoAttackDamage(target);
            var dist = Player.Distance(target.Position) + Player.BoundingRadius + target.BoundingRadius;
            var killCombo = new List<Spell>();
            if (target.HasBuff("BlindMonkQOne"))
            {
                if (target.IsValidTarget(Program._Q2.Range))
                {
                    if (UseQ2 && GetQ2Damage(target, 0) > target.Health)
                    {
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (UseQ2 && GetQ2Damage(target, 0) + iDmg >= target.Health)
                    {
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._I);
                        return killCombo;
                    }
                }

                if (UseE && Program._E.IsReady() && IsEOne && Player.Mana >= 80)
                {
                    if (target.Health <= eDmg + GetQ2Damage(target, eDmg) && dist <= Program._E.Range)
                    {
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (target.Health <= eDmg + GetQ2Damage(target, 0.0) && dist > Program._E.Range)
                    {
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._E);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= eDmg + iDmg + GetQ2Damage(target, eDmg) && dist <= Program._E.Range)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= eDmg + iDmg + GetQ2Damage(target, 0.0) && dist > Program._E.Range)
                    {
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._E);
                        return killCombo;
                    }
                    
                }
                if (UseR && Program._R.IsReady() && Player.Mana >= 30)
                {
                    if (target.Health <= rDmg + GetQ2Damage(target, rDmg) && dist <= Program._R.Range)
                    {
                        killCombo.Add(Program._R);
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (target.Health <= rDmg + GetQ2Damage(target, 0.0) && dist > Program._R.Range)
                    {
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + iDmg + GetQ2Damage(target, rDmg) && dist <= Program._R.Range)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._R);
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + iDmg + GetQ2Damage(target, 0.0) && dist > Program._R.Range)
                    {
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }

                }
                if (UseR && UseE && Program._R.IsReady() && Program._E.IsReady() && IsEOne && Player.Mana >= 80)
                {
                    if (target.Health <= rDmg + eDmg + GetQ2Damage(target, rDmg + eDmg) && dist <= Program._E.Range)
                    {
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._R);
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (target.Health <= rDmg + eDmg + GetQ2Damage(target, rDmg) && dist <= Program._R.Range)
                    {
                        killCombo.Add(Program._R);
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._E);
                        return killCombo;
                    }
                    else if (target.Health <= rDmg + eDmg + GetQ2Damage(target, 0.0) && dist > Program._R.Range)
                    {
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + eDmg + iDmg + GetQ2Damage(target, rDmg + eDmg) && dist <= Program._E.Range)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._R);
                        killCombo.Add(Program._Q2);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + eDmg + iDmg + GetQ2Damage(target, rDmg) && dist <= Program._R.Range)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._R);
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._E);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + eDmg + iDmg + GetQ2Damage(target, 0.0) && dist > Program._R.Range)
                    {
                        
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }

                }
            }
            else
            {
                if (Program._E.IsReady() && IsEOne && dist <= Program._E.Range)
                {
                    if (target.Health <= eDmg)
                    {
                        killCombo.Add(Program._E);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= eDmg + iDmg)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._E);
                        return killCombo;
                    }
                }
                if (Program._R.IsReady() && dist <= Program._R.Range)
                {
                    if (target.Health <= rDmg)
                    {
                        killCombo.Add(Program._R);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + iDmg)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }
                }
                if (Program._R.IsReady() && Program._E.IsReady() && IsEOne && dist <= Program._E.Range)
                {
                    if (target.Health <= rDmg + eDmg)
                    {
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }
                    else if (UseI && Program._I.IsReady() && target.Health <= rDmg + eDmg + iDmg)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._E);
                        killCombo.Add(Program._R);
                        return killCombo;
                    }
                }
                if (Program._Q.IsReady() && IsQOne && dist <= Program._Q.Range)
                {
                    if (target.Health <= qDmg + iDmg && Player.Mana >= 50)
                    {
                        killCombo.Add(Program._I);
                        killCombo.Add(Program._Q);
                        return killCombo;
                    }
                    if (UseI && Program._I.IsReady() && target.Health <= qDmg + iDmg + GetQ2Damage(target, qDmg) && Player.Mana >= 80)
                    {
                        killCombo.Add(Program._Q);
                        killCombo.Add(Program._Q2);
                        killCombo.Add(Program._I);
                        return killCombo;
                    }
                    if (Program._E.IsReady() && IsEOne)
                    {
                        if (UseI && Program._I.IsReady() && target.Health <= qDmg + eDmg + iDmg && Player.Mana >= 100 && dist <= Program._E.Range)
                        {
                            killCombo.Add(Program._I);
                            killCombo.Add(Program._E);
                            killCombo.Add(Program._Q);
                            return killCombo;
                        }
                        else if (UseI && Program._I.IsReady() && target.Health <= qDmg + eDmg + iDmg + GetQ2Damage(target, qDmg + eDmg) && Player.Mana >= 130 && dist <= Program._E.Range)
                        {
                            killCombo.Add(Program._I);
                            killCombo.Add(Program._E);
                            killCombo.Add(Program._Q);
                            killCombo.Add(Program._Q2);
                            return killCombo;
                        }
                        else if (UseI && Program._I.IsReady() && target.Health <= qDmg + eDmg + iDmg + GetQ2Damage(target, qDmg) && Player.Mana >= 130 && dist > Program._E.Range)
                        {
                            killCombo.Add(Program._Q);
                            killCombo.Add(Program._Q2);
                            killCombo.Add(Program._I);
                            killCombo.Add(Program._E);
                            return killCombo;                        }
                    }
                    if (Program._R.IsReady())
                    {
                        if (target.Health <= qDmg + rDmg && Player.Mana >= 50 && dist <= Program._R.Range)
                        {
                            killCombo.Add(Program._R);
                            killCombo.Add(Program._Q);
                            return killCombo;
                        }
                        else if (target.Health <= qDmg + rDmg + GetQ2Damage(target, qDmg + rDmg) && dist <= Program._R.Range)
                        {
                            killCombo.Add(Program._Q);
                            killCombo.Add(Program._R);
                            killCombo.Add(Program._Q2);
                            return killCombo;
                        }
                        else if (UseI && Program._I.IsReady() && target.Health <= qDmg + rDmg + iDmg + GetQ2Damage(target, qDmg + rDmg) && Player.Mana >= 80 && dist <= Program._R.Range)
                        {
                            killCombo.Add(Program._Q);
                            killCombo.Add(Program._I);
                            killCombo.Add(Program._R);
                            killCombo.Add(Program._Q2);
                            return killCombo;
                        }
                        else if (UseI && Program._I.IsReady() && target.Health <= qDmg + rDmg + iDmg + GetQ2Damage(target, qDmg) && Player.Mana >= 80 && dist > Program._R.Range)
                        {
                            killCombo.Add(Program._Q);
                            killCombo.Add(Program._Q2);
                            killCombo.Add(Program._I);
                            killCombo.Add(Program._R);
                            return killCombo;
                        }
                    }
                    if (Program._R.IsReady() && Program._E.IsReady() && IsEOne)
                    {
                        if (target.Health <= qDmg + eDmg + rDmg + GetQ2Damage(target, qDmg + eDmg) && Player.Mana >= 130 && dist <= Program._R.Range)
                        {
                            killCombo.Add(Program._Q);
                            killCombo.Add(Program._E);
                            killCombo.Add(Program._R);
                            killCombo.Add(Program._Q2);
                            return killCombo;
                        }
                    }
                }
            }
            return null;
        }

        public static void PrintCombo(List<Spell> combo)
        {
            var str = "Combo: ";
            foreach (var sp in combo)
            {
                if (sp == Program._Q)
                    str += "Q+";
                if (sp == Program._Q2)
                    str += "Q2+";
                if (sp == Program._E)
                    str += "E+";
                if (sp == Program._R)
                    str += "R+";
                if (sp == Program._I)
                    str += "Ignite+";
            }
            str = str.Remove(str.Length-1);

            Console.WriteLine(str);
        }

        public static string ComboString(List<Spell> combo)
        {
            var str = "";
            foreach (var sp in combo)
            {
                if (sp == Program._Q)
                    str += "Q+";
                if (sp == Program._Q2)
                    str += "Q2+";
                if (sp == Program._E)
                    str += "E+";
                if (sp == Program._R)
                    str += "R+";
                if (sp == Program._I)
                    str += "Ignite+";
            }
            str = str.Remove(str.Length - 1);

            return str;
        }
    }
}
