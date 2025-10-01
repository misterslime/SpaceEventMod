using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry.Interfaces;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Physics.Components;

internal enum ShapeAnchorMode
{
    None = 0,
    AnchorsObject = 1,
    AnchoredByObject = 2
}

internal struct PhysicsShape(PhysicsPoint[] points) : IComponent
{
    public PhysicsPoint[] Points { get; set; } = points;

    public void MoveBy(Vector2 amount)
    {
        for (var i = 0; i < Points.Length; i++)
        {
            PhysicsPoint point = Points[i];
            point.Position += amount;
            Points[i] = point;
        }
    }

    public float GetArea()
    {
        float area = 0;

        for (int i = 0; i < Points.Length; i++)
        {
            //int leftIndex = (i - 1 + physicsPoints.Length) % physicsPoints.Length;
            int rightIndex = (i + 1) % Points.Length;

            Vector2 point1 = Points[i].Position;
            Vector2 point2 = Points[rightIndex].Position;

            float width = point2.X - point1.X;
            float length = (point1.Y + point2.Y) * 0.5f;

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
