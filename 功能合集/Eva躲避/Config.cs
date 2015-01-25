// Copyright 2014 - 2014 Esk0r
// Config.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Evade
{
    internal static class Config
    {
        public const bool PrintSpellData = false;
        public const bool TestOnAllies = false;
        public const int SkillShotsExtraRadius = 9;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int ExtraEvadeDistance = 15;
        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;

        public const int CrossingTimeOffset = 250;

        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 0;

        public const int EvadingRouteChangeTimeOffset = 250;

        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu;

        public static void CreateMenu()
        {
            Menu = new Menu("花边汉化-Eva躲避", "Evade", true);

            //Create the evade spells submenus.
            var evadeSpells = new Menu("躲避 法术", "evadeSpells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                var subMenu = new Menu(spell.Name, spell.Name);

                subMenu.AddItem(
                    new MenuItem("DangerLevel" + spell.Name, "危险 等级").SetValue(
                        new Slider(spell.DangerLevel, 5, 1)));

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    subMenu.AddItem(new MenuItem("WardJump" + spell.Name, "摸 眼").SetValue(true));
                }

                subMenu.AddItem(new MenuItem("Enabled" + spell.Name, "开 启").SetValue(true));

                evadeSpells.AddSubMenu(subMenu);
            }
            Menu.AddSubMenu(evadeSpells);

            //Create the skillshots submenus.
            var skillShots = new Menu("技能 危险度", "Skillshots");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.Team != ObjectManager.Player.Team || Config.TestOnAllies)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (spell.ChampionName.ToLower() == hero.ChampionName.ToLower())
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.AddItem(
                                new MenuItem("DangerLevel" + spell.MenuItemName, "危险 等级").SetValue(
                                    new Slider(spell.DangerValue, 5, 1)));

                            subMenu.AddItem(
                                new MenuItem("IsDangerous" + spell.MenuItemName, "确认为 很危险").SetValue(
                                    spell.IsDangerous));

                            subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "显示 范围").SetValue(true));
                            subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "开 启").SetValue(true));

                            skillShots.AddSubMenu(subMenu);
                        }
                    }
                }
            }

            Menu.AddSubMenu(skillShots);

            var shielding = new Menu("友军 保护", "Shielding");

            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (ally.IsAlly && !ally.IsMe)
                {
                    shielding.AddItem(
                        new MenuItem("shield" + ally.ChampionName, "保护 " + ally.ChampionName).SetValue(true));
                }
            }
            Menu.AddSubMenu(shielding);

            var collision = new Menu("阻 挡", "Collision");
            collision.AddItem(new MenuItem("MinionCollision", "小兵 阻挡").SetValue(false));
            collision.AddItem(new MenuItem("HeroCollision", "英雄 阻挡").SetValue(false));
            collision.AddItem(new MenuItem("YasuoCollision", "亚索 风墙 阻挡").SetValue(true));
            collision.AddItem(new MenuItem("EnableCollision", "开 启").SetValue(true));
            //TODO add mode.
            Menu.AddSubMenu(collision);

            var drawings = new Menu("范 围", "Drawings");
            drawings.AddItem(new MenuItem("EnabledColor", "法术 范围显示").SetValue(Color.White));
            drawings.AddItem(new MenuItem("DisabledColor", "法术 释放显示").SetValue(Color.Red));
            drawings.AddItem(new MenuItem("MissileColor", "躲避 范围显示").SetValue(Color.LimeGreen));
            drawings.AddItem(new MenuItem("Border", "宽度 调整").SetValue(new Slider(1, 5, 1)));

            drawings.AddItem(new MenuItem("EnableDrawings", "开 启").SetValue(true));
            Menu.AddSubMenu(drawings);

            var misc = new Menu("杂 项", "Misc");
            misc.AddItem(new MenuItem("DisableFow", "禁用 战争迷雾").SetValue(false));
            misc.AddItem(new MenuItem("ShowEvadeStatus", "显示 躲避 状态").SetValue(false));
            Menu.AddSubMenu(misc);

            Menu.AddItem(
                new MenuItem("Enabled", "开启 躲避").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle, true)));

            Menu.AddItem(
                new MenuItem("OnlyDangerous", "仅躲避 危险技能").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddToMainMenu();
        }
    }
}
