using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Yasuo
{
    public static class YasuoUtility
    {
        private const int Delay = 150;
        private const float MinDistance = 400;
        private static int _lastMoveCommandT;
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        ///     Checks if the base object has Yasuo's 3rd Q passive (Whirlwind)
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <returns>True/False on the passive state</returns>
        public static bool HasWhirlwind(this Obj_AI_Base @base)
        {
            return @base.HasBuff("YasuoQ3W");
        }

        /// <summary>
        ///     Checks if the base object can dash onto target.
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <param name="target">Target Object</param>
        /// <returns>True/False on dashable state</returns>
        public static bool IsDashable(this Obj_AI_Base @base, Obj_AI_Base target)
        {
            return @base.Distance(target.Position) < YasuoSpells.E.Range && !target.HasBuff("YasuoDashWrapper");
        }

        /// <summary>
        ///     Calculates the ending poisition of the dash.
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <param name="target">Target Object</param>
        /// <returns>Vector2 Ending Position</returns>
        public static Vector2 GetDashingEnd(this Obj_AI_Base @base, Obj_AI_Base target)
        {
            if (!target.IsValidTarget())
            {
                return Vector2.Zero;
            }

            var baseX = @base.Position.X; // => Base X axis
            var baseY = @base.Position.Y; // => Base Y axis
            var targetX = target.Position.X; // => Target X axis
            var targetY = target.Position.Y; // => Target Y axis

            var vector = new Vector2(targetX - baseX, targetY - baseY); // => Vector2
            var sqrt = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y); // => Sqaure(x)

            var x = (float) (baseX + (YasuoSpells.E.Range * (vector.X / sqrt))); // => Ending X axis
            var y = (float) (baseY + (YasuoSpells.E.Range * (vector.Y / sqrt))); // => Ending Y axis

            return new Vector2(x, y).Extend(@base.Position.To2D(), -150f); // => extand by 150f to oppsite direction
        }

        /// <summary>
        ///     Calculates the real auto attack range of the base object.
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <returns>Float Attack Range</returns>
        public static float GetAutoAttackRange(this Obj_AI_Base @base)
        {
            return (@base.AttackRange + @base.BoundingRadius);
        }

        /// <summary>
        ///     Checks if the base object is knocked up.
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <param name="selfKnockup">Only self knockups</param>
        /// <returns>Knockup status</returns>
        public static bool IsKnockedup(this Obj_AI_Base @base, bool selfKnockup = false)
        {
            return selfKnockup
                ? @base.HasBuff("yasuoq3mis")
                : @base.HasBuffOfType(BuffType.Knockup) || @base.HasBuffOfType(BuffType.Knockback);
        }

        /// <summary>
        ///     Calculates the time left for the base object before ending knocked bufftype.
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <returns>Time left for knocked buff type to end</returns>
        public static float KnockupTimeLeft(this Obj_AI_Base @base)
        {
            var buff = @base.Buffs.Find(b => b.Type.Equals(BuffType.Knockup) || b.Type.Equals(BuffType.Knockback));
            return (buff != null) ? buff.EndTime - Game.Time : -1f;
        }

        /// <summary>
        ///     Calculates Dash Data
        /// </summary>
        /// <param name="base">Base Object</param>
        /// <param name="vector3">Vector3 toPosition</param>
        /// <param name="target">Target Object</param>
        /// <param name="ignoreTower">Ignore Towers</param>
        /// <returns>DashData Object</returns>
        public static DashData? GetDashData(this Obj_AI_Base @base,
            Vector3 vector3,
            Obj_AI_Hero target = null,
            bool ignoreTower = true)
        {
            var rVector = Vector3.Zero;
            Obj_AI_Base rAiBase = null;

            if (!vector3.IsValid())
            {
                return null;
            }

            var list =
                ObjectManager.Get<Obj_AI_Base>()
                    .FindAll(o => o.Distance(@base) < YasuoSpells.E.Range && @base.IsDashable(o) && o.IsTargetable);
            foreach (var o in list)
            {
                var vector = @base.Position + (o.Position - @base.Position).Normalized() * YasuoSpells.E.Range;

                if (!ignoreTower && vector.UnderTurret(true) || o == target)
                {
                    continue;
                }

                if (!rVector.IsValid())
                {
                    rVector = vector;
                    rAiBase = o;
                }
                else if (vector.Distance(vector3) < rVector.Distance(vector3))
                {
                    rVector = vector;
                    rAiBase = o;
                }
            }

            if (!rAiBase.IsValidTarget() && target != null && target.Distance(@base.Position) < YasuoSpells.E.Range)
            {
                var vector = @base.Position + (target.Position - @base.Position).Normalized() * YasuoSpells.E.Range;

                if (!ignoreTower && vector.UnderTurret(true))
                {
                    return null;
                }

                rAiBase = target;
                rVector = vector;
            }

            if (rAiBase.IsValidTarget())
            {
                return new DashData { ObjAiBase = rAiBase, Vector3 = rVector };
            }
            return null;
        }

        /// <summary>
        ///     Quick usage of moving a hero
        /// </summary>
        /// <param name="player">Hero Object</param>
        /// <param name="pVector3">Position to Move</param>
        /// <param name="holdAreaRadius">Hold Radius</param>
        public static void MoveTo(this Obj_AI_Hero @player, Vector3 pVector3, float holdAreaRadius = 0)
        {
            if (Environment.TickCount - _lastMoveCommandT < Delay)
            {
                return;
            }

            _lastMoveCommandT = Environment.TickCount;

            if (@player.ServerPosition.Distance(pVector3) < holdAreaRadius)
            {
                if (@player.Path.Count() <= 1)
                {
                    return;
                }

                @player.IssueOrder(GameObjectOrder.HoldPosition, @player.ServerPosition);

                return;
            }

            var point = @player.ServerPosition +
                        ((Random.NextFloat(0.6f, 1) + 0.2f) * MinDistance) *
                        (pVector3.To2D() - @player.ServerPosition.To2D()).Normalized().To3D();

            @player.IssueOrder(GameObjectOrder.MoveTo, point);
        }

        /// <summary>
        ///     DashData struct
        /// </summary>
        public struct DashData
        {
            public Obj_AI_Base ObjAiBase;
            public Vector3 Vector3;
        }
    }
}