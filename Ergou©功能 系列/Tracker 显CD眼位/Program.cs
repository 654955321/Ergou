#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;

#endregion

namespace Tracker
{
    
    internal class Program
    {
        public static Menu Config;

        static void Main(string[] args)
        {
            if (Game.Mode == GameMode.Running)
            {
                Game_OnStart(new EventArgs());
            }

            Game.OnStart += Game_OnStart;
        }

        static void Game_OnStart(EventArgs args)
        {
            Config = new Menu("Tracker 显CD眼位", "Tracker", true);
            HbTracker.AttachToMenu(Config);
            WardTracker.AttachToMenu(Config);
            Config.AddToMainMenu();
        }

    }

}
