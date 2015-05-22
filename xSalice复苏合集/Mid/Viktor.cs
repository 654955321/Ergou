using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Mid
{
    class Viktor : Champion
    {
        private static GameObject rObj = null;
        private static bool activeR = false;
        private static int _lastR;

        public Viktor()
        {
            LoadSpells();
            LoadMenu();
        }

        private void LoadSpells()
        {
            //intalize spell
            SpellManager.P = new Spell(SpellSlot.E, 3000);
            SpellManager.Q = new Spell(SpellSlot.Q, 700);
            SpellManager.W = new Spell(SpellSlot.W, 700);
            SpellManager.E = new Spell(SpellSlot.E, 540);
            SpellManager.E2 = new Spell(SpellSlot.E, 700);
            SpellManager.R = new Spell(SpellSlot.R, 700);

            SpellManager.W.SetSkillshot(.25f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            SpellManager.E.SetSkillshot(0.2f, 90, 1000, false, SkillshotType.SkillshotCircle);
            SpellManager.E2.SetSkillshot(0.2f, 90, 1000, false, SkillshotType.SkillshotCircle);
            SpellManager.P.SetSkillshot(0.2f, 90, 1000, false, SkillshotType.SkillshotCircle);
            SpellManager.R.SetSkillshot(0.25f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQQ", "补刀 Q", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("rAlways", "连招总是使用R", true).SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能菜单", "SpellMenu");
            {
                var qMenu = new Menu("Q菜单", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("QAARange", "只有在平A范围才使用Q", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("W菜单", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("wSlow", "自动 W 减速", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("wImmobile", "自动 W 不动的", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("wDashing", "自动 W 移动的", true).SetValue(true));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("E菜单", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("Line_If_Enemy_Count", "自动 E 如果敌人数量大于, 6 = 禁用", true).SetValue(new Slider(4, 1, 6)));
                    eMenu.AddItem(new MenuItem("Line_If_Enemy_Count_Combo", "连招使用 E 如果如果敌人数量大于, 6 = 禁用", true).SetValue(new Slider(3, 1, 6)));
                    spellMenu.AddSubMenu(eMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombos", "使用 R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(false, true, false, true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(false));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(false, true, false, false));
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
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, true, false, true));
                miscMenu.AddItem(new MenuItem("UseInt", "使用 R 中断法术", true).SetValue(true));
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
                drawMenu.AddItem(new MenuItem("Draw_E_Pred", "显示 E 最佳路线", true).SetValue(true));

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
                customMenu.AddItem(myCust.AddToMenu("Q补刀 启用: ", "LastHitQQ"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                damage += 5 * Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            }

            if (activeR)
                damage += Player.GetSpellDamage(enemy, SpellSlot.R, 1);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombos", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), false,
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            var range = E.IsReady() ? (E.Range + E2.Range) : Q.Range;
            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
            
            if (!target.IsValidTarget(range))
                return;

            var dmg = GetComboDamage(target);

            if (useE && E.IsReady())
            {
                SpellCastManager.CastBestLine(true, E, E2, (int)(E2.Range / 2), menu, 1, false);
            }

            if (useW &&  W.IsReady() && ShouldW(target, dmg))
            {
                W.Cast(target);
            }

            if (menu.Item("QAARange", true).GetValue<bool>())
            {
                if (useQ && target != null && Q.IsReady() && Player.Distance(target) <= Player.AttackRange) // Q only in AA range for guaranteed AutoAttack
                {
                    Q.Cast(target);
                    return;
                }
            }

            else if (useQ && Q.IsReady() && Player.Distance(target) <= Q.Range)
            {
                Q.Cast(target);
                return;
            }

            if (useR &&  R.IsReady() && !activeR && ShouldR(target, dmg) && R.GetPrediction(target).Hitchance >= HitChanceManager.GetRHitChance(source))
            {
                if (target != null) R.Cast(target.Position);
            }

        }

        private bool ShouldW(Obj_AI_Hero target, float dmg)
        {
            if (dmg >= target.Health)
                return true;

            var immobile = menu.Item("wSlow", true).GetValue<bool>();
            var slow = menu.Item("wImmobile", true).GetValue<bool>();
            var dashing = menu.Item("wImmobile", true).GetValue<bool>();

            if (W.GetPrediction(target).Hitchance == HitChance.Immobile && immobile)
                return true;

            if (target.HasBuffOfType(BuffType.Slow) && slow)
                return true;

            if (W.GetPrediction(target).Hitchance == HitChance.Dashing && dashing)
                return true;

            if (Player.Distance(target.ServerPosition) < 300)
                return true;

            return false;
        }

        private bool ShouldR(Obj_AI_Hero target, float dmg)
        {
            if (dmg > target.Health)
                return true;

            if (menu.Item("rAlways", true).GetValue<KeyBind>().Active)
                return true;

            return false;
        }

        private static void AutoR()
        {
            if (activeR && rObj != null)
            {
                foreach (
                    Obj_AI_Hero target in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => x.IsValidTarget(2000)).OrderByDescending(x => x.Distance(rObj.Position)))
                {
                    if (target != null)
                    {
                        if (R.IsReady())
                        {
                            R.Cast(target.ServerPosition);
                            return;
                        }
                    }
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SpellCastManager.CastBestLine(false, E, E2, (int)(E2.Range/2), menu);

            int rTimeLeft = Utils.TickCount - _lastR;
            if ((rTimeLeft <= 400))
            {
                AutoR();
                _lastR = Utils.TickCount - 250;
            }

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LastHitQQ", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

            }
        }

        private void LastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        Q.Cast(minion);
                        return;
                    }
                }
            }
        }

        public static MinionManager.FarmLocation GetBestLineFarmLocation(Vector2 lineSource, List<Vector2> minionPositions, float width, float range)
        {
            var result = new Vector2();
            var minionCount = 0;
            var startPos = ObjectManager.Player.ServerPosition.To2D();

            var max = minionPositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (minionPositions[j] != minionPositions[i])
                    {
                        minionPositions.Add((minionPositions[j] + minionPositions[i]) / 2);
                    }
                }
            }

            foreach (var pos in minionPositions)
            {
                if (pos.Distance(startPos, true) <= range * range)
                {
                    var endPos = startPos + range * (pos - startPos).Normalized();

                    var count =
                        minionPositions.Count(pos2 => pos2.Distance(startPos, endPos, true, true) <= width * width);

                    if (count >= minionCount)
                    {
                        result = endPos;
                        minionCount = count;
                    }
                }
            }

            return new MinionManager.FarmLocation(result, minionCount);
        }

        private void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            var minionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
            var points = minionsE.Select(x => x.ServerPosition.To2D()).ToList();
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady())
            {
                Q.Cast(allMinionsQ[0]);
            }

            if (useE && E.IsReady() && minionsE.Count > 0)
            {
                E2.UpdateSourcePosition(minionsE[0].ServerPosition, minionsE[0].ServerPosition);
                var pred = GetBestLineFarmLocation(minionsE[0].ServerPosition.To2D(), points, E.Width, E2.Range);

                if (pred.MinionsHit > 1)
                {
                    SpellCastManager.CastLineSpell(minionsE[0].ServerPosition, pred.Position.To3D());
                }
            }
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && R.IsReady())
            {
                R.Cast(unit.Position);
            }
        }

        protected override void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) < 3000)
            {
                if (obj.IsValid && obj.Name.Contains("Viktor_Base_R"))
                {
                    Console.WriteLine("R activated");
                    activeR = true;
                    rObj = obj;
                }
            }

        }

        protected override void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) < 3000)
            {
                if (obj.IsValid && obj.Name.Contains("Viktor_Base_R"))
                {
                    //Game.PrintChat("woot2");
                    activeR = false;
                    rObj = null;
                }
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

            if (menu.Item("Draw_E_Pred", true).GetValue<bool>() && E.IsReady())
            {
                SpellCastManager.DrawBestLine(E, E2, (int)(E2.Range / 2), 1, false);
            }
            
        }

    }
}
