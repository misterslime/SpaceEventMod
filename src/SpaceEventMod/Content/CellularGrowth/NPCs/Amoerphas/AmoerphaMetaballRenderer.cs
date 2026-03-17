using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceEventMod.Assets;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Amoerphas;

internal unsafe sealed class AmoerphaMetaballRenderer : ModSystem
{

    private const int MAX_LINES = 64;

    private static Vector3[] _pointA;
    private static Vector2[] _pointB;
    private static int _activeLineCount;

    private static RenderTarget2D _screenBuffer;

    public override void Load()
    {
        _pointA = new Vector3[MAX_LINES];
        _pointB = new Vector2[MAX_LINES];

        _activeLineCount = 0;

        Main.QueueMainThreadAction(() =>
        {
            _screenBuffer = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);

            Main.OnResolutionChanged += ReinitTargets;
        });
    }

    public override void Unload()
    {
        Main.OnResolutionChanged -= ReinitTargets;
    }

    static void ReinitTargets(Vector2 size)
    {
        _screenBuffer = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X / 2, (int)size.Y / 2);
    }

    public override void PostDrawTiles()
    {
        Main.NewText(_activeLineCount);

        if (_activeLineCount == 0) return;

        var sb = Main.spriteBatch;
        var gd = Main.instance.GraphicsDevice;
        var effect = Assets.Assets.Shaders.NPCs.AmoebaSDFs.Value;
        ApplyToBindings(gd.GetRenderTargets());
        var rts = gd.GetRenderTargets();
        ApplyToBindings(rts);

        gd.SetRenderTarget(_screenBuffer);
        gd.Clear(Color.Transparent);

        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight);
        var correctScreenTopLeft = screenCenter - worldViewDimensions / 2f;

        effect.Parameters["noiseTexture"].SetValue(Assets.Assets.Textures.Noise.Bubble.Value);
        effect.Parameters["normalTexture"].SetValue(Assets.Assets.Textures.Noise.BubbleNormal.Value);
        effect.Parameters["aData"].SetValue(_pointA);
        effect.Parameters["bData"].SetValue(_pointB);
        effect.Parameters["lineCount"].SetValue(_activeLineCount);
        effect.Parameters["smoothness"].SetValue(0.20f);
        effect.Parameters["screenPos"].SetValue(correctScreenTopLeft);
        effect.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);
        effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
        effect.Parameters["zoom"].SetValue(100f);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight/ 2), Color.White);
        sb.End();

        gd.SetRenderTargets(rts);

        Graphics.BeginPipeline(0.5f)
            .DrawSprite(_screenBuffer, Vector2.Zero, Color.White, scale: Vector2.One * 2f)
            .ApplyOutline(new Color(0, 0, 64))
            .Flush();

        _activeLineCount = 0;
    }

    private static void ApplyToBindings(RenderTargetBinding[] bindings)
    {
        foreach (var binding in bindings)
        {
            if (binding.RenderTarget is not RenderTarget2D rt)
            {
                continue;
            }

            rt.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }

    public static void New(Line line, float radius)
    {
        New(line.Point1, line.Point2, radius);
    }

    public static void New(Vector2 point1, Vector2 point2, float radius)
    {
        if (_activeLineCount < MAX_LINES)
        {
            ref var pointA = ref _pointA[_activeLineCount];
            ref var pointB = ref _pointB[_activeLineCount];
            pointA.X = point1.X;
            pointA.Y = point1.Y;
            pointA.Z = radius;
            pointB = point2;

            _activeLineCount++;
        }
    }
}
