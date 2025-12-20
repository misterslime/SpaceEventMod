using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

public class Pixel : ModDust
{
    public override string Texture => null;

    public float maxLifetime = 60;

    public override bool Update(Dust dust)
    {
        dust.velocity *= 0.95f;
        dust.position += dust.velocity;

        dust.fadeIn--;

        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        Vector2 position = dust.position - Main.screenPosition;

        position = (position * 0.5f).Floor() * 2f;

        Color color = Color.Lerp(Color.Transparent, dust.color, dust.fadeIn / 30);
        Vector2 scale = Vector2.One * 2f;
        color.A = 0;

        Graphics.BeginPipeline(0.5f)
            .DrawSprite(
                Assets.Assets.Textures.WhitePixel.Value,
                position,
                color,
                scale: scale)
            .Schedule(RenderLayer.BeforeNPCs);

        return false;
    }
}
