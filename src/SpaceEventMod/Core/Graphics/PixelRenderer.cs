using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Graphics;

public interface IDrawAction;

public struct PrimitiveDrawAction(Effect effect, PrimitiveType primitiveType, Action<PrimitiveBatch> action) : IDrawAction
{
    public Action<PrimitiveBatch> action = action;
    public PrimitiveType primitiveType = primitiveType;
    public Effect effect = effect;
}

public struct SpriteDrawAction(Effect effect, Action<SpriteBatch> action) : IDrawAction
{
    public Action<SpriteBatch> action = action;
    public Effect effect = effect;
}

[Autoload(Side = ModSide.Client)]
public class PixelRenderer : ModSystem
{
    public static RenderTarget2D PixelRenderTarget;

    public static List<IDrawAction> DrawActions = new List<IDrawAction>();

    public override void Load()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths += DrawToTarget;

            PixelRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
        });
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths -= DrawToTarget;

            PixelRenderTarget?.Dispose();
            PixelRenderTarget = null;
        });
    }

    private void DrawToTarget(On_Main.orig_CheckMonoliths orig)
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
                if (drawAction is SpriteDrawAction spriteDrawAction)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, spriteDrawAction.effect, pixelationMatrix);
                    spriteDrawAction.action.Invoke(Main.spriteBatch);
                    Main.spriteBatch.End();
                }
                else if (drawAction is PrimitiveDrawAction primitiveDrawAction)
                {
                    SpaceEventMod.PrimitiveBatch.Begin(primitiveDrawAction.primitiveType);
                    primitiveDrawAction.action.Invoke(SpaceEventMod.PrimitiveBatch);
                    SpaceEventMod.PrimitiveBatch.End();
                }
            }

            Main.instance.GraphicsDevice.SetRenderTarget(null);
            DrawActions.Clear();
        }

        orig();
    }

    public override void PostDrawTiles()
    {
        if (PixelRenderTarget == null)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(PixelRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        Main.spriteBatch.End();
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
        var drawAction = new SpriteDrawAction(effect, action);
        DrawActions.Add(drawAction);
    }

    public static void Draw(Effect effect, PrimitiveType primitiveType, Action<PrimitiveBatch> action)
    {
        var drawAction = new PrimitiveDrawAction(effect, primitiveType, action);
        DrawActions.Add(drawAction);
    }
}
