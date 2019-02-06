using System;

namespace QwertyCombat
{
    public static class Extensions
    {
        public static Eto.Drawing.Point ConvertToDrawingPoint(this Barbar.HexGrid.Point hexPoint)
        {
            return new Eto.Drawing.Point((int)hexPoint.X, (int)hexPoint.Y);
        }
        
        public static Eto.Drawing.PointF ConvertToDrawingPointF(this Barbar.HexGrid.Point hexPoint)
        {
            return new Eto.Drawing.PointF((float)hexPoint.X, (float)hexPoint.Y);
        }
        
        public static Barbar.HexGrid.Point ConvertToHexPoint(this Eto.Drawing.Point drawingPoint)
        {
            return new Barbar.HexGrid.Point(drawingPoint.X, drawingPoint.Y);
        }
        
        public static Barbar.HexGrid.Point ConvertToHexPoint(this Eto.Drawing.PointF drawingPointF)
        {
            return new Barbar.HexGrid.Point(drawingPointF.X, drawingPointF.Y);
        }

        public static double ToRadians(this double angleInDegrees)
        {
            return angleInDegrees * Math.PI / 180;
        }

        public static double ToDegrees(this double angleInRadians)
        {
            return angleInRadians * 180 / Math.PI;
        }
    }
}