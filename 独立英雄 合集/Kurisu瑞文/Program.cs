using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;

namespace KurisuRiven
{
    internal class Program
    {
        private static Menu Config;
        private static Obj_AI_Hero Me = ObjectManager.Player;
        private static Obj_AI_Hero Maintarget;
        private static Orbwalking.Orbwalker Orbwalker;

        static void Main(string[] args)
        {
            Console.WriteLine("Riven injected...");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Me.ChampionName != "Riven")
            {
                return;
            }

            // load spells
            w = new Spell(SpellSlot.W, 250f);
            e = new Spell(SpellSlot.E, 250f);

            q = new Spell(SpellSlot.Q, 260f);
            q.SetSkillshot(0.25f, 100f, 1400f, false, SkillshotType.SkillshotCircle);

            r = new Spell(SpellSlot.R, 900);
            r.SetSkillshot(0.25f, 300f, 120f, false, SkillshotType.SkillshotCone);

            // load menu
            Menu();

            // call events
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

            // moar events xP
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            // print chat -- load success
            Game.PrintChat("<font color=\"#7CFC00\"><b>KurisuRivenTest:</b></font> Loaded");
        }

        #region Riven Menu
        private static void Menu()
        {
            Config = new Menu("花边汉化-K瑞文", "kurisuriven", true);

            var TsMenu = new Menu("目标 选择", "selector");
            TargetSelector.AddToMenu(TsMenu);
            Config.AddSubMenu(TsMenu);

            var OwMenu = new Menu("走 砍", "Orbwalk");
            Orbwalker = new Orbwalking.Orbwalker(OwMenu);
            OwMenu.AddItem(new MenuItem("fleemode", "Flee")).SetValue(new KeyBind(65, KeyBindType.Press));
            Config.AddSubMenu(OwMenu);

            var DrMenu = new Menu("范 围", "drawings");
            DrMenu.AddItem(new MenuItem("drawrange", "W 范围")).SetValue(true);
            DrMenu.AddItem(new MenuItem("drawpassive", "显示 被动")).SetValue(true);
            DrMenu.AddItem(new MenuItem("drawengage", "显示 交战范围")).SetValue(true);
            DrMenu.AddItem(new MenuItem("drawkill", "显示 可击杀")).SetValue(true);
            DrMenu.AddItem(new MenuItem("drawtarg", "显示 目标 线圈")).SetValue(true);
            Config.AddSubMenu(DrMenu);

            var CMenu = new Menu("连 招", "combo");
            CMenu.AddItem(new MenuItem("engage", "模式:"))
                .SetValue(new StringList(new[] { "E->R->提亚玛特->W", "E->提亚玛特->R->W" }));
            Config.AddSubMenu(CMenu);

            var SMenu = new Menu("技 能", "Spells");

            var menuQ = new Menu("Q 设置", "cleave");
            menuQ.AddItem(new MenuItem("usecomboq", "连招时丨使用Q")).SetValue(true);
            menuQ.AddItem(new MenuItem("usejungleq", "清野时丨使用W")).SetValue(true);
            menuQ.AddItem(new MenuItem("uselaneq", "清线时丨使用W")).SetValue(true);
            menuQ.AddItem(new MenuItem("qint", "打断时丨使用W")).SetValue(true);
            menuQ.AddItem(new MenuItem("prediction", "移动 预判")).SetValue(true);
            menuQ.AddItem(new MenuItem("keepq", "保持 激活状态")).SetValue(true);
            SMenu.AddSubMenu(menuQ);

            var menuW = new Menu("W 设置", "kiburst");
            menuW.AddItem(new MenuItem("usecombow", "连招时丨使用W")).SetValue(true);
            menuW.AddItem(new MenuItem("usejunglew", "清野时丨使用W")).SetValue(true);
            menuW.AddItem(new MenuItem("uselanew", "清线时丨使用W")).SetValue(true);
            menuW.AddItem(new MenuItem("antigap", "突进时丨使用W")).SetValue(true);
            menuW.AddItem(new MenuItem("wint", "打断时丨使用W")).SetValue(true);
            SMenu.AddSubMenu(menuW);

            var menuE = new Menu("E 设置", "valor");
            menuE.AddItem(new MenuItem("usecomboe", "连招时丨使用E")).SetValue(true);
            menuE.AddItem(new MenuItem("usejunglee", "清野时丨使用E")).SetValue(true);
            menuE.AddItem(new MenuItem("uselanee", "清线时丨使用E")).SetValue(true);
            menuE.AddItem(new MenuItem("vhealth", "血量 = %")).SetValue(new Slider(40, 1));
            SMenu.AddSubMenu(menuE);

            var menuR = new Menu("R 设置", "blade");
            menuR.AddItem(new MenuItem("user", "连招时 使用R")).SetValue(true);
            menuR.AddItem(new MenuItem("usews", "使用 疾风斩")).SetValue(true);
            SMenu.AddSubMenu(menuR);

            Config.AddSubMenu(SMenu);



            var MMenu = new Menu("杂 项", "misc");
            MMenu.AddItem(new MenuItem("forceaa", "强行 清线")).SetValue(false);
            Config.AddSubMenu(MMenu);

            var DMenu = new Menu("调 试", "debug");
            DMenu.AddItem(new MenuItem("debugtrue", "调试 正确范围")).SetValue(false);
            DMenu.AddItem(new MenuItem("debugdmg", "调试 连招伤害")).SetValue(false);
            Config.AddSubMenu(DMenu);

			var donate = new Menu("捐赠 信息", "feelo");
            donate.AddItem(new MenuItem("donate", "捐赠给Kurisu作者"));
            donate.AddItem(new MenuItem("donate2", "帐号：xrobinsong@gmail.com"));
            donate.AddItem(new MenuItem("donate3", "英文本是没有这个信息,这是我自己加上去的"));
			donate.AddItem(new MenuItem("donate4", "毕竟作者写本不易,捐赠支持也好!"));

            Config.AddToMainMenu();
        }

        #endregion

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (unit.IsValidTarget(q.Range))
            {
                if (q.IsReady() && cleavecount == 2 && Config.Item("qint").GetValue<bool>())
                    q.Cast(unit.ServerPosition);
            }

            if (unit.IsValidTarget(wrange))
            {
                if (w.IsReady() && Config.Item("wint").GetValue<bool>())
                    w.Cast();
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("antigap").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(wrange) && w.IsReady())
                {
                    w.Cast();
                }
            }
        }

        #region Damage Calc
        private static void CheckDamage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var ad = Me.GetAutoAttackDamage(target);
            var ignite = Me.GetSpellSlot("summonerdot");

            var tmt = Items.HasItem(3077) && Items.CanUseItem(3077)
                ? Me.GetItemDamage(target, Damage.DamageItems.Tiamat)
                : 0;

            var hyd = Items.HasItem(3074) && Items.CanUseItem(3074)
                ? Me.GetItemDamage(target, Damage.DamageItems.Hydra)
                : 0;

            var bwc = Items.HasItem(3144) && Items.CanUseItem(3144) ?
                Me.GetItemDamage(target, Damage.DamageItems.Bilgewater)
                : 0;

            var brk = Items.HasItem(3153) && Items.CanUseItem(3153)
                ? Me.GetItemDamage(target, Damage.DamageItems.Botrk)
                : 0;

            rr = Me.GetSpellDamage(target, SpellSlot.R) - 20;
            ra = ad + (ad * (runicpassive[Me.Level] / 100) * runiccount);
            rq = q.IsReady() ? DamageQ(target) : 0;

            rw = w.IsReady()
                ? Me.GetSpellDamage(target, SpellSlot.W)
                : 0;

            ri = Me.Spellbook.CanUseSpell(ignite) == SpellState.Ready
                ? Me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)
                : 0;

            ritems = tmt + hyd + bwc + brk;

            ua = r.IsReady()
                ? ra +
                  Me.CalcDamage(target, Damage.DamageType.Physical,
                      Me.BaseAttackDamage + Me.FlatPhysicalDamageMod * 0.2)
                : ua;

            uq = r.IsReady()
                ? rq +
                  Me.CalcDamage(target, Damage.DamageType.Physical,
                      Me.BaseAttackDamage + Me.FlatPhysicalDamageMod * 0.2 * 0.7)
                : uq;

            uw = r.IsReady()
                ? rw +
                  Me.CalcDamage(target, Damage.DamageType.Physical,
                      Me.BaseAttackDamage + Me.FlatPhysicalDamageMod * 0.2 * 1)
                : uw;

            rr = r.IsReady()
                ? Me.GetSpellDamage(target, SpellSlot.R)
                : 0;
        }

        private static float DamageQ(Obj_AI_Base target)
        {
            double dmg = 0;
            if (q.IsReady())
            {
                dmg += Me.CalcDamage(target, Damage.DamageType.Physical,
                    -10 + (q.Level * 20) +
                    (0.35 + (q.Level * 0.05)) * (Me.FlatPhysicalDamageMod + Me.BaseAttackDamage));
            }

            return (float)dmg;
        }

        #endregion

        #region Drawings
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Me.IsDead)
            {
                //var wts = Drawing.WorldToScreen(Me.Position);

                if (Config.Item("drawrange").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, wrange, System.Drawing.Color.White, 3);

                if (Config.Item("drawengage").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, Me.AttackRange + e.Range + 10, System.Drawing.Color.White, 3);

                if (Maintarget.IsValidTarget(900) && Config.Item("drawtarg").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(
                        Maintarget.Position, Maintarget.BoundingRadius - 50, System.Drawing.Color.Yellow, 6);
                }

                if (Config.Item("debugtrue").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Me.Position, truerange, System.Drawing.Color.Yellow, 3);
                }

                if (Config.Item("drawpassive").GetValue<bool>())
                {
                    var wts = Drawing.WorldToScreen(Me.Position);
                    if (Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                        Drawing.DrawText(wts[0] - 48, wts[1] + 10, System.Drawing.Color.White, "Q: Not Learned!");
                    else if (QT <= 0)
                        Drawing.DrawText(wts[0] - 30, wts[1] + 10, System.Drawing.Color.White, "Q: Ready");
                    else
                        Drawing.DrawText(
                            wts[0] - 30, wts[1] + 10, System.Drawing.Color.White, "Q: " + QT.ToString("0.0"));

                }
            }

            if (Config.Item("debugdmg").GetValue<bool>() && Maintarget.IsValidTarget(1000))
            {
                var wts = Drawing.WorldToScreen(Maintarget.Position);
                if (!r.IsReady())
                    Drawing.DrawText(wts[0] - 75, wts[1] + 40, System.Drawing.Color.Yellow,
                        "Combo Damage: " + (float)(ra * 3 + rq * 3 + rw + rr + ri + ritems));
                else
                    Drawing.DrawText(wts[0] - 75, wts[1] + 40, System.Drawing.Color.Yellow,
                        "Combo Damage: " + (float)(ua * 3 + uq * 3 + uw + rr + ri + ritems));
            }

            if (Maintarget.IsValidTarget(1000) && Config.Item("drawkill").GetValue<bool>())
            {
                var wts = Drawing.WorldToScreen(Maintarget.Position);

                if ((float)(ra + rq * 2 + rw + ri + ritems) > Maintarget.Health)
                    Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LawnGreen, "Kill!");
                else if ((float)(ra * 2 + rq * 2 + rw + ritems) > Maintarget.Health)
                    Drawing.DrawText(wts[0] - 40, wts[1] + 20, System.Drawing.Color.LawnGreen, "Easy Kill!");
                else if ((float)(ua * 3 + uq * 2 + uw + ri + rr + ritems) > Maintarget.Health)
                    Drawing.DrawText(wts[0] - 65, wts[1] + 20, System.Drawing.Color.LawnGreen, "Full Combo Kill!");
                else if ((float)(ua * 3 + uq * 3 + uw + rr + ri + ritems) > Maintarget.Health)
                    Drawing.DrawText(wts[0] - 70, wts[1] + 20, System.Drawing.Color.LawnGreen, "Full Combo Hard Kill!");
                else if ((float)(ua * 3 + uq * 3 + uw + rr + ri + ritems) < Maintarget.Health)
                    Drawing.DrawText(wts[0] - 40, wts[1] + 20, System.Drawing.Color.LawnGreen, "Cant Kill!");
            }
        }

        #endregion

        #region Animations
        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Animation.Contains("Idle"))
            {
                canattack = true;
            }

            if (args.Animation.Contains("Attack"))
            {
                canmove = false;
                canattack = false;
                lastattack = Environment.TickCount;
                isattacking = true;
                cancleave = false;
                cankiburst = false;
                candash = false;
                canwindslash = false;
            }

            if (args.Animation.Contains("Spell1a"))
            {
                QTRem = Game.Time + (13 + (13 * Me.PercentCooldownMod));
            }

            if (args.Animation.Contains("Spell1a") || args.Animation.Contains("Spell1b"))
            {
                lastcleave = Environment.TickCount;
                isattacking = false;
                iscleaving = true;
                cancleave = false;
                canmove = false;

                if (Config.Item("fleemode").GetValue<KeyBind>().Active)
                {
                    return;
                }

                if (Maintarget.IsValidTarget(truerange + 100))
                {
                    Utility.DelayAction.Add(
                        140, () => Me.IssueOrder(GameObjectOrder.MoveTo, new Vector3(movePos.X, movePos.Y, movePos.Z)));
                }
            }

            if (args.Animation.Contains("Spell1c"))
            {
                if (Config.Item("fleemode").GetValue<KeyBind>().Active)
                {
                    return;
                }

                if (Maintarget.IsValidTarget(truerange + 100))
                {
                    if (args.Animation != "Idle")
                        Me.IssueOrder(GameObjectOrder.MoveTo, new Vector3(movePos.X, movePos.Y, movePos.Z));
                }
            }
        }

        #endregion

        #region OnSpellCast
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var mode = Config.Item("engage").GetValue<StringList>();

            // tickcounts ftw
            switch (args.SData.Name)
            {
                case "RivenMartyr":
                    lastkiburst = Environment.TickCount;
                    iskibursting = true;
                    cankiburst = false;
                    canmove = false;
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        Utility.DelayAction.Add(
                            Game.Ping + 75, delegate
                            {
                                if (q.IsReady() && cancleave)
                                    q.Cast(Maintarget.ServerPosition);
                            });
                    }
                    break;

                case "ItemTiamatCleave":
                    lasthydra = Environment.TickCount;
                    canattack = true;
                    break;
                case "RivenFeint":
                    lastdash = Environment.TickCount;
                    isdashing = true;
                    canmove = false;
                    candash = false;
                    break;
                case "RivenFengShuiEngine":
                    if (Maintarget.Distance(Me.ServerPosition) <= wrange)
                    {
                        if (w.IsReady())
                            w.Cast();
                    }
                    break;
                case "rivenizunablade":
                    canwindslash = false;
                    if (q.IsReady())
                        q.Cast(Maintarget.ServerPosition);
                    break;
            }

        }

        #endregion

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Maintarget = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
            CheckDamage(Maintarget);

            Orbwalker.SetAttack(canattack);
            Orbwalker.SetMovement(canmove);

            wrange = ulton ? w.Range + 135 : w.Range;
            astime = 1 / (0.318 * Me.AttackSpeedMod);
            truerange = Me.AttackRange + Me.Distance(Me.BBox.Minimum) + 1;

            ulton = Me.GetSpell(SpellSlot.R).Name != "RivenFengShuiEngine";
            hashydra = Items.HasItem(3077) || Items.HasItem(3074);
            canhydra = !isattacking && (Items.CanUseItem(3077) || Items.CanUseItem(3074));

            QT = (QTRem - Game.Time > 0) ? (QTRem - Game.Time) : 0;

            if (Maintarget.IsValidTarget(1000))
            {
                movePos = Maintarget.ServerPosition +
                          Vector3.Normalize(Me.Position - Maintarget.ServerPosition) *
                          (Me.Distance(Maintarget.ServerPosition) + 51);
            }
            else
            {
                movePos = Game.CursorPos;
            }

            if (ulton && Config.Item("usews").GetValue<bool>())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(r.Range)))
                {
                    if (target.Health <= rr && canwindslash)
                    {
                        r.CastIfHitchanceEquals(target, HitChance.Medium);
                    }
                }
            }

            if (Config.Item("fleemode").GetValue<KeyBind>().Active)
            {
                if (candash && e.IsReady())
                {
                    e.Cast(Game.CursorPos);
                }

                if (!e.IsReady() && Environment.TickCount - lastcleave >= 300 && Environment.TickCount - lastdash >= 500)
                {
                    q.Cast(Game.CursorPos);
                }

                if (canmove)
                {
                    Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }

            foreach (var b in Me.Buffs)
            {
                if (b.Name == "RivenTriCleave")
                    cleavecount = b.Count;

                if (b.Name == "rivenpassiveaaboost")
                    runiccount = b.Count;
            }

            if (Me.HasBuff("RivenTriCleave", true) && Environment.TickCount - lastcleave >= 3600)
            {
                if (Config.Item("keepq").GetValue<bool>() && !Me.IsRecalling())
                    q.Cast(Game.CursorPos);
            }

            if (!Me.HasBuff("rivenpassiveaaboost", true))
                Utility.DelayAction.Add(1000, () => runiccount = 1);

            if (!Me.HasBuff("RivenTriCleave", true))
                Utility.DelayAction.Add(1000, () => cleavecount = 0);


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                NormalCombo(Maintarget);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
                JungleClear();
            }

            if (!canwindslash && !isattacking && ulton && r.IsReady())
            {
                canwindslash = true;
            }

            if (!candash && !(iscleaving || isattacking || iskibursting) && e.IsReady())
            {
                candash = true;
            }

            if (!cankiburst && !(iscleaving || isattacking || isdashing) && w.IsReady())
            {
                cankiburst = true;
            }

            var obj = (Orbwalker.GetTarget() != null ? (Obj_AI_Base)Orbwalker.GetTarget() : Maintarget) ?? Me;

            var time = (int)(Me.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                    1000 * (int)Me.Distance(obj.ServerPosition) / (int)Me.BasicAttack.MissileSpeed;

            if (!canmove && !(isattacking || iscleaving || iskibursting || isdashing) &&
                Environment.TickCount - lastattack >= time)
            {
                canmove = true;
            }

            if (!canattack && !(iscleaving || isdashing || iskibursting) &&
                 Environment.TickCount - lastattack >= time + 200)
            {
                canattack = true;
            }

            if (isattacking && Environment.TickCount - lastattack >=
                Convert.ToInt32((astime * 100) - 10) - (Me.Level * 8 / 2))
            {
                isattacking = false;
                cancleave = true;
                canmove = true;
                candash = true;
                cankiburst = true;
                canwindslash = true;
            }

            if (iscleaving && Environment.TickCount - lastcleave >= 273)
            {
                iscleaving = false;
                canmove = true;
                canattack = true;
            }

            if (iskibursting && Environment.TickCount - lastkiburst >= 148)
            {
                iskibursting = false;
                canattack = true;
                canmove = true;
            }

            if (isdashing && Environment.TickCount - lastdash >= 200)
            {
                isdashing = false;
                canmove = true;
            }

        }

        #region Minion Clear
        private static void LaneClear()
        {
            var minionListerino = MinionManager.GetMinions(Me.ServerPosition, e.Range);
            foreach (var minion in minionListerino)
            {
                Orb(minion, "Lane");
                if (q.IsReady() && cancleave && minion.Distance(Me.ServerPosition) <= q.Range)
                {
                    if (Config.Item("uselaneq").GetValue<bool>())
                        q.Cast(minion.ServerPosition);
                }

                if (w.IsReady() && cankiburst)
                {
                    if (minionListerino.Count(m => m.IsValidTarget(w.Range)) >= 4)
                    {
                        if (Config.Item("uselanew").GetValue<bool>())
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                            w.Cast();
                        }
                    }
                }

                if (e.IsReady() && candash && minion.IsValidTarget(e.Range + q.Range))
                {
                    if (Config.Item("uselanee").GetValue<bool>())
                        e.Cast(Game.CursorPos);
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            minionList.Any(x => m.Name.StartsWith(x)) && !m.Name.StartsWith("Minion") &&
                            !m.Name.Contains("Mini")))
            {
                Orb(minion, "Combo");
                if (cancleave && q.IsReady() && minion.Distance(Me.ServerPosition) <= q.Range)
                {
                    if (Config.Item("usejungleq").GetValue<bool>())
                        q.Cast(minion.ServerPosition);
                }

                if (cankiburst && w.IsReady() && minion.Distance(Me.ServerPosition) <= w.Range)
                {
                    if (Config.Item("usejunglew").GetValue<bool>())
                        w.Cast();
                }

                if (e.IsReady() && candash)
                {
                    if (minion.Distance(Me.ServerPosition) <= e.Range + q.Range ||
                        Me.Health / Me.MaxHealth * 100 <= Config.Item("vhealth").GetValue<Slider>().Value)
                    {
                        if (Config.Item("uselanee").GetValue<bool>())
                            e.Cast(Game.CursorPos);
                    }
                }
            }
        }

        #endregion

        #region Combos
        private static void NormalCombo(Obj_AI_Base target)
        {
            var mode = Config.Item("engage").GetValue<StringList>();
            var healthpercent = Me.Health / Me.MaxHealth * 100;
            if (!target.IsValidTarget(r.Range * 2))
            {
                return;
            }

            Orb(target, "Combo");
            // valor
            if (e.IsReady() && candash && (target.Distance(Me.ServerPosition) > truerange + 50))
            {
                if (target.Distance(Me.ServerPosition) <= truerange + e.Range + 100 ||
                    healthpercent <= Config.Item("vhealth").GetValue<Slider>().Value)
                {

                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
                    ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
                    ItemData.Bilgewater_Cutlass.GetItem().Cast(target);

                    if (Config.Item("usecomboe").GetValue<bool>())
                        e.Cast(target.ServerPosition);

                    switch (mode.SelectedIndex)
                    {
                        case 0:
                        case 2:
                            CheckR(target);
                            break;
                        case 1:
                            if (hashydra && canhydra)
                            {
                                if (w.IsReady())
                                {
                                    Items.UseItem(3077);
                                    Items.UseItem(3074);
                                }

                                Utility.DelayAction.Add(250, () => CheckR(target));
                            }

                            else
                            {
                                CheckR(target);
                            }
                            break;
                    }

                }
            }

            // kiburst
            else if (w.IsReady() && cankiburst && target.Distance(Me.ServerPosition) <= wrange)
            {
                switch (mode.SelectedIndex)
                {
                    case 0:
                        if (canhydra && hashydra)
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                        }

                        Utility.DelayAction.Add(
                            130, delegate
                            {
                                if (Config.Item("usecombow").GetValue<bool>())
                                    w.Cast();
                            });
                        break;
                    case 1:
                        if (hashydra && canhydra)
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                            Utility.DelayAction.Add(250, () => CheckR(target));
                            if (Config.Item("usecombow").GetValue<bool>())
                                Utility.DelayAction.Add(300, () => w.Cast());
                        }

                        else
                        {
                            CheckR(target);
                            if (Config.Item("usecombow").GetValue<bool>())
                                w.Cast();
                        }
                        break;

                }


            }


            // cleaves
            else if (cancleave && q.IsReady() && target.Distance(Me.ServerPosition) <= q.Range + 10)
            {
                CheckR(target);
                if (!Config.Item("usecomboq").GetValue<bool>())
                {
                    return;
                }

                if (Environment.TickCount - lasthydra >= 800 && Environment.TickCount - lastdash >= 200)
                {
                    if (Config.Item("prediction").GetValue<bool>())
                        q.CastIfHitchanceEquals(target, HitChance.Medium);
                    else
                        q.Cast(target.ServerPosition);
                }
            }

            // gapclose
            else if (target.Distance(Me.ServerPosition) > truerange + 101)
            {
                if (!Config.Item("usecomboq").GetValue<bool>())
                {
                    return;
                }

                if (Environment.TickCount - lastcleave >= 1000 && !isattacking)
                {
                    if (e.IsReady())
                    {
                        return;
                    }

                    q.Cast(target.ServerPosition);
                }
            }

            // ignite
            var ignote = Me.GetSpellSlot("summonerdot");
            if ((float)ua * 3 + uq * 3 + uw + rr + ri + ritems >= target.Health)
            {
                if (Me.Spellbook.CanUseSpell(ignote) == SpellState.Ready)
                {
                    if (ulton && target.Distance(Me.ServerPosition) <= 600 && cleavecount <= 1)
                        Me.Spellbook.CastSpell(ignote, target);
                }
            }
        }

        #endregion

        private static void Orb(Obj_AI_Base target, string mode)
        {
            if (target.IsValidTarget(truerange + 100) && canattack)
            {
                if (mode == "Lane" && !Config.Item("forceaa").GetValue<bool>())
                {
                    return;
                }

                canmove = false;
                Me.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        private static void CheckR(Obj_AI_Base target)
        {
            if (target.IsValidTarget(r.Range + 100))
            {
                if (ulton || !Config.Item("user").GetValue<bool>())
                {
                    return;
                }

                if ((float)ua * 3 + uq * 3 + uw + rr + ri + ritems >= target.Health)
                {
                    if (cleavecount <= 1 && q.IsReady())
                    {
                        r.Cast();
                    }
                }
            }
        }

        #region Riven: Fat List Incoming

        private static Spell q;
        private static Spell w;
        private static Spell e;
        private static Spell r;
        private static Vector3 movePos;

        private static float QT, QTRem;
        private static double rr, ri, ritems;
        private static double rq, rw, ra;
        private static double uq, uw, ua;

        private static bool isattacking;
        private static bool iscleaving;
        private static bool iskibursting;
        private static bool isdashing;

        private static double astime;
        private static bool hashydra;
        private static bool canhydra;
        private static float wrange;
        private static bool ulton;
        private static bool canwindslash = true;
        private static bool canmove = true;
        private static bool canattack = true;
        private static bool cancleave = true;
        private static bool cankiburst = true;
        private static bool candash = true;

        private static int lasthydra;
        private static int lastattack;
        private static int lastcleave;
        private static int lastkiburst;
        private static int lastdash;
        private static int cleavecount;
        private static int runiccount;

        private static float truerange;
        private static readonly int[] runicpassive =
        {
            20, 20, 25, 25, 25, 30, 30, 30, 35, 35, 35, 40, 40, 40, 45, 45, 45, 50, 50
        };

        private static readonly string[] minionList =
        {
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_Dragon",
            "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"
        };

        #endregion
    }
}
