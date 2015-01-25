using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace KaiHelper.Activator
{
    internal class AutoPot
    {
        private readonly Menu _menu;
        private int _level;

        public AutoPot(Menu menu)
        {
            _menu = menu.AddSubMenu(new Menu("药剂 大师", "PotionManager"));
            _menu.AddItem(new MenuItem("HPTrigger", "磕红丨触发血量比").SetValue(new Slider(30)));
            _menu.AddItem(new MenuItem("HealthPotion", "开启丨自动磕红").SetValue(true));
            _menu.AddItem(new MenuItem("MPTrigger", "磕蓝丨触发血量比").SetValue(new Slider(30)));
            _menu.AddItem(new MenuItem("ManaPotion", "开启丨自动磕蓝").SetValue(true));
            MenuItem autoarrangeMenu = _menu.AddItem(new MenuItem("AutoArrange", "自动 排列").SetValue(false));
            autoarrangeMenu.ValueChanged += AutoRangeValueChanged;

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void SetMana()
        {
            _menu.Item("HPTrigger")
                .SetValue(new Slider(FomularPercent((int) ObjectManager.Player.MaxHealth, 150), 1, 99));
            if (ObjectManager.Player.MaxMana <= 0)
            {
                _menu.Item("ManaPotion").SetValue(false);
            }
            else
            {
                _menu.Item("MPTrigger")
                    .SetValue(new Slider(FomularPercent((int) ObjectManager.Player.MaxMana, 100), 1, 99));
            }
        }

        private void AutoRangeValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                SetMana();
            }
        }

        private int FomularPercent(int max, int cur)
        {
            return (int) (100 - ((cur * 1.0) / max) * 100);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (_menu.Item("AutoArrange").GetValue<bool>())
            {
                int level = ObjectManager.Player.Level;
                if (level > _level)
                {
                    _level = level;
                    SetMana();
                }
            }
            if (!ObjectManager.Player.IsDead && !ObjectManager.Player.InFountain() &&
                !ObjectManager.Player.HasBuff("Recall"))
            {
                bool hasItemCrystalFlask = Items.HasItem(2041);
                bool buffItemCrystalFlask = false;
                if (_menu.Item("HealthPotion").GetValue<bool>())
                {
                    bool hasItemMiniRegenPotion = Items.HasItem(2010);
                    bool hasHealthPotion = Items.HasItem(2003);
                    if (ObjectManager.Player.HealthPercentage() <= _menu.Item("HPTrigger").GetValue<Slider>().Value)
                    {
                        if (hasItemCrystalFlask)
                        {
                            if (ObjectManager.Player.ManaPercentage() <=
                                _menu.Item("MPTrigger").GetValue<Slider>().Value ||
                                !hasHealthPotion && !hasItemMiniRegenPotion)
                            {
                                UseItem(2041, "ItemCrystalFlask");
                                buffItemCrystalFlask = true;
                            }
                            else if (hasHealthPotion)
                            {
                                UseItem(2003, "Health Potion");
                            }
                            else
                            {
                                UseItem(2010, "ItemMiniRegenPotion");
                            }
                        }
                        else if (hasHealthPotion)
                        {
                            UseItem(2003, "Health Potion");
                        }
                        else if (hasItemMiniRegenPotion)
                        {
                            UseItem(2010, "ItemMiniRegenPotion");
                        }
                    }
                }
                if (buffItemCrystalFlask)
                {
                    return;
                }
                if (!_menu.Item("ManaPotion").GetValue<bool>())
                {
                    return;
                }
                if (!(ObjectManager.Player.ManaPercentage() <= _menu.Item("MPTrigger").GetValue<Slider>().Value))
                {
                    return;
                }
                bool hasManaPotion = Items.HasItem(2004);
                if (hasManaPotion)
                {
                    UseItem(2004, "Mana Potion");
                }
                else if (hasItemCrystalFlask)
                {
                    UseItem(2041, "ItemCrystalFlask");
                }
            }
        }

        private static void UseItem(int id, string displayName)
        {
            if (!ObjectManager.Player.HasBuff(displayName))
            {
                Items.UseItem(id);
            }
        }
    }
}