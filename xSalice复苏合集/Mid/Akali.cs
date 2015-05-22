using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Mid
{
    class Akali : Champion
    {
        public Akali()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 600);

            SpellManager.W = new Spell(SpellSlot.W, 700);

            SpellManager.E = new Spell(SpellSlot.E, 325);

            SpellManager.R = new Spell(SpellSlot.R, 700);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQ", "Q 补刀!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能菜单", "SpellMenu");
            {
                var wMenu = new Menu("W菜单", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("useW_enemyCount", "使用 W 敌人数量", true).SetValue(new Slider(3, 1, 5)));
                    wMenu.AddItem(new MenuItem("useW_Health", "使用 W 自己血量低于", true).SetValue(new Slider(25)));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("E菜单", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_On_Killable", "E 抢人头", true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Energy", "使用 E 能量限制", true).SetValue(new Slider(100, 0, 200)));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R菜单", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Wait_For_Q", "等待 Q 被动", true).SetValue(false));
                    rMenu.AddItem(new MenuItem("R_If_Killable", "R 如果敌人可击杀", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("Dont_R_If", "禁用 R 如果敌人数量大于", true).SetValue(new Slider(3, 1, 5)));
                    rMenu.AddItem(new MenuItem("R_Min", "使用 R 最小范围", true).SetValue(new Slider(400, 50, 700)));
                    spellMenu.AddSubMenu(rMenu);
                }
                //add to menu
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("Combo_mode", "连招 模式", true).SetValue(new StringList(new[] { "正常", "Q-延迟-R-AA-Q-AA" })));
                combo.AddItem(new MenuItem("Combo_Switch", "开关模式 键位", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }
            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(harass);
            }
            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E", true).SetValue(true));
                farm.AddItem(new MenuItem("LaneClear_useE_minHit", "使用 E 可攻击到小兵数量", true).SetValue(new Slider(2, 1, 6)));
                //add to menu
                menu.AddSubMenu(farm);
            }
            var drawMenu = new Menu("显示", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用 所有", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "范围 Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "范围 W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "范围 E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "范围 R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Current_Mode", "显示连招模式", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示整套连招伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
                customMenu.AddItem(myCust.AddToMenu("Q补刀 启用: ", "LastHitQ"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            var rStacks = GetRStacks();
            var comboDamage = 0d;
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;

            if (mode == 0)
            {
                if (Q.IsReady())
                    comboDamage += (Player.GetSpellDamage(target, SpellSlot.Q) +
                                    Player.CalcDamage(target, Damage.DamageType.Magical,
                                        (45 + 35 * Q.Level + 0.5 * Player.FlatMagicDamageMod)));
            }
            else if (Q.IsReady())
            {
                comboDamage += (Player.GetSpellDamage(target, SpellSlot.Q) + Player.CalcDamage(target, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * Player.FlatMagicDamageMod))) * 2;
            }

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (target.HasBuff("AkaliMota"))
                comboDamage += Player.CalcDamage(target, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * Player.FlatMagicDamageMod));

            comboDamage += Player.CalcDamage(target, Damage.DamageType.Magical, CalcPassiveDmg());

            if (rStacks > 0)
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * rStacks;

            if (SummonerManager.Ignite_Ready())
                comboDamage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private double CalcPassiveDmg()
        {
            return (0.06 + 0.01 * (Player.FlatMagicDamageMod / 6)) * (Player.FlatPhysicalDamageMod + Player.BaseAttackDamage);
        }

        private int GetRStacks()
        {
            return (from buff in Player.Buffs where buff.Name == "AkaliShadowDance" select buff.Count).FirstOrDefault();
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), false,
                 menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;

            switch (mode)
            {
                case 0:
                    if (useQ)
                        Cast_Q(true);
                    if (useE)
                        Cast_E(true);
                    if (useW)
                        Cast_W();
                    if (useR)
                        Cast_R(0);
                    break;
                case 1:
                    if (useQ)
                        Cast_Q(true, 1);
                    if (useR)
                        Cast_R(1);
                    if (useE)
                        Cast_E(true, 1);
                    if (useW)
                        Cast_W();
                    break;
            }

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
        }

        private void Farm()
        {
            if (menu.Item("UseQFarm", true).GetValue<bool>())
                Cast_Q(false);
            if (menu.Item("UseEFarm", true).GetValue<bool>())
                Cast_E(false);
        }

        private Obj_AI_Hero CheckMark(float range)
        {
            return ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(x => x.IsValidTarget(range) && x.HasBuff("AkaliMota") && x.IsVisible);
        }

        private void Cast_Q(bool combo, int mode = 0)
        {
            if (!Q.IsReady())
                return;
            if (combo)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

                if (!target.IsValidTarget(Q.Range))
                    return;

                if (CheckMark(Q.Range) != null)
                    target = CheckMark(Q.Range);

                if (mode == 0)
                {
                    Q.Cast(target);
                }
                else if (mode == 1)
                {
                    if (!target.HasBuff("AkaliMota"))
                        Q.Cast(target);
                }
            }
            else
            {
                if (MinionManager.GetMinions(Player.Position, Q.Range).Any(minion => minion.HasBuff("AkaliMota") && Orbwalker.InAutoAttackRange(minion)))
                {
                    return;
                }

                foreach (var minion in MinionManager.GetMinions(Player.Position, Q.Range).Where(minion => HealthPrediction.GetHealthPrediction(minion,
                        (int)(E.Delay + (minion.Distance(Player.Position) / E.Speed)) * 1000) <
                                                             Player.GetSpellDamage(minion, SpellSlot.Q) &&
                                                             HealthPrediction.GetHealthPrediction(minion,
                                                                 (int)(E.Delay + (minion.Distance(Player.Position) / E.Speed)) * 1000) > 0 &&
                                                             Orbwalker.InAutoAttackRange(minion)))
                    Q.Cast(minion);

                foreach (var minion in MinionManager.GetMinions(Player.Position, Q.Range).Where(minion => HealthPrediction.GetHealthPrediction(minion,
                        (int)(Q.Delay + (minion.Distance(Player.Position) / Q.Speed))) <
                                                             Player.GetSpellDamage(minion, SpellSlot.Q)))
                    Q.Cast(minion);

                foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Where(minion => Player.Distance(minion.Position) <= Q.Range))
                    Q.Cast(minion);
            }
        }

        private void Cast_W()
        {
            if (menu.Item("useW_enemyCount", true).GetValue<Slider>().Value > Player.CountEnemiesInRange(400) &&
                menu.Item("useW_Health", true).GetValue<Slider>().Value < (int)(Player.Health / Player.MaxHealth * 100))
                return;
            W.Cast(Player.Position);
        }

        private void Cast_E(bool combo, int mode = 0)
        {
            if (!E.IsReady())
                return;
            if (combo)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (target == null || !target.IsValidTarget(E.Range))
                    return;

                if (CheckMark(E.Range) != null)
                    target = CheckMark(Q.Range);

                if (mode == 0)
                {
                    if (Player.Mana >= menu.Item("E_Energy", true).GetValue<Slider>().Value)
                        E.Cast();
                    else if (E.IsKillable(target) && menu.Item("E_On_Killable", true).GetValue<bool>())
                        E.Cast();
                }
                else if (mode == 1)
                {
                    if (target.HasBuff("AkaliMota"))
                        return;
                    if (target.HasBuff("AkaliMota") && !Q.IsReady())
                        return;
                    if (Player.Mana >= menu.Item("E_Energy", true).GetValue<Slider>().Value)
                        E.Cast();
                    if (E.IsKillable(target) && menu.Item("E_On_Killable", true).GetValue<bool>())
                        E.Cast();
                }
            }
            else
            {
                if (MinionManager.GetMinions(Player.Position, E.Range).Count >= menu.Item("LaneClear_useE_minHit", true).GetValue<Slider>().Value)
                    E.Cast();
                foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Where(minion => Player.Distance(minion.Position) <= E.Range))
                    if (E.GetDamage(minion) > minion.Health + 35)
                        E.Cast();
            }
        }

        private double GetSimpleDmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1);
            if (target.HasBuff("AkaliMota"))
                dmg += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            if (E.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            if (R.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.R) * GetRStacks();

            return dmg;
        }

        private void Cast_R(int mode)
        {
            var target = TargetSelector.GetTarget(R.Range + Player.BoundingRadius, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            if (CheckMark(Q.Range) != null)
                target = CheckMark(R.Range);

            if (target.IsValidTarget(R.Range) && R.IsReady())
            {
                if (R.IsKillable(target) && menu.Item("R_If_Killable", true).GetValue<bool>())
                    R.Cast(target);
                else if (GetSimpleDmg(target) > target.Health && Player.Distance(target.Position) > Q.Range - 50)
                    R.Cast(target);

                if (target.CountEnemiesInRange(500) >=
                    menu.Item("Dont_R_If", true).GetValue<Slider>().Value)
                    return;

                if (Player.Distance(target.Position) < menu.Item("R_Min", true).GetValue<Slider>().Value)
                    return;

                if (mode == 0)
                {
                    if (menu.Item("R_Wait_For_Q", true).GetValue<bool>())
                    {
                        if (target.HasBuff("AkaliMota"))
                        {
                            R.Cast(target);
                        }
                    }
                    else
                    {
                        R.Cast(target);
                    }
                }
                else if (mode == 1)
                {
                    if (target.HasBuff("AkaliMota") && Q.IsReady())
                    {
                        R.Cast(target);
                        menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Q-Delay-R-AA-Q-AA" }));
                    }
                }
            }
        }

        private int _lasttick;

        private void ModeSwitch()
        {
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            int lasttime = Environment.TickCount - _lasttick;

            if (menu.Item("Combo_Switch", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                if (mode == 0)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Q-Delay-R-AA-Q-AA" }, 1));
                    _lasttick = Environment.TickCount + 300;
                }
                else
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Q-Delay-R-AA-Q-AA" }));
                    _lasttick = Environment.TickCount + 300;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            ModeSwitch();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LastHitQ", true).GetValue<KeyBind>().Active)
                    Cast_Q(false);

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
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

            if (menu.Item("Current_Mode", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
                if (mode == 0)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Normal");
                else if (mode == 1)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Q-Delay-R-AA-Q-AA");
            }
        }
    }
}
