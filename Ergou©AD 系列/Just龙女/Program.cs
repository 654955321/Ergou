using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Color = System.Drawing.Color;
using JustShyvana;

namespace JustShyvana
{
    internal class Program
    {
        public const string ChampName = "Shyvana";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        private static SpellSlot _smiteSlot = SpellSlot.Unknown;
        private static Spell _smite;
        //Credits to Kurisu
        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3726, 3723};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707};
        private static SpellSlot Ignite;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;

        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("JustShyvana - [V.1.0.0.0]", 8000);

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 350f);
            E = new Spell(SpellSlot.E, 925f);
            E.SetSkillshot(0.25f, 60f, 1700, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, 1000f);
            R.SetSkillshot(0.25f, 150f, 1500, false, SkillshotType.SkillshotLine);

            SetSmiteSlot();

            Config = new Menu("Just龙女", "Shyvana", true);
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));

            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "使用 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseS", "使用 惩戒 (红/蓝)").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Rene", "使用R|敌人数量").SetValue(new Slider(2, 1, 5)));

            //Harass
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("hQ", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hW", "使用 W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hE", "使用 E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("AutoHarass", "自动骚扰", true).SetValue(new KeyBind("J".ToCharArray()[0],KeyBindType.Toggle)));
            Config.SubMenu("Harass").AddItem(new MenuItem("aE", "使用 E 自动骚扰").SetValue(true));

            //Item
            Config.AddSubMenu(new Menu("物品", "Item"));
            Config.SubMenu("Item").AddItem(new MenuItem("useGhostblade", "使用幽梦").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("UseBOTRK", "使用破败").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("eL", "  使用破败|敌人血量").SetValue(new Slider(80, 0, 100)));
            Config.SubMenu("Item").AddItem(new MenuItem("oL", "  使用破败|自己血量").SetValue(new Slider(65, 0, 100)));
            Config.SubMenu("Item").AddItem(new MenuItem("UseBilge", "使用小弯刀").SetValue(true));
            Config.SubMenu("Item")
                .AddItem(new MenuItem("HLe", "  使用小弯刀|敌人血量").SetValue(new Slider(80, 0, 100)));
            Config.SubMenu("Item").AddItem(new MenuItem("UseIgnite", "使用点燃").SetValue(true));

            //Laneclear
            Config.AddSubMenu(new Menu("清线", "Clear"));
            Config.SubMenu("Clear").AddItem(new MenuItem("laneQ", "使用 Q").SetValue(true));
            Config.SubMenu("Clear").AddItem(new MenuItem("laneW", "使用 W").SetValue(true));
            Config.SubMenu("Clear").AddItem(new MenuItem("laneE", "使用 E").SetValue(true));

            //Draw
            Config.AddSubMenu(new Menu("范围", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("Draw_Disabled", "禁用所有范围").SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("Qdraw", "显示 Q 范围").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Wdraw", "显示 W 范围").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Edraw", "显示 E 范围").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Rdraw", "显示 R 范围").SetValue(true));

            //Misc
            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KsQ", "使用 Q 抢人头").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KsE", "使用 E 抢人头").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("DrawD", "伤害 指示器").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("interrupt", "中断法术").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("antigap", "防止突进").SetValue(true));

            Config.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat(
                "<font color=\"#FF003C\">JustShyvana - <font color=\"#FFFFFF\"> 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847 !</font>");
            Drawing.OnEndScene += OnEndScene;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && sender.IsValidTarget(R.Range) && Config.Item("interrupt").GetValue<bool>())
                R.CastIfHitchanceEquals(sender, HitChance.High);
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && gapcloser.Sender.IsValidTarget(Q.Range) && Config.Item("antigap").GetValue<bool>())
                W.Cast();
        }

        public static string Smitetype()
        {
            if (SmiteBlue.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(id => Items.HasItem(id)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }


        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("Draw").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.Green);
                }
            }
        }

        private static void combo()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var enemys = Config.Item("Rene").GetValue<Slider>().Value;
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range))
                if (!target.HasBuff("JudicatorIntervention") && !target.HasBuff("Undying Rage") &&
                (Config.Item("Rene").GetValue<Slider>().Value <= enemys))
                    R.CastIfHitchanceEquals(target, HitChance.High);

            UseSmite(target);

            if (W.IsReady() && target.IsValidTarget(W.Range) && Config.Item("UseW").GetValue<bool>())
                W.Cast();

            if (E.IsReady() && target.IsValidTarget(E.Range) && Config.Item("UseE").GetValue<bool>())
                E.CastIfHitchanceEquals(target, HitChance.High);

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
                Q.Cast();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();
        }


        private static int CalcDamage(Obj_AI_Base target)
        {
            var aa = player.GetAutoAttackDamage(target, true)*(1 + player.Crit);
            var damage = aa;
            Ignite = player.GetSpellSlot("summonerdot");

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //ITEM BOTRK

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {
                if (R.IsReady())
                {
                    damage += R.GetDamage(target);
                }
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<KeyBind>().Active) // qdamage
            {

                damage += Q.GetDamage(target);
            }

            if (E.IsReady() && Config.Item("UseE").GetValue<KeyBind>().Active) // edamage
            {

                damage += E.GetDamage(target);
            }

            if (_smite.IsReady() && Config.Item("UseS").GetValue<KeyBind>().Active) // edamage
            {

                damage += GetSmiteDmg();
            }

            if (W.IsReady() && Config.Item("UseW").GetValue<KeyBind>().Active) // wdamage
            {

                damage += W.GetDamage(target);
            }
            return (int) damage;
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float) player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void Killsteal()
        {
            foreach (Obj_AI_Hero target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        hero =>
                            hero.IsValidTarget(Q.Range) && !hero.HasBuffOfType(BuffType.Invulnerability) && hero.IsEnemy)
                )
            {
                var qDmg = player.GetSpellDamage(target, SpellSlot.Q);
                if (Config.Item("ksQ").GetValue<bool>() && target.IsValidTarget(Q.Range) && target.Health <= qDmg)
                {
                    Q.Cast();
                }
                var eDmg = player.GetSpellDamage(target, SpellSlot.E);
                if (Config.Item("ksE").GetValue<bool>() && target.IsValidTarget(E.Range) && target.Health <= eDmg)
                {
                    E.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        private static void items()
        {
            Ignite = player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && target.HealthPercent <= Config.Item("eL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && target.HealthPercent <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(target) &&
                target.HealthPercent <= Config.Item("HLe").GetValue<Slider>().Value
                && Config.Item("UseBilge").GetValue<bool>())

                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(E.Range)
                && Config.Item("useGhostblade").GetValue<bool>())

                Ghost.Cast();

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (player.IsDead || MenuGUI.IsChatOpen || player.IsRecalling())
            {
                return;
            }
            
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
            }

            Killsteal();
            var autoHarass = Config.Item("AutoHarass", true).GetValue<KeyBind>().Active;
            if (autoHarass)
               AutoHarass();
        }

        private static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;
            
            if (E.IsReady() && Config.Item("aE").GetValue<bool>() && target.IsValidTarget(E.Range))
                E.CastIfHitchanceEquals(target, HitChance.High);
        }

        private static void harass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            if (W.IsReady() && Config.Item("hW").GetValue<bool>() && target.IsValidTarget(W.Range))
               W.Cast();

            if (Q.IsReady() && Config.Item("hQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
                Q.Cast();

            if (E.IsReady() && Config.Item("hE").GetValue<bool>() && target.IsValidTarget(E.Range))
                E.CastIfHitchanceEquals(target, HitChance.High);
        }

        private static void Clear()
        {
            var minionObj = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly,
                MinionOrderTypes.MaxHealth);
            
            if (!minionObj.Any())
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Config.Item("laneE").GetValue<bool>()
                &&
                (minionObj.Count > 2 || minionObj.Any(i => i.MaxHealth >= 1200)))
            {
                var pos = E.GetLineFarmLocation(minionObj);
                if (pos.MinionsHit > 0 && E.Cast(pos.Position))
                {
                    return;
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Config.Item("laneQ").GetValue<bool>())
                
            {
                var pos = Q.GetLineFarmLocation(minionObj.Where(i => Q.IsInRange(i)).ToList());
                if (pos.MinionsHit > 0 && Q.Cast())
                {
                    return;
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("laneW").GetValue<bool>())
                {
                var obj = minionObj.Where(i => W.IsInRange(i)).FirstOrDefault(i => i.MaxHealth >= 1200);
                if (obj == null)
                {
                    obj = minionObj.Where(i => W.IsInRange(i)).MinOrDefault(i => i.Health);
                }
                if (obj != null)
                {
                    W.Cast();
                }
            }
        }


        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;
            
            if (Config.Item("Qdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, Q.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Wdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, W.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Edraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, E.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Rdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, R.Range, System.Drawing.Color.White, 3);
        }

        //Credits to metaphorce
        public static void UseSmite(Obj_AI_Hero target)
        {
            var usesmite = Config.Item("UseS").GetValue<bool>();
            var itemscheck = SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i));
            if (itemscheck && usesmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                target.Distance(player.Position) < _smite.Range)
            {
                ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, target);
            }
        }

        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                _smite = new Spell(_smiteSlot, 700);
                return;
            }
        }

        private static int GetSmiteDmg()
        {
            int level = player.Level;
            int index = player.Level/5;
            float[] dmgs = {370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level};
            return (int) dmgs[index];
        }
    }
}
