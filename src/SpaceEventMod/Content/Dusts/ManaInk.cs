using Microsoft.Xna.Framework;
using System;
using Terraria;
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

        if (dust.scale < 1f)
            dust.scale += 0.01f;

        dust.color = Main.hslToRgb((Main.rgbToHsl(dust.color).X) % 1, Main.rgbToHsl(dust.color).Y, Main.rgbToHsl(dust.color).Z);
        dust.rotation += manaInkData.Spin;

        var toTarget = manaInkData.TargetPosition - dust.position;

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
        var orbitVelocity = new Vector2(-toTarget.Y, toTarget.X);
        var returnVelocity = toTarget;
        returnVelocity.Normalize();
        orbitVelocity.Normalize();

        dust.position += dust.velocity + orbitVelocity * MathF.Sqrt(toTarget.Length()) * 0.05f + returnVelocity * 0.3f;
    }

    public void Spray(Dust dust, Vector2 toTarget, ManaInkData manaInkData)
    {
        var returnVelocity = toTarget;
        returnVelocity.Normalize();

        dust.velocity += returnVelocity * 0.25f;
        dust.position += dust.velocity;

        if (toTarget.Length() <= 32)
            dust.active = false;
    }

    public override bool PreDraw(Dust dust) => false;
}

