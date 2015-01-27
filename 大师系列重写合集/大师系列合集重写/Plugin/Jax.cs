using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    class Jax : Common.Helper
    {
        private bool WardCasted = false;
        private int RCount = 0;
        private Vector3 WardPlacePos = default(Vector3);

        public Jax()
        {
            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 375);
            R = new Spell(SpellSlot.R);
            Q.SetTargetted(0.5f, float.MaxValue);
            W.SetTargetted(0.2333f, float.MaxValue);

            var ChampMenu = new Menu("Plugin", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("Combo", "Combo");
                {
                    AddItem(ComboMenu, "Q", "Use Q");
                    AddItem(ComboMenu, "W", "Use W");
                    AddItem(ComboMenu, "E", "Use E");
                    AddItem(ComboMenu, "R", "Use R");
                    AddItem(ComboMenu, "RHpU", "-> If Player Hp Under", 60);
                    AddItem(ComboMenu, "RCountA", "-> If Enemy Above", 2, 1, 5);
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("Harass", "Harass");
                {
                    AddItem(HarassMenu, "Q", "Use Q");
                    AddItem(HarassMenu, "QHpA", "-> If Hp Above", 20);
                    AddItem(HarassMenu, "W", "Use W");
                    AddItem(HarassMenu, "E", "Use E");
                    AddItem(HarassMenu, "R", "Save Q/W For Third R");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("Lane/Jungle Clear", "Clear");
                {
                    var SmiteMob = new Menu("Smite Mob If Killable", "SmiteMob");
                    {
                        AddItem(SmiteMob, "Smite", "Use Smite");
                        AddItem(SmiteMob, "Baron", "-> Baron Nashor");
                        AddItem(SmiteMob, "Dragon", "-> Dragon");
                        AddItem(SmiteMob, "Red", "-> Red Brambleback");
                        AddItem(SmiteMob, "Blue", "-> Blue Sentinel");
                        AddItem(SmiteMob, "Krug", "-> Ancient Krug");
                        AddItem(SmiteMob, "Gromp", "-> Gromp");
                        AddItem(SmiteMob, "Raptor", "-> Crimson Raptor");
                        AddItem(SmiteMob, "Wolf", "-> Greater Murk Wolf");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    AddItem(ClearMenu, "Q", "Use Q");
                    AddItem(ClearMenu, "W", "Use W");
                    AddItem(ClearMenu, "E", "Use E");
                    AddItem(ClearMenu, "Item", "Use Tiamat/Hydra");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var FleeMenu = new Menu("Flee", "Flee");
                {
                    AddItem(FleeMenu, "Q", "Use Q");
                    AddItem(FleeMenu, "PinkWard", "-> Ward Jump Use Pink Ward", false);
                    ChampMenu.AddSubMenu(FleeMenu);
                }
                var MiscMenu = new Menu("Misc", "Misc");
                {
                    var KillStealMenu = new Menu("Kill Steal", "KillSteal");
                    {
                        AddItem(KillStealMenu, "Q", "Use Q");
                        AddItem(KillStealMenu, "Ignite", "Use Ignite");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    var AntiGapMenu = new Menu("Anti Gap Closer", "AntiGap");
                    {
                        AddItem(AntiGapMenu, "E", "Use E");
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy))
                        {
                            foreach (var Spell in AntiGapcloser.Spells.Where(i => i.ChampionName == Obj.ChampionName)) AddItem(AntiGapMenu, Obj.ChampionName + "_" + Spell.Slot.ToString(), "-> Skill " + Spell.Slot.ToString() + " Of " + Obj.ChampionName);
                        }
                        MiscMenu.AddSubMenu(AntiGapMenu);
                    }
                    var InterruptMenu = new Menu("Interrupt", "Interrupt");
                    {
                        AddItem(InterruptMenu, "E", "Use E");
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy))
                        {
                            foreach (var Spell in Interrupter.Spells.Where(i => i.ChampionName == Obj.ChampionName)) AddItem(InterruptMenu, Obj.ChampionName + "_" + Spell.Slot.ToString(), "-> Skill " + Spell.Slot.ToString() + " Of " + Obj.ChampionName);
                        }
                        MiscMenu.AddSubMenu(InterruptMenu);
                    }
                    AddItem(MiscMenu, "WLastHit", "Use W To Last Hit");
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("Draw", "Draw");
                {
                    AddItem(DrawMenu, "Q", "Q Range", false);
                    AddItem(DrawMenu, "E", "E Range", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                MainMenu.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Obj_AI_Hero.OnProcessSpellCast += OnProcessSpellCast;
            Orbwalk.BeforeAttack += BeforeAttack;
            Orbwalk.AfterAttack += AfterAttack;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
            {
                if (Player.IsDead) RCount = 0;
                return;
            }
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit)
            {
                LastHit();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee) Flee(Game.CursorPos);
            KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "E") && E.Level > 0) Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.IsDead || !GetValue<bool>("AntiGap", "E") || !GetValue<bool>("AntiGap", gapcloser.Sender.ChampionName + "_" + gapcloser.Slot.ToString()) || !E.CanCast(gapcloser.Sender)) return;
            if (!Player.HasBuff("JaxEvasion"))
            {
                if (E.Cast(PacketCast) && E.Cast(PacketCast)) return;
            }
            else if (E.Cast(PacketCast)) return;
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Player.IsDead || !GetValue<bool>("Interrupt", "E") || !GetValue<bool>("Interrupt", unit.ChampionName + "_" + spell.Slot.ToString()) || !E.IsReady()) return;
            if (E.IsInRange(unit))
            {
                if (!Player.HasBuff("JaxEvasion"))
                {
                    if (E.Cast(PacketCast) && E.Cast(PacketCast)) return;
                }
                else if (E.Cast(PacketCast)) return;
            }
            else if (Q.CanCast(unit) && Player.Mana >= Q.Instance.ManaCost + (Player.HasBuff("JaxEvasion") ? 0 : E.Instance.ManaCost) && Q.CastOnUnit(unit, PacketCast)) return;
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name == "jaxrelentlessattack") RCount = 0;
        }

        private void BeforeAttack(Orbwalk.BeforeAttackEventArgs Args)
        {
            if (!W.IsReady()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit && GetValue<bool>("Misc", "WLastHit") && Args.Target is Obj_AI_Minion && CanKill((Obj_AI_Minion)Args.Target, W, GetBonusDmg((Obj_AI_Minion)Args.Target)) && W.Cast(PacketCast)) return;
        }

        private void AfterAttack(AttackableUnit Target)
        {
            if (R.Level > 0) RCount += 1;
            if (!W.IsReady()) return;
            if ((Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass) && GetValue<bool>(Orbwalk.CurrentMode.ToString(), "W") && Target is Obj_AI_Hero && W.Cast(PacketCast) && Player.IssueOrder(GameObjectOrder.AttackUnit, Target))
            {
                return;
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && GetValue<bool>("Clear", "W") && Target is Obj_AI_Minion && W.Cast(PacketCast) && Player.IssueOrder(GameObjectOrder.AttackUnit, Target)) return;
        }

        private void NormalCombo(string Mode)
        {
            if (GetValue<bool>(Mode, "E") && E.IsReady() && (Mode == "Combo" || (Mode == "Harass" && (!GetValue<bool>(Mode, "R") || (GetValue<bool>(Mode, "R") && (R.Level == 0 || (R.Level > 0 && !W.IsReady() && !Q.IsReady())))))))
            {
                if (!Player.HasBuff("JaxEvasion"))
                {
                    if (GetValue<bool>(Mode, "Q") && Q.IsReady() && E.GetTarget() == null)
                    {
                        var Target = Q.GetTarget();
                        if (Target != null && E.Cast(PacketCast) && Q.CastOnUnit(Target, PacketCast)) return;
                    }
                    else if (E.GetTarget() != null && E.Cast(PacketCast)) return;
                }
                else if (Player.CountEnemiesInRange(E.Range + 100) == 1 && E.GetTarget() != null && Player.Distance(E.GetTarget(), true) >= Math.Pow(E.Range - 30, 2) && E.Cast(PacketCast)) return;
            }
            if (Mode == "Harass" && GetValue<bool>(Mode, "R") && R.Level > 0 && GetValue<bool>(Mode, "W") && GetValue<bool>(Mode, "Q") && Player.Mana >= W.Instance.ManaCost + Q.Instance.ManaCost)
            {
                if (RCount == 2 && W.IsReady() && Q.IsReady())
                {
                    var Target = Q.GetTarget();
                    if (Target != null && W.Cast(PacketCast) && Q.CastOnUnit(Target, PacketCast)) return;
                }
            }
            else
            {
                if (GetValue<bool>(Mode, "W") && W.IsReady() && GetValue<bool>(Mode, "Q") && Q.IsReady() && Player.Mana >= W.Instance.ManaCost + Q.Instance.ManaCost)
                {
                    var Target = Q.GetTarget();
                    if (Target != null && CanKill(Target, Q, Q.GetDamage(Target) + GetBonusDmg(Target)) && W.Cast(PacketCast) && Q.CastOnUnit(Target, PacketCast)) return;
                }
                if (GetValue<bool>(Mode, "Q") && Q.IsReady())
                {
                    var Target = Q.GetTarget();
                    if (Target != null)
                    {
                        if (((Player.HasBuff("JaxEmpowerTwo") && CanKill(Target, Q, Q.GetDamage(Target) + GetBonusDmg(Target))) || CanKill(Target, Q)) && Q.CastOnUnit(Target, PacketCast))
                        {
                            return;
                        }
                        else if (Mode == "Combo" || (Mode == "Harass" && Player.HealthPercentage() >= GetValue<Slider>(Mode, "QHpA").Value))
                        {
                            if ((Player.Distance(Target, true) > Math.Pow(Orbwalk.GetAutoAttackRange(Player, Target) + 40, 2) || (GetValue<bool>(Mode, "E") && E.IsReady() && Player.HasBuff("JaxEvasion") && !E.IsInRange(Target))) && Q.CastOnUnit(Target, PacketCast)) return;
                        }
                    }
                }
            }
            if (Mode == "Combo" && GetValue<bool>(Mode, "R") && R.IsReady())
            {
                var RCount = GetValue<Slider>(Mode, "RCountA").Value;
                if (((RCount > 1 && (Player.CountEnemiesInRange(Q.Range) >= RCount || Q.GetTarget() != null)) || (RCount == 1 && Q.GetTarget() != null)) && Player.HealthPercentage() < GetValue<Slider>(Mode, "RHpU").Value && R.Cast(PacketCast)) return;
            }
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0) return;
            if (GetValue<bool>("Clear", "E") && E.IsReady() && !Player.HasBuff("JaxEvasion"))
            {
                if (GetValue<bool>("Clear", "Q") && Q.IsReady() && minionObj.Count(i => E.IsInRange(i)) == 0)
                {
                    foreach (var Obj in minionObj)
                    {
                        if (minionObj.Count(i => i.Distance(Obj, true) <= E.RangeSqr) > 1 && E.Cast(PacketCast) && Q.CastOnUnit(Obj, PacketCast)) return;
                    }
                }
                else if ((minionObj.Count(i => i.MaxHealth >= 1200 && E.IsInRange(i)) > 0 || minionObj.Count(i => E.IsInRange(i)) > 2) && E.Cast(PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "W") && W.IsReady() && GetValue<bool>("Clear", "Q") && Q.IsReady() && Player.Mana >= W.Instance.ManaCost + Q.Instance.ManaCost)
            {
                var Obj = minionObj.Find(i => i.MaxHealth >= 1200 && CanKill(i, Q, Q.GetDamage(i) + GetBonusDmg(i)));
                if (Obj != null && W.Cast(PacketCast) && Q.CastOnUnit(Obj, PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var Obj = minionObj.Find(i => i.MaxHealth >= 1200 && ((Player.HasBuff("JaxEmpowerTwo") && CanKill(i, Q, Q.GetDamage(i) + GetBonusDmg(i))) || CanKill(i, Q)));
                if (Obj == null && (minionObj.Count(i => Player.Distance(i, true) <= Math.Pow(Orbwalk.GetAutoAttackRange(Player, i) + 40, 2)) == 0 || (GetValue<bool>("Clear", "E") && E.IsReady() && Player.HasBuff("JaxEvasion") && minionObj.Count(i => E.IsInRange(i)) == 0))) Obj = minionObj.MinOrDefault(i => i.Health);
                if (Obj != null && Q.CastOnUnit(Obj, PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "Item"))
            {
                var Item = Hydra.IsReady() ? Hydra : Tiamat;
                if (Item.IsReady() && (minionObj.Count(i => Item.IsInRange(i)) > 2 || minionObj.Any(i => i.MaxHealth >= 1200 && i.Distance(Player, true) <= Math.Pow(Item.Range - 80, 2))) && Item.Cast()) return;
            }
            if (GetValue<bool>("SmiteMob", "Smite") && Smite.IsReady())
            {
                var Obj = minionObj.Find(i => i.Team == GameObjectTeam.Neutral && CanSmiteMob(i.Name));
                if (Obj != null && CastSmite(Obj)) return;
            }
        }

        private void LastHit()
        {
            if (!GetValue<bool>("Misc", "WLastHit") || !W.IsReady() || !Player.HasBuff("JaxEmpowerTwo")) return;
            var minionObj = MinionManager.GetMinions(Orbwalk.GetAutoAttackRange() + 50, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FindAll(i => CanKill(i, W, GetBonusDmg(i)));
            if (minionObj.Count == 0 || Player.IssueOrder(GameObjectOrder.AttackUnit, minionObj.First())) return;
        }

        private bool Flee(Vector3 Pos)
        {
            if (!GetValue<bool>("Flee", "Q") || !Q.IsReady()) return false;
            var JumpPos = Pos;
            if (GetWardSlot() != null && !WardCasted && Player.Distance(Pos) > GetWardRange()) JumpPos = Player.Position.Extend(Pos, GetWardRange());
            var Obj = ObjectManager.Get<Obj_AI_Base>().FindAll(i => !i.IsMe && !(i is Obj_AI_Turret) && i.IsValidTarget(Q.Range + i.BoundingRadius, false) && i.Distance(WardCasted ? WardPlacePos : JumpPos) < 200).MinOrDefault(i => i.Distance(WardCasted ? WardPlacePos : JumpPos));
            if (Obj != null && Q.CastOnUnit(Obj, PacketCast)) return true;
            if (GetWardSlot() != null && !WardCasted)
            {
                Player.Spellbook.CastSpell(GetWardSlot().SpellSlot, JumpPos);
                WardPlacePos = JumpPos;
                Utility.DelayAction.Add(800, () => WardPlacePos = default(Vector3));
                WardCasted = true;
                Utility.DelayAction.Add(800, () => WardCasted = false);
                return false;
            }
            return false;
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (Target != null && CastIgnite(Target)) return;
            }
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var Target = Q.GetTarget();
                if (Target != null && CanKill(Target, Q, Q.GetDamage(Target) + ((!CanKill(Target, Q) && ((W.IsReady() && Player.Mana >= W.Instance.ManaCost + Q.Instance.ManaCost) || Player.HasBuff("JaxEmpowerTwo"))) ? GetBonusDmg(Target) : 0)))
                {
                    if (W.IsReady() && !CanKill(Target, Q) && Player.Mana >= W.Instance.ManaCost + Q.Instance.ManaCost && W.Cast(PacketCast) && Q.CastOnUnit(Target, PacketCast)) return;
                    if (((!CanKill(Target, Q) && Player.HasBuff("JaxEmpowerTwo")) || CanKill(Target, Q)) && Q.CastOnUnit(Target, PacketCast)) return;
                }
            }
        }

        private double GetBonusDmg(Obj_AI_Base Target)
        {
            double DmgItem = 0;
            if (Sheen.IsOwned() && ((Sheen.IsReady() && W.IsReady()) || Player.HasBuff("Sheen")) && Player.BaseAttackDamage > DmgItem) DmgItem = Player.BaseAttackDamage;
            if (Trinity.IsOwned() && ((Trinity.IsReady() && W.IsReady()) || Player.HasBuff("Sheen")) && Player.BaseAttackDamage * 2 > DmgItem) DmgItem = Player.BaseAttackDamage * 2;
            return ((W.IsReady() || Player.HasBuff("JaxEmpowerTwo")) ? W.GetDamage(Target) : 0) + (RCount == 2 ? R.GetDamage(Target) : 0) + Player.GetAutoAttackDamage(Target, true) + Player.CalcDamage(Target, Damage.DamageType.Physical, DmgItem);
        }
    }
}