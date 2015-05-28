using System;
using LeagueSharp;
using LeagueSharp.Common;
using myOrbwalker;

namespace myOrbwalker_Menu
{
    class Program
    {
        private static Menu Config;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName == null) return;
            Config = new Menu("my走砍", "myOrbwalker", true);
            var orbwalkerMenu = new Menu("my走砍", "myOrbwalker");
            MYOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);

            Config.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;
        }
    }
}
