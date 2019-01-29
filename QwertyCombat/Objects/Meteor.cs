using System;
using System.Collections.Generic;
using Eto.Drawing;
using Hex = Barbar.HexGrid;

namespace QwertyCombat.Objects
{
    // TODO: rename to meteoroid? meteor is already in atmosphere!
    class Meteor : SpaceObject
    {
        private const string ObjectDescription = "Moving meteor";
        public readonly int CollisionDamage;
        public readonly HexagonNeighborDirection MovementDirection;

        public Meteor(Hex.OffsetCoordinates meteorCoordinates, int health, int damage, HexagonNeighborDirection movementDirection): base(Player.None, health, ObjectType.Meteor, 1)
        {
            this.MovementDirection = movementDirection;
            this.ObjectCoordinates = meteorCoordinates;
            this.CollisionDamage = damage;





            int meteorRadius = 15;
            int spotRadius = (int)(meteorRadius / 3.0);

            this.ObjectAppearance = new List<DrawableShape>
            {
                new Ellipse(new Point(-meteorRadius, -meteorRadius), Colors.DarkGray, new Size(2 * meteorRadius, 2 * meteorRadius)),
                //TODO: add more spots, random generated
                new Ellipse(new Point(-spotRadius, -spotRadius), Colors.DimGray, new Size(spotRadius, spotRadius))
            };

            var flameSweepsOnMeteor = new Dictionary<Color, double> { { Colors.Orange, 2 * Math.PI / 3 }, { Colors.Red, Math.PI / 4 } };
            float flameSpanAngle = (float)Math.PI / 4;
            float flameSpanAngleDegrees = (float)(flameSpanAngle * 180 / Math.PI);

            foreach (var flameSweep in flameSweepsOnMeteor)
            {
                float flameBaseLengthHalf = meteorRadius * (float)Math.Sin(flameSweep.Value / 2);
                float flameHeight = flameBaseLengthHalf / (float)Math.Tan(flameSpanAngle / 2);
                // arcSpanAngle = 180 - 2 * (90 - flameSpanAngle/2) = 180 - 180 + 2 * flameSpanAngle/2 = flameSpanAngle
                float arcRadius = flameHeight / (float)Math.Sin(flameSpanAngle);

                var pathComponents = new List<DrawableShape>();
                pathComponents.Add(new Arc(new Point(- meteorRadius, - meteorRadius), new Color(),
                    2 * meteorRadius, 2 * meteorRadius, 90 - (float)(flameSweep.Value / 2 * 180 / Math.PI), (float)(flameSweep.Value * 180 / Math.PI)));
                //TODO: fix float points
                pathComponents.Add(new Arc(new Point(-2 * arcRadius, meteorRadius * (float)Math.Cos(flameSweep.Value / 2) + flameHeight - arcRadius), new Color(), 
                    2 * arcRadius, 2 * arcRadius, -flameSpanAngleDegrees, flameSpanAngleDegrees));
                pathComponents.Add(new Arc(new Point(0, meteorRadius * (float)Math.Cos(flameSweep.Value / 2) + flameHeight - arcRadius), new Color(),
                    2 * arcRadius, 2 * arcRadius, 180, flameSpanAngleDegrees));
                this.ObjectAppearance.Add(new Path(new Point(0, 0), flameSweep.Key, pathComponents));
            }
        }

        public override string Description => ObjectDescription + "\nCollision damage: " + this.CollisionDamage
                                              + "\nHP - " + this.CurrentHealth
                                              + "\nMovement direction: \n" + this.MovementDirection;

        public override void Rotate(double angle)
        {
            throw new System.NotImplementedException();
        }
    }
}
