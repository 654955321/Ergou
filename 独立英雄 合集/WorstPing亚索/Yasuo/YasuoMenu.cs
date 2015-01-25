using LeagueSharp.Common;

namespace Yasuo
{
    public class YasuoMenu
    {
        /// <summary>
        ///     Yasuo Menu Constructor
        /// </summary>
        public YasuoMenu()
        {
            // => Menu Initialize
            _menu = new Menu(RootDisplayName, RootName, true);

            // => Orbwalker Initialize
            _orbwalker = new Orbwalking.Orbwalker(AddSubMenu(OrbwalkerDisplayName, OrbwalkerName));

            // => TargetSelector Initialize
            TargetSelector.AddToMenu(AddSubMenu(TargetSelectorDisplayName, TargetSelectorName));

            #region Combo Initialize

            var combo = AddSubMenu(ComboDisplayName, ComboName); // => Combo Menu
            AddItem(combo, ComboQDisplayName, ComboQName).SetValue(true); // => Use Q
            AddItem(combo, Combo3QDisplayName, Combo3QName).SetValue(true); // => Use 3rd Q
            AddItem(combo, ComboEDisplayName, ComboEName).SetValue(true); // => Use E
            AddItem(combo, ComboRDisplayName, ComboRName).SetValue(true); // => Use R
            AddSpacer(combo); // => Spacer
            AddItem(combo, ComboRModeDisplayName, ComboRModeName)
                .SetValue(new StringList(new[] { "多个目标", "单一目标", "两者" }, 1)); // => R Mode
            AddItem(combo, ComboRPercentDisplayName, ComboRPercentName).SetValue(new Slider(40)); // => Min. Enemies Health %
            AddItem(combo, ComboRPercent2DisplayName, ComboRPercent2Name).SetValue(new Slider(40)); // => Min. Enemies Health %
            AddItem(combo, ComboRSelfDisplayName, ComboRSelfName).SetValue(false); // => Self Knockedup Enemies
            AddItem(combo, ComboRMinDisplayName, ComboRMinName).SetValue(new Slider(3, 1, 5)); // => Min. Enemies
            AddItem(combo, ComboRAirTimeDisplayName, ComboRAirTimeName).SetValue(new Slider(500, 250, 1000)); // => Min. Airtime
            AddSpacer(combo); // => Spacer
            AddItem(combo, ComboGapcloserModeDisplayName, ComboGapcloserModeName) // => Gapcloser Mode
                .SetValue(new StringList(new[] { "跟随 鼠标", "跟随 敌人" })); // => Gapcloser Mode
            AddItem(combo, ComboGapcloserFModeDisplayName, ComboGapcloserFModeName).SetValue(false); // => Gapcloser Follow even in attack range
            AddItem(combo, ComboERangeDisplayName, ComboERangeName).SetValue(new Slider(475, 0, 475));
            var items = AddSubMenu(combo, ComboItemsDisplayName, ComboItemsName); // => Items Menu
            AddItem(items, ComboItemsTiamatDisplayName, ComboItemsTiamatName).SetValue(true); // => Tiamat
            AddItem(items, ComboItemsHydraDisplayName, ComboItemsHydraName).SetValue(true); // => Ravenous Hydra
            AddItem(items, ComboItemsBilgewaterDisplayName, ComboItemsBilgewaterName).SetValue(true); // => Bilgewater Cutlass
            AddItem(items, ComboItemsBotRkDisplayName, ComboItemsBotRkName).SetValue(true); // => BladeoftheRuinedKing


            #endregion

            #region Flee Initialize

            var flee = AddSubMenu(FleeDisplayName, FleeName);
            AddItem(flee, FleeEnableDisplayName, FleeEnableName).SetValue(true);
            AddItem(flee, FleeKeyDisplayName, FleeKeyName).SetValue(new KeyBind('Z', KeyBindType.Press));
            AddItem(flee, FleeFleeIntoTowersDisplayName, FleeFleeIntoTowersName).SetValue(true);

            #endregion

            #region Farming Initialize

            var farming = AddSubMenu(FarmingDisplayName, FarmingName);
            AddItem(farming, FarmingLastHitQDisplayName, FarmingLastHitQName).SetValue(true);
            AddItem(farming, FarmingLastHit3QDisplayName, FarmingLastHit3QName).SetValue(true);
            AddItem(farming, FarmingLastHitEDisplayName, FarmingLastHitEName).SetValue(true);
            AddItem(farming, FarmingLastHitQaaDisplayName, FarmingLastHitQaaName).SetValue(true);
            AddItem(farming, FarmingLastHitEaaDisplayName, FarmingLastHitEaaName).SetValue(true);
            AddItem(farming, FarmingLastHitTurretDisplayName, FarmingLastHitTurretName).SetValue(false);
            AddSpacer(farming);
            AddItem(farming, FarmingLaneClearQDisplayName, FarmingLaneClearQName).SetValue(true);
            AddItem(farming, FarmingLaneClear3QDisplayName, FarmingLaneClear3QName).SetValue(true);
            AddItem(farming, FarmingLaneClearEDisplayName, FarmingLaneClearEName).SetValue(true);
            AddItem(farming, FarmingLaneClearQaaDisplayName, FarmingLaneClearQaaName).SetValue(true);
            AddItem(farming, FarmingLaneClearEaaDisplayName, FarmingLaneClearEaaName).SetValue(true);
            AddItem(farming, FarmingLaneClearTurretDisplayName, FarmingLaneClearTurretName).SetValue(false);

            #endregion

            #region Killsteal Initialize

            //
            var ks = AddSubMenu(KillstealDisplayName, KillstealName);
            AddItem(ks, KillstealEnabledDisplayName, KillstealEnabledName).SetValue(true);
            AddSpacer(ks);
            AddItem(ks, KillstealQDisplayName, KillstealQName).SetValue(true);
            AddItem(ks, Killsteal3QDisplayName, Killsteal3QName).SetValue(true);
            AddItem(ks, KillstealEDisplayName, KillstealEName).SetValue(true);
            AddItem(ks, KillstealEIntoTowerDisplayName, KillstealEIntoTowerName).SetValue(true);

            #endregion

            // Auto Windwall
            var aww = AutoWindMenu = AddSubMenu(AutoWindWall, AutoWindWallLoc); // => Auto Windwall
            AddItem(aww, AutoWindWallUse, AutoWindWallUseLoc).SetValue(true); // => Use
            AddItem(aww, AutoWindWallDelay, AutoWindWallDelayLoc).SetValue(new Slider(500, 150, 2000)); // => Windwall Delay
            AddSpacer(aww); // => SPACER

            // Evade
            var evade = EvadeMenu = AddSubMenu(Evade, EvadeLoc); // => Evade
            AddItem(evade, EvadeUse, EvadeUseLoc).SetValue(true); // => Use
            AddSpacer(evade); // => SPACER

            #region Misc Initialize

            var misc = AddSubMenu(MiscDisplayName, MiscName); // => Misc Menu
            AddItem(misc, MiscPacketsDisplayName, MiscPacketsName).SetValue(true); // => Packets

            #endregion

            // => Menu Footer
            AddSpacer(_menu); // => Spacer
            AddItem(_menu, RootDisplayName, RootDescriptionName); // => Description

            // => Menu Finalize
            _menu.AddToMainMenu();
        }

        /// <summary>
        ///     Add a sub menu towards the main menu.
        /// </summary>
        /// <param name="displayName">Sub Menu Display Name</param>
        /// <param name="localisationName">Sub Menu Directory</param>
        /// <returns></returns>
        public Menu AddSubMenu(string displayName, string localisationName)
        {
            return _menu.AddSubMenu(new Menu(displayName, localisationName));
        }

        /// <summary>
        ///     Add a sub menu towards a selected menu.
        /// </summary>
        /// <param name="menu">Parent Menu</param>
        /// <param name="displayName">Sub Menu Display Name</param>
        /// <param name="localisationName">Sub Menu Directory</param>
        /// <returns></returns>
        public Menu AddSubMenu(Menu menu, string displayName, string localisationName)
        {
            return menu.AddSubMenu(new Menu(displayName, RootName + localisationName));
        }

        /// <summary>
        ///     Add an item towards a selected menu.
        /// </summary>
        /// <param name="menu">Parent Menu</param>
        /// <param name="displayName">Item Display Name</param>
        /// <param name="localisationName">Item Directory</param>
        /// <returns></returns>
        public MenuItem AddItem(Menu menu, string displayName, string localisationName)
        {
            return menu.AddItem(new MenuItem(RootName + localisationName, displayName));
        }

        /// <summary>
        ///     Add a spacer towards a selected menu.
        /// </summary>
        /// <param name="menu">Parent Menu</param>
        /// <param name="localisationName">Spacer Directory</param>
        public void AddSpacer(Menu menu, string localisationName = "")
        {
            _spacer++;
            menu.AddItem(new MenuItem(RootName + localisationName + ".spacer_" + _spacer, ""));
        }

        /// <summary>
        ///     Orbwalker instance
        /// </summary>
        /// <returns>Returns the orbwalker instance</returns>
        public Orbwalking.Orbwalker GetOrbwalker()
        {
            return _orbwalker;
        }

        /// <summary>
        ///     Menu instance
        /// </summary>
        /// <returns>Returns the menu instance</returns>
        public Menu GetMenu()
        {
            return _menu;
        }

        /// <summary>
        ///     Retrives a value from a MenuItem
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="localisationName">Item Directory</param>
        /// <returns>MenuItem value</returns>
        public T GetValue<T>(string localisationName)
        {
            return _menu.Item(RootName + localisationName).GetValue<T>();
        }

        /// <summary>
        ///     Quick reference to fetch the item
        /// </summary>
        /// <param name="str">Item Location</param>
        /// <returns>Item</returns>
        public MenuItem GetItem(string str)
        {
            return _menu.Item(RootName + str);
        }

        #region Fields

        private readonly Menu _menu; // => The Menu
        private readonly Orbwalking.Orbwalker _orbwalker; // => The Orbwalker

        private int _spacer = -1; // => Spacer Counter

        private const string RootDisplayName = "花边汉化-WorstPing亚索"; // => Menu Display Name
        private const string RootName = "l33t.yasuo"; // => Menu Name
        private const string RootDescriptionName = ".desc";

        private const string OrbwalkerDisplayName = "走 砍"; // => Orbwalker Display Name
        private const string OrbwalkerName = ".orbwalker"; // => Orbwalker Name

        private const string TargetSelectorDisplayName = "目标 选择"; // => TargetSelector Display Name
        private const string TargetSelectorName = ".targetselector"; // => TargetSelector Name

        #region Combo

        private const string ComboDisplayName = "连招 设置"; // => Combo Display Name
        private const string ComboName = ".combo"; // => Combo Name

        private const string ComboQDisplayName = "使用 斩钢闪 (Q)"; // => Combo Q Display Name
        public const string ComboQName = ComboName + ".useq"; // => Combo Q Name
        private const string Combo3QDisplayName = "使用 斩钢闪 (Q3)"; // => Combo Q Display Name
        public const string Combo3QName = ComboName + ".use3q"; // => Combo Q Name
        private const string ComboEDisplayName = "使用 踏前斩 (E)"; // => Combo E Display Name
        public const string ComboEName = ComboName + ".usee"; // => Combo E Name
        private const string ComboRDisplayName = "使用 狂风绝息斩 (R)"; // => Combo R Display Name
        public const string ComboRName = ComboName + ".user"; // => Combo R Name

        private const string ComboRModeDisplayName = "大招 模式"; // => Combo R Mode Display Name
        public const string ComboRModeName = ComboName + ".rmode"; // => Combo R Mode Name
        private const string ComboRMinDisplayName = "使用R丨敌人数量"; // => R Min Enemies to use R
        public const string ComboRMinName = ComboName + ".rmin";

        private const string ComboRPercentDisplayName = "使用R丨敌人HP"; // => Combo R Min. Enemies Health % Display Name
        public const string ComboRPercentName = ComboName + ".renemieshealthper"; // => Combo R Min. Enemies Health % Name
        private const string ComboRPercent2DisplayName = "使用R丨目标HP"; // => Combo R Min. Target Health % Display Name
        public const string ComboRPercent2Name = ComboName + ".renemyhealthper"; // => Combo R Min. Enemies Health % Name
        private const string ComboRSelfDisplayName = "使用R丨只有自己击飞才用"; // => R only self knockedup enemies Display Name
        public const string ComboRSelfName = ComboName + ".ronlyself"; // => R only self knockedup enemies Name
        private const string ComboRAirTimeDisplayName = "保持敌人在空中 (毫秒)"; // => R Keep in the air before casting Display Name
        public const string ComboRAirTimeName = ComboName + ".rairtime"; // => R Keep in the air before casting Name
        private const string ComboGapcloserModeDisplayName = "突进 模式"; // => Gapcloser Mode Display name
        public const string ComboGapcloserModeName = ComboName + ".gapclosermode"; // => Gapcloser Mode Display name
        private const string ComboGapcloserFModeDisplayName = "在攻击范围内自动跟随"; // => Gapcloser Follow Mode Display name
        public const string ComboGapcloserFModeName = ComboName + ".gapcloserfollowmode"; // => Gapcloser Follow Mode Display name

        private const string ComboERangeDisplayName = "踏前斩 (E) 范围";
        public const string ComboERangeName = ComboName + ".usecastingrange";

        #region Items

        private const string ComboItemsDisplayName = "物品 设置"; // => Items Display Name
        private const string ComboItemsName = ComboName + ".items"; // => Items Name

        private const string ComboItemsTiamatDisplayName = "使用 提亚玛特"; // => Tiamat Display Name
        public const string ComboItemsTiamatName = ComboItemsName + ".usetiamat"; // => Tiamat Name
        private const string ComboItemsHydraDisplayName = "使用 九头蛇"; // => Ravenous Hydra Display Name
        public const string ComboItemsHydraName = ComboItemsName + ".usehydra"; // => Ravenous Hydra Name
        private const string ComboItemsBilgewaterDisplayName = "使用 水银弯刀"; // => Bilgewater Cutlass Display Name
        public const string ComboItemsBilgewaterName = ComboItemsName + ".usebilgewater"; // => Bilgewater Cutlass Name
        private const string ComboItemsBotRkDisplayName = "使用 破败王者之刃"; // => Blade of the Ruined King Display Name
        public const string ComboItemsBotRkName = ComboItemsName + ".usebotrk"; // => Blade of the Ruined King Name

        #endregion

        #endregion

        #region Flee

        private const string FleeDisplayName = "逃跑 设置"; // => Flee Display Name
        private const string FleeName = ".flee"; // => Flee Name

        private const string FleeEnableDisplayName = "开启逃跑模式"; // => Flee Enabled Display Name
        public const string FleeEnableName = FleeName + ".useflee"; // => Flee Enabled Name
        private const string FleeKeyDisplayName = "按 键"; // => Flee Key Display Name
        public const string FleeKeyName = FleeName + ".usefleekey"; // => Flee Key Name
        private const string FleeFleeIntoTowersDisplayName = "敌人在塔下自动跑"; // => Flee into towers Display Name
        public const string FleeFleeIntoTowersName = FleeName + ".usefleetowers"; // => Flee into towers

        #endregion

        #region Farming

        private const string FarmingDisplayName = "打钱 设置"; // => Farming Display Name
        private const string FarmingName = ".farming"; // => Farming Name

        private const string FarmingLastHitQDisplayName = "[补刀] 使用 斩钢闪 (Q)"; // => Last Hit Q Display Name
        public const string FarmingLastHitQName = FarmingName + ".lhuseq"; // => Last Hit Q Name
        private const string FarmingLastHit3QDisplayName = "[补刀] 使用 斩钢闪 (Q第三下)"; // => Last Hit 3rd Q Display Name
        public const string FarmingLastHit3QName = FarmingName + ".lhuse3q"; // => Last Hit 3rd Q Name
        private const string FarmingLastHitEDisplayName = "[补刀] 使用 踏前斩 (E)"; // => Last Hit E Display Name
        public const string FarmingLastHitEName = FarmingName + ".lhusee"; // => Last Hit E Name
        private const string FarmingLastHitEaaDisplayName = "[补刀] 优先 E 补刀"; // => Last Hit Prioritize E over AA Display Name
        public const string FarmingLastHitEaaName = FarmingName + "lheoveraa"; // => Last Hit Prioritize E over AA Name
        private const string FarmingLastHitQaaDisplayName = "[补刀] 优先 Q 补刀"; // => Last Hit Prioritize Q over AA Display Name
        public const string FarmingLastHitQaaName = FarmingName + "lhqoveraa"; // => Last Hit Prioritize Q over AA Name
        private const string FarmingLastHitTurretDisplayName = "[补刀] E 清兵时自动回塔底"; // => Last Hit Use E to towers Display Name
        public const string FarmingLastHitTurretName = FarmingName + "lhuseut"; // => Last Hit Use E to towers Name

        private const string FarmingLaneClearQDisplayName = "[清线] 使用 斩钢闪 (Q)"; // => Lane Clear Q Display Name
        public const string FarmingLaneClearQName = FarmingName + ".lcuseq"; // => Lane Clear Q Name
        private const string FarmingLaneClear3QDisplayName = "[清线] 使用 斩钢闪 (Q) - 第三下"; // => Lane Clear 3rd Q Display Name
        public const string FarmingLaneClear3QName = FarmingName + ".lcuse3q"; // => Lane Clear 3rd Q Name
        private const string FarmingLaneClearEDisplayName = "[清线] 使用 踏前斩 (E)"; // => Lane Clear E Display Name
        public const string FarmingLaneClearEName = FarmingName + ".lcusee"; // => Lane Clae E Name
        private const string FarmingLaneClearEaaDisplayName = "[清线] 优先 E 清兵"; // => Lane Clear Prioritize E over AA Display Name
        public const string FarmingLaneClearEaaName = FarmingName + "lceoveraa"; // => Lane Clear Prioritize E over AA Name
        private const string FarmingLaneClearQaaDisplayName = "[清线] 优先 Q 清兵"; // => Lane Clear Prioritize Q over AA Display Name
        public const string FarmingLaneClearQaaName = FarmingName + "lcqoveraa"; // => Lane Clear Prioritize Q over AA Name
        private const string FarmingLaneClearTurretDisplayName = "[清线] E 清兵时自动回塔底"; // => Lane Clear Use E to towers Display Name
        public const string FarmingLaneClearTurretName = FarmingName + "lcuseut"; // => Lane Clear Use E to towers Name

        #endregion

        #region Killsteal

        private const string KillstealDisplayName = "击杀 设置"; // => Killsteal Display Name
        private const string KillstealName = ".ks"; // => Killsteal Name
        private const string KillstealEnabledDisplayName = "开 启"; // => Killsteal Enabled Display Name
        public const string KillstealEnabledName = KillstealName + ".kse"; // => Killsteal Enabled Name

        private const string KillstealQDisplayName = "使用 斩钢闪 (Q)"; // => Killsteal Q Display Name
        public const string KillstealQName = KillstealName + ".useq"; // => Killsteal Q Name
        private const string Killsteal3QDisplayName = "使用 斩钢闪 (Q)第3下"; // => Killsteal 3Q Display Name
        public const string Killsteal3QName = KillstealName + ".use3q"; // => Killsteal 3Q Name
        private const string KillstealEDisplayName = "使用 踏前斩 (E)"; // => Killsteal E Display Name
        public const string KillstealEName = KillstealName + ".usee"; // => Killsteal E Name
        private const string KillstealEIntoTowerDisplayName = "E自动回塔下"; // => Killsteal E Into Towers Display Name
        public const string KillstealEIntoTowerName = KillstealName + ".useeintotower"; // => Killsteal E Into Towers Name


        #endregion

        #region Windwall/Evade

        /* AUTO WINDWALL */
        public static Menu AutoWindMenu;
        private const string AutoWindWall = "自动 风墙";
        public const string AutoWindWallLoc = ".autoww";
        private const string AutoWindWallUse = "开 启";
        public const string AutoWindWallUseLoc = AutoWindWallLoc + ".usew";
        private const string AutoWindWallDelay = "风墙 延时";
        public const string AutoWindWallDelayLoc = AutoWindWallLoc + ".delay";

        /* EVADE */
        public static Menu EvadeMenu;
        private const string Evade = "躲避 设置";
        public const string EvadeLoc = ".evade";
        private const string EvadeUse = "使用E 躲避";
        public const string EvadeUseLoc = EvadeLoc + ".use";

        #endregion

        #region Misc

        private const string MiscDisplayName = "杂项 设置"; // => Misc Display Name
        private const string MiscName = ".misc"; // => Misc Name

        private const string MiscPacketsDisplayName = "封包"; // => Packets Display Name
        public const string MiscPacketsName = MiscName + ".packets"; // => Packets Display Name

        #endregion

        #endregion
    }
}