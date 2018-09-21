using System;
using Eto.Drawing;
using QwertyCombat.Objects.Weapons;

namespace QwertyCombat.Objects
{
    public enum ShipType
    {
        Scout, Assaulter
    }

    class Ship : SpaceObject
    {
        public readonly Weapon EquippedWeapon;
        public PointF WeaponPoint;

        public Ship(Player playerId, WeaponType wpnType, int maxHealth, int maxActions): base(playerId, maxHealth, ObjectType.Ship, maxActions)
        {
            switch (wpnType)
            {
                case WeaponType.HeavyLaser:
                    this.EquippedWeapon = new HeavyLaser();
                    break;
                case WeaponType.LightIon:
                    this.EquippedWeapon = new LightIon();
                    break;
                case WeaponType.LightLaser:
                    this.EquippedWeapon = new LightLaser();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wpnType), wpnType, null);
            }
        }

        public override string Description => "";

        public int AttackDamage 
        {
            get 
            {
                Random rand = new Random();
                return rand.Next(-this.EquippedWeapon.AttackPower / 10, this.EquippedWeapon.AttackPower / 10) + this.EquippedWeapon.AttackPower;
            }
        }


        public override void Rotate(double angle)
        {
            angle = angle * Math.PI / 180;
            for (int i = 0; i < this.PolygonPoints.Count; i++)
            {
                this.PolygonPoints[i] =
                    new PointF((float) (this.PolygonPoints[i].X * Math.Cos(angle) - this.PolygonPoints[i].Y * Math.Sin(angle)),
                        (float) (this.PolygonPoints[i].X * Math.Sin(angle) + this.PolygonPoints[i].Y * Math.Cos(angle)));
            }
            this.WeaponPoint = new PointF((float) (this.WeaponPoint.X * Math.Cos(angle) - this.WeaponPoint.Y * Math.Sin(angle)),
                (float) (this.WeaponPoint.X * Math.Sin(angle) + this.WeaponPoint.Y * Math.Cos(angle)));
        }

        public void RefillEnergy()
        {
            this.ActionsLeft = this.MaxActions;
        }
    }
}
