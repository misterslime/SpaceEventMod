using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space;
using SpaceEventMod.Content.Events.Space.Rendering;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Utilities;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.GameContent.TextureAssets;

namespace SpaceEventMod.Core.Graphics;

[Autoload(Side = ModSide.Client)]
// note: this shit is ass
internal class Fade : ModSystem
{
    private record struct DrawAction(float Scale, ActionRef<Pipeline> Action);

    private static RenderTarget2D s_bufferTarget;
    private static RenderTarget2D s_fadeTarget;
    private static Queue<DrawAction> s_drawActions;

    public override void Load()
    {
        s_drawActions = new Queue<DrawAction>();

        Main.QueueMainThreadAction(() =>
        {
            On_Main.DrawNPCs += DrawSeaForeground;
            On_Main.CheckMonoliths += DrawToTarget;

            s_fadeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            s_bufferTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        });
    }

    public override void Unload()
    {
        s_drawActions.Clear();
        s_drawActions = null;

        Main.QueueMainThreadAction(() =>
        {
            On_Main.DrawNPCs -= DrawSeaForeground;
            On_Main.CheckMonoliths -= DrawToTarget;

            s_fadeTarget?.Dispose();
            s_fadeTarget = null;

            s_bufferTarget?.Dispose();
            s_bufferTarget = null;
        });
    }

    private void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (!Main.gameMenu)
        {
            if (s_fadeTarget == null || s_fadeTarget.Width != Main.screenWidth || s_fadeTarget.Height != Main.screenHeight)
            {
                s_fadeTarget?.Dispose();
                s_fadeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }

            if (s_bufferTarget == null || s_bufferTarget.Width != Main.screenWidth || s_bufferTarget.Height != Main.screenHeight)
            {
                s_bufferTarget?.Dispose();
                s_bufferTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(s_fadeTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 position = Main.screenLastPosition - Main.screenPosition;

            Color color = Color.White;

            color = Color.Lerp(Color.Transparent, Color.White, 0.995f);

            Main.spriteBatch.Draw(s_bufferTarget, position, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();

            while (s_drawActions.Any())
            {
                DrawAction action = s_drawActions.Dequeue();

                Pipeline pipeline = Graphics.BeginPipeline(action.Scale);

                action.Action.Invoke(in pipeline);

                pipeline.Flush();
            }

            DrawWithBuffer(s_fadeTarget, (spriteBatch) =>
            {
                spriteBatch.Draw(s_fadeTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            });

            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }

        orig();
    }

    private void DrawSeaForeground(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        if (s_fadeTarget is not null)
        {
            var snapshot = Main.spriteBatch.Capture();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(s_fadeTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(snapshot);
        }

        orig(self, behindTiles);
    }

    public static void Draw(float scale, ActionRef<Pipeline> action)
    {
        s_drawActions.Enqueue(new(scale, action));
    }

    public static void CopyContents(RenderTarget2D a, RenderTarget2D b)
    {
        Main.graphics.GraphicsDevice.SetRenderTarget(a);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);

        Main.spriteBatch.Draw(b, b.Bounds, Color.White);

        Main.spriteBatch.End();
    }

    public static void DrawWithBuffer(RenderTarget2D t, Action<SpriteBatch> Draw)
    {
        Main.graphics.GraphicsDevice.SetRenderTarget(s_bufferTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);

        Draw.Invoke(Main.spriteBatch);

        Main.spriteBatch.End();

        CopyContents(t, s_bufferTarget);
    }
}
