using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.World;

internal static class SignedDistanceFunctions
{
    // https://iquilezles.org/articles/smin/ from here
    public static float SmoothMinimum(float a, float b, float k)
    {
        k *= 1.0f / (1.0f - MathF.Sqrt(0.5f));
        return MathF.Max(k, MathF.Min(a, b)) -
               Vector2.Max(new Vector2(k, k) - new Vector2(a, b), new Vector2(0.0f)).Length();
    }

    public static float CircleSDF(Vector2 p, float r)
    {
        return p.Length() - r;
    }

    // https://iquilezles.org/articles/distfunctions2d/ from here
    public static float EllipseSDF(Vector2 point, Vector2 ab)
    {
        point.X = MathF.Abs(point.X);
        point.Y = MathF.Abs(point.Y);

        Vector2 q = ab * (point - ab);
        Vector2 cs = Vector2.Normalize((q.X < q.Y) ? new Vector2(0.01f, 1) : new Vector2(1, 0.01f));

        // find root with Newton solver
        for (int i = 0; i < 5; i++)
        {
            Vector2 u = ab * new Vector2(cs.X, cs.Y);
            Vector2 v = ab * new Vector2(-cs.Y, cs.X);
            float a = Vector2.Dot(point - u, v);
            float c = Vector2.Dot(point - u, u) + Vector2.Dot(v, v);
            float b = MathF.Sqrt(c * c - a * a);
            cs = new Vector2(cs.X * b - cs.Y * a, cs.Y * b + cs.X * a) / c;
        }

        // compute final point and distance
        float d = (point - ab * cs).Length();

        // return signed distance
        return (Vector2.Dot(point / ab, point / ab) > 1.0f) ? d : -d;
    }
}
