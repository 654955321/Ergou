using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Prince_Urgot
{
    internal class HarassClass
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        private static Menu HarassMenu;

        public HarassClass(Menu harassMenu)
        {
            HarassMenu = harassMenu;
            Menu();
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Menu()
        {
            HarassMenu.AddSubMenu(new Menu("骚扰", "harass"));
            HarassMenu.SubMenu("harass").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            HarassMenu.SubMenu("harass").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            HarassMenu.SubMenu("harass").AddItem(new MenuItem("HaraManaPercent", "最低蓝量百分比").SetValue(new Slider(30, 0, 100)));
            HarassMenu.SubMenu("harass").AddItem(new MenuItem("harassToggle", "骚扰切换").SetValue(new KeyBind('T', KeyBindType.Toggle)));
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if(Player.IsDead)
                return;

            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || HarassMenu.Item("harassToggle").GetValue<KeyBind>().Active)
                Harass();
        }

        private static void Harass()
        {
            var mana = Player.ManaPercentage() > HarassMenu.Item("HaraManaPercent").GetValue<Slider>().Value;
            var t = TargetSelector.GetTarget(SpellClass.Q2.Range, TargetSelector.DamageType.Physical);
            if (!mana)
                return;

            if (HarassMenu.Item("useQ").GetValue<bool>())
            {
                ComboClass.SpellQ(t);
                ComboClass.SpellSecondQ();
            }

            if (HarassMenu.Item("haraE").GetValue<bool>())
            {
                SpellClass.E.CastIfHitchanceEquals(t, HitChance.VeryHigh, ComboClass.PacketCast);
            }
        }
    }
}
