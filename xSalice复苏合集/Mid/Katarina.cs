using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.Mid
{
    class Katarina : Champion
    {
        public Katarina()
        {
            SetUpSpells();
            LoadMenu();
        }

        private void SetUpSpells()
        {
            //intalize spell
            SpellManager.Q = new Spell(SpellSlot.Q, 675);
            SpellManager.W = new Spell(SpellSlot.W, 375);
            SpellManager.E = new Spell(SpellSlot.E, 700);
            SpellManager.R = new Spell(SpellSlot.R, 550);

            SpellManager.Q.SetTargetted(400, 1400);

            SpellManager.SpellList.Add(Q);
            SpellManager.SpellList.Add(W);
            SpellManager.SpellList.Add(E);
            SpellManager.SpellList.Add(R);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("jFarm", "清野!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("lastHit", "补刀!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Wardjump", "逃跑/顺眼", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("eDis", "使用 E 只有范围 >", true).SetValue(new Slider(0, 0, 700)));
                combo.AddItem(new MenuItem("smartE", "智能 E 如果 R CD ", true).SetValue(false));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                combo.AddItem(new MenuItem("comboMode", "连招顺序", true).SetValue(new StringList(new[] { "QEW", "EQW" })));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                harass.AddItem(new MenuItem("harassMode", "连招顺序", true).SetValue(new StringList(new[] { "QEW", "EQW", "QW" }, 2)));
                //add to menu
                menu.AddSubMenu(harass);
            }
            //Farming menu:
            var farm = new Menu("清线", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q ", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "使用 W ", true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E ", true).SetValue(false));
                farm.AddItem(new MenuItem("UseQHit", "使用 Q 补刀", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWHit", "使用 W 补刀", true).SetValue(false));
                //add to menu
                menu.AddSubMenu(farm);
            }
            //killsteal
            var killSteal = new Menu("抢人头", "KillSteal");
            {
                killSteal.AddItem(new MenuItem("smartKS", "使用 智能抢人头模式", true).SetValue(true));
                killSteal.AddItem(new MenuItem("wardKs", "使用 顺眼 击杀", true).SetValue(true));
                killSteal.AddItem(new MenuItem("rKS", "使用 R 如果 可击杀", true).SetValue(true));
                killSteal.AddItem(new MenuItem("rCancel", "撤销 R 如果无法 击杀", true).SetValue(false));
                killSteal.AddItem(new MenuItem("KS_With_E", "禁用E抢人头!", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(killSteal);
            }
            //Misc Menu:
            var misc = new Menu("杂项", "Misc");
            {
                misc.AddItem(new MenuItem("autoWz", "自动 W 敌人", true).SetValue(true));
                misc.AddItem(new MenuItem("E_Delay_Slider", "E 延迟(ms)", true).SetValue(new Slider(0, 0, 1000)));
                //add to menu
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("显示", "Drawings");
            {
                drawing.AddItem(new MenuItem("QRange", "Q 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("WRange", "W 范围", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("ERange", "E 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("RRange", "R 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("Draw_Mode", "显示 E 模式", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示整套连招伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawing.AddItem(drawComboDamageMenu);
                drawing.AddItem(drawFill);
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
                menu.AddSubMenu(drawing);
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
                customMenu.AddItem(myCust.AddToMenu("清野 启用: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("补刀 启用: ", "lastHit"));
                customMenu.AddItem(myCust.AddToMenu("顺眼 启用: ", "jFarm"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) + Player.GetSpellDamage(enemy, SpellSlot.Q, 1);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady() || (RSpell.State == SpellState.Surpressed && R.Level > 0))
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;

            damage = ItemManager.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            Combo(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>());
        }

        private void Harass()
        {
            Harass(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>());
        }

        private void Combo(bool useQ, bool useW, bool useE, bool useR)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            int mode = menu.Item("comboMode", true).GetValue<StringList>().SelectedIndex;

            int eDis = menu.Item("eDis", true).GetValue<Slider>().Value;

            if (!target.IsValidTarget(E.Range))
                return;

            if (!target.HasBuffOfType(BuffType.Invulnerability) && !target.IsZombie)
            {
                if (mode == 0) //qwe
                {
                    //items

                    var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                    if (itemTarget != null && E.IsReady())
                    {
                        var dmg = GetComboDamage(itemTarget);
                        ItemManager.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ItemManager.KillableTarget = true;

                        ItemManager.UseTargetted = true;
                    }


                    if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target);
                    }

                    if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && Environment.TickCount - E.LastCastAttemptT > 0 &&
                        Player.Distance(target.Position) > eDis)
                    {
                        if (menu.Item("smartE", true).GetValue<bool>() &&
                            Player.CountEnemiesInRange(500) > 2 &&
                            (!R.IsReady() || !(RSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                        E.Cast(target);
                        E.LastCastAttemptT = Environment.TickCount + delay;
                    }
                }
                else if (mode == 1) //eqw
                {
                    //items
                    var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                    if (itemTarget != null && E.IsReady())
                    {
                        var dmg = GetComboDamage(itemTarget);
                        ItemManager.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ItemManager.KillableTarget = true;

                        ItemManager.UseTargetted = true;
                    }

                    if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && Environment.TickCount - E.LastCastAttemptT > 0 &&
                        Player.Distance(target.Position) > eDis)
                    {
                        if (menu.Item("smartE", true).GetValue<bool>() &&
                            Player.CountEnemiesInRange(500) > 2 &&
                            (!R.IsReady() || !(RSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                        E.Cast(target);
                        E.LastCastAttemptT = Environment.TickCount + delay;
                    }

                    if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target);
                    }
                }

                if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range)
                {
                    W.Cast();
                }

                if (useR && R.IsReady() &&
                    Player.CountEnemiesInRange(R.Range) > 0)
                {
                    if (!Q.IsReady() && !E.IsReady() && !W.IsReady())
                        R.Cast();
                }
            }
        }

        private void Harass(bool useQ, bool useW, bool useE)
        {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            int mode = menu.Item("harassMode", true).GetValue<StringList>().SelectedIndex;

            if (mode == 0) //qwe
            {
                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget);
                }

                if (useE && eTarget != null && E.IsReady())
                {
                    if (Player.Distance(eTarget.Position) < E.Range)
                        E.Cast(eTarget);
                }
            }
            else if (mode == 1) //eqw
            {
                if (useE && eTarget != null && E.IsReady())
                {
                    if (Player.Distance(eTarget.Position) < E.Range)
                        E.Cast(eTarget);
                }

                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget);
                }
            }
            else if (mode == 2)
            {
                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget);
                }
            }

            if (useW && wTarget != null && W.IsReady())
            {
                if (Player.Distance(wTarget.Position) <= W.Range)
                    W.Cast();
            }
        }

        private void LastHit()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.NotAlly);
            MinionManager.GetMinions(Player.ServerPosition, W.Range);

            var useQ = menu.Item("UseQHit", true).GetValue<bool>();
            var useW = menu.Item("UseWHit", true).GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget(Q.Range) &&
                        HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position) * 1000 / 1400)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 35)
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }
            }

            if (W.IsReady() && useW)
            {
                if (allMinions.Where(minion => minion.IsValidTarget(W.Range) && minion.Health < Player.GetSpellDamage(minion, SpellSlot.W) - 35).Any(minion => Player.Distance(minion.ServerPosition) < W.Range))
                {
                    W.Cast();
                }
            }
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady() && allMinionsQ[0].IsValidTarget(Q.Range))
            {
                Q.Cast(allMinionsQ[0]);
            }

            if (useE && allMinionsQ.Count > 0 && E.IsReady() && allMinionsQ[0].IsValidTarget(E.Range))
            {
                E.Cast(allMinionsE[0]);
            }

            if (useW && W.IsReady())
            {
                if (allMinionsW.Count > 0)
                    W.Cast();
            }
        }
        private void JungleFarm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.Neutral);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady() && allMinionsQ[0].IsValidTarget(Q.Range))
            {
                Q.Cast(allMinionsQ[0]);
            }

            if (useW && W.IsReady())
            {
                if (allMinionsW.Count > 0)
                    W.Cast();
            }
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            if (menu.Item("rCancel", true).GetValue<bool>() && Player.CountEnemiesInRange(570) > 1)
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1375) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                    bool shouldE = !menu.Item("KS_With_E", true).GetValue<KeyBind>().Active && Environment.TickCount - E.LastCastAttemptT > 0;
                    //QEW
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) +
                         Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady() && W.IsReady())
                        {
                            CancelUlt(target);
                            Q.Cast(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Environment.TickCount + delay;
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            return;
                        }
                    }

                    //E + W
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.W)) >
                        target.Health + 20)
                    {
                        if (E.IsReady() && W.IsReady())
                        {
                            CancelUlt(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Environment.TickCount + delay;
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            //Game.PrintChat("ks 5");
                            return;
                        }
                    }

                    //E + Q
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q)) >
                        target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady())
                        {
                            CancelUlt(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Environment.TickCount + delay;
                            Q.Cast(target);
                            //Game.PrintChat("ks 6");
                            return;
                        }
                    }

                    //Q
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                    {
                        if (Q.IsReady() && Player.Distance(target.ServerPosition) <= Q.Range)
                        {
                            CancelUlt(target);
                            Q.Cast(target);
                            //Game.PrintChat("ks 7");
                            return;
                        }
                        if (Q.IsReady() && E.IsReady() && Player.Distance(target.ServerPosition) <= 1375 &&
                            menu.Item("wardKs", true).GetValue<bool>() &&
                            target.CountEnemiesInRange(500) < 3)
                        {
                            CancelUlt(target);
                            WardJumper.JumpKs(target);
                            //Game.PrintChat("wardKS!!!!!");
                            return;
                        }
                    }

                    //E
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                    {
                        if (E.IsReady())
                        {
                            CancelUlt(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Environment.TickCount + delay;
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }

                    //R
                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R) * 5) > target.Health + 20 &&
                        menu.Item("rKS", true).GetValue<bool>())
                    {
                        if (R.IsReady())
                        {
                            R.Cast();
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }
                }
            }
        }

        private void CancelUlt(Obj_AI_Hero target)
        {
            if (Player.IsChannelingImportantSpell() || Player.HasBuff("katarinarsound", true))
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition);
                R.LastCastAttemptT = 0;
            }
        }

        private void ShouldCancel()
        {
            if (Player.CountEnemiesInRange(500) < 1)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (target == null)
                    return;

                R.LastCastAttemptT = 0;
                Player.IssueOrder(GameObjectOrder.MoveTo, target);
            }

        }

        private void AutoW()
        {
            if (!W.IsReady())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (target != null && !target.IsDead && target.IsEnemy &&
                    Player.Distance(target.ServerPosition) <= W.Range && target.IsValidTarget(W.Range))
                {
                    if (Player.Distance(target.ServerPosition) < W.Range)
                        W.Cast();
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe) return;

            if (args.SData.Name == "KatarinaR")
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
            }

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.R)
            {
                R.LastCastAttemptT = Environment.TickCount;
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            SmartKs();

            if (Player.IsChannelingImportantSpell() || Player.HasBuff("katarinarsound", true) || Player.HasBuff("KatarinaR"))
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
                ShouldCancel();
                return;
            }

            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);
            
            if (menu.Item("Wardjump", true).GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                WardJumper.WardJump();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("lastHit", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("jFarm", true).GetValue<KeyBind>().Active)
                    JungleFarm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (menu.Item("autoWz", true).GetValue<bool>())
                AutoW();
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, (spell.IsReady()) ? Color.Cyan : Color.DarkRed);
            }

            if (menu.Item("Draw_Mode", true).GetValue<Circle>().Active)
            {
                var wts = Drawing.WorldToScreen(Player.Position);

                Drawing.DrawText(wts[0], wts[1], Color.White,
                    menu.Item("KS_With_E", true).GetValue<KeyBind>().Active ? "Ks E Active" : "Ks E Off");
            }
        }

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
                return;

            if (Environment.TickCount < WardJumper.LastPlaced + 300)
            {
                var ward = (Obj_AI_Minion)sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(WardJumper.LastWardPos) < 500 && E.IsReady())
                {
                    E.Cast(ward);
                }
            }
        }
    }
}
