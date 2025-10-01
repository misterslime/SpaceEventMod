using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Geometry;

/// <summary>
/// A collection of vertices that constitutes a 2D shape.
/// </summary>
/// <param name="points">List of points in the polygon.</param>
internal class Polygon(Vector2[] points) : IGeometry<Vector2>, ITriangulate
{
    protected Vector2[] _points = points;

    public ReadOnlySpan<Vector2> Points { get => _points; }

    public Vector2 GetPoint(int index) => _points[index];
    public void SetPoint(Vector2 point, int index) => _points[index] = point;

    /// <summary>
    /// Checks if a point is inside the polygon
    /// </summary>
    /// <param name="point">Point to check.</param>
    /// <returns><see langword="true"> the point is inside the polygon, <see langword="false"> if not.</returns>
    public bool PointInside(Vector2 point)
    {
        var result = false;
        var j = _points.Length - 1;
        for (var i = 0; i < _points.Length; i++)
        {
            if (_points[i].Y < point.X && _points[j].Y >= point.Y || _points[j].Y < point.Y && _points[i].Y >= point.Y)
            {
                if (_points[i].X + (point.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) * (_points[j].X - _points[i].X) < point.X)
                    result = !result;
            }
            j = i;
        }
        return result;
    }
}