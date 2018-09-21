namespace qwerty
{
    public static class Extensions
    {
        public static System.Drawing.Point ConvertToDrawingPoint(this Barbar.HexGrid.Point hexPoint)
        {
            return new System.Drawing.Point((int)hexPoint.X, (int)hexPoint.Y);
        }
        
        public static System.Drawing.PointF ConvertToDrawingPointF(this Barbar.HexGrid.Point hexPoint)
        {
            return new System.Drawing.PointF((float)hexPoint.X, (float)hexPoint.Y);
        }
        
        public static Barbar.HexGrid.Point ConvertToHexPoint(this System.Drawing.Point drawingPoint)
        {
            return new Barbar.HexGrid.Point(drawingPoint.X, drawingPoint.Y);
        }
        
        public static Barbar.HexGrid.Point ConvertToHexPoint(this System.Drawing.PointF drawingPointF)
        {
            return new Barbar.HexGrid.Point(drawingPointF.X, drawingPointF.Y);
        }
    }
}