using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Rendering;

[Autoload(Side = ModSide.Client)]
public class SeaBackground : ILoadable
{
    private FirmamentSea Sea { get => SpaceEvent.Sea; }

    public void Load(Mod mod) => On_Main.DoDraw_WallsTilesNPCs += DrawSeaBackground;

    public void Unload() => On_Main.DoDraw_WallsTilesNPCs -= DrawSeaBackground;

    private void DrawSeaBackground(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
    {
        if (SeaTargets.BackgroundRenderTarget is not null && Sea.Springs is not null && Sea.Active)
        {
            Graphics.BeginPipeline(0.5f)
                .DrawSprite(
                    SeaTargets.BackgroundRenderTarget,
                    Vector2.Zero,
                    Color.White,
                    SeaTargets.BackgroundRenderTarget.Bounds,
                    0f,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None)
                .ApplyEffect(
                    Assets.Assets.Shaders.Space.FirmamentSeaBackgroundTransparency.Value,
                    ("sea", SeaTargets.SeaRenderTarget),
                    ("minimumAlpha", 0.8f))
                .Flush();
        }

        orig(self);
    }

    public static void DrawBackground()
    {
        var rectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

        // sea bubbles
        Matrix pixelationMatrix = Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateScale(0.5f / Main.GameViewMatrix.Zoom.X, 0.5f / Main.GameViewMatrix.Zoom.Y, 1f)
            * Matrix.CreateTranslation(Main.GameViewMatrix.Translation.X * 0.5f, Main.GameViewMatrix.Translation.Y * 0.5f, 0f);

        var palette = Assets.Assets.Textures.Space.Palettes.FirmamentSea.NightBackground2.Value;
        Vector3[] sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 0.7f), new Vector3(0.05f, 0.05f, 1), new Vector3(-0.03f, 0.02f, 0.85f)];
        var bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.15f, 1f, 0.5f, 0.35f, 0.3f);
        var color = Color.White;
        color.A = 40;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        palette = Assets.Assets.Textures.Space.Palettes.FirmamentSea.NightBackground1.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 1.35f), new Vector3(-0.03f, 0.02f, 1.1f)];
        bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.2f, 0.7f, 0.35f, 0.4f, 0.5f);
        color.A = 100;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        palette = Assets.Assets.Textures.Space.Palettes.FirmamentSea.NightBackground0.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 2), new Vector3(0.03f, 0.02f, 2.2f)];
        bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.3f, 1f, 0f, 0.4f, 0.85f);
        color.A = 180;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();
    }

    private static Effect GetBackgroundBubbleShader(Texture2D palette, Vector3[] sampleOffsetsAndScales, float speed, float gradientLength, float gradientStart, float cutoff, float parallax)
    {
        var firmamentSeaBackgroundShader = Assets.Assets.Shaders.Space.FirmamentSeaBubbles.Value;

        firmamentSeaBackgroundShader.Parameters["bubbles"].SetValue(Assets.Assets.Textures.Noise.Bubble.Value);
        firmamentSeaBackgroundShader.Parameters["distortion"].SetValue(Assets.Assets.Textures.Noise.Perlin.Value);
        firmamentSeaBackgroundShader.Parameters["palette"].SetValue(palette);
        firmamentSeaBackgroundShader.Parameters["sampleOffsetsAndScales"].SetValue(sampleOffsetsAndScales);
        firmamentSeaBackgroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight) * 1.75f);
        firmamentSeaBackgroundShader.Parameters["screenWorldPosition"].SetValue(SpaceEvent.WorldToSeaCoordinates(Main.screenPosition));
        firmamentSeaBackgroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly * speed);
        firmamentSeaBackgroundShader.Parameters["gradientLength"].SetValue(gradientLength);
        firmamentSeaBackgroundShader.Parameters["gradientStart"].SetValue(gradientStart);
        firmamentSeaBackgroundShader.Parameters["cutoff"].SetValue(cutoff);
        firmamentSeaBackgroundShader.Parameters["parallax"].SetValue(parallax);

        return firmamentSeaBackgroundShader;
    }
}
