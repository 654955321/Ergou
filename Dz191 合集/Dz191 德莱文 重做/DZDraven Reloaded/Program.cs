using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZDraven_Reloaded
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += DZDraven_Reloaded.Game_OnGameLoad;
        }
    }
}
