using System;
using System.Collections.Generic;
using Eto.Drawing;
using QwertyCombat.Objects.Weapons;

namespace QwertyCombat.Objects
{
    public enum ShipType
    {
        Scout, Assaulter
    }

    public abstract class Ship : SpaceObject
    {
        public readonly Weapon EquippedWeapon;
        public PointF WeaponPoint;
        public List<Polygon> FlameBounds;

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

        public override Dictionary<string, string> Properties => new Dictionary<string, string>
        {
            {"Name", this.Name},
            {"HP", $"{this.CurrentHealth}/{this.MaxHealth}"},
            {"Actions left", $"{this.ActionsLeft}/{this.MaxActions}"},
            {"Attack damage", $"{this.EquippedWeapon.AttackPower}"},
            {"Attack range", $"{this.EquippedWeapon.AttackRange}"}
        };

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
            foreach (var shape in this.ObjectAppearance)
            {
                shape.Rotate((float)angle);
            }
            this.WeaponPoint.Rotate((float)angle);
            foreach (var flameBounds in this.FlameBounds)
            {
                flameBounds.Rotate((float)angle);  
            }
        }

        public void RefillEnergy()
        {
            this.ActionsLeft = this.MaxActions;
        }
    }
}
