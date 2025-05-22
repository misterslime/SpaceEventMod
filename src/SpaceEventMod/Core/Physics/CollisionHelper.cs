using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry;
using System;

namespace SpaceEventMod.Core.Physics;

public class CollisionHelper
{
    /// <summary>
    /// Test collisions between polygons. 
    /// Testing twice with one being in reverse order, and then comparing length, removes artifacts where the hitbox extends too far.
    /// </summary>
    /// <param name="polygon1"></param>
    /// <param name="polygon1Position"></param>
    /// <param name="polygon2"></param>
    /// <param name="polygon2Position"></param>
    /// <returns></returns>
    public static Vector2? TestCollisions(Polygon polygon1, Vector2 polygon1Position, Polygon polygon2, Vector2 polygon2Position)
    {
        // Run a test of each polygon against the other
        Tuple<Vector2?, float> testAB = SeparatingAxisTheorem(polygon1, polygon1Position, polygon2, polygon2Position);
        Tuple<Vector2?, float> testBA = SeparatingAxisTheorem(polygon2, polygon2Position, polygon1, polygon1Position, true);  // note the 'flip' flag is set.

        if (testAB is null || testBA is null)
            return null;

        Vector2? result = (Math.Abs(testAB.Item2) < Math.Abs(testBA.Item2)) ? testAB.Item1 : testBA.Item1;

        return result;
    }

    /// <summary>
    /// https://dyn4j.org/2010/01/sat/
    /// Separating axis theorem
    /// </summary>
    /// <param name="polygon1"></param>
    /// <param name="polygon1Position"></param>
    /// <param name="polygon2"></param>
    /// <param name="polygon2Position"></param>
    /// <returns></returns>
    private static Tuple<Vector2?, float> SeparatingAxisTheorem(Polygon polygon1, Vector2 polygon1Position, Polygon polygon2, Vector2 polygon2Position, bool flipResultPositions = false)
    {
        float shortestDist = float.MaxValue;

        // Get the offset between the two shapes
        Vector2 offset = polygon1Position - polygon2Position;

        float distance = 0f;
        Vector2 normal = Vector2.Zero;

        // Loop over all of the sides on the first polygon and check the perpendicular axis
        for (int i = 0; i < polygon1.Vertices.Length; i++)
        {
            // Get the perpendicular axis that we will be projecting onto
            Vector2 axis = GetPerpendicularAxis(polygon1.Vertices, i);

            Vector2 polygon1Range = ProjectVerticesForMinMax(axis, polygon1.Vertices);
            Vector2 polygon2Range = ProjectVerticesForMinMax(axis, polygon2.Vertices);

            float scalerOffset = Vector2.Dot(axis, offset);
            polygon1Range.X += scalerOffset;
            polygon2Range.Y += scalerOffset;

            // Now check for a gap betwen the relative min's and max's
            if ((polygon1Range.X - polygon2Range.Y > 0) || (polygon2Range.X - polygon1Range.Y > 0))
                return null;

            float distanceMinimum = (polygon2Range.Y - polygon1Range.X) * -1;
            if (flipResultPositions)
                distanceMinimum *= -1;

            float distMinimumAbs = Math.Abs(distanceMinimum);
            if (distMinimumAbs < shortestDist)
            {
                shortestDist = distMinimumAbs;

                distance = distanceMinimum;
                normal = axis;
            }
        }

        if (distance == 0f && normal == Vector2.Zero)
            return null;

        // Calc the final separation
        return new Tuple<Vector2?, float>(normal * distance, distance);
    }

    /// <summary>
    /// Loops over all of the vertices in an array, projects them onto the given axis, and return the min / max range of all points
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="vertices"></param>
    /// <returns></returns>
    private static Vector2 ProjectVerticesForMinMax(Vector2 axis, Vector2[] vertices)
    {
        // Note that we project the first point to both min and max
        float minimum = Vector2.Dot(axis, vertices[0]);
        float maximum = minimum;

        for (int j = 1; j < vertices.Length; j++)
        {
            float temp = Vector2.Dot(axis, vertices[j]);
            if (temp < minimum)
                minimum = temp;
            if (temp > maximum)
                maximum = temp;
        }

        return new Vector2(minimum, maximum);
    }

    /// <summary>
    /// Small helper method that looks at the verts of the polygon and return the perpendicular axis of a particular side
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static Vector2 GetPerpendicularAxis(Vector2[] vertices, int index)
    {
        Vector2 point1 = vertices[index];
        Vector2 point2 = index >= vertices.Length - 1 ? vertices[0] : vertices[index + 1];  // Get the next index, or wrap around if at the end

        Vector2 axis = new Vector2(-(point2.Y - point1.Y), point2.X - point1.X);
        axis.Normalize();
        return axis;
    }
}