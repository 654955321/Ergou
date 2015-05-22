using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;
using ObjectManager = LeagueSharp.ObjectManager;

namespace xSaliceResurrected.Top
{
    class Rumble : Champion
    {
        public Rumble()
        {
            LoadSpells();
            LoadMenu();
        }

        private void LoadSpells()
        {
            //intalize spell
            SpellManager.P = new Spell(SpellSlot.R, 4000);
            SpellManager.Q = new Spell(SpellSlot.Q, 500);
            SpellManager.W = new Spell(SpellSlot.W);
            SpellManager.E = new Spell(SpellSlot.E, 950);
            SpellManager.R = new Spell(SpellSlot.R, 1700);
            SpellManager.R2 = new Spell(SpellSlot.R, 1000);

            SpellManager.E.SetSkillshot(0.25f, 70, 1200, true, SkillshotType.SkillshotLine);
            SpellManager.P.SetSkillshot(0.4f, 130, 2500, false, SkillshotType.SkillshotLine);
            SpellManager.R.SetSkillshot(0.4f, 130, 2500, false, SkillshotType.SkillshotLine);
            SpellManager.R2.SetSkillshot(0.4f, 130, 2600, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitE", "补刀 使用 E!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("UseMecR", "强制 大招", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能菜单", "SpellMenu");
            {
                var qMenu = new Menu("Q菜单", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Auto_Heat", "使用 Q 堆积热量", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q_Over_Heat", "Q 智能 过热 击杀", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("W菜单", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_Auto_Heat", "使用 W 堆积热量", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_Always", "总是使用W在 连招/骚扰", true).SetValue(false));
                    wMenu.AddItem(new MenuItem("W_Block_Spell", "使用 W 接入", true).SetValue(true));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("E菜单", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Auto_Heat", "使用 E 堆积热量", true).SetValue(false));
                    eMenu.AddItem(new MenuItem("E_Over_Heat", "E 智能 过热 击杀", true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R菜单", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("Line_If_Enemy_Count", "自动 R 如果敌人数量大于, 6 = 禁用", true).SetValue(new Slider(4, 1, 6)));
                    rMenu.AddItem(new MenuItem("Line_If_Enemy_Count_Combo", "连招使用R 如果敌人数量大于, 6 = 禁用", true).SetValue(new Slider(3, 1, 6)));
                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombos", "使用 R", true).SetValue(false));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(false, false, true, true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(false));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(false, false, true, true));
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("杂项", "Misc");
            {
                miscMenu.AddItem(new MenuItem("Stay_Danger", "保持过热状态", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                miscMenu.AddItem(new MenuItem("E_Gap_Closer", "使用 E 防止突进", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("显示", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用所有", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "范围 Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "范围 W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "范围 E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "范围 R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R_Pred", "显示 R Best Line", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示完整连招伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawMenu.AddItem(drawComboDamageMenu);
                drawMenu.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };

                //add to menu
                menu.AddSubMenu(drawMenu);
            }

            var customMenu = new Menu("自定义 键位启用栏 显示", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.AddItem(new MenuItem("custMenu", "移动菜单", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
                customMenu.AddItem(new MenuItem("enableCustMenu", "启用", true).SetValue(true));
                customMenu.AddItem(myCust.AddToMenu("连招 启用: ", "ComboActive"));
                customMenu.AddItem(myCust.AddToMenu("骚扰 启用: ", "HarassActive"));
                customMenu.AddItem(myCust.AddToMenu("自动骚扰(T) 启用: ", "HarassActiveT"));
                customMenu.AddItem(myCust.AddToMenu("清线 启用: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("E补刀 启用: ", "LastHitE"));
                customMenu.AddItem(myCust.AddToMenu("强制大招 启用: ", "UseMecR"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += GetCurrentHeat() > 50 ? Player.GetSpellDamage(target, SpellSlot.Q) * 2 : Player.GetSpellDamage(target, SpellSlot.Q);

            if (E.IsReady())
                comboDamage += GetCurrentHeat() > 50 ? Player.GetSpellDamage(target, SpellSlot.E) * 1.5 : Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * 3;

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombos", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            if (useQ && ShouldQ(target))
                Q.Cast(target);

            if (useW && menu.Item("W_Always", true).GetValue<bool>() && W.IsReady())
                W.Cast();

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    var dmg = GetComboDamage(itemTarget);
                    ItemManager.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ItemManager.KillableTarget = true;

                    ItemManager.UseTargetted = true;
                }
            }

            if (useE && ShouldE(target, source))
                E.Cast(target);

            if (useR && GetComboDamage(target) > target.Health)
                SpellCastManager.CastSingleLine(R, R2, true);
        }

        private void Farm()
        {
            if (!Orbwalking.CanMove(40))
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0)
                Q.Cast(allMinionsQ[0]);

            if (useE && allMinionsE.Count > 0)
                E.Cast(allMinionsE[0]);
        }

        private void LastHit()
        {
            if (!Orbwalking.CanMove(40))
                return;

            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionsE.Count > 0 && E.IsReady())
            {
                foreach (var minion in allMinionsE)
                {
                    if (E.IsKillable(minion))
                        E.Cast(minion);
                }
            }

        }

        private bool ShouldQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady())
                return false;

            if (Player.Distance(target.Position) > Q.Range)
                return false;

            if (!menu.Item("Q_Over_Heat", true).GetValue<bool>() && GetCurrentHeat() > 80)
                return false;

            if (GetCurrentHeat() > 80 && !(Player.GetSpellDamage(target, SpellSlot.Q, 1) + Player.GetAutoAttackDamage(target) * 2 > target.Health))
                return false;

            return true;
        }

        private bool ShouldE(Obj_AI_Hero target, string source)
        {
            if (!E.IsReady())
                return false;

            if (Player.Distance(target.Position) > E.Range)
                return false;

            if (E.GetPrediction(target).Hitchance < HitChanceManager.GetEHitChance(source))

                if (!menu.Item("E_Over_Heat", true).GetValue<bool>() && GetCurrentHeat() > 80)
                    return false;

            if (GetCurrentHeat() > 80 && !(Player.GetSpellDamage(target, SpellSlot.E, 1) + Player.GetAutoAttackDamage(target) * 2 > target.Health))
                return false;

            return true;
        }

        private void StayInDangerZone()
        {
            if (Player.InFountain() || Player.IsRecalling())
                return;

            if (GetCurrentHeat() < 31 && W.IsReady() && menu.Item("W_Auto_Heat", true).GetValue<bool>())
            {
                W.Cast();
                return;
            }

            if (GetCurrentHeat() < 31 && Q.IsReady() && menu.Item("Q_Auto_Heat", true).GetValue<bool>())
            {
                var enemy = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).OrderBy(x => Player.Distance(x.Position)).FirstOrDefault();

                if (enemy != null)
                    Q.Cast(enemy.ServerPosition);
                return;
            }

            if (GetCurrentHeat() < 31 && E.IsReady() && menu.Item("E_Auto_Heat", true).GetValue<bool>())
            {
                var enemy = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead).OrderBy(x => Player.Distance(x.Position)).FirstOrDefault();

                if (enemy != null)
                    E.Cast(enemy);
            }

        }

        private float GetCurrentHeat()
        {
            return Player.Mana;
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            SpellCastManager.CastBestLine(false, R, R2, (int)(R2.Range / 2), menu, .9f);

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("UseMecR", true).GetValue<KeyBind>().Active)
                    SpellCastManager.CastBestLine(true, R, R2, (int)(R2.Range / 2 + 100), menu, .9f);

                if (menu.Item("LastHitE", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
            //stay in dangerzone
            if (menu.Item("Stay_Danger", true).GetValue<KeyBind>().Active)
                StayInDangerZone();
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsEnemy && unit.Type == GameObjectType.obj_AI_Hero && W.IsReady() && menu.Item("W_Block_Spell", true).GetValue<bool>())
            {
                if (Player.Distance(args.End) < 400 && GetCurrentHeat() < 70)
                {
                    //Game.PrintChat("shielding");
                    W.Cast();
                }
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_Gap_Closer", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {

            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range - 2, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);


            if (menu.Item("Draw_R_Pred", true).GetValue<bool>() && R.IsReady())
            {
                SpellCastManager.DrawBestLine(R, R2, (int)(R2.Range/2), .9f);
            }
        }
    }
}
