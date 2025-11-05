using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Utilities.Extensions;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Mechanics.SmoothParticleHydrodynamics;
internal partial class FluidSimulation : ModSystem
{
    public override void PostDrawTiles()
    {
        if (!Active)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        Texture2D tex = Assets.Assets.Textures.WhitePixel.Value;

        Vector2 middle = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;

        for (int i = 0; i < s_positions.Length; i++)
        {

            Main.spriteBatch.Draw(tex, middle + s_positions[i] * 40f, null, Color.White, 0f, tex.Size() * 0.5f, 2f, 0, 0);
        }

        Main.spriteBatch.End();
        return;
    }
}
