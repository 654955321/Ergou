using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace xSaliceReligionAIO.Champions
{
    class Corki : Champion
    {
        public Corki()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 825);
            Q.SetSkillshot(.3f, 250, 1225, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 800);

            E = new Spell(SpellSlot.E, 600);
            E.SetSkillshot(.1f, (float)(45 * Math.PI / 180), 1500, false, SkillshotType.SkillshotCone);

            R = new Spell(SpellSlot.R, 1500);
            R.SetSkillshot(.2f, 40, 1500, true, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!",true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!",true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q",true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E",true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R",true).SetValue(true));
                combo.AddItem(new MenuItem("Always_Use", "敌人离开走A范围后才使用技能", true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q/R 命中率",true).SetValue(new Slider(3, 1, 3)));
                combo.AddItem(new MenuItem("ComboR_Limit", "保持 R 库存",true).SetValue(new Slider(0, 0, 7)));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q",true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E",true).SetValue(false));
                harass.AddItem(new MenuItem("UseRHarass", "使用 R",true).SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "Q/R 命中率",true).SetValue(new Slider(3, 1, 3)));
                harass.AddItem(new MenuItem("HarassR_Limit", "保持 R 库存", true).SetValue(new Slider(5, 0, 7)));
                AddManaManagertoMenu(harass, "Harass", 50);
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q",true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E",true).SetValue(false));
                farm.AddItem(new MenuItem("UseRFarm", "使用 R",true).SetValue(true));
                farm.AddItem(new MenuItem("LaneClearR_Limit", "保持 R 库存", true).SetValue(new Slider(5, 0, 7)));
                AddManaManagertoMenu(farm, "LaneClear", 50);
                menu.AddSubMenu(farm);
            }

            var drawMenu = new Menu("范围", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用 所有",true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Q 范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "W 范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "E 范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "R 范围",true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示组合填充伤害",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
        }

         private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 3);
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseEHarass", true).GetValue<bool>(), menu.Item("UseRHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            //items
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

            var target = TargetSelector.GetTarget(550, TargetSelector.DamageType.Magical);
            if ((target != null && source == "Combo") && menu.Item("Always_Use", true).GetValue<bool>())
                return;

            if(useR && R.IsReady())
                Cast_R(source);
            if(useQ && Q.IsReady())
                CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, GetHitchance(source));
            if(useE && E.IsReady())
                CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Physical, HitChance.Low);
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            var target = (Obj_AI_Base)mytarget;

            if (!menu.Item("ComboActive", true).GetValue<KeyBind>().Active || !unit.IsMe || !(target is Obj_AI_Hero))
                return;

            if (menu.Item("UseRCombo", true).GetValue<bool>() && R.IsReady())
                R.Cast(target, packets());
            if (menu.Item("UseQCombo", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(target, packets());
            if (menu.Item("UseECombo", true).GetValue<bool>() && E.IsReady())
                E.Cast(packets());
            
        }

        private void Farm()
        {
            if (!HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();
            var useR = menu.Item("UseRFarm", true).GetValue<bool>();

            if(useQ)
                CastBasicFarm(Q);
            if(useR)
                Cast_R("Farm");
            if (useE)
            {
                int allMinionECount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All,MinionTeam.NotAlly).Count;
                if (allMinionECount > 1)
                    E.Cast(packets());
            }
        }

        private void Cast_R(string mode)
        {
            var range = Player.HasBuff("CorkiMissileBarrageCounterBig") ? 1500 : 1300;

            if (mode == "Combo" && menu.Item("ComboR_Limit", true).GetValue<Slider>().Value < Player.Spellbook.GetSpell(SpellSlot.R).Ammo)
                CastBasicSkillShot(R, range, TargetSelector.DamageType.Magical, GetHitchance(mode));
            else if (mode == "Harass" && menu.Item("HarassR_Limit", true).GetValue<Slider>().Value < Player.Spellbook.GetSpell(SpellSlot.R).Ammo)
                CastBasicSkillShot(R, range, TargetSelector.DamageType.Magical, GetHitchance(mode));
            else if (mode == "Farm" && menu.Item("LaneClearR_Limit", true).GetValue<Slider>().Value < Player.Spellbook.GetSpell(SpellSlot.R).Ammo)
                CastBasicFarm(R);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

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
                    Render.Circle.DrawCircle(Player.Position, Player.HasBuff("CorkiMissileBarrageCounterBig") ? R.Range : 1300, R.IsReady() ? Color.Green : Color.Red);
        }

    }
}
