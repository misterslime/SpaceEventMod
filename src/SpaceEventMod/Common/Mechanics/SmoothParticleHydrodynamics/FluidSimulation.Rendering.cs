using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space;
using SpaceEventMod.Core.Graphics;
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

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        Texture2D tex = Assets.Assets.Textures.Glow.Value;

        for (int i = 0; i < s_positions.Length; i++)
        {
            Main.spriteBatch.Draw(tex, SpaceEvent.SeaToWorldCoordinates(s_positions[i] * SCALE) - Main.screenPosition, null, Color.White, 0f, tex.Size() * 0.5f, Vector2.One * 0.5f, 0, 0);
        }

        Main.spriteBatch.End();

        return;
    }
}
