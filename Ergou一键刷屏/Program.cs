using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HaydariGeceler_cici_wipi_TR
{
    internal class Program
    {
        private static LeagueSharp.Common.Menu haydarigeceler;
        public static bool duramk = false;
        public static float gameTime1 = 0;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
        Game.PrintChat(
                "<font color = \"#ff052b\">Ergou涓€閿埛灞弢鍔犺浇鎴愬姛!</font>  <font color = \"#fcdfff\">|鈽嗏槄鈽嗘父鎴忊槄鈽嗘剦蹇槄鈽嗏槄|</font> ");

            haydarigeceler = new LeagueSharp.Common.Menu("Ergou一键刷屏", "", true);
            var press1 =haydarigeceler.AddItem(new MenuItem("GGyaz", "霸气20投 键位a（自己改）").SetValue(new KeyBind(37, KeyBindType.Press)));
            var press2=haydarigeceler.AddItem(new MenuItem("WPyaz", "霸气20投 键位b").SetValue(new KeyBind(39, KeyBindType.Press)));
            var press3 = haydarigeceler.AddItem(new MenuItem("XDyaz", "SB 键位").SetValue(new KeyBind(40, KeyBindType.Press)));  
            var press4 = haydarigeceler.AddItem(new MenuItem("PNSciz", "L# 键位").SetValue(new KeyBind(96, KeyBindType.Press)));
            var press5 = haydarigeceler.AddItem(new MenuItem("Smiley", "笑脸 键位").SetValue(new KeyBind(97, KeyBindType.Press)));
            var press6 = haydarigeceler.AddItem(new MenuItem("TRBAYRAK", "小兔子 键位").SetValue(new KeyBind(98, KeyBindType.Press)));            
            var press7 = haydarigeceler.AddItem(new MenuItem("FCKyaz", "GG 键位").SetValue(new KeyBind(38, KeyBindType.Press)));
            haydarigeceler.AddItem(new MenuItem("Bilgiler", "Ergou QQ群号：361630847"));
            haydarigeceler.AddToMainMenu();


            press1.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("GGyaz").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

                        Game.Say("/all 鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枴鈻♀枅鈻堚枅鈻♀枴鈻♀枅鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枴鈻堚枴鈻堚枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枴鈻堚枴鈻堚枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻堚枅鈻♀枅鈻堚枴鈻堚枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻堚枴鈻堚枅鈻堚枴鈻堚枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻堚枴鈻堚枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枅鈻堚枴鈻堚枅鈻堚枴鈻坾");
						Game.Say("/all 鈻堚枴鈻♀枴鈻♀枴鈻堚枅鈻♀枴鈻♀枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻坾");
                        
                        duramk = true;
                        gameTime1 = Game.Time + 1;
                        

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
                
            };
            press2.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("WPyaz").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枅鈻♀枴鈻♀枴鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枴鈻♀枴鈻♀枅鈻♀枅鈻堚枴鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枴鈻堚枅鈻堚枅鈻♀枴鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枴鈻♀枴鈻♀枴鈻♀枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻♀枅鈻♀枅鈻堚枅鈻♀枅鈻坾");
						Game.Say("/all 鈻堚枴鈻♀枴鈻堚枅鈻堚枴鈻堚枴鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枅鈻堚枅鈻♀枅鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枴鈻堚枅鈻堚枴鈻堚枴鈻堚枅鈻坾");
						Game.Say("/all 鈻堚枅鈻♀枴鈻堚枴鈻♀枅鈻堚枅鈻♀枴鈻坾");
                        
                        duramk = true;
                        gameTime1 = Game.Time + 1;

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
            };
            press3.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("XDyaz").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

                        Game.Say("/all 鈻堚枅鈻♀枴鈻♀枴鈻堚枴鈻♀枴鈻♀枅鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枅鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枴鈻堚枅鈻堚枅鈻♀枴鈻♀枅鈻坾");
                        Game.Say("/all 鈻堚枅鈻堚枅鈻♀枅鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻堚枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枴鈻♀枴鈻♀枅鈻堚枴鈻♀枴鈻♀枅鈻坾");

                        duramk = true;
                        gameTime1 = Game.Time + 1;

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
            };
            press7.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("FCKyaz").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

                        Game.Say("/all 鈻堚枅鈻堚枴鈻♀枴鈻堚枅鈻堚枴鈻♀枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枅鈻堚枴鈻堚枅鈻堚枅鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枅鈻堚枴鈻堚枅鈻堚枅鈻坾");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻♀枴鈻♀枴鈻堚枅鈻♀枴鈻");
                        Game.Say("/all 鈻堚枴鈻堚枅鈻堚枴鈻堚枴鈻堚枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枴鈻堚枅鈻♀枅鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻堚枴鈻♀枅鈻堚枅鈻堚枴鈻♀枅鈻坾|");
 
                        duramk = true;
                        gameTime1 = Game.Time + 1;

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
            };
            press4.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("PNSciz").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

                        Game.Say("/all 鈻堚枴鈻♀枴鈻堚枅鈻堚枅鈻堚枴鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻堚枅鈻堚枴鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻堚枴鈻♀枴鈻♀枴鈻");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻堚枅鈻堚枴鈻堚枴鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻堚枅鈻♀枅鈻♀枅鈻坾");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻堚枴鈻♀枴鈻♀枴鈻");
                        Game.Say("/all 鈻堚枅鈻♀枅鈻堚枅鈻♀枅鈻♀枅鈻♀枅鈻坾");
                        Game.Say("/all 鈻堚枴鈻♀枴鈻♀枴鈻♀枅鈻♀枅鈻♀枅鈻坾");

                        duramk = true;
                        gameTime1 = Game.Time + 1;

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
            };
            press5.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("Smiley").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

                        Game.Say("/all 鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅");
                        Game.Say("/all 鈻堛€€銆€銆€鈼モ枅鈻堚棨銆€銆€銆€鈻坾");
                        Game.Say("/all 鈻堛€€銆€銆€銆€鈼モ棨銆€銆€銆€銆€鈻坾");
                        Game.Say("/all 鈻堚棧銆€銆€銆€銆€銆€銆€銆€銆€鈼⑩枅|");
                        Game.Say("/all 鈻堚棨銆€銆€鈼忋€€銆€鈼忋€€銆€鈼モ枅|");
                        Game.Say("/all 鈻堛€€銆€銆€銆€銆€銆€銆€銆€銆€銆€鈻坾");
						Game.Say("/all 鈻堛€€鈻娿€€銆€銆€銆€銆€銆€鈻娿€€鈻坾銆€");
						Game.Say("/all 鈻堛€€鈼モ枂鈻勨杽鈻勨杽鈻嗏棨銆€鈻坾");
						Game.Say("/all 鈻堛€€銆€銆€銆€銆€銆€銆€銆€銆€    鈻坾");
						Game.Say("/all 鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅鈻堚枅|");

                        duramk = true;
                        gameTime1 = Game.Time + 1;

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
            };
            press6.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (haydarigeceler.Item("TRBAYRAK").GetValue<KeyBind>().Active)
                    if (duramk == false)
                    {

                        Game.Say("/all 鈹忊敁銆€銆€銆€鈹忊敁");
                        Game.Say("/all 鈹冣敆鈹佲攣鈹佲敍鈹億");
                        Game.Say("/all 鈹冣暛鈺€€鈺暜鈹億");
                        Game.Say("/all 鈹冣€濄€€鈺€€鈥濃攦 鈹⑩敠a巍锝恲~");
						Game.Say("/all 鈹椻棆鈹佲攣鈹佲棆鈹泑");
                        

                        duramk = true;
                        gameTime1 = Game.Time + 1;

                    }
                if (Game.Time > gameTime1)
                {
                    duramk = false;
                }
            };
        }
    }
}
          

            
        

        
        
            
        
            


       