using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace StealthDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;  
        }

        static Menu config;
        static List<Spells> SpellList = new List<Spells>();
        static int Delay = 0;
        static float VayneBuffEndTime = 0;

        public struct Spells
        {
            public string ChampionName;
            public string SpellName;
            public SpellSlot slot;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnUpdate;

            SpellList.Add(new Spells { ChampionName = "akali", SpellName = "akalismokebomb", slot = SpellSlot.W });   //Akali W
            SpellList.Add(new Spells { ChampionName = "shaco", SpellName = "deceive", slot = SpellSlot.Q }); //Shaco Q
            SpellList.Add(new Spells { ChampionName = "khazix", SpellName = "khazixr", slot = SpellSlot.R }); //Khazix R
            SpellList.Add(new Spells { ChampionName = "khazix", SpellName = "khazixrlong", slot = SpellSlot.R }); //Khazix R Evolved
            SpellList.Add(new Spells { ChampionName = "talon", SpellName = "talonshadowassault", slot = SpellSlot.R }); //Talon R
            SpellList.Add(new Spells { ChampionName = "monkeyking", SpellName = "monkeykingdecoy", slot = SpellSlot.W }); //Wukong W
            SpellList.Add(new Spells { ChampionName = "vayne", SpellName = "vaynetumble", slot = SpellSlot.Q }); //Vayne R-Q

            Menu();
        }

        static void Game_OnUpdate(EventArgs args)
        {
            var Vayne = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "vayne");

            if (Vayne == null)
                return;

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy &&
                x.ChampionName.ToLower().Contains("vayne") &&
                x.Buffs.Any(y => y.Name == "VayneInquisition")))
            {
                VayneBuffEndTime = hero.Buffs.First(x => x.Name == "VayneInquisition").EndTime;
            }
        }

        static void Menu()
        {
            config = new Menu("隐形侦测器", "Stealth Detector", true);

            var Detector = new Menu("隐形侦测器", "Stealth Detector");
            {
                Detector.AddItem(new MenuItem("Use", "连招时使用真视守卫").SetValue(new KeyBind(32, KeyBindType.Press)));
                var Detector2 = new Menu("反隐追踪敌方", "Track Him");
                {
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                    {
                        foreach (var spell in SpellList.Where(x => x.ChampionName.ToLower() == hero.ChampionName.ToLower()))
                        {
                            Detector2.AddItem(new MenuItem(hero.ChampionName.ToLower() + spell.slot.ToString(), hero.ChampionName + " - " + spell.slot.ToString()).SetValue(true));
                        }
                    }

                    if (HeroManager.Enemies.Any(x => x.ChampionName.ToLower() == "rengar"))
                    {
                        Detector2.AddItem(new MenuItem("RengarR", "雷恩加尔（狮子狗） R").SetValue(true));
                    }

                    Detector.AddSubMenu(Detector2);
                }

                config.AddSubMenu(Detector);
            }

            config.AddToMainMenu();
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!config.Item("Use").GetValue<KeyBind>().Active)
                return;

            if (!sender.IsEnemy || sender.IsDead || !(sender is Obj_AI_Hero))
                return;

            if (SpellList.Exists(x => x.SpellName.Contains(args.SData.Name.ToLower())))
            {
                var _sender = sender as Obj_AI_Hero;

                if (!config.Item(_sender.ChampionName.ToLower() + _sender.GetSpellSlot(args.SData.Name).ToString()).GetValue<bool>())
                    return;

                if (CheckSlot() == SpellSlot.Unknown)
                    return;

                if (CheckWard())
                    return;

                if (ObjectManager.Player.Distance(sender.Position) > 700)
                    return;

                if (args.SData.Name.ToLower().Contains("vaynetumble") && Game.Time > VayneBuffEndTime)
                    return;

                if (Environment.TickCount - Delay > 1500 || Delay == 0)
                {
                    var pos = ObjectManager.Player.Distance(args.End) > 600 ? ObjectManager.Player.Position : args.End;
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), pos);
                    Delay = Environment.TickCount;
                }
            }
        }

        static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!config.Item("Use").GetValue<KeyBind>().Active)
                return;

            var Rengar = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "rengar");

            if (Rengar == null)
                return;

            if (!config.Item("RengarR").GetValue<bool>())
                return;

            if (ObjectManager.Player.Distance(sender.Position) < 1500)
            {
                Console.WriteLine("Sender : " + sender.Name);
            }

            if (sender.IsEnemy && sender.Name.Contains("Rengar_Base_R_Alert"))
            {
                if (ObjectManager.Player.HasBuff("rengarralertsound") &&
                !CheckWard() &&
                !Rengar.IsVisible &&
                !Rengar.IsDead &&
                    CheckSlot() != SpellSlot.Unknown)
                {
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                }
            }
        }

        static SpellSlot CheckSlot()
        {
            SpellSlot slot = SpellSlot.Unknown;

            if (Items.CanUseItem(3362) && Items.HasItem(3362, ObjectManager.Player))
            {
                slot = SpellSlot.Trinket;
            }
            else if (Items.CanUseItem(2043) && Items.HasItem(2043, ObjectManager.Player))
            {
                slot = ObjectManager.Player.GetSpellSlot("VisionWard");
            }
            return slot;
        }

        static bool CheckWard()
        {
            var status = false;

            foreach (var a in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name == "VisionWard"))
            {
                if (ObjectManager.Player.Distance(a.Position) < 450)
                {
                    status = true;
                }
            }

            return status;
        }
    }
}
