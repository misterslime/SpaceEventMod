using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Graphics;

/// <summary>
/// Creates a render target that holds information about terraria's lighting colors for rendering purposes.
/// </summary>
internal static class LightingBuffer
{
    private const int BUFFER_PADDING = 4;

    private class Buffers : IStatic<Buffers>
    {
        public required RenderTargetLease LightingBuffer { get; init; }
        public required RenderTargetLease ScreenspaceLightingBuffer { get; init; }

        public static Buffers LoadData(Mod mod)
        {
            return Main.RunOnMainThread(() => new Buffers
            {
                LightingBuffer = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => ((offW / 16) + BUFFER_PADDING, (offH / 16) + BUFFER_PADDING)),
                ScreenspaceLightingBuffer = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH))
            }).GetAwaiter().GetResult();
        }

        public static void UnloadData(Buffers data)
        {
            Main.RunOnMainThread(() =>
            {
                data.LightingBuffer.Dispose();
                data.ScreenspaceLightingBuffer.Dispose();
            });
        }
    }

    private static Buffers s_buffers => Buffers.Instance;

    public static RenderTarget2D ScreenLightBuffer => Buffers.Instance.ScreenspaceLightingBuffer.Target;

    [OnLoad(Side = ModSide.Client)]
    internal static void LoadSystem()
    {
        On_Main.DoDraw_WallsAndBlacks += On_Main_DoDraw_WallsAndBlacks;
    }

    private static void On_Main_DoDraw_WallsAndBlacks(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        // Make sure to refresh light buffers before anything gets drawn
        using (var _ = Main.spriteBatch.Scope())
            DrawToLightBuffers();

        orig(self);
    }

    private static void DrawToLightBuffers()
    {
        int halfPadding = BUFFER_PADDING / 2;

        // render raw lighting buffer
        using (s_buffers.LightingBuffer.Scope(true, Color.White))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);

            var pixel = Assets.Textures.WhitePixel.Asset.Value;

            int screenX = (int)(Main.screenPosition.X / 16) - halfPadding;
            int screenY = (int)(Main.screenPosition.Y / 16) - halfPadding;

            for (int i = 0; i < s_buffers.LightingBuffer.Target.Width + halfPadding; i++)
            {
                for (int j = 0; j < s_buffers.LightingBuffer.Target.Width + halfPadding; j++)
                {
                    if (!WorldGen.InWorld(i, j))
                        continue;

                    var lightColor = Lighting.GetColor(screenX + i, screenY + j);
                    var target = new Rectangle(i, j, 1, 1);

                    Main.spriteBatch.Draw(pixel, target, lightColor);
                }
            }

            Main.spriteBatch.End();
        }

        // render screenspace lighting buffer
        using (s_buffers.ScreenspaceLightingBuffer.Scope(true, Color.White))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Vector2 drawPosition = -new Vector2(halfPadding * 16);
            drawPosition -= new Vector2(Main.screenPosition.X % 16f, Main.screenPosition.Y % 16f);

            Main.spriteBatch.Draw(s_buffers.LightingBuffer.Target, drawPosition, s_buffers.LightingBuffer.Target.Bounds, Color.White, 0f, Vector2.Zero, 16f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
        }
    }
}