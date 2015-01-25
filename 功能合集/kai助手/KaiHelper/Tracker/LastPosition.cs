using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using KaiHelper.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;

namespace KaiHelper.Tracker
{
    internal class LastPosition
    {
        private readonly List<ChampionTracker> championsTracker = new List<ChampionTracker>();
        private readonly Obj_SpawnPoint enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
        public static Menu Menu;

        public LastPosition(Menu timer)
        {
            Menu = timer.AddSubMenu(new Menu("移动 跟踪(显示移动轨迹)", "Last Position"));
            Menu.AddItem(new MenuItem("Scale", "图像 比例").SetValue(new Slider(20, 1, 50)));
            Menu.AddItem(new MenuItem("Opacity", "不透 明度").SetValue(new Slider(70)));
            Menu.AddItem(new MenuItem("TextSize", "字体 大小").SetValue(new Slider(15, 1)));
            Menu.AddItem(new MenuItem("ALP", "开 启!").SetValue(true));
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Obj_AI_Base.OnTeleport += ObjAiBaseOnOnTeleport;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("LastPos Loaded!");
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            foreach (
                Obj_AI_Hero champion in
                    ObjectManager.Get<Obj_AI_Hero>().Where(champion => champion.Team != ObjectManager.Player.Team))
            {
                //Console.WriteLine(champion.ChampionName);
                championsTracker.Add(new ChampionTracker(champion));
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("ALP").GetValue<bool>())
            {
                return;
            }
            foreach (ChampionTracker champion in championsTracker)
            {
                if (champion.Champion.ServerPosition != champion.RecallPostion)
                {
                    champion.LastPotion = champion.Champion.ServerPosition;
                }
                if (champion.Champion.IsVisible)
                {
                    champion.StartInvisibleTime = Game.ClockTime;
                }
            }
        }

        private void ObjAiBaseOnOnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            try
            {
                //if (!Menu.Item("ALP").GetValue<bool>())
                //{
                //    return;
                //}
            
                var unit = sender as Obj_AI_Hero;
                if (unit == null || !unit.IsValid || unit.IsAlly)
                {
                    return;
                }
                Packet.S2C.Teleport.Struct recall = Packet.S2C.Teleport.Decoded(sender, args);
                if (recall.Type == Packet.S2C.Teleport.Type.Recall)
                {
                    ChampionTracker cham = championsTracker.FirstOrDefault(
                        c => c.Champion.NetworkId == recall.UnitNetworkId);
                    if (cham != null)
                    {
                        cham.RecallPostion = cham.Champion.ServerPosition;
                        cham.Text.Color = Color.Red;
                        if (recall.Status == Packet.S2C.Teleport.Status.Finish)
                        {
                            cham.LastPotion = enemySpawn.Position;
                            cham.Text.Color = Color.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal class ChampionTracker
        {
            public readonly Render.Text Text;

            public Vector3 LastPotion;
            public Vector3 RecallPostion;
            public float StartInvisibleTime;

            public ChampionTracker(Obj_AI_Hero champion)
            {
                Champion = champion;
                LastPotion = champion.ServerPosition;
                StartInvisibleTime = Game.ClockTime;
                var sprite =
                    new Render.Sprite(
                        Helper.ChangeOpacity(
                            Helper.CropCircleImage(
                                (Resources.ResourceManager.GetObject(Champion.ChampionName + "_Square_0") ??
                                 Resources.Katarina_Square_0) as Bitmap), Opacity), new Vector2(0, 0));
                sprite.GrayScale();
                sprite.Scale = new Vector2(Scale, Scale);
                sprite.VisibleCondition = sender => TrackerCondition;
                sprite.PositionUpdate =
                    () => Drawing.WorldToMinimap(LastPotion) + new Vector2(-(sprite.Width / 2), -(sprite.Height / 2));
                sprite.Add(0);
                Text = new Render.Text(0, 0, "", Menu.Item("TextSize").GetValue<Slider>().Value, Color.White)
                {
                    VisibleCondition = sender => TrackerCondition,
                    PositionUpdate = () => Drawing.WorldToMinimap(LastPotion),
                    TextUpdate = () => Helper.FormatTime(Game.ClockTime - StartInvisibleTime),
                    OutLined = true,
                    Centered = true
                };
                Text.Add(0);
            }

            public Obj_AI_Hero Champion { get; private set; }

            private bool TrackerCondition
            {
                get { return !Champion.IsVisible && !Champion.IsDead && Menu.Item("ALP").GetValue<bool>(); }
            }

            public float Opacity
            {
                get { return (float)Menu.Item("Opacity").GetValue<Slider>().Value / 100; }
            }

            private float Scale
            {
                get { return (float)Menu.Item("Scale").GetValue<Slider>().Value / 100; }
            }
        }
    }
}