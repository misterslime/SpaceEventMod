using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Dusts;

public struct SleepData(int parent)
{
    public int Parent = parent;
    public readonly int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);
}

public class Sleep : ModDust
{
    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not SleepData sleepData)
        {
            dust.active = false;
            return false;
        }

        var scaleReduction = Math.Clamp(dust.fadeIn / 60, 0, 1);

        dust.scale = scaleReduction * 0.7f + MathF.Pow(MathF.Sin((Main.GameUpdateCount + sleepData.RandomTimeDisplacement) / 15f), 2) * 0.3f;
        dust.rotation = MathF.Sin((Main.GameUpdateCount + sleepData.RandomTimeDisplacement) / 10f) * (MathF.PI / 180f) * 10;

        dust.velocity.X *= 0.975f;

        dust.position += dust.velocity + Vector2.UnitX * MathF.Sin(dust.fadeIn / 15f) * 0.6f;

        dust.fadeIn--;
        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        var sleepTexture = Assets.Textures.Miscellaneous.Dusts.Sleep.Asset.Value;

        Graphics.BeginPipeline(0.5f)
            .DrawSprite(
                sleepTexture,
                dust.position - Main.screenPosition,
                dust.color,
                sleepTexture.Frame(),
                dust.rotation,
                sleepTexture.Size() * 0.5f,
                new Vector2(dust.scale))
            .Schedule(RenderLayer.AfterNPCs);

        return false;
    }
}
