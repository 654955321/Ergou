using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace FioraRaven
{
    class Fiora
    {
        public static String champName = "Fiora";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu menu;
        public static Obj_AI_Hero tar;
        public static Dictionary<string, SpellSlot> spellData;
        public static DZApi api = new DZApi();
        public static bool firstQ;
        public static float QCastTime;
        static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != champName) return;
            spellData = new Dictionary<string, SpellSlot>();
            menu = new Menu("花边汉化-Dz剑姬","FioraRMenu",true);
            menu.AddSubMenu(new Menu("走 砍","Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalker1"));
            var ts = new Menu("目标 选择","TargetSelector");
            TargetSelector.AddToMenu(ts);
            menu.AddSubMenu(ts);
            menu.AddSubMenu(new Menu("连 招", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "使用 Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "使用 W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "使用 E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "使用 R").SetValue(true));
            menu.AddSubMenu(new Menu("杂 项", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("WBlock", "使用 W丨反弹攻击").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("RDodge", "使用 R丨躲避危险技能").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("SecondQDelay", "第二个 Q 延迟 (毫秒)").SetValue(new Slider(650, 0, 4000)));
            menu.AddSubMenu(new Menu("物 品", "Item"));
            menu.SubMenu("Item").AddItem(new MenuItem("Botrk", "使用 破败").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("Youmuu", "使用 幽梦").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("Tiamat", "使用 提亚玛特").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("Hydra", "使用 九头蛇").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("OwnHPercBotrk", "使用破败丨自己血量").SetValue(new Slider(50, 1, 100)));
            menu.SubMenu("Item").AddItem(new MenuItem("EnHPercBotrk", "使用破表丨敌人血量").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("Item").AddItem(new MenuItem("ItInComb", "连招时 使用物品").SetValue(true));
            menu.AddSubMenu(new Menu("危险的法术", "DangSpells"));
            Dictionary<String, String> dSpellsDName = api.getDanSpellsName();
            foreach (KeyValuePair<string, string> entry in dSpellsDName)
            {
                menu.SubMenu("DangSpells").AddItem(new MenuItem(entry.Key, entry.Value).SetValue(true));
            }
            menu.AddSubMenu(new Menu("范 围", "Drawing"));
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrQ", "Q 范围").SetValue(true));
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrR", "R 范围").SetValue(true));
           
            Game.PrintChat("Fiora The Raven By DZ191 Loaded 涓ㄦ眽鍖朆y 鑺辫竟");
            menu.AddToMainMenu();
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, float.MaxValue);
            E = new Spell(SpellSlot.E, float.MaxValue);
            R = new Spell(SpellSlot.R, 400f);
            
        }
        static void Drawing_OnDraw(EventArgs args)
        {
            if (isEn("DrQ"))
            {
                Render.Circle.DrawCircle(player.Position, Q.Range, Color.MediumPurple);
            }
            if (isEn("DrR"))
            {
                Render.Circle.DrawCircle(player.Position, R.Range, Color.MediumPurple);
            }
        }
       
        public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            String name = args.SData.Name;
            Obj_AI_Hero tar = (Obj_AI_Hero)hero;
            GameObjectProcessSpellCastEventArgs spell = args;
            if(api.getDanSpellsName().ContainsKey(args.SData.Name) && isEn(name))
            {
                //Got Dangerous Spell. Starting Predictions and Custom Evade Logics.
                if(name == "CurseofTheSadMummy")
                {
                    if(player.Distance(hero.Position)<=600f)
                    {
                        Obj_AI_Hero tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }
                if(name == "InfernalGuardian" || name == "UFSlash")
                {
                    if (player.Distance(spell.End)<=270f)
                    {
                        Obj_AI_Hero tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }
                if (name == "BlindMonkRKick" || name == "syndrar" || name == "VeigarPrimordialBurst" || name == "AlZaharNetherGrasp")
                {
                    if (spell.Target.IsMe)
                    {
                        Obj_AI_Hero tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }
                if (name == "BusterShot" || name == "ViR")
                {
                    if (spell.Target.IsMe || player.Distance(spell.Target.Position)<=50f)
                    {
                        Obj_AI_Hero tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }
                
                if (name == "GalioIdolOfDurand")
                {
                    if (player.Distance(hero.Position) <= 600f)
                    {
                        Obj_AI_Hero tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }
            }
            if(spell.SData.Name.Contains("Attack") && isEn("WBlock") && spell.Target.IsMe)
            {
                    W.Cast();      
            }
        }
        static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                tar = (Obj_AI_Hero)target;
                if (isEn("Botrk") && isCombo() && target.IsValidTarget()&&isEn("ItInComb"))
                {
                    float OwnH = api.getPlHPer();
                    if ((menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= OwnH) && ((menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= api.getEnH(tar))))
                    {
                        api.useItem(3153, tar);
                    }
                }
                if (isEn("Youmuu") && isCombo() && target.IsValidTarget() && isEn("ItInComb"))
                {
                    api.useItem(3142, tar);
                }
            }
        }
        public static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                tar = (Obj_AI_Hero)target;
                if (isCombo() && E.IsReady() && target.IsValidTarget())
                {
                    E.Cast();
                }
                if (menu.Item("Tiamat").GetValue<bool>() && isCombo())
                {
                    int itemId = 3077;
                    api.useItem(itemId);
                }
                if (menu.Item("Hydra").GetValue<bool>() && isCombo())
                {
                    int itemId = 3074;
                    api.useItem(itemId);
                }
                firstQ = false;
            }
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Q.IsReady())firstQ = false;
            if (isCombo()) { 
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                CastQ(target);
                if ((R.GetDamage(target) >= target.Health))
                {
                    CastR(target);
                }
            } 
        }
        public static void CastQ(Obj_AI_Hero target)
        {        
            if (!target.IsValidTarget()) return;
            if(target.IsValidTarget(Q.Range) && Q.IsReady()&&Q.IsInRange(target.ServerPosition) && !firstQ && isEn("UseQ") && (Game.Time-QCastTime)>=(menu.Item("SecondQDelay").GetValue<Slider>().Value/1000))
            {
                Q.Cast(target, true, false);
                firstQ = true;
                QCastTime = Game.Time;
            }  
        }
        public static void CastR(Obj_AI_Hero target)
        {
            if (isCombo() && target.IsValidTarget() && R.IsInRange(target.ServerPosition) && isEn("UseR"))
            {
                R.Cast(target,true);
            }
        }
        public static bool isCombo()
        {
            return Orbwalker.ActiveMode.ToString() == "Combo";
        }
        public static bool isEn(String item)
        {
            return menu.Item(item).GetValue<bool>();
        }
        
    }
}
