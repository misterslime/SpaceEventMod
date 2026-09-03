using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Geometry;

namespace SpaceEventMod.Common.SDFs;

/// <summary>
/// Represents an SDF line segment.
/// </summary>
internal struct Segment(float radius, Vector2 point1, Vector2 point2) : ISignedDistance2D
{
    public float Radius { get; set; } = radius;
    public Line Line { get; set; } = new Line(point1, point2);

    public Vector3 GetSignedDistance(Vector2 position)
    {
        Vector2 ba = Line.Point2 - Line.Point1, pa = position - Line.Point1;
        float h = MathHelper.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0.0f, 1.0f);
        Vector2 q = pa - h * ba;
        float d = q.Length();
        Vector2 g = q / d;

        return new Vector3(d - Radius, g.X, g.Y);
    }
}
