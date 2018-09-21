using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace qwerty.Objects.Weapons
{
    class HeavyLaser : Weapon
    {
        public HeavyLaser() : base(5, 50, 1)
        {

        }

        public override string Description=> "";

        public override Color AttackColorPrimary => Color.Orange;
        public override Color AttackColorSecondary => Color.Orange;

        public override Stream AttackSound => Properties.Resources.laser1;

        public override List<Bitmap> GetAttackSprites(PointF sourcePoint, PointF targetPoint)
        {
            List<Bitmap> sprites = new List<Bitmap>();
            
            Pen laserPen1 = new Pen(Color.Orange, 3);

            for (int i = -2; i < 2; i++)
            {
                var sprite = new Bitmap((int) Math.Max(sourcePoint.X, targetPoint.X) + 2, (int) Math.Max(sourcePoint.Y, targetPoint.Y));
                var g = Graphics.FromImage(sprite);
                g.DrawLine(laserPen1, sourcePoint, PointF.Add(targetPoint, new Size(i,0)));

                sprites.Add(sprite);
            }
            
            return sprites;
        }
    }
}
