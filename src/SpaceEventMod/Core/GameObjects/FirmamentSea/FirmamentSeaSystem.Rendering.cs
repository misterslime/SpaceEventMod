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
    public static RenderTarget2D BackgroundRenderTarget;

    private static void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (Main.gameMenu)
        {
            orig();
            return;
        }

        if (BackgroundRenderTarget == null || BackgroundRenderTarget.Width != Main.screenWidth || BackgroundRenderTarget.Height != Main.screenHeight)
        {
            BackgroundRenderTarget?.Dispose();
            BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }

        Main.graphics.GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Black);

        if (Sea.Springs is not null)
        {
            float GetNormalHeight(float height)
            {
                if (height > 0 && height < 1)
                    return 1;
                else if (height <= 0 && height > -1)
                    return 1;

                return MathF.Abs(height);
            }

            var darkestGray = new Color(10, 10, 10);

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
                        var difference = next.Value.Position - Sea.Springs[chunk][spring].Position;

                        var waveOffset = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * nodeLocation));
                        var waveOffset2 = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * (nodeLocation + 1)));

                        var begin = Sea.Position + new Vector2(Sea.NodeWidth * nodeLocation, Sea.Springs[chunk][spring].Position + waveOffset) - Main.screenPosition;
                        var end = Sea.Position + new Vector2(Sea.NodeWidth * (nodeLocation + 1), next.Value.Position + waveOffset2) - Main.screenPosition;

                        var point1 = begin;
                        var point2 = new Vector2(begin.X, 0f);
                        var point3 = new Vector2(end.X, 0f);
                        var point4 = end;

                        // red is used to show how high up in the sea the pixel is
                        // blue to tell if the pixel is in the sea at all
                        SpaceEventMod.PrimitiveBatch
                            .AddVertex(point1, Color.Magenta)
                            .AddVertex(point2, Color.Blue)
                            .AddVertex(point3, Color.Blue)
                            .AddVertex(point4, Color.Magenta)
                            .AddVertex(point1, Color.Magenta)
                            .AddVertex(point3, Color.Blue);
                    }
                }
            }

            SpaceEventMod.PrimitiveBatch.End();
        }

        Main.instance.GraphicsDevice.SetRenderTarget(null);

        orig();
    }

    public void DrawSea(On_Main.orig_DrawDust orig, Main self)
    {
        if (BackgroundRenderTarget == null || Sea.Springs is null)
            return;

        var firmamentSeaShader = Assets.Assets.Shaders.FirmamentSea.Value;

        firmamentSeaShader.Parameters["noise"].SetValue(Assets.Assets.Textures.Extra.Noise.Foam.Value);
        firmamentSeaShader.Parameters["palette"].SetValue(Assets.Assets.Textures.Palettes.FirmamentSeaBackgroundPalette.Value);
        firmamentSeaShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        firmamentSeaShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        firmamentSeaShader.Parameters["screenWorldPosition"].SetValue(Main.screenPosition * 0.5f); // this is being halved because its being pixelated

        PixelRenderer.Draw(firmamentSeaShader, (spriteBatch) =>
        {
            spriteBatch.Draw(BackgroundRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        });

        // debug chunk boundaries
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (int chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            var seaScreenPosition = Sea.Position - Main.screenPosition;
            var chunkBeginPosition = seaScreenPosition + new Vector2(Sea.NodeWidth * chunk * Sea.ChunkSize, 0f);
            var chunkEndPosition = seaScreenPosition + new Vector2(Sea.NodeWidth * (chunk + 1) * Sea.ChunkSize, 0f);

            DrawLine(Main.spriteBatch, chunkBeginPosition, chunkEndPosition, Color.Yellow, 2);
            DrawLine(Main.spriteBatch, chunkBeginPosition, new Vector2(chunkBeginPosition.X, 0f), Color.Yellow, 2);
            DrawLine(Main.spriteBatch, chunkEndPosition, new Vector2(chunkEndPosition.X, 0f), Color.Yellow, 2);
        }

        Main.spriteBatch.End();

        orig(self);
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        var v = Vector2.Normalize(begin - end);
        var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(SpaceEventMod.WhitePixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}
