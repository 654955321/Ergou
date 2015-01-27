using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace BrianSharp.Common
{
    class Survive
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static int WarmingStack = 0, HeatedStack = 0, HeatStack = 0;
        private static bool Hit = false;
        private static float Dmg = 0;

        public static void Init()
        {
            Obj_AI_Hero.OnProcessSpellCast += HeroOnProcessSpellCast;
            Obj_AI_Minion.OnProcessSpellCast += MinionOnProcessSpellCast;
            Obj_AI_Turret.OnProcessSpellCast += TurretOnProcessSpellCast;
            Obj_AI_Turret.OnProcessSpellCast += TurretChargeOnProcessSpellCast;
        }

        public static bool Cast(Spell Skill, int AtHpPer = 0)
        {
            if (!Skill.IsReady()) return false;
            return Cast(Skill.Slot, AtHpPer);
        }

        public static bool Cast(int Id, int AtHpPer = 0)
        {
            if (!Items.HasItem(Id)) return false;
            return Cast(Player.InventoryItems.First(i => i.Id == (ItemId)Id).SpellSlot, AtHpPer, true);
        }

        private static bool Cast(SpellSlot Slot, int AtHpPer = 0, bool IsItem = false)
        {
            if (!Hit) return false;
            var HpPerAfterHit = (Player.Health - (int)Dmg) / Player.MaxHealth * 100;
            var State = false;
            if ((AtHpPer == 0 && HpPerAfterHit <= 10) || (AtHpPer > 0 && HpPerAfterHit <= AtHpPer))
            {
                if (Helper.PacketCast && !IsItem)
                {
                    State = Player.Spellbook.CastSpell(Slot, Player, false);
                }
                else State = Player.Spellbook.CastSpell(Slot, Player);
                if (State)
                {
                    Hit = false;
                    return true;
                }
                else return false;
            }
            return false;
        }

        private static void HeroOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !sender.IsEnemy) return;
            double Sub = 0;
            var Caster = (Obj_AI_Hero)sender;
            var Slot = Caster.GetSpellSlot(args.SData.Name);
            if (args.Target.IsMe)
            {
                if ((Slot == SpellSlot.Summoner1 || Slot == SpellSlot.Summoner2) && args.SData.Name == "summonerdot")
                {
                    Sub = Caster.GetSummonerSpellDamage(Player, Damage.SummonerSpell.Ignite);
                }
                else if (Slot == SpellSlot.Item1 || Slot == SpellSlot.Item2 || Slot == SpellSlot.Item3 || Slot == SpellSlot.Item4 || Slot == SpellSlot.Item5 || Slot == SpellSlot.Item6)
                {
                    if (args.SData.Name.Contains("Bilgewater"))
                    {
                        Sub = Caster.GetItemDamage(Player, Damage.DamageItems.Bilgewater);
                    }
                    else if (args.SData.Name.Contains("Ruined"))
                    {
                        Sub = Caster.GetItemDamage(Player, Damage.DamageItems.Botrk);
                    }
                    else if (args.SData.Name.Contains("Hextech"))
                    {
                        Sub = Caster.GetItemDamage(Player, Damage.DamageItems.Hexgun);
                    }
                }
                else if (Slot == SpellSlot.Q || Slot == SpellSlot.W || Slot == SpellSlot.E || Slot == SpellSlot.R)
                {
                    Sub = Caster.GetSpellDamage(Player, Slot);
                }
                else if (args.SData.IsAutoAttack()) Sub = Caster.GetAutoAttackDamage(Player, true);
            }
            else if (args.Target.NetworkId == sender.NetworkId && Player.Distance(sender, true) <= Math.Pow(450, 2))
            {
                if (Slot == SpellSlot.Item1 || Slot == SpellSlot.Item2 || Slot == SpellSlot.Item3 || Slot == SpellSlot.Item4 || Slot == SpellSlot.Item5 || Slot == SpellSlot.Item6)
                {
                    if (args.SData.Name.Contains("Hydra"))
                    {
                        Sub = Caster.GetItemDamage(Player, Damage.DamageItems.Hydra);
                    }
                    else if (args.SData.Name.Contains("Tiamat"))
                    {
                        Sub = Caster.GetItemDamage(Player, Damage.DamageItems.Tiamat);
                    }
                }
            }
            if (Sub > 0)
            {
                Hit = true;
                Dmg = (float)Sub;
            }
        }

        private static void MinionOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !sender.IsEnemy || !args.Target.IsMe || !args.SData.IsAutoAttack()) return;
            double Sub = 0;
            Sub = sender.GetAutoAttackDamage(Player, true);
            if (Sub > 0)
            {
                Hit = true;
                Dmg = (float)Sub;
            }
        }

        private static void TurretOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !sender.IsEnemy || !args.Target.IsMe || !args.SData.IsAutoAttack()) return;
            double Sub = 0;
            Sub = sender.CalcDamage(Player, Damage.DamageType.Physical, sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);
            if (sender.InventoryItems.Any(i => i.DisplayName == "Penetrating Bullets"))
            {
                Sub = Sub * (1 + 0.375f * WarmingStack + 0.25f * HeatedStack);
            }
            else if (sender.InventoryItems.Any(i => i.DisplayName == "Lightning Rod")) Sub = Sub * (1 + 0.0105f * HeatStack);
            if (Sub > 0)
            {
                Hit = true;
                Dmg = (float)Sub;
            }
        }

        private static void TurretChargeOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !sender.IsEnemy || !args.Target.IsAlly || !Player.UnderTurret()) return;
            if (args.Target.IsMe)
            {
                if (sender.InventoryItems.Any(i => i.DisplayName == "Penetrating Bullets"))
                {
                    if (WarmingStack < 2)
                    {
                        WarmingStack++;
                    }
                    else if (HeatedStack < 2) HeatedStack++;
                }
                if (sender.InventoryItems.Any(i => i.DisplayName == "Lightning Rod"))
                {
                    if (HeatStack < 120) HeatStack += 6;
                }
            }
            else if (args.Target is Obj_AI_Hero)
            {
                WarmingStack = 0;
                HeatStack = 0;
            }
            else
            {
                WarmingStack = 0;
                HeatedStack = 0;
                HeatStack = 0;
            }
        }
    }
}