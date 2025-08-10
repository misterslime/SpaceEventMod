using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.GameObjects.FirmamentSea;

public partial class FirmamentSeaSystem : ModSystem
{
    public static RenderTarget2D SeaRenderTarget;

    public static RenderTarget2D BackgroundRenderTarget;

    #region Detours
    private void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (!Main.gameMenu)
        {
            if (SeaRenderTarget == null || SeaRenderTarget.Width != Main.screenWidth || SeaRenderTarget.Height != Main.screenHeight)
            {
                SeaRenderTarget?.Dispose();
                SeaRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }

            if (BackgroundRenderTarget == null || SeaRenderTarget.Width != Main.screenWidth / 2 || SeaRenderTarget.Height != Main.screenHeight / 2)
            {
                BackgroundRenderTarget?.Dispose();
                BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(SeaRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Black);

            DrawBackgroundPrimitives(Color.Blue, Color.Magenta);
            DrawForegroundPrimitives();

            Main.graphics.GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            DrawBackground();

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

        }

        orig();
    }

    private void DrawSeaBackground(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
    {
        if (BackgroundRenderTarget is not null && Sea.Springs is not null)
        {
            var firmamentSeaBackgroundShader = Assets.Assets.Shaders.FirmamentSeaBackgroundTransparency.Value;

            firmamentSeaBackgroundShader.Parameters["sea"].SetValue(SeaRenderTarget);
            firmamentSeaBackgroundShader.Parameters["minimumAlpha"].SetValue(0.65f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, firmamentSeaBackgroundShader, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(BackgroundRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
        }

        orig(self);
    }

    private void DrawSeaForeground(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (SeaRenderTarget == null || Sea.Springs is null)
            return;

        // round position to prevent artifacts
        Vector2 screenPosition = WorldToSeaCoordinates(Main.screenPosition);

        screenPosition.X = MathF.Floor(screenPosition.X * 0.5f);
        screenPosition.X *= 2f;

        screenPosition.Y = MathF.Floor(screenPosition.Y * 0.5f);
        screenPosition.Y *= 2f;

        var firmamentSeaForegroundShader = Assets.Assets.Shaders.FirmamentSeaFoam.Value;

        firmamentSeaForegroundShader.Parameters["noise"].SetValue(Assets.Assets.Textures.Noise.Foam.Value);
        firmamentSeaForegroundShader.Parameters["palette"].SetValue(Assets.Assets.Textures.Palettes.FirmamentSea.NightForeground.Value);
        firmamentSeaForegroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        firmamentSeaForegroundShader.Parameters["parallax"].SetValue(1f);
        firmamentSeaForegroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        firmamentSeaForegroundShader.Parameters["screenWorldPosition"].SetValue(screenPosition);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, firmamentSeaForegroundShader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(SeaRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }
    #endregion

    #region Primitive Drawing
    private void DrawBackgroundPrimitives(Color top, Color bottom)
    {
        if (Sea.Springs is null)
            return;

        SpaceEventMod.PrimitiveBatch.Begin(PrimitiveType.TriangleList);

        for (int chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            for (int spring = 0; spring < Sea.Springs[chunk].Length; spring++)
            {
                Spring? next = null;
                var nodeLocation = chunk * Sea.ChunkSize + spring;

                if (spring < Sea.Springs[chunk].Length - 1)
                    next = Sea.Springs[chunk][spring + 1];
                else if (chunk < Sea.Springs.Length - 1)
                    next = Sea.Springs[chunk + 1][0];

                if (next is not null)
                {
                    var waveOffset = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * nodeLocation));
                    var waveOffset2 = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * (nodeLocation + 1)));

                    var seaScreenPosition = Sea.Position - Main.screenPosition;

                    var begin = new Vector2(Sea.NodeWidth * nodeLocation, Sea.Springs[chunk][spring].Position + waveOffset) + seaScreenPosition;
                    var end = new Vector2(Sea.NodeWidth * (nodeLocation + 1), next.Value.Position + waveOffset2) + seaScreenPosition;

                    var point1 = begin;
                    var point2 = new Vector2(begin.X, 0f);
                    var point3 = new Vector2(end.X, 0f);
                    var point4 = end;

                    var point5 = new Vector2(begin.X, begin.Y + 32f) * 0.5f;
                    var point6 = new Vector2(end.X, end.Y + 32f) * 0.5f;

                    // red is used to show how high up in the sea the pixel is
                    // blue to tell if the pixel is in the sea at all
                    SpaceEventMod.PrimitiveBatch
                        .AddVertex(point1, bottom)
                        .AddVertex(point2, top)
                        .AddVertex(point3, top)
                        .AddVertex(point4, bottom)
                        .AddVertex(point1, bottom)
                        .AddVertex(point3, top);
                }
            }
        }

        SpaceEventMod.PrimitiveBatch.End();
    }

    private void DrawForegroundPrimitives()
    {
        if (Sea.Springs is null)
            return;

        SpaceEventMod.PrimitiveBatch.Begin(PrimitiveType.TriangleList);

        for (int chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            for (int spring = 0; spring < Sea.Springs[chunk].Length; spring++)
            {
                Spring? next = null;
                var nodeLocation = chunk * Sea.ChunkSize + spring;

                if (spring < Sea.Springs[chunk].Length - 1)
                    next = Sea.Springs[chunk][spring + 1];
                else if (chunk < Sea.Springs.Length - 1)
                    next = Sea.Springs[chunk + 1][0];

                if (next is not null)
                {
                    var waveOffset = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * nodeLocation));
                    var waveOffset2 = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * (nodeLocation + 1)));

                    var seaScreenPosition = Sea.Position - Main.screenPosition;

                    var begin = new Vector2(Sea.NodeWidth * nodeLocation, Sea.Springs[chunk][spring].Position + waveOffset) + seaScreenPosition;
                    var end = new Vector2(Sea.NodeWidth * (nodeLocation + 1), next.Value.Position + waveOffset2) + seaScreenPosition;

                    var point1 = begin;
                    var point2 = new Vector2(begin.X, begin.Y - 120);
                    var point3 = new Vector2(end.X, end.Y - 120);
                    var point4 = end;

                    // red is used to show how high up in the sea the pixel is
                    // blue to tell if the pixel is in the sea at all
                    SpaceEventMod.PrimitiveBatch
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point2, Color.Transparent)
                        .AddVertex(point3, Color.Transparent)
                        .AddVertex(point4, new Color(0, 255, 0, 0))
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point3, Color.Transparent);
                }
            }
        }

        SpaceEventMod.PrimitiveBatch.End();
    }
    #endregion

    #region Layer Drawing Methods
    private void DrawBackground()
    {
        Rectangle rectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

        DrawBackgroundPrimitives(new Color(10, 0, 100), new Color(10, 0, 100));

        // sea bubbles
        var palette = Assets.Assets.Textures.Palettes.FirmamentSea.NightBackground2.Value;
        Vector3[] sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 0.7f), new Vector3(0.05f, 0.05f, 1), new Vector3(-0.03f, 0.02f, 0.85f)];
        Effect bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.15f, 1f, 0.5f, 0.35f, 0.45f);
        Color color = Color.White;
        color.A = 40;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, PixelRenderer.GetPixelationMatrix());
        Main.spriteBatch.Draw(SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        palette = Assets.Assets.Textures.Palettes.FirmamentSea.NightBackground1.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 1.35f), new Vector3(-0.03f, 0.02f, 1.1f)];
        bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.2f, 0.7f, 0.35f, 0.4f, 0.65f);
        color.A = 100;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, PixelRenderer.GetPixelationMatrix());
        Main.spriteBatch.Draw(SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        palette = Assets.Assets.Textures.Palettes.FirmamentSea.NightBackground0.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 2), new Vector3(0.03f, 0.02f, 2.2f)];
        bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.3f, 1f, 0f, 0.4f, 0.85f);
        color.A = 180;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, PixelRenderer.GetPixelationMatrix());
        Main.spriteBatch.Draw(SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();
    }

    private Effect GetBackgroundBubbleShader(Texture2D palette, Vector3[] sampleOffsetsAndScales, float speed, float gradientLength, float gradientStart, float cutoff, float parallax)
    {
        var firmamentSeaBackgroundShader = Assets.Assets.Shaders.FirmamentSeaBubbles.Value;

        firmamentSeaBackgroundShader.Parameters["bubbles"].SetValue(Assets.Assets.Textures.Noise.Bubble.Value);
        firmamentSeaBackgroundShader.Parameters["distortion"].SetValue(Assets.Assets.Textures.Noise.Perlin.Value);
        firmamentSeaBackgroundShader.Parameters["palette"].SetValue(palette);
        firmamentSeaBackgroundShader.Parameters["sampleOffsetsAndScales"].SetValue(sampleOffsetsAndScales);
        firmamentSeaBackgroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        firmamentSeaBackgroundShader.Parameters["screenWorldPosition"].SetValue(WorldToSeaCoordinates(Main.screenPosition) * 0.5f); // this is being halved because its being pixelated
        firmamentSeaBackgroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly * speed);
        firmamentSeaBackgroundShader.Parameters["gradientLength"].SetValue(gradientLength);
        firmamentSeaBackgroundShader.Parameters["gradientStart"].SetValue(gradientStart);
        firmamentSeaBackgroundShader.Parameters["cutoff"].SetValue(cutoff);
        firmamentSeaBackgroundShader.Parameters["parallax"].SetValue(parallax);

        return firmamentSeaBackgroundShader;
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        var v = Vector2.Normalize(begin - end);
        var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(Assets.Assets.Textures.WhitePixel.Value, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
    #endregion
}
