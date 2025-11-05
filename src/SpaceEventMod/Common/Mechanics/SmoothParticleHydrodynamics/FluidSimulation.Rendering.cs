using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Utilities.Extensions;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Mechanics.SmoothParticleHydrodynamics;

internal partial class FluidSimulation
{
    public void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = Assets.Assets.Textures.WhitePixel.Value;

        Vector2 center = Position;
        Vector2 mouse = Main.MouseWorld / SCALE;

        for (int i = 0; i < s_positions.Length; i++)
        {
            Vector3 sdg = SmoothDistanceGradientSegment(s_positions[i], center, mouse, 0f);

            Color color = Color.Lerp(Color.Orange, Color.BlueViolet, MathHelper.Clamp(sdg.X / 2, 0, 1));
            spriteBatch.Draw(tex, s_positions[i] * SCALE - Main.screenPosition, null, color, 0f, tex.Size() * 0.5f, 2f, 0, 0);
        }
    }
}
