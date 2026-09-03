using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEventMod.Common.SDFs;

internal struct Ellipse(Vector2 center, Vector2 dimensions) : ISignedDistance2D
{
    public Vector2 Center { get; set; } = center;
    public Vector2 Dimensions { get; set; } = dimensions;

    public Vector3 GetSignedDistance(Vector2 position)
    {
        var sample = GetSignedDistance(position, Center, Dimensions);

        return sample;
    }

    public static Vector3 GetSignedDistance(Vector2 position, Vector2 Center, Vector2 Dimensions)
    {
        position -= Center;

        Vector2 sp = position.Sign(); position = position.Abs();

        bool s = Vector2.Dot(position / Dimensions, position / Dimensions) > 1.0;
        float w = MathF.Atan2(position.Y * Dimensions.X, position.X * Dimensions.Y);
        if (!s) w = (Dimensions.X * (position.X - Dimensions.X) < Dimensions.Y * (position.Y - Dimensions.Y)) ? MathHelper.PiOver2 : 0;

        for (int i = 0; i < 4; i++)
        {
            Vector2 cs = new Vector2(MathF.Cos(w), MathF.Sin(w));
            Vector2 u = Dimensions * new Vector2(cs.X, cs.Y);
            Vector2 v = Dimensions * new Vector2(-cs.Y, cs.X);
            w = w + Vector2.Dot(position - u, v) / (Vector2.Dot(position - u, u) + Vector2.Dot(v, v));
        }
        Vector2 q = Dimensions * new Vector2(MathF.Cos(w), MathF.Sin(w));

        float d = (position - q).Length();
        Vector2 g = sp * (position - q) / d;

        d *= s ? 1.0f : -1.0f;
        g *= s ? 1.0f : -1.0f;

        return new Vector3(d, g.X, g.Y);
    }

    public override int GetHashCode() => HashCode.Combine(Center, Dimensions);
}
