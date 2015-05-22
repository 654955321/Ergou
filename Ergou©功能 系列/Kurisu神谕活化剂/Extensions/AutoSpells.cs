using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Oracle.Core.Helpers;

namespace Oracle.Extensions
{
    internal class AutoSpells
    {
        private static Menu _mainMenu, _menuConfig;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public void Initialize(Menu root)
        {
            Game.OnUpdate += Game_OnGameUpdate;

            _mainMenu = new Menu("Auto Spells", "asmenu");
            _menuConfig = new Menu("Auto Spells Config", "asconfig");

            foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                _menuConfig.AddItem(new MenuItem("ason" + x.ChampionName, "Use for " + x.ChampionName)).SetValue(true);
            _mainMenu.AddSubMenu(_menuConfig);

            // auto shields
            CreateMenuItem(95, "braume", "Unbreakable", "braumshield", SpellSlot.E);
            CreateMenuItem(95, "dianaorbs", "Pale Cascade", "dianashield", SpellSlot.W);
            CreateMenuItem(95, "galiobulwark", "Bulwark", "galioshield", SpellSlot.W);
            CreateMenuItem(95, "garenw", "Courage", "garenshield", SpellSlot.W, false);
            CreateMenuItem(95, "eyeofthestorm", "Eye of the Storm", "jannashield", SpellSlot.E);
            CreateMenuItem(95, "karmasolkimshield", "Inspire", "karmashield", SpellSlot.E);
            CreateMenuItem(95, "lulue", "Help Pix!", "lulushield", SpellSlot.E);
            CreateMenuItem(95, "luxprismaticwave", "Prismatic Barrier", "luxshield", SpellSlot.W);
            CreateMenuItem(95, "nautiluspiercinggaze", "Titans Wraith", "nautilusshield", SpellSlot.W);
            CreateMenuItem(95, "orianaredactcommand", "Command Protect", "oriannashield", SpellSlot.E);
            CreateMenuItem(95, "shenfeint", "Feint", "shenshield", SpellSlot.W, false);
            CreateMenuItem(95, "moltenshield", "Molten Shield", "annieshield", SpellSlot.E);
            CreateMenuItem(95, "jarvanivgoldenaegis", "Golden Aegis", "jarvanivshield", SpellSlot.W);
            CreateMenuItem(95, "blindmonkwone", "Safegaurd", "leeshield", SpellSlot.W, false);
            CreateMenuItem(95, "rivenfeint", "Valor", "rivenshield", SpellSlot.E, false);
            CreateMenuItem(95, "fiorariposte", "Riposte", "fiorashield", SpellSlot.W, false);
            CreateMenuItem(95, "rumbleshield", "Scrap Shield", "rumbleshield", SpellSlot.W, false);
            CreateMenuItem(95, "sionw", "Soul Furnace", "sionshield", SpellSlot.W);
            CreateMenuItem(95, "skarnerexoskeleton", "Exoskeleton", "skarnershield", SpellSlot.W);
            CreateMenuItem(95, "urgotterrorcapacitoractive2", "Terror Capacitor", "urgotshield", SpellSlot.W);
            CreateMenuItem(95, "obduracy", "Brutal Strikes", "malphshield", SpellSlot.W);
            CreateMenuItem(95, "defensiveballcurl", "Defensive Ball Curl", "rammusshield", SpellSlot.W);

            // auto heals
            CreateMenuItem(80, "triumphantroar", "Triumphant Roar", "alistarheal", SpellSlot.E);
            CreateMenuItem(80, "primalsurge", "Primal Surge", "nidaleeheal", SpellSlot.E);
            CreateMenuItem(80, "removescurvy", "Remove Scurvy", "gangplankheal", SpellSlot.W);
            CreateMenuItem(80, "judicatordivineblessing", "Divine Blessing", "kayleheal", SpellSlot.W);
            CreateMenuItem(80, "namie", "Ebb and Flow", "namiheal", SpellSlot.W);
            CreateMenuItem(80, "sonaw", "Aria of Perseverance", "sonaheal", SpellSlot.W);
            CreateMenuItem(80, "sorakaw", "Astral Infusion", "sorakaheal", SpellSlot.W, false);
            CreateMenuItem(80, "Imbue", "Imbue", "taricheal", SpellSlot.Q);

            // auto ultimates
            CreateMenuItem(25, "lulur", "Wild Growth", "luluult", SpellSlot.R, false);
            CreateMenuItem(25, "sadism", "Sadism", "drmundoult", SpellSlot.R, false);
            CreateMenuItem(15, "undyingrage", "Undying Rage", "tryndult", SpellSlot.R, false);
            CreateMenuItem(15, "chronoshift", "Chorno Shift", "zilult", SpellSlot.R, false);
            CreateMenuItem(15, "yorickreviveally", "Omen of Death", "yorickult", SpellSlot.R, false);
            CreateMenuItem(15, "kalistarx", "Fate's Call", "kalistault", SpellSlot.R, false);
            CreateMenuItem(15, "sorakar", "Wish", "sorakault", SpellSlot.R, false);

            // slow removers
            CreateMenuItem(0, "evelynnw", "Draw Frenzy", "eveslow", SpellSlot.W, false);
            CreateMenuItem(0, "garenq", "Decisive Strike", "garenslow", SpellSlot.Q, false);
            CreateMenuItem(0, "highlander", "Highlander", "masteryislow", SpellSlot.R, false);

            // untargetable/evade spells           
            CreateMenuItem(0, "judicatorintervention", "Intervention", "teamkaylezhonya", SpellSlot.R, false);
            CreateMenuItem(0, "fioradance", "Blade Waltz", "herofiorazhonya", SpellSlot.R, false);
            CreateMenuItem(0, "elisespidereinitial", "Rappel", "teamelisezhonya", SpellSlot.E, false);
            CreateMenuItem(0, "fizzjump", "Playful Trickster", "teamfizzzhonyaCC", SpellSlot.E);
            CreateMenuItem(0, "lissandrar", "Frozen Tomb", "teamlissandrazhonya", SpellSlot.R, false);
            CreateMenuItem(0, "maokaiunstablegrowth", "Unstabe Growth", "heromaokaizhonya", SpellSlot.W);
            CreateMenuItem(0, "alphastrike", "Alpha Strike", "heromasteryizhonyaCC", SpellSlot.Q);
            CreateMenuItem(0, "blackshield", "Black Shield", "teammorganazhonyaCC", SpellSlot.E);
            CreateMenuItem(0, "hallucinatefull", "Hallucinate", "teamshacozhonya", SpellSlot.R, false);
            CreateMenuItem(0, "sivire", "Spell Shield", "teamsivirzhonyaCC", SpellSlot.E, false);
            CreateMenuItem(0, "vladimirsanguinepool", "Sanguine Pool", "teamvladimirzhonya", SpellSlot.W, false);
            CreateMenuItem(0, "zedult", "Death Mark", "herozedzhonya", SpellSlot.R, false);
            CreateMenuItem(0, "nocturneshroudofdarkness", "Shroud of Darkness", "teamnocturnezhonyaCC", SpellSlot.W, false);

            root.AddSubMenu(_mainMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // prevent errors before spawning to the rift or dead
            if (!Me.IsValidTarget(300, false))
            {
                return;
            }

            // slow removals
            UseSpell("garenq", "garenslow", float.MaxValue, false);
            UseSpell("evelynnw", "eveslow", float.MaxValue, false);
            UseSpell("highlander", "masteryislow", float.MaxValue, false);

            // spell shields
            UseSpellShield("blackshield", "teammorganazhonyaCC", 600f);
            UseSpellShield("sivire", "teamsivirzhonyaCC", float.MaxValue, false);
            UseSpellShield("nocturneshroudofdarkness", "teamnocturnezhonyaCC", float.MaxValue, false);

            // auto heals
            UseSpell("triumphantroar", "alistarheal", 575f);
            UseSpell("primalsurge", "nidaleeheal", 600f);
            UseSpell("removescurvy", "gangplankheal");
            UseSpell("judicatordivineblessing", "kayleheal", 900f);
            UseSpell("namie", "namiheal", 725f);
            UseSpell("sonaw", "sonaheal", 1000f);
            UseSpell("sorakaw", "sorakaheal", 450f, false);
            UseSpell("imbue", "taricheal", 750f);

            // untargable/evade spells            
            UseEvade("judicatorintervention", "teamkaylezhonya", 900f);
            UseEvade("fioradance", "herofiorazhonya", 300f);
            UseEvade("elisespidereinitial", "teamelisezhonya");
            UseEvade("fizzjump", "teamfizzzhonyaCC");
            UseEvade("lissandrar", "teamlissandrazhonya");
            UseEvade("maokaiunstablegrowth", "heromaokaizhonya", 525f);
            UseEvade("alphastrike", "heromasteryizhonyaCC", 600f);
            UseEvade("hallucinatefull", "teamshacozhonya");
            UseEvade("vladimirsanguinepool", "teamvladimirzhonya");
            UseEvade("zedult", "herozedzhonya", 625f);

            // auto shields
            UseSpell("braume", "braumshield");
            UseSpell("dianaorbs", "dianashield");
            UseSpell("galiobulwark", "galioshield", 800f);
            UseSpell("garenw", "garenshield", float.MaxValue, false);
            UseSpell("eyeofthestorm", "jannashield", 800f);
            UseSpell("karmasolkimshield", "karmashield", 800f);
            UseSpell("luxprismaticwave", "luxshield", 1075f);
            UseSpell("nautiluspiercinggaze", "nautilusshield");
            UseSpell("orianaredactcommand", "oriannashield", 1100f);
            UseSpell("shenfeint", "shenshield", float.MaxValue, false);
            UseSpell("jarvanivgoldenaegis", "jarvanivshield");
            UseSpell("blindmonkwone", "leeshield", 700f, false);
            UseSpell("rivenfeint", "rivenshield", float.MaxValue, false);
            UseSpell("rumbleshield", "rumbleshield");
            UseSpell("sionw", "sionshield");
            UseSpell("skarnerexoskeleton", "skarnershield");
            UseSpell("urgotterrorcapacitoractive2", "urgotshield");
            UseSpell("moltenshield", "annieshield");
            UseSpell("fiorariposte", "fiorashield", float.MaxValue, false);
            UseSpell("obduracy", "malphshield");
            UseSpell("defensiveballcurl", "rammusshield");

            // auto ults
            UseSpell("lulur", "luluult", 900f, false);
            UseSpell("undyingrage", "tryndult", float.MaxValue, false);
            UseSpell("chronoshift", "zilult", 900f, false);
            UseSpell("yorickreviveally", "yorickult", 900f, false);
            UseSpell("sadism", "drmundoult", float.MaxValue, false);

            // soraka global heal
            if (Oracle.ChampionName == "Soraka")
            {
                var sorakaslot = Me.GetSpellSlot("sorakar");
                var sorakar = new Spell(sorakaslot);
                if (!sorakar.IsReady())
                {
                    return;
                }

                if (sorakaslot == SpellSlot.Unknown && !_mainMenu.Item("usesorakault").GetValue<bool>())
                {
                    return;
                }

                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .First(huro => huro.IsValidTarget(float.MaxValue, false) && huro.IsAlly);

                if (!_menuConfig.Item("ason" + target.ChampionName).GetValue<bool>())
                {
                    return;
                }

                var aHealthPercent = target.Health/target.MaxHealth*100;
                if (aHealthPercent <= _mainMenu.Item("usesorakaultPct").GetValue<Slider>().Value)
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        sorakar.Cast();
                        Oracle.Logger(Oracle.LogType.Action,
                            "(Auto Spell: Ult) Saving ally target: " + target.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }
            }

            // kalista save soulbound
            if (Oracle.ChampionName != "Kalista")
            {
                return;
            }

            var slot = Me.GetSpellSlot("kalistarx");
            var kalistar = new Spell(slot, 1200f);
            if (!kalistar.IsReady())
            {
                return;
            }

            if (slot == SpellSlot.Unknown && !_mainMenu.Item("usekalistault").GetValue<bool>())
            {
                return;
            }

            var cooptarget =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(hero => hero.HasBuff("kalistacoopstrikeally", true));
             
            if (cooptarget.IsValidTarget(1200, false) && cooptarget.IsAlly && 
                _menuConfig.Item("ason" + cooptarget.ChampionName).GetValue<bool>())
            {
                var aHealthPercent = (int) ((cooptarget.Health/cooptarget.MaxHealth)*100);
                if (aHealthPercent <= _mainMenu.Item("usekalistaultPct").GetValue<Slider>().Value)
                {
                    if (Oracle.AggroTarget.NetworkId == cooptarget.NetworkId)
                    {
                        kalistar.Cast();
                        Oracle.Logger(Oracle.LogType.Action,
                            "Saving soulbound target: " + cooptarget.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }
            }
        }

        private static void UseSpellShield(string sname, string menuvar, float range = float.MaxValue, bool usemana = true)
        {
            if (!menuvar.Contains(Oracle.ChampionName.ToLower()))
            {
                return;
            }

            var slot = Me.GetSpellSlot(sname);
            if (slot == SpellSlot.Unknown)
                return;

            if (slot != SpellSlot.Unknown && !_mainMenu.Item("use" + menuvar).GetValue<bool>())
            {
                return;
            }
          
            var spell = new Spell(slot, range);

            var target = range < 5000 ? Oracle.Friendly() : Me;
            if (target.Distance(Me.ServerPosition, true) > range * range)
            {
                return;
            }

            var aHealthPercent = target.Health/target.MaxHealth*100;
            if (!spell.IsReady() || !_menuConfig.Item("ason" + target.ChampionName).GetValue<bool>() || Me.IsRecalling() ||
                !target.IsValidState())
            {
                return;
            }

            if (_mainMenu.Item("use" + menuvar + "Ults").GetValue<bool>())
            {
                if (Oracle.DangerUlt && menuvar.Contains("team"))
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        spell.CastOnUnit(target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "(SCC) Casting " + spell.Slot + "(Dangerous Ult) on " + target.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }
            }

            if (_mainMenu.Item("use" + menuvar + "CC").GetValue<bool>())
            {
                if (Oracle.Dangercc && menuvar.Contains("team"))
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        spell.CastOnUnit(target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "(SCC) Casting " + spell.Slot + " (Dangerous CC) on " + target.ChampionName +" (" + aHealthPercent + "%)");
                    }
                }
            }

            if (_mainMenu.Item("use" + menuvar + "Norm").GetValue<bool>())
            {
                 if (Oracle.Danger && menuvar.Contains("team"))
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        spell.CastOnUnit(target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "(SCC) Casting " + spell.Slot + "(Dangerous Spell) on " + target.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }               
            }

            if (_mainMenu.Item("use" + menuvar + "Any").GetValue<bool>())
            {
                if (Oracle.Spell && menuvar.Contains("team"))
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        spell.CastOnUnit(target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "(SCC) Casting " + spell.Slot + "(Any Spell) on " + target.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }
            }
        }


        private static void UseEvade(string sdataname, string menuvar, float range = float.MaxValue)
        {
            if (!menuvar.Contains(Oracle.ChampionName.ToLower()))
                return;

            var slot = Me.GetSpellSlot(sdataname);
            if (slot == SpellSlot.Unknown)
                return;

            if (slot != SpellSlot.Unknown && !_mainMenu.Item("use" + menuvar).GetValue<bool>())
                return;
            
            var spell = new Spell(slot, range);

            var target = range < 5000 ? Oracle.Friendly() : Me;
            if (target.Distance(Me.ServerPosition, true) > range * range)
                return;

            var aHealthPercent = target.Health / target.MaxHealth * 100;
            if (!spell.IsReady() || !_menuConfig.Item("ason" + target.ChampionName).GetValue<bool>()|| !target.IsValidState())
            {
                return;
            }

            if (_mainMenu.Item("use" + menuvar + "Norm").GetValue<bool>())
            {
                if ((Oracle.Danger || Oracle.IncomeDamage >= target.Health || target.Health/target.MaxHealth*100 <= 20) &&
                    menuvar.Contains("team"))
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        spell.CastOnUnit(target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "(Evade) Casting " + spell.Slot + "(Dangerous Spell) on " + target.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }

                if ((Oracle.Danger || Oracle.IncomeDamage >= target.Health || target.Health/target.MaxHealth*100 <= 20) &&
                    menuvar.Contains("hero"))
                {
                    // +1 to allow for potential counterplay
                    if (target.CountHerosInRange(false) + 1 >= target.CountHerosInRange(true)) 
                    {
                        if (Oracle.AggroTarget.NetworkId == Me.NetworkId)
                        {
                            foreach (
                                var ene in
                                    ObjectManager.Get<Obj_AI_Hero>()
                                        .Where(x => x.IsValidTarget(range))
                                        .OrderByDescending(ene => ene.Health/ene.MaxHealth*100))
                            {
                                spell.CastOnUnit(ene);
                                Oracle.Logger(Oracle.LogType.Action, "(Evade) Casting " + spell.Slot + "(DS) on " + ene.ChampionName);
                                Oracle.Logger(Oracle.LogType.Info, "Evade target info: ");
                                Oracle.Logger(Oracle.LogType.Info, "HP %: " + ene.Health/ene.MaxHealth*100);
                                Oracle.Logger(Oracle.LogType.Info, "Current HP %: " + ene.Health);
                            }
                        }
                    }
                }
            }

            if (_mainMenu.Item("use" + menuvar + "Ults").GetValue<bool>())
            {
                foreach (var buff in GameBuff.EvadeBuffs)
                {
                    foreach (var aura in target.Buffs)
                    {
                        if (!aura.Name.ToLower().Contains(buff.SpellName) && aura.Name.ToLower() != buff.BuffName)
                            continue;

                        Utility.DelayAction.Add(
                            buff.Delay, delegate
                            {
                                Oracle.Attacker = Oracle.GetEnemy(buff.ChampionName);
                                Oracle.AggroTarget = target;
                                Oracle.IncomeDamage =
                                    (float)Oracle.GetEnemy(buff.ChampionName).GetSpellDamage(Oracle.AggroTarget, buff.Slot);

                                // check if we still have buff and didn't walk out of it
                                if (aura.Name.ToLower().Contains(buff.SpellName) || aura.Name.ToLower() == buff.BuffName)
                                {
                                    Oracle.DangerUlt = Oracle.Origin.Item(buff.SpellName + "ccc").GetValue<bool>();
                                }

                                Oracle.Logger(Oracle.LogType.Danger,
                                    "(" + Oracle.Attacker.ChampionName + ") Dangerous buff on " + Oracle.AggroTarget.ChampionName + " should evade!");
                            });
                    }
                }    

                if ((Oracle.DangerUlt || Oracle.IncomeDamage >= target.Health || target.Health/target.MaxHealth*100 <= 18) &&
                    menuvar.Contains("team"))
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        spell.CastOnUnit(target);
                        Oracle.Logger(Oracle.LogType.Action,
                            "(Evade) Casting " + spell.Slot + "(DS) on " + target.ChampionName + " (" + aHealthPercent + "%)");
                    }
                }

                if ((Oracle.DangerUlt || Oracle.IncomeDamage >= target.Health || target.Health/target.MaxHealth*100 <= 18) &&
                    menuvar.Contains("hero"))
                {
                    if (Me.CountHerosInRange(false) + 1 > Me.CountHerosInRange(true))
                    {
                        if (Oracle.AggroTarget.NetworkId == Me.NetworkId)
                        {
                            foreach (
                                var ene in
                                    ObjectManager.Get<Obj_AI_Hero>()
                                        .Where(x => x.IsValidTarget(range))
                                        .OrderByDescending(ene => ene.Health/ene.MaxHealth*100))
                            {
                                spell.CastOnUnit(ene);
                                Oracle.Logger(Oracle.LogType.Action, "(Evade) Casting " + spell.Slot + "(DS) on " + ene.ChampionName);
                                Oracle.Logger(Oracle.LogType.Info, "Evade target info: ");
                                Oracle.Logger(Oracle.LogType.Info, "HP %: " + ene.Health / ene.MaxHealth * 100);
                                Oracle.Logger(Oracle.LogType.Info, "Current HP %: " + ene.Health);
                            }
                        }
                    }
                }
            }
        }

        private static void UseSpell(string sdataname, string menuvar, float range = float.MaxValue, bool usemana = true)
        {
            if (!menuvar.Contains(Oracle.ChampionName.ToLower()))
                return;
            
            var slot = Me.GetSpellSlot(sdataname);
            if (slot == SpellSlot.Unknown)
                return;

            if (slot != SpellSlot.Unknown && !_mainMenu.Item("use" + menuvar).GetValue<bool>())
                return;
         
            var spell = new Spell(slot, range);
            var target = range < 5000 ? Oracle.Friendly() : Me;

            if (target.Distance(Me.ServerPosition, true) > range*range)
                return;

            if (!spell.IsReady() || !_menuConfig.Item("ason" + target.ChampionName).GetValue<bool>() ||
                Me.IsRecalling() ||  Me.InFountain() || !target.IsValidState())
            {
                return;
            }
         
            var manaPercent = (int) (Me.Mana/Me.MaxMana*100);
            var mHealthPercent = (int)(Me.Health / Me.MaxHealth * 100);
            var aHealthPercent = (int)((target.Health / target.MaxHealth) * 100);
            var iDamagePercent = (int)((Oracle.IncomeDamage / target.MaxHealth) * 100);

            if (menuvar.Contains("slow") && Me.HasBuffOfType(BuffType.Slow))
            {
                spell.Cast();
                Oracle.Logger(Oracle.LogType.Action,  "(Auto Spell: Slow) Im slowed, casting " + spell.Slot);
            }

            if (menuvar.Contains("slow")) 
                return;

            if (menuvar.Contains("shield") || menuvar.Contains("ult"))
            {
                
                if (aHealthPercent > _mainMenu.Item("use" + menuvar + "Pct").GetValue<Slider>().Value)
                    return;

                if (usemana && manaPercent <= _mainMenu.Item("use" + menuvar + "Mana").GetValue<Slider>().Value)
                    return;

                if (iDamagePercent >= 1 || Oracle.IncomeDamage >= target.Health)
                {
                    if (Oracle.AggroTarget.NetworkId == target.NetworkId)
                    {
                        switch (menuvar)
                        {
                            case "rivenshield":
                                spell.Cast(Game.CursorPos);
                                Oracle.Logger(Oracle.LogType.Action,
                                    "(Auto Spell: Shield/Ult) Casting " + spell.Slot + " to game cursor! (Low HP)");
                                Oracle.Logger(Oracle.LogType.Action, "Target HP %: " + aHealthPercent);
                                break;
                            case "braumshield":
                                spell.Cast(Oracle.Attacker.ServerPosition);
                                break;
                            case "luxshield":
                                spell.Cast(target.IsMe ? Game.CursorPos : target.ServerPosition);
                                break;
                            default:
                                spell.CastOnUnit(target);
                                Oracle.Logger(Oracle.LogType.Action,
                                    "(Auto Spell: Shield/Ult) Casting " + spell.Slot + " on " + target.ChampionName + " (Low HP)");
                                Oracle.Logger(Oracle.LogType.Action, "Target HP %: " + aHealthPercent);
                                break;
                        }
                    }
                }
            }

            else if (menuvar.Contains("heal"))
            {
                if (aHealthPercent > _mainMenu.Item("use" + menuvar + "Pct").GetValue<Slider>().Value)
                    return;

                if (menuvar.Contains("soraka"))   
                {
                    if (mHealthPercent <= _mainMenu.Item("useSorakaMana").GetValue<Slider>().Value || target.IsMe)
                        return;
                }

                if (usemana && manaPercent <= _mainMenu.Item("use" + menuvar + "Mana").GetValue<Slider>().Value)
                    return;

                if (Oracle.ChampionName == "Sona") 
                    spell.Cast(); 
                else
                {
                    spell.Cast(target);
                    Oracle.Logger(Oracle.LogType.Action, "(Auto Spell: Heal) Casting " + spell.Slot + " on " + target.ChampionName + " (Low HP)");
                    Oracle.Logger(Oracle.LogType.Action, "Target HP %: " + aHealthPercent);
                }
            }

            if (!menuvar.Contains("zhonya"))
            {
                if (iDamagePercent >= _mainMenu.Item("use" + menuvar + "Dmg").GetValue<Slider>().Value)
                {
                    spell.Cast(target);
                    Oracle.Logger(Oracle.LogType.Action, "(SS) Casting " + spell.Slot + " on " + target.ChampionName + " (Damage Chunk)");
                    Oracle.Logger(Oracle.LogType.Action, "Target HP %: " + aHealthPercent);
                }
            }

        }

        private static void CreateMenuItem(int dfv, string sdname, string name, string menuvar, SpellSlot slot, bool usemana = true)
        {
            var champslot = Me.GetSpellSlot(sdname.ToLower());
            if (champslot == SpellSlot.Unknown || champslot != SpellSlot.Unknown && champslot != slot)
            {
                return;
            }

            var menuName = new Menu(name + " | " + slot, menuvar);
            menuName.AddItem(new MenuItem("use" + menuvar, "Use " + name)).SetValue(true);

            if (!menuvar.Contains("zhonya"))
            {
                if (menuvar.Contains("slow"))
                    menuName.AddItem(new MenuItem("use" + menuvar + "Slow", "Remove slows").SetValue(true));

                if (!menuvar.Contains("slow"))
                    menuName.AddItem(new MenuItem("use" + menuvar + "Pct", "Use on HP %"))
                        .SetValue(new Slider(dfv, 1, 99));

                if (!menuvar.Contains("ult") && !menuvar.Contains("slow"))
                    menuName.AddItem(new MenuItem("use" + menuvar + "Dmg", "Use on Dmg dealt %"))
                        .SetValue(new Slider(45));

                if (menuvar.Contains("soraka"))
                    menuName.AddItem(new MenuItem("useSorakaMana", "Minimum HP % to use")).SetValue(new Slider(35));

                if (usemana)
                    menuName.AddItem(new MenuItem("use" + menuvar + "Mana", "Minimum mana % to use"))
                        .SetValue(new Slider(45));
            }

            if (menuvar.Contains("zhonya"))
            {
                if (menuvar.Contains("CC"))
                {
                    menuName.AddItem(new MenuItem("use" + menuvar + "Any", "Use on any Spell")).SetValue(false);
                    menuName.AddItem(new MenuItem("use" + menuvar + "CC", "Use on Crowd Control")).SetValue(true);
                }

                menuName.AddItem(new MenuItem("use" + menuvar + "Norm", "Use on Dangerous (Spells)")).SetValue(slot != SpellSlot.R);
                menuName.AddItem(new MenuItem("use" + menuvar + "Ults", "Use on Dangerous (Ultimates Only)")).SetValue(true);
            }

            _mainMenu.AddSubMenu(menuName);
        }
    }
}