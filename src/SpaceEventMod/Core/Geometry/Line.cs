using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Geometry;

/// <summary>
/// Random line segment.
/// </summary>
/// <param name="point1">First point of the line.</param>
/// <param name="point2">Second point of the line.</param>
public struct Line(Vector2 point1, Vector2 point2)
{
    public Vector2 point1 = point1, point2 = point2;

    public Vector2[] GetPoints(int quantity)
    {
        var points = new Vector2[quantity];
        float yDifference = point2.Y - point1.Y, xDifference = point2.X - point1.X;
        float slope = yDifference / xDifference;
        float x, y;

        --quantity;

        for (float i = 0; i < quantity; i++)
        {
            y = slope == 0 ? 0 : yDifference * (i / quantity);
            x = slope == 0 ? xDifference * (i / quantity) : y / slope;
            points[(int)i] = new Vector2(MathF.Round(x) + point1.X, MathF.Round(y) + point1.Y);
        }

        points[quantity] = point2;
        return points;
    }

    public bool Intersects(Rectangle rectangle)
    {
        var left = this.Intersects(new Line(rectangle.TopLeft(), rectangle.BottomLeft()));
        var right = this.Intersects(new Line(rectangle.TopRight(), rectangle.BottomRight()));
        var top = this.Intersects(new Line(rectangle.TopLeft(), rectangle.TopRight()));
        var bottom = this.Intersects(new Line(rectangle.BottomLeft(), rectangle.BottomRight()));

        return left || right || top || bottom;
    }

    public bool Intersects(Line line)
    {
        var uA = ((line.point2.X - line.point1.X) * (this.point1.Y - line.point1.Y) - (line.point2.Y - line.point1.Y) * (this.point1.X - line.point1.X)) / ((line.point2.Y - line.point1.Y) * (this.point2.X - this.point1.X) - (line.point2.X - line.point1.X) * (this.point2.Y - this.point1.Y));

        var uB = ((this.point2.X - this.point1.X) * (this.point1.Y - line.point1.Y) - (this.point2.Y - this.point1.Y) * (this.point1.X - line.point1.X)) / ((line.point2.Y - line.point1.Y) * (this.point2.X - this.point1.X) - (line.point2.X - line.point1.X) * (this.point2.Y - this.point1.Y));

        return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;
    }

    public Vector2 IntersectionPoint(Line line)
    {
        var uA = ((line.point2.X - line.point1.X) * (this.point1.Y - line.point1.Y) - (line.point2.Y - line.point1.Y) * (this.point1.X - line.point1.X)) / ((line.point2.Y - line.point1.Y) * (this.point2.X - this.point1.X) - (line.point2.X - line.point1.X) * (this.point2.Y - this.point1.Y));

        var uB = ((this.point2.X - this.point1.X) * (this.point1.Y - line.point1.Y) - (this.point2.Y - this.point1.Y) * (this.point1.X - line.point1.X)) / ((line.point2.Y - line.point1.Y) * (this.point2.X - this.point1.X) - (line.point2.X - line.point1.X) * (this.point2.Y - this.point1.Y));

        float intersectionX = this.point1.X + (uA * (this.point2.X - this.point1.X));
        float intersectionY = this.point1.Y + (uA * (this.point2.Y - this.point1.Y));

        return new Vector2(intersectionX, intersectionY);
    }
}
