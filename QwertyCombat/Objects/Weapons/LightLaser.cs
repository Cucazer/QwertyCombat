using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace qwerty.Objects.Weapons
{
    class LightLaser : Weapon
    {
        public override Color AttackColorPrimary => Color.GreenYellow;
        public override Color AttackColorSecondary => Color.GreenYellow;

        public LightLaser() : base(3, 25, 1)
        {

        }
        public override string Description => "";

        public override Stream AttackSound => Properties.Resources.laser3;

        public override List<Bitmap> GetAttackSprites(PointF sourcePoint, PointF targetPoint)
        {
            List<Bitmap> sprites = new List<Bitmap>();
            Pen laserPen1 = new Pen(Color.GreenYellow, 2);
            for (int i = 0; i < 5; i++)
            {
                var sprite = new Bitmap((int) Math.Max(sourcePoint.X, targetPoint.X) + 5, (int) Math.Max(sourcePoint.Y, targetPoint.Y) + 2);
                var g = Graphics.FromImage(sprite);

                g.DrawLine(laserPen1, sourcePoint, PointF.Add(targetPoint, new Size(i,0)));

                sprites.Add(sprite);
            }
            return sprites;
        }
    }
}
