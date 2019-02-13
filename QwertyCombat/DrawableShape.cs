using System;
using System.Collections.Generic;
using Eto.Drawing;

namespace QwertyCombat
{
    public abstract class DrawableShape
    {
        public PointF Origin { get; protected set; }
        public Color Color { get; private set; }
        public bool IsTeamColor { get; private set; }

        public DrawableShape(PointF origin, Color color, bool isTeamColor = false)
        {
            this.Origin = origin;
            this.Color = color;
            this.IsTeamColor = isTeamColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        public abstract void Rotate(float angle);
    }

    public class Ellipse : DrawableShape
    {
        public SizeF Size { get; private set; }
        
        public Ellipse(PointF origin, Color color, SizeF size, bool isTeamColor = false) : base(origin, color, isTeamColor)
        {
            this.Size = size;
        }

        public override void Rotate(float angle)
        {
            var arcCenter = this.Origin + this.Size / 2;
            arcCenter.Rotate(angle);
            this.Origin = arcCenter - this.Size / 2;
        }
    }

    public class Polygon : DrawableShape
    {
        public List<PointF> Points { get; private set; }
        
        //TODO: apply same or similar constructor refactoring to all other drawable shapes
        public Polygon(Point origin, Color color, List<PointF> points) : base(origin, color)
        {
            this.Points = points;
        }

        public Polygon(Point origin, List<PointF> points) : base(origin, new Color(), true)
        {
            this.Points = points;
        }


        public override void Rotate(float angle)
        {
            for (int i = 0; i < this.Points.Count; i++)
            {
                var rotatedPoint = new PointF((SizeF)this.Points[i]);
                rotatedPoint.Rotate(angle);
                this.Points[i] = rotatedPoint;
            }
        }
    }

    public class Arc : Ellipse
    {
        public float StartAngle { get; private set; }
        public float SweepAngle { get; private set; }

        public Arc(PointF origin, Color color, SizeF size, float startAngle, float sweepAngle, bool isTeamColor = false) : base(origin, color, size, isTeamColor)
        {
            this.StartAngle = startAngle;
            this.SweepAngle = sweepAngle;
        }

        public override void Rotate(float angle)
        {
            base.Rotate(angle);
            this.StartAngle += angle;
        }
    }

    public class Path : DrawableShape
    {
        public List<DrawableShape> Components { get; private set; }

        public Path(Point origin, Color color, List<DrawableShape> components, bool isTeamColor = false) : base(origin, color, isTeamColor)
        {
            this.Components = components;
        }


        public override void Rotate(float angle)
        {
            foreach (var component in this.Components)
            {
                component.Rotate(angle);
            }
        }
    }
}