using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Common.Geometry;

// Original code by George W. Clingerman -> http://www.xnadevelopment.com/tutorials/rotatedrectanglecollisions/rotatedrectanglecollisions.shtml
internal struct RotatedRectangle(Rectangle rectangle, float rotation)
{
    public Rectangle Rectangle { get; set; } = rectangle;
    public float Rotation { get; set; } = rotation;
    public Vector2 Origin { get; set; } = new Vector2((int)rectangle.Width / 2, (int)rectangle.Height / 2);

    /// <summary>
    /// Change the position of the rectangle.
    /// </summary>
    /// <param name="offset">Amount to move the rectangle by.</param>
    public void Offset(Point offset) => Rectangle.Offset(offset);

    /// <summary>
    /// Check if this rectangle is intersecting a regular rectangle.
    /// </summary>
    /// <param name="rectangle">Rectangle to check.</param>
    public bool Intersects(Rectangle rectangle) => Intersects(new RotatedRectangle(rectangle, 0.0f));

    /// <summary>
    /// Check to see if two Rotated Rectangles have collided.
    /// </summary>
    /// <param name="rectangle">Rectangle to check.</param>
    public bool Intersects(RotatedRectangle rectangle)
    {
        Vector2[] axises = 
        [
            TopRight() - TopLeft(),
            TopRight() - BottomRight(),
            rectangle.TopLeft() - rectangle.BottomLeft(),
            rectangle.TopLeft() - rectangle.TopRight()
        ];

        foreach (Vector2 axis in axises)
        {
            if (!IsAxisCollision(rectangle, axis))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines if a collision has occurred on an axis of one of the
    /// planes parallel to the Rectangle
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private bool IsAxisCollision(RotatedRectangle rectangle, Vector2 axis)
    {
        int[] rectangleAScalars =
        [
            GenerateScalar(rectangle.TopLeft(), axis),
            GenerateScalar(rectangle.TopRight(), axis),
            GenerateScalar(rectangle.BottomLeft(), axis),
            GenerateScalar(rectangle.BottomRight(), axis)
        ];

        int[] rectangleBScalars =
        [
            GenerateScalar(TopLeft(), axis),
            GenerateScalar(TopRight(), axis),
            GenerateScalar(BottomLeft(), axis),
            GenerateScalar(BottomRight(), axis)
        ];

        int aRectangleAMinimum = rectangleAScalars.Min();
        int aRectangleAMaximum = rectangleAScalars.Max();
        int aRectangleBMinimum = rectangleBScalars.Min();
        int aRectangleBMaximum = rectangleBScalars.Max();

        if (aRectangleBMinimum <= aRectangleAMaximum && aRectangleBMaximum >= aRectangleAMaximum)
            return true;
        else if (aRectangleAMinimum <= aRectangleBMaximum && aRectangleAMaximum >= aRectangleBMaximum)
            return true;

        return false;
    }

    /// <summary>
    /// Generate a scalar value that can be used to compare where corners of 
    /// a rectangle have been projected onto a particular axis. 
    /// </summary>
    /// <param name="corner"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private int GenerateScalar(Vector2 corner, Vector2 axis)
    {
        float numerator = (corner.X * axis.X) + (corner.Y * axis.Y);
        float denominator = (axis.X * axis.X) + (axis.Y * axis.Y);
        float result = numerator / denominator;
        Vector2 projection = new Vector2(result * axis.X, result * axis.Y);

        return (int)((axis.X * projection.X) + (axis.Y * projection.Y));
    }

    public Vector2 TopLeft()
    {
        Vector2 topLeft = new Vector2(Rectangle.Left, Rectangle.Top);
        return topLeft.RotatedBy(Rotation, topLeft + Origin);
    }

    public Vector2 TopRight()
    {
        Vector2 topRight = new Vector2(Rectangle.Right, Rectangle.Top);
        return topRight.RotatedBy(Rotation, topRight + new Vector2(-Origin.X, Origin.Y));
    }

    public Vector2 BottomLeft()
    {
        Vector2 bottomLeft = new Vector2(Rectangle.Left, Rectangle.Bottom);
        return bottomLeft.RotatedBy(Rotation, bottomLeft + new Vector2(Origin.X, -Origin.Y));
    }

    public Vector2 BottomRight()
    {
        Vector2 bottomRight = new Vector2(Rectangle.Right, Rectangle.Bottom);
        return bottomRight.RotatedBy(Rotation, bottomRight + new Vector2(-Origin.X, -Origin.Y));
    }

    public int X { get => Rectangle.X; }

    public int Y { get => Rectangle.Y; }

    public int Width { get => Rectangle.Width; }

    public int Height { get => Rectangle.Height; }
}
