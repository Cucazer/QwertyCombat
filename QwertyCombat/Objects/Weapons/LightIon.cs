using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace qwerty.Objects.Weapons
{
    class LightIon : Weapon
    {
        public override Color AttackColorPrimary => Color.CadetBlue;
        public override Color AttackColorSecondary => Color.CornflowerBlue;

        public LightIon() : base(4, 18, 1)
        {

        }

        public override string Description => "";

        public override Stream AttackSound => Properties.Resources.laser2;

        public override List<Bitmap> GetAttackSprites(PointF sourcePoint, PointF targetPoint)
        {
            List<Bitmap> sprites = new List<Bitmap>();
            SolidBrush brush1 = new SolidBrush(Color.CadetBlue);
            SolidBrush brush = new SolidBrush(Color.CornflowerBlue);

            // TODO: review to match points
            var dx = (targetPoint.X - sourcePoint.X) / 10;
            var dy = (targetPoint.Y - sourcePoint.Y) / 10;

            for (int i = 0; i < 10; i++)
            {
                var sprite = new Bitmap((int) Math.Max(sourcePoint.X, targetPoint.X) + 10, (int) Math.Max(sourcePoint.Y, targetPoint.Y) + 10);
                var g = Graphics.FromImage(sprite);

                g.FillEllipse(brush, sourcePoint.X - 5 + dx * i, sourcePoint.Y - 5 + dy * i, 10, 10);
                g.FillEllipse(brush1, sourcePoint.X - 3 + dx * i, sourcePoint.Y - 3 + dy * i, 6, 6);

                sprites.Add(sprite);
            }
            
            return sprites;
        }
    }
}
