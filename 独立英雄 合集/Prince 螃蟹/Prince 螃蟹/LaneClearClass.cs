using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Prince_Urgot
{
    internal class LaneClearClass
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        private static Menu LaneClearMenu;
        static readonly bool PacketCast = ComboClass.PacketCast;

        public LaneClearClass(Menu laneclearMenu)
        {
            LaneClearMenu = laneclearMenu;
            Menu();
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Menu()
        {
            LaneClearMenu.AddSubMenu(new Menu("清线", "laneclear"));
            LaneClearMenu.SubMenu("laneclear").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            LaneClearMenu.SubMenu("laneclear").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            LaneClearMenu.SubMenu("laneclear").AddItem(new MenuItem("LaneClearManaPercent", "最低蓝量百分比").SetValue(new Slider(30, 0, 100)));
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                LaneClear();
        }

        private static void LaneClear()
        {
            var mana = Player.ManaPercentage() > LaneClearMenu.Item("LaneClearManaPercent").GetValue<Slider>().Value;
             var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange, MinionTypes.All, MinionTeam.NotAlly);
            if (!mana)
                return;

            if (LaneClearMenu.Item("useQ").GetValue<bool>() && SpellClass.Q.IsReady())
            {
                foreach (var minion in minions.Where(minion => minion.IsValidTarget()))
                {
                    if (SpellClass.Q2.IsInRange(minion) && minion.HasBuff("urgotcorrosivedebuff", true))
                    {
                        SpellClass.Q2.Cast(minion.ServerPosition, PacketCast);
                    }
                    else
                    {
                        ComboClass.SpellQ(minion);
                    }
                }
            }

            if (LaneClearMenu.Item("haraE").GetValue<bool>() && SpellClass.E.IsReady())
            {
                foreach (var minion in minions.Where(minion => minion.IsValidTarget()))
                {
                    if (minion.IsValidTarget(SpellClass.E.Range))
                    {
                        SpellClass.E.Cast(minion.ServerPosition, PacketCast);
                    }
                }
            }
        }
    }
}
