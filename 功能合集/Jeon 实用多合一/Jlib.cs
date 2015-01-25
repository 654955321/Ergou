#region
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
#endregion

namespace JeonUtility
{
    class Jlib:Program
    {

        public enum test
        {
            show_inventory,
            show_enemybuff,
            show_allybuff,
            show_mebuff
        }


        public static bool getm_bool(String menu)
        {
            bool result;
            result = Program.baseMenu.Item(menu).GetValue<bool>();
            return result;    
        }
        public static int getm_value(String menu)
        {
            int result;
            result = Program.baseMenu.Item(menu).GetValue<Slider>().Value;
            return result;
        }
        public static void targetPing(Vector2 Position)
        {
            if (Environment.TickCount - pastTime < 2000)
                return;
            pastTime = Environment.TickCount;
            //Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(Position.X, Position.Y, 0, 0, Packet.PingType.Danger)).Process();
        }
        public static void targetPing(Vector2 Position, Packet.PingType ptype)
        {
            if (Environment.TickCount - pastTime < 2000)
                return;
            pastTime = Environment.TickCount;
            
            //Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(Position.X, Position.Y, 0, 0, ptype)).Process();
        }
        public static void testf(test a)
        {
            switch (a)
            {
                case test.show_inventory:

                    foreach (var temp in Player.InventoryItems)
                    {
                        Game.PrintChat("Slot : {0} || id : {1} || name {2}", temp.Slot, Convert.ToInt32(temp.Id), temp.Name);
                    }
                    return;

                case test.show_allybuff:
                    foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(hero => !hero.IsMe && hero.IsValid && hero.IsAlly))
                    {
                        String temptext = "";
                        foreach (var venoms in target.Buffs)
                        {
                            temptext = temptext + " " + venoms.DisplayName;
                        }
                        Game.PrintChat("Name: {0} || Buff: {1}", target.BaseSkinName, temptext);
                    }
                    return;

                case test.show_enemybuff:

                    foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(hero => !hero.IsMe && hero.IsValid && !hero.IsAlly))
                    {
                        String temptext = "";
                        foreach (var venoms in target.Buffs)
                        {
                            temptext = temptext + " " + venoms.DisplayName;
                        }
                        Game.PrintChat("Name: {0} || Buff: {1}", target.BaseSkinName, temptext);
                    }
                    return;

                case test.show_mebuff:

                    foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsMe && hero.IsValid))
                    {
                        String temptext = "";
                        foreach (var venoms in target.Buffs)
                        {
                            temptext = temptext + " " + venoms.DisplayName;
                        }
                        Game.PrintChat("Name: {0} || Buff: {1}", target.BaseSkinName, temptext);
                    }
                    return;
            }
        }
        //public static string bool2string(bool a)
        //{
        //    String total;
        //    if (a)
        //    {
        //        total = "ON";
        //    }
        //    else
        //    {
        //        total = "OFF";
        //    }
        //    return total;
        //}
    }
}
