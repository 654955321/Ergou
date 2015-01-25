using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Prince_Urgot
{
    internal class ComboClass
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        public static bool PacketCast;
        private static Menu ComboMenu;

        public ComboClass(Menu comboMenu)
        {
            ComboMenu = comboMenu;
            Menu();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            PacketCast = ComboMenu.SubMenu("misc").Item("packetcast").GetValue<bool>();
            SpellClass.R.Range = 400 + (150 * SpellClass.R.Level);

            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();
        }
        #region Menu
        internal static void Menu()
        {
            ComboMenu.AddSubMenu(new Menu("连招", "combo"));

            ComboMenu.SubMenu("combo").AddItem(new MenuItem("useQ", "使用 Q")).SetValue(true);
            ComboMenu.SubMenu("combo").AddItem(new MenuItem("useW", "使用 W")).SetValue(true);
            ComboMenu.SubMenu("combo").AddItem(new MenuItem("useE", "使用 E")).SetValue(true);
            ComboMenu.SubMenu("combo").AddItem(new MenuItem("useR", "使用 R (自动R塔下)")).SetValue(true);
            ComboMenu.SubMenu("combo").AddItem(new MenuItem("preE", "高命中率 E").SetValue(new StringList((new[] { "正常", "缓慢", "高", "非常高" }))));

            ComboMenu.AddSubMenu(new Menu("额外选项", "misc"));
            ComboMenu.SubMenu("misc").AddItem(new MenuItem("autoInt", "打断使用 " + "R").SetValue(false));
            ComboMenu.SubMenu("misc").AddItem(new MenuItem("packetcast", "使用封包")).SetValue(true);
        }
        #endregion

        #region Interrupter
        private static void Interrupter_OnPossibleToInterrupt (Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (SpellClass.R.IsReady() && unit.IsEnemy && SpellClass.R.IsInRange(unit) && ComboMenu.Item("autoInt").GetValue<bool>())
            {
                SpellClass.R.CastOnUnit(unit, PacketCast);
            }
        }
        #endregion

        #region Combo
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
            var castQ = (ComboMenu.Item("useQ").GetValue<bool>());
            var castE = (ComboMenu.Item("useW").GetValue<bool>());
            var castW = (ComboMenu.Item("useE").GetValue<bool>());
            var castR = (ComboMenu.Item("useR").GetValue<bool>());

            if (target == null)
            {
                return;
            }
            if (castE)
            {
                SpellE(target);
            }
            if (castQ)
            {
                SpellSecondQ();
                SpellQ(target);
            }
            if (castW)
            {
                SpellW(target);
            }
            if (castR)
            {
                AutoR(target);
            }
            
        }

        internal static void SpellSecondQ()
        {
            foreach (var obj in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(obj => obj.IsValidTarget(SpellClass.Q2.Range) && obj.HasBuff("urgotcorrosivedebuff", true)))
            {
                SpellClass.Q2.Cast(obj.ServerPosition, PacketCast);
            }
        }

        internal static void SpellQ(Obj_AI_Base t)
        {
            if (t.HasBuff("urgotcorrosivedebuff", true))
                return;

            if (SpellClass.Q.IsReady() && SpellClass.Q.IsInRange(t))
            {
                SpellClass.Q.Cast(t, PacketCast);
            }
        }

        private static void SpellW(Obj_AI_Base t)
        {
            var distance = ObjectManager.Player.Distance(t);

            if (SpellClass.W.IsReady() && distance <= 100 || (distance >= 900 && distance <= 1200) && t.HasBuff("urgotcorrosivedebuff", true))
            {
                SpellClass.W.Cast(Player, PacketCast);
            }
        }

        internal static void SpellE(Obj_AI_Base t)
        {
            var hitchance = (HitChance)(ComboMenu.Item("preE").GetValue<StringList>().SelectedIndex + 3);

            if (SpellClass.E.IsInRange(t) && SpellClass.E.IsReady())
            {
                SpellClass.E.CastIfHitchanceEquals(t, hitchance, PacketCast);
            }
        }

        private static void AutoR(Obj_AI_Base t)
        {
            var useR = SpellClass.R.IsInRange(t);
            var turret = ObjectManager.Get<Obj_AI_Turret>().First(obj => obj.IsAlly && obj.Distance(Player) <= 775f);
            var minions = MinionManager.GetMinions(turret.ServerPosition, 775, MinionTypes.All, MinionTeam.All);

            if (turret != null && useR && minions == null)
            {
                SpellClass.R.Cast(t, true, PacketCast);
            }
        }
        #endregion
    }
}
