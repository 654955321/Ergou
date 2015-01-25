using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Karthus : Champion
    {
        public Karthus()
        {
            QMana = new []{ 20, 20 , 26 , 32 , 38 , 44 };
            WMana = new []{ 100, 100, 100, 100, 100, 100 };
            EMana = new[] { 30, 30, 42, 54, 66, 78 };
            RMana = new []{ 150, 150 , 175 , 200 };

            LoadSpell();
            LoadMenu();
        }

        private const int QWidth = 200;
        private static int _lastPing;

        private void LoadSpell()
        {
            Q = new Spell(SpellSlot.Q, 875);
            Q2 = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 520);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(.6f, 50f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Q2.SetSkillshot(.6f, 190f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 50f, 1600f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(3f, float.MaxValue, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //Keys
            var keys = new Menu("按 键", "Keys");
            {
                keys.AddItem(new MenuItem("ComboActive", "连 招!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("HarassActive", "骚 扰!", true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("HarassActiveT", "骚 扰 (自 动)!", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                keys.AddItem(new MenuItem("wTar", "选中后 使用W", true).SetValue(new KeyBind("W".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("lastHitQ", "Q 最后一下", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("LaneClearActive", "打 钱!", true).SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                menu.AddSubMenu(keys);
            }

            //Spell Menu
            var spells = new Menu("法术 设置", "Spell"); { 
                //Q Menu
                var qMenu = new Menu("Q 设置", "QSpell");
                { 
                    //qMenu.AddItem(new MenuItem("qAA", "Auto Q AAing target", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                    qMenu.AddItem(new MenuItem("qImmo", "自动 Q (固定)", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("qDash", "自动 Q (快速)", true).SetValue(true));
                    spells.AddSubMenu(qMenu);
                }

                //W
                var wMenu = new Menu("W 设置", "WSpell");
                {
                    wMenu.AddItem(new MenuItem("wTower", "自动 W 敌人丨在塔下", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("wIfMana", "假如有蓝丨只Q", true).SetValue(true));
                    spells.AddSubMenu(wMenu);
                }

                //E
                var eMenu = new Menu("E 设置", "ESpell"); { 
                    eMenu.AddItem(new MenuItem("eManaCombo", "连招丨最小蓝量", true).SetValue(new Slider(10)));
                    eMenu.AddItem(new MenuItem("eManaHarass", "骚扰丨最小蓝量", true).SetValue(new Slider(70)));
                    spells.AddSubMenu(eMenu);
                }

                //R
                var rMenu = new Menu("R 设置", "RSpell");
                {
                    rMenu.AddItem(new MenuItem("rPing", "本地提示:本地可击杀的敌人", true).SetValue(true));
                    spells.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spells);
            }

            //Combo menu:
            var combo = new Menu("连 招", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "寻找 重点目标", true).SetValue(true));
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q 命中几率", true).SetValue(new Slider(3, 1, 4)));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("骚 扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "Q 命中几率", true).SetValue(new Slider(3, 1, 4)));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                harass.AddItem(new MenuItem("minMana", "最小 蓝量 >", true).SetValue(new Slider(60)));
                menu.AddSubMenu(harass);
            }

            //Farming menu:
            var farm = new Menu("打 钱", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E", true).SetValue(false));
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("杂 项", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "使用 E 丨打断法术", true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "使用 E 丨防突进", true).SetValue(true));
                misc.AddItem(new MenuItem("packet", "使用 封包", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "使用 智能击杀系统", true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawMenu = new Menu("范 围", "Drawings");
            { 
                drawMenu.AddItem(new MenuItem("QRange", "Q 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("WRange", "W 范围", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("ERange", "E 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("RRange", "R 范围", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("drawUlt", "大招 击杀范围", true).SetValue(true));
                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示 连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示 充能后伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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

        /*
        public override void ObjAiHeroOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsEnemy || !menu.Item("qAA", true).GetValue<KeyBind>().Active || (args.Target is Obj_AI_Minion))
                return;

            if (sender.IsValidTarget(Q.Range))
                Q.Cast(sender.ServerPosition, packets());
        }*/

        /*static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            GamePacket g = new GamePacket(args.PacketData);
            if (g.Header != 0xFE)
                return;

            if (menu.Item("qAA", true).GetValue<KeyBind>().Active)
            {
                if (Packet.MultiPacket.OnAttack.Decoded(args.PacketData).Type == Packet.AttackTypePacket.TargetedAA)
                {
                    g.Position = 1;
                    var k = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(g.ReadInteger());
                    if (k is Obj_AI_Hero && k.IsEnemy)
                    {
                        if (Vector3.Distance(k.Position, Player.Position) <= Q.Range)
                        {
                            Q.Cast(k.Position, packets());
                        }
                    }
                }
            }
        }*/

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;


            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E) * 2;

            if (R.IsReady())
                damage += GetUltDmg((Obj_AI_Hero)enemy);

            damage = ActiveItems.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, string source)
        {
            var range = W.IsReady() ? W.Range : Q.Range;
            var focusSelected = menu.Item("selected", true).GetValue<bool>();
            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
            if (TargetSelector.GetSelectedTarget() != null)
                if (focusSelected && TargetSelector.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    target = TargetSelector.GetSelectedTarget();

            float dmg = GetComboDamage(target);
            var minManaHarass = menu.Item("minMana", true).GetValue<Slider>().Value;
            Obj_AI_Hero eTar = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (source == "Harass" && Player.ManaPercentage() < minManaHarass)
                return;

            if (target == null)
                return;

            //W
            if (useW && W.IsReady() && Player.Distance(target) <= W.Range && ShouldW() &&
                W.GetPrediction(target).Hitchance >= HitChance.High)
            {
                W.Cast(target, packets());
            }

            //Q
            if (useQ && Q.IsReady())
            {
                var qPred = Q.GetPrediction(target, true);

                if (qPred.Hitchance >= GetHitchance(source))
                    Q.Cast(qPred.CastPosition, true);

            }

            //E
            if (useE && E.IsReady() && Player.Distance(eTar) < E.Range && ESpell.ToggleState == 1 && HasManaForE(source) && Environment.TickCount - E.LastCastAttemptT > 500)
            {
                E.Cast(packets());
                E.LastCastAttemptT = Environment.TickCount;
            }

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
        }

        private bool ShouldW()
        {
            if (menu.Item("wIfMana", true).GetValue<bool>() && ManaCheck2())
                return true;

            return false;
        }

        private float GetUltDmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            dmg += Player.GetSpellDamage(target, SpellSlot.R);

            dmg -= target.HPRegenRate * 3.25;

            if (Items.HasItem(3155, target))
            {
                dmg = dmg - 250;
            }

            if (Items.HasItem(3156, target))
            {
                dmg = dmg - 400;
            }

            return (float)dmg;
        }

        private void DrawEnemyKillable()
        {
            int kill = 0;

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(
                            x => x.IsValidTarget()))
            {
                if (GetUltDmg(enemy) > enemy.Health - 30)
                {
                    if (menu.Item("rPing", true).GetValue<bool>() && Environment.TickCount - _lastPing > 15000)
                    {
                        Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(enemy.ServerPosition.X,enemy.ServerPosition.Y, 0, 0, Packet.PingType.Normal)).Process();
                        _lastPing = Environment.TickCount;

                    }
                    kill++;
                }
            }

            if (kill > 0)
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(wts[0] - 100, wts[1], Color.Red, "RAWR Killable with R: " + kill);
            }
            else
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(wts[0] - 100, wts[1], Color.White, "RAWR Killable with R: " + kill);
            }
        }


        private bool HasManaForE(string source)
        {
            var eManaCombo = menu.Item("eManaCombo", true).GetValue<Slider>().Value;
            var eManaHarass = menu.Item("eManaHarass", true).GetValue<Slider>().Value;

            if (source == "Combo" && Player.ManaPercentage() > eManaCombo)
                return true;

            if (source == "Harass" && Player.ManaPercentage() > eManaHarass)
                return true;

            return false;
        }

        private void AutoQ()
        {
            var qDashing = menu.Item("qImmo", true).GetValue<bool>();
            var qImmo = menu.Item("qDash", true).GetValue<bool>();

            if (!Q.IsReady())
                return;

            if (!qDashing && !qImmo)
                return;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range)))
            {
                if ((Q.GetPrediction(target).Hitchance == HitChance.Immobile || IsStunned(target)) && qImmo && Player.Distance(target) < Q.Range)
                {
                    Q.Cast(target, true);
                    return;
                }

                if (Q.GetPrediction(target).Hitchance == HitChance.Dashing && qDashing && Player.Distance(target) < Q.Range)
                {
                    Q.Cast(target, true);
                }
            }
        }

        private void CheckUnderTower()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.Distance(x) < W.Range && x.IsValidTarget(W.Range) && !x.IsDead && x.IsEnemy && x.IsVisible))
            {
                if (enemy != null)
                {
                    foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret != null && turret.IsValid && turret.IsAlly && turret.Health > 0)
                        {
                            if (Vector2.Distance(enemy.Position.To2D(), turret.Position.To2D()) < 750 && W.IsReady())
                            {
                                var vec = enemy.ServerPosition +
                                          Vector3.Normalize(enemy.ServerPosition - Player.ServerPosition) * 100;

                                W.Cast(vec, packets());
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range)))
            {
                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 30)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target, true);
                        return;
                    }
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range && ESpell.ToggleState == 1 &&
                    (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 30)
                {
                    if (E.IsReady())
                    {
                        E.Cast(packets());
                        return;
                    }
                }

            }
        }

        private void CheckEState()
        {
            if (ESpell.ToggleState == 1)
                return;

            var target = ObjectManager.Get<Obj_AI_Hero>().Count(x => x.IsValidTarget(E.Range));

            //return if Target in range
            if (target > 0)
                return;

            //check if around minion
            if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
            {
                var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

                if (allMinionsE.Count > 0)
                    return;
            }

            if (E.IsReady() && ESpell.ToggleState != 1)
                E.Cast();
        }


        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsChannelingImportantSpell())
                return;

            SmartKs();

            AutoQ();

            CheckEState();

            if (menu.Item("wTower", true).GetValue<bool>())
                CheckUnderTower();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {

                if (menu.Item("lastHitQ", true).GetValue<KeyBind>().Active)
                    LastHitQ();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (menu.Item("wTar", true).GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero target = null;

                if (TargetSelector.GetSelectedTarget() != null)
                    target = TargetSelector.GetSelectedTarget();

                if (target != null && target.IsEnemy && target.Type == GameObjectType.obj_AI_Hero)
                {
                    if (W.GetPrediction(target).Hitchance >= HitChance.High)
                        W.Cast(target, packets());
                }
            }
        }

        private void LastHitQ()
        {
            if (!Q.IsReady())
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + QWidth, MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionsQ.Count > 0)
            {
                foreach (var minion in allMinionsQ)
                {
                    var health = HealthPrediction.GetHealthPrediction(minion, 700);

                    var qPred = Q2.GetCircularFarmLocation(allMinionsQ);

                    if (qPred.MinionsHit == 1)
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q) - 15 > health)
                            Q2.Cast(minion, packets());
                    }
                    else
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q, 1) - 15 > health)
                            Q2.Cast(minion, packets());
                    }
                }
            }
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                CastBasicFarm(Q2);
            }

            if (useE && allMinionsE.Count > 0 && E.IsReady() && ESpell.ToggleState == 1)
            {
                MinionManager.FarmLocation ePos = E.GetCircularFarmLocation(allMinionsE);

                if (ePos.MinionsHit > 1)
                    E.Cast(ePos.Position, packets());
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range",true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            if (R.IsReady() && menu.Item("drawUlt", true).GetValue<bool>())
                DrawEnemyKillable();
        }


        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range))
                W.Cast(gapcloser.Sender, packets());
        }
    }
}
