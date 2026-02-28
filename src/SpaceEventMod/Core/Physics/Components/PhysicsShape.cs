using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components;

internal enum ShapeAnchorMode
{
    None = 0,
    AnchorsObject = 1,
    AnchoredByObject = 2
}

internal struct PhysicsShape(PhysicsPoint[] points, bool closed = false) : IComponent
{
    public PhysicsPoint[] Points { get; set; } = points;
    public bool Closed { get; init; } = closed;

    public void MoveBy(Vector2 amount)
    {
        for (var i = 0; i < Points.Length; i++)
        {
            var point = Points[i];
            point.Position += amount;
            Points[i] = point;
        }
    }

    public float GetArea()
    {
        float area = 0;

        for (var i = 0; i < Points.Length; i++)
        {
            //int leftIndex = (i - 1 + physicsPoints.Length) % physicsPoints.Length;
            var rightIndex = (i + 1) % Points.Length;

            var point1 = Points[i].Position;
            var point2 = Points[rightIndex].Position;

            var width = point2.X - point1.X;
            var length = (point1.Y + point2.Y) * 0.5f;

            area += width * length;
        }

        return area;
    }

    public Vector2 GetCentroid()
    {
        var center = Vector2.Zero;

        foreach (var point in Points)
            center += point.Position;

        return center / Points.Length;
    }

    public Vector2 FindCenterOfPoints(int[] indexes)
    {
        var center = Vector2.Zero;

        foreach (var index in indexes)
            center += Points[index].Position;

        return center / indexes.Length;
    }

    public bool PointInside(Vector2 point)
    {
        var result = false;
        var j = Points.Length - 1;
        for (var i = 0; i < Points.Length; i++)
        {
            if (Points[i].Position.Y < point.X && Points[j].Position.Y >= point.Y || Points[j].Position.Y < point.Y && Points[i].Position.Y >= point.Y)
            {
                if (Points[i].Position.X + (point.Y - Points[i].Position.Y) / (Points[j].Position.Y - Points[i].Position.Y) * (Points[j].Position.X - Points[i].Position.X) < point.X)
                    result = !result;
            }
            j = i;
        }
        return result;
    }
}
