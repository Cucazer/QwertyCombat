using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;

namespace QwertyCombat.Objects.Weapons
{
    class HeavyLaser : Weapon
    {
        public HeavyLaser() : base(5, 50, 1)
        {

        }

        public override string Description=> "";

        public override Color AttackColorPrimary => Colors.Orange;
        public override Color AttackColorSecondary => Colors.Orange;

        public override Stream AttackSound => Properties.Resources.laser1;

        public override List<Bitmap> GetAttackSprites(PointF sourcePoint, PointF targetPoint)
        {
            List<Bitmap> sprites = new List<Bitmap>();
            
            Pen laserPen1 = new Pen(Colors.Orange, 3);

            for (int i = -2; i < 2; i++)
            {
                var sprite = new Bitmap((int) Math.Max(sourcePoint.X, targetPoint.X) + 2, (int) Math.Max(sourcePoint.Y, targetPoint.Y), PixelFormat.Format32bppRgba);
                using (var g = new Graphics(sprite))
                {
                    g.DrawLine(laserPen1, sourcePoint, targetPoint + new Size(i, 0));
                }

                sprites.Add(sprite);
            }
            
            return sprites;
        }
    }
}
