using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Geometry;

// https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
// Ear clipping triangulation
public class Polygon
{
    public Vector2[] Vertices { get; set; }

    public Polygon(Vector2[] points)
    {
        Vertices = points;
    }

    public Polygon(Rectangle rectangle)
    {
        Vertices = [
            rectangle.TopLeft(),
            rectangle.TopRight(),
            rectangle.BottomRight(),
            rectangle.BottomLeft()
        ];
    }

    /// <summary>
    /// Triangulates the polygon using the ear clipping method.
    /// </summary>
    /// <returns></returns>
    public List<Triangle> Triangulate()
    {
        Vector2[] trianglePoints = new Vector2[Vertices.Length];
        trianglePoints = Vertices;
        Polygon polygon = new Polygon(trianglePoints);

        polygon.OrientClockwise();

        List<Triangle> triangles = new List<Triangle>();

        // If the polygon is not a triangle remove an ear from the polygon.
        while (polygon.Vertices.Count() > 3)
            polygon.FindAndRemoveEar(ref triangles);

        triangles.Add(new Triangle(polygon.Vertices[0], polygon.Vertices[1], polygon.Vertices[2]));
        return triangles;
    }

    /// <summary>
    /// Makes sure the polygon is clockwise oriented so we can find concave & convex vertices
    /// </summary>
    /// <returns></returns>
    private void OrientClockwise()
    {
        List<Vector2> vertices = Vertices.ToList();
        vertices.Add(Vertices[0]);

        float polygonArea = 0;
        for (int i = 0; i < Vertices.Length; i++)
            polygonArea += (vertices[i + 1].X - vertices[i].X) * (vertices[i + 1].Y + vertices[i].Y) / 2;

        if (polygonArea > 0)
            Vertices.Reverse();
    }

    private void FindAndRemoveEar(ref List<Triangle> triangles)
    {
        // Check for an ear
        int[] triangle = [0, 0, 0];
        triangle = FindEar(triangle[0], triangle[1], triangle[2]);

        // Add a new ear to the list
        if (triangle != null)
        {
            triangles.Add(new Triangle(Vertices[triangle[0]], Vertices[triangle[1]], Vertices[triangle[2]]));
            // Remove the ear from the polygon.
            List<Vector2> vertices = Vertices.ToList();
            vertices.RemoveAt(triangle[1]);
            Vertices = vertices.ToArray();
        }
    }

    private int[] FindEar(int leftVertex, int middleVertex, int rightVertex)
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

    private bool CheckEar(Vector2[] points, int leftVertex, int middleVertex, int rightVertex)
    {
        // Check if p1 is concave
        float angle = GetAngle(points[leftVertex], points[middleVertex], points[rightVertex]);
        if (angle > 180 || angle < -180)
            return false;

        Polygon triangle = new Polygon([points[leftVertex], points[middleVertex], points[rightVertex]]);
        // Make sure there is no point inside our ear
        for (int i = 0; i < points.Length; i++)
        {
            if ((i != leftVertex) && (i != middleVertex) && (i != rightVertex))
            {
                if (triangle.PointInsidePolygon(points[i]))
                    return false;
            }
        }
        return true;
    }

    public float GetAngle(Vector2 leftVertex, Vector2 middleVertex, Vector2 rightVertex)
    {
        // Get angle
        double radians = Math.Atan(Cross(leftVertex - middleVertex, rightVertex - middleVertex) / Vector2.Dot(leftVertex - middleVertex, rightVertex - middleVertex));
        double angle = radians * (180 / Math.PI);

        return (float)angle;
    }

    /// <summary>
    /// Normal cross product method.
    /// </summary>
    /// <returns></returns>
    public float Cross(Vector2 value1, Vector2 value2) => (value1.X * value2.Y) - (value1.Y * value2.X);

    /// <summary>
    /// Checks if a point is inside the polygon
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool PointInsidePolygon(Vector2 point)
    {
        bool result = false;
        int j = Vertices.Count() - 1;
        for (int i = 0; i < Vertices.Count(); i++)
        {
            if (Vertices[i].Y < point.X && Vertices[j].Y >= point.Y || Vertices[j].Y < point.Y && Vertices[i].Y >= point.Y)
            {
                if (Vertices[i].X + (point.Y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y) * (Vertices[j].X - Vertices[i].X) < point.X)
                    result = !result;
            }
            j = i;
        }
        return result;
    }
}