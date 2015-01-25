using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace BaseUlt3
{
    class Program
    {
        public static BaseUlt BaseUlt;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            BaseUlt = new BaseUlt();
        }
    }
}
