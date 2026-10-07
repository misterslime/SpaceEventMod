using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Tweening;
using SpaceEventMod.Content.Space.LevelElements;
using System;
using System.Collections.Generic;
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

    public static event EventHandler<DrawEventArgs> BeforeWalls;
    public static event EventHandler<DrawEventArgs> AfterWalls;
    public static event EventHandler<DrawEventArgs> BeforeTiles;
    public static event EventHandler<DrawEventArgs> AfterTiles;
    //public static event EventHandler<DrawEventArgs> BeforeProjectiles;
    //public static event EventHandler<DrawEventArgs> AfterProjectiles;
    public static event EventHandler<DrawEventArgs> BeforeNPCs;
    public static event EventHandler<DrawEventArgs> AfterNPCs;
    //public static event EventHandler<DrawEventArgs> BeforePlayers;
    //public static event EventHandler<DrawEventArgs> AfterPlayers;
    public static event EventHandler<DrawEventArgs> AfterDusts;

    [OnLoad(Side = ModSide.Client)]
    internal static void LoadSystem()
    {
        On_Main.DoDraw_WallsAndBlacks += On_Main_DoDraw_WallsAndBlacks;
        On_Main.DrawBlack += On_Main_DrawBlack;
        On_Main.DrawDust += On_Main_DrawDust;
        On_Main.DrawNPCs += On_Main_DrawNPCs;
    }

    private static void On_Main_DoDraw_WallsAndBlacks(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        orig(self);
        CallDraw(AfterWalls, Vector2.Zero);
    }

    private static void On_Main_DrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
    {
        orig(self, force);
        CallDraw(BeforeWalls, !Main.drawToScreen ? - new Vector2(Main.offScreenRange) : Vector2.Zero);
    }

    private static void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        if (behindTiles)
        {
            CallDraw(BeforeTiles, Vector2.Zero);
            orig(self, behindTiles);
        }
        else
        {
            CallDraw(AfterTiles, Vector2.Zero);
            CallDraw(BeforeNPCs, Vector2.Zero);
            orig(self, behindTiles);
            CallDraw(AfterNPCs, Vector2.Zero);
        }
    }

    private static void On_Main_DrawDust(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);
        CallDraw(AfterDusts, Vector2.Zero);
    }

    private static void CallDraw(EventHandler<DrawEventArgs> drawEvent, Vector2 lightDisplacement)
    {
        if (drawEvent is null)
            return;

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

            drawEvent?.Invoke(null, new DrawEventArgs(Main.spriteBatch, LightingBuffer.ScreenLightmap.Target));

            Main.spriteBatch.End();
        }

        var effect = Assets.Shaders.Fragment.LightingBuffer.CreateLightedTargetPass();
        effect.Parameters.LightingBuffer = LightingBuffer.ScreenLightmap.Target;
        effect.Parameters.ScreenDisplacement = new Vector2(lightDisplacement.X / Buffers.Instance.ExtraDrawStuffBuffer.Target.Width, lightDisplacement.Y / Buffers.Instance.ExtraDrawStuffBuffer.Target.Height * Main.LocalPlayer.gravDir);
        effect.Apply();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, effect.Shader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(Buffers.Instance.ExtraDrawStuffBuffer.Target, Vector2.Zero, Buffers.Instance.ExtraDrawStuffBuffer.Target.Bounds, Color.White, 0f, Vector2.Zero, 1f, Main.GameViewMatrix.Effects, 0f);
        Main.spriteBatch.End();
    }
}
