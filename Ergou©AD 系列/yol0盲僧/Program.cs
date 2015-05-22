using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace yol0LeeSin
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Spell _Q = new Spell(SpellSlot.Q, 1100);
        public static Spell _Q2 = new Spell(SpellSlot.Q, 1300);
        public static Spell _W = new Spell(SpellSlot.W, 700);
        public static Spell _E = new Spell(SpellSlot.E, 350);
        public static Spell _E2 = new Spell(SpellSlot.E, 500);
        public static Spell _R = new Spell(SpellSlot.R, 375);
        public static Spell _F = new Spell(SpellSlot.Unknown, 425);
        public static Spell _I = new Spell(SpellSlot.Unknown, 600);

        private static bool IsQOne { get { return _Q.Instance.Name == "BlindMonkQOne"; } }
        private static bool IsQTwo { get { return _Q.Instance.Name == "blindmonkqtwo"; } }
        private static bool IsWOne { get { return _W.Instance.Name == "BlindMonkWOne"; } }
        private static bool IsWTwo { get { return _W.Instance.Name == "blindmonkwtwo"; } }
        private static bool IsEOne { get { return _E.Instance.Name == "BlindMonkEOne"; } }
        private static bool IsETwo { get { return _E.Instance.Name == "blindmonketwo"; } }

        private static bool UseQ { get { return _menu.SubMenu("Combo").Item("useQ").GetValue<bool>(); } }
        private static bool UseQ2 { get { return _menu.SubMenu("Combo").Item("useQ2").GetValue<bool>(); } }
        private static bool UseE { get { return _menu.SubMenu("Combo").Item("useE").GetValue<bool>(); } }
        private static bool UseE2 { get { return _menu.SubMenu("Combo").Item("useE2").GetValue<bool>(); } }
        private static bool UseR { get { return _menu.SubMenu("Combo").Item("useR").GetValue<bool>(); } }
        private static bool UseI { get { return Program._I.Slot != SpellSlot.Unknown && _menu.SubMenu("Combo").Item("useI").GetValue<bool>(); } }

        public static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;

        private static GameObject _ward;

        private static int qTimer;
        private static int wTimer;
        private static int eTimer;

        private static int lastWardCast;

        private static Obj_AI_Hero _target;

        private static bool inKillCombo;
        private static List<Spell> _killCombo;
        private static int _comboStep;
        private static Spell _lastSpell;
        private static Spell _nextSpell;
        private static int _lastSpellCast;

        private static Items.Item _Tiamat = new Items.Item(3077, 185);
        private static Items.Item _Hydra = new Items.Item(3074, 185);
        private static Items.Item _Ghostblade = new Items.Item(3142);
        private static Items.Item _Bilgewater = new Items.Item(3144, 450);
        private static Items.Item _Botrk = new Items.Item(3153, 450);
        private static Items.Item _Hextech = new Items.Item(3146, 700);
        private static Items.Item _Randuins = new Items.Item(3143, 500);

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
			if (Player.ChampionName != "LeeSin")
				return;
			
            _menu = new Menu("yol0 盲僧", "yol0LeeSin", true);
            _menu.AddSubMenu(new Menu("目标选择", "Target Selector"));
            _menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            _menu.AddSubMenu(new Menu("键位", "Keys"));
            _menu.AddSubMenu(new Menu("连招", "Combo"));
            _menu.AddSubMenu(new Menu("R设置", "Insec"));
            _menu.AddSubMenu(new Menu("躲避", "Dodge"));
            _menu.AddSubMenu(new Menu("显示", "Draw"));

            TargetSelector.AddToMenu(_menu.SubMenu("Target Selector"));
            _orbwalker = new Orbwalking.Orbwalker(_menu.SubMenu("Orbwalker"));

            _menu.SubMenu("Keys").AddItem(new MenuItem("Insec", "连招（回旋踢）").SetValue(new KeyBind("X".ToArray()[0], KeyBindType.Press)));
            _menu.SubMenu("Keys").AddItem(new MenuItem("Escape", "逃跑").SetValue(new KeyBind("A".ToArray()[0], KeyBindType.Press)));
            _menu.SubMenu("Keys").AddItem(new MenuItem("Wardjump", "瞬眼").SetValue(new KeyBind("Z".ToArray()[0], KeyBindType.Press)));

            _menu.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("useQ2", "使用 Q2").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("useE2", "使用 E2").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("useR", "使用 R").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("useI", "使用 点燃").SetValue(true));
            _menu.SubMenu("Combo").AddItem(new MenuItem("useItems", "使用 物品").SetValue(true));

            _menu.SubMenu("Insec").AddItem(new MenuItem("method", "回旋踢（连招方式）").SetValue(new StringList(new[] { "仅使用瞬眼", "仅使用闪现", "瞬眼 + 闪现" }, 2)));
            _menu.SubMenu("Insec").AddItem(new MenuItem("mode", "回旋踢（模式）").SetValue(new StringList(new[] { "踢向盟友", "踢向鼠标方向", "踢向塔下" }, 0)));

            _menu.SubMenu("Draw").AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(new Circle(true, Color.Green)));
            _menu.SubMenu("Draw").AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(new Circle(true, Color.Green)));
            _menu.SubMenu("Draw").AddItem(new MenuItem("drawE", "显示 E 范围").SetValue(new Circle(true, Color.Green)));
            _menu.SubMenu("Draw").AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(new Circle(true, Color.Green)));

            _menu.SubMenu("Draw").AddItem(new MenuItem("drawInsec", "Draw Insec").SetValue(true));
            _menu.SubMenu("Draw").AddItem(new MenuItem("drawDamage", "显示血量伤害").SetValue(true));
            _menu.SubMenu("Draw").AddItem(new MenuItem("drawCombo", "显示击杀连招方式").SetValue(true));
            _menu.SubMenu("Draw").AddItem(new MenuItem("drawTarget", "显示攻击目标").SetValue(true));

            _menu.AddToMainMenu();

            SpellDodger.Initialize(_menu.SubMenu("Dodge"));

            if (Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name == "summonerdot")
                _I.Slot = SpellSlot.Summoner1;
            else if (Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name == "summonerdot")
                _I.Slot = SpellSlot.Summoner2;

            if (Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name == "summonerflash")
                _F.Slot = SpellSlot.Summoner1;
            else if (Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name == "summonerflash")
                _F.Slot = SpellSlot.Summoner2;

            _Q.SetSkillshot(0.25f, 60f, 1750f, true, SkillshotType.SkillshotLine);

            Utility.HpBarDamageIndicator.DamageToUnit = delegate(Obj_AI_Hero enemy)
            {
                return (float)GetDamage(enemy);
            };
            GameObject.OnCreate += OnCreateObject;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += KillCombo_OnProcessSpellCast;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        

        #region Events
        private static void OnDraw(EventArgs args)
        {
            if (_menu.SubMenu("Draw").Item("drawQ").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, _Q.Range, _menu.SubMenu("Draw").Item("drawQ").GetValue<Circle>().Color);
            }

            if (_menu.SubMenu("Draw").Item("drawW").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, _W.Range, _menu.SubMenu("Draw").Item("drawW").GetValue<Circle>().Color);
            }

            if (_menu.SubMenu("Draw").Item("drawE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, _E.Range, _menu.SubMenu("Draw").Item("drawE").GetValue<Circle>().Color);
            }

            if (_menu.SubMenu("Draw").Item("drawR").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, _R.Range, _menu.SubMenu("Draw").Item("drawR").GetValue<Circle>().Color);
            }

            if (_menu.SubMenu("Draw").Item("drawTarget").GetValue<bool>() && _target != null && !_target.IsDead)
            {
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red);
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius + 10, Color.Red);
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius + 25, Color.Red);
            }

            if (_menu.SubMenu("Draw").Item("drawInsec").GetValue<bool>() && _target != null && !_target.IsDead && Hud.SelectedUnit != null && _target.NetworkId == Hud.SelectedUnit.NetworkId && _R.IsReady())
            {
                var insecPos = GetInsecPosition(_target);
                Render.Circle.DrawCircle(insecPos.To3D(), 40f, Color.Green);
                var dirPos = (_target.Position.To2D() - insecPos).Normalized();
                var endPos = _target.Position.To2D() + (dirPos * 1200);

                var wts1 = Drawing.WorldToScreen(insecPos.To3D());
                var wts2 = Drawing.WorldToScreen(endPos.To3D());

                Drawing.DrawLine(wts1, wts2, 2, Color.Green);
            }

            if (_menu.SubMenu("Draw").Item("drawCombo").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsVisible && !hero.IsDead))
                {
                    var pos = enemy.HPBarPosition;
                    pos.Y += 35;
                    pos.X += 5;
                    var combo = ComboGenerator.GetKillCombo(enemy);
                    if (combo != null)
                    {
                        Drawing.DrawText(pos.X, pos.Y, Color.Red, ComboGenerator.ComboString(combo));
                    }
                    else
                    {
                        Drawing.DrawText(pos.X, pos.Y, Color.Red, "Not Killable");
                    }
                }
            }

        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (Player.Distance(sender.Position) <= 700 && sender.IsAlly && (sender.Name == "VisionWard" || sender.Name == "SightWard"))
            {
                _ward = sender;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            Utility.HpBarDamageIndicator.Enabled = _menu.SubMenu("Draw").Item("drawDamage").GetValue<bool>();
            _target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
            if (_menu.SubMenu("Keys").Item("Escape").GetValue<KeyBind>().Active)
            {
                Escape();
            }

            if (_menu.SubMenu("Keys").Item("Wardjump").GetValue<KeyBind>().Active)
            {
                Wardjump();
            }

            if (_menu.SubMenu("Keys").Item("Insec").GetValue<KeyBind>().Active)
            {
                if (Hud.SelectedUnit != null && _target != null && _target.NetworkId == Hud.SelectedUnit.NetworkId)
                    Insec(_target);
            }

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && _target != null)
            {
                Combo(_target);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "BlindMonkQOne")
                    qTimer = Environment.TickCount + 2700;
                else if (args.SData.Name == "BlindMonkWOne")
                    wTimer = Environment.TickCount + 2700;
                else if (args.SData.Name == "BlindMonkEOne")
                    eTimer = Environment.TickCount + 2700;
                else if (args.SData.Name == "BlindMonkRKick")
                    Orbwalking.ResetAutoAttackTimer();

                else if (args.SData.Name.Contains("ward") || args.SData.Name.Contains("Trinket"))
                    lastWardCast = Environment.TickCount;

            }
        }
        #endregion
        #region Damage Calculations
        private static double GetDamage(Obj_AI_Hero target)
        {
            var qDmg = _Q.IsReady() ? Player.GetSpellDamage(target, SpellSlot.Q) : 0.0;
            if (_Q.IsReady() && _Q.Instance.Name == "BlindMonkQOne")
            {
                qDmg += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }
            var eDmg = _E.IsReady() ? Player.GetSpellDamage(target, SpellSlot.E) : 0.0;
            var rDmg = _R.IsReady() ? Player.GetSpellDamage(target, SpellSlot.R) : 0.0;
            var aDmg = Player.GetAutoAttackDamage(target);
            var iDmg = 0.0;

            if (_I.Slot != SpellSlot.Unknown && _I.IsReady())
                iDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            return qDmg + eDmg + rDmg + aDmg * 2 + iDmg;
        }

        private static double GetQ2Damage(Obj_AI_Hero target, double dmg)
        {
            var hpafter = target.Health - dmg;
            var qDmg = ((_Q.Level * 30) + 20) + (Player.BaseAttackDamage * 0.9) + (0.08 * (target.MaxHealth - hpafter));
            return Damage.CalcDamage(Player, target, Damage.DamageType.Physical, qDmg);
        }
        #endregion
        #region Combo
        private static void UseItems(Obj_AI_Hero target)
        {
            if (_Tiamat.IsReady() && target.IsValidTarget(_Tiamat.Range))
            {
                _Tiamat.Cast();
            }
            if (_Hydra.IsReady() && target.IsValidTarget(_Hydra.Range))
            {
                _Hydra.Cast();
            }
            if (_Bilgewater.IsReady() && target.IsValidTarget(_Bilgewater.Range))
            {
                _Bilgewater.Cast(target);
            }
            if (_Botrk.IsReady() && target.IsValidTarget(_Botrk.Range))
            {
                _Botrk.Cast(target);
            }
            if (_Hextech.IsReady() && target.IsValidTarget(_Hextech.Range))
            {
                _Hextech.Cast(target);
            }
            if (_Ghostblade.IsReady() && target.IsValidTarget(600))
            {
                _Ghostblade.Cast();
            }
            if (_Randuins.IsReady() && target.IsValidTarget(_Randuins.Range))
            {
                _Randuins.Cast();
            }
        }

        private static void AutoSkills()
        {
            if (_menu.SubMenu("Combo").Item("useW2").GetValue<bool>() && wTimer < Environment.TickCount)
            {
                _W.Cast();
            }
            if (_menu.SubMenu("Combo").Item("useE2").GetValue<bool>() && eTimer < Environment.TickCount)
            {
                _E.Cast();
            }

        }

        private static void Combo(Obj_AI_Hero target)
        {
            if (_menu.SubMenu("Combo").Item("useItems").GetValue<bool>())
                UseItems(target);

            if (!KillCombo(target))
            {
                _killCombo = null;
                _nextSpell = null;
                _lastSpell = null;
                if (_lastSpellCast + 300 <= Environment.TickCount)
                {
                    if (UseQ && _Q.IsReady() && IsQOne && target.IsValidTarget(_Q.Range))
                    {
                        _lastSpellCast = Environment.TickCount;
                        _Q.Cast(target);
                    }
                    if (UseE && _E.IsReady() && IsEOne && target.IsValidTarget(_E.Range))
                    {
                        _lastSpellCast = Environment.TickCount;
                        _E.Cast();
                    }

                    if (target.HasBuff("BlindMonkQOne") && UseQ2 && IsQTwo)
                    {
                        _Q2.CastOnUnit(target);
                    }
                    if (UseE2 && _E2.IsReady() && IsETwo && target.IsValidTarget(_E2.Range) && Player.Mana >= 50)
                    {
                        _E2.Cast();
                    }
                }
            }
        }

        private static bool KillCombo(Obj_AI_Hero target)
        {
            var tmpCombo = ComboGenerator.GetKillCombo(target);
            if (tmpCombo != null && _killCombo == null)
            {
                _killCombo = tmpCombo;
                _comboStep = 0;
                _lastSpell = null;
                _nextSpell = _killCombo[_comboStep];
                inKillCombo = true;
                ComboGenerator.PrintCombo(_killCombo);
                return true;
            }
            else if (tmpCombo == null && inKillCombo && _killCombo != null && _comboStep == _killCombo.Count)
            {
                _killCombo = null;
                _comboStep = 0;
                _lastSpell = null;
                _nextSpell = null;
                inKillCombo = false;
                return false;
            }
            else if (GetDamage(target) < target.Health)
            {
                return false;
            }
            else if (_killCombo != null)
            {
                if (_lastSpellCast + 300 <= Environment.TickCount)
                {
                    if (_nextSpell == _Q)
                    {
                        _lastSpellCast = Environment.TickCount;
                        _Q.Cast(target);
                    }
                    else if (_nextSpell == _Q2)
                    {
                        _lastSpellCast = Environment.TickCount;
                        _Q2.CastOnUnit(target);
                    }
                    else if (_nextSpell == _E)
                    {
                        _lastSpellCast = Environment.TickCount;
                        _E.Cast();
                    }
                    else if (_nextSpell == _I)
                    {
                        _lastSpellCast = Environment.TickCount;
                        _I.CastOnUnit(target);
                    }
                    else if (_nextSpell == _R)
                    {
                        _lastSpellCast = Environment.TickCount;
                        _R.CastOnUnit(target);
                    }
                }
                return true;
            }
            else if (tmpCombo == null && _killCombo == null)
            {
                return false;
            }
            
            return false;
        }

        private static void KillCombo_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (!inKillCombo)
                return;

            switch (args.SData.Name)
            {
                case "BlindMonkQOne": _lastSpell = _Q; _lastSpellCast = Environment.TickCount; break;
                case "blindmonkqtwo": _lastSpell = _Q2; _lastSpellCast = Environment.TickCount; break;
                case "BlindMonkEOne": _lastSpell = _E; _lastSpellCast = Environment.TickCount; break;
                case "BlindMonkRKick": _lastSpell = _R; _lastSpellCast = Environment.TickCount; break;
                case "summonerdot": _lastSpell = _I; _lastSpellCast = Environment.TickCount; break;
                default: return;
            }


            if (_killCombo != null && _killCombo.Count - 1 > _comboStep && _killCombo[_comboStep] == _lastSpell)
            {
                _nextSpell = _killCombo[++_comboStep];
            }
            else
            {
                inKillCombo = false;
                _killCombo = null;
                _comboStep = 0;
                _nextSpell = null;
            }
        }
            
        
        #endregion
        #region Escape

        private static bool CanCastWard()
        {
            return _W.Instance.Name == "BlindMonkWOne" && Environment.TickCount - 2000 > lastWardCast && !Player.HasBuff("BlindMonkWOne");
        }
        private static Obj_AI_Base GetEscapeObject(Vector3 pos, int range = 700)
        {
            var allies = HeroManager.Allies.Where(hero => hero.Distance(pos) <= range).OrderBy(hero => hero.Distance(pos)).ToList();
            var minions = MinionManager.GetMinions(pos, range, MinionTypes.All, MinionTeam.Ally).OrderBy(minion => minion.Distance(pos)).ToList();
            var wards = ObjectManager.Get<Obj_AI_Minion>().Where(obj => (obj.Name.Contains("Ward") || obj.Name.Contains("ward") || obj.Name.Contains("Trinket")) && obj.IsAlly && pos.Distance(obj.Position) <= range).OrderBy(obj => obj.Distance(pos)).ToList();
            foreach (var ally in allies)
            {
                if (!ally.IsMe)
                    return ally;
            }

            if (_ward != null && _ward.IsValid && !_ward.IsDead && Player.Distance(_ward.Position) <= range)
            {
                return _ward as Obj_AI_Base;
            }

            foreach (var ward in wards)
            {
                return ward;
            }

            foreach (var minion in minions)
            {
                return minion;
            }


            return null;
        }

        private static void Wardjump()
        {
            var escapeObject = GetEscapeObject(Game.CursorPos);
            if (escapeObject != null)
            {
                if (CanCastW())
                {
                    wTimer = Environment.TickCount + 3000;
                    _W.CastOnUnit(escapeObject);
                }
            }
            else if (_W.IsReady() && !Player.HasBuff("BlindMonkWOne"))
            {
                var wardSlot = Items.GetWardSlot();
                if (wardSlot.IsValidSlot() && (Player.Spellbook.CanUseSpell(wardSlot.SpellSlot) == SpellState.Ready || wardSlot.Stacks != 0) && CanCastWard())
                {
                    lastWardCast = Environment.TickCount;
                    Player.Spellbook.CastSpell(wardSlot.SpellSlot, GetCorrectedMousePosition());
                }
            }
        }
        private static void Escape()
        {
            var escapeObject = GetEscapeObject(Game.CursorPos);
            if (escapeObject != null)
            {
                if (CanCastW())
                {
                    wTimer = Environment.TickCount + 3000;
                    _W.CastOnUnit(escapeObject);
                }
            }
        }

        private static Vector3 GetCorrectedMousePosition()
        {
            return Player.Position - (Player.Position - Game.CursorPos).Normalized() * 600;
        }
        #endregion
        #region Insec
        private static Vector2 GetInsecPosition(Obj_AI_Hero target)
        {
            if (_menu.SubMenu("Insec").Item("mode").GetValue<StringList>().SelectedValue == "To Ally")
            {
                var nearestTurret = ObjectManager.Get<Obj_AI_Turret>().Where(obj => obj.IsAlly).OrderBy(obj => Player.Distance(obj.Position)).ToList()[0];
                var allies = HeroManager.Allies.Where(hero => hero.Distance(target.Position) <= 1500).OrderByDescending(hero => hero.Distance(target.Position)).ToList();
                if (allies.Count() > 0 && !allies[0].IsMe)
                {
                    var directionVector = (target.Position - allies[0].Position).Normalized().To2D();
                    return target.Position.To2D() + (directionVector * 250);
                }
                var dirVector = (target.Position - nearestTurret.Position).Normalized().To2D();
                return target.Position.To2D() + (dirVector * 250);
            }
            else if (_menu.SubMenu("Insec").Item("mode").GetValue<StringList>().SelectedValue == "To Mouse")
            {
                var directionVector = (target.Position - Game.CursorPos).Normalized().To2D();
                return target.Position.To2D() + (directionVector * 250);
            }
            else
            {
                var nearestTurret = ObjectManager.Get<Obj_AI_Turret>().Where(obj => obj.IsAlly).OrderBy(obj => Player.Distance(obj.Position)).ToList()[0];
                var dirVector = (target.Position - nearestTurret.Position).Normalized().To2D();
                return target.Position.To2D() + (dirVector * 250);
            }
        }

        private static Obj_AI_Base GetInsecObject(Vector3 pos, int range = 700)
        {
            var allies = HeroManager.Allies.Where(hero => hero.Distance(pos) <= range).OrderBy(hero => hero.Distance(pos)).ToList();
            var minions = MinionManager.GetMinions(pos, range, MinionTypes.All, MinionTeam.Ally).OrderBy(minion => minion.Distance(pos)).ToList();
            var wards = ObjectManager.Get<Obj_AI_Minion>().Where(obj => (obj.Name.Contains("ward") || obj.Name.Contains("Ward") || obj.Name.Contains("Trinket")) && obj.IsAlly && pos.Distance(obj.Position) <= range).OrderByDescending(obj => obj.Distance(pos)).ToList();
            foreach (var ally in allies)
            {
                if (!ally.IsMe)
                    return ally;
            }

            if (_ward != null && _ward.IsValid && !_ward.IsDead && Player.Distance(_ward.Position) <= range)
            {
                return _ward as Obj_AI_Base;
            }

            foreach (var minion in minions)
            {
                return minion;
            }

            foreach (var ward in wards)
            {
                return ward;
            }
            return null;
        }

        private static void Insec(Obj_AI_Hero target)
        {
            if (!_R.IsReady())
                return;

            if (!target.IsValidTarget() || target.IsDead)
                return;

            var insecPos = GetInsecPosition(target);
            if (Player.Distance(insecPos) < 150)
            {
                _R.CastOnUnit(target);
                return;
            }

            if (_menu.SubMenu("Insec").Item("method").GetValue<StringList>().SelectedValue == "Wardjump only")
            {
                if (_W.IsReady() && Player.Mana >= 50)
                {
                    if (Player.Distance(insecPos) <= 600)
                    {
                        var insecObj = GetInsecObject(insecPos.To3D(), 200);
                        if (insecObj != null)
                        {
                            if (Player.Distance(insecObj.Position) <= 600)
                            {
                                _W.CastOnUnit(insecObj);
                            }
                        }
                        else
                        {
                            var slot = Items.GetWardSlot();
                            if (slot.IsValidSlot() && Player.Spellbook.CanUseSpell(slot.SpellSlot) == SpellState.Ready)
                            {
                                Player.Spellbook.CastSpell(slot.SpellSlot, insecPos.To3D());
                            }
                            return;
                        }
                    }
                    if (!Items.GetWardSlot().IsValidSlot())
                        return;

                    if (!_Q.IsReady())
                        return;

                    if (Player.Mana >= 130 && Player.Distance(insecPos) > 600)
                    {
                        _Q.Cast(target);
                    }

                    if (_target.HasBuff("BlindMonkQOne") && Player.Mana >= 80 && target.Distance(insecPos) <= 600)
                    {
                        _Q2.CastOnUnit(target);
                    }
                }
            }
            else if (_menu.SubMenu("Insec").Item("method").GetValue<StringList>().SelectedValue == "Flash Only")
            {
                if (_F.Slot == SpellSlot.Unknown || !_F.IsReady())
                    return;

                if (Player.Distance(insecPos) < 425)
                {
                    _F.Cast(insecPos);
                    return;
                }

                if (!_Q.IsReady())
                    return;
                if (Player.Mana >= 80 && Player.Distance(insecPos) > 600)
                {
                    _Q.Cast(target);
                }

                if (_target.HasBuff("BlindMonkQOne") && Player.Mana >= 80 && target.Distance(insecPos) <= 325)
                {
                    _Q2.CastOnUnit(target);
                }
            }
            else
            {
                if (_W.IsReady() && Player.Mana >= 50)
                {
                    if (Player.Distance(insecPos) <= 600)
                    {
                        var insecObj = GetInsecObject(insecPos.To3D(), 200);
                        if (insecObj != null)
                        {
                            if (Player.Distance(insecObj.Position) <= 600)
                            {
                                _W.CastOnUnit(insecObj);
                            }
                        }
                        else
                        {
                            var slot = Items.GetWardSlot();
                            if (slot.IsValidSlot() && Player.Spellbook.CanUseSpell(slot.SpellSlot) == SpellState.Ready)
                            {
                                Player.Spellbook.CastSpell(slot.SpellSlot, insecPos.To3D());
                            }
                            return;
                        }
                    }
                    if (!Items.GetWardSlot().IsValidSlot())
                        return;

                    if (!_Q.IsReady())
                        return;

                    if (Player.Mana >= 130 && Player.Distance(insecPos) > 600)
                    {
                        _Q.Cast(target);
                    }

                    if (_target.HasBuff("BlindMonkQOne") && Player.Mana >= 80 && target.Distance(insecPos) <= 600)
                    {
                        _Q2.CastOnUnit(target);
                    }
                }
                else if (_F.Slot != SpellSlot.Unknown && _F.IsReady() && _W.IsReady())
                {
                    if (Player.Distance(insecPos) < 425 && _F.Slot != SpellSlot.Unknown && _F.IsReady())
                    {
                        _F.Cast(insecPos);
                        return;
                    }

                    if (!_Q.IsReady())
                        return;

                    if (Player.Mana >= 80 && Player.Distance(insecPos) > 600)
                    {
                        _Q.Cast(target);
                    }

                    if (_target.HasBuff("BlindMonkQOne") && Player.Mana >= 80 && target.Distance(insecPos) <= 325)
                    {
                        _Q2.CastOnUnit(target);
                    }
                }
            }
        }
        #endregion
        #region Cast Checks
        private static bool CanCastQ()
        {
            return _Q.IsReady();
        }

        private static bool CanCastW()
        {
            return _W.IsReady() && _W.Instance.Name == "BlindMonkWOne";
        }

        private static bool CanCastE()
        {
            return _E.IsReady() && _E.Instance.Name == "BlindMonkEOne";
        }

        #endregion
    }
}
