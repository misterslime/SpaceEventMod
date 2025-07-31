using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
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

        if (firmamentSea.Nodes is not null)
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

            for (var i = 0; i < firmamentSea.Nodes.Length - 1; i++)
            {
                var difference = firmamentSea.Nodes[i + 1].Height - firmamentSea.Nodes[i].Height;

                var waveOffset = firmamentSea.OverlapSines((float)(firmamentSea.NodeWidth * i)) / GetNormalHeight(difference * MathF.PI);
                var waveOffset2 = firmamentSea.OverlapSines((float)(firmamentSea.NodeWidth * (i + 1))) / GetNormalHeight(difference * MathF.PI);

                var begin = firmamentSea.Position + new Vector2(firmamentSea.NodeWidth * i, firmamentSea.Nodes[i].Height) - Main.screenPosition;
                var end = firmamentSea.Position + new Vector2(firmamentSea.NodeWidth * (i + 1), firmamentSea.Nodes[i + 1].Height) - Main.screenPosition;

                begin *= 0.5f;
                begin = new Vector2(MathF.Floor(begin.X), MathF.Floor(begin.Y));
                begin *= 2f;

                end *= 0.5f;
                end = new Vector2(MathF.Floor(end.X), MathF.Floor(end.Y));
                end *= 2f;

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

            SpaceEventMod.PrimitiveBatch.End();
        }

        Main.instance.GraphicsDevice.SetRenderTarget(null);

        orig();
    }

    public void DrawSea(On_Main.orig_DrawDust orig, Main self)
    {
        if (BackgroundRenderTarget == null || firmamentSea.Nodes is null)
            return;

        var inkStencilShader = Assets.Assets.Shaders.FirmamentSea.Value;

        inkStencilShader.Parameters["noise"].SetValue(Assets.Assets.Textures.Extra.Noise.Foam.Value);
        inkStencilShader.Parameters["palette"].SetValue(Assets.Assets.Textures.Palettes.FirmamentSeaBackgroundPalette.Value);
        inkStencilShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        inkStencilShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        inkStencilShader.Parameters["screenWorldPosition"].SetValue(Main.screenPosition * 0.5f); // this is being halved because its being pixelated

        PixelRenderer.Draw(inkStencilShader, (spriteBatch) =>
        {
            spriteBatch.Draw(BackgroundRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        });

        orig(self);
    }
}
