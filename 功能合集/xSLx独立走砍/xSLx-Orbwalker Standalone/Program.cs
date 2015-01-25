using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using xSLx_Orbwalker;

namespace xSLx_Orbwalker_Standalone
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("<font color=\"#33CC00\">xSLx Orbwalker standalone</font> loaded. - ByE2Slayer <font color=\"#0066FF\">汉化By花边</font>");

            //xSLxOrbwalker Load part
            var menu = new Menu("花边汉化-xSLx走砍", "my_mainmenu", true);
            var orbwalkerMenu = new Menu("花边汉化-xSLx走砍", "my_Orbwalker");
            xSLxOrbwalker.AddToMenu(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);
            menu.AddToMainMenu();


            //xSLxActivator Load part
            var targetselectormenu = new Menu("目标 选择", "Common_TargetSelector");
            TargetSelector.AddToMenu(targetselectormenu);
            menu.AddSubMenu(targetselectormenu);
        }
    }
}
