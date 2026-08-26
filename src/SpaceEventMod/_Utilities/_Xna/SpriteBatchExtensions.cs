using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.DataStructures;
using System;
using System.Runtime.CompilerServices;

namespace SpaceEventMod;

public static class SpriteBatchExtensions
{
    public static void Begin(this SpriteBatch spriteBatch, SpriteBatchSnapshot data)
    {
        spriteBatch.Begin(
            data.SortMode,
            data.BlendState,
            data.SamplerState,
            data.DepthStencilState,
            data.RasterizerState,
            data.CustomEffect,
            data.TransformMatrix
        );
    }

    public static void EndBegin(this SpriteBatch spriteBatch, SpriteBatchSnapshot data)
    {
        spriteBatch.End();
        spriteBatch.Begin(data);
    }

    public static SpriteBatchSnapshot Capture(this SpriteBatch spriteBatch) => new(spriteBatch);

    public static void End(this SpriteBatch spriteBatch, out SpriteBatchSnapshot snapshot)
    {
        snapshot = spriteBatch.Capture();
        spriteBatch.End();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        var v = Vector2.Normalize(begin - end);
        var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(Assets.Textures.WhitePixel.Asset.Value, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}