using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Oracle
{
    public struct GameObj
    {
        public float Damage;
        public bool Included;
        public string Name;
        public GameObject Obj;
        public int Start;

        public GameObj(string name, GameObject obj, bool included, float incdmg, int start)
        {
            Start = start;
            Name = name;
            Obj = obj;
            Included = included;
            Damage = incdmg;
        }
    }

    public class ObjectHandler
    {
        public static GameObj Satchel;
        public static GameObj Miasma;
        public static GameObj Minefield;
        public static GameObj Crowstorm;
        public static GameObj Fizzbait;
        public static GameObj Caittrap;
        public static GameObj Chaosstorm;
        public static GameObj Glacialstorm;
        public static GameObj Lightstrike;
        public static GameObj Equinox;
        public static GameObj Tormentsoil;
        public static GameObj Depthcharge;
        public static GameObj Tremors;
        public static GameObj Acidtrail;
        public static GameObj Catalyst;

        public static void Load()
        {
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            // Get ground game object damage update
            if (Tremors.Included)
            {
                if (Tremors.Obj.IsValid && Oracle.Friendly().Distance(Tremors.Obj.Position, true) <= 400 * 400)
                {
                    if (Oracle.GetEnemy("Rammus").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Rammus");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Tremors.Damage;
                        Oracle.Logger(Oracle.LogType.Damage,
                            Oracle.AggroTarget.SkinName + " is in Tremors (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Acidtrail.Included)
            {
                if (Acidtrail.Obj.IsValid && Oracle.Friendly().Distance(Acidtrail.Obj.Position, true) <= 150 * 150)
                {
                    if (Oracle.GetEnemy("Singed").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Singed");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Acidtrail.Damage;

                        Oracle.Logger(Oracle.LogType.Damage,
                            Oracle.AggroTarget.SkinName + " is in Poison Trail (Game Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Catalyst.Included)
            {
                if (Catalyst.Obj.IsValid && Oracle.Friendly().Distance(Catalyst.Obj.Position, true) <= 400 * 400)
                {
                    if (Oracle.GetEnemy("Viktor").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Viktor");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Gravity Field (Ground Object) for: 0");
                    }
                }
            }

            if (Glacialstorm.Included)
            {
                if (Glacialstorm.Obj.IsValid && Oracle.Friendly().Distance(Glacialstorm.Obj.Position, true) <= 400 * 400)
                {
                    if (Oracle.GetEnemy("Anivia").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Anivia");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Glacialstorm.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Glacialstorm (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Chaosstorm.Included)
            {
                if (Chaosstorm.Obj.IsValid && Oracle.Friendly().Distance(Chaosstorm.Obj.Position, true) <= 400 * 400)
                {
                    if (Oracle.GetEnemy("Viktor").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Viktor");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Chaosstorm.Damage;
                        Oracle.Dangercc = true;

                        if (Oracle.AggroTarget.NetworkId == Oracle.Friendly().NetworkId &&
                            Oracle.Origin.Item("viktorchaosstormccc").GetValue<bool>())
                        {
                            if (Oracle.Friendly().CountHerosInRange(false) + 1 >= Oracle.Friendly().CountHerosInRange(true) ||
                                Oracle.IncomeDamage >= Oracle.Friendly().Health)
                            {
                                Oracle.Danger = true;
                                Oracle.DangerUlt = true;
                            }
                        }

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Chaostorm (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Fizzbait.Included)
            {
                if (Fizzbait.Obj.IsValid && Oracle.Friendly().Distance(Fizzbait.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Fizz").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Fizz");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Fizzbait.Damage;

                        if (Oracle.Friendly().CountHerosInRange(false) + 1 >= Oracle.Friendly().CountHerosInRange(true) ||
                            Oracle.IncomeDamage >= Oracle.Friendly().Health)
                        {
                            if (Environment.TickCount - Fizzbait.Start >= 900)
                            {
                                Oracle.Danger = true;
                                Oracle.DangerUlt = true;
                                Oracle.Dangercc = true;
                            }
                        }

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in fizz bait (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Depthcharge.Included)
            {
                if (Depthcharge.Obj.IsValid && Oracle.Friendly().Distance(Depthcharge.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Nautilus").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Nautilus");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Depthcharge.Damage;

                        if (Oracle.Friendly().CountHerosInRange(false) + 1 >= Oracle.Friendly().CountHerosInRange(true) ||
                            Oracle.IncomeDamage >= Oracle.Friendly().Health)
                        {
                            if (Oracle.Friendly().HasBuff("nautilusgrandlinetarget", true))
                            {
                                Oracle.Danger = true;
                                Oracle.Dangercc = true;
                                Oracle.DangerUlt = true;
                            }

                            else
                            {
                                Oracle.Dangercc = true;
                            }
                        }

                        Oracle.Logger(Oracle.LogType.Danger,
                            "Nautilus depth charge is homing " + Oracle.AggroTarget.SkinName + " for: " + Oracle.IncomeDamage);
                    }

                }
            }

            if (Caittrap.Included)
            {
                if (Caittrap.Obj.IsValid && Oracle.Friendly().Distance(Caittrap.Obj.Position, true) <= 150 * 150)
                {
                    if ( Oracle.GetEnemy("Caitlyn").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Caitlyn");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Caittrap.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in yordle trap (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Crowstorm.Included)
            {
                // 575 Fear Range
                if (Crowstorm.Obj.IsValid && Oracle.Friendly().Distance(Crowstorm.Obj.Position, true) <= 575 * 575)
                {
                    if ( Oracle.GetEnemy("Fiddlesticks").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Fiddlesticks");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Chaosstorm.Damage;

                        if (Oracle.AggroTarget.NetworkId == Oracle.Friendly().NetworkId &&
                            Oracle.Origin.Item("crowstormccc").GetValue<bool>())
                        {
                            if (Oracle.Friendly().CountHerosInRange(false) + 1 >= Oracle.Friendly().CountHerosInRange(true) ||
                                Oracle.IncomeDamage >= Oracle.Friendly().Health)
                            {
                                Oracle.Danger = true;
                                Oracle.DangerUlt = true;
                            }
                        }

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Crowstorm (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Minefield.Included)
            {
                if (Minefield.Obj.IsValid && Oracle.Friendly().Distance(Minefield.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Ziggs").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Ziggs");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Minefield.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Minefield (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Satchel.Included)
            {
                if (Satchel.Obj.IsValid && Oracle.Friendly().Distance(Satchel.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Ziggs").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Ziggs");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Satchel.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Satchel (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Tormentsoil.Included)
            {
                if (Tormentsoil.Obj.IsValid && Oracle.Friendly().Distance(Tormentsoil.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Morgana").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Morgana");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Tormentsoil.Damage;

                        Oracle.Logger(Oracle.LogType.Damage,
                            Oracle.AggroTarget.SkinName + " is in Torment Soil (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Miasma.Included)
            {
                if (Miasma.Obj.IsValid && Oracle.Friendly().Distance(Miasma.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Cassiopeia").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Cassiopeia");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Satchel.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Miasma (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Lightstrike.Included)
            {
                if (Lightstrike.Obj.IsValid && Oracle.Friendly().Distance(Lightstrike.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Lux").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Lux");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Lightstrike.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Lightstrike (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }

            if (Equinox.Included)
            {
                if (Equinox.Obj.IsValid && Oracle.Friendly().Distance(Equinox.Obj.Position, true) <= 300 * 300)
                {
                    if ( Oracle.GetEnemy("Soraka").IsValid)
                    {
                        Oracle.Attacker =  Oracle.GetEnemy("Soraka");
                        Oracle.AggroTarget = Oracle.Friendly();
                        Oracle.IncomeDamage = Equinox.Damage;
                        Oracle.Dangercc = true;

                        Oracle.Logger(Oracle.LogType.Danger,
                            Oracle.AggroTarget.SkinName + " is in Equinox (Ground Object) for: " + Oracle.IncomeDamage);
                    }
                }
            }
        }

        static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (Oracle.Origin.Item("catchobject").GetValue<KeyBind>().Active)
                Console.WriteLine(obj.Name);

            // red troy is always the enemy team no matter what side.
            if (obj.Name.Contains("Fizz_Ring_Red") &&  Oracle.GetEnemy("Fizz").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Fizz").GetSpellDamage(Oracle.Friendly(), SpellSlot.R);
                Fizzbait = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Fizz)");
            }

            else if (obj.Name.Contains("Acidtrail_buf_red") &&  Oracle.GetEnemy("Singed").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Singed").GetSpellDamage(Oracle.Friendly(), SpellSlot.Q);
                Acidtrail = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Poison)");
            }

            else if (obj.Name.Contains("Tremors_cas") && obj.IsEnemy &&  Oracle.GetEnemy("Rammus").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Rammus").GetSpellDamage(Oracle.Friendly(), SpellSlot.R);
                Tremors = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Tremors)");
            }

            else if (obj.Name.Contains("Crowstorm_red") &&  Oracle.GetEnemy("Fiddlesticks").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Fiddlesticks").GetSpellDamage(Oracle.Friendly(), SpellSlot.R);
                Crowstorm = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Crowstorm)");
            }

            else if (obj.Name.Contains("Nautilus_R_sequence_impact") && obj.IsEnemy &&  Oracle.GetEnemy("Nautilus").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Nautilus").GetSpellDamage(Oracle.Friendly(), SpellSlot.R, 1);
                Depthcharge = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Depth Charge)");
            }

            else if (obj.Name.Contains("caitlyn_Base_yordleTrap_idle_red") &&  Oracle.GetEnemy("Caitlyn").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Caitlyn").GetSpellDamage(Oracle.Friendly(), SpellSlot.W);
                Caittrap = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Yordle Trap)");
            }

            else if (obj.Name.Contains("LuxLightstrike_tar_red") &&  Oracle.GetEnemy("Lux").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Lux").GetSpellDamage(Oracle.Friendly(), SpellSlot.E);
                Lightstrike = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Lightstrike)");
            }

            else if (obj.Name.Contains("ViktorChaosstorm_red") &&  Oracle.GetEnemy("Viktor").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Viktor").GetSpellDamage(Oracle.Friendly(), SpellSlot.R);
                Chaosstorm = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Chaos Storm)");
            }

            else if (obj.Name.Contains("ViktorCatalyst_red") &&  Oracle.GetEnemy("Viktor").IsValid)
            {
                Catalyst = new GameObj(obj.Name, obj, true, 0, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Gravity Field)");
            }

            else if (obj.Name.Contains("cryo_storm_red") &&  Oracle.GetEnemy("Anivia").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Anivia").GetSpellDamage(Oracle.Friendly(), SpellSlot.R);
                Glacialstorm = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Glacialstorm)");
            }

            else if (obj.Name.Contains("ZiggsE_red") &&  Oracle.GetEnemy("Ziggs").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Ziggs").GetSpellDamage(Oracle.Friendly(), SpellSlot.E);
                Minefield = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Minefield)");
            }

            else if (obj.Name.Contains("ZiggsWRingRed") &&  Oracle.GetEnemy("Ziggs").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Ziggs").GetSpellDamage(Oracle.Friendly(), SpellSlot.W);
                Satchel = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Satchel)");
            }

            else if (obj.Name.Contains("CassMiasma_tar_red") &&  Oracle.GetEnemy("Cassiopeia").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Cassiopeia").GetSpellDamage(Oracle.Friendly(), SpellSlot.W);
                Miasma = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Miasma)");
            }

            else if (obj.Name.Contains("Soraka_Base_E_rune_RED") &&  Oracle.GetEnemy("Soraka").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Soraka").GetSpellDamage(Oracle.Friendly(), SpellSlot.E);
                Equinox = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Equinox)");
            }

            else if (obj.Name.Contains("Morgana_Base_W_Tar_red") &&  Oracle.GetEnemy("Morgana").IsValid)
            {
                var dmg = (float) Oracle.GetEnemy("Morgana").GetSpellDamage(Oracle.Friendly(), SpellSlot.W);
                Tormentsoil = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Oracle.Logger(Oracle.LogType.Info, obj.Name + " detected/created (Tormentsoil)");
            }
        }
    }
}