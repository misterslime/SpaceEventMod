using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Tweening;
using SpaceEventMod.Content.Space;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Dusts;

//all stolen from azasplosion

internal class FlashbangDust : ModDust
{
    private const int FRAMES = 13;
    private const int FPS = 3;

    public override bool Update(Dust dust)
    {
        dust.fadeIn++;

        if (dust.fadeIn > FRAMES * FPS)
            dust.active = false;


        if (!Filters.Scene["SpaceEventMod:Flashbang"].IsActive())
            Filters.Scene.Activate("SpaceEventMod:Flashbang");


        float ratio = 1f - (dust.fadeIn / (FRAMES * FPS));
        ratio = MathHelper.Clamp(MathF.Sin(ratio * MathF.PI), 0f, 1f);

        FlashBangScreenShaderData.FlashRadius = EasingFunctions.InSine(ratio * 0.67f);
        FlashBangScreenShaderData.FlashStrength = EasingFunctions.InCirc(ratio);
        FlashBangScreenShaderData.FlashPosition = dust.position;
        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        var texture = Texture2D;

        int frameY = (int)Math.Floor(dust.fadeIn / (float)FPS);
        int frameX = dust.customData is null || dust.customData is not int num ? 0 : num;

        var frame = texture.Frame(1, FRAMES, frameX, frameY);

        //var lightColor = Lighting.GetSubLight(dust.position);
        var drawColor = Color.White;
        drawColor.A = 0;

        Main.EntitySpriteDraw(texture.Value, dust.position - Main.screenPosition, frame, drawColor, dust.rotation, frame.Size() * 0.5f, 1f, 0f);

        return false;
    }

}