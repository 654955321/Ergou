using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Oracle.Core.Helpers;

namespace Oracle.Extensions
{
    internal class Defensives
    {
        private static Menu _mainMenu, _menuConfig;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public void Initialize(Menu root)
        {
            Game.OnUpdate += Game_OnGameUpdate;

            _mainMenu = new Menu("Defensives", "dmenu");
            _menuConfig = new Menu("Defensives Config", "dconfig");

            foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                _menuConfig.AddItem(new MenuItem("DefenseOn" + x.SkinName, "Use for " + x.SkinName)).SetValue(true);
            _mainMenu.AddSubMenu(_menuConfig);

            CreateMenuItem("Randuin's Omen", "Randuins", "selfcount", 40, 40);
            CreateMenuItem("Seraph's Embrace", "Seraphs",  "selfhealth", 40, 45);
            CreateMenuItem("Zhonya's Hourglass", "Zhonyas", "selfzhonya", 20, 50);
            CreateMenuItem("Face of the Mountain", "Mountain", "allyhealth", 20, 45);
            CreateMenuItem("Locket of Iron Solari", "Locket", "allyhealth", 40, 45);

            var tMenu = new Menu("Talisman", "tboost");
            tMenu.AddItem(new MenuItem("useTalisman", "Use Talisman of Ascension")).SetValue(true);
            tMenu.AddItem(new MenuItem("useAllyPct", "Use on ally %")).SetValue(new Slider(50, 1));
            tMenu.AddItem(new MenuItem("useEnemyPct", "Use on enemy %")).SetValue(new Slider(50, 1));
            tMenu.AddItem(new MenuItem("talismanMode", "Mode: ")).SetValue(new StringList(new[] {"Always", "Combo"}));
            _mainMenu.AddSubMenu(tMenu);

            var bMenu = new Menu("Banner", "bannerc");
            bMenu.AddItem(new MenuItem("useBanner", "Use Banner of Command")).SetValue(true);
            _mainMenu.AddSubMenu(bMenu);

            CreateMenuItem("Wooglet's Witchcap", "Wooglets", "selfzhonya", 20, 40);
            CreateMenuItem("Odyn's Veil", "Odyns", "selfcount", 40, 40);

            var oMenu = new Menu("Oracle's Lens", "olens");
            oMenu.AddItem(new MenuItem("useOracles", "Use on Stealth")).SetValue(true);
            oMenu.AddItem(new MenuItem("oracleMode", "Mode: ")).SetValue(new StringList(new[] { "Always", "Combo" }));
            _mainMenu.AddSubMenu(oMenu);

            root.AddSubMenu(_mainMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Me.IsValidTarget(300, false))
            {
                return;
            }

            UseItemCount("Odyns", 3180, 450f);
            UseItemCount("Randuins", 3143, 450f);

            UseItem("allyshieldlocket", "Locket", 3190, 600f);
            UseItem("selfshieldseraph", "Seraphs", 3040);
            UseItem("selfshieldzhonya", "Zhonyas", 3157);
            UseItem("allyshieldmountain", "Mountain", 3401, 700f);
            UseItem("selfshieldzhonya", "Wooglets", 3090);

            // Oracle's Lens 
            if (Items.HasItem(3364) && Items.CanUseItem(3364) && _mainMenu.Item("useOracles").GetValue<bool>())
            {
                if (Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active ||
                    _mainMenu.Item("oracleMode").GetValue<StringList>().SelectedIndex != 1)
                {
                    var target = Oracle.Friendly();
                    if (target.Distance(Me.ServerPosition, true) <= 600*600 && Oracle.Stealth ||
                        target.HasBuff("RengarRBuff", true))
                    {
                        Items.UseItem(3364, target.ServerPosition);
                        Oracle.Logger(Oracle.LogType.Action, "Using oracle's lens near " + target.SkinName + " (stealth)");
                    }
                }
            }

            // Banner of command (basic)
            if (Items.HasItem(3060) && Items.CanUseItem(3060) && _mainMenu.Item("useBanner").GetValue<bool>())
            {
                var minionList = MinionManager.GetMinions(Me.Position, 1000);

                foreach (
                    var minyone in 
                        minionList.Where(minion => minion.IsValidTarget(1000) && minion.BaseSkinName.Contains("MechCannon")))
                {
                    if (minyone.Health > minyone.Health/minyone.MaxHealth*50)
                    {
                        Items.UseItem(3060, minyone);
                        Oracle.Logger(Oracle.LogType.Action, "Using banner of command item on MechCannon!");
                    }
                }
            }

            // Talisman of Ascension
            if (Items.HasItem(3069) && Items.CanUseItem(3069) && _mainMenu.Item("useTalisman").GetValue<bool>())
            {
                if (!Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active &&
                    _mainMenu.Item("talismanMode").GetValue<StringList>().SelectedIndex == 1)
                {
                    return;
                }

                var target = Oracle.Friendly();
                if (target.Distance(Me.ServerPosition, true) > 600*600)
                {
                    return;
                }

                var lowTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .OrderBy(ex => ex.Health/ex.MaxHealth*100)
                        .First(x => x.IsValidTarget(1000));

                var aHealthPercent = target.Health/target.MaxHealth*100;
                var eHealthPercent = lowTarget.Health/lowTarget.MaxHealth*100;

                if (lowTarget.Distance(target.ServerPosition, true) <= 900*900 &&
                    (target.CountHerosInRange(false) > target.CountHerosInRange(true) &&
                     eHealthPercent <= _mainMenu.Item("useEnemyPct").GetValue<Slider>().Value))
                {
                    Items.UseItem(3069);
                    Oracle.Logger(Oracle.LogType.Action, "Using speed item on enemy " + lowTarget.SkinName + " (" +
                                                 lowTarget.Health/lowTarget.MaxHealth*100 + "%) is low!");
                }

                if (target.CountHerosInRange(false) > target.CountHerosInRange(true) &&
                    aHealthPercent <= _mainMenu.Item("useAllyPct").GetValue<Slider>().Value)
                {
                    Items.UseItem(3069);
                    Oracle.Logger(Oracle.LogType.Action,
                        "Using speed item on ally " + target.SkinName + " (" + aHealthPercent + "%) is low!");
                }
            }
        }

        private static void UseItemCount(string name, int itemId, float itemRange)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
            {
                return;
            }

            if (_mainMenu.Item("use" + name).GetValue<bool>())
            {
                if (Me.CountHerosInRange(true, itemRange) >=
                    _mainMenu.Item("use" + name + "Count").GetValue<Slider>().Value)
                {
                    Items.UseItem(itemId);
                    Oracle.Logger(Oracle.LogType.Action, "Used " + name + " on me ! (Item count)");
                }
            }
        }

        private static void UseItem(string menuvar, string name, int itemId, float itemRange = float.MaxValue)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
                return;

            if (!_mainMenu.Item("use" + name).GetValue<bool>())
                return;

            var target = itemRange > 5000 ? Me : Oracle.Friendly();
            if (target.Distance(Me.ServerPosition, true) > itemRange*itemRange || !target.IsValidState())
            {
                return;
            }
            
            var aHealthPercent = (int) ((target.Health/target.MaxHealth)*100);
            var iDamagePercent = (int) (Oracle.IncomeDamage/target.MaxHealth*100);

            if (!_mainMenu.Item("DefenseOn" + target.SkinName).GetValue<bool>())
            {
                return;
            }
  
            if (_mainMenu.Item("use" + name + "Ults").GetValue<bool>())
            {
                foreach (var buff in GameBuff.EvadeBuffs)
                {
                    foreach (var aura in target.Buffs)
                    {
                        if (!aura.Name.ToLower().Contains(buff.SpellName) && aura.Name.ToLower() != buff.BuffName)
                            continue;

                        Utility.DelayAction.Add(
                            buff.Delay, delegate
                            {
                                Oracle.Attacker = Oracle.GetEnemy(buff.ChampionName);
                                Oracle.AggroTarget = target;
                                Oracle.IncomeDamage =
                                    (float) Oracle.GetEnemy(buff.ChampionName).GetSpellDamage(Oracle.AggroTarget, buff.Slot);

                                // check if we still have buff and didn't walk out of it
                                if (aura.Name.ToLower().Contains(buff.SpellName) || aura.Name.ToLower() == buff.BuffName)
                                {
                                    Oracle.DangerUlt = Oracle.Origin.Item(buff.SpellName + "ccc").GetValue<bool>();
                                }

                                Oracle.Logger(Oracle.LogType.Danger,
                                    "(" + Oracle.Attacker.SkinName + ") Dangerous buff on " + Oracle.AggroTarget.SkinName + " should zhonyas!");
                            });
                    }
                }

                // +1 to allow potential counterplay
                if (target.CountHerosInRange(false) + 1 >= target.CountHerosInRange(true))
                {
                    if (Oracle.DangerUlt || Oracle.IncomeDamage >= target.Health || target.Health/target.MaxHealth*100 <= 15)
                    {
                        if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                        {
                            Items.UseItem(itemId, target);
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used " + name + " on " + target.SkinName + " (" + aHealthPercent +
                                "%)! (Dangerous Ult)");
                        }
                    }
                }


                if (_mainMenu.Item("use" + name + "Zhy").GetValue<bool>())
                {
                    if (Oracle.Danger || Oracle.IncomeDamage >= target.Health || target.Health/target.MaxHealth*100 <= 15)
                    {
                        if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                        {
                            Items.UseItem(itemId, target);
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used " + name + " on " + target.SkinName + " (" + aHealthPercent + "%)! (Dangerous Spell)");
                        }
                    }
                }
            }

            if (menuvar.Contains("shield"))
            {
                if (menuvar.Contains("zhonya"))
                {
                    if (_mainMenu.Item("use" + name + "Only").GetValue<bool>() &&
                        !(target.Health/target.MaxHealth*100 <= 20))
                    {
                        return;
                    }
                }

                if (aHealthPercent <= _mainMenu.Item("use" + name + "Pct").GetValue<Slider>().Value)
                {
                    if ((iDamagePercent >= 1 || Oracle.IncomeDamage >= target.Health))
                    {
                        if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                        {
                            Items.UseItem(itemId, target);
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used " + name + " on " + target.SkinName + " (" + aHealthPercent + "%)! (Low HP)");
                        }
                    }

                    if (iDamagePercent >= _mainMenu.Item("use" + name + "Dmg").GetValue<Slider>().Value)
                    {
                        if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                        {
                            Items.UseItem(itemId, target);
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used " + name + " on " + target.SkinName + " (" + aHealthPercent + "%)! (Damage Chunk)");
                        }
                    }                    
                }
            }
        }

        private static void CreateMenuItem(string displayname, string name, string type, int hpvalue, int dmgvalue)
        {
            var menuName = new Menu(name, name.ToLower());
            menuName.AddItem(new MenuItem("use" + name, "Use " + displayname)).SetValue(true);

            if (!type.Contains("count"))
            {
                menuName.AddItem(new MenuItem("use" + name + "Pct", "Use on HP %")).SetValue(new Slider(hpvalue));
                menuName.AddItem(new MenuItem("use" + name + "Dmg", "Use on Dmg dealt %")).SetValue(new Slider(dmgvalue));
            }

            if (type.Contains("count"))
                menuName.AddItem(new MenuItem("use" + name + "Count", "Use on Count")).SetValue(new Slider(3, 1, 5));

            if (!type.Contains("count"))
            {
                menuName.AddItem(new MenuItem("use" + name + "Zhy", "Use on Dangerous (Spells)")).SetValue(false);
                menuName.AddItem(new MenuItem("use" + name + "Ults", "Use on Dangerous (Ultimates Only)")).SetValue(true);

                if (type.Contains("zhonya"))
                    menuName.AddItem(new MenuItem("use" + name + "Only", "Use Only on Dangerous")).SetValue(true);
            }
  
            _mainMenu.AddSubMenu(menuName);
        }      
    }
}