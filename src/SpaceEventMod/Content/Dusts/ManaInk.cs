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
using static Terraria.GameContent.Animations.IL_Actions.NPCs;

namespace SpaceEventMod.Content.Dusts;

public struct ManaInkData(int variant, int lifetime, float spin, Vector2 orbitPosition, int parentProjectile)
{
    public int FrameVariant = variant;
    public int Lifetime = lifetime;
    public float Spin = spin;
    public Vector2 OrbitPosition = orbitPosition;
    public int ParentProjectile = parentProjectile;
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
        dust.velocity *= 0.85f;

        Vector2 toOrbitPosition = manaInkData.OrbitPosition - dust.position;

        Vector2 orbitVelocity = new Vector2(-toOrbitPosition.Y, toOrbitPosition.X);
        Vector2 returnVelocity = Vector2.Zero;

        if (!Main.projectile[manaInkData.ParentProjectile].active || Main.projectile[manaInkData.ParentProjectile].type != ModContent.DustType<ManaInk>())
        {
            returnVelocity = toOrbitPosition;
            returnVelocity.Normalize();
        }

        orbitVelocity.Normalize();

        dust.position += dust.velocity + orbitVelocity * MathF.Sqrt(toOrbitPosition.Length()) * 0.05f + returnVelocity * 0.3f;

        dust.fadeIn--;
        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust) => false;
}

