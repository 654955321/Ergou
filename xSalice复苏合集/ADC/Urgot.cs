using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.ADC
{
    class Urgot : Champion
    {
        public Urgot ()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 1000);
            SpellManager.Q.SetSkillshot(0.2667f, 60f, 1600f, true, SkillshotType.SkillshotLine);

            SpellManager.Q2 = new Spell(SpellSlot.Q, 1200);
            SpellManager.Q2.SetSkillshot(0.3f, 60f, 1800f, false, SkillshotType.SkillshotLine);

            SpellManager.W = new Spell(SpellSlot.W);

            SpellManager.E = new Spell(SpellSlot.E, 850);
            SpellManager.E.SetSkillshot(0.2658f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            SpellManager.R = new Spell(SpellSlot.R, 550);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能菜单", "SpellMenu");
            {
                var wMenu = new Menu("W菜单", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_If_HP", "使用 W 如果 HP <= ", true).SetValue(new Slider(50)));
                    spellMenu.AddSubMenu(wMenu);
                }

                var rMenu = new Menu("R菜单", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Safe_Net", "禁用 R >= 交换后敌人数量", true).SetValue(new Slider(2, 0, 5)));
                    rMenu.AddItem(new MenuItem("R_If_UnderTurret", "如果敌人在我方塔下自动R", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("R_On_Killable", "只有敌人可击杀才使用R", true).SetValue(true));
                    rMenu.AddSubMenu(new Menu("Don't use R on", "禁用_R"));
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team)
                        )
                        rMenu.SubMenu("Dont_R")
                            .AddItem(new MenuItem("Dont_R" + enemy.BaseSkinName, enemy.BaseSkinName, true).SetValue(false));

                    spellMenu.AddSubMenu(rMenu);
                }
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, false, true, false));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, false, true, false));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 50);
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E", true).SetValue(false));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 50);
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("杂项", "Misc");
            {
                //aoe
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, false, true, false));
                miscMenu.AddItem(new MenuItem("smartKS", "智能 抢人头", true).SetValue(true));

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
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q) * 2;

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 5);
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

            var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget(Q2.Range))
                return;

            //items
            if (source == "Combo")
            {
                var dmg = GetComboDamage(target);
                ItemManager.Target = target;

                //see if killable
                if (dmg > target.Health - 50)
                    ItemManager.KillableTarget = true;

                ItemManager.UseTargetted = true;
            }

            if (useR && R.IsReady())
                Cast_R();
            if (useW && W.IsReady())
                Cast_W(target);
            if (useE && E.IsReady())
                SpellCastManager.CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Physical, HitChanceManager.GetEHitChance(source));
            if (useQ && Q.IsReady())
                Cast_Q(target, source);
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            var target = (Obj_AI_Base)mytarget;

            if (!menu.Item("ComboActive", true).GetValue<KeyBind>().Active || !unit.IsMe || !(target is Obj_AI_Hero))
                return;

            if (menu.Item("UseECombo", true).GetValue<bool>() && E.IsReady())
                E.Cast(target);
            if (menu.Item("UseQCombo", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(target);
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                SpellCastManager.CastBasicFarm(Q);
            if (useE)
            {
                var allMinionECount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
                var pred = E.GetCircularFarmLocation(allMinionECount);
                if (pred.MinionsHit > 1)
                    E.Cast(pred.Position);
            }
        }

        private void Cast_R()
        {
            if (R.Instance.Level == 1)
                R.Range = 550;
            else if (R.Instance.Level == 2)
                R.Range = 700;
            else if (R.Instance.Level == 3)
                R.Range = 850;

            var safeNet = menu.Item("R_Safe_Net", true).GetValue<Slider>().Value;

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                if (menu.Item("Dont_R" + target.BaseSkinName, true) != null)
                {
                    if (!menu.Item("Dont_R" + target.BaseSkinName, true).GetValue<bool>())
                    {
                        if (!(target.CountEnemiesInRange(1000) >= safeNet))
                        {
                            //if killable
                            if (menu.Item("R_On_Killable", true).GetValue<bool>())
                            {
                                if (GetComboDamage(target) > target.Health && Player.Distance(target.Position) < R.Range)
                                {
                                    R.Cast(target);
                                    return;
                                }
                            }

                            //if player is under turret
                            if (menu.Item("R_If_UnderTurret", true).GetValue<bool>())
                            {
                                if (Util.UnderAllyTurret() && Player.Distance(target) > 300f)
                                {
                                    R.Cast(target);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Cast_W(Obj_AI_Hero target)
        {
            if (target.HasBuff("urgotcorrosivedebuff", true))
                W.Cast();

            var hp = menu.Item("W_If_HP", true).GetValue<Slider>().Value;

            if (Player.HealthPercent <= hp)
                W.Cast();
        }

        private void Cast_Q(Obj_AI_Hero target, string source)
        {
            if (target.HasBuff("urgotcorrosivedebuff", true))
                Q2.Cast(target);
            else 
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Physical, HitChanceManager.GetQHitChance(source));
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q2.Range)).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Player.Distance(target) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //R
                if (Player.Distance(target) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target);
                    return;
                }
            }
        }


        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (Player.IsChannelingImportantSpell())
                return;
                
            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

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
                    Render.Circle.DrawCircle(Player.Position,R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}
