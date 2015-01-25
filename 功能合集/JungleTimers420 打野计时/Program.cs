using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace JungleTimerHax
{
    internal class Program
    {
        public static Menu Config;
        public const String BaseUrl = "http://www.lolnexus.com/ajax/get-game-info/";
        public const String UrlPartial = ".json?name=";
        public const String SearchString = "lrf://spectator ";
        
        public static Dictionary<UInt32, Vector3> junglePos = new Dictionary<UInt32, Vector3>();
        public static Dictionary<UInt32, Single> jungleRespawns = new Dictionary<UInt32, Single>();
        public static String Key;
        public static String GameId;
        public static String PlatformId;
        public static String RegionTag;
        public static String SpecUrl;
        public static Single TimeOffset = 0;
        private static void Main(string[] args)
        {
            
            junglePos[0x1] = new Vector3(3632.7f, 7600.373f, 60.0f); jungleRespawns[0x1] = 115.0f;
            junglePos[0x2] = new Vector3(3373.678f, 6223.346f, 60.0f); jungleRespawns[0x2] = 115.0f;
            junglePos[0x3] = new Vector3(6300.05f, 5300.06f, 60.0f); jungleRespawns[0x3] = 115.0f;
            junglePos[0x4] = new Vector3(7455.615f, 3890.203f, 60.0f); jungleRespawns[0x4] = 115.0f;
            junglePos[0x5] = new Vector3(7916.842f, 2533.963f, 60.0f); jungleRespawns[0x5] = 115.0f;
            junglePos[0x6] = new Vector3(9459.52f, 4193.03f, 60.0f); jungleRespawns[0x6] = 150.0f;
            junglePos[0x7] = new Vector3(10386.61f, 6811.112f, 60.0f); jungleRespawns[0x7] = 115.0f;
            junglePos[0x8] = new Vector3(10651.52f, 8116.424f, 60.0f); jungleRespawns[0x8] = 115.0f;
            junglePos[0x9] = new Vector3(7580.368f, 9250.405f, 60.0f); jungleRespawns[0x9] = 115.0f;
            junglePos[0xA] = new Vector3(6504.241f, 10584.56f, 60.0f); jungleRespawns[0xA] = 115;
            junglePos[0xB] = new Vector3(5810.464f, 11925.47f, 60.0f); jungleRespawns[0xB] = 115.0f;
            junglePos[0xC] = new Vector3(4600.495f, 10250.46f, 60.0f); jungleRespawns[0xC] = 1200.0f;
            junglePos[0xD] = new Vector3(1684.0f, 8207.0f, 60.0f); jungleRespawns[0xD] = 115.0f;
            junglePos[0xE] = new Vector3(12337.0f, 6263.0f, 60.0f); jungleRespawns[0xE] = 115.0f;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            new System.Threading.Thread(() =>
            {
                GetRegionInfo();
                GetSpecInfo();
            }).Start();

            (Config = new Menu("打野计时420", "JungleTimers420", true)).AddToMainMenu();
            Config.AddItem(new MenuItem("TextColor", "字体 颜色").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.AddItem(new MenuItem("OutlineColor", "轮廓线 颜色").SetValue(new Circle(true, Color.FromArgb(255, 0, 0, 0))));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat(
                "<font color='#70DBDB'>Dev - </font><font color='#FFFFFF'>鎵撻噹璁℃椂 420</font> <font color='#70DBDB'> 鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍锛丵Q缇361630847!</font>");

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }
        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0xC1 || args.PacketData[0] == 0xC2)
            {
                TimeOffset = BitConverter.ToSingle(args.PacketData, 5) - Game.Time;
                new System.Threading.Thread(() =>
                {
                    GetTimers();
                }).Start();
            }
            else if (args.PacketData[0] == Packet.S2C.EmptyJungleCamp.Header)
            {
                Byte Camp = args.PacketData[9];
                TimeSpan time = TimeSpan.FromSeconds(jungleRespawns[Camp] - Game.Time - TimeOffset);
                if (time.TotalSeconds < 0)
                {
                    if (Camp == 0xC)
                        jungleRespawns[Camp] = Game.Time + TimeOffset + 420;
                    else if (Camp == 0x6)
                        jungleRespawns[Camp] = Game.Time + TimeOffset + 360;
                    else if (Camp == 0x1 || Camp == 0x4 || Camp == 0x7 || Camp == 0xA)
                        jungleRespawns[Camp] = Game.Time + TimeOffset + 300;
                    else
                        jungleRespawns[Camp] = Game.Time + TimeOffset + 100;
                }
            }
        }
        static void GetRegionInfo()
        {
            Process proc = Process.GetProcesses().First(p => p.ProcessName.Contains("League of Legends"));
            String propFile = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(proc.Modules[0].FileName))))));
            propFile += @"\projects\lol_air_client\releases\";
            DirectoryInfo di = new DirectoryInfo(propFile).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();
            propFile = di.FullName + @"\deploy\lol.properties";
            propFile = File.ReadAllText(propFile);
            SpecUrl = new Regex("featuredGamesURL=(.+)featured").Match(propFile).Groups[1].Value;
            RegionTag = new Regex("regionTag=(.+)\r").Match(propFile).Groups[1].Value;
            SpectatorService.SpectatorDownloader.specHtml = SpecUrl;
        }
        static void GetSpecInfo()
        {
            String GameInfo = new WebClient().DownloadString(BaseUrl + RegionTag + UrlPartial + ObjectManager.Player.Name);
            GameInfo = GameInfo.Substring(GameInfo.IndexOf(SearchString) + SearchString.Length);
            GameInfo = GameInfo.Substring(GameInfo.IndexOf(" ") + 1);
            Key = GameInfo.Substring(0, GameInfo.IndexOf(" "));
            GameInfo = GameInfo.Substring(GameInfo.IndexOf(" ") + 1);
            GameId = GameInfo.Substring(0, GameInfo.IndexOf(" "));
            GameInfo = GameInfo.Substring(GameInfo.IndexOf(" ") + 1);
            PlatformId = GameInfo.Substring(0, GameInfo.IndexOf(" "));
        }
        static void GetTimers()
        {
            List<Packets.Packet> packets = new List<Packets.Packet>();
            List<Byte[]> fullGameBytes = SpectatorService.SpectatorDownloader.DownloadGameFiles(GameId, PlatformId, Key, "Chunk");
            foreach (Byte[] chunkBytes in fullGameBytes)
            {
                packets.AddRange(SpectatorService.SpectatorDecoder.DecodeBytes(chunkBytes));
            }
            foreach (Packets.Packet p in packets)
            {
                if (p.header == Packet.S2C.EmptyJungleCamp.Header && p.content[3] != 0)
                {
                    UInt32 camp = p.content[4];
                    if ((camp == 0x1 || camp == 0x4 || camp == 0x7 || camp == 0xA) &&
                        TimeSpan.FromSeconds(p.time + 300 - Game.Time - TimeOffset + 2).TotalSeconds > 0)
                        jungleRespawns[camp] = p.time + 300;
                    else if ((camp == 0x6) &&
                        TimeSpan.FromSeconds(p.time + 360 - Game.Time - TimeOffset + 2).TotalSeconds > 0)
                        jungleRespawns[camp] = p.time + 360;
                    else if ((camp == 0xC) &&
                        TimeSpan.FromSeconds(p.time + 420 - Game.Time - TimeOffset + 2).TotalSeconds > 0)
                        jungleRespawns[camp] = p.time + 420;
                }
            }
        }
        static void Drawing_OnDraw(EventArgs args)
        {
            foreach (KeyValuePair<UInt32, Single> creep in jungleRespawns)
            {
                Vector2 pos = Drawing.WorldToMinimap(junglePos[creep.Key]);
                TimeSpan time = TimeSpan.FromSeconds(creep.Value - Game.Time - TimeOffset + 2);
                if (time.TotalSeconds > 0)
                {
                    string display = string.Format("{0}:{1:D2}", time.Minutes, time.Seconds);

                    var OutlineColor = Config.Item("OutlineColor").GetValue<Circle>();
                    var TextColor = Config.Item("TextColor").GetValue<Circle>();

                    Drawing.DrawText(pos.X + 1 - display.Length * 3, pos.Y - 4, OutlineColor.Color, display);
                    Drawing.DrawText(pos.X - 1 - display.Length * 3, pos.Y - 6, OutlineColor.Color, display);
                    Drawing.DrawText(pos.X - 1 - display.Length * 3, pos.Y - 4, OutlineColor.Color, display);
                    Drawing.DrawText(pos.X + 1 - display.Length * 3, pos.Y - 6, OutlineColor.Color, display);
                    Drawing.DrawText(pos.X - display.Length * 3, pos.Y - 5, TextColor.Color, display);
                }
            }
        }
    }
}

