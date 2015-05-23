using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ElAlistarReborn
{
    /// <summary>
    /// ElAlistar:Reborn by jQuery - BETA
    /// Version 1.0.0.0
    /// 
    /// Combo
    /// Q, W, E, R
    /// Auto ignite when target is killable
    /// 
    /// Harass
    /// Q
    /// 
    /// Drawings (Misc)
    /// Draws combo damage
    /// Q, W, E, R ranges
    /// 
    /// 
    /// Updated and tested 4/8/2015
    /// </summary>
    /// 
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class Alistar
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 365) },
            { Spells.W, new Spell(SpellSlot.W, 650) },
            { Spells.E, new Spell(SpellSlot.E, 575) },
            { Spells.R, new Spell(SpellSlot.R, 0) }
        };

        #region Gameloaded 

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Alistar")
                return;

            spells[Spells.R].SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);

            Notifications.AddNotification("ElAlistarReborn by jQuery", 5000);
            _ignite = Player.GetSpellSlot("summonerdot");

            ElAlistarMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        #endregion

        #region OnUpdate

        private static void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
            }

            HealManager();
        }

        #endregion

        #region Heal

        private static void HealManager()
        {

            var useHeal = ElAlistarMenu._menu.Item("ElAlistar.Heal.Activated").GetValue<bool>();
            var useHealAlly = ElAlistarMenu._menu.Item("ElAlistar.Heal.Ally.Activated").GetValue<bool>();
            var playerMana = ElAlistarMenu._menu.Item("ElAlistar.Heal.Player.Mana").GetValue<Slider>().Value;

            if (Player.HasBuff("Recall") || Player.InFountain() || Player.ManaPercent < playerMana || !spells[Spells.E].IsReady())
                return;

            var playerHp = ElAlistarMenu._menu.Item("ElAlistar.Heal.Player.HP").GetValue<Slider>().Value;
            var allyHp = ElAlistarMenu._menu.Item("ElAlistar.Heal.Ally.HP").GetValue<Slider>().Value;
  
            //self heal
            if (useHeal && (Player.Health / Player.MaxHealth) * 100 <= playerHp)
            {
                spells[Spells.E].Cast(Player);
            }

            //ally
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                if (useHealAlly && (hero.Health / hero.MaxHealth) * 100 <= allyHp && spells[Spells.E].IsInRange(hero))
                {
                    spells[Spells.E].Cast(Player);
                }
             }
        }

        #endregion

        #region onCombo

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;

            var useQ = ElAlistarMenu._menu.Item("ElAlistar.Harass.Q").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(target);
            }
        }

        #endregion

        #region onCombo
        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;

            var useQ = ElAlistarMenu._menu.Item("ElAlistar.Combo.Q").GetValue<bool>();
            var useW = ElAlistarMenu._menu.Item("ElAlistar.Combo.W").GetValue<bool>();
            //var useE = ElAlistarMenu._menu.Item("ElAlistar.Combo.E").GetValue<bool>();
            var useR = ElAlistarMenu._menu.Item("ElAlistar.Combo.R").GetValue<bool>();
            var useI = ElAlistarMenu._menu.Item("ElAlistar.Combo.Ignite").GetValue<bool>();
            //var playerHp = ElAlistarMenu._menu.Item("ElAlistar.Heal.Player.HP").GetValue<Slider>().Value;
            var enemiesInRange = ElAlistarMenu._menu.Item("ElAlistar.Combo.Count.Enemies").GetValue<Slider>().Value;
            var rHealth = ElAlistarMenu._menu.Item("ElAlistar.Combo.HP.Enemies").GetValue<Slider>().Value;

            SpellDataInst qmana = Player.Spellbook.GetSpell(SpellSlot.Q);
            SpellDataInst wmana = Player.Spellbook.GetSpell(SpellSlot.W);

            if (useQ && useW && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() && qmana.ManaCost + wmana.ManaCost <= Player.Mana)
            {
                spells[Spells.W].Cast(target);
                var comboTime = Math.Max(0, Player.Distance(target) - 500) * 10 / 25 + 25;

                Utility.DelayAction.Add((int)comboTime, () => spells[Spells.Q].Cast());
                Utility.DelayAction.Add(1000, () => spells[Spells.E].Cast(Player));
            }

            // check player HP 
           /* if (useE && spells[Spells.E].IsReady() && (Player.Health / Player.MaxHealth) * 100 >= playerHp)
            {
                spells[Spells.E].Cast(Player);
            }*/

            if (useR && Player.CountEnemiesInRange(spells[Spells.W].Range) >= enemiesInRange && (Player.Health / Player.MaxHealth) * 100 >= rHealth)
            {
                spells[Spells.R].Cast(Player);
            }

            //Check if target is killable with W when Q is on CD
            if (spells[Spells.W].IsReady() && !spells[Spells.Q].IsReady() && spells[Spells.W].IsInRange(target) &&
                GetWDamage(target) > target.Health)
            {
                spells[Spells.W].Cast(target);
            }
       
            // Ignite when killable
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
                Player.Spellbook.CastSpell(_ignite, target);
        }

        #endregion


        #region GetComboDamage   

        private static float GetWDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.W].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            return (float)damage;
        }

        #endregion

        #region Ignite
        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Intterupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.Q].Range)
                return;

            if (sender.IsValidTarget(spells[Spells.Q].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(sender);
            }

            if (sender.IsValidTarget(spells[Spells.W].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && !spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(sender);
            }
        }

        #endregion

        #region Gapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = ElAlistarMenu._menu.Item("ElAlistar.Interrupt").GetValue<bool>();

            if (gapCloserActive && spells[Spells.W].IsReady() &&
                gapcloser.Sender.Distance(Player) < spells[Spells.W].Range)
            {
                spells[Spells.W].Cast(gapcloser.Sender);
            }

            if (gapCloserActive && !spells[Spells.W].IsReady() && spells[Spells.Q].IsReady() &&
                gapcloser.Sender.Distance(Player) < spells[Spells.Q].Range)
            {
                spells[Spells.Q].Cast(gapcloser.Sender);
            }
        }
        #endregion
    }
}
