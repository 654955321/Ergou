
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Reflection;
using SharpDX;
using Color = System.Drawing.Color;

namespace meta_Smite
{
    class Program
    {
        public static Menu Config;
        public static Dictionary<string, SpellSlot> spellList = new Dictionary<string, SpellSlot>();
        public static Dictionary<string, float> rangeList = new Dictionary<string, float>();
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static Spell smite;
        public static Spell champSpell;
        public static bool hasSpell = false;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        private static void Game_OnGameStart(EventArgs args)
        {
            Game.PrintChat("Starting load of Meta Smite");
            setSmiteSlot();
            Config = new Menu("meta惩戒", "metaSmite", true);
            Config.AddItem(new MenuItem("Enabled", "启用 切换").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle, true)));
            Config.AddItem(new MenuItem("EnabledH", "启用 保存").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Press)));
            Config.AddItem(new MenuItem("DrawStatus", "显示 状态!").SetValue(true));
            Config.AddItem(new MenuItem("RangeDraw", "使用惩戒时显示范围").SetValue(true));
            //Config.AddItem(new MenuItem("SmiteCast", "Cast smite using packet")).SetValue(true);
            Config.AddToMainMenu();
            champSpell = addSupportedChampSkill();
            if (smiteSlot == SpellSlot.Unknown && champSpell.Slot == SpellSlot.Unknown)
            {
                Game.PrintChat("Smite and spell not found, disabling Meta Smite");
                return;
            }
            setupCampMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("Meta 鎯╂垝 by metaphorce 鍔犺級鎴愬姛!婕㈠寲by浜岀嫍!QQ缇361630847");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<KeyBind>().Active || Config.Item("EnabledH").GetValue<KeyBind>().Active)
            {
                bool smiteReady = false;
                bool spellReady = false;
                Obj_AI_Base mob = GetNearest(ObjectManager.Player.ServerPosition);
                if (mob != null && Config.Item(mob.BaseSkinName).GetValue<bool>())
                {
                    double smitedamage = smiteDamage();
                    double spelldamage = spellDamage(mob);

                    if (ObjectManager.Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready && Vector3.Distance(ObjectManager.Player.ServerPosition, mob.ServerPosition) < smite.Range)
                    {
                        smiteReady = true;
                    }

                    if (smiteReady && mob.Health < smitedamage) //Smite is ready and enemy is killable with smite
                    {
                        setSmiteSlot();
                        smite.Slot = smiteSlot;
                        ObjectManager.Player.Spellbook.CastSpell(smiteSlot, mob);
                    }

                    if (!hasSpell) 
                    {
                        return; 
                    }

                    if (Config.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>())
                    {
                        spellReady = true;
                    }
                    if (champSpell.IsReady() && spellReady && Vector3.Distance(ObjectManager.Player.ServerPosition, mob.ServerPosition) < champSpell.Range && !mob.IsDead) //skill is ready 
                    {
                        if (smiteReady)
                        {
                            if (mob.Health < smitedamage + spelldamage) //Smite is ready and combined damage will kill
                            {
                                if(ObjectManager.Player.ChampionName == "Lux")
                                {
                                    champSpell.Cast(mob.ServerPosition);
                                }
                                if (ObjectManager.Player.ChampionName == "Twitch" ||
                                    ObjectManager.Player.ChampionName == "MonkeyKing" ||
                                    ObjectManager.Player.ChampionName == "Rammus" ||
                                    ObjectManager.Player.ChampionName == "Rengar" ||
                                    ObjectManager.Player.ChampionName == "Nasus" ||
                                    ObjectManager.Player.ChampionName == "LeeSin" ||
                                    ObjectManager.Player.ChampionName == "Udyr" ||
                                    ObjectManager.Player.ChampionName == "Kalista")
                                {
                                    if (ObjectManager.Player.ChampionName == "LeeSin")
                                    {
                                        if (!mob.HasBuff("BlindMonkSonicWave"))
                                        {
                                            return;
                                        }
                                    }
                                    champSpell.Cast();
                                } 
                                else
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(champSpell.Slot, mob);
                                }
                            }
                        }
                        else if (mob.Health < spelldamage) //Killable with spell
                        {
                            if (ObjectManager.Player.ChampionName == "Lux" || ObjectManager.Player.ChampionName == "Xerath")
                            {
                                champSpell.Cast(mob.ServerPosition);
                            }
                            if (ObjectManager.Player.ChampionName == "Twitch" ||
                                ObjectManager.Player.ChampionName == "MonkeyKing" ||
                                ObjectManager.Player.ChampionName == "Rammus" ||
                                ObjectManager.Player.ChampionName == "Rengar" ||
                                ObjectManager.Player.ChampionName == "Nasus" ||
                                ObjectManager.Player.ChampionName == "LeeSin" ||
                                ObjectManager.Player.ChampionName == "Udyr" ||
                                ObjectManager.Player.ChampionName == "Kalista")
                            {
                                if (ObjectManager.Player.ChampionName == "LeeSin")
                                {
                                    if (!mob.HasBuff("BlindMonkSonicWave"))
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        champSpell.Cast();
                                    }
                                }
                                else
                                {
                                    champSpell.CastOnUnit(ObjectManager.Player);
                                }
                            }
                            else
                            {
                                ObjectManager.Player.Spellbook.CastSpell(champSpell.Slot, mob);
                            }
                        }
                    }
                }
            }
        }
        
        
        
        public static void Drawing_OnDraw(EventArgs args)
        {

            var drawStatus = Config.Item("DrawStatus").GetValue<bool>();
            if (drawStatus)
            {
                if (Config.Item("Enabled").GetValue<KeyBind>().Active || Config.Item("EnabledH").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(ObjectManager.Player.HPBarPosition.X + 10, ObjectManager.Player.HPBarPosition.Y + 36, Color.Gold, "Smite Enabled!");
                }
                else
                {
                    Drawing.DrawText(ObjectManager.Player.HPBarPosition.X + 10, ObjectManager.Player.HPBarPosition.Y + 36, Color.Red, "Smite Disabled!");
                }
            }

            var rangeDrawStatus = Config.Item("RangeDraw").GetValue<bool>(); 
            if(rangeDrawStatus)
            {
                if (Config.Item("Enabled").GetValue<KeyBind>().Active || Config.Item("EnabledH").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, 760f, Color.Red);
                }
            }

            Obj_AI_Base mob1 = GetNearest(ObjectManager.Player.ServerPosition);
            if (Vector3.Distance(ObjectManager.Player.ServerPosition, mob1.ServerPosition) < 1500 && mob1.IsVisible)
            {
                bool smiteR = false;
                bool spellR = false;
                if (ObjectManager.Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready)
                {
                    smiteR = true;
                }
                if (Config.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>())
                {
                    spellR = true;
                }
                double smited = smiteDamage();
                double spelld = 0;
                if (champSpell.IsReady() && spellR)
                {
                    spelld = spellDamage(mob1);
                }

                Vector2 hpBarPos = mob1.HPBarPosition;
                hpBarPos.X += 45;
                hpBarPos.Y += 18;
                var smitePercent = smited / mob1.MaxHealth;
                var spellPercent = spelld / mob1.MaxHealth;
                double smiteMini = hpBarPos.X + (73 * smitePercent);
                double spellMini = hpBarPos.X + (73 * spellPercent);
                double spellsmiteMini = hpBarPos.X + ((73 * spellPercent) + (73 * smitePercent));

                double smiteGromp = hpBarPos.X + (85 * smitePercent);
                double spellGromp = hpBarPos.X + (85 * spellPercent);
                double spellsmiteGromp = hpBarPos.X + ((85 * spellPercent) + (85 * smitePercent));

                double smiteKrug = hpBarPos.X + (83 * smitePercent);
                double spellKrug = hpBarPos.X + (83 * spellPercent);
                double spellsmiteKrug = hpBarPos.X + ((83 * spellPercent) + (83 * smitePercent));

                double smiteBuffs = hpBarPos.X + (143 * smitePercent);
                double spellBuffs = hpBarPos.X + (143 * spellPercent);
                double spellsmiteBuffs = hpBarPos.X + ((143 * spellPercent) + (143 * smitePercent));

                double smiteBaron = hpBarPos.X + (194 * smitePercent);
                double spellBaron = hpBarPos.X + (194 * spellPercent);
                double spellsmiteBaron = hpBarPos.X + ((194 * spellPercent) + (194 * smitePercent));

                #region DrawRazorbeak/Murwolf
                if (mob1.BaseSkinName == "SRU_Razorbeak" ||
                    mob1.BaseSkinName == "SRU_Murkwolf")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float) (smiteMini - 6), hpBarPos.Y, (float) (smiteMini - 6), hpBarPos.Y + 5, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float) (spellsmiteMini - 6), hpBarPos.Y, (float) (spellsmiteMini - 6), hpBarPos.Y + 5, 2, smited + spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float) (smiteMini - 6), hpBarPos.Y, (float) (smiteMini - 6), hpBarPos.Y + 5, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                       Drawing.DrawLine((float) (spellMini - 6), hpBarPos.Y, (float) (spellMini - 6), hpBarPos.Y + 5, 2, spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
                #endregion
                #region DrawGromp
                else if (mob1.BaseSkinName == "SRU_Gromp")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)(smiteGromp - 12), hpBarPos.Y, (float)(smiteGromp - 12), hpBarPos.Y + 5, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)(spellsmiteGromp - 12), hpBarPos.Y, (float)(spellsmiteGromp - 12), hpBarPos.Y + 5, 2, smited + spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)(smiteGromp - 12), hpBarPos.Y, (float)(smiteGromp - 12), hpBarPos.Y + 5, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)(spellGromp - 12), hpBarPos.Y, (float)(spellGromp - 12), hpBarPos.Y + 5, 2, spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
                #endregion
                #region DrawKrug
                else if (mob1.BaseSkinName == "SRU_Krug")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)(smiteKrug - 10), hpBarPos.Y, (float)(smiteKrug - 10), hpBarPos.Y + 5, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)(spellsmiteKrug - 10), hpBarPos.Y, (float)(spellsmiteKrug - 10), hpBarPos.Y + 5, 2, smited + spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)(smiteKrug - 10), hpBarPos.Y, (float)(smiteKrug - 10), hpBarPos.Y + 5, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)(spellKrug - 10), hpBarPos.Y, (float)(spellKrug - 10), hpBarPos.Y + 5, 2, spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
                #endregion
                #region DrawRed/Blue
                else if (mob1.BaseSkinName == "SRU_Red" ||
                         mob1.BaseSkinName == "SRU_Blue" ||
                         mob1.BaseSkinName == "SRU_Dragon")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)(smiteBuffs - 40), hpBarPos.Y, (float)(smiteBuffs - 40), hpBarPos.Y + 12, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)(spellsmiteBuffs - 40), hpBarPos.Y, (float)(spellsmiteBuffs - 40), hpBarPos.Y + 12, 2, smited + spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)(smiteBuffs - 40), hpBarPos.Y, (float)(smiteBuffs - 40), hpBarPos.Y + 12, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)(spellBuffs - 40), hpBarPos.Y, (float)(spellBuffs - 40), hpBarPos.Y + 12, 2, spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
                #endregion
                else if (mob1.BaseSkinName == "SRU_Baron")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)(smiteBaron - 65), hpBarPos.Y, (float)(smiteBaron - 65), hpBarPos.Y + 12, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)(spellsmiteBaron-65), hpBarPos.Y, (float)(spellsmiteBaron - 65), hpBarPos.Y + 12, 2, smited + spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)(smiteBaron - 65), hpBarPos.Y, (float)(smiteBaron - 65), hpBarPos.Y + 12, 2, smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)(spellBaron - 65), hpBarPos.Y, (float)(spellBaron - 65), hpBarPos.Y + 12, 2, spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
            }
        }
	
	
	    //Start Credits to Kurisu
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        public static string smitetype()
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
        //End credits


        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 700);
                return;
            }
        }

        public static double smiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] damage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return damage.Max();
        }

        public static double spellDamage(Obj_AI_Base mob)
        {
            double result = 0;
            Obj_AI_Hero hero = ObjectManager.Player;
            if(hero.ChampionName == "Nunu")
            {
                return (250 + (150 * hero.Spellbook.GetSpell(champSpell.Slot).Level));
            }
            if (hero.ChampionName == "Chogath")
            {
                return (1000 + (hero.FlatMagicDamageMod * 0.7));
            }
            if (hero.ChampionName == "Elise")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Lux")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot) - 100);
            }
            if (hero.ChampionName == "Volibear")
            {
                return (((35 + (45 * hero.Spellbook.GetSpell(SpellSlot.W).Level)) + ((hero.MaxHealth - (86 * hero.Level + 440)) * 0.15)) * ((mob.MaxHealth - mob.Health) / mob.MaxHealth + 1));
            }
            if (hero.ChampionName == "Warwick")
            {
                return (25 + (50 * hero.Spellbook.GetSpell(champSpell.Slot).Level));
            }
            if (hero.ChampionName == "Olaf")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Twitch")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Shaco")
            {
                return (10 + (40 * hero.Spellbook.GetSpell(champSpell.Slot).Level));
            }
            if (hero.ChampionName == "Vi")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Pantheon")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "MasterYi")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "MonkeyKing")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "KhaZix")
            {
                return (getKhazixDmg(mob));
            }
            if (hero.ChampionName == "Rammus")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Rengar")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Nasus")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Xerath")
            {
                champSpell.Range = 2000 + champSpell.Level * 1200;//Update R range
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "LeeSin")
            {
                return (getQ2Dmg(mob));
            }
            if (hero.ChampionName == "Veigar")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Udyr")
            {
                return (getUdyrR(mob));
            }
            if (hero.ChampionName == "Fizz")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Kalista")
            {
                return (getKalistaEDmg(mob));
            }
            if(hero.ChampionName == "Irelia")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            return result;
        }

        public static Spell addSupportedChampSkill()
        {
            spellList.Add("Chogath", SpellSlot.R);
            spellList.Add("Nunu", SpellSlot.Q);
            spellList.Add("Elise", SpellSlot.Q);
            spellList.Add("Kayle", SpellSlot.Q);
            spellList.Add("Lux", SpellSlot.R);
            spellList.Add("Volibear", SpellSlot.W);
            spellList.Add("Warwick", SpellSlot.Q);
            spellList.Add("Olaf", SpellSlot.E);
            spellList.Add("Twitch", SpellSlot.E);
            spellList.Add("Shaco", SpellSlot.E);
            spellList.Add("Vi", SpellSlot.E);
            spellList.Add("Pantheon", SpellSlot.Q);
            spellList.Add("MasterYi", SpellSlot.Q);
            spellList.Add("MonkeyKing", SpellSlot.Q);
            spellList.Add("Khazix", SpellSlot.Q);
            spellList.Add("Rammus", SpellSlot.Q);
            spellList.Add("Rengar", SpellSlot.Q);
            spellList.Add("Nasus", SpellSlot.Q);
            spellList.Add("Xerath", SpellSlot.R);
            spellList.Add("LeeSin", SpellSlot.Q);
            spellList.Add("Veigar", SpellSlot.Q);
            spellList.Add("Udyr", SpellSlot.R);
            spellList.Add("Fizz", SpellSlot.Q);
            spellList.Add("Kalista", SpellSlot.E);
            spellList.Add("Irelia", SpellSlot.Q);

            if(spellList.ContainsKey(ObjectManager.Player.ChampionName))
            {
                string champ = ObjectManager.Player.ChampionName;
                SpellSlot slot;
                spellList.TryGetValue(champ, out slot);
                Spell comboSpell = new Spell(slot, 0);
                comboSpell.Range = getRange(champ);
                Config.AddItem(new MenuItem("Enabled-" + champ, "Enabled-" + champ + "-" + slot)).SetValue(true);
                hasSpell = true;
                return comboSpell;
            }
            else
            {
                return new Spell(SpellSlot.Unknown, 0);
            }
        }

        public static float getRange(string champName)
        {
            rangeList.Add("Chogath", 175f);
            rangeList.Add("Nunu", 175f);
            rangeList.Add("Elise", 475f);
            rangeList.Add("Kayle", 650f);
            rangeList.Add("Lux", 3340f);
            rangeList.Add("Volibear", 400f);
            rangeList.Add("Warwick", 400f);
            rangeList.Add("Olaf", 325f);
            rangeList.Add("Twitch", 1200);
            rangeList.Add("Shaco", 625f);
            rangeList.Add("Vi", 600f);
            rangeList.Add("Pantheon", 600f);
            rangeList.Add("MasterYi", 600f);
            rangeList.Add("MonkeyKing", 100f);
            rangeList.Add("Khazix", 325f);
            rangeList.Add("Rammus", 100f);
            rangeList.Add("Rengar", ObjectManager.Player.AttackRange);
            rangeList.Add("Nasus", ObjectManager.Player.AttackRange);
            rangeList.Add("Xerath", 3200f);
            rangeList.Add("LeeSin", 1300f);
            rangeList.Add("Veigar", 650f);
            rangeList.Add("Udyr", ObjectManager.Player.AttackRange);
            rangeList.Add("Fizz", 550f);
            rangeList.Add("Kalista", 950);
            rangeList.Add("Irelia", 650);
            float res;
            rangeList.TryGetValue(champName, out res);
            return res;
        }

        public static void setupCampMenu()
        {
            Config.AddSubMenu(new Menu("野区 设置", "Camps"));
            if(Game.MapId == GameMapId.TwistedTreeline)
            {
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_Spiderboss", "惩戒 小幽灵").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_NGolem", "惩戒 石头人").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_NWolf", "惩戒 三狼").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_NWraith", "惩戒 大幽灵").SetValue(true));
            }
	    if (Game.MapId == (GameMapId)11)
	    {
                //start by SKO
		Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Baron", "惩戒 大龙").SetValue(true));
		Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Dragon", "惩戒 小龙").SetValue(true));
		Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Blue", "惩戒 蓝buff").SetValue(true));
		Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Red", "惩戒 红buff").SetValue(true));
                //end
                Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Gromp", "惩戒 石甲虫buff").SetValue(false));
                Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Murkwolf", "惩戒 暗影狼buff").SetValue(false));
                Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Krug", "惩戒 魔沼蛙buff").SetValue(false));
                Config.SubMenu("Camps").AddItem(new MenuItem("SRU_Razorbeak", "惩戒 锋啄鸟buff").SetValue(false));
                Config.SubMenu("Camps").AddItem(new MenuItem("Sru_Crab", "惩戒 河蟹buff").SetValue(false));
	    }
        }

        public static double getQ2Dmg(Obj_AI_Base target)
        {
            Int32[] dmgQ = { 50, 80, 110, 140, 170 };
            double damage = ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, dmgQ[champSpell.Level - 1] + 0.9 * ObjectManager.Player.FlatPhysicalDamageMod + 0.08 * (target.MaxHealth - target.Health));
            if(damage > 400)
            {
                return 400;
            }
            return damage;
        }

        public static double getUdyrR(Obj_AI_Base target)
        {
            Int32[] dmgQ = { 40, 80, 120, 160, 200 };
            double damage = ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, dmgQ[champSpell.Level - 1] + 0.45 * ObjectManager.Player.FlatMagicDamageMod);
            return damage;
        }

        public static double getKhazixDmg(Obj_AI_Base target)
        {
            List<Obj_AI_Base> allMobs = MinionManager.GetMinions(target.ServerPosition, 500f, MinionTypes.All, MinionTeam.Neutral);
            Int32[] dmgQ = {70, 95, 120, 145, 170};
            double damage = ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, (dmgQ[champSpell.Level - 1] + (1.2 * ObjectManager.Player.FlatPhysicalDamageMod)));
            if (allMobs.Count == 1)
            {
                if (ObjectManager.Player.HasBuff("khazixqevo", true))
                {
                    return (damage * 1.3) + (ObjectManager.Player.Level * 10) + (1.04 * ObjectManager.Player.FlatPhysicalDamageMod);
                }
                return damage + (damage*0.3);
            }
            return damage;
        }

        public static double getKalistaEDmg(Obj_AI_Base t)
        {
            var buff = t.Buffs.FirstOrDefault(xBuff => xBuff.DisplayName.ToLower() == "kalistaexpungemarker");
            if (buff != null)
            {
                double damage = ObjectManager.Player.FlatPhysicalDamageMod + ObjectManager.Player.BaseAttackDamage;
                double eDmg = damage * 0.60 + new double[] { 0, 20, 30, 40, 50, 60 }[champSpell.Level];
                damage += buff.Count * (0.004 * damage) + eDmg;
                return ObjectManager.Player.CalcDamage(t, Damage.DamageType.Physical, damage);
            }
            return 0;
        }

        //Credits to Lizzaran & SKO
        private static readonly string[] MinionNames =
        {
            "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon", "Sru_Crab", "SRU_Baron"
        };

        public static void testFind(Vector3 pos)
        {
            double? nearest = null;
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid);
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            foreach (Obj_AI_Minion minion in minions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            Game.PrintChat("Minion name is: " + sMinion.Name);
        }
        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")) && !MinionNames.Any(name => minion.Name.Contains("Spawn")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
    }
}
