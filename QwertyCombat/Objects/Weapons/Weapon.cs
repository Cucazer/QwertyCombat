using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using Eto.Drawing;

namespace QwertyCombat.Objects.Weapons
{
    public enum WeaponType
    {
        LightLaser, HeavyLaser, LightIon
    }

    public abstract class Weapon
    {
        public abstract string Description { get; }
        public abstract List<Bitmap> GetAttackSprites(PointF sourcePoint, PointF targetPoint);

        public abstract Color AttackColorPrimary { get; }
        public abstract Color AttackColorSecondary { get; }

        public readonly int AttackRange;
        public readonly int AttackPower;
        public readonly int EnergyСonsumption;

        protected Weapon(int attackRange, int attackPower, int energyConsumption)
        {
            this.AttackRange = attackRange;
            this.AttackPower = attackPower;
            this.EnergyСonsumption = energyConsumption;
        }

        public abstract Stream AttackSound { get; }
    }
    
}
