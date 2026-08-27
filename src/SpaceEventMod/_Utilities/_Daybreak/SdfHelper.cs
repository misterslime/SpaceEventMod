using Daybreak.Common.Mathematics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod;

internal static class SdfHelper
{
    // https://iquilezles.org/articles/distfunctions2d/ from here
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SdfSample Ellipse(Vector2 point, Vector2 ab)
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
        return new SdfSample((Vector2.Dot(point / ab, point / ab) > 1.0f) ? d : -d, Vector2.Zero);
    }
}
