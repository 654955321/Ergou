using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Prince_Urgot
{
    internal class ItemClass
    {
        public static Items.Item Muramana;
        public static SpellSlot IgniteSlot;
        private static Menu ItemMenu;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public ItemClass(Menu itemMenu)
        {
            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            Muramana = new Items.Item(3042, 0);
            ItemMenu = itemMenu;
            Menu();
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ItemMenu.Item("useMura").GetValue<bool>())
            {
                if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.HasBuff("Muramana", false))
                {
                    Muramana.Cast(Player);
                }
                if (Program.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Player.HasBuff("Muramana", true))
                {
                    Muramana.Cast(Player);
                }
            }

            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() && ItemMenu.Item("useIgnite").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
                var dmg = Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);
                if (t.Health < dmg && Player.Distance(t) < 600)
                    Player.Spellbook.CastSpell(IgniteSlot, t);
            }
        }

        private static void Menu()
        {
            ItemMenu.AddSubMenu(new Menu("物品", "items"));
            ItemMenu.SubMenu("items").AddItem(new MenuItem("useMura", "使用魔切").SetValue(true));
            ItemMenu.SubMenu("items").AddItem(new MenuItem("useIgnite", "使用点燃").SetValue(true));
        }
    }
}
