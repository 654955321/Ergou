using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Oracle.Extensions
{
    internal class Summoners
    {
        private static bool _isjungling;
        private static string _smiteslot;
        private static Menu _menuconfig, _mainmenu;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public static readonly string[] SmallMinions =
        {
            "SRU_Murkwolf",
            "SRU_Razorbeak",
            "SRU_Krug",
            "Sru_Crab",
            "SRU_Gromp"
        };

        public static readonly string[] EpicMinions =
        {
            "TT_Spiderboss",
            "SRU_Baron",
            "SRU_Dragon"
        };

        public static readonly string[] LargeMinions =
        {
            "SRU_Blue",
            "SRU_Red",
            "TT_NWraith",
            "TT_NGolem",
            "TT_NWolf"
        };

        public static readonly int[] SmiteAll =
        {
            3713, 3726, 3725, 3724, 3723,
            3711, 3722, 3721, 3720, 3719,
            3715, 3718, 3717, 3716, 3714,
            3706, 3710, 3709, 3708, 3707,
        };

        private static float ddmg;
        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3724, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        public void Initialize(Menu root)
        {
            _mainmenu = new Menu("Summoners", "summoners");
            _isjungling = SmiteAll.Any(x => Items.HasItem(x));

            var smite = Me.GetSpellSlot("summonersmite");
            if (smite != SpellSlot.Unknown || _isjungling)
            {
                _menuconfig = new Menu("Smite Config", "sconfig");

                foreach (var x in SmallMinions)
                    _menuconfig.AddItem(new MenuItem("smiteon" + x, "Dont use on " + x)).SetValue(false);

                _mainmenu.AddSubMenu(_menuconfig);

                var Smite = new Menu("Smite", "msmite");
                Smite.AddItem(new MenuItem("usesmite", "Use Smite")).SetValue(new KeyBind('M', KeyBindType.Toggle));
                Smite.AddItem(new MenuItem("smitespell", "Use Smite + Spell")).SetValue(true);
                Smite.AddItem(new MenuItem("drawsmite", "Draw Smite Range")).SetValue(true);
                Smite.AddItem(new MenuItem("drawstatus", "Draw Smite Status")).SetValue(true);
                Smite.AddItem(new MenuItem("smitesmall", "Smite Small Camps")).SetValue(true);
                Smite.AddItem(new MenuItem("smitelarge", "Smite Large Camps")).SetValue(true);
                Smite.AddItem(new MenuItem("smitesuper", "Smite Epic Camps")).SetValue(true);
                Smite.AddItem(new MenuItem("savesmite", "Save a Smite Charge").SetValue(true));
                Smite.AddItem(new MenuItem("smitemode", "Smite Enemies: "))
                    .SetValue(new StringList(new[] { "Killsteal", "Combo", "Nope"}));
                _mainmenu.AddSubMenu(Smite);

                Drawing.OnDraw += args =>
                {
                    if (_mainmenu.Item("drawstatus").GetValue<bool>())
                    {
                        var worldToScreen = Drawing.WorldToScreen(Me.Position);

                        if (_mainmenu.Item("usesmite").GetValue<KeyBind>().Active)
                            Drawing.DrawText(worldToScreen[0] - 25, worldToScreen[1] + 25, Color.SpringGreen, "Smite: ON");
                        else
                            Drawing.DrawText(worldToScreen[0] - 25, worldToScreen[1] + 25, Color.Red, "Smite: OFF");
                    }

                    if (!_mainmenu.Item("usesmite").GetValue<KeyBind>().Active || Me.IsDead)
                        return;

                    if (_mainmenu.Item("drawsmite").GetValue<bool>() && !Me.IsDead)
                        Render.Circle.DrawCircle(Me.Position, 500, Color.SpringGreen, 2);


                    var height = 6;
                    var width = 150;
                    var yoffset = 20;
                    var xoffset = -7;

                    foreach (
                        var minion in
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    th =>
                                        (LargeMinions.Any(x => th.Name.StartsWith(x)) ||
                                         EpicMinions.Any(e => th.Name.StartsWith(e))) && !th.Name.Contains("Mini")))
                    {
                        if (!minion.IsValidTarget(1000) || !minion.IsHPBarRendered)
                        {
                            continue;
                        }

                        var barPos = minion.HPBarPosition;
                        var snmite = Me.GetSpellSlot(_smiteslot).IsReady()
                            ? Me.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite)
                            : 0;

                        var damage = snmite + ddmg;
                        var pctafter = Math.Max(0, minion.Health - damage) / minion.MaxHealth;

                        var yaxis = barPos.Y + yoffset;
                        var xaxisdmg = (float)(barPos.X + xoffset + width * pctafter);
                        var xaxisnow = barPos.X + xoffset + width * minion.Health / minion.MaxHealth;

                        var ana = xaxisnow - xaxisdmg;
                        var pos = barPos.X + xoffset + 12 + (138 * pctafter);

                        for (var i = 0; i < ana; i++)
                        {
                            Drawing.DrawLine((float)pos + i, yaxis, (float)pos + i, yaxis + height, 1, Color.SpringGreen);
                        }
                    }

                };

                Game.OnUpdate += args =>
                {
                    FindSmite();
                    CheckSmite();
                };
            }

            var ignite = Me.GetSpellSlot("summonerdot");
            if (ignite != SpellSlot.Unknown)
            {
                _menuconfig = new Menu("Ignite Config", "iconfig");

                foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                    _menuconfig.AddItem(new MenuItem("igniteon" + x.ChampionName, "Use for " + x.ChampionName))
                        .SetValue(true);

                _mainmenu.AddSubMenu(_menuconfig);

                var Ignite = new Menu("Ignite", "mignite");
                Ignite.AddItem(new MenuItem("useignite", "Enable Ignite")).SetValue(true);
                Ignite.AddItem(new MenuItem("dotmode", "Mode: ")).SetValue(new StringList(new[] {"Killsteal", "Combo"}));
                _mainmenu.AddSubMenu(Ignite);

                Game.OnUpdate += args =>
                {
                    CheckIgnite();
                };
            }

            var heal = Me.GetSpellSlot("summonerheal");
            if (heal != SpellSlot.Unknown)
            {
                 _menuconfig = new Menu("Heal Config", "hconfig");

                foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                    _menuconfig.AddItem(new MenuItem("healon" + x.ChampionName, "Use for " + x.ChampionName))
                        .SetValue(true);

                _mainmenu.AddSubMenu(_menuconfig);

                var Heal = new Menu("Heal", "mheal");
                Heal.AddItem(new MenuItem("useheal", "Enable Heal")).SetValue(true);
                Heal.AddItem(new MenuItem("usehealpct", "Heal on min HP % ")).SetValue(new Slider(20, 1));
                Heal.AddItem(new MenuItem("usehealdmg", "Heal on Dmg dealt %")).SetValue(new Slider(40, 1));
                _mainmenu.AddSubMenu(Heal);

                Game.OnUpdate += args =>
                {
                    CheckHeal(Oracle.IncomeDamage);
                };
            }

            var clarity = Me.GetSpellSlot("summonermana");
            if (clarity != SpellSlot.Unknown)
            {
                _menuconfig = new Menu("Clarity Config", "cconfig");

                foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                    _menuconfig.AddItem(new MenuItem("clarityon" + x.ChampionName, "Use for " + x.ChampionName))
                        .SetValue(true);

                _mainmenu.AddSubMenu(_menuconfig);

                var Clarity = new Menu("Clarity", "mclarity");
                Clarity.AddItem(new MenuItem("useclarity", "Enable Clarity")).SetValue(true);
                Clarity.AddItem(new MenuItem("useclaritypct", "Clarity on Mana % ")).SetValue(new Slider(40, 1));
                _mainmenu.AddSubMenu(Clarity);

                Game.OnUpdate += args =>
                {
                    CheckClarity();
                };
            }

            var barrier = Me.GetSpellSlot("summonerbarrier");
            if (barrier != SpellSlot.Unknown)
            {
                _menuconfig = new Menu("Barrier Config", "bconfig");

                foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsMe))
                    _menuconfig.AddItem(new MenuItem("barrieron" + x.ChampionName, "Use for " + x.ChampionName))
                        .SetValue(true);

                _mainmenu.AddSubMenu(_menuconfig);

                var Barrier = new Menu("Barrier", "mbarrier");
                Barrier.AddItem(new MenuItem("usebarrier", "Enable Barrier")).SetValue(true);
                Barrier.AddItem(new MenuItem("usebarrierpct", "Barrior on min HP % ")).SetValue(new Slider(20, 1));
                Barrier.AddItem(new MenuItem("usebarrierdmg", "Barrier on Dmg dealt %")).SetValue(new Slider(40, 1));
                Barrier.AddItem(new MenuItem("barrierdanger", "Use on Dangerous Spells (Ults)")).SetValue(true);
                Barrier.AddItem(new MenuItem("barrierdot", "Use on Ignite")).SetValue(true);
                _mainmenu.AddSubMenu(Barrier);

                Game.OnUpdate += args =>
                {
                    CheckBarrier(Oracle.IncomeDamage);
                };
            }

            var exhaust = Me.GetSpellSlot("summonerexhaust");
            if (exhaust != SpellSlot.Unknown)
            {
                _menuconfig = new Menu("Exhaust Config", "econfig");

                foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                    _menuconfig.AddItem(new MenuItem("exhauston" + x.ChampionName, "Use for " + x.ChampionName))
                        .SetValue(true);

                _mainmenu.AddSubMenu(_menuconfig);

                var Exhaust = new Menu("Exhaust", "mexhaust");
                Exhaust.AddItem(new MenuItem("useexhaust", "Enable Exhaust")).SetValue(true);
                Exhaust.AddItem(new MenuItem("aexhaustpct", "Exhaust on ally HP %")).SetValue(new Slider(35));
                Exhaust.AddItem(new MenuItem("eexhaustpct", "Exhaust on enemy HP %")).SetValue(new Slider(35));
                Exhaust.AddItem(new MenuItem("exhdanger", "Use on Dangerous")).SetValue(true);
                Exhaust.AddItem(new MenuItem("exhaustmode", "Mode: ")).SetValue(new StringList(new[] {"Always", "Combo"}));
                _mainmenu.AddSubMenu(Exhaust);

                Game.OnUpdate += args =>
                {
                    CheckExhaust();
                };
            }

            root.AddSubMenu(_mainmenu);
        }

        #region Ignite

        private static void CheckIgnite()
        {
            var ignite = Me.GetSpellSlot("summonerdot");
            if (ignite == SpellSlot.Unknown)
                return;

            if (!_mainmenu.Item("useignite").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
                return;

            if (_mainmenu.Item("dotmode").GetValue<StringList>().SelectedIndex == 0)
            {
                foreach (
                    var target in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(target => target.IsValidTarget(600) && !target.IsZombie)
                            .Where(target => !target.HasBuff("summonerdot", true)))
                {

                    var tHealthPercent = target.Health/target.MaxHealth*100;
                    if (target.Health <= Me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) &&
                        _mainmenu.Item("igniteon" + target.ChampionName).GetValue<bool>())
                    {
                        Me.Spellbook.CastSpell(ignite, target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "Used ignite (KS) on " + target.SkinName + " (" + tHealthPercent + "%)!");
                    }

                }
            }

            if (_mainmenu.Item("dotmode").GetValue<StringList>().SelectedIndex == 1)
            {
                if (Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active)
                {
                    // Get current target near mouse cursor.
                    foreach (
                        var targ in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(hero => hero.IsValidTarget(600) && hero.Health <= Oracle.GetComboDamage(Me, hero) && !hero.IsZombie)
                                .OrderByDescending(hero => hero.Distance(Game.CursorPos)))
                    {
                        Me.Spellbook.CastSpell(ignite, targ);
                        Oracle.Logger(Oracle.LogType.Action,
                            "Used ignite (Combo) on " + targ.SkinName + " (" + targ.Health/targ.MaxHealth*100 + "%)!");
                    }
                }
            }
        }

        #endregion

        #region Barrier
        private static void CheckBarrier(float incdmg = 0)
        {
            var barrier = Me.GetSpellSlot("summonerbarrier");
            if (barrier == SpellSlot.Unknown)
                return;

            if (!_mainmenu.Item("usebarrier").GetValue<bool>())
                return;

            if (!_mainmenu.Item("barrieron" + Me.ChampionName).GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(barrier) != SpellState.Ready)
                return;

            var iDamagePercent = (int) ((incdmg/Me.MaxHealth)*100);
            var mHealthPercent = (int) ((Me.Health/Me.MaxHealth)*100);

            if (Oracle.DangerUlt && Oracle.AggroTarget.NetworkId == Me.NetworkId)
            {
                if (_mainmenu.Item("barrierdanger").GetValue<bool>())
                {
                    if (Oracle.Attacker.Distance(Me.ServerPosition, true) <= 600*600)
                    {
                        Me.Spellbook.CastSpell(barrier, Me);
                        Oracle.Logger(Oracle.LogType.Action, "Used barrier (Danger) on me (" + mHealthPercent + "%)!");
                    }
                }
            }
            
            if (mHealthPercent <= _mainmenu.Item("usebarrierpct").GetValue<Slider>().Value)
            {
                if ((iDamagePercent >= 1 || incdmg >= Me.Health))
                {
                    if (Oracle.AggroTarget.NetworkId == Me.NetworkId)
                    {
                        Me.Spellbook.CastSpell(barrier, Me);
                        Oracle.Logger(Oracle.LogType.Action, "Used barrier (Low HP) on me (" + mHealthPercent + "%)!");
                    }
                }
            }

            else if (iDamagePercent >= _mainmenu.Item("usebarrierdmg").GetValue<Slider>().Value)
            {
                if (Oracle.AggroTarget.NetworkId == Me.NetworkId)
                {
                    Me.Spellbook.CastSpell(barrier, Me);
                    Oracle.Logger(Oracle.LogType.Action, "Used barrier (Damage Chunk) on me (" + mHealthPercent + ")!");
                }
            }

            else if (Me.HasBuff("summonerdot", true) && _mainmenu.Item("barrierdot").GetValue<bool>())
            {
                if (Oracle.AggroTarget.NetworkId == Me.NetworkId &&
                    Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active)
                {
                    Me.Spellbook.CastSpell(barrier, Me);
                    Oracle.Logger(Oracle.LogType.Action, "Used barrier (Ignite) on me (" + mHealthPercent + "%)!");
                }
            }
        }

        #endregion

        #region Heal

        private static void CheckHeal(float incdmg = 0)
        {
            var heal = Me.GetSpellSlot("summonerheal");
            if (heal == SpellSlot.Unknown)
                return;

            if (!_mainmenu.Item("useheal").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(heal) != SpellState.Ready)
                return;

            var target = Oracle.Friendly();
            var iDamagePercent = (int) ((incdmg/Me.MaxHealth)*100);

            if (target.Distance(Me.ServerPosition) <= 700f)
            {
                var aHealthPercent = (int) ((target.Health/target.MaxHealth)*100);
                if (aHealthPercent <= _mainmenu.Item("usehealpct").GetValue<Slider>().Value &&
                    _mainmenu.Item("healon" + target.ChampionName).GetValue<bool>())
                {
                    if ((iDamagePercent >= 1 || incdmg >= target.Health))
                    {
                        if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                        {
                            Me.Spellbook.CastSpell(heal, target);
                            Oracle.Logger(Oracle.LogType.Action,
                                "Used Heal (Low HP) for: " + target.SkinName + " (" + aHealthPercent + "%)!");
                        }
                    }
                }

                else if (iDamagePercent >= _mainmenu.Item("usehealdmg").GetValue<Slider>().Value &&
                         _mainmenu.Item("healon" + target.ChampionName).GetValue<bool>())
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        Me.Spellbook.CastSpell(heal, target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "Used Heal (Damage Chunk) for: " + target.SkinName + " (" + aHealthPercent + "%)!");
                    }
                }
            }
        }

        #endregion

        #region Clarity
        private static void CheckClarity()
        {
            var clarity = Me.GetSpellSlot("summonermana");
            if (clarity == SpellSlot.Unknown)
                return;

            if (!_mainmenu.Item("useclarity").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(clarity) != SpellState.Ready)
                return;

            var target = Oracle.Friendly();
            if (target.Distance(Me.Position) > 600f)
                return;

            var aManaPercent = (int) ((target.Mana/target.MaxMana)*100);
            if (aManaPercent > _mainmenu.Item("useclaritypct").GetValue<Slider>().Value)
                return;

            if (!_mainmenu.Item("clarityon" + target.ChampionName).GetValue<bool>())
                return;

            if (!Me.InFountain() && !Me.IsRecalling())
            {
                Me.Spellbook.CastSpell(clarity, target);
                Oracle.Logger(Oracle.LogType.Action, "Used Clarity for: " + target.SkinName + " (" + aManaPercent + "%)!");
            }
        }

        #endregion

        #region Smite
        private static void FindSmite()
        {
            if (SmiteBlue.Any(x => Items.HasItem(x)))
                _smiteslot = "s5_summonersmiteplayerganker";
            else if (SmiteRed.Any(x => Items.HasItem(x)))
                _smiteslot = "s5_summonersmiteduel";
            else if (SmiteGrey.Any(x => Items.HasItem(x)))
                _smiteslot = "s5_summonersmitequick";
            else if (SmitePurple.Any(x => Items.HasItem(x)))
                _smiteslot = "itemsmiteaoe";
            else
                _smiteslot = "summonersmite";

            _isjungling = SmiteAll.Any(x => Items.HasItem(x));
        }

        private static void ChampionSmite()
        {
            CheckChampSmite("Nunu", "target", 250f, SpellSlot.Q);
            CheckChampSmite("Vi", "self", 250f, SpellSlot.E);
            CheckChampSmite("JarvanIV", "vector", 770f, SpellSlot.Q);
            CheckChampSmite("Poppy", "target", 250f, SpellSlot.Q);
            CheckChampSmite("Riven", "self", 250f, SpellSlot.W);
            CheckChampSmite("Malphite", "self", 250f, SpellSlot.E);
            CheckChampSmite("LeeSin", "self", 1100f, SpellSlot.Q, 1);
            CheckChampSmite("Olaf", "target", 350f, SpellSlot.E);
            CheckChampSmite("Elise", "target", 450f, SpellSlot.Q);
            CheckChampSmite("Warwick", "target", 400f, SpellSlot.Q);
            CheckChampSmite("MasterYi", "target", 600f, SpellSlot.Q);
            CheckChampSmite("Kayle", "target", 650, SpellSlot.Q);
            CheckChampSmite("Khazix", "target", 325f, SpellSlot.Q);
            CheckChampSmite("MonkeyKing", "target", 300f, SpellSlot.Q);
            CheckChampSmite("Amumu", "self", 250f, SpellSlot.E);
            CheckChampSmite("Chogath", "target", 250f, SpellSlot.R);           
        }


        private static void CheckSmite()
        {
            var smite = Me.GetSpellSlot(_smiteslot);
            if (smite == SpellSlot.Unknown)
                return;

            if (!_mainmenu.Item("usesmite").GetValue<KeyBind>().Active)
                return;

            if (Me.Spellbook.CanUseSpell(smite) != SpellState.Ready)
                return;

            ChampionSmite();

            // Smite Hero Blue/Red
            if (Me.GetSpell(smite).Name == "S5_SummonerSmiteDuel" ||
                Me.GetSpell(smite).Name == "S5_SummonerSmitePlayerGanker")
            {
                if (!_mainmenu.Item("savesmite").GetValue<bool>() ||
                     _mainmenu.Item("savesmite").GetValue<bool>() && Me.GetSpell(smite).Ammo > 1)
                {
                    // KS Smite
                    if (_mainmenu.Item("smitemode").GetValue<StringList>().SelectedIndex == 0 &&
                        Me.GetSpell(smite).Name == "S5_SummonerSmitePlayerGanker")
                    {
                        var firsthero =
                            ObjectManager.Get<Obj_AI_Hero>()
                                .First(h => h.IsValidTarget(500) && !h.IsZombie && h.Health <= 20 + 8*Me.Level);
                        
                        Me.Spellbook.CastSpell(smite, firsthero);
                        Oracle.Logger(Oracle.LogType.Action, "Used Smite (KS) on: " + firsthero.BaseSkinName + "!");
                    }

                    // Combo Smite
                    if (_mainmenu.Item("smitemode").GetValue<StringList>().SelectedIndex == 1 ||
                        Me.GetSpell(smite).Name == "S5_SummonerSmiteDuel")
                    {
                        if (Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active)
                        {
                            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>()
                                .Where(h => h.IsValidTarget(500) && !h.IsZombie).OrderByDescending(h => h.Distance(Game.CursorPos)))
                            {
                                Me.Spellbook.CastSpell(smite, hero);
                                Oracle.Logger(Oracle.LogType.Action, "Used Smite (Combo) on: " + hero.BaseSkinName + "!");
                            }
                        }
                    }
                }
            }
          
            // Smite Minion
            foreach (var minion in MinionManager.GetMinions(Me.Position, 500f, MinionTypes.All, MinionTeam.Neutral))
            {
                if (!minion.IsValidTarget(500))
                    return;

                var damage = (float) Me.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite);

                if (LargeMinions.Any(name => minion.Name.StartsWith(name) && !minion.Name.Contains("Mini")))
                {
                    if (_mainmenu.Item("smitelarge").GetValue<bool>() && minion.Health <= damage)
                    {
                        Me.Spellbook.CastSpell(smite, minion);
                        Oracle.Logger(Oracle.LogType.Action, "Used Smite (Large Minion) on: " + minion.Name + "!");
                    }
                }

                else if (SmallMinions.Any(name => minion.Name.StartsWith(name) && !minion.Name.Contains("Mini")))
                {
                    if (_mainmenu.Item("smitesmall").GetValue<bool>() &&
                       !_mainmenu.Item("smiteon" + minion.BaseSkinName).GetValue<bool>() && minion.Health <= damage)
                    {
                        Me.Spellbook.CastSpell(smite, minion);
                        Oracle.Logger(Oracle.LogType.Action, "Used Smite (Small Minion) on: " + minion.Name + "!");
                    }
                }

                else if (EpicMinions.Any(name => minion.Name.StartsWith(name)))
                {
                    if (_mainmenu.Item("smitesuper").GetValue<bool>() && minion.Health <= damage)
                    {
                        Me.Spellbook.CastSpell(smite, minion);
                        Oracle.Logger(Oracle.LogType.Action, "Used Smite (Epic Minion) on: " + minion.Name + "!");
                    }
                }
            }
        }

        private static void CheckChampSmite(string name, string type, float range, SpellSlot slot, int stage = 0)
        {
            if (Oracle.ChampionName != name)
                return;

            ddmg = 0f;
            var spell = new Spell(slot, range);
            if (spell.IsReady() && _mainmenu.Item("smitespell").GetValue<bool>())
            {
                var inst = Me.Spellbook.GetSpell(slot);
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                {
                    if (minion.Distance(Me.ServerPosition, true) > 1200*1200)
                        continue;

                    if (EpicMinions.Any(xe => minion.Name.StartsWith(xe) && !minion.Name.Contains("Mini")) &&
                        _mainmenu.Item("smitesuper").GetValue<bool>() ||

                        LargeMinions.Any(xe => minion.Name.StartsWith(xe) && !minion.Name.Contains("Mini")) &&
                        _mainmenu.Item("smitelarge").GetValue<bool>())
                    {

                        var champdamage = (float) Me.GetSpellDamage(minion, slot, stage);
                        var smitedamage = (float) Me.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite);

                        ddmg = champdamage;
                        if (minion.Health <= smitedamage + champdamage)
                        {                
                            if (name == "LeeSin" && inst.Name == "blindmonkqtwo" &&
                                !minion.HasBuff("BlindMonkSonicWave"))
                            {
                                return;
                            }

                            switch (type)
                            {
                                case "self":
                                    spell.Cast();
                                    Oracle.Logger(Oracle.LogType.Action, "Casting (Self) " + inst.Name + " near: " + minion.Name);
                                    break;
                                case "vector":
                                    spell.Cast(minion.ServerPosition);
                                    Oracle.Logger(Oracle.LogType.Action, "Casting (Vector) " + inst.Name + " on: " + minion.Name);
                                    break;
                                case "target":
                                    spell.CastOnUnit(minion);
                                    Oracle.Logger(Oracle.LogType.Action, "Casting (Target) " + inst.Name + " on: " + minion.Name);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Exhaust

        private static void CheckExhaust()
        {
            var exhaust = Me.GetSpellSlot("summonerexhaust");
            if (exhaust == SpellSlot.Unknown)
                return;

            if (!_mainmenu.Item("useexhaust").GetValue<bool>())
                return;

            if (!Oracle.Origin.Item("usecombo").GetValue<KeyBind>().Active &&
                _mainmenu.Item("exhaustmode").GetValue<StringList>().SelectedIndex == 1)
            {
                return;
            }

            var target = Oracle.Friendly();
            if (Me.Spellbook.CanUseSpell(exhaust) == SpellState.Ready)
            {
                if (Oracle.DangerUlt && _mainmenu.Item("exhdanger").GetValue<bool>())
                {
                    if (Oracle.Attacker.Distance(Me.ServerPosition, true) <= 650*650)
                    {
                        Me.Spellbook.CastSpell(exhaust, Oracle.Attacker);
                        Oracle.Logger(Oracle.LogType.Action, "Used Exhaust (Danger) on: " + Oracle.Attacker.SkinName + "!");
                        Oracle.Logger(Oracle.LogType.Info,
                            "Attackers AD: " + Oracle.Attacker.FlatPhysicalDamageMod + Oracle.Attacker.BaseAttackDamage);
                    }
                }

                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => x.IsValidTarget(900) && !x.IsZombie)
                            .OrderByDescending(xe => xe.BaseAttackDamage + xe.FlatPhysicalDamageMod))
                {
                    if (enemy.Distance(Me.ServerPosition, true) <= 650*650)
                    {
                        var aHealthPercent = target.Health/target.MaxHealth*100;
                        var eHealthPercent = enemy.Health/enemy.MaxHealth*100;

                        if (eHealthPercent <= _mainmenu.Item("eexhaustpct").GetValue<Slider>().Value)
                        {
                            if (!enemy.IsFacing(target))
                            {
                                Me.Spellbook.CastSpell(exhaust, enemy);
                                Oracle.Logger(Oracle.LogType.Action, "Used Exhaust (Offensive) on: " + enemy.SkinName + " (" + eHealthPercent + "%)!");
                                Oracle.Logger(Oracle.LogType.Info,
                                    "Attackers AD: " + Oracle.Attacker.FlatPhysicalDamageMod + Oracle.Attacker.BaseAttackDamage);
                            }
                        }

                        else if (aHealthPercent <= _mainmenu.Item("aexhaustpct").GetValue<Slider>().Value)
                        {
                            if (enemy.IsFacing(target))
                            {
                                Me.Spellbook.CastSpell(exhaust, enemy);
                                Oracle.Logger(Oracle.LogType.Action, "Used Exhaust (Defensive) on: " + enemy.SkinName + " (" + aHealthPercent + "%)!");
                                Oracle.Logger(Oracle.LogType.Info,
                                    "Attackers AD: " + Oracle.Attacker.FlatPhysicalDamageMod + Oracle.Attacker.BaseAttackDamage);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}