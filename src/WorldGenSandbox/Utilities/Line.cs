using Microsoft.Xna.Framework;
using System;
using WorldGenSandbox.SDFs;

namespace WorldGenSandbox.Utilities;

/// <summary>
/// Random line segment.
/// </summary>
/// <param name="point1">First point of the line.</param>
/// <param name="point2">Second point of the line.</param>
public struct Line(Vector2 point1, Vector2 point2)
{
    public Vector2 Point1 { get; set; } = point1;
    public Vector2 Point2 { get; set; } = point2;

    public ReadOnlySpan<Vector2> GetPoints(int quantity)
    {
        var scale = 1f / quantity;
        var points = new Vector2[quantity];

        for (var i = 0; i < quantity; i++)
            points[i] = Vector2.Lerp(Point1, Point2, scale * i);

        return points;
    }

    public bool Intersects(Line line)
    {
        var uA = ((line.Point2.X - line.Point1.X) * (this.Point1.Y - line.Point1.Y) - (line.Point2.Y - line.Point1.Y) * (this.Point1.X - line.Point1.X)) / ((line.Point2.Y - line.Point1.Y) * (this.Point2.X - this.Point1.X) - (line.Point2.X - line.Point1.X) * (this.Point2.Y - this.Point1.Y));

        var uB = ((this.Point2.X - this.Point1.X) * (this.Point1.Y - line.Point1.Y) - (this.Point2.Y - this.Point1.Y) * (this.Point1.X - line.Point1.X)) / ((line.Point2.Y - line.Point1.Y) * (this.Point2.X - this.Point1.X) - (line.Point2.X - line.Point1.X) * (this.Point2.Y - this.Point1.Y));

        return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;
    }

    public Vector2 IntersectionPoint(Line line)
    {
        var uA = ((line.Point2.X - line.Point1.X) * (this.Point1.Y - line.Point1.Y) - (line.Point2.Y - line.Point1.Y) * (this.Point1.X - line.Point1.X)) / ((line.Point2.Y - line.Point1.Y) * (this.Point2.X - this.Point1.X) - (line.Point2.X - line.Point1.X) * (this.Point2.Y - this.Point1.Y));

        var uB = ((this.Point2.X - this.Point1.X) * (this.Point1.Y - line.Point1.Y) - (this.Point2.Y - this.Point1.Y) * (this.Point1.X - line.Point1.X)) / ((line.Point2.Y - line.Point1.Y) * (this.Point2.X - this.Point1.X) - (line.Point2.X - line.Point1.X) * (this.Point2.Y - this.Point1.Y));

        var intersectionX = this.Point1.X + (uA * (this.Point2.X - this.Point1.X));
        var intersectionY = this.Point1.Y + (uA * (this.Point2.Y - this.Point1.Y));

        return new Vector2(intersectionX, intersectionY);
    }

    public override int GetHashCode() => HashCode.Combine(Point1, Point2);
}
