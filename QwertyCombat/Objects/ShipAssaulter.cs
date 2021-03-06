﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using QwertyCombat.Objects.Weapons;

namespace QwertyCombat.Objects
{
    class ShipAssaulter : Ship
    {
        public override string Name => "Heavy Assaulter ship";

        public ShipAssaulter(Player playerId, WeaponType weaponType) : base(playerId, weaponType, 100, 2)
        {
            var polygonPoints = new List<PointF>
            {
                new PointF(-16, -15),
                new PointF(6, -10),
                new PointF(18, 0),
                new PointF(6, 10),
                new PointF(-16, 15),  
            };

            var windowPoints = new List<PointF>
            {
                new PointF(-5, 0),
                new PointF(-5, -9),
                new PointF(4, -7),
                new PointF(10, -2),
                new PointF(10, 0)
            };

            if (playerId == Player.SecondPlayer)
            {
                // hotfix, assuming Rotate(180)
                windowPoints = windowPoints.Select(p => new PointF(p.X, -p.Y)).ToList();
            }

            var weaponPoints = new List<PointF>
            {
                new PointF(14, 4),
                new PointF(28, 0),
                new PointF(14, -4)
            };

            var nozzlePoints = new List<PointF>
            {
                new PointF(-22, -20),
                new PointF(-16, -20),
                new PointF(-12, -15),
                new PointF(-12, 15),
                new PointF(-16, 20),
                new PointF(-22, 20)
            };

            this.WeaponPoint = weaponPoints[1];
            this.FlameBounds = new List<Polygon>
            {
                new Polygon(Colors.Red,
                    new List<PointF>
                    {
                        nozzlePoints[0],
                        nozzlePoints[0] - new SizeF(10, 0),
                        nozzlePoints[5] - new SizeF(10, 0),
                        nozzlePoints[5]
                    })
            };

            this.ObjectAppearance = new List<DrawableShape>
            {
                new Polygon(polygonPoints),
                new Polygon(Colors.Aqua, windowPoints),
                new Polygon(Colors.LightGrey, weaponPoints),
                new Polygon(Colors.DarkSeaGreen, nozzlePoints)
            };

            if (this.Owner == Player.SecondPlayer)
            {
                this.Rotate(180);
            }
        }
    }
}
