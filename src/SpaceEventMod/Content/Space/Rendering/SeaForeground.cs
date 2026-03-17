using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Rendering;

[Autoload(Side = ModSide.Client)]
public class SeaForeground : ILoadable
{
    private FirmamentSea Sea { get => SpaceEvent.Sea; }

    public void Load(Mod mod) => On_Main.DrawInfernoRings += DrawSeaForeground;

    public void Unload() => On_Main.DrawInfernoRings -= DrawSeaForeground;

    private void DrawSeaForeground(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        if (SeaTargets.SeaRenderTarget is not null && Sea.Springs is not null && Sea.Active)
        {
            // round position to prevent artifacts
            var screenPosition = SpaceEvent.WorldToSeaCoordinates(Main.screenPosition);

            screenPosition.X = MathF.Floor(screenPosition.X * 0.5f);
            screenPosition.X *= 2f;

            screenPosition.Y = MathF.Floor(screenPosition.Y * 0.5f);
            screenPosition.Y *= 2f;

            var color1 = new Color(118, 129, 247);
            var color2 = new Color(169, 201, 234);

            var snapshot = Main.spriteBatch.Capture();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var firmamentSeaForegroundShader = Assets.Assets.Shaders.Events.FirmamentSeaFoam.Value;

            firmamentSeaForegroundShader.Parameters["noise"].SetValue(Assets.Assets.Textures.Noise.Foam.Value);
            firmamentSeaForegroundShader.Parameters["palette"].SetValue(Assets.Assets.Textures.Space.Palettes.FirmamentSea.NightForeground.Value);
            firmamentSeaForegroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
            firmamentSeaForegroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            firmamentSeaForegroundShader.Parameters["screenWorldPosition"].SetValue(screenPosition);

            firmamentSeaForegroundShader.Parameters["edgeColor1"].SetValue(color1.ToVector4());
            firmamentSeaForegroundShader.Parameters["edgeColor2"].SetValue(color2.ToVector4());

            firmamentSeaForegroundShader.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(118, 129, 247), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(snapshot);
        }

        orig(self);
    }
}
