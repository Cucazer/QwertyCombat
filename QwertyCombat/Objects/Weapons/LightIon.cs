﻿using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;

namespace QwertyCombat.Objects.Weapons
{
    class LightIon : Weapon
    {
        public override Color AttackColorPrimary => Colors.CadetBlue;
        public override Color AttackColorSecondary => Colors.CornflowerBlue;

        public LightIon() : base(4, 18, 1)
        {

        }

        public override string Description => "";

        public override Stream AttackSound => Properties.Resources.laser2;

        public override List<Bitmap> GetAttackSprites(PointF sourcePoint, PointF targetPoint)
        {
            List<Bitmap> sprites = new List<Bitmap>();
            SolidBrush brush1 = new SolidBrush(Colors.CadetBlue);
            SolidBrush brush = new SolidBrush(Colors.CornflowerBlue);

            // TODO: review to match points
            var dx = (targetPoint.X - sourcePoint.X) / 10;
            var dy = (targetPoint.Y - sourcePoint.Y) / 10;

            for (int i = 0; i < 10; i++)
            {
                var sprite = new Bitmap((int) Math.Max(sourcePoint.X, targetPoint.X) + 10, (int) Math.Max(sourcePoint.Y, targetPoint.Y) + 10, PixelFormat.Format32bppRgba);
                var g = new Graphics(sprite);

                g.FillEllipse(brush, sourcePoint.X - 5 + dx * i, sourcePoint.Y - 5 + dy * i, 10, 10);
                g.FillEllipse(brush1, sourcePoint.X - 3 + dx * i, sourcePoint.Y - 3 + dy * i, 6, 6);

                sprites.Add(sprite);
            }
            
            return sprites;
        }
    }
}
