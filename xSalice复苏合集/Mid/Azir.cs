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
    class Azir : Champion
    {
        public Azir()
        {
            LoadSpells();
            LoadMenu();
        }

        private static Obj_AI_Hero _insecTarget;
        private Vector3 _rVec;
        private int qRange = 900;

        private void LoadSpells()
        {
            //intalize spell
            SpellManager.Q = new Spell(SpellSlot.Q, 900);
            SpellManager.Q2 = new Spell(SpellSlot.Q, 2000);
            SpellManager.W = new Spell(SpellSlot.W, 450);
            SpellManager.W2 = new Spell(SpellSlot.W, 625);
            SpellManager.E = new Spell(SpellSlot.E, 2000);
            SpellManager.R = new Spell(SpellSlot.R, 450);

            SpellManager.Q.SetSkillshot(0, 80, 1600, false, SkillshotType.SkillshotCircle);
            SpellManager.Q2.SetSkillshot(0, 80, 1600, false, SkillshotType.SkillshotCircle);
            SpellManager.E.SetSkillshot(0.25f, 100, 1200, false, SkillshotType.SkillshotLine);
            SpellManager.R.SetSkillshot(0.5f, 700, 1400, false, SkillshotType.SkillshotLine);

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
                key.AddItem(new MenuItem("escape", "逃跑", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("insec", "切换攻击目标", true).SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("qeCombo", "Q->E 眩晕最近目标", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("qMulti", "使用Q只有存在2+沙兵", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Spell Menu
            var spell = new Menu("法术菜单", "Spell");
            {

                var qMenu = new Menu("Q菜单", "QSpell");
                {
                    qMenu.AddItem(new MenuItem("qOutRange", "Only Use When target out of range", true).SetValue(false));
                    spell.AddSubMenu(qMenu);
                }
                //W Menu
                var wMenu = new Menu("W菜单", "WSpell");
                {
                    wMenu.AddItem(new MenuItem("wAtk", "自动攻击敌人", true).SetValue(true));
                    spell.AddSubMenu(wMenu);
                }
                //E Menu
                var eMenu = new Menu("E菜单", "ESpell");
                {
                    eMenu.AddItem(new MenuItem("eKill", "如果可击杀连招才使用", true).SetValue(false));
                    eMenu.AddItem(new MenuItem("eKnock", "总是 Knockup/DMG", true).SetValue(false));
                    eMenu.AddItem(new MenuItem("eHP", "使用E自己血量 >", true).SetValue(new Slider(100)));
                    spell.AddSubMenu(eMenu);
                }
                //R Menu
                var rMenu = new Menu("R菜单", "RSpell");
                {
                    rMenu.AddItem(new MenuItem("rHP", "使用R自己血量 <", true).SetValue(new Slider(20)));
                    rMenu.AddItem(new MenuItem("rWall", "将敌人R入墙内", true).SetValue(true));
                    spell.AddSubMenu(rMenu);
                }
                menu.AddSubMenu(spell);
            }

            //Combo menu:
            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, false, false, false));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(false));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, false, false, false));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 60);
                menu.AddSubMenu(harass);
            }

            //killsteal
            var killSteal = new Menu("抢人头", "KillSteal");
            {
                killSteal.AddItem(new MenuItem("smartKS", "使用 智能抢人头模式", true).SetValue(true));
                killSteal.AddItem(new MenuItem("eKS", "使用 E KS", true).SetValue(false));
                killSteal.AddItem(new MenuItem("wqKS", "使用 WQ KS", true).SetValue(true));
                killSteal.AddItem(new MenuItem("qeKS", "使用 WQE KS", true).SetValue(false));
                killSteal.AddItem(new MenuItem("rKS", "使用 R KS", true).SetValue(true));
                menu.AddSubMenu(killSteal);
            }

            //farm menu
            var farm = new Menu("清线", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(false));
                farm.AddItem(new MenuItem("qFarm", "使用 Q 小兵数量", true).SetValue(new Slider(3, 0, 5)));
                ManaManager.AddManaManagertoMenu(farm, "Farm", 50);
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("杂项", "Misc");
            {
                misc.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, false, false, true));
                misc.AddItem(new MenuItem("UseInt", "使用 E 中断法术", true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "使用 R 防止突进", true).SetValue(true));
                misc.AddItem(new MenuItem("escapeDelay", "减少逃避延迟", true).SetValue(new Slider(0, 0, 300)));
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var draw = new Menu("显示", "Drawings");
            {
                draw.AddItem(new MenuItem("QRange", "Q 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("WRange", "W 范围", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("ERange", "E 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("RRange", "R 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示整套连招伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                draw.AddItem(drawComboDamageMenu);
                draw.AddItem(drawFill);
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

                menu.AddSubMenu(draw);
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
                customMenu.AddItem(myCust.AddToMenu("逃跑 启用: ", "escape"));
                customMenu.AddItem(myCust.AddToMenu("切换攻击目标 启用: ", "insec"));
                customMenu.AddItem(myCust.AddToMenu("使用Q只有存在2+沙兵 启用: ", "qMulti"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (soilderCount() > 0 || W.IsReady())
            {
                damage += AzirManager.GetAzirAaSandwarriorDamage(enemy);
            }

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

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
            if (source == "Harass" && !ManaManager.HasMana(source))
                return;

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var soilderTarget = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
            var dmg = GetComboDamage(soilderTarget);

            if (soilderTarget == null || qTarget == null)
                return;

            //R
            if (useR && R.IsReady() && ShouldR(qTarget) && Player.Distance(qTarget.Position) < R.Range)
                R.Cast(qTarget);

            //W
            if (useW && W.IsReady())
            {
                CastW(qTarget);
            }

            //Q
            if (useQ && Q.IsReady())
            {
                CastQ(qTarget, source);
                return;
            }

            //items
            if (source == "Combo")
            {
                ItemManager.Target = soilderTarget;

                //see if killable
                if (dmg > soilderTarget.Health - 50)
                    ItemManager.KillableTarget = true;

                ItemManager.UseTargetted = true;

            }

            //E
            if (useE && (E.IsReady() || ESpell.State == SpellState.Surpressed))
            {
                CastE(soilderTarget);
            }
        }

        private bool WallStun(Obj_AI_Hero target)
        {
            var pushedPos = R.GetPrediction(target).UnitPosition;

            if (Util.IsPassWall(Player.ServerPosition, pushedPos))
                return true;

            return false;
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1200) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    //R
                    if ((Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 20 && Player.Distance(target.Position) < R.Range && menu.Item("rKS", true).GetValue<bool>())
                    {
                        R.Cast(target);
                    }

                    if (soilderCount() < 1 && !W.IsReady())
                        return;

                    //WQ
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20 && menu.Item("wqKS", true).GetValue<bool>())
                    {
                        CastW(target);
                    }

                    //qe
                    if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20 && Player.Distance(target.Position) < Q.Range && menu.Item("qeKS", true).GetValue<bool>())
                    {
                        CastQe(target, "Null");
                    }

                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe)
                return;


            if (args.SData.Name == "AzirE" && (Q.IsReady() || QSpell.State == SpellState.Surpressed))
            {
                if (Utils.TickCount - E.LastCastAttemptT < 0)
                    Q2.Cast(Game.CursorPos);
            }
        }

        private void Escape()
        {
            Vector3 wVec = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * W.Range;

            if ((E.IsReady() || ESpell.State == SpellState.Surpressed))
            {
                if (soilderCount() < 1 && W.IsReady())
                    W.Cast(wVec);
                else if (soilderCount() < 1 && !W.IsReady())
                    return;

                if (GetNearestSoilderToMouse() == null)
                    return;

                var nearSlave = GetNearestSoilderToMouse();

                if (E.IsReady() || ESpell.State == SpellState.Surpressed)
                {
                    E.Cast(nearSlave.Position);
                    E.LastCastAttemptT = Utils.TickCount + 250;
                }
            }
            
        }

        private GameObject GetNearestSoilderToMouse()
        {
            var soilder = AzirManager.Soilders.ToList().OrderBy(x => Game.CursorPos.Distance(x.Position));

            if (soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private void CastQe(Obj_AI_Hero target, string source)
        {
            if (!target.IsValid)
                return;

            if (soilderCount() > 0)
            {
                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && E.IsReady())
                {
                    var slaves = AzirManager.Soilders.ToList();

                    foreach (var slave in slaves)
                    {
                        if (Player.Distance(target.Position) < qRange)
                        {
                            Q.UpdateSourcePosition(slave.Position, Player.ServerPosition);
                            var qPred = Q.GetPrediction(target);

                            if (Q.IsReady() && Player.Distance(target.Position) < qRange && qPred.Hitchance >= HitChanceManager.GetQHitChance(source))
                            {
                                var vec = target.ServerPosition - Player.ServerPosition;
                                var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                                Q.Cast(castBehind);
                                E.Cast(slave.Position);
                                return;

                            }
                        }
                    }
                }
            }
            else if (W.IsReady())
            {
                Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;

                var qPred = Util.GetP(wVec, Q, target, true);

                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && (E.IsReady() || ESpell.State == SpellState.Surpressed) && Player.Distance(target.Position) < qRange && qPred.Hitchance >= HitChanceManager.GetQHitChance(source))
                {
                    var vec = target.ServerPosition - Player.ServerPosition;
                    var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                    W.Cast(wVec);
                    Q.Cast(castBehind);
                    Utility.DelayAction.Add(1, () => E.Cast(getNearestSoilderToEnemy(target).Position));
                }
            }
        }

        private void Insec()
        {
            var target = _insecTarget;

            if (target == null)
                return;

            if (soilderCount() > 0)
            {
                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && E.IsReady())
                {
                    var slaves = AzirManager.Soilders.ToList();

                    foreach (var slave in slaves)
                    {
                        if (Player.Distance(target.Position) < qRange && slave.IsValid)
                        {
                            var qPred = Util.GetP(slave.Position, Q, target, true);
                            var vec = target.ServerPosition - Player.ServerPosition;
                            var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;
                            _rVec = qPred.CastPosition - Vector3.Normalize(vec) * 300;

                            if (Q.IsReady() && (E.IsReady() || ESpell.State == SpellState.Surpressed) && R.IsReady() && qPred.Hitchance >= HitChanceManager.GetQHitChance("Combo"))
                            {
                                Q.Cast(castBehind);
                                E.Cast(slave.Position);
                                E.LastCastAttemptT = Environment.TickCount;
                            }
                        }
                    }
                }
                if (R.IsReady())
                {
                    if (Player.Distance(target.Position) < 200 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        R.Cast(_rVec);
                    }
                }
            }
            else if (W.IsReady())
            {
                Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;

                var qPred = Util.GetP(wVec, Q, target, true);

                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && (E.IsReady() || ESpell.State == SpellState.Surpressed) && getNearestSoilderToEnemy(target) != null
                    && R.IsReady() && Player.Distance(target.Position) < qRange && qPred.Hitchance >= HitChanceManager.GetQHitChance("Combo"))
                {
                    var vec = target.ServerPosition - Player.ServerPosition;
                    var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;
                    _rVec = Player.Position;

                    W.Cast(wVec);
                    Q.Cast(castBehind);
                    E.Cast(getNearestSoilderToEnemy(target).Position);
                }
                if (R.IsReady())
                {
                    if (Player.Distance(target.Position) < 200 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        R.Cast(_rVec);
                    }
                }
            }
        }

        private void CastW(Obj_AI_Hero target)
        {
            if (target == null)
                return;

            if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) || Player.Distance(Prediction.GetPrediction(target, W.Delay).UnitPosition, true) < W2.RangeSqr)
            {
                var vec = Player.Distance(target, true) > W.RangeSqr
                    ? Player.Position.To2D().Extend(target.Position.To2D(), W.Range)
                    : Prediction.GetPrediction(target, W.Delay).UnitPosition.To2D();
                W.Cast(vec);
            }
        }

        protected override void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
                args.Process = !(!(Q.IsReady() || QSpell.State == SpellState.Surpressed) && W.IsReady() && !AzirManager.InSoldierAttackRange(args.Target));
        }

        private void CastQ(Obj_AI_Hero target, string source)
        {
            if (soilderCount() < 1)
                return;

            var slaves = AzirManager.Soilders.ToList();

            foreach (var slave in slaves)
            {
                if (Player.Distance(target.Position) < Q.Range && ShouldQ(target, slave))
                {

                    Q.UpdateSourcePosition(slave.Position, Player.ServerPosition);
                    var qPred = Q.GetPrediction(target);

                    if (Q.IsReady() && Player.Distance(target.Position) < qRange && qPred.Hitchance >= HitChanceManager.GetQHitChance(source))
                    {
                        Q.Cast(qPred.CastPosition);
                        return;
                    }
                }
            }
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (soilderCount() < 1)
                return;

            var slaves = AzirManager.Soilders.ToList();

            foreach (var slave in slaves)
            {
                if (target != null && Player.Distance(slave.Position) < E.Range)
                {
                    var ePred = E.GetPrediction(target);
                    Object[] obj = Util.VectorPointProjectionOnLineSegment(Player.ServerPosition.To2D(), slave.Position.To2D(), ePred.UnitPosition.To2D());
                    var isOnseg = (bool)obj[2];
                    var pointLine = (Vector2)obj[1];

                    if (E.IsReady() && isOnseg && pointLine.Distance(ePred.UnitPosition.To2D()) < E.Width && ShouldE(target))
                    {
                        E.Cast(slave.Position);
                        return;
                    }
                }
            }
        }

        private bool ShouldQ(Obj_AI_Hero target, GameObject slave)
        {

            if (soilderCount() < 2 && menu.Item("qMulti", true).GetValue<KeyBind>().Active)
                return false;

            if (!menu.Item("qOutRange", true).GetValue<bool>())
                return true;

            if (target.Distance(slave.Position) > 390)
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health + 10)
                return true;


            return false;
        }
        private bool ShouldE(Obj_AI_Hero target)
        {
            if (menu.Item("eKnock", true).GetValue<bool>())
                return true;

            if (menu.Item("eKill", true).GetValue<bool>() && GetComboDamage(target) > target.Health + 15)
                return true;

            if (menu.Item("eKS", true).GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.E) > target.Health + 10)
                return true;

            //hp 
            var hp = menu.Item("eHP", true).GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (hpPercent > hp)
                return true;

            return false;
        }

        private bool ShouldR(Obj_AI_Hero target)
        {
            if (Player.GetSpellDamage(target, SpellSlot.R) > target.Health - 150)
                return true;

            var hp = menu.Item("rHP", true).GetValue<Slider>().Value;
            if (Player.HealthPercent < hp)
                return true;

            if (WallStun(target) && GetComboDamage(target) > target.Health / 2 && menu.Item("rWall", true).GetValue<bool>())
            {
                return true;
            }

            return false;
        }

        private void AutoAtk()
        {
            if (soilderCount() < 1)
                return;

            var soilderTarget = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

            if (soilderTarget == null)
                return;

            AttackTarget(soilderTarget);
        }

        private int soilderCount()
        {
            return AzirManager.Soilders.Count();
        }


        private void AttackTarget(Obj_AI_Hero target)
        {
            if (soilderCount() < 1)
                return;

            var tar = getNearestSoilderToEnemy(target);
            if (tar != null && Player.Distance(tar.Position) < 800)
            {
                if (target != null && target.Distance(tar.Position) <= 350)
                {
                    Orbwalking.Orbwalk(target, Game.CursorPos);
                }
            }

        }

        private GameObject getNearestSoilderToEnemy(Obj_AI_Base target)
        {
            var soilder = AzirManager.Soilders.ToList().OrderBy(x => target.Distance(x.Position));

            if (soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("Farm"))
                return;

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var min = menu.Item("qFarm", true).GetValue<Slider>().Value;


            if (useQ && (Q.IsReady() || QSpell.State == SpellState.Surpressed))
            {
                int hit;
                if (soilderCount() > 0)
                {
                    var slaves = AzirManager.Soilders.ToList();
                    foreach (var slave in slaves)
                    {
                        foreach (var enemy in allMinionsQ)
                        {
                            hit = 0;
                            Q.UpdateSourcePosition(slave.Position, Player.ServerPosition);
                            var prediction = Q.GetPrediction(enemy);

                            if (Q.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                            {
                                hit += allMinionsQ.Count(enemy2 => enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady());
                                if (hit >= min)
                                {
                                    if (Q.IsReady())
                                    {
                                        Q.Cast(prediction.CastPosition);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (W.IsReady())
                {
                    var wpred = W.GetCircularFarmLocation(allMinionsW);
                    if (wpred.MinionsHit > 0)
                        W.Cast(wpred.Position);

                    foreach (var enemy in allMinionsQ)
                    {
                        hit = 0;
                        Q.UpdateSourcePosition(Player.Position, Player.ServerPosition);
                        var prediction = Q.GetPrediction(enemy);

                        if (Q.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                        {
                            hit += allMinionsQ.Count(enemy2 => enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady());
                            if (hit >= min)
                            {
                                if (Q.IsReady())
                                {
                                    Q.Cast(prediction.CastPosition);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            if (menu.Item("escape", true).GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                Escape();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else if (menu.Item("insec", true).GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);

                _insecTarget = TargetSelector.GetSelectedTarget();
                if (_insecTarget != null)
                    Insec();
            }
            else if (menu.Item("qeCombo", true).GetValue<KeyBind>().Active)
            {
                var soilderTarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

                Orbwalking.Orbwalk(null, Game.CursorPos);
                CastQe(soilderTarget, "Combo");
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("wAtk", true).GetValue<bool>())
                    AutoAtk();
            }

        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
            if (menu.Item("QRange", true).GetValue<Circle>().Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.LightBlue);
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (R.IsReady() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast(gapcloser.Sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < R.Range && R.IsReady())
            {
                R.Cast(unit);
            }
        }

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            AzirManager.Obj_OnCreate(sender, args);
        }

        protected override void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            AzirManager.OnDelete(sender, args);
        }
    }
}
