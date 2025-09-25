using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry;
using System;

namespace SpaceEventMod.Core.Physics.Collision;

public class SeparatingAxisTheorem
{
    private Polygon polygon1;
    private Vector2 polygon1Position;
    private Polygon polygon2;
    private Vector2 polygon2Position;

    public SeparatingAxisTheorem()
    {

    }

    public SeparatingAxisTheorem SetFirstPolygon(Polygon polygon, Vector2 position)
    {
        polygon1 = polygon;
        polygon1Position = position;

        return this;
    }

    public SeparatingAxisTheorem SetSecondPolygon(Polygon polygon, Vector2 position)
    {
        polygon2 = polygon;
        polygon2Position = position;

        return this;
    }

    /// <summary>
    /// Test collisions between polygons. 
    /// Testing twice with one being in reverse order, and then comparing length, removes artifacts where the hitbox extends too far.
    /// </summary>
    /// <returns>The normal collision <see cref="Vector2"/>, returns <see langword="null""/> if the polygons arent colliding.</returns>
    public Vector2? TestCollisionNormal()
    {
        // Run a test of each polygon against the other
        var testAB = SolveCollision(polygon1, polygon1Position, polygon2, polygon2Position);
        var testBA = SolveCollision(polygon2, polygon2Position, polygon1, polygon1Position, true);  // note the 'flip' flag is set.

        if (testAB is null || testBA is null)
            return null;

        var result = Math.Abs(testAB.Item2) < Math.Abs(testBA.Item2) ? testAB.Item1 : testBA.Item1;

        return result;
    }

    /// <summary>
    /// Apply separating axis theorem.<br/>
    /// <see href="https://dyn4j.org/2010/01/sat/">Click here for more info.</see>
    /// </summary>
    /// <param name="polygon1">First polygon.</param>
    /// <param name="polygon1Position">Position of the first polygon.</param>
    /// <param name="polygon2">Second polygon.</param>
    /// <param name="polygon2Position">Position of the second polygon.</param>
    /// <returns></returns>
    private Tuple<Vector2?, float> SolveCollision(Polygon polygon1, Vector2 polygon1Position, Polygon polygon2, Vector2 polygon2Position, bool flipResultPositions = false)
    {
        var shortestDist = float.MaxValue;

        // Get the offset between the two shapes
        var offset = polygon1Position - polygon2Position;

        var distance = 0f;
        var normal = Vector2.Zero;

        // Loop over all of the sides on the first polygon and check the perpendicular axis
        for (var i = 0; i < polygon1.Vertices.Length; i++)
        {
            // Get the perpendicular axis that we will be projecting onto
            var axis = GetPerpendicularAxis(polygon1.Vertices, i);

            var polygon1Range = ProjectVerticesForMinMax(axis, polygon1.Vertices);
            var polygon2Range = ProjectVerticesForMinMax(axis, polygon2.Vertices);

            var scalerOffset = Vector2.Dot(axis, offset);
            polygon1Range.X += scalerOffset;
            polygon2Range.Y += scalerOffset;

            // Now check for a gap betwen the relative min's and max's
            if (polygon1Range.X - polygon2Range.Y > 0 || polygon2Range.X - polygon1Range.Y > 0)
                return null;

            var distanceMinimum = (polygon2Range.Y - polygon1Range.X) * -1;
            if (flipResultPositions)
                distanceMinimum *= -1;

            var distMinimumAbs = Math.Abs(distanceMinimum);
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
    private Vector2 ProjectVerticesForMinMax(Vector2 axis, Vector2[] vertices)
    {
        // Note that we project the first point to both min and max
        var minimum = Vector2.Dot(axis, vertices[0]);
        var maximum = minimum;

        for (var j = 1; j < vertices.Length; j++)
        {
            var temp = Vector2.Dot(axis, vertices[j]);
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
    private Vector2 GetPerpendicularAxis(Vector2[] vertices, int index)
    {
        var point1 = vertices[index];
        var point2 = index >= vertices.Length - 1 ? vertices[0] : vertices[index + 1];  // Get the next index, or wrap around if at the end

        var axis = new Vector2(-(point2.Y - point1.Y), point2.X - point1.X);
        axis.Normalize();
        return axis;
    }
}