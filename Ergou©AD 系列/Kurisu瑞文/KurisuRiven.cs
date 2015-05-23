using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using SharpDX;

namespace KurisuRiven
{
    internal class KurisuRiven
    {
        #region Riven: Main Vars

        private static int lastq;
        private static int lastw;
        private static int laste;
        private static int lastaa;
        private static int lasthd;

        private static bool canq;
        private static bool canw;
        private static bool cane;
        private static bool canmv;
        private static bool canaa;
        private static bool canws;
        private static bool canhd;
        private static bool hashd;

        private static bool didq;
        private static bool didw;
        private static bool dide;
        private static bool didws;
        private static bool didaa;
        private static bool didhd;
        private static bool ssfl;

        private static Menu menu;
        private static Spell q, w, e, r;
        private static Orbwalking.Orbwalker orbwalker;
        private static Obj_AI_Hero player = ObjectManager.Player;
        private static HpBarIndicator hpi = new HpBarIndicator();

        private static Obj_AI_Base qtarg; // semi q target
        private static Obj_AI_Hero rtarg; // riven target

        private static bool ulton;
        private static bool canburst;

        private static int cleavecount;
        private static int passivecount;
        private static SpellSlot flash;

        private static float myhitbox;
        private static Vector3 movepos;

        private static readonly string[] minionlist =
        {
            // summoners rift
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_Dragon",
            "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp", 
            
            // twisted treeline
            "TT_NGolem5", "TT_NGolem2", "TT_NWolf6", "TT_NWolf3",
            "TT_NWraith1", "TT_Spider"
        };

        #endregion

        # region Riven: Utils

        private static bool menubool(string item)
        {
            return menu.Item(item).GetValue<bool>();
        }

        private static int menuslide(string item)
        {
            return menu.Item(item).GetValue<Slider>().Value;
        }

        private static int menulist(string item)
        {
            return menu.Item(item).GetValue<StringList>().SelectedIndex;
        }

        private static float xtra(float dmg)
        {
           return r.IsReady() ? (float) (dmg + (dmg*0.2)) : dmg;
        }

        private static void UseInventoryItems(Obj_AI_Base target)
        {
            if (Items.HasItem(3142) && Items.CanUseItem(3142))
                Items.UseItem(3142);

            if (target.Distance(player.ServerPosition, true) <= 450 * 450)
            {
                if (Items.HasItem(3144) && Items.CanUseItem(3144))
                    Items.UseItem(3144, target);
                if (Items.HasItem(3153) && Items.CanUseItem(3153))
                    Items.UseItem(3153, target);
            }
        }

        #endregion

        private static int build = 26;
        public KurisuRiven()
        {
            Console.WriteLine("KurisuRiven enabled!");
            CustomEvents.Game.OnGameLoad += args =>
            {
                try
                {
                    if (player.ChampionName == "Riven")
                    {
                        w = new Spell(SpellSlot.W, 250f);
                        e = new Spell(SpellSlot.E, 270f);

                        q = new Spell(SpellSlot.Q, 260f);
                        q.SetSkillshot(0.25f, 100f, 2200f, false, SkillshotType.SkillshotCircle);

                        r = new Spell(SpellSlot.R, 1100f);
                        r.SetSkillshot(0.25f, 225f, 1600f, false, SkillshotType.SkillshotCone);
                        flash = player.GetSpellSlot("summonerflash");

                        Menu_OnSet();
                        Interrupter();
                        OnGapcloser();
                        OnPlayAnimation();
                        OnCast();
                        Drawings();

                        Game.OnUpdate += Game_OnUpdate;
                        Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
                        Game.PrintChat("<b><font color=\"#FF9900\">KurisuRiven:</font></b> " +
                                       "Build <b><font color=\"#FF9900\">" + build + "</font></b> Loaded!");

                        if (menu.Item("Farm").GetValue<KeyBind>().Key == menu.Item("semiq").GetValue<KeyBind>().Key ||
                            menu.Item("Orbwalk").GetValue<KeyBind>().Key == menu.Item("semiq").GetValue<KeyBind>().Key ||
                            menu.Item("LaneClear").GetValue<KeyBind>().Key == menu.Item("semiq").GetValue<KeyBind>().Key ||
                            menu.Item("LastHit").GetValue<KeyBind>().Key == menu.Item("semiq").GetValue<KeyBind>().Key)
                        {
                            Game.PrintChat(
                                "<b><font color=\"#FF9900\">" +
                                "WARNING: Semi-Q Keybind Should not be the same key as any of " +
                                "the other orbwalking modes or it will not Work!</font></b>");
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Fatal Error: " + e.Message);
                }
            };
        }

        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe && !args.IsDash)
            {
                if (didq)
                {
                    didq = false;
                    canaa = true;
                    canmv = true;
                }
            }
        }

        #region Riven: Update
        private static void Game_OnUpdate(EventArgs args)
        {
            rtarg = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);

            myhitbox = player.AttackRange + player.Distance(player.BBox.Minimum) + 1;

            hashd = Items.HasItem(3077) || Items.HasItem(3074);
            canhd = !didaa && (Items.CanUseItem(3077) || Items.CanUseItem(3074));

            if (!qtarg.IsValidTarget(myhitbox + 100))
                qtarg = player;

            ulton = player.GetSpell(SpellSlot.R).Name != "RivenFengShuiEngine";

            canburst = rtarg != null && r.IsReady() && q.IsReady() && 
                ((ComboDamage(rtarg)/1.6) >= rtarg.Health || rtarg.CountEnemiesInRange(w.Range) >= menuslide("multic"));

            if (menulist("cancelt") == 0 && qtarg != player)
                movepos = player.ServerPosition + (player.ServerPosition - qtarg.ServerPosition).Normalized()*28;
            if (menulist("cancelt") == 1 && qtarg != player)
                movepos = RotateVector(player.ServerPosition.To2D(), (player.ServerPosition.To2D() +
                         (qtarg.ServerPosition.To2D() - player.ServerPosition.To2D()).Normalized()*270), 40).To3D();

            if (qtarg == player)
                movepos = Game.CursorPos;

            orbwalker.SetAttack(canmv);
            orbwalker.SetMovement(canmv);

            // reqs ->
            SemiQ();
            AuraUpdate();
            CombatCore();

            Windslash();

            if (rtarg.IsValidTarget() && 
                menu.Item("combokey").GetValue<KeyBind>().Active)
            {
                TryFlashInitiate(rtarg);
                ComboTarget(rtarg);
            }

            if (rtarg.IsValidTarget() && 
                menu.Item("harasskey").GetValue<KeyBind>().Active)
            {
                HarassTarget(rtarg);
            }

            if (player.IsValid &&
                menu.Item("clearkey").GetValue<KeyBind>().Active)
            {
                Clear();
                Wave();
            }

            if (player.IsValid &&
                menu.Item("fleekey").GetValue<KeyBind>().Active)
            {
                Flee();
            }
        }

        #endregion

        #region Riven: Menu
        private static void Menu_OnSet()
        {
            menu = new Menu("Kurisu's 瑞文", "kurisuriven", true);

            var tsmenu = new Menu("目标选择", "selector");
            TargetSelector.AddToMenu(tsmenu);
            menu.AddSubMenu(tsmenu);

            var orbwalkah = new Menu("走砍", "rorb");
            orbwalker = new Orbwalking.Orbwalker(orbwalkah);
            menu.AddSubMenu(orbwalkah);

            var keybinds = new Menu("键位", "keybinds");
            keybinds.AddItem(new MenuItem("combokey", "使用 连招")).SetValue(new KeyBind(32, KeyBindType.Press));
            keybinds.AddItem(new MenuItem("harasskey", "使用 骚扰")).SetValue(new KeyBind(67, KeyBindType.Press));
            keybinds.AddItem(new MenuItem("clearkey", "使用 清野/清线")).SetValue(new KeyBind(86, KeyBindType.Press));
            keybinds.AddItem(new MenuItem("fleekey", "使用 逃跑模式")).SetValue(new KeyBind(65, KeyBindType.Press));

            var mitem = new MenuItem("semiqlane", "使用 手动-Q 清线");
            mitem.ValueChanged += (sender, args) =>
            {
                if (menu.Item("Farm").GetValue<KeyBind>().Key == args.GetNewValue<KeyBind>().Key ||
                    menu.Item("Orbwalk").GetValue<KeyBind>().Key == args.GetNewValue<KeyBind>().Key ||
                    menu.Item("LaneClear").GetValue<KeyBind>().Key == args.GetNewValue<KeyBind>().Key ||
                    menu.Item("LastHit").GetValue<KeyBind>().Key == args.GetNewValue<KeyBind>().Key)
                {
                    Game.PrintChat(
                        "<b><font color=\"#FF9900\">" +
                        "WARNING: Semi-Q Keybind Should not be the same key as any of " +
                        "the other orbwalking modes or it will not Work!</font></b>");
                }
            };

            keybinds.AddItem(mitem).SetValue(new KeyBind(71, KeyBindType.Press));
            keybinds.AddItem(new MenuItem("semiq", "使用 手动-Q 骚扰/清野")).SetValue(true);
            menu.AddSubMenu(keybinds);

            var drMenu = new Menu("显示", "drawings");
            drMenu.AddItem(new MenuItem("drawengage", "显示连招范围")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawdmg", "显示造成血量伤害")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawburst", "显示R连招范围")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawtarg", "调试攻击目标")).SetValue(false);
            menu.AddSubMenu(drMenu);

            var combo = new Menu("连招", "combo");
            var qmenu = new Menu("Q  设置", "rivenq");
            qmenu.AddItem(new MenuItem("qint", "使用 3rd Q 中断法术")).SetValue(true);
            qmenu.AddItem(new MenuItem("usegap", "Q 防止突进")).SetValue(true);
            qmenu.AddItem(new MenuItem("gaptime", "防止突进  使用 Q 延迟 (ms)")).SetValue(new Slider(110, 50, 200));
            qmenu.AddItem(new MenuItem("keepq", "保持 Q Buff（持续使用）")).SetValue(true);
            qmenu.AddItem(new MenuItem("cancelt", "禁用 Q 移动："))
                .SetValue(
                    new StringList(
                        new[] {"移动 (目标落后于自己)", "移动 (与目标同一位置)"}));

            qmenu.AddItem(new MenuItem("sepp", "增加延迟直到 AA's 不能取消:"));
            qmenu.AddItem(new MenuItem("aaq", "自动-攻击 -> 禁用Q 延迟 (ms)")).SetValue(new Slider(15, 0, 300));
            combo.AddSubMenu(qmenu);

            var wmenu = new Menu("W 设置", "rivenw");
            wmenu.AddItem(new MenuItem("usecombow", "使用 连招")).SetValue(true);
            wmenu.AddItem(new MenuItem("wgap", "使用 W 防止突进")).SetValue(true);
            wmenu.AddItem(new MenuItem("wint", "使用 W 中断法术")).SetValue(true);
            combo.AddSubMenu(wmenu);

            var emenu = new Menu("E  设置", "rivene");
            emenu.AddItem(new MenuItem("usecomboe", "使用 E 连招")).SetValue(true);
            emenu.AddItem(new MenuItem("emode", "使用 E 模式"))
                .SetValue(new StringList(new[] { "E -> W/R -> 提亚马特 -> Q", "E -> 提亚马特 -> W/R -> Q" }));
            emenu.AddItem(new MenuItem("erange", "使用 E 当目标距离 > AA范围 或 连招范围")).SetValue(true);
            emenu.AddItem(new MenuItem("vhealth", "或 使用 E 当自己 HP% <=")).SetValue(new Slider(40));
            combo.AddSubMenu(emenu);

            var rmenu = new Menu("R  设置", "rivenr");
            rmenu.AddItem(new MenuItem("user", "使用 R 连招")).SetValue(true);
            rmenu.AddItem(new MenuItem("useignote", "使用 R + 智能点燃")).SetValue(true);
            rmenu.AddItem(new MenuItem("multib", "闪现 R/W 如果可以将目标连招击杀")).SetValue(false);
            rmenu.AddItem(new MenuItem("multic", "闪现 R/W 如果可W中目标 >= (6 = 禁用)")).SetValue(new Slider(3, 2, 6));
            rmenu.AddItem(new MenuItem("overk", "禁用 R 如果目标 HP % <=")).SetValue(new Slider(25, 1, 99));
            rmenu.AddItem(new MenuItem("userq", "使用 R 只有Q的使用计数 <=")).SetValue(new Slider(1, 1, 3));
            rmenu.AddItem(new MenuItem("ultwhen", "使用 R 目标击杀难度："))
                .SetValue(new StringList(new[] {"一般", "困难", "总是"}, 1));
            rmenu.AddItem(new MenuItem("usews", "使用 光速QA (R2) 连招")).SetValue(true);
            rmenu.AddItem(new MenuItem("wsmode", "光速QA (R2) 当"))
                .SetValue(new StringList(new[] {"目标可以击杀", "目标击杀 或 造成最大伤害"}, 1));
            rmenu.AddItem(new MenuItem("rmulti", "光速QA 当可攻击到的目标 >=")).SetValue(new Slider(3, 2, 5));
            combo.AddSubMenu(rmenu);


            menu.AddSubMenu(combo);

            var harass = new Menu("骚扰", "harass");
            harass.AddItem(new MenuItem("usegaph", "使用 Q 突进骚扰")).SetValue(true);
            harass.AddItem(new MenuItem("gaptimeh", "突进骚扰时 Q 延迟(ms)")).SetValue(new Slider(110, 50, 200));
            harass.AddItem(new MenuItem("maxgap", "使用 Q's 次数")).SetValue(new Slider(2, 1, 3));
            harass.AddItem(new MenuItem("useharasse", "使用 E 骚扰")).SetValue(true);
            harass.AddItem(new MenuItem("etoo", "使用 E 接近："))
                .SetValue(new StringList(new[] { "目标", "我方塔下", "鼠标方向" }, 1));

            harass.AddItem(new MenuItem("useharassw", "使用 W 骚扰")).SetValue(true);
            menu.AddSubMenu(harass);

            var farming = new Menu("清线|清野", "farming");


            var jg = new Menu("清线", "jungle");
            jg.AddItem(new MenuItem("uselaneq", "使用 Q 清线")).SetValue(true);
            jg.AddItem(new MenuItem("uselanew", "使用 W 清线")).SetValue(true);
            jg.AddItem(new MenuItem("wminion", "使用 W 可击杀小兵数量 >=")).SetValue(new Slider(3, 1, 6));
            jg.AddItem(new MenuItem("uselanee", "使用 E 清线")).SetValue(true);
            farming.AddSubMenu(jg);

            var wc = new Menu("清野", "waveclear");
            wc.AddItem(new MenuItem("usejungleq", "使用 Q 清野")).SetValue(true);
            wc.AddItem(new MenuItem("usejunglew", "使用 W 清野")).SetValue(true);
            wc.AddItem(new MenuItem("usejunglee", "使用 E 清野")).SetValue(true);
            farming.AddSubMenu(wc);


            menu.AddSubMenu(farming);


            menu.AddToMainMenu();

        }

        #endregion

        #region Riven : Flash Initiate

        private static void TryFlashInitiate(Obj_AI_Hero target)
        {
            // use r at appropriate distance
            // on spell cast takes over

            if (!menubool("multib"))
                return;

            if (!menu.Item("combokey").GetValue<KeyBind>().Active ||
                !target.IsValid<Obj_AI_Hero>() || ulton || !menubool("user"))
            {
                return;
            }

            if (rtarg == null || !canburst || ulton)
            {
                return;
            }

            if (!flash.IsReady())
            {
                return;
            }

            if (e.IsReady() && target.Distance(player.ServerPosition) <= e.Range + w.Range + 300)
            {
                if (target.Distance(player.ServerPosition) > e.Range + myhitbox)
                {
                    e.Cast(target.ServerPosition);
                    r.Cast();
                }
            }

            if (!e.IsReady() && target.Distance(player.ServerPosition) <= w.Range + 300)
            {
                if (target.Distance(player.ServerPosition) > myhitbox + 35)
                {
                    r.Cast();
                }
            }
        }

        #endregion

        #region Riven: Combo

        private static void ComboTarget(Obj_AI_Base target)
        {
            // orbwalk ->
            OrbTo(target);

            // ignite ->
            var ignote = player.GetSpellSlot("summonerdot");
            if (player.Spellbook.CanUseSpell(ignote) == SpellState.Ready)
            {
                if (rtarg.Distance(player.ServerPosition) <= 600 * 600)
                {
                    if (cleavecount <= menuslide("userq") && q.IsReady() && menubool("useignote"))
                    {
                        if (ComboDamage(target) >= target.Health &&
                            target.Health/target.MaxHealth*100 > menuslide("overk"))
                        {
                            if (r.IsReady() && ulton)
                            {
                                player.Spellbook.CastSpell(ignote, target);
                            }
                        }
                    }
                }
            }

            if (e.IsReady() && cane && menubool("usecomboe") &&
               (player.Health/player.MaxHealth*100 <= menuslide("vhealth") ||
                 target.Distance(player.ServerPosition) > myhitbox + 25))
            {
                if (menubool("usecomboe"))
                    e.Cast(target.ServerPosition);

                if (target.Distance(player.ServerPosition) <= r.Range)
                {
                    if (menulist("emode") == 1)
                    {
                        if (canhd && hashd && !canburst)
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                        }

                        else
                        {
                            CheckR();
                        }
                    }

                    if (menulist("emode") == 0)
                    {
                        CheckR();
                    }
                }
            }

            else if (w.IsReady() && canw && menubool("usecombow") &&
                target.Distance(player.ServerPosition) <= w.Range + 25)
            {
                if (menulist("emode") == 0)
                {
                    if (menubool("usecombow"))
                        w.Cast();

                    if (canhd && hashd)
                    {
                        Items.UseItem(3077);
                        Items.UseItem(3074);
                    }
                }

                if (menulist("emode") == 1)
                {
                    if (canhd && hashd && !canburst)
                    {
                        Items.UseItem(3077);
                        Items.UseItem(3074);
                        if (menubool("usecombow"))
                            Utility.DelayAction.Add(250, () => w.Cast());
                    }

                    else
                    {
                        CheckR();
                        if (menubool("usecombow"))
                            w.Cast();
                    }
                }

                UseInventoryItems(target);
                CheckR();
            }

            else if (q.IsReady() && target.Distance(player.ServerPosition) <= q.Range + 30)
            {
                UseInventoryItems(target);
                CheckR();

                if (menulist("emode") == 0 || (ComboDamage(target)/1.7) >= target.Health)
                {
                    if (Items.CanUseItem(3077) || Items.CanUseItem(3074))
                        return;
                }

                if (canq)
                    q.Cast(target.ServerPosition);
            }

            else if (target.Distance(player.ServerPosition) > myhitbox + 100)
            {
                if (menubool("usegap"))
                {
                    if (!e.IsReady() && Environment.TickCount - lastq >= menuslide("gaptime")*10 && !didaa)
                    {
                        if (q.IsReady() && Environment.TickCount - laste >= 700)
                        {
                            q.Cast(target.ServerPosition);
                        }
                    }
                }
            }
        }

        #endregion

        #region Riven: Harass

        private static void HarassTarget(Obj_AI_Base target)
        {
            OrbTo(target);

            Vector3 epos;
            switch (menulist("etoo"))
            {
                case 0:
                    epos = player.ServerPosition +
                        (player.ServerPosition - target.ServerPosition).Normalized()*45;
                    break;
                case 1:
                    epos = ObjectManager.Get<Obj_AI_Turret>()
                        .Where(t => (t.IsAlly)).OrderBy(t => t.Distance(player.Position)).First().Position;
                    break;
                default:
                    epos = Game.CursorPos;
                    break;
            }


            if (target.Distance(player.ServerPosition) <= w.Range + 10)
            {
                if (w.IsReady() && canw && (cleavecount >= 2 || !q.IsReady()))
                {
                    if (menubool("useharassw"))
                    {
                        w.Cast();
                    }
                }

                if ((!w.IsReady() || player.GetSpell(SpellSlot.W).State == SpellState.NotLearned) &&
                   (!q.IsReady() || player.GetSpell(SpellSlot.Q).State == SpellState.NotLearned))
                {
                    if (e.IsReady() && cane)
                    {
                        // dash away ->
                        e.Cast(epos);
                    }
                }
            }

            if (target.Distance(player.ServerPosition) <= q.Range + 30)
            {
                // q engage ->
                if (q.IsReady() && Environment.TickCount - laste >= 800)
                {
                    if (canq && !player.Position.Extend(player.Position, q.Range).UnderTurret(true))
                        q.Cast(target.ServerPosition);
                }

                else
                {
                    if (Environment.TickCount - lastq >= 500)
                    {
                        if (menubool("useharasse"))
                        {
                            // stun? ->
                            if (!w.IsReady() || player.GetSpell(SpellSlot.W).State == SpellState.NotLearned)
                            {
                                if (e.IsReady() && cane)
                                {
                                    // dash away ->
                                    e.Cast(epos);
                                }
                            }
                        }
                    }
                }
            }

            else if (target.Distance(player.ServerPosition) > myhitbox + 100)
            {
                // gapclose harass ->
                if (Environment.TickCount - lastq >= menuslide("gaptimeh") * 10 && !didaa)
                {
                    if (q.IsReady() && menubool("usegaph") &&
                        Environment.TickCount - laste >= 800)
                    {
                        if (cleavecount < menuslide("maxgap"))
                        {
                            q.Cast(target.ServerPosition);
                        }
                    }
                }
            }

        }
        #endregion

        #region Riven: Windslash

        private static void Windslash()
        {
            if (ulton && menubool("usews") && r.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(r.Range)))
                {
                    // only kill or killsteal etc ->
                    if (r.GetDamage(target) >= rtarg.Health && canws)
                    {
                        if (r.GetPrediction(target, true).Hitchance >= HitChance.Low)
                            r.Cast(r.GetPrediction(target, true).CastPosition);
                    }
                }

                // kill or maxdamage ->
                if (menulist("wsmode") == 1 && rtarg.IsValidTarget(r.Range))
                {
                    r.CastIfWillHit(rtarg, menuslide("rmulti"));

                    var po = r.GetPrediction(rtarg, true);
                    if ((r.GetDamage(rtarg) / rtarg.MaxHealth * 100) >= rtarg.Health / rtarg.MaxHealth * 100)
                    {
                        if (po.Hitchance >= HitChance.Low && canws)
                            r.Cast(po.CastPosition);
                    }

                    if (q.IsReady() && rtarg.Health <= xtra((float)
                       (r.GetDamage(rtarg) + player.GetAutoAttackDamage(rtarg) * 2 + Qdmg(rtarg) * 2)))
                    {
                        if (rtarg.Distance(player.ServerPosition) <= myhitbox + 100)
                        {
                            if (po.Hitchance >= HitChance.Low && canws)
                                r.Cast(po.CastPosition);
                        }
                    }
                }
            }
        }

        #endregion

        #region Riven: Lane/Jungle

        private static void Clear()
        {
            var minions = MinionManager.GetMinions(player.Position, 600f,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            foreach (var unit in minions)
            {
                OrbTo(unit);
                if (q.IsReady() && unit.Distance(player.ServerPosition) <= q.Range + 100)
                {
                    if (canq && menubool("usejungleq"))
                        q.Cast(unit.ServerPosition);
                }

                if (w.IsReady() && unit.Distance(player.ServerPosition) <= w.Range + 10)
                {
                    if (canw && menubool("usejunglew"))
                        w.Cast();
                }

                if (e.IsReady() && (unit.Distance(player.ServerPosition) > myhitbox + 30 ||
                    player.Health / player.MaxHealth * 100 <= 70))
                {
                    if (cane && menubool("usejunglee"))
                        e.Cast(unit.ServerPosition);
                }
            }
        }

        private static void Wave()
        {
            var minions = MinionManager.GetMinions(player.Position, 600f,
            MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

            foreach (var unit in minions)
            {
                if (player.GetAutoAttackDamage(unit, true) >= unit.Health)
                {
                    OrbTo(unit);
                }

                if (q.IsReady() && unit.Distance(player.ServerPosition) <= q.Range + 100)
                {
                    if (canq && menubool("uselaneq") && minions.Count >= 2)
                        q.Cast(unit.ServerPosition);
                }

                if (w.IsReady() &&
                    minions.Count(m => m.Distance(player.ServerPosition) <= w.Range + 10) >= menuslide("wminion"))
                {
                    if (canw && menubool("uselanew"))
                    {
                        Items.UseItem(3077);
                        Items.UseItem(3074);
                        w.Cast();
                    }
                }

                if (e.IsReady() && (unit.Distance(player.ServerPosition) > myhitbox + 30 ||
                    player.Health / player.MaxHealth * 100 <= 70))
                {
                    if (cane && menubool("uselanee"))
                        e.Cast(unit.ServerPosition);
                }
            }
        }

        #endregion

        #region Riven: Flee

        private static void Flee()
        {
            if (canmv)
            {
                player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (ssfl)
            {
                if (Environment.TickCount - lastq >= 600)
                {
                    q.Cast(Game.CursorPos);
                }

                if (cane && e.IsReady())
                {
                    if (cleavecount >= 2 || !q.IsReady() && !player.HasBuff("RivenTriCleave", true))
                    {
                        e.Cast(Game.CursorPos);
                    }
                }
            }

            else
            {
                if (q.IsReady())
                {
                    q.Cast(Game.CursorPos);
                }

                if (e.IsReady() && Environment.TickCount - lastq >= 250)
                {
                    e.Cast(Game.CursorPos);
                }
            }
        }

        #endregion

        #region Riven: SemiQ

        private static void SemiQ()
        {
            if (canq && Environment.TickCount - lastaa >= 150 && menubool("semiq"))
            {
                if (q.IsReady() && Environment.TickCount - lastaa < 1200 && qtarg != null)
                {
                    if (qtarg.IsValidTarget(q.Range + 100))
                    {
                        if (qtarg.IsValid<Obj_AI_Hero>())
                            q.Cast(qtarg.ServerPosition);
                    }

                    if (!menu.Item("harasskey").GetValue<KeyBind>().Active &&
                        !menu.Item("clearkey").GetValue<KeyBind>().Active)
                    {
                        if (qtarg.IsValidTarget(q.Range + 100) && !qtarg.Name.Contains("Mini"))
                        {
                            if (!qtarg.Name.StartsWith("Minion") && minionlist.Any(name => qtarg.Name.StartsWith(name)))
                            {
                                q.Cast(qtarg.ServerPosition);
                            }
                        }

                        if (qtarg.IsValidTarget(q.Range + 100))
                        {
                            if (qtarg.IsValid<Obj_AI_Minion>() || qtarg.IsValid<Obj_AI_Turret>())
                            {
                                if (menu.Item("semiqlane").GetValue<KeyBind>().Active)
                                    q.Cast(qtarg.ServerPosition);
                            }

                            if (qtarg.IsValid<Obj_AI_Hero>() || qtarg.IsValid<Obj_AI_Turret>())
                            {
                                if (ulton)
                                    q.Cast(qtarg.ServerPosition);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Riven: Check R
        private static void CheckR()
        {
            if (!r.IsReady() || ulton || !menubool("user"))
                return;

            if (menulist("ultwhen") == 2 && q.IsReady() && cleavecount < 3)
                r.Cast();

            var enemies = HeroManager.Enemies.Where(ene => ene.IsValidTarget(r.Range + 250));
            var targets = enemies as IList<Obj_AI_Hero> ?? enemies.ToList();
            foreach (var target in targets)
            {
                if (cleavecount <= menuslide("userq") && (q.IsReady() || Environment.TickCount - lastq < 1000))
                {
                    if (targets.Count(ene => ene.Distance(player.ServerPosition) <= 650) >= 2)
                    {
                        r.Cast();
                    }

                    if (targets.Count() < 2 && target.Health / target.MaxHealth * 100 <= menuslide("overk"))
                    {
                        return;
                    }

                    if (menulist("ultwhen") == 0)
                    {
                        if ((ComboDamage(target) / 1.7) >= target.Health)
                        {
                            r.Cast();
                        }
                    }

                    // hard kill ->
                    if (menulist("ultwhen") == 1)
                    {
                        if (ComboDamage(target) >= target.Health)
                        {
                            r.Cast();
                        }
                    }
                }
            }
        }

        #endregion

        #region Riven: On Cast

        private static void OnCast()
        {
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (!sender.IsMe)
                    return;

                switch (args.SData.Name)
                {
                    case "RivenTriCleave":
                        canmv = false;
                        didq = true;
                        lastq = Environment.TickCount;
                        canq = false;

                        if (!ulton)
                            ssfl = false;

                        // cancel q animation
                        if (qtarg.IsValidTarget(myhitbox + 100))
                        {
                            Utility.DelayAction.Add(100 + Game.Ping / 2,
                                () => player.IssueOrder(GameObjectOrder.MoveTo, movepos));
                        }

                        break;
                    case "RivenMartyr":
                        didw = true;
                        lastw = Environment.TickCount;
                        canw = false;

                        break;
                    case "RivenFeint":
                        dide = true;
                        laste = Environment.TickCount;
                        cane = false;

                        if (menu.Item("fleekey").GetValue<KeyBind>().Active)
                        {
                            if (ulton && r.IsReady() && cleavecount == 2 && q.IsReady())
                            {
                                if (rtarg == null || rtarg.Distance(player.ServerPosition) > r.Range)
                                    r.Cast(Game.CursorPos);
                            }
                        }

                        if (menu.Item("combokey").GetValue<KeyBind>().Active)
                        {
                            if (cleavecount >= 2)
                            {
                                CheckR();
                                Utility.DelayAction.Add(Game.Ping + 100, () => q.Cast(Game.CursorPos));
                            }
                        }

                        break;
                    case "RivenFengShuiEngine":
                        ssfl = true;
                        if (rtarg != null && canburst)
                        {
                            if (!flash.IsReady() || !menubool("multib"))
                                return;

                            var ww = w.IsReady() ? w.Range + 20 : myhitbox;

                            if (menu.Item("combokey").GetValue<KeyBind>().Active)
                            {
                                if (rtarg.Distance(player.ServerPosition) > e.Range + ww &&
                                    rtarg.Distance(player.ServerPosition) <= e.Range + ww + 350)
                                {
                                    player.Spellbook.CastSpell(flash, rtarg.ServerPosition);
                                }
                            }
                        }

                        break;
                    case "rivenizunablade":
                        ssfl = false;
                        didws = true;
                        canws = false;

                        if (q.IsReady() && rtarg.IsValidTarget(1200))
                            q.Cast(rtarg.ServerPosition);

                        break;
                    case "ItemTiamatCleave":
                        lasthd = Environment.TickCount;
                        didhd = true;
                        canws = true;
                        canhd = false;

                        if (menulist("wsmode") == 1 && ulton && canws)
                        {
                            if (menu.Item("combokey").GetValue<KeyBind>().Active)
                            {
                                if (canburst && r.GetPrediction(rtarg).Hitchance >= HitChance.Medium)
                                    Utility.DelayAction.Add(150, () => r.Cast(r.GetPrediction(rtarg).CastPosition));
                            }
                        }

                        if (menulist("emode") == 1 && menu.Item("combokey").GetValue<KeyBind>().Active)
                        {
                            CheckR();
                            Utility.DelayAction.Add(Game.Ping + 175, () => q.Cast(Game.CursorPos));
                        }
                        break;
                }

                if (args.SData.Name.Contains("BasicAttack"))
                {
                    if (menu.Item("combokey").GetValue<KeyBind>().Active)
                    {
                        if (canburst || !menubool("usecombow") || !menubool("usecomboe"))
                        {
                            // delay till after aa
                            Utility.DelayAction.Add(
                                50 + (int)(player.AttackDelay * 100) + Game.Ping / 2 + menuslide("aaq"), delegate
                                {
                                    if (Items.CanUseItem(3077))
                                        Items.UseItem(3077);
                                    if (Items.CanUseItem(3074))
                                        Items.UseItem(3074);
                                });
                        }
                    }

                    else if (menu.Item("clearkey").GetValue<KeyBind>().Active)
                    {
                        if (qtarg.IsValid<Obj_AI_Minion>() && !qtarg.Name.StartsWith("Minion"))
                        {
                            Utility.DelayAction.Add(
                                50 + (int)(player.AttackDelay * 100) + Game.Ping / 2 + menuslide("aaq"), delegate
                                {
                                    if (Items.CanUseItem(3077))
                                        Items.UseItem(3077);
                                    if (Items.CanUseItem(3074))
                                        Items.UseItem(3074);
                                });
                        }
                    }
                }

                if (!didq && args.SData.Name.Contains("BasicAttack"))
                {
                    didaa = true;
                    canaa = false;
                    canq = false;
                    canw = false;
                    cane = false;
                    canws = false;
                    lastaa = Environment.TickCount;
                    qtarg = (Obj_AI_Base)args.Target;
                }
            };
        }

        #endregion

        #region Riven: Misc Events
        private static void Interrupter()
        {
            Interrupter2.OnInterruptableTarget += (sender, args) =>
            {
                if (menubool("wint") && w.IsReady())
                {
                    if (sender.IsValidTarget(w.Range))
                        w.Cast();
                }

                if (menubool("qint") && q.IsReady() && cleavecount >= 2)
                {
                    if (sender.IsValidTarget(q.Range))
                        q.Cast(sender.ServerPosition);
                }
            };
        }

        private static void OnGapcloser()
        {
            AntiGapcloser.OnEnemyGapcloser += gapcloser =>
            {
                if (menubool("wgap") && w.IsReady())
                {
                    if (gapcloser.Sender.IsValidTarget(w.Range))
                        w.Cast();
                }

                if (q.IsReady() && cleavecount == 2)
                {
                    if (gapcloser.Sender.IsValidTarget(q.Range) && !player.IsFacing(gapcloser.Sender))
                    {
                        if (Items.CanUseItem((int)Items.GetWardSlot().Id))
                        {
                            q.Cast(Game.CursorPos);

                            if (didq)
                                Items.UseItem((int)Items.GetWardSlot().Id, gapcloser.Sender.Position);
                        }
                    }
                }
            };
        }

        private void OnPlayAnimation()
        {
            Obj_AI_Base.OnPlayAnimation += (sender, args) =>
            {
                if (!(didq || didw || didws || dide) && !player.IsDead)
                {
                    if (sender.IsMe)
                    {
                        if (args.Animation.Contains("Idle"))
                        {
                            canq = false;
                            canaa = true;
                        }

                        if (args.Animation.Contains("Run"))
                        {
                            canq = false;
                            canaa = true;
                        }
                    }
                }

            };
        }

        #endregion

        #region Riven: Aura

        private static void AuraUpdate()
        {
            if (!player.IsDead)
            {
                foreach (var buff in player.Buffs)
                {
                    if (buff.Name == "RivenTriCleave")
                        cleavecount = buff.Count;

                    if (buff.Name == "rivenpassiveaaboost")
                        passivecount = buff.Count;
                }

                if (player.HasBuff("RivenTriCleave", true))
                {
                    if (Environment.TickCount - lastq >= 3650)
                    {
                        if (!player.IsRecalling() && !player.Spellbook.IsChanneling)
                        {
                            if (menubool("keepq"))
                                q.Cast(Game.CursorPos);
                        }
                    }
                }

                if (!player.HasBuff("rivenpassiveaaboost", true))
                    Utility.DelayAction.Add(1000, () => passivecount = 1);

                if (!player.HasBuff("RivenTriCleave", true))
                    Utility.DelayAction.Add(1000, () => cleavecount = 0);
            }
        }

        #endregion

        #region Riven : Combat/Orbwalk
        private static void OrbTo(Obj_AI_Base target)
        {
            if (canaa && canmv)
            {
                if (!(didq || didw || dide || didaa))
                {
                    canq = false;
                    player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }
        }

        private static void CombatCore()
        {
            if (didhd && canhd && Environment.TickCount - lasthd >= 250)
            {
                didhd = false;
            }

            if (didq)
            {
                if (Environment.TickCount - lastq >= (int)(player.AttackCastDelay * 1000) + 800)
                {
                    if (didq)
                    {
                        didq = false;
                        canmv = true;
                        canaa = true;
                    }
                }
            }

            if (didw)
            {
                if (Environment.TickCount - lastw >= 266)
                {
                    didw = false;
                    canmv = true;
                    canaa = true;
                }
            }

            if (dide)
            {
                if (Environment.TickCount - laste >= 300)
                {
                    dide = false;
                    canmv = true;
                }
            }

            var delay = (int)(player.AttackDelay * 100) + Game.Ping / 2;
            var timer = menulist("cancelt") == 0 ? delay - 30 : delay;
            if (didaa)
            {
                if (Environment.TickCount - lastaa >= +timer + menuslide("aaq"))
                {
                    didaa = false;
                    canmv = true;
                    canq = true;
                    cane = true;
                    canw = true;
                    canws = true;
                }
            }

            if (!canw && w.IsReady())
            {
                if (!(didaa || didq || dide))
                {
                    canw = true;
                }
            }

            if (!cane && e.IsReady())
            {
                if (!(didaa || didq || didw))
                {
                    cane = true;
                }
            }

            if (!canws && r.IsReady())
            {
                if (!(didaa || didw) && ulton)
                {
                    canws = true;
                }
            }

            if (!canaa)
            {
                if (!(didq || didw || dide || didws || didhd))
                {
                    if (Environment.TickCount - lastaa >= 1000)
                    {
                        canaa = true;
                    }
                }
            }

            if (!canmv)
            {
                if (!(didq || didw || dide || didws || didhd))
                {
                    if (Environment.TickCount - lastaa >= 1100)
                    {
                        canmv = true;
                    }
                }
            }
        }

        #endregion

        #region Riven: Math/Damage

        private static float ComboDamage(Obj_AI_Base target)
        {
            if (target == null)
                return 0f;

            var ignote = player.GetSpellSlot("summonerdot");
            var ad = (float)player.GetAutoAttackDamage(target);
            var runicpassive = new[] { 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5 };

            var ra = ad +
                        (float)
                            ((+player.FlatPhysicalDamageMod + player.BaseAttackDamage) *
                            runicpassive[player.Level / 3]);

            var rw = Wdmg(target);
            var rq = Qdmg(target);
            var rr = r.IsReady() ? r.GetDamage(target) : 0;

            var ii = (ignote != SpellSlot.Unknown && player.GetSpell(ignote).State == SpellState.Ready && r.IsReady()
                ? player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)
                : 0);

            var tmt = Items.HasItem(3077) && Items.CanUseItem(3077)
                ? player.GetItemDamage(target, Damage.DamageItems.Tiamat)
                : 0;

            var hyd = Items.HasItem(3074) && Items.CanUseItem(3074)
                ? player.GetItemDamage(target, Damage.DamageItems.Hydra)
                : 0;

            var bwc = Items.HasItem(3144) && Items.CanUseItem(3144)
                ? player.GetItemDamage(target, Damage.DamageItems.Bilgewater)
                : 0;

            var brk = Items.HasItem(3153) && Items.CanUseItem(3153)
                ? player.GetItemDamage(target, Damage.DamageItems.Botrk)
                : 0;

            var items = tmt + hyd + bwc + brk;

            var damage = (rq * 3 + ra * 3 + rw + rr + ii + items);

            return xtra((float)damage);
        }


        private static double Wdmg(Obj_AI_Base target)
        {
            double dmg = 0;
            if (w.IsReady() && target != null)
            {
                dmg += player.CalcDamage(target, Damage.DamageType.Physical,
                    new[] { 50, 80, 110, 150, 170 }[w.Level - 1] + 1 * player.FlatPhysicalDamageMod + player.BaseAttackDamage);
            }

            return dmg;
        }

        private static double Qdmg(Obj_AI_Base target)
        {
            double dmg = 0;
            if (q.IsReady() && target != null)
            {
                dmg += player.CalcDamage(target, Damage.DamageType.Physical,
                    -10 + (q.Level * 20) + (0.35 + (q.Level * 0.05)) * (player.FlatPhysicalDamageMod + player.BaseAttackDamage));
            }

            return dmg;
        }

        public static Vector2 RotateVector(Vector2 start, Vector2 end, float angle)
        {
            angle = angle * ((float)(Math.PI / 180));
            Vector2 ret = end;

            ret.X = ((float) Math.Cos(angle)*(end.X - start.X) -
                     (float) Math.Sin(angle)*(end.Y - start.Y) + start.X);

            ret.Y = ((float) Math.Sin(angle)*(end.X - start.X) +
                     (float) Math.Cos(angle)*(end.Y - start.Y) + start.Y);
            return ret;
        }

        #endregion

        #region Riven: Drawings

        private static void Drawings()
        {
            Drawing.OnDraw += args =>
            {
                if (!player.IsDead)
                {
                    if (menubool("drawengage"))
                    {
                        Render.Circle.DrawCircle(player.Position,
                            player.AttackRange + e.Range + 10, Color.White, 1);
                    }

                    if (menubool("drawburst") && canburst && flash.IsReady())
                    {
                        var ee = e.IsReady() ? e.Range : 0f;
                        var ww = w.IsReady() ? w.Range + 20 : myhitbox;
                        Render.Circle.DrawCircle(player.Position, ee + ww + 300, Color.GreenYellow, 2);
                    }

                    if (menubool("drawtarg") && rtarg != null)
                    {
                        Render.Circle.DrawCircle(rtarg.Position, rtarg.BoundingRadius + 10, Color.Red);
                    }
                }
            };

            Drawing.OnEndScene += args =>
            {
                if (!menubool("drawdmg"))
                    return;

                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(ene => ene.IsValidTarget() && !ene.IsZombie))
                {
                    var color = enemy.Health <= ComboDamage(enemy) / 1.6 ||
                                enemy.CountEnemiesInRange(w.Range) >= menuslide("multic")
                        ? new ColorBGRA(0, 255, 0, 90)
                        : new ColorBGRA(255, 255, 0, 90);

                    hpi.unit = enemy;
                    hpi.drawDmg(ComboDamage(enemy), color);
                }

            };
        }

        #endregion

    }
}