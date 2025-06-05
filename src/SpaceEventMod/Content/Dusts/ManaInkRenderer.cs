using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

public class ManaInkRenderer : ModSystem
{
    public override void Load()
    {
        On_Main.DrawDust += DrawManaInk;
    }

    public override void Unload()
    {
        On_Main.DrawDust -= DrawManaInk;
    }

    public void DrawManaInk(On_Main.orig_DrawDust orig, Main self)
    {
        PixelRenderer.Draw((SpriteBatch spriteBatch) =>
        {
            foreach (Dust dust in Main.dust.Where(d => d.type == ModContent.DustType<ManaInk>() && d.active))
            {
                if (dust.customData == null || dust.customData is not ManaInkData manaInkData)
                    continue;

                Texture2D texture = Assets.Assets.Textures.Dusts.MediumSmoke_Glow.Value;
                Rectangle frame = new Rectangle(0, 38 * manaInkData.FrameVariant, 36, 38);

                Color drawColor = new Color(69, 77, 255);

                spriteBatch.Draw(texture, dust.position - Main.screenPosition, frame, drawColor, dust.rotation, frame.Size() * 0.5f, dust.scale * 1.1f, SpriteEffects.None, 0f);
            }

            foreach (Dust dust in Main.dust.Where(d => d.type == ModContent.DustType<ManaInk>() && d.active))
            {
                if (dust.customData == null || dust.customData is not ManaInkData manaInkData)
                    return;

                Texture2D texture = Assets.Assets.Textures.Dusts.MediumSmoke.Value;
                Rectangle frame = new Rectangle(0, 34 * manaInkData.FrameVariant, 32, 34);

                spriteBatch.Draw(texture, dust.position - Main.screenPosition, frame, dust.color, dust.rotation, frame.Size() / 2f, dust.scale, SpriteEffects.None, 0f);
            }

            foreach (Dust dust in Main.dust.Where(d => d.type == ModContent.DustType<ManaInkSparkle>() && d.active))
            {
                if (dust.customData == null || dust.customData is not ManaSparkleData manaInkData)
                    return;

                Texture2D whitePixel = SpaceEventMod.WhitePixel;
                float opacity = (float)Math.Sin((dust.fadeIn / 20) * MathHelper.Pi);

                spriteBatch.Draw(SpaceEventMod.WhitePixel, dust.position - Main.screenPosition, SpaceEventMod.WhitePixel.Bounds, manaInkData.PixelColor * opacity, 0f, SpaceEventMod.WhitePixel.Size() * 0.5f, 2f, SpriteEffects.None, 0f);
            }
        });

        orig(self);
    }
}
