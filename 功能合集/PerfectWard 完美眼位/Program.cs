using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using Color = System.Drawing.Color;

namespace PerfectWard
{
    internal class Program
    {
        private static Obj_AI_Hero Player;
        private static string WelcomeMsg = ("<font color = '#cc33cc'>瀹岀編鐪间綅</font><font color='#FFFFFF'> by Da'ath.</font> <font color = '#66ff33'> ~~ 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847 ~~</font>");
        private static Menu Menu;
        private static List<Vector3> standingCoords;
        private static List<Vector3> placeCoords;
        private static Items.Item sWard, vWard, sightStone, rSightStone, trinket, gsT, gvT;
        private static int Time;
        private static float wR = 1000f;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.PrintChat(WelcomeMsg);
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
                    
            Player = ObjectManager.Player;

            sWard = new Items.Item(2044, wR);
            vWard = new Items.Item(2043, wR);
            sightStone = new Items.Item(2049, wR);
            rSightStone = new Items.Item(2045, wR);
            trinket = new Items.Item(3340, wR);
            gsT = new Items.Item(3361, wR);
            gvT = new Items.Item(3362, wR);

            Menu = new Menu("完美眼位", "menu", true);
            Menu.AddItem(new MenuItem("on", "启用").SetValue(true));
            Menu.AddItem(new MenuItem("on1", "显示附近眼位").SetValue(true));
            Menu.AddItem(new MenuItem("on2", "显示范围").SetValue(new Slider(1500, 600, 2000)));
            Menu.AddItem(new MenuItem("on3", "显示站立位置眼位").SetValue(true));
            Menu.AddItem(new MenuItem("on4", "移动时显示周围眼位").SetValue(true));
            Menu.AddItem(new MenuItem("key", "自动插眼").SetValue(new KeyBind("Z".ToArray()[0], KeyBindType.Press)));
            Menu.AddItem(new MenuItem("key1", "插眼标记（粉色）").SetValue(new KeyBind("A".ToArray()[0], KeyBindType.Press)));

            Menu.AddToMainMenu();

            standingCoords = new List<Vector3>();
            placeCoords = new List<Vector3>();
            GetStandingCoords();
            GetPlaceCoords();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (!Menu.Item("on").GetValue<bool>())
                return;

            PlaceWards();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (!Menu.Item("on").GetValue<bool>())
                return;

            //if (Menu.Item("key").GetValue<KeyBind>().Active || Menu.Item("key1").GetValue<KeyBind>().Active)
                //Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (Menu.Item("on1").GetValue<bool>())
            {
                foreach (var StandingCoords in standingCoords.Where(StandingCoords => Vector3.Distance(Player.ServerPosition, StandingCoords) <= Menu.Item("on2").GetValue<Slider>().Value))
                {
                    Utility.DrawCircle(StandingCoords, 50f, Color.Red);
                    if (Menu.Item("on4").GetValue<bool>())
                    {
                        if (Menu.Item("key").GetValue<KeyBind>().Active || Menu.Item("key1").GetValue<KeyBind>().Active)
                        {
                            Utility.DrawCircle(StandingCoords, 500, Color.Yellow);
                        }
                    }
                }
                if (!Menu.Item("on3").GetValue<bool>())
                {
                    foreach (var PlaceCoords in placeCoords.Where(PlaceCoords => Vector3.Distance(Player.ServerPosition, PlaceCoords) <= Menu.Item("on2").GetValue<Slider>().Value))
                    {
                        Utility.DrawCircle(PlaceCoords, 15f, Color.Blue);
                    }
                        
                }
            }
            else
            {
                foreach (var StandingCoords in standingCoords)
                {
                    Utility.DrawCircle(StandingCoords, 50f, Color.Red);
                    if (Menu.Item("on4").GetValue<bool>())
                    {
                        if (Menu.Item("key").GetValue<KeyBind>().Active || Menu.Item("key1").GetValue<KeyBind>().Active)
                        {
                            Utility.DrawCircle(StandingCoords, 500, Color.Yellow);
                        }
                    }
                }
                if (!Menu.Item("on3").GetValue<bool>())
                {
                    foreach (var PlaceCoords in placeCoords)
                        Utility.DrawCircle(PlaceCoords, 15f, Color.Blue);
                }
            }
        }
        private static void PlaceWards()
        {

             foreach (var StandingCoords in standingCoords.Where(StandingCoords => Vector3.Distance(Player.ServerPosition, StandingCoords) <= 500))
             {

                 if (Menu.Item("key").GetValue<KeyBind>().Active || Menu.Item("key1").GetValue<KeyBind>().Active)
                    Player.IssueOrder(GameObjectOrder.MoveTo, StandingCoords);

                 if (Vector3.Distance(Player.ServerPosition, StandingCoords) < 50)
                 {

                     foreach (var PlaceCoords in placeCoords.Where(PlaceCoords => Vector3.Distance(Player.ServerPosition, PlaceCoords) <= wR))
                     {
                         if (Menu.Item("key").GetValue<KeyBind>().Active)
                         {

                             if (trinket.IsReady())
                             {
                                 trinket.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }
                             if (sightStone.IsReady() && (Environment.TickCount > Time))
                             {
                                 sightStone.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }
                             if (rSightStone.IsReady() && (Environment.TickCount > Time))
                             {
                                 rSightStone.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }
                             if (gsT.IsReady() && (Environment.TickCount > Time))
                             {
                                 gsT.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }
                             if (sWard.IsReady() && (Environment.TickCount > Time))
                             {
                                 sWard.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }



                         }
                         if (Menu.Item("key1").GetValue<KeyBind>().Active)
                         {

                             if (vWard.IsReady())
                             {
                                 vWard.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }
                             if (gvT.IsReady() && (Environment.TickCount > Time))
                             {
                                 gvT.Cast(PlaceCoords);
                                 Time = Environment.TickCount + 5000;
                             }
                         }

                     }
                 }

             }


        }


        private static void GetStandingCoords()
        {
            Vector3 stand1 = new Vector3(2524, 10406, 54f);
            standingCoords.Add(stand1);
            Vector3 stand2 = new Vector3(1774, 10756, 52f);
            standingCoords.Add(stand2);
            Vector3 stand3 = new Vector3(5520, 6342, 51f);
            standingCoords.Add(stand3);
            Vector3 stand4 = new Vector3(5674, 7358, 51f);
            standingCoords.Add(stand4);
            Vector3 stand5 = new Vector3(7990, 4282, 53f);
            standingCoords.Add(stand5);
            Vector3 stand6 = new Vector3(8256, 2920, 51f);
            standingCoords.Add(stand6);
            Vector3 stand7 = new Vector3(4818, 10866, -71f);
            standingCoords.Add(stand7);
            Vector3 stand8 = new Vector3(6824, 10656, 55f);
            standingCoords.Add(stand8);
            Vector3 stand9 = new Vector3(6574, 12006, 56f);
            standingCoords.Add(stand9);
            Vector3 stand10 = new Vector3(9130, 8346, 53f);
            standingCoords.Add(stand10);
            Vector3 stand11 = new Vector3(9422, 7408, 52f);
            standingCoords.Add(stand11);
            Vector3 stand12 = new Vector3(12372, 4508, 51f);
            standingCoords.Add(stand12);
            Vector3 stand13 = new Vector3(13003, 3818, 51f);
            standingCoords.Add(stand13);


        }

        private static void GetPlaceCoords()
        {
            Vector3 place1 = new Vector3(2729, 10879, -71f);
            placeCoords.Add(place1);
            Vector3 place2 = new Vector3(2303, 10868, 53f);
            placeCoords.Add(place2);
            Vector3 place3 = new Vector3(5223, 6789, 50f);
            placeCoords.Add(place3);
            Vector3 place4 = new Vector3(5191, 7137, 50f);
            placeCoords.Add(place4);
            Vector3 place5 = new Vector3(8368, 4594, 51f);
            placeCoords.Add(place5);
            Vector3 place6 = new Vector3(8100, 3429, 51f);
            placeCoords.Add(place6);
            Vector3 place7 = new Vector3(4634, 11283, 49f);
            placeCoords.Add(place7);
            Vector3 place8 = new Vector3(6672, 11466, 53f);
            placeCoords.Add(place8);
            Vector3 place9 = new Vector3(6518, 10367, 53f);
            placeCoords.Add(place9);
            Vector3 place10 = new Vector3(9572, 8038, 57f);
            placeCoords.Add(place10);
            Vector3 place11 = new Vector3(9697, 7854, 51f);
            placeCoords.Add(place11);
            Vector3 place12 = new Vector3(12235, 4068, -68f);
            placeCoords.Add(place12);
            Vector3 place13 = new Vector3(12443, 4021, -7f);
            placeCoords.Add(place13);

        }
    }
}
