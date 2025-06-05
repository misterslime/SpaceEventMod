using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Graphics;

namespace SpaceEventMod.Content.Dusts;

public struct ManaSparkleData(Vector2 orbitPosition, Color pixelColor)
{
    public Vector2 OrbitPosition = orbitPosition;
    public Color PixelColor = pixelColor;
}

public class ManaInkSparkle : ModDust
{
    public override string Texture => null;

    public float maxLifetime = 20;

    public override void OnSpawn(Dust dust)
    {
        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not ManaSparkleData manaInkData)
        {
            dust.active = false;
            return false;
        }

        float opacity = (float)Math.Sin((dust.fadeIn / 20) * MathHelper.Pi);

        Color lightColor = dust.color * opacity;
        Lighting.AddLight(dust.position, lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f);
        dust.velocity *= 0.95f;
        dust.rotation += dust.velocity.X * 0.004f;

        Vector2 toOrbitPosition = manaInkData.OrbitPosition - dust.position;

        Vector2 orbitVelocity = new Vector2(-toOrbitPosition.Y, toOrbitPosition.X);
        orbitVelocity.Normalize();

        Vector2 returnVelocity = toOrbitPosition;
        returnVelocity.Normalize();

        dust.position += dust.velocity + orbitVelocity * MathF.Sqrt(toOrbitPosition.Length()) * 0.04f + returnVelocity * 0.25f * dust.fadeIn / 20;

        dust.fadeIn--;

        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust) => false;
}
