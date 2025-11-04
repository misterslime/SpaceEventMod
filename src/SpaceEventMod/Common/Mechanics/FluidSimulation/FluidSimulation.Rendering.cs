using ComputeSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;
using SpaceEventMod.Content.Events.Space.Rendering;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

using GraphicsDevice = ComputeSharp.GraphicsDevice;
using Vector2 = System.Numerics.Vector2;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation;

internal partial class FluidSimulation : ModSystem
{
    private static ReadWriteTexture2D<Bgra32, float4> _computeTexture;
    private static Texture2D _fluidTexture;
    private static MemoryStream _stream;

    public override void PostDrawTiles()
    {
        if (!Active)
            return;

        Vector2 middle = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) * 0.5f;

        Vector4 blue = Color.Blue.ToVector4();
        Vector4 yellow = Color.Yellow.ToVector4();

        float4 rest = new Float4(blue.X, blue.Y, blue.Z, blue.W);
        float4 moving = new float4(yellow.X, yellow.Y, yellow.Z, yellow.W);

        if (_stream == null)
        {
            _stream = new MemoryStream(5000);
        }

        if (_computeTexture == null)
        {
            _computeTexture = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<Bgra32, float4>(Main.screenWidth / 2, Main.screenHeight / 2);
        }

        if (_fluidTexture != null)
        {
            _fluidTexture.Dispose();
        }

        GraphicsDevice.GetDefault().For(s_numParticles, new DrawToTexture(positionsBuffer, velocityBuffer, _computeTexture, rest, moving, (float2)middle));

        _computeTexture.Save(_stream, ImageFormat.Png);

        _fluidTexture = Texture2D.FromStream(Main.spriteBatch.GraphicsDevice, _stream);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Microsoft.Xna.Framework.Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(118, 129, 247), 0f, Microsoft.Xna.Framework.Vector2.Zero, 2f, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        GraphicsDevice.GetDefault().ForEach(_computeTexture, new ClearFrame());

        Reset(_stream);
    }

    public static void Reset(MemoryStream source)
    {
        source.Position = 0;
        source.SetLength(0);
    }
}