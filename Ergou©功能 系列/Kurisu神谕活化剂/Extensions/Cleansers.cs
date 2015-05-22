﻿using System;
﻿using System.Linq;
﻿using LeagueSharp;
using LeagueSharp.Common;
﻿using Oracle.Core.Helpers;

namespace Oracle.Extensions
{
    internal class Cleansers
    {
        private static Menu _menuConfig, _mainMenu;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public void Initialize(Menu root)
        {
            Game.OnUpdate += Game_OnGameUpdate;

            _mainMenu = new Menu("Cleansers", "cmenu");
            _menuConfig = new Menu("Cleansers Config", "cconfig");

            foreach (var a in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.Team == Me.Team))
                _menuConfig.AddItem(new MenuItem("cccon" + a.SkinName, "Use for " + a.SkinName)).SetValue(true);
            _mainMenu.AddSubMenu(_menuConfig);

            CreateMenuItem("Dervish Blade", "Dervish", 2);
            CreateMenuItem("Quicksilver Sash", "Quicksilver", 2);
            CreateMenuItem("Mercurial Scimitar", "Mercurial", 2);
            CreateMenuItem("Mikael's Crucible", "Mikaels", 2);

            // delay the cleanse value * 100
            _mainMenu.AddItem(new MenuItem("cleansedelay", "Cleanse delay ")).SetValue(new Slider(0, 0, 25));

            _mainMenu.AddItem(
                new MenuItem("cmode", "Mode: "))
                .SetValue(new StringList(new[] {"Always", "Combo"}, 1));


            root.AddSubMenu(_mainMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active ||
                _mainMenu.Item("cmode").GetValue<StringList>().SelectedIndex != 1)
            {
                UseItem("Mikaels", 3222, 600f);
                UseItem("Quicksilver", 3140);
                UseItem("Mercurial", 3139);
                UseItem("Dervish", 3137);
            }
        }

        private static void UseItem(string name, int itemId, float range = float.MaxValue)
        {
            if (!Items.CanUseItem(itemId) || !Items.HasItem(itemId))
                return;

            if (!_mainMenu.Item("use" + name).GetValue<bool>())
                return;

            var target = range > 5000 ? Me : Oracle.Friendly();
            if (_mainMenu.Item("cccon" + target.SkinName).GetValue<bool>())
            {
                if (target.Distance(Me.ServerPosition, true) <= range * range && target.IsValidState())
                {
                    var tHealthPercent = target.Health/target.MaxHealth*100;
                    var delay = _mainMenu.Item("cleansedelay").GetValue<Slider>().Value * 10;

                    foreach (var buff in GameBuff.CleanseBuffs)
                    {
                        var buffinst = target.Buffs;
                        if (buffinst.Any(aura => aura.Name.ToLower() == buff.BuffName ||
                                                 aura.Name.ToLower().Contains(buff.SpellName)))
                        {
                            if (!Oracle.Origin.Item("cure" + buff.BuffName).GetValue<bool>())
                            {
                                return;
                            }

                            Utility.DelayAction.Add(delay + buff.Delay, delegate
                            {
                                Items.UseItem(itemId, target);
                                Oracle.Logger(Oracle.LogType.Action,
                                    "Used " + name + " on " + target.SkinName + " (" + tHealthPercent + "%) for: " + buff.BuffName);
                            });
                        }
                    }

                    foreach (var b in target.Buffs)
                    {
                        if (Oracle.Origin.Item("slow").GetValue<bool>() && b.Type == BuffType.Slow)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (slow)");
                        }

                        if (Oracle.Origin.Item("stun").GetValue<bool>() && b.Type == BuffType.Stun)
                        {
                            
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (stun)");
                        }

                        if (Oracle.Origin.Item("charm").GetValue<bool>() && b.Type == BuffType.Charm)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (charm)");
                        }

                        if (Oracle.Origin.Item("taunt").GetValue<bool>() && b.Type == BuffType.Taunt)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (taunt)");
                        }

                        if (Oracle.Origin.Item("fear").GetValue<bool>() && b.Type == BuffType.Fear)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (fear)");
                        }

                        if (Oracle.Origin.Item("snare").GetValue<bool>() && b.Type == BuffType.Snare)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (snare)");
                        }

                        if (Oracle.Origin.Item("silence").GetValue<bool>() && b.Type == BuffType.Silence)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (silence)");
                        }

                        if (Oracle.Origin.Item("suppression").GetValue<bool>() && b.Type == BuffType.Suppression)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (suppression)");
                        }

                        if (Oracle.Origin.Item("polymorph").GetValue<bool>() && b.Type == BuffType.Polymorph)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (polymorph)");
                        }

                        if (Oracle.Origin.Item("blind").GetValue<bool>() && b.Type == BuffType.Blind)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (blind)");
                        }

                        if (Oracle.Origin.Item("poison").GetValue<bool>() && b.Type == BuffType.Poison)
                        {
                            Utility.DelayAction.Add(delay, () => Items.UseItem(itemId, target));
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used  " + name + "  on " + target.SkinName + " (" + tHealthPercent + "%) (poison)");
                        }
                    }
                }
            }
        }

        private static void CreateMenuItem(string displayname, string name, int ccvalue)
        {
            var menuName = new Menu(name, name);
            menuName.AddItem(new MenuItem("use" + name, "Use " + displayname)).SetValue(true);
            menuName.AddItem(new MenuItem(name + "Count", "Min spells to use"));
            menuName.AddItem(new MenuItem(name + "Duration", "Buff duration to use"));
            _mainMenu.AddSubMenu(menuName);
        }
    }
}