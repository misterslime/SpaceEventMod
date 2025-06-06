using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

public struct ManaInkData(int variant, InkType inkType, int lifetime, float spin, Vector2 targetPosition, int parent = -1)
{
    public int FrameVariant = variant;
    public InkType InkType = inkType;
    public int Lifetime = lifetime;
    public float Spin = spin;
    public Vector2 TargetPosition = targetPosition;
    public int Parent = parent;
}

public class ManaInk : ModDust
{
    public override string Texture => "SpaceEventMod/Assets/Textures/Dusts/MediumSmoke";

    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not ManaInkData manaInkData)
        {
            dust.active = false;
            return false;
        }

        if (dust.fadeIn / (float)manaInkData.Lifetime > 0.8f)
            dust.scale += 0.01f;
        else
            dust.scale *= 0.99f;

        dust.color = Main.hslToRgb((Main.rgbToHsl(dust.color).X) % 1, Main.rgbToHsl(dust.color).Y, Main.rgbToHsl(dust.color).Z);
        dust.rotation += manaInkData.Spin;

        Vector2 toTarget = manaInkData.TargetPosition - dust.position;

        if (manaInkData.InkType == InkType.Orbiting)
            Orbit(dust, toTarget, manaInkData);
        else if (manaInkData.InkType == InkType.Spraying)
            Spray(dust, toTarget, manaInkData);

        dust.fadeIn--;
        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public void Orbit(Dust dust, Vector2 toTarget, ManaInkData manaInkData)
    {
        dust.velocity *= 0.85f;
        Vector2 orbitVelocity = new Vector2(-toTarget.Y, toTarget.X);
        Vector2 returnVelocity = Vector2.Zero;

        if (!Main.projectile[manaInkData.Parent].active || Main.projectile[manaInkData.Parent].type != ModContent.DustType<ManaInk>())
        {
            returnVelocity = toTarget;
            returnVelocity.Normalize();
        }

        orbitVelocity.Normalize();

        dust.position += dust.velocity + orbitVelocity * MathF.Sqrt(toTarget.Length()) * 0.05f + returnVelocity * 0.3f;
    }

    public void Spray(Dust dust, Vector2 toTarget, ManaInkData manaInkData)
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

