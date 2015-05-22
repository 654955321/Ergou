#region
using System;
using System.IO;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using Oracle.Core.Helpers;
using Oracle.Core.Skillshots;
using Oracle.Core.Targeted;
using Oracle.Extensions;
using SpellSlot = LeagueSharp.SpellSlot;
using Utils = Oracle.Core.Helpers.Utils;
#endregion

namespace Oracle
{
    //  _____             _     
    // |     |___ ___ ___| |___ 
    // |  |  |  _| .'|  _| | -_|
    // |_____|_| |__,|___|_|___|
    // Copyright © Kurisu Solutions 2015
    internal static class Oracle
    {
        #region Enumerator
        internal enum LogType
        {
            Error = 0,
            Danger = 1,
            Info = 2,
            Damage = 3,
            Action = 4
        };
        #endregion

        public static Menu Origin;
        public static Obj_AI_Hero Attacker;
        public static Obj_AI_Hero AggroTarget;
        public static float IncomeDamage, MinionDamage;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        private static void Main(string[] args)
        {
            Console.WriteLine("Oracle is loading...");
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static bool Spell;
        public static bool Stealth;
        public static bool Danger;
        public static bool Dangercc;
        public static bool DangerUlt;
        public static string FileName;
        public static bool CanManamune;
        public static string ChampionName;
        public const string Revision = "233";

        private static void OnGameLoad(EventArgs args)
        {
            ChampionName = Me.ChampionName;

            #region OnLoad
            FileName = "Oracle - " + DateTime.Now.ToString("yy.MM.dd") + " " + DateTime.Now.ToString("h.mm.ss") + ".txt";
            if (!Directory.Exists(Config.AppDataDirectory + @"\Logs\Oracle"))
            {
                Directory.CreateDirectory(Config.AppDataDirectory + @"\Logs\Oracle");
                Game.PrintChat(
                    "<font color=\"#FFFFCC\"><b>Thank you for choosing Oracle! :^)</b></font>");
                Game.PrintChat(
                    "<font color=\"#FFFFCC\"><b>Log files are generated in </b></font>" + Config.AppDataDirectory + @"\Logs\Oracle\");
            }

            else
            {
                Game.PrintChat("<font color=\"#1FFF8F\">Oracle# r." + Revision + " -</font><font color=\"#FFFFCC\"> by Kurisu</font>");
            }

            #endregion

            #region Git Update
            //try
            //{
            //    var wc = new WebClient { Proxy = null };
            //    var gitrevision =
            //        wc.DownloadString(
            //            "https://raw.githubusercontent.com/xKurisu/KurisuSolutions/master/Oracle%20(Activator)/Oracle.txt");

            //    if (Revision != gitrevision)
            //    {
            //        Game.PrintChat("<font color=\"#FFFFCC\"><b>Oracle is outdated, please Update!</b></font>");
            //    }
            //}

            //catch (Exception e)
            //{
            //    Logger(LogType.Error, string.Format("Something went wrong with update checker! {0}", e.Message));
            //}
            #endregion

            Origin = new Menu("Oracle", "oracle", true);

            new Cleansers().Initialize(Origin);
            new Defensives().Initialize(Origin);
            new Summoners().Initialize(Origin);
            new Offensives().Initialize(Origin);
            new Consumables().Initialize(Origin);
            new AutoSpells().Initialize(Origin);

            #region Oracle Config
            var config = new Menu("Oracle Config", "oracleconfig");
            var dangerMenu = new Menu("Dangerous Spells", "dangerconfig");

            foreach (var i in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != Me.Team))
            {
                var menu = new Menu(i.ChampionName, i.ChampionName + "cccmenu");

                foreach (
                    var spell in
                        TargetSpellDatabase.Spells.Where(spell => spell.ChampionName == i.ChampionName.ToLower()))
                {
                    var danger = spell.Spellslot.ToString() == "R" ||
                                    spell.CcType != CcType.No && spell.Type != SpellType.AutoAttack;

                    menu.AddItem(new MenuItem(spell.Name + "ccc", spell.Name + " - " + spell.Spellslot)).SetValue(danger);
                }
                dangerMenu.AddSubMenu(menu);
            }

            config.AddItem(
                new MenuItem("usecombo", "Combo (Active)")
                    .SetValue(new KeyBind(32, KeyBindType.Press)));

            config.AddSubMenu(dangerMenu);

            var cskills = new Menu("Cleanse Special", "cskills");
            foreach (var i in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != Me.Team))
            {
                foreach (var debuff in GameBuff.CleanseBuffs.Where(t => t.ChampionName == i.ChampionName))
                    cskills.AddItem(new MenuItem("cure" + debuff.BuffName, debuff.ChampionName + " - " + debuff.Slot))
                        .SetValue(true);
            }

            config.AddSubMenu(cskills);

            var cleanseMenu = new Menu("Cleanse Debuffs", "cdebufs");
            cleanseMenu.AddItem(new MenuItem("stun", "Stuns")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("charm", "Charms")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("taunt", "Taunts")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("fear", "Fears")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("snare", "Snares")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("silence", "Silences")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("suppression", "Supression")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("polymorph", "Polymorphs")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("blind", "Blinds")).SetValue(false);
            cleanseMenu.AddItem(new MenuItem("slow", "Slows")).SetValue(false);
            cleanseMenu.AddItem(new MenuItem("poison", "Poisons")).SetValue(false);
            config.AddSubMenu(cleanseMenu);

            var debugMenu = new Menu("Debugging", "debugmenu");
            debugMenu.AddItem(new MenuItem("dbool", "Enable Console Debugging")).SetValue(false);
            debugMenu.AddItem(new MenuItem("catchobject", "Log object names"))
                .SetValue(new KeyBind(89, KeyBindType.Press));
            config.AddSubMenu(debugMenu);

            #endregion

            Origin.AddSubMenu(config);
            Origin.AddToMainMenu();

            // Events & Handlers
            Utils.Load();
            ObjectHandler.Load();
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            #region Log Information

            Logger(LogType.Info, "Oracle Revision: " + Revision);
            Logger(LogType.Info, "Local Player: " + ChampionName);
            Logger(LogType.Info, "Local Version: " + Game.Version);
            Logger(LogType.Info, "Local Game Map: " + Game.MapId);
            Logger(LogType.Info, "Local Summoners: " + Me.Spellbook.GetSpell(SpellSlot.Summoner1).Name + " - " +
                                                       Me.Spellbook.GetSpell(SpellSlot.Summoner2).Name);

            foreach (var i in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (i.Team == Me.Team)
                    Logger(LogType.Info, "Ally added: " + i.ChampionName);
                if (i.Team != Me.Team)
                    Logger(LogType.Info, "Enemy added: " + i.ChampionName);
            }

            #endregion
        }

        public static void Logger(LogType type, string msg)
        {
            var prefix = "[" + DateTime.Now.ToString("T") + " " + type + "]: ";
            using (var file = new StreamWriter(Config.AppDataDirectory+ @"\Logs\Oracle\" + FileName, true))
            {
                file.WriteLine(prefix + msg);
                file.Close();
            }

            if (Origin.Item("dbool").GetValue<bool>())
                Console.WriteLine("Oracle: (" + type + ") " + msg);
        }

        public static Obj_AI_Hero Friendly()
        {
            var target = Me;
            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsAlly && x.IsValidTarget(900, false))
                        .OrderByDescending(
                            xe => xe.Health/xe.MaxHealth*100))
            {
                target = unit;
            }

            return target;
        }

        public static Obj_AI_Hero GetEnemy(string championname)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .First(enemy => enemy.Team != Me.Team && enemy.ChampionName == championname);
        }     

        public static bool IsValidState(this Obj_AI_Hero target)
        {
            return !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.SpellImmunity) &&
                   !target.HasBuffOfType(BuffType.Invulnerability);
        }

        public static int CountHerosInRange(this Obj_AI_Hero target, bool checkteam, float range = 1200f)
        {
            var objListTeam =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }

        public static float GetComboDamage(Obj_AI_Hero player, Obj_AI_Base target)
        {
            var ignite = player.GetSpellSlot("summonerdot");

            // todo: get damage for different spell states
            var qready = player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready;
            var wready = player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready;
            var eready = player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready;
            var rready = player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready;
            var iready = player.Spellbook.CanUseSpell(ignite) == SpellState.Ready;

            var tmt = Items.HasItem(3077) && Items.CanUseItem(3077)
                ? Me.GetItemDamage(target, Damage.DamageItems.Tiamat)
                : 0;

            var hyd = Items.HasItem(3074) && Items.CanUseItem(3074)
                ? Me.GetItemDamage(target, Damage.DamageItems.Hydra)
                : 0;

            var bwc = Items.HasItem(3144) && Items.CanUseItem(3144)
                ? Me.GetItemDamage(target, Damage.DamageItems.Bilgewater)
                : 0;

            var brk = Items.HasItem(3153) && Items.CanUseItem(3153)
                ? Me.GetItemDamage(target, Damage.DamageItems.Botrk)
                : 0;

            var dfg = Items.HasItem(3128) && Items.CanUseItem(3128)
                ? Me.GetItemDamage(target, Damage.DamageItems.Dfg)
                : 0;

            var guise = Items.HasItem(3151)
                ? Me.GetItemDamage(target, Damage.DamageItems.LiandrysTorment)
                : 0;

            var torch = Items.HasItem(3188) && Items.CanUseItem(3188)
                ? Me.GetItemDamage(target, Damage.DamageItems.Dfg)
                : 0;

            var items = tmt + hyd + bwc + brk + dfg + torch + guise;
            var aa = player.GetAutoAttackDamage(target);

            var qq = qready ? player.GetSpellDamage(target, SpellSlot.Q) : 0;
            var ww = wready ? player.GetSpellDamage(target, SpellSlot.W) : 0;
            var ee = eready ? player.GetSpellDamage(target, SpellSlot.E) : 0;
            var rr = rready ? player.GetSpellDamage(target, SpellSlot.R) : 0;

            var ii = iready 
                ? player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) 
                : 0;

            var damage = aa + qq + ww + ee + rr + ii + items;

            return (float) damage;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (Items.HasItem(3042) || Items.HasItem(3043)))
            {
                foreach (var o in SkillshotDatabase.Spells.Where(x => x.SpellName == args.SData.Name))
                {
                    foreach (var i in Damage.Spells.Where(d => d.Key == o.ChampionName)
                        .SelectMany(item => item.Value).Where(i => i.Slot == (SpellSlot)o.Slot))
                    {
                        if (i.DamageType == Damage.DamageType.Physical)
                            CanManamune = true;
                    }
                }

                foreach (var o in TargetSpellDatabase.Spells.Where(x => x.Name == args.SData.Name.ToLower()))
                {
                    foreach (var i in Damage.Spells.Where(d => d.Key == o.ChampionName)
                        .SelectMany(item => item.Value).Where(i => i.Slot == (SpellSlot)o.Spellslot))
                    {
                        if (i.DamageType == Damage.DamageType.Physical)
                            CanManamune = true;
                    }
                }

                if (Me.GetSpellSlot(args.SData.Name) == SpellSlot.Unknown &&
                   (Origin.Item("usecombo").GetValue<KeyBind>().Active || args.Target.Type == Me.Type))
                {
                    CanManamune = true;
                }

                else
                {
                    Utility.DelayAction.Add(400, () => CanManamune = false);
                }
            }

            Attacker = null;

            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
            {
                var heroSender = ObjectManager.Get<Obj_AI_Hero>().First(x => x.NetworkId == sender.NetworkId);
                if (heroSender.GetSpellSlot(args.SData.Name) == SpellSlot.Unknown && args.Target.Type == Me.Type)
                {
                    Danger = false;
                    Dangercc = false;
                    DangerUlt = false;
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    IncomeDamage = (float) heroSender.GetAutoAttackDamage(AggroTarget);
                    Logger(LogType.Damage,
                        heroSender.SkinName + " hit (AA) " + AggroTarget.SkinName + " for: " + IncomeDamage);
                }

                if (heroSender.ChampionName == "Jinx" && args.SData.Name.Contains("JinxQAttack") &&
                    args.Target.Type == Me.Type)
                {
                    Danger = false;
                    Dangercc = false;
                    DangerUlt = false;
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    IncomeDamage = (float) heroSender.GetAutoAttackDamage(AggroTarget);
                    Logger(LogType.Damage,
                        heroSender.SkinName + " hit (JinxQ) " + AggroTarget.SkinName + " for: " + IncomeDamage);
                }

                Attacker = heroSender;
                foreach (var o in TargetSpellDatabase.Spells.Where(x => x.Name == args.SData.Name.ToLower()))
                {

                    Stealth = o.Stealth;
                    if (o.Type == SpellType.Skillshot)
                    {
                        continue;
                    }

                    if (o.Type == SpellType.Self)
                    {
                        Utility.DelayAction.Add((o.Delay), delegate
                        {
                            var vulnerableTarget =
                                ObjectManager.Get<Obj_AI_Hero>().OrderBy(x => x.Distance(heroSender.ServerPosition))
                                    .FirstOrDefault(x => x.IsAlly);

                            if (vulnerableTarget != null && vulnerableTarget.Distance(heroSender.ServerPosition, true) <= o.Range*o.Range)
                            {
                                AggroTarget = vulnerableTarget;
                                IncomeDamage = (float) heroSender.GetSpellDamage(AggroTarget, (SpellSlot) o.Spellslot);

                                if (o.Wait)
                                {
                                    return;
                                }

                                Spell = true;
                                Danger = Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();
                                DangerUlt = Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>() &&
                                            o.Spellslot.ToString() == "R";
                                Dangercc = o.CcType != CcType.No && o.Type != SpellType.AutoAttack &&
                                           Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();

                                Logger(LogType.Damage, "Danger (Self: " + o.Spellslot + "): " + Danger);
                                Logger(LogType.Damage,
                                    heroSender.SkinName + " hit (Self: " + o.Spellslot + ") " + AggroTarget.SkinName +
                                    " for: " + IncomeDamage);
                            }
                        });
                    }

                    if (o.Type == SpellType.Targeted && args.Target.Type == Me.Type)
                    {
                        Utility.DelayAction.Add((o.Delay), delegate
                        {
                            AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);
                            IncomeDamage = (float) heroSender.GetSpellDamage(AggroTarget, (SpellSlot) o.Spellslot);

                            Logger(LogType.Damage, "Dangerous (Targetd: " + o.Spellslot + "): " + Danger);
                            Logger(LogType.Damage,
                                heroSender.SkinName + " hit (Targeted: " + o.Spellslot + ") " + AggroTarget.SkinName +
                                " for: " + IncomeDamage);

                            if (o.Wait)
                            {
                                return;
                            }

                            Spell = true;
                            Danger = o.Dangerous && Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();
                            DangerUlt = o.Dangerous && Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>() &&
                                        o.Spellslot.ToString() == "R";

                            Dangercc = o.CcType != CcType.No && o.Type != SpellType.AutoAttack &&
                                           Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();
                        });
                    }
                }

                foreach (
                    var o in
                        SkillshotDatabase.Spells.Where(
                            x => x.SpellName == args.SData.Name || x.ExtraSpellNames.Contains(args.SData.Name)))
                {
                    var skillData =
                        new SkillshotData(o.ChampionName, o.SpellName, o.Slot, o.Type, o.Delay, o.Range,
                            o.Radius, o.MissileSpeed, o.AddHitbox, o.FixedRange, o.DangerValue);

                    var endPosition = args.Start.To2D() +
                                      o.Range*(args.End.To2D() - heroSender.ServerPosition.To2D()).Normalized();

                    var skillShot = new Skillshot(DetectionType.ProcessSpell, skillData, Environment.TickCount,
                        heroSender.ServerPosition.To2D(), endPosition, heroSender);

                    var castTime = (o.DontAddExtraDuration ? 0 : o.ExtraDuration) + o.Delay +
                                   (int) (1000*heroSender.Distance(Friendly().ServerPosition)/o.MissileSpeed) -
                                   (Environment.TickCount - skillShot.StartTick);

                    var vulnerableTarget =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .FirstOrDefault(x => !skillShot.IsSafe(x.ServerPosition.To2D()) && x.IsAlly);

                    if (vulnerableTarget != null && vulnerableTarget.Distance(heroSender.ServerPosition, true) <= o.Range*o.Range)
                    {
                        Utility.DelayAction.Add(castTime - 400, delegate
                        {
                            AggroTarget = vulnerableTarget;
                            IncomeDamage =
                                (float) heroSender.GetSpellDamage(AggroTarget, (SpellSlot) skillShot.SkillshotData.Slot);

                            Spell = true;
                            Danger = o.IsDangerous && Origin.Item(o.SpellName.ToLower() + "ccc").GetValue<bool>();
                            DangerUlt = o.IsDangerous && Origin.Item(o.SpellName.ToLower() + "ccc").GetValue<bool>() &&
                                        o.Slot.ToString() == "R";

                            Logger(LogType.Damage, "Dangerous (Skillshot " + o.Slot + "): " + Danger);
                            Logger(LogType.Damage,
                                heroSender.SkinName + " may hit (SkillShot: " + o.Slot + ") " + AggroTarget.SkinName +
                                " for: " + IncomeDamage);

                        });
                    }
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Minion && sender.IsEnemy)
            {
                if (args.Target.NetworkId == Me.NetworkId)
                {
                    Danger = false; Dangercc = false; DangerUlt = false;
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    MinionDamage =
                        (float)sender.CalcDamage(AggroTarget, Damage.DamageType.Physical,
                            sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Turret && sender.IsEnemy)
            {
                if (args.Target.Type == Me.Type)
                {
                    Danger = false; Dangercc = false; DangerUlt = false;
                    if (sender.Distance(Friendly().ServerPosition, true) <= 900*900)
                    {
                        AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                        IncomeDamage =
                            (float)sender.CalcDamage(AggroTarget, Damage.DamageType.Physical,
                                sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);

                        Logger(LogType.Damage,
                            sender.Name + " (Turret Attack) " + AggroTarget.SkinName + " for: " + IncomeDamage);
                    }
                }
            }
        }
    }
}
