using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Oracle.Extensions
{
    internal class Consumables
    {
        private static Menu _mainMenu;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public void Initialize(Menu root)
        {
            Game.OnUpdate += Game_OnGameUpdate;

            _mainMenu = new Menu("Consumables", "imenu");

            CreateMenuItem("Biscuit", "BiscuitHealthMana", 45, 35);
            CreateMenuItem("Mana Potion", "Mana", 45, 0);
            CreateMenuItem("Crystaline Flask", "FlaskHealthMana", 45, 35);
            CreateMenuItem("Health Potion", "Health", 45, 45);
            CreateMenuItem("Red Elixir", "ElixirHealth,", 20, 45);

            root.AddSubMenu(_mainMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            UseItem("FlaskOfCrystalWater", 2004, "Mana");
            UseItem("ItemMiniRegenPotion", 2010, "BiscuitHealthMana");
            UseItem("ItemCrystalFlask", 2041, "FlaskHealthMana");
            UseItem("RegenerationPotion", 2003, "Health");
            UseItem("ElixirOfWrath", 2140, "Health");
        }

        private static void UseItem(string name, int itemId, string menuvar)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
                return;

            if (Me.HasBuff(name, true) || Me.IsRecalling() || Me.InFountain())
                return;

            if (!_mainMenu.Item("use" + menuvar).GetValue<bool>())
                return;

            var aManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var mHealthPercent = (int) ((Me.Health/Me.MaxHealth)*100);

            var iDamagePercent = (int) ((Oracle.IncomeDamage/Me.MaxHealth)*100);
            var mDamagePercent = (int) ((Oracle.MinionDamage/Me.MaxHealth)*100);

            if (menuvar.Contains("Mana") && aManaPercent <= _mainMenu.Item("use" + menuvar + "Mana").GetValue<Slider>().Value)
            {
                if (Me.Mana != 0)
                    Items.UseItem(itemId);
            }

            if (menuvar.Contains("Health") && mHealthPercent <= _mainMenu.Item("use" + menuvar + "Pct").GetValue<Slider>().Value)
            {
                if (iDamagePercent >= 1 || Oracle.IncomeDamage >= Me.Health || Me.HasBuff("summonerdot", true) ||
                    mDamagePercent >= 1 || Oracle.MinionDamage >= Me.Health || Me.HasBuffOfType(BuffType.Damage))
                {
                    if (Oracle.AggroTarget.NetworkId == Me.NetworkId)
                    {
                        Items.UseItem(itemId);
                        Oracle.Logger(Oracle.LogType.Action, "Used " + name + " (Low HP) on " + Me.SkinName + " (" + mHealthPercent + "%) !");
                    }
                }

                else if (iDamagePercent >= _mainMenu.Item("use" + menuvar + "Dmg").GetValue<Slider>().Value)
                {
                    if (Oracle.AggroTarget.NetworkId == Me.NetworkId)
                    {
                        Items.UseItem(itemId);
                        Oracle.Logger(Oracle.LogType.Action, "Used " + name + " (Damage Chunk) on " + Me.SkinName + " (" + mHealthPercent + "%) !");
                    }
                }
            }
        }

        private static void CreateMenuItem(string name, string menuvar, int dvalue, int dmgvalue)
        {
            var menuName = new Menu(name, "m" + menuvar);
            menuName.AddItem(new MenuItem("use" + menuvar, "Use " + name)).SetValue(true);

            if (menuvar.Contains("Health"))
            {
                menuName.AddItem(new MenuItem("use" + menuvar + "Pct", "Use on HP %")).SetValue(new Slider(dvalue));
                menuName.AddItem(new MenuItem("use" + menuvar + "Dmg", "Use on Dmg dealt %")).SetValue(new Slider(dmgvalue));
            }

            if (menuvar.Contains("Mana"))
                menuName.AddItem(new MenuItem("use" + menuvar + "Mana", "Use on Mana %")).SetValue(new Slider(40));

            _mainMenu.AddSubMenu(menuName);
        }
    }
}