using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space.LevelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.Space.Rendering;

[Autoload(Side = ModSide.Client)]
public class SeaForeground : ILoadable
{
    private FirmamentSea Sea { get => SpaceEvent.Sea; }

    public void Load(Mod mod) => On_Main.DrawInfernoRings += DrawSeaForeground;

    public void Unload() => On_Main.DrawInfernoRings -= DrawSeaForeground;

    private void DrawSeaForeground(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (SeaTargets.SeaRenderTarget == null || Sea.Springs is null || !Sea.Active)
            return;

        // round position to prevent artifacts
        var screenPosition = SpaceEvent.WorldToSeaCoordinates(Main.screenPosition);

        screenPosition.X = MathF.Floor(screenPosition.X * 0.5f);
        screenPosition.X *= 2f;

        screenPosition.Y = MathF.Floor(screenPosition.Y * 0.5f);
        screenPosition.Y *= 2f;

        Color color1 = new Color(118, 129, 247);
        Color color2 = new Color(169, 201, 234);

        var firmamentSeaForegroundShader = Assets.Assets.Shaders.Events.FirmamentSeaFoam.Value;

        firmamentSeaForegroundShader.Parameters["noise"].SetValue(Assets.Assets.Textures.Noise.Foam.Value);
        firmamentSeaForegroundShader.Parameters["palette"].SetValue(Assets.Assets.Textures.Palettes.FirmamentSea.NightForeground.Value);
        firmamentSeaForegroundShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        firmamentSeaForegroundShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        firmamentSeaForegroundShader.Parameters["screenWorldPosition"].SetValue(screenPosition);

        firmamentSeaForegroundShader.Parameters["edgeColor1"].SetValue(color1.ToVector4());
        firmamentSeaForegroundShader.Parameters["edgeColor2"].SetValue(color2.ToVector4());

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, firmamentSeaForegroundShader, Main.GameViewMatrix.TransformationMatrix);
        Main.spriteBatch.Draw(SeaTargets.SeaRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(118, 129, 247), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }
}
