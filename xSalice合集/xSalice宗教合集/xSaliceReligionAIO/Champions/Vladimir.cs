using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Vladimir : Champion
    {
        public Vladimir()
        {
            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 575);
            R = new Spell(SpellSlot.R, 700);

            R.SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "补刀!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("StackE", "堆叠E (自动)!", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                combo.AddItem(new MenuItem("R_Killable", "连招可击杀使用R", true).SetValue(true));
                combo.AddItem(new MenuItem("useR_Hit", "使用 R 击杀", true).SetValue(new Slider(3, 1, 5)));
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
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                miscMenu.AddItem(new MenuItem("W_Gap_Closer", "使用 W 防止突进", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("smartKS", "使用 智能抢人头", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("R_KS", "Use 使用 R 击杀", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("Q_Before_E", "在E后使用Q", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("范围", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用 所有", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Q 范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "E 范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "R 范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R_Killable", "显示 R 击杀提示", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示组合填充伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);
                comboDamage += comboDamage * 1.12;
            }
            else if (target.HasBuff("vladimirhemoplaguedebuff", true))
            {
                comboDamage += comboDamage * 1.12;
            }

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useE, bool useR, string source)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            
            if (target == null)
                return;

            var dmg = GetComboDamage(target);

            if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                CastR(target, dmg);

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }

            if (menu.Item("Q_Before_E", true).GetValue<bool>())
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    Q.CastOnUnit(target, packets());

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                    E.Cast(packets());
            }
            else
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                    E.Cast(packets());

                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    Q.CastOnUnit(target, packets());
            }
        }

        private void CastR(Obj_AI_Hero target, float dmg)
        {
            R.UpdateSourcePosition();

            RMec();

            if (menu.Item("R_Killable", true).GetValue<bool>())
            {
                var pred = R.GetPrediction(target);
                if (dmg > target.Health && pred.Hitchance >= HitChance.High)
                    R.Cast(target);
            }
        }

        private void RMec()
        {
            var minHit = menu.Item("useR_Hit", true).GetValue<Slider>().Value;

            if (!R.IsReady() && minHit == 0)
                return;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)))
            {
                var pred = R.GetPrediction(target, true);

                if (pred.Hitchance >= HitChance.High && pred.AoeTargetsHitCount >= minHit)
                    R.Cast(target);
            }
        }

        private void LastHit()
        {
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        Q.CastOnUnit(minion, packets());
                        return;
                    }
                }
            }
        }

        private void Farm()
        {
            var rangedMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                LastHit();

            if (useE && E.IsReady())
            {
                if (rangedMinionsE.Count > 1)
                    E.Cast(packets());

            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(1300)).OrderByDescending(GetComboDamage))
            {
                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E)  > target.Health && Q.IsReady() && E.IsReady())
                {
                    E.Cast(packets());
                    Q.Cast(target, packets());
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target, packets());
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(packets());
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && menu.Item("R_KS", true).GetValue<bool>())
                {
                    R.Cast(target);
                    return;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {

            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (IsRecalling())
                return;

            if (menu.Item("StackE", true).GetValue<KeyBind>().Active)
            {
                if (E.IsReady() && Environment.TickCount - E.LastCastAttemptT >= 9900)
                    E.Cast(packets());
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe)
                return;
            
            if (args.SData.Name == "VladimirTidesofBlood")
            {
                E.LastCastAttemptT = Environment.TickCount + 250;
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("W_Gap_Closer", true).GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.Distance(Player) < 300)
                W.Cast(packets());
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}
