namespace SpaceEventMod.Core.Geometry.Helpers;

internal static class PolygonHelpers
{
    /*/// <summary>
    /// Triangulates the polygon using the ear clipping method.<br/>
    /// <see href="https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf">Click here for more info.</see>
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="Triangle"/>s that compose the polygon.</returns>
    public static List<Triangle> Triangulate(ITriangulate shape)
    {
        var trianglePoints = shape.Points;
        var polygon = new Polygon(trianglePoints.ToArray());

        polygon.OrientClockwise();

        var triangles = new List<Triangle>();

        // If the polygon is not a triangle remove an ear from the polygon.
        while (polygon.Vertices.Count() > 3)
            polygon.FindAndRemoveEar(ref triangles);

        triangles.Add(new Triangle(polygon.Vertices[0], polygon.Vertices[1], polygon.Vertices[2]));
        return triangles;
    }

    /// <summary>
    /// Makes sure the polygon is clockwise oriented so we can find concave & convex vertices
    /// </summary>
    private static Polygon OrientClockwise(in Polygon polygon)
    {
        var vertices = Vertices.ToList();
        vertices.Add(Vertices[0]);

        float polygonArea = 0;
        for (var i = 0; i < Vertices.Length; i++)
            polygonArea += (vertices[i + 1].X - vertices[i].X) * (vertices[i + 1].Y + vertices[i].Y) / 2;

        if (polygonArea > 0)
            Vertices.Reverse();
    }

    private static void FindAndRemoveEar(ref List<Triangle> triangles)
    {
        // Check for an ear
        int[] triangle = [0, 0, 0];
        triangle = FindEar(triangle[0], triangle[1], triangle[2]);

        // Add a new ear to the list
        if (triangle != null)
        {
            triangles.Add(new Triangle(Vertices[triangle[0]], Vertices[triangle[1]], Vertices[triangle[2]]));
            // Remove the ear from the polygon.
            var vertices = Vertices.ToList();
            vertices.RemoveAt(triangle[1]);
            Vertices = vertices.ToArray();
        }
    }

    private static int[] FindEar(int leftVertex, int middleVertex, int rightVertex)
    {
        for (leftVertex = 0; leftVertex < Vertices.Length; leftVertex++)
        {
            middleVertex = (leftVertex + 1) % Vertices.Length;//if vertex0 was the last point or last - 1 point take the last or first point
            rightVertex = (middleVertex + 1) % Vertices.Length;//if vertex0 was the last point or last - 1 point take the first or second point
            // Send three points and check if it's an ear or not
            if (CheckEar(Vertices, leftVertex, middleVertex, rightVertex))
                return [leftVertex, middleVertex, rightVertex];
        }
        return null;
    }

    private static bool CheckEar(Vector2[] points, int leftVertex, int middleVertex, int rightVertex)
    {
        // Check if p1 is concave
        var angle = GetAngle(points[leftVertex], points[middleVertex], points[rightVertex]);
        if (angle > 180 || angle < -180)
            return false;

        var triangle = new Polygon([points[leftVertex], points[middleVertex], points[rightVertex]]);
        // Make sure there is no point inside our ear
        for (var i = 0; i < points.Length; i++)
        {
            if ((i != leftVertex) && (i != middleVertex) && (i != rightVertex))
            {
                if (triangle.PointInsidePolygon(points[i]))
                    return false;
            }
        }
        return true;
    }

    private static float GetAngle(Vector2 leftVertex, Vector2 middleVertex, Vector2 rightVertex)
    {
        // Get angle
        var radians = Math.Atan(Cross(leftVertex - middleVertex, rightVertex - middleVertex) / Vector2.Dot(leftVertex - middleVertex, rightVertex - middleVertex));
        var angle = radians * (180 / Math.PI);

        return (float)angle;
    }

    private static float Cross(Vector2 value1, Vector2 value2) => (value1.X * value2.Y) - (value1.Y * value2.X);*/

}
