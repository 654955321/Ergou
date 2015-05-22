#region LICENSE

// Copyright 2014 - 2014 SpellDetector
// Skillshot.cs is part of SpellDetector.
// SpellDetector is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// SpellDetector is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with SpellDetector. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Oracle.Core.Helpers;
using SharpDX;
using SpellDetector.Helpers;
using Collision = SpellDetector.Helpers.Collision;
using SpellSlot = Oracle.Core.Targeted.SpellSlot;

#endregion

namespace Oracle.Core.Skillshots
{
    public enum SkillShotType
    {
        SkillshotCircle,
        SkillshotLine,
        SkillshotMissileLine,
        SkillshotCone,
        SkillshotMissileCone,
        SkillshotRing,
    }

    public enum DetectionType
    {
        RecvPacket,
        ProcessSpell
    }

    public struct SafePathResult
    {
        public FoundIntersection Intersection;
        public bool IsSafe;

        public SafePathResult(bool isSafe, FoundIntersection intersection)
        {
            IsSafe = isSafe;
            Intersection = intersection;
        }
    }

    public struct FoundIntersection
    {
        public Vector2 ComingFrom;
        public float Distance;
        public Vector2 Point;
        public int Time;
        public bool Valid;

        public FoundIntersection(float distance, int time, Vector2 point, Vector2 comingFrom)
        {
            Distance = distance;
            ComingFrom = comingFrom;
            Valid = (point.X != 0) && (point.Y != 0);
            Point = point + 10*(ComingFrom - point).Normalized();
            Time = time;
        }
    }

    public class SkillshotData
    {
        public bool AddHitbox;
        public bool CanBeRemoved = false;
        public bool Centered;
        public string ChampionName;
        public CollisionObjectTypes[] CollisionObjects = {};
        public int DangerValue;
        public int Delay;
        public bool DisableFowDetection = false;
        public bool DontAddExtraDuration;
        public bool DontCross = false;
        public bool DontRemove = false;
        public int ExtraDuration;
        public string[] ExtraMissileNames = {};
        public int ExtraRange = -1;
        public string[] ExtraSpellNames = {};
        public bool FixedRange;
        public bool ForceRemove = false;
        public string FromObject = "";
        public string[] FromObjects = {};
        public int Id = -1;
        public bool Invert;
        public bool IsDangerous = false;
        public int MissileAccel = 0;
        public bool MissileDelayed;
        public bool MissileFollowsUnit;
        public int MissileMaxSpeed;
        public int MissileMinSpeed;
        public int MissileSpeed;
        public string MissileSpellName = "";
        public float MultipleAngle;
        public int MultipleNumber = -1;
        public int RingRadius;
        public SpellSlot Slot;
        public string SpellName;
        public string ToggleParticleName = "";
        public SkillShotType Type;
        private int _radius;
        private int _range;

        public SkillshotData()
        {
        }

        public SkillshotData(string championName,
            string spellName,
            SpellSlot slot,
            SkillShotType type,
            int delay,
            int range,
            int radius,
            int missileSpeed,
            bool addHitbox,
            bool fixedRange,
            int defaultDangerValue)
        {
            ChampionName = championName;
            SpellName = spellName;
            Slot = slot;
            Type = type;
            Delay = delay;
            Range = range;
            _radius = radius;
            MissileSpeed = missileSpeed;
            AddHitbox = addHitbox;
            FixedRange = fixedRange;
            DangerValue = defaultDangerValue;
        }

        public int Radius
        {
            get
            {
                return (!AddHitbox)
                    ? _radius
                    : _radius + (int) ObjectManager.Player.BoundingRadius;
            }
            set { _radius = value; }
        }

        public int RawRadius
        {
            get { return _radius; }
        }

        public int RawRange
        {
            get { return _range; }
        }

        public int Range
        {
            get { return _range; }
            set { _range = value; }
        }
    }

    public class Skillshot
    {
        public DetectionType DetectionType { get; set; }
        public SkillshotGeometry.Circle Circle { get; set; }
        public SkillshotGeometry.Polygon Polygon { get; set; }
        public SkillshotGeometry.Rectangle Rectangle { get; set; }
        public SkillshotGeometry.Ring Ring { get; set; }
        public SkillshotGeometry.Sector Sector { get; set; }

        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }
        public Vector2 Direction { get; set; }

        public bool ForceDisabled { get; set; }
        public Vector2 MissilePosition { get; set; }
        public SkillshotData SkillshotData { get; set; }
        public int StartTick { get; set; }
        public Obj_AI_Base Caster { get; set; }

        public Vector2 CollisionEnd
        {
            get
            {
                if (_collisionEnd.IsValid())
                {
                    return _collisionEnd;
                }

                if (IsGlobal)
                {
                    return GlobalGetMissilePosition(0) +
                           Direction*SkillshotData.MissileSpeed*
                           (0.5f + SkillshotData.Radius*2/ObjectManager.Player.MoveSpeed);
                }

                return EndPosition;
            }
        }

        public bool IsGlobal
        {
            get { return SkillshotData.RawRange == 20000; }
        }

        private Vector2 _collisionEnd;
        private int _lastCollisionCalc;

        public Skillshot(DetectionType detectionType, SkillshotData skillshotData, int startT, Vector2 startPosition,
            Vector2 endPosition,
            Obj_AI_Base caster)
        {
            DetectionType = detectionType;
            SkillshotData = skillshotData;
            StartTick = startT;
            StartPosition = startPosition;
            EndPosition = endPosition;
            MissilePosition = startPosition;
            Direction = (endPosition - startPosition).Normalized();
            Caster = caster;

            //Create the spatial object for each type of skillshot.
            switch (skillshotData.Type)
            {
                case SkillShotType.SkillshotCircle:
                    Circle = new SkillshotGeometry.Circle(CollisionEnd, skillshotData.Radius);
                    break;
                case SkillShotType.SkillshotLine:
                    Rectangle = new SkillshotGeometry.Rectangle(StartPosition, CollisionEnd, skillshotData.Radius);
                    break;
                case SkillShotType.SkillshotMissileLine:
                    Rectangle = new SkillshotGeometry.Rectangle(StartPosition, CollisionEnd, skillshotData.Radius);
                    break;
                case SkillShotType.SkillshotCone:
                    Sector = new SkillshotGeometry.Sector(startPosition, CollisionEnd - startPosition, skillshotData.Radius*(float) Math.PI/180,
                        skillshotData.Range);
                    break;
                case SkillShotType.SkillshotRing:
                    Ring = new SkillshotGeometry.Ring(CollisionEnd, skillshotData.Radius, skillshotData.RingRadius);
                    break;
            }

            UpdatePolygon(); // Create the polygon
        }

        public bool IsActive
        {
            get
            {
                if (SkillshotData.MissileAccel != 0)
                {
                    return Environment.TickCount <= StartTick + 5000;
                }

                return Environment.TickCount <=
                       StartTick + SkillshotData.Delay + SkillshotData.ExtraDuration +
                       1000*(StartPosition.Distance(EndPosition)/SkillshotData.MissileSpeed);
            }
        }

        public void Game_OnGameUpdate()
        {
            //Even if it doesnt consume a lot of resources with 20 updatest second works k
            if (SkillshotData.CollisionObjects.Count() > 0 && SkillshotData.CollisionObjects != null &&
                Environment.TickCount - _lastCollisionCalc > 50)
            {
                _lastCollisionCalc = Environment.TickCount;
                _collisionEnd = Collision.GetCollisionPoint(this);
            }

            //Update the missile position each time the game updates.
            if (SkillshotData.Type == SkillShotType.SkillshotMissileLine)
            {
                Rectangle = new SkillshotGeometry.Rectangle(GetMissilePosition(0), CollisionEnd, SkillshotData.Radius);
                UpdatePolygon();
            }

            //Spells that update to the Caster position.
            if (SkillshotData.MissileFollowsUnit)
            {
                if (Caster.IsVisible)
                {
                    EndPosition = Caster.ServerPosition.To2D();
                    Direction = (EndPosition - StartPosition).Normalized();
                    UpdatePolygon();
                }
            }
        }

        public void UpdatePolygon()
        {
            switch (SkillshotData.Type)
            {
                case SkillShotType.SkillshotCircle:
                    Polygon = Circle.ToPolygon();
                    break;
                case SkillShotType.SkillshotLine:
                    Polygon = Rectangle.ToPolygon();
                    break;
                case SkillShotType.SkillshotMissileLine:
                    Polygon = Rectangle.ToPolygon();
                    break;
                case SkillShotType.SkillshotCone:
                    Polygon = Sector.ToPolygon();
                    break;
                case SkillShotType.SkillshotRing:
                    Polygon = Ring.ToPolygon();
                    break;
            }
        }

        /// <summary>
        ///     Returns the missile position after time time.
        /// </summary>
        public Vector2 GlobalGetMissilePosition(int time)
        {
            var t = Math.Max(0, Environment.TickCount + time - StartTick - SkillshotData.Delay);
            t = (int) Math.Max(0, Math.Min(EndPosition.Distance(StartPosition), t*SkillshotData.MissileSpeed/1000));
            return StartPosition + Direction*t;
        }

        /// <summary>
        ///     Returns the missile position after time time.
        /// </summary>
        public Vector2 GetMissilePosition(int time)
        {
            var t = Math.Max(0, Environment.TickCount + time - StartTick - SkillshotData.Delay);


            var x = 0;

            //Missile with acceleration = 0.
            if (SkillshotData.MissileAccel == 0)
            {
                x = t*SkillshotData.MissileSpeed/1000;
            }

                //Missile with constant acceleration.
            else
            {
                var t1 = (SkillshotData.MissileAccel > 0
                    ? SkillshotData.MissileMaxSpeed
                    : SkillshotData.MissileMinSpeed - SkillshotData.MissileSpeed)*1000f/SkillshotData.MissileAccel;

                if (t <= t1)
                {
                    x =
                        (int)
                            (t*SkillshotData.MissileSpeed/1000d + 0.5d*SkillshotData.MissileAccel*Math.Pow(t/1000d, 2));
                }
                else
                {
                    x =
                        (int)
                            (t1*SkillshotData.MissileSpeed/1000d +
                             0.5d*SkillshotData.MissileAccel*Math.Pow(t1/1000d, 2) +
                             (t - t1)/1000d*
                             (SkillshotData.MissileAccel < 0
                                 ? SkillshotData.MissileMaxSpeed
                                 : SkillshotData.MissileMinSpeed));
                }
            }

            t = (int) Math.Max(0, Math.Min(CollisionEnd.Distance(StartPosition), x));
            return StartPosition + Direction*t;
        }

        /// <summary>
        ///     Returns if the skillshot will hit the unit if the unit follows the path.
        /// </summary>
        public SafePathResult IsSafePath(List<Vector2> path,
            int timeOffset,
            int speed = -1,
            int delay = 0,
            Obj_AI_Base unit = null)
        {
            var Distance = 0f;
            timeOffset += Game.Ping/2;

            speed = (speed == -1) ? (int) ObjectManager.Player.MoveSpeed : speed;

            if (unit == null)
            {
                unit = ObjectManager.Player;
            }

            var allIntersections = new List<FoundIntersection>();
            for (var i = 0; i <= path.Count - 2; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var segmentIntersections = new List<FoundIntersection>();

                for (var j = 0; j <= Polygon.Points.Count - 1; j++)
                {
                    var sideStart = Polygon.Points[j];
                    var sideEnd = Polygon.Points[j == (Polygon.Points.Count - 1) ? 0 : j + 1];

                    var intersection = from.Intersection(to, sideStart,
                        sideEnd);

                    if (intersection.Intersects)
                    {
                        segmentIntersections.Add(
                            new FoundIntersection(
                                Distance + intersection.Point.Distance(from),
                                (int) ((Distance + intersection.Point.Distance(from))*1000/speed),
                                intersection.Point, from));
                    }
                }

                var sortedList = segmentIntersections.OrderBy(o => o.Distance).ToList();
                allIntersections.AddRange(sortedList);

                Distance += from.Distance(to);
            }

            //Skillshot with missile.
            if (SkillshotData.Type == SkillShotType.SkillshotMissileLine ||
                SkillshotData.Type == SkillShotType.SkillshotMissileCone)
            {
                //Outside the skillshot
                if (IsSafe(ObjectManager.Player.ServerPosition.To2D()))
                {
                    //No intersections -> Safe
                    if (allIntersections.Count == 0)
                    {
                        return new SafePathResult(true, new FoundIntersection());
                    }

                    for (var i = 0; i <= allIntersections.Count - 1; i = i + 2)
                    {
                        var enterIntersection = allIntersections[i];
                        var enterIntersectionProjection = enterIntersection.Point.ProjectOn(StartPosition, EndPosition).SegmentPoint;

                        //Intersection with no exit point.
                        if (i == allIntersections.Count - 1)
                        {
                            var missilePositionOnIntersection =
                                GetMissilePosition(enterIntersection.Time - timeOffset);
                            return
                                new SafePathResult(
                                    (EndPosition.Distance(missilePositionOnIntersection) + 50 <=
                                     EndPosition.Distance(enterIntersectionProjection)) &&
                                    ObjectManager.Player.MoveSpeed < SkillshotData.MissileSpeed, allIntersections[0]);
                        }


                        var exitIntersection = allIntersections[i + 1];
                        var exitIntersectionProjection = exitIntersection.Point.ProjectOn(StartPosition, EndPosition).SegmentPoint;

                        var missilePosOnEnter = GetMissilePosition(enterIntersection.Time - timeOffset);
                        var missilePosOnExit = GetMissilePosition(exitIntersection.Time + timeOffset);

                        //Missile didnt pass.
                        if (missilePosOnEnter.Distance(EndPosition) + 50 > enterIntersectionProjection.Distance(EndPosition))
                        {
                            if (missilePosOnExit.Distance(EndPosition) <= exitIntersectionProjection.Distance(EndPosition))
                            {
                                return new SafePathResult(false, allIntersections[0]);
                            }
                        }
                    }

                    return new SafePathResult(true, allIntersections[0]);
                }
                //Inside the skillshot.
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(false, new FoundIntersection());
                }

                if (allIntersections.Count > 0)
                {
                    //Check only for the exit point
                    var exitIntersection = allIntersections[0];
                    var exitIntersectionProjection = exitIntersection.Point.ProjectOn(StartPosition, EndPosition).SegmentPoint;

                    var missilePosOnExit = GetMissilePosition(exitIntersection.Time + timeOffset);
                    if (missilePosOnExit.Distance(EndPosition) <= exitIntersectionProjection.Distance(EndPosition))
                    {
                        return new SafePathResult(false, allIntersections[0]);
                    }
                }
            }


            if (IsSafe(ObjectManager.Player.ServerPosition.To2D()))
            {
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(true, new FoundIntersection());
                }

                if (SkillshotData.DontCross)
                {
                    return new SafePathResult(false, allIntersections[0]);
                }
            }
            else
            {
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(false, new FoundIntersection());
                }
            }

            var timeToExplode = (SkillshotData.DontAddExtraDuration ? 0 : SkillshotData.ExtraDuration) +
                                SkillshotData.Delay +
                                (int) (1000*StartPosition.Distance(EndPosition)/SkillshotData.MissileSpeed) -
                                (Environment.TickCount - StartTick);


            var myPositionWhenExplodes = path.PosAfter(timeToExplode, speed, delay);

            if (!IsSafe(myPositionWhenExplodes))
            {
                return new SafePathResult(false, allIntersections[0]);
            }

            var myPositionWhenExplodesWithOffset = path.PosAfter(timeToExplode, speed, timeOffset);

            return new SafePathResult(IsSafe(myPositionWhenExplodesWithOffset), allIntersections[0]);
        }

        public bool IsSafe(Vector2 point)
        {
            return Polygon.IsOutside(point);
        }

        public bool IsDanger(Vector2 point)
        {
            return !IsSafe(point);
        }

        //Returns if the skillshot is about to hit the unit in the next time seconds.
        public bool IsAboutToHit(int time, Obj_AI_Base unit)
        {
            if (SkillshotData.Type == SkillShotType.SkillshotMissileLine)
            {
                var missilePos = GetMissilePosition(0);
                var missilePosAfterT = GetMissilePosition(time);

                //TODO: Check for minion collision etc.. in the future.
                var projection = unit.ServerPosition.To2D()
                    .ProjectOn(missilePos, missilePosAfterT);

                if (projection.IsOnSegment &&
                    projection.SegmentPoint.Distance(unit.ServerPosition) < SkillshotData.Radius)
                {
                    return true;
                }

                return false;
            }

            if (!IsSafe(unit.ServerPosition.To2D()))
            {
                var timeToExplode = SkillshotData.ExtraDuration + SkillshotData.Delay +
                                    (int) ((1000*StartPosition.Distance(EndPosition))/SkillshotData.MissileSpeed) -
                                    (Environment.TickCount - StartTick);
                if (timeToExplode <= time)
                {
                    return true;
                }
            }

            return false;
        }
    }
}