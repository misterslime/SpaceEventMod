using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Core.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.Space.Rendering;

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
            var firmamentSeaBackgroundShader = Assets.Assets.Shaders.Events.FirmamentSeaBackgroundTransparency.Value;

            firmamentSeaBackgroundShader.Parameters["sea"].SetValue(SeaTargets.SeaRenderTarget);
            firmamentSeaBackgroundShader.Parameters["minimumAlpha"].SetValue(0.65f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, firmamentSeaBackgroundShader, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(SeaTargets.BackgroundRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
        }

        orig(self);
    }

    public static void DrawBackground(Action<Color, Color> drawBackgroundPrimitives)
    {
        var rectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

        drawBackgroundPrimitives.Invoke(new Color(10, 0, 100), new Color(10, 0, 100));

        // sea bubbles
        var palette = Assets.Assets.Textures.Palettes.FirmamentSea.NightBackground2.Value;
        Vector3[] sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 0.7f), new Vector3(0.05f, 0.05f, 1), new Vector3(-0.03f, 0.02f, 0.85f)];
        var bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.15f, 1f, 0.5f, 0.35f, 0.3f);
        var color = Color.White;
        color.A = 40;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, PixelRenderer.GetPixelationMatrix());
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        palette = Assets.Assets.Textures.Palettes.FirmamentSea.NightBackground1.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 1.35f), new Vector3(-0.03f, 0.02f, 1.1f)];
        bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.2f, 0.7f, 0.35f, 0.4f, 0.5f);
        color.A = 100;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, PixelRenderer.GetPixelationMatrix());
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();

        palette = Assets.Assets.Textures.Palettes.FirmamentSea.NightBackground0.Value;
        sampleOffsetsAndScales = [new Vector3(0.02f, 0.03f, 1), new Vector3(0.05f, 0.05f, 2), new Vector3(0.03f, 0.02f, 2.2f)];
        bubbleShader = GetBackgroundBubbleShader(palette, sampleOffsetsAndScales, 0.3f, 1f, 0f, 0.4f, 0.85f);
        color.A = 180;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, bubbleShader, PixelRenderer.GetPixelationMatrix());
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, rectangle, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
        Main.spriteBatch.End();
    }

    private static Effect GetBackgroundBubbleShader(Texture2D palette, Vector3[] sampleOffsetsAndScales, float speed, float gradientLength, float gradientStart, float cutoff, float parallax)
    {
        var firmamentSeaBackgroundShader = Assets.Assets.Shaders.Events.FirmamentSeaBubbles.Value;

        firmamentSeaBackgroundShader.Parameters["bubbles"].SetValue(Assets.Assets.Textures.Noise.Bubble.Value);
        firmamentSeaBackgroundShader.Parameters["distortion"].SetValue(Assets.Assets.Textures.Noise.Perlin.Value);
        firmamentSeaBackgroundShader.Parameters["palette"].SetValue(palette);
        firmamentSeaBackgroundShader.Parameters["sampleOffsetsAndScales"].SetValue(sampleOffsetsAndScales);
        firmamentSeaBackgroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        firmamentSeaBackgroundShader.Parameters["screenWorldPosition"].SetValue(SpaceEvent.WorldToSeaCoordinates(Main.screenPosition) * 0.5f); // this is being halved because its being pixelated
        firmamentSeaBackgroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly * speed);
        firmamentSeaBackgroundShader.Parameters["gradientLength"].SetValue(gradientLength);
        firmamentSeaBackgroundShader.Parameters["gradientStart"].SetValue(gradientStart);
        firmamentSeaBackgroundShader.Parameters["cutoff"].SetValue(cutoff);
        firmamentSeaBackgroundShader.Parameters["parallax"].SetValue(parallax);

        return firmamentSeaBackgroundShader;
    }
}
