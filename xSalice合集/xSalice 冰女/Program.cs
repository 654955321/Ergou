using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace LissandraLetitGoLetItGOOOOO
{
    class Program
    {
        public const string ChampionName = "Lissandra";
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell Q2;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //item and summoner
        public static Items.Item DFG;
        public static SpellSlot IgniteSlot;

        //Menu
        public static Menu menu;

        //spell settings
        public static Obj_SpellMissile eMissle;
        public static bool eCreated = false;

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != ChampionName) return;

            //intalize spell
            Q = new Spell(SpellSlot.Q, 725);
            Q2 = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 700);

            Q.SetSkillshot(0.50f, 100, 1300, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.50f, 150, 1300, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.50f, 110, 850, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            DFG = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline || Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            //Create the menu
            menu = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("My 走砍", "my_Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            //Target selector
            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);


            //Keys
            menu.AddSubMenu(new Menu("键位", "Keys"));
            menu.SubMenu("Keys").AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("stunMelles", "眩晕范围内敌人").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("stunTowers", "眩晕塔下敌人").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitQQ", "使用 Q 补刀").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LaneClearActive", "清线!").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            //Combo menu:
            menu.AddSubMenu(new Menu("连招", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("qHit", "Q 命中数量").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("rHp", "使用R|自己血量").SetValue(new Slider(20, 0, 100)));
            menu.SubMenu("Combo").AddItem(new MenuItem("defR", "R保护|敌人数量").SetValue(new Slider(3, 0, 5)));
            menu.SubMenu("Combo").AddItem(new MenuItem("dfg", "使用 冥火").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "使用 点燃").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("igniteMode", "模式").SetValue(new StringList(new[] { "连招", "抢人头" }, 0)));

            //Harass menu:
            menu.AddSubMenu(new Menu("Harass", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("qHit2", "Q HitChance").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));

            //Farming menu:
            menu.AddSubMenu(new Menu("骚扰", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "使用 Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "使用 W").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "使用 E").SetValue(false));

            //Misc Menu:
            menu.AddSubMenu(new Menu("杂项", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "使用 R 打断技能").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "使用 W 防止突进").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "使用 封包").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("smartKS", "使用 智能击杀系统").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseHAM", "总是 使用 E").SetValue(false));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseEGap", "使用 E 突进").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("gapD", "E突进|最小距离").SetValue(new Slider(600, 300, 1050)));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示组合连招伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings menu:
            menu.AddSubMenu(new Menu("范围", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("qExtend", "延伸 Q 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.PrintChat(ChampionName + " 鍐伴湝濂冲帆-|涓芥鍗搢--- 鍔犺級鎴愬姛锛佹饥鍖朾y楝肩伒锛丵Q缇361630847");
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (DFG.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (DFG.IsReady())
                damage = damage * 1.2;

            damage = damage - 15;

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        private static void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, string Source)
        {
            var qTarget = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;

            //E
            if (useE && eTarget != null && E.IsReady() && Player.Distance(eTarget) < E.Range && shouldE(eTarget))
            {
                E.Cast(eTarget, packets());
            }

            //R
            if (useR && qTarget != null && R.IsReady() && Player.Distance(qTarget) < R.Range)
            {
                castR(qTarget);
            }

            //Ignite
            if (qTarget != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (IgniteMode == 0 && GetComboDamage(qTarget) > qTarget.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, qTarget);
                }
            }

            //W
            if (useW && qTarget != null && W.IsReady())
            {
                var wPred = GetPCircle(Player.ServerPosition, W, qTarget, true);
                if(wPred.Hitchance > HitChance.High && Player.Distance(qTarget) <= W.Range)
                    W.Cast();
            }

            //Q
            if (useQ && Q.IsReady() && qTarget != null)
            {
                var qPred = Q2.GetPrediction(qTarget);
                var collision = qPred.CollisionObjects;

                if (collision.Count > 0 && Player.Distance(qTarget) <= Q2.Range)
                {
                    Q.Cast(qPred.CastPosition, packets());
                    return;
                }
                if(Q.GetPrediction(qTarget).Hitchance >= getHit(Source) && Player.Distance(qTarget) < Q.Range)
                {
                    Q.Cast(qTarget, packets());
                    return;
                }
            }

        }

        public static bool shouldE(Obj_AI_Hero target)
        {
            if (eCreated)
                return false;

            if (GetComboDamage(target) >= target.Health + 20)
                return true;

            if (menu.Item("UseHAM").GetValue<bool>())
                return true;

            return false;
        }

        public static void castR(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) > target.Health + 20)
            {
                if (target != null && DFG.IsReady() && menu.Item("dfg").GetValue<bool>())
                {
                    Items.UseItem(DFG.Id, target);
                }

                R.Cast(target, packets());
                return;
            }

            if ((Player.GetItemDamage(target, Damage.DamageItems.Dfg) + (Player.GetSpellDamage(target, SpellSlot.R) * 1.2)) > target.Health + 20)
            {
                if (target != null && DFG.IsReady() && menu.Item("dfg").GetValue<bool>())
                {
                    Items.UseItem(DFG.Id, target);
                }

                R.Cast(target, packets());
                return;
            }

            //Defensive R
            var rHp = menu.Item("rHp").GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (hpPercent < rHp)
            {
                R.CastOnUnit(Player, packets());
                return;
            }

            var rDef = menu.Item("defR").GetValue<Slider>().Value;

            if (Player.CountEnemiesInRange(300) >= rDef)
            {
                R.CastOnUnit(Player, packets());
                return;
            }
        }

        public static void smartKS()
        {
            if (!menu.Item("smartKS").GetValue<bool>())
                return;

            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= 1375 && champ.IsEnemy select champ).ToList();
            nearChamps.OrderBy(x => x.Health);

            foreach (var target in nearChamps)
            {
                //ignite
                if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Player.Distance(target.ServerPosition) <= 600)
                {
                    var IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
                    if (IgniteMode == 1 && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }

                //dfg
                if (DFG.IsReady() && Player.GetItemDamage(target, Damage.DamageItems.Dfg) > target.Health + 20 && Player.Distance(target.ServerPosition) <= 750)
                {
                    Items.UseItem(DFG.Id, target);
                    return;
                }

                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range && (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                {
                    if (E.IsReady() && E.GetPrediction(target).Hitchance >= HitChance.High)
                    {
                        E.Cast(target, packets());
                        return;
                    }
                }

                //W
                if (Player.Distance(target.ServerPosition) <= W.Range && (Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                        return;
                    }
                }
            }
        }

        public static HitChance getHit(string Source)
        {
            var hitC = HitChance.High;
            var qHit = menu.Item("qHit").GetValue<Slider>().Value;
            var harassQHit = menu.Item("qHit2").GetValue<Slider>().Value;

            // HitChance.Low = 3, Medium , High .... etc..
            if (Source == "Combo")
            {
                switch (qHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }
            else if (Source == "Harass")
            {
                switch (harassQHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }

            return hitC;
        }

        public static PredictionOutput GetPCircle(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {

            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = 1,
                Speed = float.MaxValue,
                From = pos,
                Range = float.MaxValue,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        public static void detonateE()
        {
            var enemy = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);

            if (eMissle != null && enemy.ServerPosition.Distance(eMissle.Position) < 110 && enemy != null && eCreated && menu.Item("ComboActive").GetValue<KeyBind>().Active && E.IsReady())
            {
                E.Cast();
                return;
            }
            else if (eMissle != null && enemy != null && eCreated && menu.Item("ComboActive").GetValue<KeyBind>().Active && menu.Item("UseEGap").GetValue<bool>()
                && Player.Distance(enemy) > enemy.Distance(eMissle.Position) && E.IsReady())
            {
                if (eMissle.EndPosition.Distance(eMissle.Position) < 400 && enemy.Distance(eMissle.Position) < enemy.Distance(eMissle.EndPosition))
                    E.Cast();
                else if (eMissle.Position == eMissle.EndPosition)
                    E.Cast();
            }

        }

        public static void gapClose()
        {
            var Target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
            var distance = menu.Item("gapD").GetValue<Slider>().Value;

            if (Player.Distance(Target.ServerPosition) >= distance && Target.IsValidTarget(E.Range) && !eCreated && E.GetPrediction(Target).Hitchance >= HitChance.Medium && E.IsReady())
            {
                E.Cast(Target, packets());
            }
        }

        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Damage.GetSpellDamage(Player, minion, SpellSlot.Q) - 10)
                    {
                        if (Q.IsReady())
                        {
                            Q.Cast(minion, packets());
                            return;
                        }
                    }
                }
            }
        }

        private static void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useW = menu.Item("UseWFarm").GetValue<bool>();
            var useE = menu.Item("UseEFarm").GetValue<bool>();

            if (useE && E.IsReady() && !eCreated)
            {
                var ePos = E.GetLineFarmLocation(allMinionsE);
                if (ePos.MinionsHit >= 3)
                    E.Cast(ePos.Position, packets());
            }

            if (useQ && Q.IsReady())
            {
                var qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 2)
                    Q.Cast(qPos.Position, packets());
            }

            if (useW && W.IsReady())
            {
                var wPos = W.GetLineFarmLocation(allMinionsW);
                if (wPos.MinionsHit >= 2)
                    W.Cast();
            }
        }
        public static void checkUnderTower()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && Player.Distance(enemy.ServerPosition) <= R.Range && enemy != null)
                {
                    foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret != null && turret.IsValid && turret.IsAlly && turret.Health > 0)
                        {
                            if (Vector2.Distance(enemy.Position.To2D(), turret.Position.To2D()) < 750 && R.IsReady())
                            {
                                R.Cast(enemy, packets());
                                return;
                            }
                        }
                    }
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            detonateE();

            smartKS();

            if (menu.Item("stunMelles").GetValue<KeyBind>().Active)
            {
                var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= 200 && champ.IsEnemy select champ).ToList();
                nearChamps.OrderBy(x => x.Health);

                if(nearChamps.FirstOrDefault() != null && R.IsReady()){
                    R.Cast(nearChamps.FirstOrDefault());
                        return;
                }
            }

            if (menu.Item("stunTowers").GetValue<KeyBind>().Active)
            {
                checkUnderTower();
            }

            if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                if (menu.Item("UseEGap").GetValue<bool>())
                    gapClose();

                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                {
                    lastHit();
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            if (menu.Item("qExtend").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Player.Position, Q2.Range, Color.Aquamarine);
            }

        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap").GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range))
                W.Cast();
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && unit != null && R.IsReady())
            {
                R.Cast(unit, packets());
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && name == "LissandraEMissile")
            {
                eMissle = spell;
                eCreated = true;
                return;
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && name == "LissandraEMissile")
            {
                eMissle = null;
                eCreated = false;
                return;
            }
        }
    }
}
