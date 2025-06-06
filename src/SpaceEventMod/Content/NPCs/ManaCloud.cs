using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpaceEventMod.Content.Dusts;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using System;
using Terraria.ID;
using static Terraria.GameContent.Animations.IL_Actions.NPCs;
using System.IO;

namespace SpaceEventMod.Content.NPCs;

public class ManaCloud : ModProjectile
{
    public ref float Timer => ref Projectile.ai[0];

    public override string Texture => "SpaceEventMod/Assets/Textures/Extra/EmptyPixel";

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.hostile = true;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 3600;
    }

    public override void AI()
    {
        if (Timer > 480)
        {
            Projectile.Kill();
        }

        float maxVelocity = Math.Clamp((float)Math.Pow(Timer / 20, 2), 0, 12);
        Vector2 vel = new Vector2(Main.rand.NextFloat(Math.Clamp((float)Math.Pow(Timer / 20, 2), 0, 12)), 0).RotatedByRandom(MathHelper.TwoPi) * 1.5f;
        float rotate = MathHelper.ToRadians(Main.rand.NextFloat(-3, 0));

        Dust mist = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40, 40), ModContent.DustType<ManaInk>(), vel);
        mist.noGravity = true;
        mist.color = new Color(9, 17, 51);
        mist.fadeIn = 120;
        mist.scale = 1.1f;
        mist.customData = new ManaInkData(Main.rand.Next(3), InkType.Orbiting, 120, rotate, Projectile.position, Projectile.whoAmI);

        float lifetimeStarRadiusRatio = (float)Math.Clamp(maxVelocity / 12, 0.4, 1);
        Vector2 dustPosition = Projectile.Center + (new Vector2(Main.rand.NextFloat(20 * lifetimeStarRadiusRatio, 120 * lifetimeStarRadiusRatio), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)) * new Vector2(1, 0.6f));

        Dust sparkle = Dust.NewDustPerfect(dustPosition, ModContent.DustType<InkStar>(), Vector2.Zero);
        sparkle.noGravity = true;
        sparkle.color = new Color(89, 97, 255);
        sparkle.fadeIn = 20;
        sparkle.scale = 1f;
        sparkle.customData = new InkStarData(InkType.Orbiting, Projectile.position, Color.Lerp(Color.Yellow, Color.Purple, Main.rand.NextFloat()));

        if (maxVelocity < 12)
        {
            sparkle.velocity = Main.rand.NextVector2Circular(1, 1);
        }

        Timer++;
    }
}