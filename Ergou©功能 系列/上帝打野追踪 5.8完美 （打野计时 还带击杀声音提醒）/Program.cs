/*
██████╗ ██╗   ██╗     █████╗ ██╗     ██████╗ ██╗  ██╗ █████╗  ██████╗  ██████╗ ██████╗ 
██╔══██╗╚██╗ ██╔╝    ██╔══██╗██║     ██╔══██╗██║  ██║██╔══██╗██╔════╝ ██╔═══██╗██╔══██╗
██████╔╝ ╚████╔╝     ███████║██║     ██████╔╝███████║███████║██║  ███╗██║   ██║██║  ██║
██╔══██╗  ╚██╔╝      ██╔══██║██║     ██╔═══╝ ██╔══██║██╔══██║██║   ██║██║   ██║██║  ██║
██████╔╝   ██║       ██║  ██║███████╗██║     ██║  ██║██║  ██║╚██████╔╝╚██████╔╝██████╔╝
╚═════╝    ╚═╝       ╚═╝  ╚═╝╚══════╝╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚═════╝ 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using FontDrawFlags = SharpDX.Direct3D9.FontDrawFlags;
using Vector2 = SharpDX.Vector2;

namespace GodJungleTracker
{
    class Program
    {
        static Menu menu;

        #region Definitions

        public static Font MinimapText = new Font(Drawing.Direct3DDevice,
                        new FontDescription
                        {
                            FaceName = "Calibri",
                            Height = 13,
                            OutputPrecision = FontPrecision.Default,
                            Quality = FontQuality.Default
                        });

        public static Font MapText = new Font(Drawing.Direct3DDevice,
                    new FontDescription
                    {
                        FaceName = "Calibri",
                        Height = 13,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });

        public static short HeaderRangedAttack = 220;

        public static short HeaderMeleeAttack = 48;

        public static short HeaderDisengaged = 75;

        public static short HeaderSkill = 151;

        public static short HeaderCreateGromp = 301;

        public static float BaronSpawn = 1199;

        public static int EnemyTeamStacks = 0;

        public static int AllyTeamStacks = 0;

        public static bool EnemyTeamNashor = false;

        public static bool AllyTeamNashor = false;

        public static string TestMinionName = "";

        public static int TestMinionState = 0;

        public static int GuessNetworkID1 = 1;

        public static int GuessNetworkID2 = 1;

        public static int GuessDragonID = 1;

        public static int EnemyFoWTime = 0;

        public static int AllyFoWTime = 0;

        public static int LastPlayedDragonSound = 0;

        public static int LastPlayedBaronSound = 0;

        public static int LastPlayedBaronSound2 = 0;

        public static int BaronSoundDelay = 0;

        public static int Seed1 = 3;

        public static int Seed2 = 2;

        public static float ClockTimeAdjust = 0;

        public static int BiggestNetworkID = 0;

        public static int l = 0;

        public static int BufferDragonSound = 0;

        public static int PlayingDragonSound = 0;

        public static Vector3[] CampPosition = 
        { 
        new Vector3(9866f  , 4414f  , -71f),
        new Vector3(5007f  , 10471f , -71f),
        new Vector3(3872f  , 7900f  , 51f),
        new Vector3(10930f , 6992f  , 52f),
        new Vector3(7862f  , 4111f  , 54f),
        new Vector3(7017f  , 10775f , 56f),
        new Vector3(10508f , 5271f  , -62f),
        new Vector3(4418f  , 9664f  , -69f),
        new Vector3(7857f  , 9471f  , 52f),
        new Vector3(6954f  , 5458f  , 53f),
        new Vector3(2091f  , 8428f  , 52f),
        new Vector3(12703f , 6444f  , 52f),
        new Vector3(6449f  , 12117f , 56f),
        new Vector3(8381f  , 2711f  , 51f),
        new Vector3(10957f , 8350f  , 62f),
        new Vector3(3825f  , 6491f  , 52f),
        };

        private static readonly SoundPlayer danger = new SoundPlayer(Properties.Resources.danger);
        private static readonly SoundPlayer danger10 = new SoundPlayer(Properties.Resources.danger10);
        private static readonly SoundPlayer danger25 = new SoundPlayer(Properties.Resources.danger25);
        private static readonly SoundPlayer danger50 = new SoundPlayer(Properties.Resources.danger50);
        private static readonly SoundPlayer danger75 = new SoundPlayer(Properties.Resources.danger75);
        private static SoundPlayer sound = danger;

        private static List<Obj_AI_Minion> TrackingList { get; set; }

        public static int[] OnFow = { 0, 0 };

        public static int[] OnScreen = { 0, 0 };

        public static int[] NetworkID = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] JustDied = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] LastChangeOnState = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] LastChangeOnCampState = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] HeroNetworkID = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static string[] Heros = { "Caitlyn", "Fizz", "Jinx", "Nidalee", "Wukong", "Zed", "Zyra", "Kalista", "Yasuo" };

        public static string[] HeroName = { "", "", "", "", "", "", "", "", "", "" };

        public static int[] State = { 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] UnitToCamp = { 0, 1, 4, 7, 10, 13, 24, 25, 19, 23, 14, 15, 27, 29, 32, 35 };

        public static int[] CampState = { 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] SeedOrder = { 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0 };

        public static int[] CreateOrder = { 14, 15, 10, 9, 8, 13, 12, 11, 4, 3, 2, 7, 6, 5, 23, 22, 21, 20, 29, 28, 27, 26, 19, 18, 17, 16, 35, 34, 33, 32, 31, 30 };

        public static int[] IDOrder = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 2, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] CampRespawnTime = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int[] CampRespawnTimer = { 360, 
                                                 420, 
                                                 300, 
                                                 300, 
                                                 300, 
                                                 300, 
                                                 180, 
                                                 180, 
                                                 100, 
                                                 100, 
                                                 100, 
                                                 100, 
                                                 100, 
                                                 100, 
                                                 100, 
                                                 100 };


        public static string[] CampName = { "Dragon",
                                            "Baron",
                                            "Blue W", 
                                            "Blue E", 
                                            "Red S", 
                                            "Red N", 
                                            "Crab S", 
                                            "Crab N", 
                                            "Raptor N", 
                                            "Raptor S", 
                                            "Gromp W", 
                                            "Gromp E", 
                                            "Krug N", 
                                            "Krug S", 
                                            "Wolf E", 
                                            "Wolf W" };


        public static string[] NameToCompare = { "SRU_Dragon6.1.1", 
                                                 "SRU_Baron12.1.1",
                                                 "SRU_BlueMini21.1.3", "SRU_BlueMini1.1.2", "SRU_Blue1.1.1", //4
                                                 "SRU_BlueMini27.1.3", "SRU_BlueMini7.1.2", "SRU_Blue7.1.1", //7
                                                 "SRU_RedMini4.1.3", "SRU_RedMini4.1.2", "SRU_Red4.1.1",//10
                                                 "SRU_RedMini10.1.3", "SRU_RedMini10.1.2", "SRU_Red10.1.1", //13
                                                 "SRU_Gromp13.1.1",
                                                 "SRU_Gromp14.1.1", 
                                                 "SRU_RazorbeakMini9.1.4", "SRU_RazorbeakMini9.1.3", "SRU_RazorbeakMini9.1.2", "SRU_Razorbeak9.1.1",//19
                                                 "SRU_RazorbeakMini3.1.4", "SRU_RazorbeakMini3.1.3", "SRU_RazorbeakMini3.1.2", "SRU_Razorbeak3.1.1",//23 
                                                 "Sru_Crab15.1.1",
                                                 "Sru_Crab16.1.1",
                                                 "SRU_Krug11.1.2", "SRU_KrugMini11.1.1", //27
                                                 "SRU_Krug5.1.2", "SRU_KrugMini5.1.1",//29
                                                 "SRU_MurkwolfMini8.1.3", "SRU_MurkwolfMini8.1.2", "SRU_Murkwolf8.1.1", //32
                                                 "SRU_MurkwolfMini2.1.3", "SRU_MurkwolfMini2.1.2", "SRU_Murkwolf2.1.1", //35
                                                 "SRU_BaronSpawn12.1.2" };

        #endregion

        static void Main(string[] args)
        {
            Game.OnStart += OnGameStart;
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameStart(EventArgs args)
        {
            ClockTimeAdjust = Game.ClockTime;

            CampRespawnTime[0] = (int)((145f + Game.ClockTime) * 1000) + Environment.TickCount;

            CampRespawnTime[1] = (int)((1195f + Game.ClockTime) * 1000) + Environment.TickCount;
        }

        public static void OnGameLoad(EventArgs args)
        {
            TrackingList = new List<Obj_AI_Minion>();

            GameObject.OnCreate += GameObjectOnCreate;
            GameObject.OnDelete += GameObjectOnDelete;
            Game.OnProcessPacket += OnProcessPacket;
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnPreReset += DrawingOnPreReset;
            Drawing.OnPostReset += DrawingOnPostReset;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
           

            LoadMenu();
            Game.PrintChat("<font color=\"#00BFFF\">God Jungle Tracker</font> <font color=\"#FFFFFF\"> - 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847 !!</font>");

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name.Contains("SRU_") || x.Name.Contains("Sru_")))
            {
                for (int i = 0; i <= 35; i++)
                {
                    if (!menu.Item("dragon").GetValue<bool>() && i == 0) continue;
                    if (!menu.Item("baron").GetValue<bool>() && i == 1) continue;
                    if (!menu.Item("blue").GetValue<bool>() && i >= 2 && i <= 7) continue;
                    if (!menu.Item("red").GetValue<bool>() && i >= 8 && i <= 13) continue;
                    if (!menu.Item("gromp").GetValue<bool>() && i >= 14 && i <= 15) continue;
                    if (!menu.Item("raptor").GetValue<bool>() && i >= 16 && i <= 23) continue;
                    if (!menu.Item("crab").GetValue<bool>() && i >= 24 && i <= 25) continue;

                    if (minion.Name.Contains(NameToCompare[i]))
                    {
                        if (!minion.IsDead && NetworkID[i] != minion.NetworkId)
                        {
                            NetworkID[i] = minion.NetworkId;
                            State[i] = 1;
                            LastChangeOnState[i] = Environment.TickCount;
                            //Console.WriteLine(minion.NetworkId + " Name: " + minion.Name);
                            TrackingList.Add(minion);
                            
                        }
                    }
                }
            }

            MinimapText = new Font(Drawing.Direct3DDevice,
                     new FontDescription
                     {
                         FaceName = "Calibri",
                         Height = menu.Item("timerfontminimap").GetValue<Slider>().Value,
                         OutputPrecision = FontPrecision.Default,
                         Quality = FontQuality.Default
                     });

            MapText = new Font(Drawing.Direct3DDevice,
                    new FontDescription
                    {
                        FaceName = "Calibri",
                        Height = menu.Item("timerfontmap").GetValue<Slider>().Value,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });
            

            if (Game.ClockTime > 400f)
            {
                GuessNetworkID1 = 0;

                GuessNetworkID2 = 0;
            }

            int c = 0;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                HeroNetworkID[c] = hero.NetworkId;
                HeroName[c] = hero.BaseSkinName;
                //Console.WriteLine(HeroName[c] + " ; " + HeroNetworkID[c]);
                c++;
                if (hero.NetworkId > BiggestNetworkID)
                {
                    BiggestNetworkID = hero.NetworkId;
                }

                if (!hero.IsAlly)
                {
                    for (int i = 0; i <= 8; i++)
                    {
                        if (hero.ChampionName.Contains(Heros[i]))
                        {
                            GuessDragonID = 0;
                        }
                    }
                }
            }
        }

        private static void PlaySound(SoundPlayer sound = null)
        {
            if (sound != null)
            {
                try
                {
                    sound.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static void GameObjectOnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion) || sender.Team != GameObjectTeam.Neutral)
            {
                return;
            }

            var minion = (Obj_AI_Minion)sender;
            var n = minion.Name;


            for (int i = 0; i <= 35; i++)
            {
                if (!menu.Item("dragon").GetValue<bool>() && i == 0) continue;
                if (!menu.Item("baron").GetValue<bool>() && i == 1) continue;
                if (!menu.Item("blue").GetValue<bool>() && i >= 2 && i <= 7) continue;
                if (!menu.Item("red").GetValue<bool>() && i >= 8 && i <= 13) continue;
                if (!menu.Item("gromp").GetValue<bool>() && i >= 14 && i <= 15) continue;
                if (!menu.Item("raptor").GetValue<bool>() && i >= 16 && i <= 23) continue;
                if (!menu.Item("crab").GetValue<bool>() && i >= 24 && i <= 25) continue;

                if (NameToCompare[i] == n)
                {
                    NetworkID[i] = minion.NetworkId;
                    State[i] = 1;
                    LastChangeOnState[i] = Environment.TickCount;
                    JustDied[i] = 0;
                    //Console.WriteLine("Added " + minion.Name + " to the Tracking List " + minion.NetworkId);
                    TrackingList.Add(minion);

                    if (i == 0 && Game.ClockTime < (BaronSpawn + ClockTimeAdjust))
                    {
                        ClockTimeAdjust = 0;
                        BaronSpawn = Game.ClockTime-1;
                    }
                    return;
                }
            }
        }

        private static void GameObjectOnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion) || sender.Team != GameObjectTeam.Neutral)
            {
                return;
            }

            var minion = (Obj_AI_Minion)sender;
            var n = minion.Name;

            for (int i = 0; i <= 35; i++)
            {
                if (!menu.Item("dragon").GetValue<bool>() && i == 0) continue;
                if (!menu.Item("baron").GetValue<bool>() && i == 1) continue;
                if (!menu.Item("blue").GetValue<bool>() && i >= 2 && i <= 7) continue;
                if (!menu.Item("red").GetValue<bool>() && i >= 8 && i <= 13) continue;
                if (!menu.Item("gromp").GetValue<bool>() && i >= 14 && i <= 15) continue;
                if (!menu.Item("raptor").GetValue<bool>() && i >= 16 && i <= 23) continue;
                if (!menu.Item("crab").GetValue<bool>() && i >= 24 && i <= 25) continue;

                if (NameToCompare[i] == n)
                {
                    State[i] = 7;
                    LastChangeOnState[i] = Environment.TickCount - 3000;
                    JustDied[i] = 1;
                    //Console.WriteLine("Removed " + minion.Name + " of the Tracking List");
                    TrackingList.RemoveAll(x => x.Name == minion.Name);
                    
                    return;
                }
            }
        }

        public static void OnGameUpdate(EventArgs args)
        {
            #region Update CampState

            if (l <= 11 && (l < 2 || l > 5) && l != 8 && l != 9)
            {

                var visible = 0;

                CampState[l] = State[UnitToCamp[l]];
                LastChangeOnCampState[l] = LastChangeOnState[UnitToCamp[l]];

                foreach (Obj_AI_Minion minion in TrackingList.Where(x => x.IsVisible && x.Name.Contains(NameToCompare[UnitToCamp[l]]) && !x.IsDead))
                {
                    visible = 1;
                }

                if ((CampState[l] == 7 || CampState[l] == 4) && visible == 1)
                {
                    State[UnitToCamp[l]] = 1;
                    CampState[l] = 1;
                    JustDied[UnitToCamp[l]] = 0;
                }

                if (JustDied[UnitToCamp[l]] == 1)
                {
                    CampRespawnTime[l] = (LastChangeOnCampState[l] + CampRespawnTimer[l] * 1000);
                    JustDied[UnitToCamp[l]] = 0;
                }
            }

            if  (GuessDragonID == 1)
            {

                if (CampState[0] == 6 && ((CampState[1] == 7 || CampState[1] == 0) && CampRespawnTime[1] <= Environment.TickCount + 1000))
                {
                    CampState[0] = 0;
                    State[0] = 0;
                }

                if (CampState[1] == 6 && ((CampState[0] == 7 || CampState[0] == 0) && CampRespawnTime[0] <= Environment.TickCount + 1000))
                {
                    CampState[1] = 0;
                    State[1] = 0;
                }

                if (CampState[0] == 1 || (CampState[0] == 7 && CampRespawnTime[0] > Environment.TickCount + 1000))
                {
                    if ((CampState[1] == 7 || CampState[1] == 0) && CampRespawnTime[1] <= Environment.TickCount + 1000)
                    {
                        State[1] = 6;
                        CampState[1] = 6;
                    }
                }

                if (CampState[1] == 1 || (CampState[1] == 7 && CampRespawnTime[1] > Environment.TickCount + 1000) || CampState[1] == 8)
                {
                    if ((CampState[0] == 7 || CampState[0] == 0) && CampRespawnTime[0] <= Environment.TickCount + 1000)
                    {
                        State[0] = 6;
                        CampState[0] = 6;
                    }
                }
            }


            if ((l >= 2 && l <= 5)) //Red and Blue
            {

                var visible = 0;

                foreach (Obj_AI_Minion minion in TrackingList.Where(x => x.IsVisible && x.Name.Contains(NameToCompare[UnitToCamp[l]]) && !x.IsDead))
                {
                    visible = 1;
                }

                if (visible == 0)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l] - 1], State[UnitToCamp[l] - 2]);
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], LastChangeOnState[UnitToCamp[l] - 2]));
                }

                if ((State[UnitToCamp[l]] == 2 || State[UnitToCamp[l] - 1] == 2 || State[UnitToCamp[l] - 2] == 2))
                {
                    CampState[l] = 2;
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], LastChangeOnState[UnitToCamp[l] - 2]));
                }

                if (visible == 1)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l]] , Math.Min(State[UnitToCamp[l] - 1] ,State[UnitToCamp[l] - 2]));
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], LastChangeOnState[UnitToCamp[l] - 2]));

                    if (CampRespawnTime[l] > Environment.TickCount) CampRespawnTime[l] = (Environment.TickCount + CampRespawnTimer[l] * 1000);
                }

                if (CampState[l] == 7 && JustDied[UnitToCamp[l] - 1] == 1 && JustDied[UnitToCamp[l] - 2] == 1)
                {
                    CampRespawnTime[l] = (LastChangeOnCampState[l] + CampRespawnTimer[l] * 1000);
                    JustDied[UnitToCamp[l]] = 0;
                    JustDied[UnitToCamp[l] - 1] = 0;
                    JustDied[UnitToCamp[l] - 2] = 0;
                }
            }

            if ((l >= 8 && l <= 9)) //Razor
            {

                var visible = 0;

                foreach (Obj_AI_Minion minion in TrackingList.Where(x => x.IsVisible &&
                    (x.Name.Contains(NameToCompare[UnitToCamp[l] - 1]) || x.Name.Contains(NameToCompare[UnitToCamp[l] - 2]) || x.Name.Contains(NameToCompare[UnitToCamp[l] - 3]))
                    && !x.IsDead))
                {
                    visible = 1;
                }

                if (visible == 0)
                {
                    CampState[l] = State[UnitToCamp[l]];
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], Math.Max(LastChangeOnState[UnitToCamp[l] - 2], LastChangeOnState[UnitToCamp[l] - 3])));
                }

                if ((State[UnitToCamp[l]] == 2 || State[UnitToCamp[l] - 1] == 2 || State[UnitToCamp[l] - 2] == 2 || State[UnitToCamp[l] - 3] == 2))
                {
                    CampState[l] = 2;
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], Math.Max(LastChangeOnState[UnitToCamp[l] - 2], LastChangeOnState[UnitToCamp[l] - 3])));
                }

                if (visible == 1)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l]], Math.Min(State[UnitToCamp[l] - 1], Math.Min(State[UnitToCamp[l] - 2], State[UnitToCamp[l] - 3])));
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], Math.Max(LastChangeOnState[UnitToCamp[l] - 2], LastChangeOnState[UnitToCamp[l] - 3])));

                    if (CampRespawnTime[l] > Environment.TickCount) CampRespawnTime[l] = (Environment.TickCount + CampRespawnTimer[l] * 1000);
                }

                if (CampState[l] == 7 && JustDied[UnitToCamp[l]] == 1)
                {
                    CampRespawnTime[l] = (LastChangeOnCampState[l] + CampRespawnTimer[l] * 1000);
                    JustDied[UnitToCamp[l]] = 0;
                    JustDied[UnitToCamp[l] - 1] = 0;
                    JustDied[UnitToCamp[l] - 2] = 0;
                    JustDied[UnitToCamp[l] - 3] = 0;
                }
            }

            if (l == 12 || l == 13) //Krug
            {
                var visible = 0;

                foreach (Obj_AI_Minion minion in TrackingList.Where(x => x.IsVisible &&
                    (x.Name.Contains(NameToCompare[UnitToCamp[l]]) || x.Name.Contains(NameToCompare[UnitToCamp[l] - 1]))
                    && !x.IsDead))
                {
                    visible = 1;
                }

                if (visible == 0)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l]], State[UnitToCamp[l] - 1]);
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], LastChangeOnState[UnitToCamp[l] - 1]);
                }

                if (State[UnitToCamp[l]] == 2 || State[UnitToCamp[l] - 1] == 2)
                {
                    CampState[l] = 2;
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], LastChangeOnState[UnitToCamp[l] - 1]);
                }

                if (visible == 1)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l]], State[UnitToCamp[l] - 1]);
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], LastChangeOnState[UnitToCamp[l] - 1]);

                    if (CampRespawnTime[l] > Environment.TickCount) CampRespawnTime[l] = (Environment.TickCount + CampRespawnTimer[l] * 1000);
                }

                if (CampState[l] == 7 && JustDied[UnitToCamp[l]] == 1 && JustDied[UnitToCamp[l] - 1] == 1)
                {
                    CampRespawnTime[l] = (LastChangeOnCampState[l] + CampRespawnTimer[l] * 1000);
                    JustDied[UnitToCamp[l]] = 0;
                    JustDied[UnitToCamp[l] - 1] = 0;
                }
            }

            if (l == 14 || l == 15) //Wolf
            {
                var visible = 0;

                foreach (Obj_AI_Minion minion in TrackingList.Where(x => x.IsVisible && 
                    (x.Name.Contains(NameToCompare[UnitToCamp[l]]) || x.Name.Contains(NameToCompare[UnitToCamp[l] - 1]) || x.Name.Contains(NameToCompare[UnitToCamp[l] - 2])) 
                    && !x.IsDead))
                {
                    visible = 1;
                }

                if (visible == 0)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l]], Math.Min(State[UnitToCamp[l] - 1], State[UnitToCamp[l] - 2]));
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], LastChangeOnState[UnitToCamp[l] - 2]));
                }

                if ((State[UnitToCamp[l]] == 2 || State[UnitToCamp[l] - 1] == 2 || State[UnitToCamp[l] - 2] == 2))
                {
                    CampState[l] = 2;
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], LastChangeOnState[UnitToCamp[l] - 2]));
                }

                if (visible == 1)
                {
                    CampState[l] = Math.Min(State[UnitToCamp[l]], Math.Min(State[UnitToCamp[l] - 1], State[UnitToCamp[l] - 2]));
                    LastChangeOnCampState[l] = Math.Max(LastChangeOnState[UnitToCamp[l]], Math.Max(LastChangeOnState[UnitToCamp[l] - 1], LastChangeOnState[UnitToCamp[l] - 2]));

                    if (CampRespawnTime[l] > Environment.TickCount) CampRespawnTime[l] = (Environment.TickCount + CampRespawnTimer[l] * 1000);
                }

                if (CampState[l] == 7 && JustDied[UnitToCamp[l]] == 1 && JustDied[UnitToCamp[l] - 1] == 1 && JustDied[UnitToCamp[l] - 2] == 1)
                {
                    CampRespawnTime[l] = (LastChangeOnCampState[l] + CampRespawnTimer[l] * 1000);
                    JustDied[UnitToCamp[l]] = 0;
                    JustDied[UnitToCamp[l] - 1] = 0;
                    JustDied[UnitToCamp[l] - 2] = 0;
                }
            }

            
            
            #endregion

            #region Play Dragon/Baron Sound

            if (CampState[0] != 2)
            {
                BufferDragonSound = 0;
                PlayingDragonSound = 0;
            }

            if (BufferDragonSound > 0 && (Environment.TickCount - LastPlayedDragonSound > 500) && CampState[0] == 2)
            {
                LastPlayedDragonSound = Environment.TickCount;
                PlaySound(sound);
                BufferDragonSound -= 1;
                if (BufferDragonSound == 0) PlayingDragonSound = 0;
            }

            if ((menu.Item("soundfow").GetValue<bool>() && OnFow[1] == 0) || !menu.Item("soundfow").GetValue<bool>())
            {
                if ((menu.Item("soundscreen").GetValue<bool>() && OnScreen[1] == 0) || !menu.Item("soundscreen").GetValue<bool>())
                {
                    if ((CampState[1] == 2 && menu.Item("baronsound").GetValue<bool>()) && (Environment.TickCount - BaronSoundDelay >= (menu.Item("sounddelay").GetValue<Slider>().Value * 1000)))
                    {
                        for (int i = 1; i <= menu.Item("baronsoundtimes").GetValue<Slider>().Value; i++)
                        {
                            if (i == 1 && (LastPlayedBaronSound == 0 || Environment.TickCount - LastPlayedBaronSound >= (menu.Item("sounddelay").GetValue<Slider>().Value * 1000)) && Environment.TickCount - LastChangeOnCampState[1] < 500)
                            {
                                LastPlayedBaronSound = Environment.TickCount;
                                LastPlayedBaronSound2 = Environment.TickCount;
                                PlaySound(sound);
                            }
                            else if (i > 1 && Environment.TickCount - LastPlayedBaronSound > 550 * (i - 1) && Environment.TickCount - LastPlayedBaronSound < 600 * (i - 1) && Environment.TickCount - LastPlayedBaronSound2 > 500)
                            {
                                LastPlayedBaronSound += (50 * (i - 1));

                                LastPlayedBaronSound2 = Environment.TickCount;


                                if (i == menu.Item("baronsoundtimes").GetValue<Slider>().Value)
                                {
                                    BaronSoundDelay = Environment.TickCount;
                                }
                                PlaySound(sound);
                            }
                        }
                    }
                }
            }

            #endregion

            #region Sound Volume Update

            if (menu.Item("soundvolume").GetValue<StringList>().SelectedIndex.Equals(0))
            {
                sound = danger10;
            }
            else if (menu.Item("soundvolume").GetValue<StringList>().SelectedIndex.Equals(1))
            {
                sound = danger25;
            }
            else if (menu.Item("soundvolume").GetValue<StringList>().SelectedIndex.Equals(2))
            {
                sound = danger50;
            }
            else if (menu.Item("soundvolume").GetValue<StringList>().SelectedIndex.Equals(3))
            {
                sound = danger75;
            }
            else if (menu.Item("soundvolume").GetValue<StringList>().SelectedIndex.Equals(4))
            {
                sound = danger;
            }

            #endregion

            #region Guess Blue/Red NetworkID

            if (GuessNetworkID1 == 1 && NetworkID[4] != 0 &&NetworkID[3] != 0 && NetworkID[2] != 0)
            {
                Seed1 = (NetworkID[3] - NetworkID[4]);
                Seed2 = (NetworkID[2] - NetworkID[3]);

                //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                int id = 0;

                for (int c = 0; c <= 31; c++)
                {
                    int order = CreateOrder[c];

                    if (c == 2)
                    {
                        id += Seed1;
                        id += Seed2;
                    }
                    else
                    {
                        if (SeedOrder[c] == 1) id += Seed1;
                        else id += Seed2;
                    }

                    IDOrder[order] = id;
                }

            
                for (int j = 5; j <= 7; j++)
                {
                    if (j == 4 || IDOrder[j] == 0) continue;

                    if (NetworkID[j] == 0)
                    {
                        if (IDOrder[j] < IDOrder[4])
                        {
                            NetworkID[j] = NetworkID[4] - ((IDOrder[4] - IDOrder[j]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        else if (IDOrder[j] > IDOrder[4])
                        {
                            NetworkID[j] = NetworkID[4] + ((IDOrder[j] - IDOrder[4]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        //Console.WriteLine("NetworkID[" + j + "]:" + NetworkID[j] + " and Name: " + NameToCompare[j]);
                    }
                }
                GuessNetworkID1 = 0;
            }
            else if (GuessNetworkID1 == 1 && NetworkID[7] != 0 && NetworkID[6] != 0 && NetworkID[5] != 0 && NetworkID[7] < NetworkID[6])
            {
                Seed1 = (NetworkID[6] - NetworkID[7]);
                Seed2 = (NetworkID[5] - NetworkID[6]);

                //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                int id = 0;

                for (int c = 0; c <= 31; c++)
                {
                    int order = CreateOrder[c];

                    if (c == 2)
                    {
                        id += Seed1;
                        id += Seed2;
                    }
                    else
                    {
                        if (SeedOrder[c] == 1) id += Seed1;
                        else id += Seed2;
                    }

                    IDOrder[order] = id;
                }

                for (int j = 2; j <= 4; j++)
                {
                    if (j == 7 || IDOrder[j] == 0) continue;

                    if (NetworkID[j] == 0)
                    {
                        if (IDOrder[j] < IDOrder[7])
                        {
                            NetworkID[j] = NetworkID[7] - ((IDOrder[7] - IDOrder[j]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        else if (IDOrder[j] > IDOrder[7])
                        {
                            NetworkID[j] = NetworkID[7] + ((IDOrder[j] - IDOrder[7]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        //Console.WriteLine("NetworkID[" + j + "]:" + NetworkID[j] + " and Name: " + NameToCompare[j]);
                    }
                }
                GuessNetworkID1 = 0;
            }

            else if (GuessNetworkID2 == 1 && NetworkID[10] != 0 && NetworkID[9] != 0 && NetworkID[8] != 0 &&  NetworkID[10] < NetworkID[9])
            {
                Seed1 = (NetworkID[9] - NetworkID[10]);
                Seed2 = (NetworkID[8] - NetworkID[9]);

                //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                int id = 0;

                for (int c = 0; c <= 31; c++)
                {
                    int order = CreateOrder[c];

                    if (c == 2)
                    {
                        id += Seed1;
                        id += Seed2;
                    }
                    else
                    {
                        if (SeedOrder[c] == 1) id += Seed1;
                        else id += Seed2;
                    }

                    IDOrder[order] = id;
                }

                for (int j = 11; j <= 13; j++)
                {
                    if (j == 10 || IDOrder[j] == 0) continue;

                    if (NetworkID[j] == 0)
                    {
                        if (IDOrder[j] < IDOrder[10])
                        {
                            NetworkID[j] = NetworkID[10] - ((IDOrder[10] - IDOrder[j]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        else if (IDOrder[j] > IDOrder[10])
                        {
                            NetworkID[j] = NetworkID[10] + ((IDOrder[j] - IDOrder[10]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        //Console.WriteLine("NetworkID[" + j + "]:" + NetworkID[j] + " and Name: " + NameToCompare[j]);
                    }
                }
                GuessNetworkID2 = 0;
            }

            else if (GuessNetworkID2 == 1 && NetworkID[13] != 0 && NetworkID[12] != 0 && NetworkID[11] != 0 && NetworkID[13] < NetworkID[12])
            {
                Seed1 = (NetworkID[12] - NetworkID[13]);
                Seed2 = (NetworkID[11] - NetworkID[12]);

                //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                int id = 0;

                for (int c = 0; c <= 31; c++)
                {
                    int order = CreateOrder[c];

                    if (c == 2)
                    {
                        id += Seed1;
                        id += Seed2;
                    }
                    else
                    {
                        if (SeedOrder[c] == 1) id += Seed1;
                        else id += Seed2;
                    }

                    IDOrder[order] = id;
                }

                for (int j = 8; j <= 10; j++)
                {
                    if (j == 13 || IDOrder[j] == 0) continue;

                    if (NetworkID[j] == 0)
                    {
                        if (IDOrder[j] < IDOrder[13])
                        {
                            NetworkID[j] = NetworkID[13] - ((IDOrder[13] - IDOrder[j]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        else if (IDOrder[j] > IDOrder[13])
                        {
                            NetworkID[j] = NetworkID[13] + ((IDOrder[j] - IDOrder[13]));
                            State[j] = 5;
                            LastChangeOnState[j] = Environment.TickCount;
                        }
                        //Console.WriteLine("NetworkID[" + j + "]:" + NetworkID[j] + " and Name: " + NameToCompare[j]);
                    }
                }
                GuessNetworkID2 = 0;
            }

            #endregion

            #region Dragon/Baron vision and OnScreen

            foreach (Obj_AI_Minion minion in TrackingList.Where(x => x.Name.Contains("Baron") || x.Name.Contains("Dragon")))
            {
                for (int i = 0; i <= 1; i++)
                {

                    if (!menu.Item("dragon").GetValue<bool>() && i == 0) continue;
                    if (!menu.Item("baron").GetValue<bool>() && i == 1) continue;


                    if (minion.Name.Contains(NameToCompare[i]))
                    {
                        if (CampPosition[i].IsOnScreen()) OnScreen[i] = 1;

                        else OnScreen[i] = 0;

                        if (minion.IsVisible)
                        {
                            OnFow[i] = 1;
                        }
                        else if (!minion.IsVisible && i < 2)
                        {
                            OnFow[i] = 0;
                            OnScreen[i] = 0;
                        }
                    }
                }
            }

            #endregion

            #region Get Dragon/Baron buff 

            var enemy = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget());
            var ally = HeroManager.Allies.FirstOrDefault(x => x.IsValidTarget(50000f, false));

            if (enemy != null)
            {
                if (GetDragonStacks(enemy) > EnemyTeamStacks)
                {
                    EnemyTeamStacks = EnemyTeamStacks + 1;

                    if (Environment.TickCount - EnemyFoWTime > 100 || (Environment.TickCount - EnemyFoWTime <= 100 && (State[0] != 4 && State[0] != 7)))
                    {
                        State[0] = 4;
                        LastChangeOnState[0] = Environment.TickCount;
                        LastChangeOnCampState[0] = Environment.TickCount;
                        CampRespawnTime[0] = (LastChangeOnCampState[0] + CampRespawnTimer[0] * 1000);
                    }
                    //Console.WriteLine("Enemy Dragon Stacks: " + EnemyTeamStacks);
                }
                else if (GetNashorBuff(enemy) && !EnemyTeamNashor)
                {
                    EnemyTeamNashor = true;

                    if (Environment.TickCount - EnemyFoWTime > 100 || (Environment.TickCount - EnemyFoWTime <= 100 && State[1] != 4 && State[1] != 7))
                    {
                        State[1] = 4;
                        LastChangeOnState[1] = Environment.TickCount;
                        LastChangeOnCampState[1] = Environment.TickCount;
                        CampRespawnTime[1] = (LastChangeOnCampState[1] + CampRespawnTimer[1] * 1000);
                    }
                    //Console.WriteLine("Enemy Baron: " + EnemyTeamNashor);
                }
                else if (GetDragonStacks(enemy) < EnemyTeamStacks)
                {
                    EnemyTeamStacks--;
                    //Console.WriteLine("Enemy Dragon Stacks: " + EnemyTeamStacks);
                }
                else if (!GetNashorBuff(enemy) && EnemyTeamNashor)
                {
                    EnemyTeamNashor = false;
                    //Console.WriteLine("Enemy Baron: " + EnemyTeamNashor);
                }
            }
            else
            {
                EnemyFoWTime = Environment.TickCount;
            }

            if (ally != null)
            {
                if (GetDragonStacks(ally) > AllyTeamStacks)
                {
                    AllyTeamStacks = AllyTeamStacks + 1;

                    if (Environment.TickCount - AllyFoWTime > 100 || (Environment.TickCount - AllyFoWTime <= 100 && (State[0] != 4 && State[0] != 7)))
                    {
                        State[0] = 4;
                        LastChangeOnState[0] = Environment.TickCount;
                        LastChangeOnCampState[0] = Environment.TickCount;
                        CampRespawnTime[0] = (LastChangeOnCampState[0] + CampRespawnTimer[0] * 1000);
                    }
                    //Console.WriteLine("Ally Dragon Stacks: " + AllyTeamStacks);
                }
                else if (GetNashorBuff(ally) && !AllyTeamNashor)
                {
                    AllyTeamNashor = true;

                    if (Environment.TickCount - AllyFoWTime > 100 || (Environment.TickCount - AllyFoWTime <= 100 && State[1] != 4 && State[1] != 7))
                    {
                        State[1] = 4;
                        LastChangeOnState[1] = Environment.TickCount;
                        LastChangeOnCampState[1] = Environment.TickCount;
                        CampRespawnTime[1] = (LastChangeOnCampState[1] + CampRespawnTimer[1] * 1000);
                    }
                    //Console.WriteLine("Ally Baron: " + AllyTeamNashor);
                }
                else if (GetDragonStacks(ally) < AllyTeamStacks)
                {
                    AllyTeamStacks--;
                    //Console.WriteLine("Ally Dragon Stacks: " + AllyTeamStacks);
                }
                else if (!GetNashorBuff(ally) && AllyTeamNashor)
                {
                    AllyTeamNashor = false;
                    //Console.WriteLine("Ally Baron: " + AllyTeamNashor);
                }
            }
            else
            {
                AllyFoWTime = Environment.TickCount;
            }

            if (ally != null && enemy != null && AllyTeamNashor == false && EnemyTeamNashor == false && State[1] == 4)
            {
                State[1] = 1;
            }

            #endregion  //Credits to Inferno

            #region Update States

            int t = 3000;

            if (l == 0)
            {
                if (Game.ClockTime - ClockTimeAdjust < 420f) t = 60000;
                else if (Game.ClockTime - ClockTimeAdjust < 820f) t = 40000;
                else  t = 15000;
            }

            if (l == 1 && State[l] == 8 && CampRespawnTime[l] <= Environment.TickCount)
            {
                State[l] = 0;
            }

            if (State[l] == 2 && (Environment.TickCount - LastChangeOnState[l]) >= t && !(NameToCompare[l].Contains("Crab")))    //presumed dead
            {
                if (l <= 1)
                {
                    if (OnFow[l] == 0 && enemy == null)
                    {
                        State[l] = 4;
                        LastChangeOnState[l] = Environment.TickCount - 2000;
                    }
                }
                else
                {
                    State[l] = 4;
                    LastChangeOnState[l] = Environment.TickCount - 2000;
                }
            }
            else if (State[l] == 2 && (Environment.TickCount - LastChangeOnState[l]) >= 10000 && (NameToCompare[l].Contains("Crab")))
            {
                State[l] = 1;
                LastChangeOnState[l] = Environment.TickCount;
            }

            else if (State[l] == 3 && (Environment.TickCount - LastChangeOnState[l]) >= 2000)    //after desingaged wait 2 sec's
            {
                State[l] = 1;
                LastChangeOnState[l] = Environment.TickCount;
            }

            else if (State[l] == 4 && (Environment.TickCount - LastChangeOnState[l]) >= 5000)
            {
                State[l] = 7;
                JustDied[l] = 1;
            }

            else if (State[l] == 5 && (Environment.TickCount - LastChangeOnState[l]) >= 30000)  
            {
                State[l] = 0;
            }


            #endregion

            l++;
            if (l > 35) l = 0;
        }

        static int GetDragonStacks(Obj_AI_Hero hero)
        {
            var Baff = hero.Buffs.Where(Buff => Buff.Name == "s5test_dragonslayerbuff").FirstOrDefault();
            if (Baff == null)
                return 0;
            else
                return Baff.Count;
        }//Credits to Inferno

        static bool GetNashorBuff(Obj_AI_Hero hero)
        {
            return hero.Buffs.Any(Buff => Buff.Name == "exaltedwithbaronnashor");
        } //Credits to Inferno

        private static void OnProcessPacket(GamePacketEventArgs args)
        {
            short header = BitConverter.ToInt16(args.PacketData, 0);

            #region Update States

            for (int i = 0; i <= 25; i++)
            {
                if (NetworkID[i] == 0) continue;

                if (BitConverter.ToInt32(args.PacketData, 2) == NetworkID[i])
                {
                    if (header == HeaderSkill)
                    {
                        //Console.WriteLine(NameToCompare[i] + " is Attacking");   //"using skill" or crab dead

                        if (NameToCompare[i].Contains("Crab"))
                        {
                            State[i] = 4;
                        }
                        else
                        {
                            if (NameToCompare[i].Contains("Dragon") && State[i] != 2)
                            {
                                if (BufferDragonSound == 0 && PlayingDragonSound == 0 && ((menu.Item("soundfow").GetValue<bool>() && OnFow[0] == 0) || !menu.Item("soundfow").GetValue<bool>()))
                                {
                                    if ((menu.Item("soundscreen").GetValue<bool>() && OnScreen[0] == 0) || !menu.Item("soundscreen").GetValue<bool>())
                                    {
                                        if ((menu.Item("dragonsound").GetValue<bool>()))
                                        {
                                            BufferDragonSound = menu.Item("dragonsoundtimes").GetValue<Slider>().Value;
                                            PlayingDragonSound = 1;
                                            CampState[0] = 2;
                                        }
                                    }
                                }
                            }
                            State[i] = 2;
                        }
                        LastChangeOnState[i] = Environment.TickCount;
                    }

                    else if (header == HeaderMeleeAttack)
                    {
                        //Console.WriteLine(NameToCompare[i] + " is Attacking");

                        State[i] = 2;
                        LastChangeOnState[i] = Environment.TickCount;
                    }

                    else if (header == HeaderRangedAttack)
                    {
                        //Console.WriteLine(NameToCompare[i] + " is Attacking (ranged)");

                        State[i] = 2;
                        LastChangeOnState[i] = Environment.TickCount;
                    }

                    else if (header == HeaderDisengaged)
                    {
                        //Console.WriteLine(NameToCompare[i] + " is Disengaged");
                        if (NameToCompare[i].Contains("Crab"))
                        {
                            if (State[i] == 0) State[i] = 5;
                            else State[i] = 2;
                        }
                        else
                        {
                            if (State[i] == 0) State[i] = 5;
                            else State[i] = 3;
                        }
                        LastChangeOnState[i] = Environment.TickCount;
                    }
                }
            }

            #endregion

            #region Guess Dragon/Baron NetworkID

            if (header == HeaderSkill &&
                BitConverter.ToInt32(args.PacketData, 2) != NetworkID[0] &&
                BitConverter.ToInt32(args.PacketData, 2) != NetworkID[1] &&
                BitConverter.ToInt32(args.PacketData, 2) > BiggestNetworkID &&
                BitConverter.ToString(args.PacketData, 0).Length == 47 &&
                GuessDragonID == 1
                )
            {
                bool AI_Base = false;
                foreach (Obj_AI_Base obj in ObjectManager.Get<Obj_AI_Base>().Where(x => x.NetworkId == BitConverter.ToInt32(args.PacketData, 2)))
                {
                    if (!obj.IsAlly)
                    {
                        if (!obj.Name.Contains("SRU_Dragon") && !obj.Name.Contains("SRU_Baron") )//&& !obj.Name.Contains("TestCube"))
                        {
                            Game.PrintChat("<font color=\"#FF0000\"> God Jungle Tracker (debug): Tell AlphaGod he forgot to consider: " + obj.Name + " - " + obj.SkinName + " - " + obj.BaseSkinName + " - Guess Dragon NetWorkID disabled</font>");
                            GuessDragonID = 0;
                        }
                    }
                    AI_Base = true;
                }

                if (!AI_Base)
                {
                    if (State[1] == 1 || (State[1] == 7 && CampRespawnTime[1] > Environment.TickCount + 1000) || State[1] == 8)
                    {
                        if (BufferDragonSound == 0 && PlayingDragonSound == 0 && ((menu.Item("soundfow").GetValue<bool>() && OnFow[0] == 0) || !menu.Item("soundfow").GetValue<bool>()))
                        {
                            if ((menu.Item("soundscreen").GetValue<bool>() && OnScreen[0] == 0) || !menu.Item("soundscreen").GetValue<bool>())
                            {
                                if ((menu.Item("dragonsound").GetValue<bool>()))
                                {
                                    BufferDragonSound = menu.Item("dragonsoundtimes").GetValue<Slider>().Value;
                                    PlayingDragonSound = 1;
                                    CampState[0] = 2;
                                }
                            }
                        }

                        State[0] = 2;
                        LastChangeOnState[0] = Environment.TickCount;
                        NetworkID[0] = BitConverter.ToInt32(args.PacketData, 2);
                    }
                    else if (State[0] == 1 || (State[0] == 7 && CampRespawnTime[0] > Environment.TickCount + 1000))
                    {
                        State[1] = 2;
                        LastChangeOnState[1] = Environment.TickCount;
                        NetworkID[1] = BitConverter.ToInt32(args.PacketData, 2);
                    }
                }
            }

            #endregion

            #region Gromp Created

            if (header == HeaderCreateGromp)  //Gromp Created
            {
                if (BitConverter.ToString(args.PacketData, 0).Length == 284)
                {
                    NetworkID[14] = BitConverter.ToInt32(args.PacketData, 2);
                    State[14] = 6;
                    LastChangeOnState[14] = Environment.TickCount;

                    if (Game.ClockTime - 111f < 90 && ClockTimeAdjust == 0)
                    {
                        ClockTimeAdjust = Game.ClockTime - 111f;
                        State[0] = 0;
                        CampRespawnTime[0] = Environment.TickCount + 39000;
                        CampState[0] = 0;
                        BiggestNetworkID = BitConverter.ToInt32(args.PacketData, 2);
                    }
                }
                else if (BitConverter.ToString(args.PacketData, 0).Length == 293)
                {
                    NetworkID[15] = BitConverter.ToInt32(args.PacketData, 2);
                    State[15] = 6;
                    LastChangeOnState[15] = Environment.TickCount;
                }
            }
            #endregion
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            Color cor = Color.FromArgb(255, 0, 255, 0);

            int c = 0;

            /*
            CampState == 0 Not Tracking
            CampState == 1 Tracking/Iddle
            CampState == 2 Attacking
            CampState == 3 Disengaged
            CampState == 4 Presumed Dead
            CampState == 5 Guessed on fow
            CampState == 6 Guessed on fow
            CampState == 7 on timer to respawn
            */

            for (int i = 0; i <= 11; i++)
            {

                if (!menu.Item("drawtracklist").GetValue<bool>()) break;

                if (CampState[i] > 0 && CampState[i] != 7)
                {
                    int x = menu.Item("posX").GetValue<Slider>().Value;
                    int y = menu.Item("posY").GetValue<Slider>().Value - c * 30;

                    c += 1;

                    if (CampName[i].Contains("Dragon"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 255, 165, 15), CampName[i]);
                    }
                    else if (CampName[i].Contains("Baron"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 153, 50, 204), CampName[i]);
                    }
                    else if (CampName[i].Contains("Blue"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 0, 255, 255), CampName[i]);
                    }
                    else if (CampName[i].Contains("Red"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 255, 0, 0), CampName[i]);
                    }
                    else if (CampName[i].Contains("Crab"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 152, 251, 152), CampName[i]);
                    }
                    else if (CampName[i].Contains("Raptor"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 255, 127, 80), CampName[i]);
                    }
                    else if (CampName[i].Contains("Gromp"))
                    {
                        Drawing.DrawText(x, y, Color.FromArgb(255, 0, 139, 45), CampName[i]);
                    }
                    if (CampState[i] == 1)
                    {
                        cor = Color.FromArgb(255, 0, 255, 0);
                    }
                    else if (CampState[i] == 2)
                    {
                        cor = Color.FromArgb(255, 255, 0, 0);
                    }
                    else if (CampState[i] == 3)
                    {
                        cor = Color.FromArgb(255, 255, 210, 0);
                    }
                    else if (CampState[i] == 4)
                    {
                        cor = Color.FromArgb(255, 200, 200, 200);
                    }
                    else if (CampState[i] == 5 || CampState[i] == 6)
                    {
                        cor = Color.Cyan;
                    }


                    Drawing.DrawLine(
                        new Vector2(x - 4.5f, y - 5),
                        new Vector2(x + 70, y - 5), 3, cor);

                    Drawing.DrawLine(
                        new Vector2(x + 70, y - 4.5f),
                        new Vector2(x + 70, y + 21), 3, cor);

                    Drawing.DrawLine(
                        new Vector2(x + 70, y + 21),
                        new Vector2(x - 3, y + 21), 3, cor);

                    Drawing.DrawLine(
                        new Vector2(x - 5, y + 21),
                        new Vector2(x - 5, y - 4.5f), 3, cor);
                }
            }

            if (c == 0 && menu.Item("drawtracklist").GetValue<bool>() )
            {
                cor = Color.FromArgb(255, 255, 255, 255);

                int x = menu.Item("posX").GetValue<Slider>().Value;
                int y = menu.Item("posY").GetValue<Slider>().Value - c * 30;

                Drawing.DrawText(x, y, cor, "TrackList");

                Drawing.DrawLine(
                        new Vector2(x - 4.5f, y - 5),
                        new Vector2(x + 70, y - 5), 3, cor);

                Drawing.DrawLine(
                    new Vector2(x + 70, y - 4.5f),
                    new Vector2(x + 70, y + 21), 3, cor);

                Drawing.DrawLine(
                    new Vector2(x + 70, y + 21),
                    new Vector2(x - 3, y + 21), 3, cor);

                Drawing.DrawLine(
                    new Vector2(x - 5, y + 21),
                    new Vector2(x - 5, y - 4.5f), 3, cor);

            }
        }

        public static void Drawing_OnEndScene(EventArgs args)
        {

            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
            {
                return;
            }

            for (int i = 0; i <= 15; i++)
            {

                if (CampRespawnTime[i] > Environment.TickCount && CampState[i] == 7)
                {
                    var t = TimeSpan.FromSeconds(((float)CampRespawnTime[i] - (float)Environment.TickCount) / 1000f);

                    bool format = false;

                    string text = string.Format(format ? "{1}" : "{0}:{1:00}", (int)t.TotalMinutes, format ? (int)t.TotalSeconds : t.Seconds);

                    ColorBGRA white = new ColorBGRA(255, 255, 255, 255);

                    var pos = Drawing.WorldToScreen(CampPosition[i]);

                    var pos2 = Drawing.WorldToMinimap(CampPosition[i]);

                    if (CampPosition[i].IsOnScreen() && menu.Item("timeronmap").GetValue<bool>())
                    {
                        if (menu.Item("timeronmapformat").GetValue<StringList>().SelectedIndex.Equals(0))
                        {
                            format = false;
                        }
                        else
                        {
                            format = true;
                        }

                        text = string.Format(format ? "{1}" : "{0}:{1:00}", (int)t.TotalMinutes, format ? (int)t.TotalSeconds : t.Seconds);

                        MapText.DrawText(null, text, (int)(pos.X - MapText.MeasureText(null, text, FontDrawFlags.Center).Width / 2f),
                                (int)(pos.Y - MapText.MeasureText(null, text, FontDrawFlags.Center).Height / 2f), white);
                    }

                    if (menu.Item("timeronminimap").GetValue<bool>())
                    {
                        if (menu.Item("timeronminimapformat").GetValue<StringList>().SelectedIndex.Equals(0))
                        {
                            format = false;
                        }
                        else
                        {
                            format = true;
                        }

                        text = string.Format(format ? "{1}" : "{0}:{1:00}", (int)t.TotalMinutes, format ? (int)t.TotalSeconds : t.Seconds);

                        MinimapText.DrawText(null, text, (int)(pos2.X - MinimapText.MeasureText(null, text, FontDrawFlags.Center).Width / 2f),
                                (int)(pos2.Y - MinimapText.MeasureText(null, text, FontDrawFlags.Center).Height / 2f), white);
                    }
                }

                if (i > 11) continue;


                if (!menu.Item("TrackonMinimap").GetValue<bool>()) break;

                if ((i == 0 || i == 6 || i == 7) && CampState[i] == 2 && (Environment.TickCount - LastChangeOnCampState[i]) < 60000)
                {
                    Utility.DrawCircle(CampPosition[i], 500, Color.Red, 2, 15, true);
                }
                else if (i != 0 && CampState[i] == 2 && (Environment.TickCount - LastChangeOnCampState[i]) < 10000)
                {
                    Utility.DrawCircle(CampPosition[i], 500, Color.Red, 2, 15, true);
                }
                else if (CampState[i] == 1)
                {
                    Utility.DrawCircle(CampPosition[i], 400, Color.Lime, 1, 15, true);
                }
                else if (CampState[i] == 3)
                {
                    Utility.DrawCircle(CampPosition[i], 450, Color.FromArgb(255, 255, 210, 0), 2, 15, true);
                }
                else if (CampState[i] == 4)
                {
                    Utility.DrawCircle(CampPosition[i], 400, Color.FromArgb(255, 200, 200, 200), 1, 15, true);
                }
                else if (CampState[i] == 5 || CampState[i] == 6)
                {
                    Utility.DrawCircle(CampPosition[i], 400, Color.Cyan, 1, 15, true);
                }
            }
        }

        private static void DrawingOnPostReset(EventArgs args)
        {
            MapText.OnResetDevice();
            MinimapText.OnResetDevice();
        }

        private static void DrawingOnPreReset(EventArgs args)
        {
            MapText.OnLostDevice();
            MinimapText.OnLostDevice();
        }

        static void LoadMenu()
        {
            //Start Menu
            menu = new Menu("上帝打野追踪", "God Jungle Tracker", true);

            //Camp to Track Menu
            menu.AddSubMenu(new Menu("野区追踪", "Camp to Track"));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("baron", "男爵").SetValue(true));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("dragon", "小龙").SetValue(true));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("blue", "蓝buff").SetValue(true));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("red", "红buff").SetValue(true));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("crab", "螃蟹").SetValue(true));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("raptor", "肉食鸟").SetValue(true));
            menu.SubMenu("Camp to Track").AddItem(new MenuItem("gromp", "石头人").SetValue(true));

            //Track List Menu
            menu.AddSubMenu(new Menu("轨道 目录", "Track List"));
            menu.SubMenu("Track List").AddItem(new MenuItem("drawtracklist", "显示轨道目录").SetValue(false));
            menu.SubMenu("Track List").AddItem(new MenuItem("posX", "Track List Pos X").SetValue(new Slider(Drawing.Width / 2, 0, Drawing.Width)));
            menu.SubMenu("Track List").AddItem(new MenuItem("posY", "Track List Pos Y").SetValue(new Slider(Drawing.Height / 2, 0, Drawing.Height)));

            //Play Danger Sound
            menu.AddSubMenu(new Menu("危险声音提醒", "Play Danger Sound"));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("dragonsound", "小龙第一次被抢").SetValue(true));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("dragonsoundtimes", "播放声音选择").SetValue(new Slider(2, 1, 4)));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("baronsound", "男爵被击杀").SetValue(true));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("baronsoundtimes", "播放声音选择").SetValue(new Slider(2, 1, 4)));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("sounddelay", "播放声音延迟 (s)").SetValue(new Slider(20, 1, 60)));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("soundfow", "只在战争的迷雾").SetValue(false));
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("soundscreen", "只有当野区不在屏幕时播放").SetValue(true));
            String [] volume = {"10%","25%","50%","75%","100%"};
            menu.SubMenu("Play Danger Sound").AddItem(new MenuItem("soundvolume", "提醒音量").SetValue(new StringList(volume, 2)));

            //Timers
            menu.AddSubMenu(new Menu("打野计时", "Timers"));
            String[] format = { "mm:ss", "ss" };
            var Timers = menu.SubMenu("Timers");
            Timers.SubMenu("On Map").AddItem(new MenuItem("timeronmapformat", "格式 ").SetValue(new StringList(format, 0)));
            Timers.SubMenu("On Map").AddItem(new MenuItem("timerfontmap", "文字大小").SetValue(new Slider(20, 3, 30)));
            Timers.SubMenu("On Map").AddItem(new MenuItem("timeronmap", "启用").SetValue(true));
            Timers.SubMenu("On Minimap").AddItem(new MenuItem("timeronminimapformat", "格式 ").SetValue(new StringList(format, 0)));
            Timers.SubMenu("On Minimap").AddItem(new MenuItem("timerfontminimap", "文字高度").SetValue(new Slider(13, 3, 30)));
            Timers.SubMenu("On Minimap").AddItem(new MenuItem("timeronminimap", "启用").SetValue(true));
            
            //Track on Minimap
            menu.AddItem(new MenuItem("TrackonMinimap", "小地图追踪").SetValue(true));

            menu.AddToMainMenu();
        }
    }
}
