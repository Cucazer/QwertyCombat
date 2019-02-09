using System;
using System.Collections.Generic;
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

            this.WeaponPoint = polygonPoints[2];
            
            this.ObjectAppearance = new List<DrawableShape> { new Polygon(new Point(0,0), new Color(), polygonPoints, true)};

            if (this.Owner == Player.SecondPlayer)
            {
                this.Rotate(180);
            }
        }
    }
}
