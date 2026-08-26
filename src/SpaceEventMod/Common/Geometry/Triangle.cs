using Microsoft.Xna.Framework;

namespace SpaceEventMod.Common.Geometry;

/// <summary>
/// A polygon of just 3 vertices.
/// </summary>
/// <param name="A">First point of the triangle.</param>
/// <param name="B">Second point of the triangle.</param>
/// <param name="C">Third point of the triangle.</param>
internal sealed class Triangle(Vector2 A, Vector2 B, Vector2 C) : Polygon([A, B, C])
{
}