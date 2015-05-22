using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace xSaliceResurrected.Managers
{
    class LagManager
    {
        private static Menu _menu;
        private static int _incrementTick;

        public static void AddLagManager(Menu menu)
        {
            _menu = new Menu("延迟管理", "Lag Manager");
            _menu.AddItem(new MenuItem("lagManagerEnabled", "启用", true).SetValue(true));
            _menu.AddItem(new MenuItem("lagManagerDelay", "延迟计算(秒), 增加或减少延迟", true).SetValue(new Slider(0)));

            menu.AddSubMenu(_menu);

            Game.OnUpdate += OnUpdate;
        }

        public static bool Enabled
        {
            get { return _menu.Item("lagManagerEnabled", true).GetValue<bool>(); }
        }

        public static bool ReadyState
        {
            get { return _incrementTick == 0; }
        }

        private static void OnUpdate(EventArgs args)
        {
            _incrementTick++;

            if (_incrementTick > _menu.Item("lagManagerDelay", true).GetValue<Slider>().Value)
                _incrementTick = 0;
        }

    }
}
