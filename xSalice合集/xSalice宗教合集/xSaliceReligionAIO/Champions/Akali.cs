using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
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
            Q = new Spell(SpellSlot.Q, 600);

            W = new Spell(SpellSlot.W, 700);

            E = new Spell(SpellSlot.E, 325);

            R = new Spell(SpellSlot.R, 800);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQ", "使用 Q 补刀!").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("法术菜单", "SpellMenu");
            {
                var wMenu = new Menu("W法术", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("useW_enemyCount", "使用W|周围敌人数量")).SetValue(new Slider(3, 1, 5));
                    wMenu.AddItem(new MenuItem("useW_Health", "使用W|血量低于").SetValue(new Slider(25)));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("E法术", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_On_Killable", "E 抢人头").SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Wait_Q", "等待Q被动").SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R法术", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Wait_For_Q", "等待Q被动").SetValue(false));
                    rMenu.AddItem(new MenuItem("R_If_Killable", "使用R|敌人可以击杀").SetValue(true));
                    rMenu.AddItem(new MenuItem("Dont_R_If", "禁用R|敌人数量大于")).SetValue(new Slider(3, 1, 5));
                    spellMenu.AddSubMenu(rMenu);
                }
                //add to menu
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "攻击 选定目标").SetValue(true));
                combo.AddItem(new MenuItem("Combo_mode", "连招模式").SetValue(new StringList(new[] { "正常", "Q-R-AA-Q-E", "Q-Q-R-E-AA" })));
                combo.AddItem(new MenuItem("Combo_Switch", "模式转换键位").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }
            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
                //add to menu
                menu.AddSubMenu(harass);
            }
            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q").SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E").SetValue(true));
                farm.AddItem(new MenuItem("LaneClear_useE_minHit", "使用E|小兵数量").SetValue(new Slider(2, 1, 6)));
                //add to menu
                menu.AddSubMenu(farm);
            }
            var drawMenu = new Menu("范围+", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用 所有").SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "显示 Q 范围").SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "显示 W 范围").SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "显示 E 范围").SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "显示 R 范围").SetValue(true));
                drawMenu.AddItem(new MenuItem("Current_Mode", "显示 当前 模式").SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害").SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示组合填充伤害").SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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

            if (HasBuff(target, "AkaliMota"))
                comboDamage += Player.CalcDamage(target, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * Player.FlatMagicDamageMod));

            comboDamage += Player.CalcDamage(target, Damage.DamageType.Magical, CalcPassiveDmg());

            if (rStacks > 0)
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * rStacks;

            if (Ignite_Ready())
                comboDamage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

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
                case 2:
                    if (useQ)
                        Cast_Q(true, 2);
                    if (useR)
                        Cast_R(2);
                    if (useE)
                        Cast_E(true, 2);
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
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }

            if (source == "Harass")
            {
                if (menu.Item("UseQFarmH", true).GetValue<bool>() && Q.IsReady())
                    Cast_Q(false);
            }

        }

        public void Farm()
        {
            if (menu.Item("UseQFarm", true).GetValue<bool>())
                Cast_Q(false);
            if (menu.Item("UseEFarm", true).GetValue<bool>())
                Cast_E(false);
        }

        private Obj_AI_Hero CheckMark(float range)
        {
            return ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(x => x.IsValidTarget(range) && HasBuff(x, "AkaliMota") && x.IsVisible);
        }

        private void Cast_Q(bool combo, int mode = 0)
        {
            if (!Q.IsReady())
                return;
            if (combo)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

                //focus target
                float range = Q.Range;
                if (GetTargetFocus(range) != null)
                    target = GetTargetFocus(range);

                if (!target.IsValidTarget(Q.Range))
                    return;

                if (CheckMark(Q.Range) != null)
                    target = CheckMark(Q.Range);

                if (mode == 0)
                {
                    Q.Cast(target, packets());
                }
                else if (mode == 1)
                {
                    if (!HasBuff(target, "AkaliMota"))
                        Q.Cast(target);
                }
                else if (mode == 2)
                {
                    Q.Cast(target);
                    if (HasBuff(target, "AkaliMota"))
                        Q.LastCastAttemptT = Environment.TickCount + 400;
                }
            }
            else
            {
                foreach (var minion in MinionManager.GetMinions(Player.Position, Q.Range).Where(minion => HasBuff(minion, "AkaliMota") && xSLxOrbwalker.InAutoAttackRange(minion)))
                    xSLxOrbwalker.ForcedTarget = minion;

                foreach (var minion in MinionManager.GetMinions(Player.Position, Q.Range).Where(minion => HealthPrediction.GetHealthPrediction(minion,
                        (int)(E.Delay + (minion.Distance(Player) / E.Speed)) * 1000) <
                                                             Player.GetSpellDamage(minion, SpellSlot.Q) &&
                                                             HealthPrediction.GetHealthPrediction(minion,
                                                                 (int)(E.Delay + (minion.Distance(Player) / E.Speed)) * 1000) > 0 &&
                                                             xSLxOrbwalker.InAutoAttackRange(minion)))
                    Q.Cast(minion);

                foreach (var minion in MinionManager.GetMinions(Player.Position, Q.Range).Where(minion => HealthPrediction.GetHealthPrediction(minion,
                        (int)(Q.Delay + (minion.Distance(Player) / Q.Speed))) <
                                                             Player.GetSpellDamage(minion, SpellSlot.Q)))
                    Q.Cast(minion);

                foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Where(minion => Player.Distance(minion) <= Q.Range))
                    Q.Cast(minion);
            }
        }

        private void Cast_W()
        {
            if (menu.Item("useW_enemyCount", true).GetValue<Slider>().Value > Player.CountEnemiesInRange(400) &&
                menu.Item("useW_Health", true).GetValue<Slider>().Value < (int)(Player.Health / Player.MaxHealth * 100))
                return;
            W.Cast(Player.Position, packets());
        }

        private void Cast_E(bool combo, int mode = 0)
        {
            if (!E.IsReady())
                return;
            if (combo)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                float range = E.Range;
                if (GetTargetFocus(range) != null)
                    target = GetTargetFocus(range);

                if (target == null || !target.IsValidTarget(E.Range))
                    return;

                if (CheckMark(E.Range) != null)
                    target = CheckMark(Q.Range);

                if (mode == 0)
                {
                    if (HasBuff(target, "AkaliMota") && !Q.IsReady())
                        E.Cast();
                    else if (E.IsKillable(target) && menu.Item("E_On_Killable", true).GetValue<bool>())
                        E.Cast();
                    else if (!menu.Item("E_Wait_Q", true).GetValue<bool>())
                        E.Cast();
                }
                else if (mode == 1)
                {
                    if (HasBuff(target, "AkaliMota") && xSLxOrbwalker.InAutoAttackRange(target))
                        xSLxOrbwalker.ForcedTarget = target;
                    else if (HasBuff(target, "AkaliMota") && !Q.IsReady())
                        E.Cast();
                    else if (E.IsKillable(target) && menu.Item("E_On_Killable", true).GetValue<bool>())
                        E.Cast();
                    else if (!menu.Item("E_Wait_Q", true).GetValue<bool>())
                        E.Cast();
                }
                else if (mode == 2)
                {
                    if (HasBuff(target, "AkaliMota"))
                    {
                        E.Cast();
                        menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-R-AA-Q-E", "Q-Q-R-E-AA" }));
                    }
                    else if (E.IsKillable(target) && menu.Item("E_On_Killable", true).GetValue<bool>())
                        E.Cast();
                }
            }
            else
            {
                if (MinionManager.GetMinions(Player.Position, E.Range).Count >= menu.Item("LaneClear_useE_minHit", true).GetValue<Slider>().Value)
                    E.Cast();
                foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Where(minion => Player.Distance(minion) <= E.Range))
                    if(E.GetDamage(minion) > minion.Health + 35)
                        E.Cast();
            }
        }

        private double GetSimpleDmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1);
            if (HasBuff(target, "AkaliMota"))
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

            float range = R.Range + Player.BoundingRadius;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (target == null)
                return;

            if (CheckMark(Q.Range) != null)
                target = CheckMark(R.Range);

            if (target.IsValidTarget(R.Range) && R.IsReady())
            {
                if (R.IsKillable(target) && menu.Item("R_If_Killable", true).GetValue<bool>())
                    R.Cast(target, packets());
                else if (GetSimpleDmg(target) > target.Health && Player.Distance(target) > Q.Range - 50)
                    R.Cast(target, packets());

                if (countEnemiesNearPosition(target.ServerPosition, 500) >=
                    menu.Item("Dont_R_If", true).GetValue<Slider>().Value)
                    return;

                if (mode == 0)
                {
                    if (menu.Item("R_Wait_For_Q", true).GetValue<bool>())
                    {
                        if (HasBuff(target, "AkaliMota"))
                        {
                            R.Cast(target, packets());
                        }
                    }
                    else
                    {
                        R.Cast(target, packets());
                    }
                }
                else if (mode == 1)
                {
                    if (HasBuff(target, "AkaliMota") && Q.IsReady())
                    {
                        R.Cast(target, packets());
                        menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-R-AA-Q-E", "Q-Q-R-E-AA" }));
                    }
                }
                else if (mode == 2)
                {
                    if (HasBuff(target, "AkaliMota") && Environment.TickCount - Q.LastCastAttemptT < Game.Ping)
                    {
                        R.Cast(target, packets());
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
                    menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-R-AA-Q-E", "Q-Q-R-E-AA" }, 1));
                    _lasttick = Environment.TickCount + 300;
                }
                else if (mode == 1)
                {
                    menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-R-AA-Q-E", "Q-Q-R-E-AA" }, 2));
                    _lasttick = Environment.TickCount + 300;
                }
                else
                {
                    menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-R-AA-Q-E", "Q-Q-R-E-AA" }));
                    _lasttick = Environment.TickCount + 300;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

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
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Q-R-AA-Q-E");
                else if (mode == 2)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Q-Q-R-E-AA");
            }
        }
    }
}
