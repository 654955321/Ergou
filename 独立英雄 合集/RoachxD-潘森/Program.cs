#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Pantheon
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (Variable.Player.BaseSkinName != Variable.CharName)
            {
                return;
            }

            Variable.Q = new Spell(SpellSlot.Q, 600);
            Variable.W = new Spell(SpellSlot.W, 600);
            Variable.E = new Spell(SpellSlot.E, 700);

            Function.SetIgniteSlot();
            Function.SetSmiteSlot();

            Variable.Spells.Add(Variable.Q);
            Variable.Spells.Add(Variable.W);
            Variable.Spells.Add(Variable.E);

            Variable.Config = new Menu("花边汉化-RoachxD潘森", Variable.CharName, true);

            Variable.Config.AddSubMenu(new Menu("连招 设置", "combo"));
            Variable.Config.SubMenu("combo")
                .AddItem(new MenuItem("comboKey", "连招 按键").SetValue(new KeyBind(32, KeyBindType.Press)));
            Variable.Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "爆发时丨使用物品").SetValue(true));
            if (Variable.SmiteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("combo")
                    .AddItem(new MenuItem("autoSmite", "QWE中了之后丨使用惩戒").SetValue(true));
            }

            if (Variable.IgniteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("combo")
                    .AddItem(new MenuItem("autoIgnite", "爆发时丨使用点燃").SetValue(true));
            }

            Variable.Config.AddSubMenu(new Menu("骚扰 设置", "harass"));
            Variable.Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("harassKey", "骚扰 按键").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Variable.Config.SubMenu("harass")
                .AddItem(new MenuItem("hMode", "骚扰 模式: ").SetValue(new StringList(new[] {"Q", "W+E"})));
            Variable.Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("autoQ", "自动Q丨目标在范围内").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Variable.Config.SubMenu("harass")
                .AddItem(new MenuItem("aQT", "目标在塔下丨不自动Q").SetValue(true));
            Variable.Config.SubMenu("harass")
                .AddItem(new MenuItem("harassMana", "最低蓝量 = %").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("打钱 设置", "farm"));
            Variable.Config.SubMenu("farm")
                .AddItem(
                    new MenuItem("farmKey", "打钱 按键").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Variable.Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "打钱时丨使用Q").SetValue(true));
            Variable.Config.SubMenu("farm")
                .AddItem(new MenuItem("wFarm", "打钱时丨使用W").SetValue(true));
            Variable.Config.SubMenu("farm")
                .AddItem(new MenuItem("farmMana", "最低蓝量 = %").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("清野 设置", "jungle"));
            Variable.Config.SubMenu("jungle")
                .AddItem(
                    new MenuItem("jungleKey", "清野 按键").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("jungle")
                .AddItem(new MenuItem("qJungle", "清野时丨使用Q").SetValue(true));
            Variable.Config.SubMenu("jungle")
                .AddItem(new MenuItem("wJungle", "清野时丨使用Q").SetValue(true));
            Variable.Config.SubMenu("jungle")
                .AddItem(new MenuItem("eJungle", "清野时丨使用Q").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("范围 设置", "drawing"));
            Variable.Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "禁止 所有范围").SetValue(false));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("Target", "显示目标 线圈").SetValue(new Circle(true,
                        Color.FromArgb(255, 255, 0, 0))));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("QDraw", "Q 范围").SetValue(new Circle(true,
                        Color.FromArgb(255, 178, 0, 0))));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("WDraw", "W 范围").SetValue(new Circle(false,
                        Color.FromArgb(255, 32, 178, 170))));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("EDraw", "E 范围").SetValue(new Circle(true,
                        Color.FromArgb(255, 128, 0, 128))));

            Variable.Config.AddSubMenu(new Menu("杂项 设置", "misc"));
            Variable.Config.SubMenu("misc")
                .AddItem(new MenuItem("stopChannel", "打断 法术").SetValue(true));
            Variable.Config.SubMenu("misc")
                .AddItem(new MenuItem("usePackets", "使用 封包").SetValue(false));

            Variable.Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));
            Variable.Orbwalker = new Orbwalking.Orbwalker(Variable.Config.SubMenu("Orbwalking"));

            var tsMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Variable.Config.AddSubMenu(tsMenu);

            Variable.Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = Function.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;
            GameObject.OnDelete += Game_OnObjectDelete;

            Game.PrintChat("<font color=\"#00BFFF\">Pantheon# -Loaded</font> <font color=\"#FFFFFF\">花边汉化-RoachxD潘森</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);

            var comboKey = Variable.Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Variable.Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Variable.Config.Item("farmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Variable.Config.Item("jungleKey").GetValue<KeyBind>().Active;

            Variable.Orbwalker.SetAttack(!Function.UsingEorR());
            Variable.Orbwalker.SetMovement(!Function.UsingEorR());

            if (comboKey && target != null)
            {
                Function.Combo(target);
            }
            else
            {
                if (harassKey && target != null)
                {
                    Function.Harass(target);
                }

                if (farmKey)
                {
                    Function.Farm();
                }

                if (jungleClearKey)
                {
                    Function.JungleClear();
                }

                if (!Variable.Config.Item("autoQ").GetValue<KeyBind>().Active || target == null)
                {
                    return;
                }

                if (Variable.Config.Item("aQT").GetValue<bool>()
                    ? !Variable.Player.UnderTurret(true)
                    : Variable.Player.UnderTurret(true) && Variable.Player.Distance(target) <= Variable.Q.Range &&
                      Variable.Q.IsReady())
                {
                    Variable.Q.CastOnUnit(target, Variable.Config.Item("usePackets").GetValue<bool>());
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Variable.Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

            foreach (
                var spell in
                    Variable.Spells.Where(spell => Variable.Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Utility.DrawCircle(Variable.Player.Position, spell.Range,
                    Variable.Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);
            if (Variable.Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Utility.DrawCircle(target.Position, 50, Variable.Config.Item("Target").GetValue<Circle>().Color, 1, 50);
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Variable.Config.Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if (!(Variable.Player.Distance(unit) <= Variable.W.Range) || !Variable.W.IsReady())
            {
                return;
            }

            Variable.W.CastOnUnit(unit, Variable.Config.Item("usePackets").GetValue<bool>());
        }

        private static void Game_OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe)
            {
                return;
            }

            Variable.UsingE = false;
            if (spell.SData.Name != "PantheonE")
            {
                return;
            }

            Variable.UsingE = true;
            Utility.DelayAction.Add(750, () => Variable.UsingE = false);
        }

        private static void Game_OnObjectDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Pantheon_") || !sender.Name.Contains("_E_cas.troy"))
            {
                return;
            }

            Variable.UsingE = false;
        }
    }
}