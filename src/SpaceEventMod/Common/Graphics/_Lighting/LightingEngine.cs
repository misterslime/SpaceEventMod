using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Graphics;

internal class DrawEventArgs(SpriteBatch spriteBatch, RenderTarget2D lightBuffer) : EventArgs
{
    public SpriteBatch SpriteBatch { get; } = spriteBatch;
    public RenderTarget2D LightingBuffer { get; } = lightBuffer;
}

/// <summary>
/// API for rendering various things at arbitrary points of the rendering process, with things being colored using a lighting buffer.
/// </summary>
internal class LightingEngine
{
    private class Buffers : IStatic<Buffers>
    {
        public required RenderTargetLease ExtraDrawStuffBuffer { get; init; }

        public static Buffers LoadData(Mod mod)
        {
            return Main.RunOnMainThread(() => new Buffers
            {
                ExtraDrawStuffBuffer = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH))
            }).GetAwaiter().GetResult();
        }

        public static void UnloadData(Buffers data)
        {
            Main.RunOnMainThread(() =>
            {
                data.ExtraDrawStuffBuffer.Dispose();
            });
        }
    }

    public static event EventHandler<DrawEventArgs> PreDrawWalls;
    public static event EventHandler<DrawEventArgs> PreTilesPostWalls;


    [OnLoad(Side = ModSide.Client)]
    internal static void LoadSystem()
    {
        On_Main.DoDraw_WallsAndBlacks += On_Main_DoDraw_WallsAndBlacks;
        On_Main.DrawBlack += On_Main_DrawBlack;
    }

    private static void On_Main_DoDraw_WallsAndBlacks(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        orig(self);
        CallDraw(PreTilesPostWalls);
    }

    private static void On_Main_DrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
    {
        CallDraw(PreDrawWalls);
        orig(self, force);
    }

    private static void CallDraw(EventHandler<DrawEventArgs> drawEvent)
    {
        using var _ = Main.spriteBatch.Scope();

        using (Buffers.Instance.ExtraDrawStuffBuffer.Scope(clearColor: Color.Transparent))
        {
            Main.spriteBatch.Begin(
                        SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        Main.DefaultSamplerState,
                        DepthStencilState.Default,
                        Main.Rasterizer,
                        null,
                        Main.GameViewMatrix.TransformationMatrix);

            drawEvent?.Invoke(null, new DrawEventArgs(Main.spriteBatch, LightingBuffer.ScreenLightBuffer));

            Main.spriteBatch.End();
        }

        var effect = Assets.Shaders.Fragment.LightingBuffer.CreateLightedTargetPass();
        effect.Parameters.LightingBuffer = LightingBuffer.ScreenLightBuffer;
        effect.Apply();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, effect.Shader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(Buffers.Instance.ExtraDrawStuffBuffer.Target, Vector2.Zero, Buffers.Instance.ExtraDrawStuffBuffer.Target.Bounds, Color.White, 0f, Vector2.Zero, 1f, Main.GameViewMatrix.Effects, 0f);
        Main.spriteBatch.End();
    }
}
