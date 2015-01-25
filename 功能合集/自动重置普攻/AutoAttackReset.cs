using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoAttackResetter
{
    class AutoAttackReset
    {
        private static Obj_AI_Base Player;
        private static Spell Reset;
        public static Menu Config;
        private Boolean special;
        private Boolean special2;
        private Boolean special3;
        private Boolean correctForm;
        public AutoAttackReset()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            //Player
            switch (ObjectManager.Player.ChampionName)
            {
                case "Jax": Reset = new Spell(SpellSlot.W);
                    break;
                case "Fiora": Reset = new Spell(SpellSlot.E);
                    break;
                case "Poppy": Reset = new Spell(SpellSlot.Q);
                    break;
                case "MasterYi": Reset = new Spell(SpellSlot.W);
                    break;
                case "Talon": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Leona": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Blitzcrank": Reset = new Spell(SpellSlot.E);
                    break;
                case "Darius": Reset = new Spell(SpellSlot.W);
                    break;
                case "Garen": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Hecarim": Reset = new Spell(SpellSlot.E);
                    break;
                case "Jayce": Reset = new Spell(SpellSlot.W);
                    break;
                case "Kassadin": Reset = new Spell(SpellSlot.W);
                    break;
                case "MissFortune": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Mordekaiser": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Nasus": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Nautilus": Reset = new Spell(SpellSlot.W);
                    break;
                case "Nidalee": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Renekton": Reset = new Spell(SpellSlot.W);
                    break;
                case "Riven": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Sejuani": Reset = new Spell(SpellSlot.W);
                    break;
                case "Shyvana": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Sivir": Reset = new Spell(SpellSlot.W);
                    break;
                case "Trundle": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Vi": Reset = new Spell(SpellSlot.E);
                    break;
                case "Volibear": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Wukong": Reset = new Spell(SpellSlot.Q);
                    break;
                case "XinZhao": Reset = new Spell(SpellSlot.Q);
                    break;
                case "Yorick": Reset = new Spell(SpellSlot.Q);
                    break;
                default: Game.PrintChat("Champion not supported");
                    break;

                    switch (ObjectManager.Player.ChampionName)
                    {
                        case "MasterYi": special = true;
                            break;
                        case "Riven": special2 = true;
                            break;
                        case "Nidalee": special3 = true;
                            break;
                        case "Jayce": special3 = true;
                            break;
                    }
            }
            Game.PrintChat(ObjectManager.Player.ChampionName + " Loaded");
            //Menu 
            Config = new Menu("普攻 重置", "AtkReset", true);
            Config.AddItem(new MenuItem("ChampEnabled", "英雄 启用").SetValue(new KeyBind(78, KeyBindType.Toggle)));
            Config.AddItem(new MenuItem("TowerEnabled", "炮塔 启用").SetValue(new KeyBind(77, KeyBindType.Toggle)));
            Config.AddToMainMenu();

            //Events
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (special3 == true)
            {
                if (ObjectManager.Player.ChampionName == "Jayce" && Player.Spellbook.GetSpell(SpellSlot.Q).Name == "ShockBlast")
                {
                    correctForm = true;
                }
                if (ObjectManager.Player.ChampionName == "Nidalee" && Player.Spellbook.GetSpell(SpellSlot.Q).Name == "Takedown")
                {
                    correctForm = true;
                }
            }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && target.IsValidTarget())
            {
                if (target.Type == GameObjectType.obj_AI_Hero && Config.Item("ChampEnabled").GetValue<KeyBind>().Active)
                {
                    if (special == true)
                    {
                        Reset.Cast();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    else if (special2 == true)
                    {
                        Reset.Cast(target.Position);
                    }
                    else if (special3 == true && correctForm)
                    {
                        Reset.Cast();
                    }
                    else if (special3 == true && !correctForm)
                    {
                        correctForm = false;
                    }
                    else
                    {
                        Reset.Cast();
                    }
                }
                else if (target.Type == GameObjectType.obj_AI_Turret && Config.Item("TowerEnabled").GetValue<KeyBind>().Active)
                {
                    if (special == true)
                    {
                        Reset.Cast();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    else if (special2 == true)
                    {
                        Reset.Cast(target.Position);
                    }
                    else if (special3 == true && correctForm)
                    {
                            Reset.Cast();
                    }
                    else if (special3 == true && !correctForm)
                    {
                        correctForm = false;
                    }
                    else
                    {
                        Reset.Cast();
                    }
                }
            }
        }
    }
}
