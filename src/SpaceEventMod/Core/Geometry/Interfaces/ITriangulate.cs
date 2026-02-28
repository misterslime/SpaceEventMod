using Microsoft.Xna.Framework;
using System;

namespace SpaceEventMod.Core.Geometry.Interfaces;

internal interface ITriangulate
{
    public ReadOnlySpan<Vector2> Points { get; }

    public bool PointInside(Vector2 point);
}
