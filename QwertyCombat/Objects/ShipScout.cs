using System;
using System.Collections.Generic;
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
            this.PolygonPoints = new List<PointF>
            {
                new PointF(-15, -14),
                new PointF(-15, 14),
                new PointF(17, 0)
            };

            this.WeaponPoint = this.PolygonPoints[2];

            if (this.Owner == Player.SecondPlayer)
            {
                this.Rotate(180);
            }
        }
    }
}
