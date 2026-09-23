using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace WorldGenSandbox.Utilities;

internal static class Vector2Extensions
{
    public static Vector2 Sign(this Vector2 v) => 
        new Vector2(MathF.Sign(v.X), MathF.Sign(v.Y));

    public static Vector2 Abs(this Vector2 v) => 
        new Vector2(MathF.Abs(v.X), MathF.Abs(v.Y));

    public static Vector2 Clamp(this Vector2 v, float max) => 
        v.LengthSquared() > max * max ? v.SafeNormalize(Vector2.Zero) * max : v;

    public static Vector2 PerpendicularClockwise(this Vector2 v) =>
        new Vector2(v.Y, -v.X);

    public static Vector2 PerpendicularCounterClockwise(this Vector2 v) =>
        new Vector2(-v.Y, v.X);
}
