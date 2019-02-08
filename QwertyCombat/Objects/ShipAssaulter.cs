using System;
using System.Collections.Generic;
using Eto.Drawing;
using QwertyCombat.Objects.Weapons;

namespace QwertyCombat.Objects
{
    class ShipAssaulter : Ship
    {
        private const string StaticDescription = "Heavy Assaulter ship";

        public override Dictionary<string, string> Properties => new Dictionary<string, string>
        {
            {"Name", StaticDescription},
            {"HP", $"{this.CurrentHealth}/{this.MaxHealth}"},
            {"Actions in this turn", $"{this.ActionsLeft}/{this.MaxActions}"},
            {"Attack damage", $"{this.EquippedWeapon.AttackPower}"},
            {"Attack range", $"{this.EquippedWeapon.AttackRange}"}
        };

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
