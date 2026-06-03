using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Animation.Tweening;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Dusts;

public struct MistData(int variant, float spin)
{
    public int FrameVariant = variant;
    public float Spin = spin;
    public bool HasParticle = Main.rand.NextBool(8);
}

public class Mist : ModDust
{
    public int counter;

    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Dusts/MediumSmoke";

    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not MistData mistData)
        {
            dust.active = false;
            return false;
        }

        dust.fadeIn++;
        dust.rotation += mistData.Spin;
        dust.velocity.X *= 0.995f;
        dust.velocity.Y -= 0.002f;

        if (dust.fadeIn > 70)
        {
            dust.color *= 1 - (dust.fadeIn - 75) / 60f;
        }

        if (dust.fadeIn > 50)
            dust.scale += 0.01f;
        else
            dust.scale *= 0.99f;

        if (dust.fadeIn >= 120)
        {
            dust.active = false;
            return false;
        }

        Lighting.AddLight(dust.position, new Color(0, 0, 118).ToVector3());

        dust.color = new Color(0, 0, 118) * 0.25f;
        dust.color.A = (byte)Math.Min(dust.color.A * 0.5f, 80);

        dust.position += dust.velocity;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        if (dust.customData == null || dust.customData is not MistData mistData)
            return false;

        var inkTexture = Assets.Textures.CellularGrowth.Dusts.MediumSmoke.Asset.Value;

        var frame = new Rectangle(0, 34 * mistData.FrameVariant, 32, 34);

        var pixelPosition = dust.rotation.ToRotationVector2() * 8f * dust.scale;

        var pipeline = Graphics.BeginPipeline(0.5f);

        pipeline.DrawSprite(
            inkTexture,
            dust.position - Main.screenPosition,
            dust.color,
            frame,
            dust.rotation,
            frame.Size() * 0.5f,
            new Vector2(dust.scale));

        if (mistData.HasParticle)
        {
            pipeline.DrawSprite(
                Assets.Textures.WhitePixel.Asset.Value,
                dust.position - Main.screenPosition + pixelPosition,
                new Color(42, 176, 191) { A = 0 },
                scale: Vector2.One * 2f);
        }

        pipeline.Schedule(RenderLayer.AfterPlayers);

        return false;
    }
}

