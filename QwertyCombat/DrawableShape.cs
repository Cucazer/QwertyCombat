using System;
using System.Collections.Generic;
using Eto.Drawing;

namespace QwertyCombat
{
    public abstract class DrawableShape
    {
        public PointF Origin { get; private set; }
        public Color Color { get; private set; }
        public bool IsTeamColor { get; private set; }

        public DrawableShape(PointF origin, Color color, bool isTeamColor = false)
        {
            this.Origin = origin;
            this.Color = color;
            this.IsTeamColor = isTeamColor;
        }

        public abstract void Rotate(double angle);
    }

    public class Ellipse : DrawableShape
    {
        public Size Size { get; private set; }
        
        public Ellipse(PointF origin, Color color, Size size, bool isTeamColor = false) : base(origin, color, isTeamColor)
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
                var rotatedPoint = new PointF((SizeF)this.Points[i]);
                rotatedPoint.Rotate((float)angle);
                this.Points[i] = rotatedPoint;
            }
        }
    }

    public class Arc : DrawableShape
    {
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float StartAngle { get; private set; }
        public float SweepAngle { get; private set; }

        public Arc(PointF origin, Color color, float width, float height, float startAngle, float sweepAngle, bool isTeamColor = false) : base(origin, color, isTeamColor)
        {
            this.Width = width;
            this.Height = height;
            this.StartAngle = startAngle;
            this.SweepAngle = sweepAngle;
        }

        public override void Rotate(double angle)
        {
            throw new NotImplementedException();
        }
    }

    public class Path : DrawableShape
    {
        public List<DrawableShape> Components { get; private set; }

        public Path(Point origin, Color color, List<DrawableShape> components, bool isTeamColor = false) : base(origin, color, isTeamColor)
        {
            this.Components = components;
        }


        public override void Rotate(double angle)
        {
            foreach (var component in this.Components)
            {
                component.Rotate(angle);
            }
        }
    }
}