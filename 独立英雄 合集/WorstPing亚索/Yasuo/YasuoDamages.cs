using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Yasuo
{
    public static class YasuoDamages
    {
        /// <summary>
        ///     Calculates Yasuo's Steel Tempest (Q) damage.
        /// </summary>
        /// <param name="player">Player Object</param>
        /// <param name="target">Target Object</param>
        /// <returns>Steel Tempest Damage</returns>
        public static double GetSteelTempestDamage(this Obj_AI_Hero player, Obj_AI_Base target)
        {
            // Damage
            double damage;

            // => Crit
            if ((int) player.Crit == 1)
            {
                var ie = ItemData.Infinity_Edge.GetItem().IsOwned();
                if (ie)
                {
                    var critDamage = (20 * YasuoSpells.Q.QStage0.Level) + player.TotalAttackDamage() * 1.875;
                    damage = CalcPhysicalDamage(player, target, critDamage);
                }
                else
                {
                    var critDamage = (20 * YasuoSpells.Q.QStage0.Level) + player.TotalAttackDamage() * 1.50;
                    damage = CalcPhysicalDamage(player, target, critDamage);
                }
            }
            else
            {
                damage = player.GetSpellDamage(target, SpellSlot.Q);
            }

            // => Stattik
            if ((player.HasBuff("ItemStatikShankCharge") && !player.HasWhirlwind()) ||
                (player.HasBuff("ItemStatikShankCharge") && player.IsDashing()))
            {
                var magicResist = (target.SpellBlock * player.PercentMagicPenetrationMod) -
                                  player.FlatMagicPenetrationMod;

                double k;
                if (magicResist < 0)
                {
                    k = 2 - 100 / (100 - magicResist);
                }
                else
                {
                    k = 100 / (100 + magicResist);
                }

                //Take into account the percent passives
                k = PassivePercentMod(player, target, k);

                k = k * (1 - target.PercentMagicReduction) * (1 + target.PercentMagicDamageMod);

                damage += k * 100;
            }

            // TODO: Add Tiamat/Hydra

            // Return calculated damage
            return damage;
        }

        /// <summary>
        ///     Calculates Yasuo's Sweeping Blade (E) damage.
        /// </summary>
        /// <param name="player">Player Object</param>
        /// <param name="target">Target Object</param>
        /// <returns>Sweeping Blade Damage</returns>
        public static double GetSweepingBladeDamage(this Obj_AI_Hero player, Obj_AI_Base target)
        {
            var stacksPassive = Yasuo.Player.Buffs.Find(b => b.DisplayName.Equals("YasuoDashScalar"));
            var stacks = 1 + 0.25 * ((stacksPassive != null) ? stacksPassive.Count : 0);
            var damage = 50 + (20 * YasuoSpells.E.Level * stacks + (player.FlatMagicDamageMod * 0.6));
            return player.CalcDamage(target, Damage.DamageType.Magical, damage);
        }

        private static double CalcPhysicalDamage(Obj_AI_Base source, Obj_AI_Base target, double amount)
        {
            double armorPenPercent = source.PercentArmorPenetrationMod;
            double armorPenFlat = source.FlatArmorPenetrationMod;

            //Minions return wrong percent values.
            if (source is Obj_AI_Minion)
            {
                armorPenFlat = 0;
                armorPenPercent = 1;
            }

            //Turrets passive.
            if (source is Obj_AI_Turret)
            {
                armorPenFlat = 0;
                armorPenPercent = 0.7f; //Penetrating Bullets passive.
            }

            var armor = (target.Armor * armorPenPercent) - armorPenFlat;

            double k;
            if (armor < 0)
            {
                k = 2 - 100 / (100 - armor);
            }
            else
            {
                k = 100 / (100 + armor);
            }

            //Take into account the percent passives
            k = PassivePercentMod(source, target, k);

            return k * amount + PassiveFlatMod(source, target);
        }

        private static double PassivePercentMod(Obj_AI_Base source, Obj_AI_Base target, double k)
        {
            var siegeMinionList = new List<string> { "Red_Minion_MechCannon", "Blue_Minion_MechCannon" };
            var normalMinionList = new List<string>
            {
                "Red_Minion_Wizard",
                "Blue_Minion_Wizard",
                "Red_Minion_Basic",
                "Blue_Minion_Basic"
            };

            //Minions and towers passives:
            if (source is Obj_AI_Turret)
            {
                //Siege minions receive 70% damage from turrets
                if (siegeMinionList.Contains(target.BaseSkinName))
                {
                    k = 0.7d * k;
                }

                //Normal minions take 114% more damage from towers.
                else if (normalMinionList.Contains(target.BaseSkinName))
                {
                    k = (1 / 0.875) * k;
                }

                // Turrets deal 105% damage to champions for the first attack.
                else if (target is Obj_AI_Hero)
                {
                    k = 1.05 * k;
                }
            }

            //Masteries:

            //Offensive masteries:
            var hero = source as Obj_AI_Hero;
            if (hero != null)
            {
                var sourceAsHero = hero;

                //Double edge sword:
                //  Melee champions: You deal 2% increase damage from all sources, but take 1% increase damage from all sources.
                //  Ranged champions: You deal and take 1.5% increased damage from all sources. 
                if (sourceAsHero.Masteries.Any(m => m.Page == MasteryPage.Offense && m.Id == 65 && m.Points == 1))
                {
                    if (sourceAsHero.CombatType == GameObjectCombatType.Melee)
                    {
                        k = k * 1.02d;
                    }
                    else
                    {
                        k = k * 1.015d;
                    }
                }

                //Havoc:
                //  Increases damage by 3%. 
                if (sourceAsHero.Masteries.Any(m => m.Page == MasteryPage.Offense && m.Id == 146 && m.Points == 1))
                {
                    k = k * 1.03d;
                }

                //Executioner
                //  Increases damage dealt to champions below 20 / 35 / 50% by 5%. 
                if (target is Obj_AI_Hero)
                {
                    var mastery =
                        (sourceAsHero).Masteries.FirstOrDefault(m => m.Page == MasteryPage.Offense && m.Id == 100);
                    if (mastery != null && mastery.Points >= 1 &&
                        target.Health / target.MaxHealth <= 0.05d + 0.15d * mastery.Points)
                    {
                        k = k * 1.05;
                    }
                }
            }


            if (!(target is Obj_AI_Hero))
            {
                return k;
            }

            var targetAsHero = (Obj_AI_Hero) target;

            //Defensive masteries:

            //Double edge sword:
            //     Melee champions: You deal 2% increase damage from all sources, but take 1% increase damage from all sources.
            //     Ranged champions: You deal and take 1.5% increased damage from all sources. 
            if (targetAsHero.Masteries.Any(m => m.Page == MasteryPage.Offense && m.Id == 65 && m.Points == 1))
            {
                if (target.CombatType == GameObjectCombatType.Melee)
                {
                    k = k * 1.01d;
                }
                else
                {
                    k = k * 1.015d;
                }
            }

            return k;
        }

        private static double PassiveFlatMod(Obj_AI_Base source, Obj_AI_Base target)
        {
            double d = 0;

            if (!(source is Obj_AI_Hero))
            {
                return d;
            }

            //Offensive masteries:

            //Butcher
            //  Basic attacks and single target abilities do 2 bonus damage to minions and monsters. 
            if (target is Obj_AI_Minion)
            {
                if (
                    ((Obj_AI_Hero) source).Masteries.Any(
                        m => m.Page == MasteryPage.Offense && m.Id == 65 && m.Points == 1))
                {
                    d = d + 2;
                }
            }

            return d;
        }
    }
}