using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldGenSandbox.Utilities;

internal static class Vector2Extensions
{
    public static Vector2 Sign(this Vector2 v) => new Vector2(MathF.Sign(v.X), MathF.Sign(v.Y));

    public static Vector2 Abs(this Vector2 v) => new Vector2(MathF.Abs(v.X), MathF.Abs(v.Y));
}
