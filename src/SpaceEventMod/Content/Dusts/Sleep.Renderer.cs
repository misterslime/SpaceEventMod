using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

public class SleepRenderer : ModSystem
{
    public override void Load()
    {
        On_Main.DrawDust += DrawPixelatedDust;
    }

    public override void Unload()
    {
        On_Main.DrawDust -= DrawPixelatedDust;
    }

    public void DrawPixelatedDust(On_Main.orig_DrawDust orig, Main self)
    {
        PixelRenderer.Draw(null, (SpriteBatch spriteBatch) =>
        {
            foreach (var dust in Main.dust.Where(d => d.type == ModContent.DustType<Sleep>() && d.active))
            {
                var sleepTexture = Assets.Assets.Textures.Dusts.Sleep.Value;

                spriteBatch.Draw(sleepTexture, dust.position - Main.screenPosition, sleepTexture.Frame(), dust.color, dust.rotation, sleepTexture.Size() / 2f, dust.scale, SpriteEffects.None, 0f);
            }
        });

        orig(self);
    }
}
