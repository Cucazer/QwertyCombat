using System;
using System.Collections.Generic;
using Eto.Drawing;

namespace QwertyCombat
{
    public abstract class DrawableShape
    {
        public Point Origin { get; private set; }
        public Color Color { get; private set; }
        public bool IsTeamColor { get; private set; }

        public DrawableShape(Point origin, Color color, bool isTeamColor = false)
        {
            this.Origin = origin;
            this.Color = color;
            this.IsTeamColor = isTeamColor;
        }

        public abstract void Rotate(double angle);
    }

    public class Ellipsis : DrawableShape
    {
        public Size Size { get; private set; }
        
        public Ellipsis(Point origin, Color color, Size size, bool isTeamColor = false) : base(origin, color, isTeamColor)
        {
            this.Size = size;
        }

        public override void Rotate(double angle)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Polygon : DrawableShape
    {
        public List<PointF> Points { get; private set; }
        
        public Polygon(Point origin, Color color, List<PointF> points, bool isTeamColor = false) : base(origin, color, isTeamColor)
        {
            this.Points = points;
        }


        public override void Rotate(double angle)
        {
            for (int i = 0; i < this.Points.Count; i++)
            {
                this.Points[i] =
                    new PointF((float) (this.Points[i].X * Math.Cos(angle) - this.Points[i].Y * Math.Sin(angle)),
                        (float) (this.Points[i].X * Math.Sin(angle) + this.Points[i].Y * Math.Cos(angle)));
            }
        }
    }
}