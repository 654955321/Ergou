using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace xSaliceResurrected.Managers
{
    class ItemManager
    {

        private static Menu _myMenu;

        private string ActiveName { get; set; }

        private int ActiveId { get; set; }

        private int Range { get; set; }

        private string Type { get; set; }

        private int Mode { get; set; } // 0 = target, 1 = on click, 2 = toggle

        private static readonly List<ItemManager> ItemList = new List<ItemManager>();

        //summoners
        private static readonly SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

        public static bool UseTargetted;

        public static bool KillableTarget;

        public static Obj_AI_Hero Target;

        private static int lastMura;

        private static void CreateList()
        {
            ItemList.Add(new ItemManager
            {
                ActiveId = 3144,
                ActiveName = "水银",
                Type = "Offensive",
                Range = 450,
                Mode = 0,
            });

            ItemList.Add(new ItemManager
            {
                ActiveId = 3153,
                ActiveName = "破败",
                Type = "Offensive",
                Range = 450,
                Mode = 0,
            });

            ItemList.Add(new ItemManager
            {
                ActiveId = 3146,
                ActiveName = "科技枪",
                Type = "Offensive",
                Range = 700,
                Mode = 0,
            });

            ItemList.Add(new ItemManager
            {
                ActiveId = 3042,
                ActiveName = "魔切",
                Type = "Offensive",
                Range = int.MaxValue,
                Mode = 2,
            });

            ItemList.Add(new ItemManager
            {
                ActiveId = 3074,
                ActiveName = "九头蛇",
                Type = "Offensive",
                Range = 400,
                Mode = 1,
            });

            ItemList.Add(new ItemManager
            {
                ActiveId = 3077,
                ActiveName = "提亚马特",
                Type = "Offensive",
                Range = 400,
                Mode = 1,
            });

            ItemList.Add(new ItemManager
            {
                ActiveId = 3142,
                ActiveName = "幽梦",
                Type = "Offensive",
                Range = (int)(ObjectManager.Player.AttackRange * 2),
                Mode = 1,
            });

        }

        public static void AddToMenu(Menu theMenu)
        {
            _myMenu = theMenu;

            //add item list to menu
            CreateList();

            var offensiveItem = new Menu("进攻物品", "Offensive Items");
            {
                foreach (var item in ItemList)
                {
                    AddOffensiveItem(offensiveItem, item);
                }
                _myMenu.AddSubMenu(offensiveItem);
            }


            var summoners = new Menu("召唤师技能", "Summoners");
            {
                var ignite = new Menu("点燃", "Ignite");
                {
                    ignite.AddItem(new MenuItem("ignite", "使用点燃", true).SetValue(true));
                    ignite.AddItem(
                        new MenuItem("igniteMode", "点燃 模式", true).SetValue(new StringList(new[] { "连招", "抢人头" })));
                    summoners.AddSubMenu(ignite);
                }
                _myMenu.AddSubMenu(summoners);
            }

            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.OnAttack += OnAttack;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void AddOffensiveItem(Menu subMenu, ItemManager item)
        {
            var active = new Menu(item.ActiveName, item.ActiveName);
            {
                active.AddItem(new MenuItem(item.ActiveName, item.ActiveName, true).SetValue(true));
                active.AddItem(new MenuItem(item.ActiveName + "dmgCalc", "增加在伤害计算", true).SetValue(true));
                active.AddItem(new MenuItem(item.ActiveName + "killAble", "只有在敌人可以击杀时使用", true).SetValue(false));
                active.AddItem(new MenuItem(item.ActiveName + "always", "总是使用", true).SetValue(item.Mode == 1 || item.Mode == 2));
                active.AddItem(new MenuItem(item.ActiveName + "myHP", "使用|自己生命值 <= %", true).SetValue(new Slider(25)));
                active.AddItem(new MenuItem(item.ActiveName + "enemyHP", "使用|敌人生命值 <= %", true).SetValue(new Slider(50)));

                subMenu.AddSubMenu(active);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("Muramana") && Items.CanUseItem(3042) && Environment.TickCount - lastMura > 5000)
            {
                Items.UseItem(3042);
            }

            if (Target == null || ObjectManager.Player.IsDead)
                return;

            if (_myMenu.Item("ignite", true).GetValue<bool>())
            {
                //ignite
                int igniteMode = _myMenu.Item("igniteMode", true).GetValue<StringList>().SelectedIndex;

                if (KillableTarget && igniteMode == 0 && Ignite_Ready())
                    Use_Ignite(Target);
                else if (ObjectManager.Player.GetSummonerSpellDamage(Target, Damage.SummonerSpell.Ignite) > Target.Health + 20 && Ignite_Ready())
                    Use_Ignite(Target);
            }

            if (!UseTargetted)
                return;

            foreach (var item in ItemList.Where(x => x.Mode == 0 && Items.HasItem(x.ActiveId) && ShouldUse(x.ActiveName)))
            {
                if (Target != null && Items.CanUseItem(item.ActiveId))
                {
                    if (AlwaysUse(item.ActiveName))
                        Items.UseItem(item.ActiveId, Target);

                    if (KillableTarget)
                        Items.UseItem(item.ActiveId, Target);

                    if (ObjectManager.Player.HealthPercent <= UseAtMyHp(item.ActiveName) && !OnlyIfKillable(item.ActiveName))
                    {
                        Items.UseItem(item.ActiveId, Target);
                    }

                    if (Target.HealthPercent <= UseAtEnemyHp(item.ActiveName) && !OnlyIfKillable(item.ActiveName))
                    {
                        Items.UseItem(item.ActiveId, Target);
                    }
                }
            }

            //reset mode
            UseTargetted = false;
            Target = null;
            KillableTarget = false;
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !(target is Obj_AI_Hero))
                return;

            foreach (var item in ItemList.Where(x => x.Mode == 1 && Items.CanUseItem(x.ActiveId) && ShouldUse(x.ActiveName)))
            {
                if (AlwaysUse(item.ActiveName))
                    Items.UseItem(item.ActiveId);

                if (KillableTarget)
                    Items.UseItem(item.ActiveId);

                if (ObjectManager.Player.HealthPercent <= UseAtMyHp(item.ActiveName) && !OnlyIfKillable(item.ActiveName))
                {
                    Items.UseItem(item.ActiveId);
                }

                if (Target.HealthPercent <= UseAtEnemyHp(item.ActiveName) && !OnlyIfKillable(item.ActiveName))
                {
                    Items.UseItem(item.ActiveId);
                }
            }
        }

        private static void OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !(target is Obj_AI_Hero))
                return;

            foreach (var item in ItemList.Where(x => x.Mode == 2 && Items.CanUseItem(x.ActiveId) && ShouldUse(x.ActiveName)))
            {
                if (!ObjectManager.Player.HasBuff("Muramana"))
                {
                    //Game.PrintChat("RAWR");
                    if (AlwaysUse(item.ActiveName))
                    {
                        Items.UseItem(item.ActiveId);
                        lastMura = Environment.TickCount;
                    }

                    if (KillableTarget)
                    {
                        Items.UseItem(item.ActiveId, Target);
                        lastMura = Environment.TickCount;
                    }

                    if (ObjectManager.Player.HealthPercent <= UseAtMyHp(item.ActiveName) && !OnlyIfKillable(item.ActiveName))
                    {
                        Items.UseItem(item.ActiveId);
                        lastMura = Environment.TickCount;
                    }

                    if (Target.HealthPercent <= UseAtEnemyHp(item.ActiveName) && !OnlyIfKillable(item.ActiveName))
                    {
                        Items.UseItem(item.ActiveId);
                        lastMura = Environment.TickCount;
                    }
                }
                else if (ObjectManager.Player.HasBuff("Muramana"))
                {
                    lastMura = Environment.TickCount;
                }
            }
        }
        public static float CalcDamage(Obj_AI_Base target, double currentDmg)
        {
            double dmg = currentDmg;

            foreach (var item in ItemList.Where(x => Items.HasItem(x.ActiveId) && ShouldUse(x.ActiveName) && Items.CanUseItem(x.ActiveId) && AddToDmgCalc(x.ActiveName)))
            {
                //bilge
                if (item.ActiveId == 3144)
                    dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Bilgewater);

                //Botrk
                if (item.ActiveId == 3153)
                    dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Botrk);

                //hextech
                if (item.ActiveId == 3146)
                    dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Hexgun);

                //hydra
                if (item.ActiveId == 3074)
                    dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Hydra);

                //tiamat
                if (item.ActiveId == 3077)
                    dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Tiamat);

                //sheen
                if (Items.HasItem(3057))
                    dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, SheenDamage());

                //lich bane
                if (Items.HasItem(3100))
                    dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, LichDamage());
            }


            if (Ignite_Ready())
                dmg += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            return (float)dmg;
        }

        private static double SheenDamage()
        {
            double dmg = 0;

            dmg += ObjectManager.Player.FlatPhysicalDamageMod;
            return dmg;
        }

        public static double LichDamage()
        {
            double dmg = 0;

            dmg += .75 * ObjectManager.Player.FlatPhysicalDamageMod;
            dmg += .5 * ObjectManager.Player.FlatMagicDamageMod;
            return dmg;
        }

        private static bool Ignite_Ready()
        {
            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                return true;
            return false;
        }

        private static void Use_Ignite(Obj_AI_Hero target)
        {
            if (target != null && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target.Position) < 650)
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
        }

        private static bool ShouldUse(string name)
        {
            return _myMenu.Item(name, true).GetValue<bool>();
        }

        private static bool AlwaysUse(string name)
        {
            return _myMenu.Item(name + "always", true).GetValue<bool>();
        }

        private static bool AddToDmgCalc(string name)
        {
            return _myMenu.Item(name + "dmgCalc", true).GetValue<bool>();
        }

        private static bool OnlyIfKillable(string name)
        {
            return _myMenu.Item(name + "killAble", true).GetValue<bool>();
        }

        private static int UseAtMyHp(string name)
        {
            return _myMenu.Item(name + "myHP", true).GetValue<Slider>().Value;
        }

        private static int UseAtEnemyHp(string name)
        {
            return _myMenu.Item(name + "enemyHP", true).GetValue<Slider>().Value;
        }


    }
}
