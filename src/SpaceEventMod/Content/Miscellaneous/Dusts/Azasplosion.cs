using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Dusts;

internal class Azasplosion : ModDust
{
    private const int FRAMES = 9;
    private const int FPS = 4;

    public override bool Update(Dust dust)
    {
        dust.fadeIn++;
        
        if (dust.fadeIn > FRAMES * FPS)
            dust.active = false;

        dust.position += dust.velocity;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        var texture = Texture2D;

        int frameY = (int)Math.Floor(dust.fadeIn / (float)FPS);
        int frameX = dust.customData is null || dust.customData is not int num ? 0 : num;

        var frame = texture.Frame(3, 9, frameX, frameY);

        var lightColor = Lighting.GetSubLight(dust.position);
        var drawColor = new Color(lightColor);

        Main.EntitySpriteDraw(texture.Value, dust.position - Main.screenPosition, frame, drawColor, dust.rotation, frame.Size() * 0.5f, 1f, 0f);

        return false;
    }

}
