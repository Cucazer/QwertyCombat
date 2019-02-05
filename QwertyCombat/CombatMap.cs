using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using QwertyCombat.Objects;
using Hex = Barbar.HexGrid;

namespace QwertyCombat
{
    /// <summary>
    /// Hexagon neighbor direction names as they are listed in "Directions" list of the cube coordinates in HexGrid library
    /// </summary>
    public enum HexagonNeighborDirection
    {
        SouthEast = 0,
        NorthEast,
        North,
        NorthWest,
        SouthWest,
        South
    }
    
    class CombatMap
    {
        public readonly int FieldWidth;
        public readonly int FieldHeight;
        public const float HexagonSideLength = 40;
        private const int FieldBorderPixels = 10;

        public Hex.HexLayout<Hex.Point, Hex.PointPolicy> HexGrid = Hex.HexLayoutFactory.CreateFlatHexLayout(
            new Hex.Point(HexagonSideLength, HexagonSideLength), new Hex.Point((int)(HexagonSideLength + FieldBorderPixels),  (int)(Math.Sin(Math.PI / 3) * HexagonSideLength + FieldBorderPixels)),
            Hex.Offset.Odd);

        public CombatMap(int w, int h) 
        {
            this.FieldWidth = w;
            this.FieldHeight = h;
        }

        public int BitmapWidth
        {
            get
            {
                var hexagonOffset = this.HexGrid.HexToPixel(this.HexGrid.ToCubeCoordinates(new Hex.OffsetCoordinates(this.FieldWidth - 1, this.FieldHeight - 1)));
                var cornerOffset = this.HexGrid.HexCornerOffset(0);
                return (int)(hexagonOffset.X + cornerOffset.X + FieldBorderPixels);
            }
        }

        public int BitmapHeight
        {
            get
            {
                var hexagonOffset = this.HexGrid.HexToPixel(this.HexGrid.ToCubeCoordinates(new Hex.OffsetCoordinates(this.FieldWidth - 1, this.FieldHeight - 1)));
                var cornerOffset = this.HexGrid.HexCornerOffset(5);
                return (int)(hexagonOffset.Y + cornerOffset.Y + FieldBorderPixels);
            }
        }

        public Point HexToPixel(int x, int y)
        {
            var cubeCoordinates = this.HexGrid.ToCubeCoordinates(new Hex.OffsetCoordinates(x,y));
            return this.HexGrid.HexToPixel(cubeCoordinates).ConvertToDrawingPoint();
        }
        
        public Point GetHexagonCornerOffset(int cornerIndex)
        {
            return this.HexGrid.HexCornerOffset(cornerIndex).ConvertToDrawingPoint();
        }

        public bool AreNeighbors(Hex.OffsetCoordinates firstHexagon, Hex.OffsetCoordinates secondHexagon)
        {
            return this.AreNeighbors(this.HexGrid.ToCubeCoordinates(firstHexagon), this.HexGrid.ToCubeCoordinates(secondHexagon));
        }
        
        public bool AreNeighbors(Hex.CubeCoordinates firstHexagon, Hex.CubeCoordinates secondHexagon)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Hex.CubeCoordinates.Neighbor(firstHexagon, i).Equals(secondHexagon))
                {
                    return true;
                }
            }
            return false;
        }

        public List<Hex.OffsetCoordinates> GetAllNeighbors(Hex.OffsetCoordinates hexagon)
        {
            var neighbors = new List<Hex.OffsetCoordinates>();

            for (int i = 0; i < 6; i++)
            {
                var potentialNeighbor = this.HexGrid.ToOffsetCoordinates(Hex.CubeCoordinates.Neighbor(this.HexGrid.ToCubeCoordinates(hexagon), i));
                if (potentialNeighbor.Column >= 0 && potentialNeighbor.Column < this.FieldWidth &&
                    potentialNeighbor.Row >= 0 && potentialNeighbor.Row < this.FieldHeight)
                {
                    neighbors.Add(potentialNeighbor);
                }
            }

            return neighbors;
        }

        public PointF[] GetHexagonCorners(int x, int y)
        {
            return this.GetHexagonCorners(new Hex.OffsetCoordinates(x, y));
        }

        public PointF[] GetHexagonCorners(Hex.OffsetCoordinates offsetCoordinates)
        {
            var coordinates = this.HexGrid.ToCubeCoordinates(offsetCoordinates);
            return this.HexGrid.PolygonCorners(coordinates).Select(c => c.ConvertToDrawingPointF()).ToArray();
        }

        public List<PointF[]> AllHexagonCorners
        {
            get
            {
                var cornerPointList = new List<PointF[]>();
                for (int x = 0; x < this.FieldWidth; x++)
                {
                    for (int y = 0; y < this.FieldHeight; y++)
                    {
                        cornerPointList.Add(this.GetHexagonCorners(x, y));
                    }
                }
                return cornerPointList;
            }
        }

        public double GetAngle(Hex.CubeCoordinates sourceCoordinates, Hex.CubeCoordinates targetCoordinates)
        {
            double dx = this.HexGrid.HexToPixel(targetCoordinates).X - this.HexGrid.HexToPixel(sourceCoordinates).X;
            double dy = this.HexGrid.HexToPixel(targetCoordinates).Y - this.HexGrid.HexToPixel(sourceCoordinates).Y;
            return Math.Atan2(dy, dx) * 180 / Math.PI;
        }

        public double GetAngle(Hex.OffsetCoordinates sourceOffsetCoordinates, Hex.OffsetCoordinates targetOffsetCoordinates)
        {
            return this.GetAngle(this.HexGrid.ToCubeCoordinates(sourceOffsetCoordinates), this.HexGrid.ToCubeCoordinates(targetOffsetCoordinates));
        }

        public Hex.OffsetCoordinates PixelToOffsetCoordinates(Point pixelCoordinates)
        {
            var cubeCoordinates = this.HexGrid.PixelToHex(pixelCoordinates.ConvertToHexPoint()).Round();
            var offsetCoordinates = this.HexGrid.ToOffsetCoordinates(cubeCoordinates);
            // column/row indices are 0-based! => index == width/height is already outside field
            if (offsetCoordinates.Column < 0 || offsetCoordinates.Column >= this.FieldWidth ||
                offsetCoordinates.Row < 0 || offsetCoordinates.Row >= this.FieldHeight)
            {
                throw new ArgumentOutOfRangeException($"Pixel ({pixelCoordinates.X},{pixelCoordinates.Y}) is outside game field.");
            }
            return offsetCoordinates;
        }

        public int GetDistance(Hex.CubeCoordinates firstHexagon, Hex.CubeCoordinates secondHexagon)
        {
            return Hex.CubeCoordinates.Distance(firstHexagon,secondHexagon);
        }
        
        public int GetDistance(Hex.OffsetCoordinates firstHexagon, Hex.OffsetCoordinates secondHexagon)
        {
            return Hex.CubeCoordinates.Distance(this.HexGrid.ToCubeCoordinates(firstHexagon), this.HexGrid.ToCubeCoordinates(secondHexagon));
        }

        public List<Hex.OffsetCoordinates> GetAllHexagonsInRange(Hex.OffsetCoordinates centerHexagon, int range)
        {
            var allHexagons = new List<Hex.OffsetCoordinates>();
            //  TODO: implement spiral ring algorithm from redblobgames
            for (int x = 0; x< this.FieldWidth; x++)
            {
                for (int y = 0; y< this.FieldHeight; y++)
                {
                    var coordinates = new Hex.OffsetCoordinates(x, y);
                    var distance = this.GetDistance(centerHexagon, coordinates);
                    if (distance <= range && distance > 0)
                    {
                        allHexagons.Add(coordinates);
                    }
                }
            }
            return allHexagons;
        }

        public List<PointF[]> GetAllHexagonCornersInRange(Hex.OffsetCoordinates centerHexagon, int range)
        {
            var allHexagonCorners = new List<PointF[]>();
            foreach (var hexagonCoordinates in this.GetAllHexagonsInRange(centerHexagon, range))
            {
                allHexagonCorners.Add(this.GetHexagonCorners(hexagonCoordinates));
            }
            return allHexagonCorners;
        }

        public Point HexToPixel(Hex.OffsetCoordinates objectCoordinates)
        {
            return this.HexGrid.HexToPixel(this.HexGrid.ToCubeCoordinates(objectCoordinates)).ConvertToDrawingPoint();
        }

        public Hex.OffsetCoordinates GetNeighborCoordinates(Hex.OffsetCoordinates hexagonOffsetCoordinates, int neighborDirection)
        {
            return this.HexGrid.ToOffsetCoordinates(
                Hex.CubeCoordinates.Neighbor(this.HexGrid.ToCubeCoordinates(hexagonOffsetCoordinates), neighborDirection));
        }
    } 
}
