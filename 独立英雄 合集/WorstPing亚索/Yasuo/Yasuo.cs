using System.Collections.Generic;
using LeagueSharp;
using Yasuo.Evade;

namespace Yasuo
{
    public class Yasuo
    {
        /// <summary>
        ///     Player Instance
        /// </summary>
        public static Obj_AI_Hero Player;

        /// <summary>
        ///     Menu Instance
        /// </summary>
        public static YasuoMenu Menu;

        /// <summary>
        ///     Game Instance
        /// </summary>
        public static YasuoGame Game;

        public static List<Skillshot> DetectedSkillShots = new List<Skillshot>();
        public static List<Skillshot> EvadeDetectedSkillshots = new List<Skillshot>();
        public static List<MenuData> MenuWallsList = new List<MenuData>();
        public static List<MenuData> MenuDashesList = new List<MenuData>();

        public struct MenuData
        {
            public string ChampionName;
            public string DisplayName;
            public bool IsWindwall;
            public string Slot;
            public string SpellDisplayName;
            public string SpellName;

            public void AddToMenu()
            {
                if (
                    Menu.GetItem(
                        ((IsWindwall) ? YasuoMenu.AutoWindWallLoc : YasuoMenu.EvadeLoc) + "." + ChampionName + "." +
                        Slot) == null)
                {
                    Menu.AddItem(
                        (IsWindwall) ? YasuoMenu.AutoWindMenu : YasuoMenu.EvadeMenu, SpellDisplayName,
                        ((IsWindwall) ? YasuoMenu.AutoWindWallLoc : YasuoMenu.EvadeLoc) + "." + ChampionName + "." +
                        Slot).SetValue(true);
                }
            }
        }
    }
}