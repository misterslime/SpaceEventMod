using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceEventMod.Core.Graphics;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal unsafe sealed class AmoerphaMetaballRenderer : ModSystem
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Metaball
    {
        [FieldOffset(0)] public Vector2 Position;
        [FieldOffset(8)] public float Radius;
        [FieldOffset(12)] public Vector2 Velocity;
        [FieldOffset(20)] public float TimeLeft;
        [FieldOffset(24)] public float MaxTime;
        [FieldOffset(28)] public float InitialRadius;
    }

    private static Metaball[] _metaballs;
    private static int _activeMetaballCount;

    private static Vector4[] _metaballData;
    private const int max_metaballs = 64;

    private static Vector2[] _vertexData;
    private static int _vertices;

    private static RenderTarget2D _screenBuffer;

    public override void Load()
    {
        _metaballs = new Metaball[max_metaballs];
        _metaballData = new Vector4[max_metaballs];
        _vertexData = new Vector2[max_metaballs];

        _activeMetaballCount = 0;
        _vertices = 0;

        Main.QueueMainThreadAction(() =>
        {
            Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            Main.graphics.ApplyChanges();

            _screenBuffer = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

            Main.OnResolutionChanged += ReinitTargets;
        });
    }

    public override void Unload()
    {
        Main.OnResolutionChanged -= ReinitTargets;
    }

    static void ReinitTargets(Vector2 size)
    {
        _screenBuffer = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X, (int)size.Y);
    }

    public override void PostUpdateEverything()
    {
        if (!Main.hasFocus) return;

        var dt = 1f / 60f;

        var balls = _metaballs.AsSpan(0, _activeMetaballCount);

        for (var i = balls.Length - 1; i >= 0; i--)
        {
            ref var ball = ref balls[i];

            ball.TimeLeft -= dt;

            if (ball.TimeLeft <= 0)
            {
                ball = balls[_activeMetaballCount - 1];
                _activeMetaballCount--;
                continue;
            }

            ball.Position += ball.Velocity * dt;
            var lifeProgress = ball.TimeLeft / ball.MaxTime;
            ball.Radius = ball.InitialRadius * lifeProgress;
        }
    }

    public override void PostDrawTiles()
    {
        if (_activeMetaballCount == 0 && _vertices == 0)
        {
            AmoerphaScreenShaderManager.Deactivate();
            return;
        }

        var sb = Main.spriteBatch;
        var gd = Main.instance.GraphicsDevice;
        var effect = Assets.Assets.Shaders.NPCs.AmoerphaMetaballs.Value;
        ApplyToBindings(gd.GetRenderTargets());
        var rts = gd.GetRenderTargets();
        ApplyToBindings(rts);

        gd.SetRenderTarget(_screenBuffer);
        gd.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);

        var src = _metaballs.AsSpan(0, _activeMetaballCount);
        var dst = _metaballData.AsSpan(0, _activeMetaballCount);

        fixed (Metaball* sourcePtr = src)
        fixed (Vector4* destPtr = dst)
        {
            for (var i = 0; i < _activeMetaballCount; i++)
            {
                var sourceBall = sourcePtr + i;
                var destVec = destPtr + i;

                destVec->X = sourceBall->Position.X;
                destVec->Y = sourceBall->Position.Y;
                destVec->Z = sourceBall->Radius;
                destVec->W = 0.0f;
            }
        }

        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight);
        var correctScreenTopLeft = screenCenter - worldViewDimensions / 2f;

        effect.Parameters["metaballData"].SetValue(_metaballData);
        effect.Parameters["metaballCount"].SetValue(_activeMetaballCount);
        effect.Parameters["smoothness"].SetValue(1.75f);
        effect.Parameters["screenPos"].SetValue(correctScreenTopLeft);
        effect.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);

        /*effect.Parameters["vertexData"].SetValue(_vertexData);
        effect.Parameters["vertexCount"].SetValue(_vertices);
        effect.Parameters["smoothness"].SetValue(1.75f);
        effect.Parameters["radius"].SetValue(32f);
        effect.Parameters["roundness"].SetValue(64f);
        effect.Parameters["screenPos"].SetValue(correctScreenTopLeft);
        effect.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);*/

        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        sb.End();

        gd.SetRenderTargets(rts);

        AmoerphaScreenShaderManager.Update(in _screenBuffer);
    }

    public static void AddVertexData(Vector2[] vertices, int numVertices)
    {
        _vertexData = vertices;
        _vertices = numVertices;
    }

    public static void New(Vector2 pos, float radius, float lifetime, Vector2 velocity = default)
    {
        if (_activeMetaballCount < max_metaballs)
        {
            ref var newBall = ref _metaballs[_activeMetaballCount];
            newBall.Position = pos;
            newBall.Radius = radius;
            newBall.Velocity = velocity;
            newBall.InitialRadius = radius;
            newBall.MaxTime = lifetime;
            newBall.TimeLeft = lifetime;

            _activeMetaballCount++;
        }
    }

    public static void ApplyToBindings(RenderTargetBinding[] bindings)
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
}
