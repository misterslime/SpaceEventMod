using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldGenSandbox;

internal static class SpriteBatchExtensions
{
    public static void DrawLine(this SpriteBatch spriteBatch, Texture2D pixel, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        var v = Vector2.Normalize(begin - end);
        var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(pixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}
