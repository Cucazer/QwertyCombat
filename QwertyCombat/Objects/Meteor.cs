using System;
using System.Collections.Generic;
using System.Linq;
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

            this.ObjectAppearance = new List<DrawableShape>
            {
                new Ellipse(new Point(-meteorRadius, -meteorRadius), Colors.DarkGray, new Size(2 * meteorRadius, 2 * meteorRadius)),
            };

            var darkSpots = new List<Ellipse>();
            Random rand = new Random();
            int minSpotRadius = meteorRadius / 7;
            int maxSpotRadius = meteorRadius / 5;
            for (int i = 0; i < 20; i++)
            {
                var spotRadius = rand.Next(minSpotRadius, maxSpotRadius + 1);

                var r = rand.Next(spotRadius, meteorRadius - spotRadius);
                var phi = 2 * Math.PI * rand.NextDouble();
                int x = (int)(r * Math.Cos(phi)) - spotRadius;
                int y = (int)(r * Math.Sin(phi)) - spotRadius;
                
                var spotCenter = new PointF(x + spotRadius, y + spotRadius);
                if (!darkSpots.Any(existingSpot => spotCenter.Distance(existingSpot.Origin + existingSpot.Size / 2) < spotRadius + existingSpot.Size.Width))
                {
                    darkSpots.Add(new Ellipse(new Point(x, y), Colors.DimGray, new Size(2 * spotRadius, 2 * spotRadius)));
                }
            }
            this.ObjectAppearance.AddRange(darkSpots);


            var flameSweepsOnMeteor = new Dictionary<Color, double> { { Colors.Orange, 2 * Math.PI / 3 }, { Colors.Red, Math.PI / 4 } };
            var flameSpanAngle = Math.PI / 4;

            foreach (var flameSweep in flameSweepsOnMeteor)
            {
                float flameBaseLengthHalf = meteorRadius * (float)Math.Sin(flameSweep.Value / 2);
                float flameHeight = flameBaseLengthHalf / (float)Math.Tan(flameSpanAngle / 2);
                float flameTipOffset = meteorRadius * (float)Math.Cos(flameSweep.Value / 2) + flameHeight;
                // arcSpanAngle = 180 - 2 * (90 - flameSpanAngle/2) = 180 - 180 + 2 * flameSpanAngle/2 = flameSpanAngle
                float arcRadius = flameHeight / (float)Math.Sin(flameSpanAngle);

                var pathComponents = new List<DrawableShape>();
                pathComponents.Add(new Arc(new Point(- meteorRadius, - meteorRadius), new Color(),
                    2 * meteorRadius, 2 * meteorRadius, 180 - (float)(flameSweep.Value.ToDegrees() / 2), (float)flameSweep.Value.ToDegrees()));
                pathComponents.Add(new Arc(new PointF(-flameTipOffset - arcRadius, -2 * arcRadius), new Color(), 
                    2 * arcRadius, 2 * arcRadius, 90 - (float)flameSpanAngle.ToDegrees(), (float)flameSpanAngle.ToDegrees()));
                pathComponents.Add(new Arc(new PointF(-flameTipOffset - arcRadius, 0), new Color(),
                    2 * arcRadius, 2 * arcRadius, -90, (float)flameSpanAngle.ToDegrees()));
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
