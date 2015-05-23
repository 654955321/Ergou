using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElVladimirReborn
{

    public class ElVladimirMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("El吸血鬼（重做）", "menu", true);

            //ElVladimir.Orbwalker
            var orbwalkerMenu = new Menu("走砍", "orbwalker");
            Vladimir.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _menu.AddSubMenu(orbwalkerMenu);

            //ElVladimir.TargetSelector
            var targetSelector = new Menu("目标选择", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            _menu.AddSubMenu(targetSelector);

            //ElVladimir.Combo
            var comboMenu = new Menu("连招", "Combo");
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.Q", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.W", "使用 W").SetValue(false));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.E", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.R", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.SmartUlt", "使用 智能大招").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.Count.R", "使用 R 最小敌方人数")).SetValue(new Slider(1, 1, 5));
            comboMenu.AddItem(new MenuItem("separator", ""));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.R.Killable", "使用 R 只有敌方可击杀").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.Ignite", "使用 点燃").SetValue(true));

            _menu.AddSubMenu(comboMenu);

            //ElVladimir.Harass
            var harassMenu = new Menu("骚扰", "Harass");
            harassMenu.AddItem(new MenuItem("ElVladimir.Harass.Q", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElVladimir.Harass.E", "使用 E").SetValue(true));

            //ElVladimir.Auto.Harass
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.Health.E", "使用 E 最低血量").SetValue(new Slider(20)));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.Activated", "自动骚扰", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("spacespacespace", ""));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.Q", "使用 Q").SetValue(true));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.E", "使用 E").SetValue(true));

            _menu.AddSubMenu(harassMenu);

            var clearMenu = new Menu("清线|清野", "Waveclear");
            clearMenu.AddItem(new MenuItem("ElVladimir.WaveClear.Q", "使用 Q 清线").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.WaveClear.E", "使用 E 清线").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.JungleClear.Q", "使用 Q 清野").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.JungleClear.E", "使用 E 清野").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.WaveClear.Health.E", "使用 E 最低血量").SetValue(new Slider(20)));

            _menu.AddSubMenu(clearMenu);

            var settingsMenu = new Menu("设置", "Settings");
            settingsMenu.AddItem(new MenuItem("ElVladimir.Settings.Stack.E", "自动堆叠 E 层数", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            settingsMenu.AddItem(new MenuItem("ElVladimir.Settings.Stack.HP", "自动堆叠 最少HP")).SetValue(new Slider(20));
            settingsMenu.AddItem(new MenuItem("ElVladimir.Settings.AntiGapCloser.Active", "防止突进")).SetValue(true);

            _menu.AddSubMenu(settingsMenu);

            //ElVladimir.Misc
            var miscMenu = new Menu("杂项", "Misc");
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.off", "禁用所有").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.Q", "显示 Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.W", "显示 W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.E", "显示 E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.R", "显示 R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.Text", "显示 文本信息").SetValue(true));
            miscMenu.AddItem(new MenuItem("separator1", ""));
            miscMenu.AddItem(new MenuItem("ElVladimir.misc.Notifications", "显示 通知").SetValue(true));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("关于作者", "jQuery");
            credits.AddItem(new MenuItem("ElVladimir.Paypal", "如果你喜欢作者的脚本 paypal捐赠地址:"));
            credits.AddItem(new MenuItem("ElVladimir.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "版本: 1.0.0.1"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "作者： jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}