using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space;

internal class SpaceEventFogShaderData : ScreenShaderData
{
    private static Filter _myFilter;

    public SpaceEventFogShaderData(Asset<Effect> shader, string passName)
            : base(shader, passName)
    {
    }

    [OnLoad]
    private static void Load()
    {
        var shader = Assets.Shaders.Space.SeaDistortFog.Asset;

        _myFilter = new Filter(new SpaceEventFogShaderData(shader, "Pass0")
            .UseImage(Assets.Textures.Noise.SwirlyDisplaceNoise.Asset, 0, SamplerState.LinearWrap), EffectPriority.VeryHigh);

        Filters.Scene["SeaDistortFog"] = _myFilter;
        Filters.Scene["SeaDistortFog"].Load();
    }

    [ModSystemHooks.PostUpdateEverything]
    private static void UpdateShaderParameters()
    {
        if (_myFilter is null || SpaceEvent.SeaMeshBuffer is null)
            return;

        Filters.Scene["SeaDistortFog"]._shader.UseImage(SpaceEvent.SeaMeshBuffer.Target, 1, SamplerState.LinearWrap);
    }

    public override void Apply()
    {
        // base.Shader.Parameters["fogColor"]?.SetValue(new Vector4(0.0f, 0.25f, 1.0f, 0.25f));
        base.Shader.Parameters["fogColor"]?.SetValue(new Vector4(0.0f, 0.25f, 1.0f, 0.35f));
        base.Shader.Parameters["fogStart"]?.SetValue(0.15f);
        base.Shader.Parameters["fogEnd"]?.SetValue(0.65f);
        base.Shader.Parameters["distortIntensity"]?.SetValue(0.07f);
        base.Shader.Parameters["distortNoiseScale"]?.SetValue(0.001f);
        base.Shader.Parameters["timeScale"]?.SetValue(0.02f);
        base.Shader.Parameters["blurMulti"]?.SetValue(0.0005f);

        base.Apply();
    }
}

internal static partial class SpaceEvent
{
    private sealed class BufferData : IStatic<BufferData>
    {
        public required WrapperShaderData<Assets.Shaders.Space.FirmamentSeaBubbles.Parameters> BubbleShader { get; init; }

        public required WrapperShaderData<Assets.Shaders.Space.FirmamentSeaFoam.Parameters> FoamShader { get; init; }

        public required RenderTargetLease SeaMeshBuffer { get; init; }

        public required RenderTargetLease BackgroundBuffer { get; init; }

        public static BufferData LoadData(Mod mod)
        {
            return Main.RunOnMainThread(
                () => new BufferData
                {
                    BubbleShader = Assets.Shaders.Space.FirmamentSeaBubbles.CreatePass0(),
                    FoamShader = Assets.Shaders.Space.FirmamentSeaFoam.CreatePass0(),
                    SeaMeshBuffer = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice),
                    BackgroundBuffer = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice),
                }
            ).GetAwaiter().GetResult();
        }

        public static void UnloadData(BufferData data)
        {
            Main.RunOnMainThread(
                () =>
                {
                    data.SeaMeshBuffer.Dispose();
                    data.BackgroundBuffer.Dispose();
                }
            );
        }
    }

    public static RenderTargetLease SeaMeshBuffer => BufferData.Instance.SeaMeshBuffer;

    private static RenderTargetLease BackgroundBuffer => BufferData.Instance.BackgroundBuffer;


    [OnLoad(Side = ModSide.Client)]
    private static void LoadTargetHook()
    {
        On_Main.DoDraw_UpdateCameraPosition += RenderBuffers;
        On_Main.DoDraw_WallsTilesNPCs += DrawSeaBackground;
        On_Main.DrawInfernoRings += DrawSeaForeground;
        On_Main.DrawInfernoRings += DrawRipples;
    }

    #region Buffer Rendering
    private static void RenderBuffers(On_Main.orig_DoDraw_UpdateCameraPosition orig)
    {
        if (!Main.gameMenu && !Main.dedServ)
        {
            using (SeaMeshBuffer.Target.Scope(clearColor: Color.Transparent)) 
            {
                Pipeline pipeline = Graphics.BeginPipeline();

                DrawBackgroundPrimitives(in pipeline, Color.Blue, Color.Magenta);
                DrawForegroundPrimitives(in pipeline);

                pipeline.Flush();
            }

            using (BackgroundBuffer.Target.Scope(clearColor: Color.Transparent))
            {
                Pipeline pipeline = Graphics.BeginPipeline();

                DrawBackgroundPrimitives(in pipeline, new Color(10, 0, 100), new Color(10, 0, 100));

                pipeline.Flush();

                DrawBackground(Main.spriteBatch);
            }
        }

        orig();
    }

    private static void DrawBackground(SpriteBatch spriteBatch)
    {
        /*var rectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

        // sea bubbles
        Matrix pixelationMatrix = Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateScale(0.5f / Main.GameViewMatrix.Zoom.X, 0.5f / Main.GameViewMatrix.Zoom.Y, 1f)
            * Matrix.CreateTranslation(Main.GameViewMatrix.Translation.X * 0.5f, Main.GameViewMatrix.Translation.Y * 0.5f, 0f);*/

        using var _ = spriteBatch.Scope();

        var palette = Assets.Textures.Space.Palettes.FirmamentSea.NightBackground2.Asset.Value;
        Vector3[] sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 0.7f), new Vector3(0.05f, 0.05f, 1), new Vector3(-0.03f, 0.02f, 0.85f)];
        DrawBubbles(spriteBatch, palette, sampleOffsetsAndScales, 0.15f, 1f, 0.5f, 0.35f, 0.3f);

        palette = Assets.Textures.Space.Palettes.FirmamentSea.NightBackground1.Asset.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 1.35f), new Vector3(-0.03f, 0.02f, 1.1f)];
        DrawBubbles(spriteBatch, palette, sampleOffsetsAndScales, 0.2f, 0.7f, 0.35f, 0.4f, 0.5f);

        palette = Assets.Textures.Space.Palettes.FirmamentSea.NightBackground0.Asset.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 2), new Vector3(0.03f, 0.02f, 2.2f)];
        DrawBubbles(spriteBatch, palette, sampleOffsetsAndScales, 0.3f, 1f, 0f, 0.4f, 0.85f);
    }

    private static void DrawBubbles(SpriteBatch spriteBatch, Texture2D palette, Vector3[] sampleOffsetsAndScales, float speed, float gradientLength, float gradientStart, float cutoff, float parallax)
    {
        var bubbleShader = BufferData.Instance.BubbleShader;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer);

        bubbleShader.Parameters.bubbles = Assets.Textures.Noise.Bubble.Asset.Value;
        bubbleShader.Parameters.distortion = Assets.Textures.Noise.Perlin.Asset.Value;
        bubbleShader.Parameters.palette = palette;
        bubbleShader.Parameters.sampleOffsetsAndScales = sampleOffsetsAndScales;
        bubbleShader.Parameters.screenSize = new Vector2(Main.screenWidth, Main.screenHeight) * 1.75f;
        bubbleShader.Parameters.screenWorldPosition = SpaceEvent.WorldToSeaCoordinates(Main.screenPosition);
        bubbleShader.Parameters.globalTime = Main.GlobalTimeWrappedHourly * speed;
        bubbleShader.Parameters.gradientLength = gradientLength;
        bubbleShader.Parameters.gradientStart = gradientStart;
        bubbleShader.Parameters.cutoff = cutoff;
        bubbleShader.Parameters.parallax = parallax;

        bubbleShader.Apply();

        spriteBatch.Draw(SeaMeshBuffer.Target, Vector2.Zero, Color.White);

        spriteBatch.End();
    }
    #endregion

    #region Sea Mesh
    public static void DrawBackgroundPrimitives(in Pipeline pipeline, Color top, Color bottom)
    {
        if (Sea.Springs is null || !Sea.Active)
            return;

        for (var chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            Mesh seaChunk = new Mesh(PrimitiveType.TriangleList);

            for (var spring = 0; spring < Sea.Springs[chunk].Length; spring++)
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
                    seaChunk
                        .AddVertex(point1, bottom)
                        .AddVertex(point2, top)
                        .AddVertex(point3, top)
                        .AddVertex(point4, bottom)
                        .AddVertex(point1, bottom)
                        .AddVertex(point3, top);
                }
            }

            pipeline.DrawMesh(seaChunk, SpaceEventMod.basicEffect);
        }
    }

    private static void DrawForegroundPrimitives(in Pipeline pipeline)
    {
        if (Sea.Springs is null || !Sea.Active)
            return;

        for (var chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            Mesh seaChunk = new Mesh(PrimitiveType.TriangleList);

            for (var spring = 0; spring < Sea.Springs[chunk].Length; spring++)
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
                    seaChunk
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point2, Color.Transparent)
                        .AddVertex(point3, Color.Transparent)
                        .AddVertex(point4, new Color(0, 255, 0, 0))
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point3, Color.Transparent);
                }
            }

            pipeline.DrawMesh(seaChunk, SpaceEventMod.basicEffect);
        }
    }
    #endregion

    #region Background and Foreground
    private static void DrawSeaBackground(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
    {
        if (BackgroundBuffer.Target is not null && Sea.Springs is not null && Sea.Active)
        {
            Graphics.BeginPipeline(0.5f)
                .DrawSprite(
                    BackgroundBuffer.Target,
                    Vector2.Zero,
                    Color.White,
                    BackgroundBuffer.Target.Bounds,
                    0f,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None)
                .ApplyEffect(
                    Assets.Shaders.Space.FirmamentSeaBackgroundTransparency.Asset.Value,
                    ("sea", SeaMeshBuffer.Target),
                    ("minimumAlpha", 0.8f))
                .Flush();
        }

        orig(self);
    }

    private static void DrawSeaForeground(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        if (SeaMeshBuffer.Target is not null && Sea.Springs is not null && Sea.Active)
        {
            // round position to prevent artifacts
            var screenPosition = SpaceEvent.WorldToSeaCoordinates(Main.screenPosition);

            screenPosition.X = MathF.Floor(screenPosition.X * 0.5f);
            screenPosition.X *= 2f;

            screenPosition.Y = MathF.Floor(screenPosition.Y * 0.5f);
            screenPosition.Y *= 2f;

            var color1 = new Color(118, 129, 247);
            var color2 = new Color(169, 201, 234);

            var sb = Main.spriteBatch;

            using (var _ = sb.Scope())
            {
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);

                var foamShader = BufferData.Instance.FoamShader;

                /*firmamentSeaForegroundShader.Parameters["noise"].SetValue(Assets.Textures.Noise.Foam.Asset.Value);
                firmamentSeaForegroundShader.Parameters["palette"].SetValue(Assets.Textures.Space.Palettes.FirmamentSea.NightForeground.Asset.Value);
                firmamentSeaForegroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
                firmamentSeaForegroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                firmamentSeaForegroundShader.Parameters["screenWorldPosition"].SetValue(screenPosition);

                firmamentSeaForegroundShader.Parameters["edgeColor1"].SetValue(color1.ToVector4());
                firmamentSeaForegroundShader.Parameters["edgeColor2"].SetValue(color2.ToVector4());

                firmamentSeaForegroundShader.CurrentTechnique.Passes[0].Apply();*/

                foamShader.Parameters.noise = Assets.Textures.Noise.Foam.Asset.Value;
                foamShader.Parameters.palette = Assets.Textures.Space.Palettes.FirmamentSea.NightForeground.Asset.Value;
                foamShader.Parameters.globalTime = Main.GlobalTimeWrappedHourly;
                foamShader.Parameters.screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                foamShader.Parameters.screenWorldPosition = screenPosition;

                foamShader.Parameters.edgeColor1 = color1.ToVector4();
                foamShader.Parameters.edgeColor2 = color2.ToVector4();

                foamShader.Apply();

                sb.Draw(SeaMeshBuffer.Target, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(118, 129, 247), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                sb.End();
            }

        }

        orig(self);
    }
    #endregion

    #region Tile Ripples
    private static void DrawRipples(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        /*if (Main.wallTarget is null || Main.tileTarget is null || Main.tile2Target is null)
        {
            orig(self);
            return;
        }

        using var rippleTarget = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (width, height) => (width / 2, height / 2));
        using var rippleTarget2 = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (width, height) => (width / 2, height / 2));

        using (rippleTarget.Target.Scope(clearColor: Color.Transparent))
        {
            using var sbScope = Main.spriteBatch.Scope();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var changeColor = Assets.Shaders.Fragment.ChangeColor.CreateAwesomePass();

            changeColor.Parameters.color = Color.Red.ToVector4();
            changeColor.Apply();

            Main.spriteBatch.Draw(Main.wallTarget, (Main.sceneWallPos - Main.screenPosition) * 0.5f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Main.tileTarget, (Main.sceneTilePos - Main.screenPosition) * 0.5f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Main.tile2Target, (Main.sceneTile2Pos - Main.screenPosition) * 0.5f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();

        }

        using (rippleTarget2.Target.Scope(clearColor: Color.Transparent))
        {
            using var sbScope = Main.spriteBatch.Scope();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var seaRipplesBlur = Assets.Shaders.Space.SeaRippleBlur.CreatePass0();

            seaRipplesBlur.Parameters.blurRadius = 0.002f;
            seaRipplesBlur.Apply();

            Main.spriteBatch.Draw(rippleTarget.Target, Vector2.Zero, null, new Color(1, 1, 1, 0), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();

        }

        using (Main.spriteBatch.Scope())
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var seaRipples = Assets.Shaders.Space.SeaRipples.CreatePass0();

            seaRipples.Parameters.pixelSize = (Vector2.One * 2f) / rippleTarget2.Target.Size();
            seaRipples.Parameters.noise = Assets.Textures.Noise.Bubble.Asset.Value;
            seaRipples.Parameters.sea = Assets.Textures.WhitePixel.Asset.Value;
            seaRipples.Parameters.uTime = Main.GlobalTimeWrappedHourly * 0.06f;
            seaRipples.Parameters.uScale = 1f;
            seaRipples.Parameters.factor = 2f;
            seaRipples.Parameters.quantization = 4;
            seaRipples.Apply();

            Main.spriteBatch.Draw(rippleTarget2.Target, Vector2.Zero, null, new Color(103, 126, 255), 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
        }

        orig(self);*/
    }
    #endregion
}
