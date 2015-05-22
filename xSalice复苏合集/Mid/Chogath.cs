using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.Mid
{
    class Chogath : Champion
    {
        public Chogath()
        {
            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 950);
            SpellManager.W = new Spell(SpellSlot.W, 650);
            SpellManager.R = new Spell(SpellSlot.R, 175);

            SpellManager.Q.SetSkillshot(1200f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            SpellManager.W.SetSkillshot(.25f, (float)(30 * 0.5), float.MaxValue, false, SkillshotType.SkillshotCone);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("flashR", "闪现 R", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, true, false, false));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, true, false, false));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 60);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "使用 W", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(farm, "Farm", 50);
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("杂项", "Misc");
            {
                //aoe
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(true, true, false, false));
                miscMenu.AddItem(new MenuItem("Q_Gap_Closer", "使用Q防止突进", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("UseInt", "使用 Q/E 防止突进", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("smartKS", "智能抢人头", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("R_KS", "使用R抢人头", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("R_KS2", "使用闪现R抢人头", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("显示", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用所有", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "范围 Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "范围 W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "范围 R", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示整天连招伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(),
                menu.Item("UseWCombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(),
                menu.Item("UseWHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useR, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana(source))
                return;

            var itemTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (itemTarget != null)
            {
                var dmg = GetComboDamage(itemTarget);
                ItemManager.Target = itemTarget;

                //see if killable
                if (dmg > itemTarget.Health - 50)
                    ItemManager.KillableTarget = true;

                ItemManager.UseTargetted = true;
            }

            if(useQ && Q.IsReady())
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance(source));
            if(useW && W.IsReady())
                SpellCastManager.CastBasicSkillShot(W, W.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetWHitChance(source));

            if (useR && R.IsReady())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(GetRealRRange(x))))
                {
                    var dmg = GetComboDamage(enemy);
                    if (dmg > enemy.Health)
                        R.Cast(enemy);
                }
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("Farm"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ)
                SpellCastManager.CastBasicFarm(Q);

            if (useW && W.IsReady())
                SpellCastManager.CastBasicFarm(W);
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range)).OrderByDescending(GetComboDamage))
            {
                //Q + W
                if (Player.Distance(target.ServerPosition) <= W.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W) > target.Health && Q.IsReady() && W.IsReady())
                {
                    Q.Cast(target);
                    W.Cast(target);
                    return;
                }

                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //W
                if (Player.Distance(target.ServerPosition) <= W.Range && Player.GetSpellDamage(target, SpellSlot.W) > target.Health && W.IsReady())
                {
                    W.Cast(target);
                    return;
                }

                //R
                if (Player.Distance(target.ServerPosition) <= GetRealRRange(target) && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && menu.Item("R_KS", true).GetValue<bool>())
                {
                    R.Cast(target);
                    return;
                }

                //Flash + R
                if (Player.Distance(target.ServerPosition) <= GetRealRRange(target) + 375 && Player.Distance(target.ServerPosition) > GetRealRRange(target) + 25
                    && R.IsReady() && SummonerManager.Flash_Ready() && menu.Item("R_KS2", true).GetValue<bool>())
                {
                    if (Player.GetSpellDamage(target, SpellSlot.R) +
                        (SummonerManager.Ignite_Ready()
                            ? ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) - 20
                            : 0) > target.Health)
                    {
                        CastFlashR(target);
                        _lastFlash = Environment.TickCount;
                        return;
                    }
                }
            }
        }

        private float GetRealRRange(Obj_AI_Hero target)
        {
            return R.Range + Player.BoundingRadius + target.BoundingRadius;
        }

        private int _lastFlash;

        private void CastFlashR(Obj_AI_Hero target)
        {
            Game.PrintChat("flashing");

            if(SummonerManager.Flash_Ready())
                SummonerManager.UseFlash(target.ServerPosition);

            var dmg = GetComboDamage(target);
            ItemManager.Target = target;

            //see if killable
            if (dmg > target.Health - 50)
                ItemManager.KillableTarget = true;

            ItemManager.UseTargetted = true;

            Orbwalking.Orbwalk(target, target.ServerPosition);

            Utility.DelayAction.Add(25, () => R.Cast(target));
            if(R.IsReady())
                R.Cast(target);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            if (menu.Item("flashR", true).GetValue<KeyBind>().Active || Environment.TickCount - _lastFlash < 2500)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                var target = TargetSelector.GetSelectedTarget();

                if(target != null)
                    if(target.IsValidTarget(R.Range + 425 + target.BoundingRadius))
                        CastFlashR(TargetSelector.GetSelectedTarget());
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
            if (!menu.Item("Q_Gap_Closer", true).GetValue<bool>()) return;

            if (Q.IsReady() && gapcloser.Sender.Distance(Player.Position) < 500)
                Q.Cast(gapcloser.Sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < W.Range && W.IsReady())
            {
                W.Cast(unit);
                return;
            }

            if (Player.Distance(unit.Position) < Q.Range && Q.IsReady())
            {
                Q.Cast(unit);
            }

        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.MediumBlue : Color.Red);

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.MediumBlue : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.MediumBlue : Color.Red);
        }
    }
}
