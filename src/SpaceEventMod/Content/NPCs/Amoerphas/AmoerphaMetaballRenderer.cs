using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal unsafe sealed class AmoerphaMetaballRenderer : ModSystem
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Metaball
    {
        [FieldOffset(0)] public Vector2 Position;
        [FieldOffset(8)] public float Radius;
    }

    private struct MetaballSet(Metaball[] balls, int count)
    {
        public Metaball[] Balls = balls;
        public int Count = count;
    }

    private const int MAX_METABALLS = 64;

    private static Stack<MetaballSet> _metaballs;

    private static Vector4[] _metaballData;
    private static int _activeMetaballCount;

    private static RenderTarget2D _outlineTarget;
    private static RenderTarget2D _colorTarget;
    private static RenderTarget2D _metaballBufferA;
    private static RenderTarget2D _metaballBufferB;

    public override void Load()
    {
        _metaballs = new Stack<MetaballSet>();
        _metaballData = new Vector4[MAX_METABALLS];

        Main.QueueMainThreadAction(() =>
        {
            Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            Main.graphics.ApplyChanges();

            _outlineTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            _colorTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            _metaballBufferA = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            _metaballBufferB = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);

            Main.OnResolutionChanged += ReinitTargets;
        });
    }

    public override void Unload()
    {
        Main.OnResolutionChanged -= ReinitTargets;
    }

    static void ReinitTargets(Vector2 size)
    {
        _outlineTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X / 2, (int)size.Y / 2);
        _colorTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X / 2, (int)size.Y / 2);
        _metaballBufferA = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X / 2, (int)size.Y / 2);
        _metaballBufferB = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X / 2, (int)size.Y / 2);
    }

    public override void PostDrawTiles()
    {
        if (_metaballs.Count == 0) return;

        var sb = Main.spriteBatch;
        var gd = Main.instance.GraphicsDevice;
        ApplyToBindings(gd.GetRenderTargets());
        var rts = gd.GetRenderTargets();
        ApplyToBindings(rts);

        gd.SetRenderTarget(_metaballBufferA);
        gd.Clear(Color.Transparent);

        DrawMetaballs(in sb, in gd, out RenderTarget2D sdfBuffer, out RenderTarget2D normalBuffer);

        gd.SetRenderTarget(normalBuffer);
        gd.Clear(Color.Transparent);

        Effect normal = Assets.Assets.Shaders.Metaballs.MetaballNormals.Value;
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, normal, Matrix.Identity);
        sb.Draw(sdfBuffer, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
        sb.End();

        gd.SetRenderTarget(_outlineTarget);
        gd.Clear(Color.Transparent);

        DrawOutline(in sb, in sdfBuffer, in normalBuffer);

        gd.SetRenderTarget(_colorTarget);
        gd.Clear(Color.Black);

        DrawFractalNoise(in sb);

        gd.SetRenderTarget(sdfBuffer);
        gd.Clear(Color.Transparent);

        /*DrawLighting(in sb, in normalBuffer);

        Graphics.BeginPipeline(0.5f)
            .DrawSprite(sdfBuffer, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White)
            .Schedule(RenderLayer.AfterPlayers);*/

        DrawBody(in sb, in normalBuffer);

        gd.SetRenderTarget(_colorTarget);
        gd.Clear(Color.Transparent);

        Vector3 lightDirection = (Main.MouseWorld - Main.LocalPlayer.Center).ToVector3(64f);

        lightDirection = new Vector3(-0.4f, -0.7f, 0.4f);
        lightDirection.Normalize();

        Color shadow = Color.Black;

        shadow.A = 127;


        var effect = Assets.Assets.Shaders.NPCs.AmoebaLighting.Value;

        effect.Parameters["incomingLight"].SetValue(lightDirection);
        effect.Parameters["shininess"].SetValue(16f);
        effect.Parameters["shadowColor"].SetValue(shadow.ToVector4());
        effect.Parameters["shadowThreshold"].SetValue(0.1f);
        effect.Parameters["pixelation"].SetValue(0.5f);
        effect.Parameters["bodyTarget"].SetValue(sdfBuffer);
        effect.Parameters["outlineTarget"].SetValue(_outlineTarget);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
        sb.Draw(normalBuffer, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
        sb.End();

        gd.SetRenderTargets(rts);

        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight);
        var correctScreenTopLeft = screenCenter - worldViewDimensions / 2f;

        Graphics.BeginPipeline(1f)
            .DrawSprite(_colorTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White)
            .ApplyEffect(
                Assets.Assets.Shaders.NPCs.AmoebaCore.Value,
                ("flowmap", Assets.Assets.Textures.Noise.Flowmap.Value),
                ("flowDisplacement", Main.GlobalTimeWrappedHourly * 0.1f * Vector2.One),
                ("scale", 0.010f),
                ("strength", 0.004f),
                ("worldViewDimensions", new Vector2(Main.screenWidth, Main.screenHeight)),
                ("screenPosition", Main.screenPosition))
            .Schedule(RenderLayer.AfterPlayers);
    }

    private void DrawMetaballs(in SpriteBatch sb, in GraphicsDevice gd, out RenderTarget2D sdfBuffer, out RenderTarget2D normalBuffer)
    {
        DrawMetaballs(in sb, _metaballs.Pop());

        bool useB = true;
        sdfBuffer = _metaballBufferA;
        normalBuffer = _metaballBufferB;

        while (_metaballs.Count > 0)
        {
            MetaballSet chunk = _metaballs.Pop();

            var sdfTemp = sdfBuffer;
            var normalTemp = normalBuffer;

            sdfBuffer = normalTemp;
            normalBuffer = sdfTemp;

            gd.SetRenderTarget(sdfBuffer);
            gd.Clear(Color.Transparent);
            DrawMetaballs(in sb, in normalBuffer, in chunk);

            useB = !useB;
        }

        //_metaballs.Clear();
    }

    private void DrawMetaballs(in SpriteBatch sb, in MetaballSet balls)
    {
        var effect = Assets.Assets.Shaders.Metaballs.FirstPassMetaballs.Value;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GetMetaballShader(effect, in balls), Matrix.Identity);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
        sb.End();
    }

    private void DrawMetaballs(in SpriteBatch sb, in RenderTarget2D buffer, in MetaballSet balls)
    {
        var effect = Assets.Assets.Shaders.Metaballs.TargetPassMetaballs.Value;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GetMetaballShader(effect, in balls), Matrix.Identity);
        sb.Draw(buffer, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
        sb.End();
    }

    private void DrawBody(in SpriteBatch sb, in RenderTarget2D normalTarget)
    {
        Effect glow = Assets.Assets.Shaders.NPCs.AmoebaBody.Value;

        glow.Parameters["noiseTarget"].SetValue(_colorTarget);
        glow.Parameters["pixelSize"].SetValue((Vector2.One) / (new Vector2(Main.screenWidth, Main.screenHeight)));
        glow.Parameters["displacement"].SetValue(50f);
        glow.Parameters["minAlpha"].SetValue(0.5f);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, glow, Matrix.Identity);
        sb.Draw(normalTarget, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.Blue);
        sb.End();
    }

    private void DrawOutline(in SpriteBatch sb, in RenderTarget2D glowTarget, in RenderTarget2D normalTarget)
    {
        Effect glow = Assets.Assets.Shaders.NPCs.AmoebaGlow.Value;

        glow.Parameters["dropoff"].SetValue(4f);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, glow, Matrix.Identity);
        sb.Draw(glowTarget, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.MidnightBlue);
        sb.End();
        return;
        Effect outline = Assets.Assets.Shaders.NPCs.AmoebaOutline.Value;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, outline, Matrix.Identity);
        sb.Draw(normalTarget, new Rectangle(-1, -1, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
        sb.Draw(normalTarget, new Rectangle(2, 2, Main.screenWidth / 2, Main.screenHeight / 2), Color.Black);
        sb.End();
    }

    private void DrawFractalNoise(in SpriteBatch sb)
    {
        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight);
        var correctScreenTopLeft = screenCenter - worldViewDimensions / 2;

        var fractalNoise = Assets.Assets.Shaders.Noise.FractalNoise.Value;

        fractalNoise.Parameters["zoom"].SetValue(16f / Main.screenWidth);
        fractalNoise.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.5f);
        fractalNoise.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

        fractalNoise.Parameters["displacementA"].SetValue(Vector2.UnitY * 0.025f);
        fractalNoise.Parameters["displacementB"].SetValue(Vector2.UnitX * 0.05f);

        fractalNoise.Parameters["backgroundColor"].SetValue(Color.Black.ToVector4());
        fractalNoise.Parameters["lowColor"].SetValue(Color.MidnightBlue.ToVector4());
        fractalNoise.Parameters["middleColor"].SetValue(Color.BlueViolet.ToVector4());
        fractalNoise.Parameters["highColor"].SetValue(Color.Magenta.ToVector4());

        fractalNoise.Parameters["gradientPixelation"].SetValue(0.25f);
        fractalNoise.Parameters["backgroundThreshold"].SetValue(0f);
        fractalNoise.Parameters["lowColorThreshold"].SetValue(0.24f);
        fractalNoise.Parameters["midColorThreshold"].SetValue(0.48f);

        fractalNoise.Parameters["screenPos"].SetValue(correctScreenTopLeft);
        fractalNoise.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, fractalNoise, Matrix.Identity);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
        sb.End();

        Matrix matrix = Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateScale(0.5f / Main.GameViewMatrix.Zoom.X, 0.5f / Main.GameViewMatrix.Zoom.Y, 1f)
            * Matrix.CreateTranslation(Main.GameViewMatrix.Translation.X * 0.5f, Main.GameViewMatrix.Translation.Y * 0.5f, 0f);

        var distortion = Assets.Assets.Shaders.NPCs.AmoebaCore.Value;

        distortion.Parameters["flowmap"].SetValue(Assets.Assets.Textures.Noise.Flowmap.Value);
        distortion.Parameters["flowDisplacement"].SetValue(Main.GlobalTimeWrappedHourly * 0.1f * Vector2.One);
        distortion.Parameters["scale"].SetValue(0.2f);
        distortion.Parameters["strength"].SetValue(0.1f);

        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);

        foreach (NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<Amoerpha>()))
        {
            var texture = Assets.Assets.Textures.NPCs.Amoerphas.AmoebaCenter.Value;


            Rectangle frame = texture.Frame(1, 8, 0, 0);
            Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;

            float rotation = MathF.Sin((Main.GameUpdateCount + npc.whoAmI) / 160f) * (MathF.PI / 180f) * 10f;

            Vector2 displacement = Vector2.Zero;
            displacement.Y += MathF.Sin((Main.GameUpdateCount + npc.whoAmI) / 40f) * 8f;

            sb.Draw(texture, npc.Center - Main.screenPosition + displacement, null, Color.White, rotation, origin, 1f, 0f, 0);
            //sb.End();
        }
        sb.End();
    }

    private Effect GetMetaballShader(Effect effect, in MetaballSet balls)
    {
        int count = balls.Balls.Length;

        var src = balls.Balls.AsSpan(0, count);
        var dst = _metaballData.AsSpan(0, MAX_METABALLS);

        fixed (Metaball* sourcePtr = src)
        fixed (Vector4* destPtr = dst)
        {
            for (var i = 0; i < count; i++)
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
        effect.Parameters["metaballCount"].SetValue(count);
        effect.Parameters["smoothness"].SetValue(0.35f);
        effect.Parameters["screenPos"].SetValue(correctScreenTopLeft);
        effect.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);

        return effect;
    }
    
    public static void AddMetaballData(in Vector2[] positions, float radius, float scale)
    {
        var sets = positions.Chunk(MAX_METABALLS);

        foreach( var set in sets )
        {
            Metaball[] metaballs = new Metaball[set.Length];

            for (int i = 0; i < set.Length; i++)
            {
                Metaball metaball = new Metaball();

                metaball.Position = positions[i] * scale;
                metaball.Radius = radius;

                metaballs[i] = metaball;
            }

            _metaballs.Push(new MetaballSet(metaballs, set.Length));
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
