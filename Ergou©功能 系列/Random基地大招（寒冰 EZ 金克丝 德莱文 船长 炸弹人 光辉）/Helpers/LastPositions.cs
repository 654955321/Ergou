using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using UnderratedAIO.Helpers;
using Collision = LeagueSharp.Common.Collision;

// using Beaving.s.Baseult;

namespace UnderratedAIO.Helpers
{
    internal class LastPositions
    {
        public static List<Positions> Enemies;
        private static Menu configMenu;
        public bool enabled = true;
        public static Spell R;
        private float LastUltTime;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public Vector3 SpawnPos;

        public static List<string> SupportedHeroes =
            new List<string>(new string[] { "Ezreal", "Jinx", "Ashe", "Draven", "Gangplank", "Ziggs", "Lux" });

        public static List<string> BaseUltHeroes = new List<string>(new string[] { "Ezreal", "Jinx", "Ashe", "Draven" });

        public LastPositions(Menu config)
        {
            configMenu = config;
            R = new Spell(SpellSlot.R);
            if (player.ChampionName == "Ezreal")
            {
                R.SetSkillshot(1000f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Jinx")
            {
                R.SetSkillshot(600f, 140f, 1700f, true, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Ashe")
            {
                R.SetSkillshot(250f, 130f, 1600f, true, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Draven")
            {
                R.SetSkillshot(400f, 160f, 2000f, true, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Lux")
            {
                R.SetSkillshot(500f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Ziggs")
            {
                R.SetSkillshot(1000f, 525f, 1750f, false, SkillshotType.SkillshotCircle);
            }
            if (player.ChampionName == "Gangplank")
            {
                R.SetSkillshot(100f, 600f, R.Speed, false, SkillshotType.SkillshotCircle);
            }
            SpawnPos = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;
            if (SupportedChamps())
            {
                config.AddItem(new MenuItem("UseR", "使用 R")).SetValue(true);
                if (player.ChampionName == "Gangplank")
                {
                    config.AddItem(new MenuItem("gpWaves", "船长自动大招 预测击中秒数")).SetValue(new Slider(2, 1, 7));
                }
                config.AddItem(new MenuItem("Hitchance", "命中率")).SetValue(new Slider(3, 1, 5));
                config.AddItem(new MenuItem("Info ", "--5 的命中几率最高"));
                Menu DontUlt = new Menu("禁用 基地大招", "DontUltRandomUlt");
                foreach (var e in HeroManager.Enemies)
                {
                    DontUlt.AddItem(new MenuItem(e.ChampionName + "DontUltRandomUlt", e.ChampionName + "禁用 基地大招")).SetValue(false);
                }
                config.AddSubMenu(DontUlt);
                config.AddItem(new MenuItem("Alliesrange", "可击杀目标存在盟友的范围："))
                    .SetValue(new Slider(1700, 500, 2000));
                config.AddItem(new MenuItem("InfoI ", "--启用基地大招击杀或者阻止（如寒冰大）"));
                config.AddItem(new MenuItem("waitBeforeUlt", "等待基地大招延迟(ms)"))
                    .SetValue(new Slider(600, 0, 3000));
                config.AddItem(new MenuItem("InfoII ", "--启用基地大招击敌方传送回程延迟"));
                config.AddItem(new MenuItem("BaseUltFirst", "基地大招 拥有更高的优先级")).SetValue(false);
                config.AddItem(new MenuItem("InfoIII ", "--如果大招存在击杀对象"));
            }
            config.AddItem(new MenuItem("RandomUltDrawings", "显示大招预测位置")).SetValue(false);
            config.AddItem(new MenuItem("ComboBlock", "按住连招键位时 基地大招不启用"))
                .SetValue(new KeyBind(32, KeyBindType.Press));
            Enemies = HeroManager.Enemies.Select(x => new Positions(x)).ToList();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
        }

        private void Obj_AI_Base_OnTeleport(Obj_AI_Base sender, GameObjectTeleportEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || unit.IsAlly)
            {
                return;
            }

            var recall = Packet.S2C.Teleport.Decoded(unit, args);
            Enemies.Find(x => x.Player.NetworkId == recall.UnitNetworkId).RecallData.Update(recall);
        }

        private bool SupportedChamps()
        {
            return SupportedHeroes.Any(h => h.Contains(player.ChampionName));
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!configMenu.Item("RandomUltDrawings").GetValue<bool>() || !enabled ||
                configMenu.Item("ComboBlock").GetValue<KeyBind>().Active)
            {
                return;
            }
            foreach (var enemy in
                Enemies.Where(
                    x =>
                        x.Player.IsValid<Obj_AI_Hero>() && !x.Player.IsDead &&
                        x.RecallData.Recall.Status == Packet.S2C.Teleport.Status.Start &&
                        x.RecallData.Recall.Type == Packet.S2C.Teleport.Type.Recall)
                    .OrderBy(x => x.RecallData.GetRecallTime()))
            {
                var trueDist = Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 *
                               enemy.Player.MoveSpeed;
                var dist = (Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 * enemy.Player.MoveSpeed) -
                           enemy.Player.MoveSpeed / 3;
                if (dist > 1500)
                {
                    return;
                }

                if (dist < 50)
                {
                    dist = 50;
                }
                var line = getpos(enemy, dist);
                dist = enemy.Player.Distance(line);
                Vector3 pos = line;
                if (enemy.Player.IsVisible)
                {
                    pos = enemy.Player.Position;
                }
                else
                {
                    pos =
                        PointsAroundTheTarget(enemy.Player.Position, trueDist)
                            .Where(
                                p => !p.IsWall() && line.Distance(p) < dist / 1.2 && GetPath(enemy.Player, p) < trueDist)
                            .OrderByDescending(p => NavMesh.IsWallOfGrass(p, 10))
                            .ThenBy(p => line.Distance(p))
                            .FirstOrDefault();
                }
                if (pos != null)
                {
                    Drawing.DrawCircle(pos, 50, Color.Red);
                }
                if (!enemy.Player.IsVisible)
                {
                    Drawing.DrawCircle(line, dist / 1.2f, Color.LawnGreen);
                }
            }
        }

        private Vector3 getpos(Positions enemy, float dist)
        {
            var line = enemy.Player.Position.Extend(enemy.predictedpos, dist);
            if (enemy.Player.Position.Distance(enemy.predictedpos) < dist &&
                ((enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000) < 1)
            {
                line = enemy.predictedpos;
            }
            return line;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            float time = System.Environment.TickCount;
            foreach (Positions enemyInfo in
                Enemies.Where(x => x.Player.IsVisible && !x.Player.IsDead && x.Player.IsValidTarget()))
            {
                enemyInfo.LastSeen = time;
                var prediction = Prediction.GetPrediction(enemyInfo.Player, 4);
                if (prediction != null)
                {
                    enemyInfo.predictedpos = prediction.UnitPosition;
                }
            }
            if (!SupportedChamps() || !configMenu.Item("UseR").GetValue<bool>() || !R.IsReady() || !enabled ||
                configMenu.Item("ComboBlock").GetValue<KeyBind>().Active)
            {
                return;
            }
            var HitChance = configMenu.Item("Hitchance").GetValue<Slider>().Value;
            foreach (Positions enemy in
                Enemies.Where(
                    x =>
                        x.Player.IsValid<Obj_AI_Hero>() && !x.Player.IsDead &&
                        !configMenu.Item(x.Player.ChampionName + "DontUltRandomUlt").GetValue<bool>() &&
                        x.RecallData.Recall.Status == Packet.S2C.Teleport.Status.Start &&
                        x.RecallData.Recall.Type == Packet.S2C.Teleport.Type.Recall)
                    .OrderBy(x => x.RecallData.GetRecallTime()))
            {
                if (!checkdmg(enemy.Player) || (checkdmg(enemy.Player) && CheckBuffs(enemy.Player)) ||
                    CheckBaseUlt(enemy.RecallData.GetRecallCountdown()) ||
                    !(Environment.TickCount - enemy.RecallData.RecallStartTime >
                      configMenu.Item("waitBeforeUlt").GetValue<Slider>().Value))
                {
                    continue;
                }
                var dist = (Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 * enemy.Player.MoveSpeed) -
                           enemy.Player.MoveSpeed / 3;
                var trueDist = Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 *
                               enemy.Player.MoveSpeed;
                var line = getpos(enemy, dist);
                Console.WriteLine(enemy.Player.SkinName+ trueDist);
                switch (HitChance)
                {
                    case 1:
                        break;
                    case 2:
                        if (trueDist > 1000 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                    case 3:
                        if (trueDist > 800 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                    case 4:
                        if (trueDist > 650 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                    case 5:
                        if (trueDist > 400 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                }
                Vector3 pos = line;
                if (enemy.Player.IsVisible)
                {
                    pos = enemy.Player.Position;
                }
                else
                {
                    if (dist > 1500)
                    {
                        return;
                    }
                    pos =
                        PointsAroundTheTarget(enemy.Player.Position, trueDist)
                            .Where(
                                p => !p.IsWall() && line.Distance(p) < dist / 1.2 && GetPath(enemy.Player, p) < trueDist)
                            .OrderByDescending(p => NavMesh.IsWallOfGrass(p, 10))
                            .ThenBy(p => line.Distance(p))
                            .FirstOrDefault();
                }
                if (pos != null)
                {
                    if (player.ChampionName == "Ziggs" && player.Distance(pos) > 5000f)
                    {
                        continue;
                    }
                    if (player.ChampionName == "Lux" && player.Distance(pos) > 3000f)
                    {
                        continue;
                    }
                    kill(enemy, new Vector3(pos.X, pos.Y, 0));
                }
            }
        }

        private bool CheckBaseUlt(float recallCooldown)
        {
            if (configMenu.Item("BaseUltFirst").GetValue<bool>() &&
                BaseUltHeroes.Any(h => h.Contains(player.ChampionName)) && recallCooldown > UltTime(SpawnPos))
            {
                return true;
            }
            return false;
        }

        private bool CheckBuffs(Obj_AI_Hero enemy)
        {
            if (enemy.ChampionName == "Anivia")
            {
                if (enemy.HasBuff("rebirthcooldown"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            if (enemy.ChampionName == "Aatrox")
            {
                if (enemy.HasBuff("aatroxpassiveready"))
                {
                    return true;
                }
            }
            return false;
        }

        public static float GetPath(Obj_AI_Hero hero, Vector3 b)
        {
            var path = hero.GetPath(b);
            var lastPoint = path[0];
            var distance = 0f;
            foreach (var point in path.Where(point => !point.Equals(lastPoint)))
            {
                distance += lastPoint.Distance(point);
                lastPoint = point;
            }
            return distance;
        }

        private bool CheckShieldTower(Vector3 pos)
        {
            if (Game.MapId != GameMapId.SummonersRift)
            {
                return false;
            }
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(t => t.Distance(pos) < 1100f && t.IsEnemy && !t.IsDead && t.InventoryItems.Count() > 3);
        }

        private void kill(Positions positions, Vector3 pos)
        {
            if (R.IsReady() && pos.Distance(positions.Player.Position) < 1200 &&
                pos.CountAlliesInRange(configMenu.Item("Alliesrange").GetValue<Slider>().Value) < 1)
            {
                if (checkdmg(positions.Player) && UltTime(pos) < positions.RecallData.GetRecallTime() &&
                    !isColliding(pos))
                {
                    R.Cast(pos);
                }
            }
        }

        private bool isColliding(Vector3 pos)
        {
            if (player.ChampionName == "Draven" && player.ChampionName == "Ashe" && player.ChampionName == "Jinx")
            {
                var input = new PredictionInput { Radius = R.Width, Unit = player, };

                input.CollisionObjects[0] = CollisionableObjects.Heroes;

                return Collision.GetCollision(new List<Vector3> { pos }, input).Any();
            }
            return false;
        }

        private float UltTime(Vector3 pos)
        {
            var dist = player.ServerPosition.Distance(pos);
            if (player.ChampionName == "Ezreal")
            {
                return (dist / 2000) * 1000 + 1000;
            }
            //Beaving's calculations
            if (player.ChampionName == "Jinx" && dist > 1350)
            {
                const float accelerationrate = 0.3f;

                var acceldifference = dist - 1350f;

                if (acceldifference > 150f)
                {
                    acceldifference = 150f;
                }

                var difference = dist - 1500f;
                return (dist /
                        ((1350f * 1700f + acceldifference * (1700f + accelerationrate * acceldifference) +
                          difference * 2200f) / dist)) * 1000 + 250;
            }
            if (player.ChampionName == "Ashe")
            {
                return (dist / 1600) * 1000 + 250;
            }
            if (player.ChampionName == "Draven")
            {
                return (dist / 2000) * 1000 + 400;
            }
            if (player.ChampionName == "Ziggs")
            {
                return (dist / 1750f) * 1000 + 1000;
            }
            if (player.ChampionName == "Lux")
            {
                return 500f;
            }
            return 0;
        }

        public static List<Vector3> PointsAroundTheTarget(Vector3 pos, float dist, float prec = 15, float prec2 = 6)
        {
            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            if (dist > 205)
            {
                prec = 30;
                prec2 = 8;
            }
            if (dist > 805)
            {
                dist = (float) (dist * 1.5);
                prec = 45;
                prec2 = 10;
            }
            var angle = 360 / prec * Math.PI / 180.0f;
            var step = dist * 2 / prec2;
            for (int i = 0; i < prec; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    list.Add(
                        new Vector3(
                            pos.X + (float) (Math.Cos(angle * i) * (j * step)),
                            pos.Y + (float) (Math.Sin(angle * i) * (j * step)) - 90, pos.Z));
                }
            }

            return list;
        }

        private bool checkdmg(Obj_AI_Hero target)
        {
            var dmg = R.GetDamage(target);
            var bonuShieldNearTowers = 0f;
            if (CheckShieldTower(target.Position))
            {
                bonuShieldNearTowers = -300f;
            }
            if (player.ChampionName == "Ezreal" || player.ChampionName == "Draven")
            {
                if (dmg * 0.7 - 50 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Jinx")
            {
                if (R.GetDamage(target, 1) - 50 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Gangplank")
            {
                if (configMenu.Item("gpWaves").GetValue<Slider>().Value * dmg - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Ashe" || player.ChampionName == "Lux" || player.ChampionName == "Ziggs")
            {
                if (dmg - 50 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            return false;
        }
    }

    internal class Positions
    {
        public Obj_AI_Hero Player;
        public float LastSeen;
        public Vector3 predictedpos;

        public RecallData RecallData;

        public Positions(Obj_AI_Hero player)
        {
            Player = player;
            RecallData = new RecallData(this);
        }
    }

    internal class RecallData
    {
        public Positions Positions;
        public Packet.S2C.Teleport.Struct Recall;
        public Packet.S2C.Teleport.Struct Aborted;
        public float AbortTime;
        public float RecallStartTime;
        public bool started;
        public int FADEOUT_TIME = 3000;

        public RecallData(Positions positions)
        {
            Positions = positions;
            Recall = new Packet.S2C.Teleport.Struct(
                Positions.Player.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0);
        }

        public float GetRecallTime()
        {
            float time = System.Environment.TickCount;
            float countdown = 0;

            if (time - AbortTime < 2000)
            {
                countdown = Aborted.Duration - (AbortTime - Aborted.Start);
            }
            else if (AbortTime > 0)
            {
                countdown = 0;
            }
            else
            {
                countdown = Recall.Start + Recall.Duration - time;
            }

            return countdown < 0 ? 0 : countdown;
        }

        public float GetRecallCountdown()
        {
            float time = Environment.TickCount;
            float countdown = 0;

            if (time - AbortTime < FADEOUT_TIME)
            {
                countdown = Aborted.Duration - (AbortTime - Aborted.Start);
            }
            else if (AbortTime > 0)
            {
                countdown = 0;
            }
            else
            {
                countdown = Recall.Start + Recall.Duration - time;
            }

            return countdown < 0 ? 0 : countdown;
        }

        public Positions Update(Packet.S2C.Teleport.Struct newData)
        {
            if (newData.Type == Packet.S2C.Teleport.Type.Recall && newData.Status == Packet.S2C.Teleport.Status.Abort)
            {
                Aborted = Recall;
                AbortTime = System.Environment.TickCount;
                started = false;
            }
            else
            {
                AbortTime = 0;
            }
            if (newData.Type == Packet.S2C.Teleport.Type.Recall && newData.Status == Packet.S2C.Teleport.Status.Finish)
            {
                started = false;
            }
            if (newData.Type == Packet.S2C.Teleport.Type.Recall && newData.Status == Packet.S2C.Teleport.Status.Start)
            {
                if (!started)
                {
                    RecallStartTime = System.Environment.TickCount;
                }
                started = true;
            }
            Recall = newData;
            return Positions;
        }
    }
}