using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Syndra
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            Program.Config.AddSubMenu(new Menu("刺客 选项", "MenuAssassin"));
            Program.Config.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinActive", "启动").SetValue(true));
            Program.Config.SubMenu("MenuAssassin").AddItem(new MenuItem("Ax", ""));
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(
                    new MenuItem("AssassinSelectOption", "设置: ").SetValue(
                        new StringList(new[] {"单一 选择", "多 选择"})));
            Program.Config.SubMenu("MenuAssassin").AddItem(new MenuItem("Ax", ""));
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSetClick", "点击 添加/删除").SetValue(true));
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(
                    new MenuItem("AssassinReset", "重置 列表").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Press)));

            Program.Config.SubMenu("MenuAssassin").AddSubMenu(new Menu("范围:", "Draw"));

            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawSearch", "搜索 范围").SetValue(new Circle(true, Color.GreenYellow)));
            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawActive", "活跃的敌人").SetValue(new Circle(true, Color.GreenYellow)));
            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawNearest", "最近的敌人").SetValue(new Circle(true, Color.DarkSeaGreen)));
            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawStatus", "显示 状态").SetValue(true));


            Program.Config.SubMenu("MenuAssassin").AddSubMenu(new Menu("刺客 列表:", "AssassinMode"));
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Program.Config.SubMenu("MenuAssassin")
                    .SubMenu("AssassinMode")
                    .AddItem(
                        new MenuItem("Assassin" + enemy.ChampionName, enemy.ChampionName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSearchRange", "搜索 范围")).SetValue(new Slider(1000, 2000));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Program.Config.Item("Assassin" + enemy.ChampionName).SetValue(false);
            }
        }
        private static void OnGameUpdate(EventArgs args)
        {
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {

            if (Program.Config.Item("AssassinReset").GetValue<KeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Game.PrintChat(
                    "<font color='#FFFFFF'>Reset Assassin List is Complete! Click on the enemy for Add/Remove.</font>");
            }

            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            if (Program.Config.Item("AssassinSetClick").GetValue<bool>())
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                                          where hero.IsValidTarget()
                                          select hero
                                              into h
                                              orderby h.Distance(Game.CursorPos) descending
                                              select h
                                                  into enemy
                                                  where enemy.Distance(Game.CursorPos) < 150f
                                                  select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        var xSelect =
                            Program.Config.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex;

                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Program.Config.Item("Assassin" + objAiHero.ChampionName).SetValue(true);
                                Game.PrintChat(
                                    string.Format(
                                        "<font color='FFFFFF'>Added to Assassin List</font> <font color='#09F000'>{0} ({1})</font>",
                                        objAiHero.Name, objAiHero.ChampionName));
                                break;
                            case 1:
                                var menuStatus =
                                    Program.Config.Item("Assassin" + objAiHero.ChampionName)
                                        .GetValue<bool>();
                                Program.Config.Item("Assassin" + objAiHero.ChampionName)
                                    .SetValue(!menuStatus);
                                Game.PrintChat(
                                    string.Format("<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                        !menuStatus ? "#FFFFFF" : "#FF8877",
                                        !menuStatus ? "Added to Assassin List:" : "Removed from Assassin List:",
                                        objAiHero.Name, objAiHero.ChampionName));
                                break;
                        }
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program.Config.Item("AssassinActive").GetValue<bool>())
                return;

            if (Program.Config.Item("DrawStatus").GetValue<bool>())
            {
                var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(xEnemy => xEnemy.IsEnemy);
                var objAiHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();
                Drawing.DrawText(Drawing.Width*0.89f, Drawing.Height*0.58f, Color.GreenYellow, "Assassin Status");
                Drawing.DrawText(Drawing.Width*0.89f, Drawing.Height*0.58f, Color.GhostWhite, "_____________");
                for (int i = 0; i < objAiHeroes.Count(); i++)
                {
                    var xCaption = objAiHeroes[i].ChampionName;
                    var xWidth = Drawing.Width*0.90f;
                    if (Program.Config.Item("Assassin" + objAiHeroes[i].ChampionName).GetValue<bool>())
                    {
                        xCaption = "+ " + xCaption;
                        xWidth = Drawing.Width*0.8910f;

                    }
                    Drawing.DrawText(xWidth, Drawing.Height*0.58f + (float) (i + 1)*15, Color.Gainsboro,
                        xCaption);
                }
            }

            var drawSearch = Program.Config.Item("DrawSearch").GetValue<Circle>();
            var drawActive = Program.Config.Item("DrawActive").GetValue<Circle>();
            var drawNearest = Program.Config.Item("DrawNearest").GetValue<Circle>();

            var drawSearchRange = Program.Config.Item("AssassinSearchRange").GetValue<Slider>().Value;
            if (drawSearch.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, drawSearchRange, drawSearch.Color);
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                        .Where(
                            enemy =>
                                enemy.IsVisible &&
                                Program.Config.Item("Assassin" + enemy.ChampionName) != null &&
                                !enemy.IsDead)
                        .Where(
                            enemy => Program.Config.Item("Assassin" + enemy.ChampionName).GetValue<bool>()))
            {
                if (ObjectManager.Player.Distance(enemy) < drawSearchRange)
                {
                    if (drawActive.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawActive.Color);
                }
                else if (ObjectManager.Player.Distance(enemy) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy) < drawSearchRange + 400)
                {
                    if (drawNearest.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawNearest.Color);
                }
            }
        }
    }
}
