using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Graphics;

public struct DrawAction(Effect effect, Action<SpriteBatch> action)
{
    public Action<SpriteBatch> action = action;
    public Effect effect = effect;
}

[Autoload(Side = ModSide.Client)]
public class PixelRenderer : ModSystem
{
    public static RenderTarget2D PixelRenderTarget;

    public static List<DrawAction> DrawActions = new List<DrawAction>();

    public override void Load()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths += DrawToTarget;
            On_Main.DrawInfernoRings += DrawPixelatedSprites;

            PixelRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
        });
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths -= DrawToTarget;
            On_Main.DrawInfernoRings -= DrawPixelatedSprites;

            PixelRenderTarget?.Dispose();
            PixelRenderTarget = null;
        });
    }

    private static void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (Main.gameMenu)
        {
            orig();
            return;
        }

        var pixelationMatrix = GetPixelationMatrix();

        if (PixelRenderTarget == null || PixelRenderTarget.Width != Main.screenWidth || PixelRenderTarget.Height != Main.screenHeight)
        {
            PixelRenderTarget?.Dispose();
            PixelRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
        }

        if (DrawActions.Count > 0)
        {
            Main.graphics.GraphicsDevice.SetRenderTarget(PixelRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (var drawAction in DrawActions)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, drawAction.effect, pixelationMatrix);
                drawAction.action.Invoke(Main.spriteBatch);
                Main.spriteBatch.End();
            }

            Main.instance.GraphicsDevice.SetRenderTarget(null);
            DrawActions.Clear();
        }

        orig();
    }

    private static void DrawPixelatedSprites(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        if (PixelRenderTarget == null)
            return;

        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(PixelRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        orig(self);
    }

    public static Matrix GetPixelationMatrix()
    {
        // Credit to Nycro for the math here!
        // (and also to fry for helping me a lot with this impl)
        return Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateScale(0.5f / Main.GameViewMatrix.Zoom.X, 0.5f / Main.GameViewMatrix.Zoom.Y, 1f)
            * Matrix.CreateTranslation(Main.GameViewMatrix.Translation.X * 0.5f, Main.GameViewMatrix.Translation.Y * 0.5f, 0f);
    }

    public static void Draw(Effect effect, Action<SpriteBatch> action)
    {
        var drawAction = new DrawAction(effect, action);
        DrawActions.Add(drawAction);
    }
}
