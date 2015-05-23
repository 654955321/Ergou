using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElAlistarReborn
{

    public class ElAlistarMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("El牛头 （重做）", "menu", true);

            var orbwalkerMenu = new Menu("走砍", "orbwalker");
            Alistar.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("目标选择", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            _menu.AddSubMenu(targetSelector);

            var comboMenu = new Menu("连招", "Combo");
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.Q", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.W", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.E", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.R", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.Count.Enemies", "使用R 范围内敌人数量").SetValue(new Slider(2, 1, 5)));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.HP.Enemies", "使用R 当自己血量低于").SetValue(new Slider(55)));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.Ignite", "使用 点燃").SetValue(true));
  
            _menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("骚扰", "Harass");
            harassMenu.AddItem(new MenuItem("ElAlistar.Harass.Q", "使用 Q").SetValue(true));

            _menu.AddSubMenu(harassMenu);

            var healMenu = new Menu("治愈", "Heal");
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Activated", "使用 治愈").SetValue(true));
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Player.HP", "使用 治愈 自己血量低于").SetValue(new Slider(55)));

            //heal ally
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Ally.Activated", "治愈 队友").SetValue(true));
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Ally.HP", "治愈 队友 血量低于").SetValue(new Slider(55)));
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Player.Mana", "禁用治愈 魔量低于").SetValue(new Slider(55)));

            _menu.AddSubMenu(healMenu);

            var miscMenu = new Menu("杂项", "Misc");
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.off", "禁用所有显示").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.Q", "显示 Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.W", "显示 W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.E", "显示 E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("xxx", ""));
            miscMenu.AddItem(new MenuItem("ElAlistar.Interrupt", "中断 法术").SetValue(true));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("关于作者", "jQuery"));
            credits.AddItem(new MenuItem("ElZilean.Paypal", "如果你喜欢作者的脚本 paypal捐赠地址:"));
            credits.AddItem(new MenuItem("ElZilean.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "版本: 1.0.0.0"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "作者：jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}