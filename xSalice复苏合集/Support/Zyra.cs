using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Support
{
    class Zyra : Champion
    {
        public Zyra()
        {
            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            SpellManager.P = new Spell(SpellSlot.Q, 1470);
            SpellManager.Q = new Spell(SpellSlot.Q, 800);
            SpellManager.W = new Spell(SpellSlot.W, 825);
            SpellManager.E = new Spell(SpellSlot.E, 950);
            SpellManager.R = new Spell(SpellSlot.R, 700);

            SpellManager.P.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            SpellManager.Q.SetSkillshot(0.8f, 60f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            SpellManager.E.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            SpellManager.R.SetSkillshot(0.1f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Escape", "逃跑 使用 E", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, false, true, false));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }
            //Farming menu:
            var farm = new Menu("清线", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "使用 W", true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E", true).SetValue(false));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, false, true, false));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("杂项", "Misc");
            {
                misc.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(true, false, true, true));
                misc.AddItem(new MenuItem("E_GapCloser", "使用 E 防止突进", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "智能 抢人头", true).SetValue(true));
                misc.AddItem(new MenuItem("Auto_Bloom", "敌人靠近自动种子开花", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("显示", "Drawings");
            {
                drawing.AddItem(new MenuItem("Draw_Disabled", "禁用所有", true).SetValue(false));
                drawing.AddItem(new MenuItem("Draw_Q", "范围 Q", true).SetValue(true));
                drawing.AddItem(new MenuItem("Draw_W", "范围 Q", true).SetValue(true));
                drawing.AddItem(new MenuItem("Draw_E", "范围 E", true).SetValue(true));
                drawing.AddItem(new MenuItem("Draw_R", "范围 R", true).SetValue(true));

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
                customMenu.AddItem(myCust.AddToMenu("逃跑 启用: ", "Escape"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 2;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 3;

            damage = ItemManager.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            float dmg = GetComboDamage(target);

            if (useW)
            {
                if (useE)
                {
                    var pred = E.GetPrediction(target, true);
                    if (pred.Hitchance >= HitChanceManager.GetEHitChance(source) && E.IsReady())
                    {
                        E.Cast(target);
                        Cast_W(pred.CastPosition);
                    }
                }

                //items
                if (source == "Combo")
                {
                    var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                    if (itemTarget != null)
                    {
                        ItemManager.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ItemManager.KillableTarget = true;

                        ItemManager.UseTargetted = true;
                    }
                }

                if (useQ)
                {
                    var pred = Q.GetPrediction(target);
                    if (pred.Hitchance >= HitChanceManager.GetQHitChance(source) && pred.CastPosition.Distance(Player.ServerPosition) < Q.Range)
                    {
                        Q.Cast(pred.CastPosition);
                        Cast_W(pred.CastPosition);
                        return;
                    }
                }
            }
            else
            {
                if (useQ)
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance((source)));

                if (useE)
                    SpellCastManager.CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetEHitChance((source)));
            }

            if (useR)
                Cast_R();
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe) return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.E || castedSlot == SpellSlot.Q)
            {
                E.LastCastAttemptT = Environment.TickCount;
            }
        }

        private void Cast_W(Vector3 pos)
        {
            if (!W.IsReady() || Player.Distance(pos) > W.Range || W.Instance.Ammo == 0)
                return;

            if (Environment.TickCount - E.LastCastAttemptT > 100 + Game.Ping)
                return;

            if (W.Instance.Ammo == 1)// 1 cast
            {
                Utility.DelayAction.Add(50, () => W.Cast(pos));
            }
            else if (W.Instance.Ammo == 2)// 2 cast
            {
                Utility.DelayAction.Add(50, () => W.Cast(pos));
                Utility.DelayAction.Add(350, () => W.Cast(pos));
            }
        }

        private void Cast_R()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            var pred = R.GetPrediction(target, true);

            if (GetComboDamage(target) > target.Health - 150 && pred.Hitchance >= HitChance.High)
            {
                R.Cast(target);
            }
        }


        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady())
            {
                var pred = Q.GetCircularFarmLocation(allMinionsQ);
                Q.Cast(pred.Position);

                if (useW)
                    Cast_W(pred.Position.To3D());
            }
            if (useE && allMinionsE.Count > 0 && E.IsReady())
            {
                var pred = E.GetLineFarmLocation(allMinionsE);
                E.Cast(pred.Position);

                if (useW)
                    Cast_W(pred.Position.To3D());
            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(E.Range)).OrderByDescending(GetComboDamage))
            {
                //QEW
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health && Q.IsReady() && E.IsReady())
                {
                    E.Cast(target);
                    Q.Cast(target);
                    W.Cast(Q.GetPrediction(target).CastPosition);
                    return;
                }
                //Q + plants
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) * 2 > target.Health && Q.IsReady() && W.IsReady())
                {
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                    W.Cast(Q.GetPrediction(target).CastPosition);
                    return;
                }
                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target);
                    return;
                }

                //R
                if (Player.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && menu.Item("R_KS", true).GetValue<bool>())
                {
                    R.Cast(target);
                    return;
                }
            }
        }

        private void AutoBloom()
        {
            if (!Q.IsReady() || !menu.Item("Auto_Bloom", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(Q.Range)).OrderByDescending(GetComboDamage))
            {
                foreach (Obj_AI_Minion plants in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name == "Zyra" && x.Distance(Player.Position) < Q.Range))
                {
                    var predQ = Q.GetPrediction(target, true);

                    if (Q.IsReady() && plants.Distance(predQ.UnitPosition) < Q.Width)
                        Q.Cast(plants);
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Player.IsZombie)
            {
                var target = TargetSelector.GetTarget(P.Range, TargetSelector.DamageType.True);
                if (target == null)
                    return;
                var pred = P.GetPrediction(target);
                if (pred.Hitchance >= HitChance.High)
                    Q.Cast(target);
            }

            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            AutoBloom();

            if (menu.Item("Escape", true).GetValue<KeyBind>().Active && E.IsReady())
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(E.Range)).OrderBy(x => x.Distance(Player.Position)))
                {
                    E.Cast(target);
                    return;
                }
            }

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_GapCloser", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.Distance(Player.Position) < 300)
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
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}
