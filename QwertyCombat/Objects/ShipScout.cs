﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using QwertyCombat.Objects.Weapons;

namespace QwertyCombat.Objects
{
    class ShipScout : Ship
    {
        private const string ObjectDescription = "Light Scout ship";

        public override string Description =>
            $"{ObjectDescription}{Environment.NewLine}" +
            $"HP - {this.CurrentHealth}/{this.MaxHealth}{Environment.NewLine}" +
            $"Actions in this turn - {this.ActionsLeft}/{this.MaxActions}{Environment.NewLine}" +
            $"Attack damage - {this.EquippedWeapon.AttackPower}{Environment.NewLine}" +
            $"Attack range - {this.EquippedWeapon.AttackRange}";

        public ShipScout(Player playerId, WeaponType weaponType) : base(playerId, weaponType, 50, 3)
        {
            var polygonPoints = new List<PointF>
            {
                new PointF(-15, -14),
                new PointF(-15, 14),
                new PointF(17, 0)
            };

            var windowPoints = new List<PointF>
            {
                new PointF(-7, 0),
                new PointF(-7, 6),
                new PointF(9, 0)
            };

            var weaponPoints = new List<PointF>
            {
                new PointF(13, 1),
                new PointF(23, 1),
                new PointF(23, -1),
                new PointF(13, -1)
            };

            var nozzlePoints = new List<PointF>
            {
                new PointF(-20, 6),
                new PointF(-12, 6),
                new PointF(-7, 11),
                new PointF(-7, 18),
                new PointF(-12, 22),
                new PointF(-20, 22)
            };

            this.WeaponPoint = new PointF(23, 0);

            this.ObjectAppearance = new List<DrawableShape> { new Polygon(new Point(0,0), new Color() , polygonPoints, true),
                                                                new Polygon(new Point(0,0), Colors.Aqua, windowPoints),
                                                                new Polygon(new Point(0,0), Colors.LightGrey, weaponPoints),
                                                                new Polygon(new Point(0,0), Colors.DarkSeaGreen, nozzlePoints),
                                                                new Polygon(new Point(0,0), Colors.DarkSeaGreen, nozzlePoints.Select(p => new PointF(p.X, -p.Y)).ToList()) };

            if (this.Owner == Player.SecondPlayer)
            {
                this.Rotate(180);
            }
        }
    }
}
