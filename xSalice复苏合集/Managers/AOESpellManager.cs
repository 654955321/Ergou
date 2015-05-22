using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Base;
using xSaliceResurrected.Mid;

namespace xSaliceResurrected.Managers
{
    internal class AoeSpellManager : SpellBase
    {
        private static Menu _menu;
        private static Boolean _qEnabled, _wEnabled, _eEnabled, _rEnabled, _qeEnabled;

        public static Menu AddHitChanceMenuCombo(Boolean q, Boolean w, Boolean e, Boolean r, Boolean qe = false)
        {
            _menu = new Menu("AOE 技能消耗", "AOE Spells");

            _menu.AddItem(new MenuItem("enabledAOE", "启用", true).SetValue(true));

            if (q)
            {
                _menu.AddItem(new MenuItem("qAutoLaunch", "自动 Q 如果敌人数量 >=", true).SetValue(new Slider(3, 1, 5)));
                _qEnabled = true;
            }
            if (w)
            {
                _menu.AddItem(new MenuItem("wAutoLaunch", "自动 W 如果敌人数量 >=", true).SetValue(new Slider(3, 1, 5)));
                _wEnabled = true;
            }
            if (e)
            {
                _menu.AddItem(new MenuItem("eAutoLaunch", "自动 E 如果敌人数量 >=", true).SetValue(new Slider(3, 1, 5)));
                _eEnabled = true;
            }
            if (r)
            {
                _menu.AddItem(new MenuItem("rAutoLaunch", "自动 R 如果敌人数量 >=", true).SetValue(new Slider(3, 1, 5)));
                _rEnabled = true;
            }
            if (qe)
            {
                _menu.AddItem(new MenuItem("qeAutoLaunch", "自动 QE 如果敌人数量 >=", true).SetValue(new Slider(3, 1, 5)));
                _qeEnabled = true;
            }

            Game.OnUpdate += Mec;
            return _menu;
        }

        private static void Mec(EventArgs arg)
        {
            if (!_menu.Item("enabledAOE", true).GetValue<bool>())
                return;

            if (_qeEnabled)
                CastComboMec(QExtend, _menu.Item("qeAutoLaunch", true).GetValue<Slider>().Value);
            if (_qEnabled)
                CastMec(Q, _menu.Item("qAutoLaunch", true).GetValue<Slider>().Value);
            if (_wEnabled)
                CastMec(W, _menu.Item("wAutoLaunch", true).GetValue<Slider>().Value);
            if (_eEnabled)
                CastMec(E, _menu.Item("eAutoLaunch", true).GetValue<Slider>().Value);
            if (_rEnabled)
                CastMec(R, _menu.Item("rAutoLaunch", true).GetValue<Slider>().Value);

        }

        private static void CastMec(Spell spell, int minHit)
        {
            if (!spell.IsReady())
                return;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range)))
            {
                var pred = spell.GetPrediction(target, true);
                 Obj_AI_Hero target1 = target;
                var nearByEnemies = spell.Collision || spell.Type == SkillshotType.SkillshotCircle ? 
                    1 + ObjectManager.Get<Obj_AI_Hero>().Where(x => x.NetworkId != target1.NetworkId && x.IsValidTarget(spell.Range + 200))
                    .Count(x => pred.CastPosition.Distance(spell.GetPrediction(x, true).UnitPosition) < spell.Width + x.BoundingRadius) : pred.AoeTargetsHitCount;

                if (nearByEnemies >= minHit)
                {
                    Console.WriteLine("Hit Regular: " + nearByEnemies);
                    spell.Cast(target);
                    return;
                }
            }
        }

        private static void CastComboMec(Spell spell, int minHit)
        {
            if (!spell.IsReady() || !E.IsReady())
                return;

            const int gateDis = 200;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range)))
            {
                var tarPred = spell.GetPrediction(target, true);
                Obj_AI_Hero target1 = target;
                int nearByTargets = 1 + ObjectManager.Get<Obj_AI_Hero>().Where(x => x.NetworkId != target1.NetworkId && x.IsValidTarget(spell.Range + 200))
                    .Count(x => tarPred.CastPosition.Distance(spell.GetPrediction(x, true).UnitPosition) < spell.Width + x.BoundingRadius);

                Vector3 gateVector = ObjectManager.Player.Position + Vector3.Normalize(target.ServerPosition - ObjectManager.Player.Position)*gateDis;

                if (ObjectManager.Player.Distance(tarPred.CastPosition) < spell.Range + 100 && nearByTargets >= minHit)
                {
                    if (Jayce.HammerTime && R.IsReady() && Jayce.CanQcd == 0 && Jayce.CanEcd == 0)
                        R.Cast();
                    else if(Jayce.HammerTime)
                        return;

                    Console.WriteLine("Hit Combo: " + nearByTargets);
                    E.Cast(gateVector);
                    spell.Cast(tarPred.CastPosition);
                    return;
                }
            }
        }
    }
}