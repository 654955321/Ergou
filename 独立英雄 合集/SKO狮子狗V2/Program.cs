using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SKO_Rengar_V2
{
    class Program
    {
        private static Obj_AI_Hero player;
        private static Spell Q, W, E, R;
        private static Items.Item BWC, BRK, RO, YMG, STD, TMT, HYD, DFG;
        private static bool PacketCast;
        private static Menu SKOMenu;
        private static SpellSlot IgniteSlot, TeleportSlot, smiteSlot;
        private static Obj_AI_Hero target;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            if (player.ChampionName != "Rengar")
                return;

            SKOMenu = new Menu("带头大哥汉化-SKO狮子狗V2", "SKORengar", true);

            var SKOTs = new Menu("目标 选择", "TargetSelector");
            TargetSelector.AddToMenu(SKOTs);

            var OrbMenu = new Menu("走 砍", "Orbwalker");
            LXOrbwalker.AddToMenu(OrbMenu);


            var Combo = new Menu("连 招", "Combo");
            Combo.AddItem(new MenuItem("CPrio", "优先 使用").SetValue(new StringList(new[] { "Q", "W", "E" }, 2)));
            Combo.AddItem(new MenuItem("UseQ", "使用 Q").SetValue(true));
            Combo.AddItem(new MenuItem("UseW", "使用 W").SetValue(true));
            Combo.AddItem(new MenuItem("UseE", "使用 E").SetValue(true));
            Combo.AddItem(new MenuItem("UseEEm", "假如目标离开Q范围丨使用E束缚").SetValue(false));
            Combo.AddItem(new MenuItem("UseItemsCombo", "使用 项目").SetValue(true));
            Combo.AddItem(new MenuItem("UseAutoW", "自动 W").SetValue(true));
            Combo.AddItem(new MenuItem("HpAutoW", "最低 hp").SetValue(new Slider(20, 1, 100)));
            Combo.AddItem(new MenuItem("TripleQ", "三重 Q").SetValue(new KeyBind(OrbMenu.Item("Flee_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Combo.AddItem(new MenuItem("activeCombo", "连招").SetValue(new KeyBind(OrbMenu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var items = new Menu("项目", "Items");
            items.AddItem(new MenuItem("Hydra", "九头蛇").SetValue(true));
            items.AddItem(new MenuItem("BOTRK", "破败").SetValue(true));
            items.AddItem(new MenuItem("RO", "蓝盾").SetValue(true));
            items.AddItem(new MenuItem("SOD", "神圣之剑").SetValue(true));
            items.AddItem(new MenuItem("YMU", "幽梦之灵").SetValue(true));
            items.AddItem(new MenuItem("DFG", "冥火").SetValue(true));

            var Harass = new Menu("骚扰", "Harass");
            Harass.AddItem(new MenuItem("HPrio", "优先 使用").SetValue(new StringList(new[] { "W", "E" }, 1)));
            Harass.AddItem(new MenuItem("UseWH", "使用W").SetValue(true));
            Harass.AddItem(new MenuItem("UseEH", "使用E").SetValue(true));
            Harass.AddItem(new MenuItem("UseItemsHarass", "使用项目").SetValue(true));
            Harass.AddItem(new MenuItem("activeHarass", "骚扰").SetValue(new KeyBind(OrbMenu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var JLClear = new Menu("清野/清线", "JLClear");
            JLClear.AddItem(new MenuItem("FPrio", "优先 使用").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));
            JLClear.AddItem(new MenuItem("UseQC", "使用Q").SetValue(true));
            JLClear.AddItem(new MenuItem("UseWC", "使用W").SetValue(true));
            JLClear.AddItem(new MenuItem("UseEC", "使用E").SetValue(true));
            JLClear.AddItem(new MenuItem("Save", "保持 凶暴值").SetValue(false));
            JLClear.AddItem(new MenuItem("UseItemsClear", "使用项目").SetValue(true));
            JLClear.AddItem(new MenuItem("activeClear", "开 启!").SetValue(new KeyBind(OrbMenu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var TROLLZINHONASRANKEDS = new Menu("抢 人头", "TristanaKillZiggs");
            TROLLZINHONASRANKEDS.AddItem(new MenuItem("Foguinho", "使用点燃").SetValue(true));
            TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseQKs", "使用Q").SetValue(true));
            TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseWKs", "使用W").SetValue(true));
            TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseEKs", "使用E").SetValue(true));
            TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseFlashKs", "使用闪现抢人头(技能冷却后)").SetValue(false));

            var CHUPARUNSCUEPA = new Menu("范 围", "Drawing");
            CHUPARUNSCUEPA.AddItem(new MenuItem("DrawQ", "Q范围").SetValue(true));
            CHUPARUNSCUEPA.AddItem(new MenuItem("DrawW", "W范围").SetValue(true));
            CHUPARUNSCUEPA.AddItem(new MenuItem("DrawE", "E范围").SetValue(true));
            CHUPARUNSCUEPA.AddItem(new MenuItem("DrawR", "R范围").SetValue(true));
            CHUPARUNSCUEPA.AddItem(new MenuItem("CircleLag", "自由 延迟").SetValue(true));
            CHUPARUNSCUEPA.AddItem(new MenuItem("CircleWidth", "线圈 宽度").SetValue(new Slider(1, 1, 100)));

            var Misc = new Menu("杂 项", "Misc");
            Misc.AddItem(new MenuItem("UsePacket", "使用 封包").SetValue(true));
            Misc.AddItem(new MenuItem("TpREscape", "R + TP 逃跑").SetValue<KeyBind>(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Game.PrintChat("<font color='#07B88C'>SKO Rengar V2</font> Loaded!");

            SKOMenu.AddSubMenu(SKOTs);
            SKOMenu.AddSubMenu(OrbMenu);
            SKOMenu.AddSubMenu(Combo);
            SKOMenu.AddSubMenu(items);
            SKOMenu.AddSubMenu(Harass);
            SKOMenu.AddSubMenu(JLClear);
            SKOMenu.AddSubMenu(TROLLZINHONASRANKEDS);
            SKOMenu.AddSubMenu(CHUPARUNSCUEPA);
            SKOMenu.AddSubMenu(Misc);
            SKOMenu.AddToMainMenu();

            W = new Spell(SpellSlot.W, 450f);
            E = new Spell(SpellSlot.E, 980f);
            R = new Spell(SpellSlot.R, 2000f);

            E.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);

            HYD = new Items.Item(3074, 420f);
            TMT = new Items.Item(3077, 420f);
            BRK = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            RO = new Items.Item(3143, 500f);
            DFG = new Items.Item(3128, 750f);


            IgniteSlot = player.GetSpellSlot("SummonerDot");
            TeleportSlot = player.GetSpellSlot("SummonerTeleport");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Draw_OnDraw;
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            
            PacketCast = SKOMenu.Item("UsePacket").GetValue<bool>();

            TpREscape();

            
            switch (LXOrbwalker.CurrentMode)
            {             
                case LXOrbwalker.Mode.Flee:
                    if (!player.HasBuff("RengarR") && R.IsReady())
                    {
                        target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    }else{
                        target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                    }
                    break;
                default:
                    target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    break;

            }

            if (SKOMenu.Item("TripleQ").GetValue<KeyBind>().Active)
            {
                TripleQ(target);
            }

            if (SKOMenu.Item("activeClear").GetValue<KeyBind>().Active)
            {
                Clear();
            }

            Q = new Spell(SpellSlot.Q, player.AttackRange + 100);
            YMG = new Items.Item(3142, player.AttackRange + 50);
            STD = new Items.Item(3131, player.AttackRange + 50);

            AutoHeal();
            KillSteal(target);

            if (SKOMenu.Item("activeCombo").GetValue<KeyBind>().Active)
            {
                if (player.Mana <= 4)
                {
                    if (SKOMenu.Item("UseQ").GetValue<bool>() && player.Distance(target) <= Q.Range)
                    {
                        CastQ(target);
                    }
                    if (SKOMenu.Item("UseW").GetValue<bool>() && player.Distance(target) <= W.Range)
                    {
                        CastW(target);
                    }
                    if (SKOMenu.Item("UseE").GetValue<bool>() && player.Distance(target) <= E.Range)
                    {
                        CastE(target);
                    }
                }

                if (player.Mana == 5)
                {
                    if (SKOMenu.Item("UseQ").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 0 && player.Distance(target) <= Q.Range)
                    {
                        CastQ(target);
                    }
                    if (SKOMenu.Item("UseW").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 1 && player.Distance(target) <= W.Range)
                    {
                        CastW(target);
                    }
                    if (SKOMenu.Item("UseE").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 2 && player.Distance(target) <= E.Range)
                    {
                        CastE(target);
                    }

                    //E if !Q.IsReady()
                    if (SKOMenu.Item("UseEEm").GetValue<bool>() && player.Distance(target) > Q.Range + 100f)
                    {
                        CastE(target);
                    }
                    
                }
                UseItems(target);
            }
            if (SKOMenu.Item("activeHarass").GetValue<KeyBind>().Active)
            {
                Harass();
            }
                        

        }

        private static void Draw_OnDraw(EventArgs args)
        {
            if (SKOMenu.Item("CircleLag").GetValue<bool>())
            {
                if (SKOMenu.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(player.ServerPosition, Q.Range, Color.White, SKOMenu.Item("CircleWidth").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawW").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(player.ServerPosition, W.Range, Color.White, SKOMenu.Item("CircleWidth").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(player.ServerPosition, E.Range, Color.White, SKOMenu.Item("CircleWidth").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(player.ServerPosition, R.Range, Color.White, SKOMenu.Item("CircleWidth").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (SKOMenu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.ServerPosition, Q.Range, Color.Green);
                }
                if (SKOMenu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.ServerPosition, W.Range, Color.Green);
                }
                if (SKOMenu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.ServerPosition, E.Range, Color.Green);
                }
                if (SKOMenu.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.ServerPosition, R.Range, Color.Green);
                }
            }
        }

        private static void KillSteal(Obj_AI_Hero unitHero)
        {
            if (!unitHero.IsValidTarget()) return;
            var igniteDmg = player.GetSummonerSpellDamage(unitHero, Damage.SummonerSpell.Ignite);
            var qDmg = player.GetSpellDamage(unitHero, SpellSlot.Q);
            var wDmg = player.GetSpellDamage(unitHero, SpellSlot.Q);
            var eDmg = player.GetSpellDamage(unitHero, SpellSlot.Q);


                if (SKOMenu.Item("Foguinho").GetValue<bool>() && player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > unitHero.Health && player.Distance(unitHero) < 600)
                    {
                        player.Spellbook.CastSpell(IgniteSlot, unitHero);
                    }
                }

            if (SKOMenu.Item("UseQKs").GetValue<bool>())
            {
                if (qDmg > unitHero.Health && player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
            }
            if (SKOMenu.Item("UseWKs").GetValue<bool>())
            {
                if (wDmg > unitHero.Health && player.Distance(unitHero) <= W.Range)
                {
                    CastW(unitHero);
                }
            }
            if (SKOMenu.Item("UseEKs").GetValue<bool>())
            {
                if (eDmg > unitHero.Health && player.Distance(unitHero) <= E.Range)
                {
                    CastE(unitHero);
                }
            }
        }


        private static void TpREscape()
        {
            if (!SKOMenu.Item("TpREscape").GetValue<KeyBind>().Active) return;

            if (R.IsReady() && player.Spellbook.CanUseSpell(TeleportSlot) == SpellState.Ready)
            {
                R.Cast();

                foreach (Obj_AI_Turret turrenttp in ObjectManager.Get<Obj_AI_Turret>().Where(turrenttp => turrenttp.IsAlly && turrenttp.Name == "Turret_T1_C_02_A" || turrenttp.Name == "Turret_T2_C_01_A"))
                {
                    player.Spellbook.CastSpell(TeleportSlot, turrenttp);
                }
            }
        }

        private static void Clear()
        {
            var allminions = MinionManager.GetMinions(player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            foreach (var minion in allminions.Where(minion => minion.IsValidTarget()))
            {
                if (player.Mana <= 4)
                {
                    if (Q.IsReady() && SKOMenu.Item("UseQC").GetValue<bool>() && player.Distance(minion) <= Q.Range)
                    {
                        Q.Cast();
                    }
                    if (W.IsReady() && SKOMenu.Item("UseWC").GetValue<bool>() && player.Distance(minion) <= W.Range)
                    {
                        W.Cast();
                    }
                    if (E.IsReady() && SKOMenu.Item("UseEC").GetValue<bool>() && player.Distance(minion) <= E.Range)
                    {
                        E.Cast(minion, PacketCast);
                    }
                }
                if (player.Mana == 5)
                {
                    if (SKOMenu.Item("Save").GetValue<bool>()) return;
                    if (Q.IsReady() && SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 0 && SKOMenu.Item("UseQC").GetValue<bool>() && player.Distance(minion) <= Q.Range)
                    {
                        Q.Cast();
                    }
                    if (W.IsReady() && SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 1 && SKOMenu.Item("UseWC").GetValue<bool>() && player.Distance(minion) <= W.Range)
                    {
                        W.Cast();
                    }
                    if (E.IsReady() && SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 2 && SKOMenu.Item("UseEC").GetValue<bool>() && player.Distance(minion) <= E.Range)
                    {
                        E.Cast(minion, PacketCast);
                    }
                }
                UseItems(minion, true);
            }
        }

        private static void Harass()
        {
            var unitHero = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            if (!unitHero.IsValidTarget()) return;

                if (player.Mana <= 4)
                {
                    if (SKOMenu.Item("UseWH").GetValue<bool>() && player.Distance(unitHero) <= W.Range)
                    {
                        CastW(unitHero);
                    }
                    if (SKOMenu.Item("UseEH").GetValue<bool>() && player.Distance(unitHero) <= E.Range)
                    {
                        CastE(unitHero);
                    }
                }
                if (player.Mana == 5)
                {
                    if (SKOMenu.Item("UseWH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 0)
                    {
                        CastW(unitHero);
                    }
                    if (SKOMenu.Item("UseEH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 1)
                    {
                        CastE(unitHero);
                    }
                }
            UseItems(unitHero);
        }

        private static void TripleQ(Obj_AI_Hero unitHero)
        {
            if(!unitHero.IsValidTarget()) return;

                if (player.Mana == 5 && R.IsReady() && player.Distance(unitHero) <= R.Range && Q.IsReady())
                {
                    R.Cast();
                }
                if (player.Mana == 5 && player.HasBuff("RengarR") && player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
                if (player.Mana == 5 && !player.HasBuff("RengarR") && player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
                if (player.Mana <= 4)
                {
                    if (player.Distance(unitHero) <= Q.Range)
                    {
                        CastQ(unitHero);
                    }
                    if (player.Distance(unitHero) <= W.Range)
                    {
                        CastW(unitHero);
                    }
                    if (player.Distance(unitHero) <= E.Range)
                    {
                        CastE(unitHero);
                    }
                }
            UseItems(unitHero);
        }

        private static void AutoHeal()
        {
            if (player.HasBuff("Recall") || player.Mana <= 4) return;

            if (SKOMenu.Item("UseAutoW").GetValue<bool>())
            {
                if (W.IsReady() && player.Health < (player.MaxHealth * (SKOMenu.Item("HpAutoW").GetValue<Slider>().Value) / 100))
                {
                    W.Cast();
                }
            }
        }

        private static void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !target.IsValidTarget(Q.Range)) return;
            try
            {
                //if(LXOrbwalker.IsAutoAttackReset(_player.Spellbook.GetSpell(SpellSlot.Q).SData.Name))
                //Utility.DelayAction.Add(260, LXOrbwalker.ResetAutoAttackTimer);
                LXOrbwalker.ResetAutoAttackTimer();
                Q.Cast(PacketCast);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
            }
        }

        private static void CastW(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !target.IsValidTarget(W.Range)) return;
            try
            {
                W.Cast(PacketCast);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !target.IsValidTarget(E.Range)) return;
            try
            {
                var epred = E.GetPrediction(target);
                if (epred.Hitchance >= HitChance.High)
                {
                    E.Cast(epred.CastPosition, PacketCast);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        private static void UseItems(Obj_AI_Base unit, bool isMinion = false)
        {
            if (!unit.IsValidTarget()) return;


            if (SKOMenu.Item("Hydra").GetValue<bool>() && player.Distance(unit) < HYD.Range)
            {
                HYD.Cast();
            }
            if (SKOMenu.Item("Hydra").GetValue<bool>() && player.Distance(unit) < TMT.Range)
            {
                TMT.Cast();
            }
            if (SKOMenu.Item("BOTRK").GetValue<bool>() && player.Distance(unit) <= BRK.Range)
            {
                if (isMinion) return;
                BRK.Cast(unit);
            }
            if (SKOMenu.Item("BOTRK").GetValue<bool>() && player.Distance(unit) <= BWC.Range)
            {

                BWC.Cast(unit);
            }
            if (SKOMenu.Item("RO").GetValue<bool>() && player.Distance(unit) <= RO.Range)
            {
                if (isMinion) return;
                RO.Cast();
            }
            if (SKOMenu.Item("DFG").GetValue<bool>() && player.Distance(unit) <= DFG.Range)
            {
                if (isMinion) return;
                DFG.Cast(unit);
            }
            if (SKOMenu.Item("YMU").GetValue<bool>() && player.Distance(unit) <= YMG.Range)
            {
                YMG.Cast();
            }
            if (SKOMenu.Item("SOD").GetValue<bool>() && player.Distance(unit) <= STD.Range)
            {
                STD.Cast();
            }
        }

    }
}
