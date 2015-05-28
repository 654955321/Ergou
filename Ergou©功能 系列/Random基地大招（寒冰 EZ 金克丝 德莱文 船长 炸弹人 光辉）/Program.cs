using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using UnderratedAIO.Helpers;

namespace RandomUlt
{
    internal class Program
    {
        public static Menu config;
        public static Orbwalking.Orbwalker orbwalker;
        public static LastPositions positions;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            config = new Menu("Random基地大招", "RandomUlt Beta", true);
            Menu RandomUltM = new Menu("设置", "Options");
            positions = new LastPositions(RandomUltM);
            config.AddSubMenu(RandomUltM);
            config.AddItem(new MenuItem("RandomUlt ", "汉化: Ergou"));
            config.AddToMainMenu();
            Notifications.AddNotification(new Notification("RandomUlt by Soresu", 3000, true).SetTextColor(Color.Peru));
        }
    }
}