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
using Microsoft.Build.Utilities;

namespace SpaceEventMod.Content.Dusts;

public struct InkStarData(InkType inkType, Vector2 targetPosition, Color pixelColor)
{
    public InkType InkType = inkType;
    public Vector2 TargetPosition = targetPosition;
    public Color PixelColor = pixelColor;
}

public class InkStar : ModDust
{
    public override string Texture => null;

    public float maxLifetime = 20;

    public override void OnSpawn(Dust dust)
    {
        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not InkStarData inkStarData)
        {
            dust.active = false;
            return false;
        }

        float opacity = (float)Math.Sin((dust.fadeIn / 20) * MathHelper.Pi);

        Color lightColor = dust.color * opacity;
        Lighting.AddLight(dust.position, lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f);
        dust.velocity *= 0.95f;
        dust.rotation += dust.velocity.X * 0.004f;

        Vector2 toTarget = inkStarData.TargetPosition - dust.position;

        if (inkStarData.InkType == InkType.Orbiting)
            Orbit(dust, toTarget, inkStarData);
        else if (inkStarData.InkType == InkType.Spraying)
            Spray(dust, toTarget, inkStarData);

        dust.fadeIn--;

        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public void Orbit(Dust dust, Vector2 toTarget, InkStarData manaInkData)
    {
        Vector2 orbitVelocity = new Vector2(-toTarget.Y, toTarget.X);
        orbitVelocity.Normalize();

        Vector2 returnVelocity = toTarget;
        returnVelocity.Normalize();

        dust.position += dust.velocity + orbitVelocity * MathF.Sqrt(toTarget.Length()) * 0.04f + returnVelocity * 0.25f * dust.fadeIn / 20;
    }

    public void Spray(Dust dust, Vector2 toTarget, InkStarData manaInkData)
    {
        Vector2 returnVelocity = toTarget;
        returnVelocity.Normalize();

        dust.velocity += returnVelocity * 0.25f;
        dust.position += dust.velocity;

        if (toTarget.Length() <= 32)
            dust.active = false;
    }

    public override bool PreDraw(Dust dust) => false;
}
