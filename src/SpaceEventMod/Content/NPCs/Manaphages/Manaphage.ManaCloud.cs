using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Dusts;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Manaphages;

public class ManaCloud : ModProjectile
{
    public ref float Timer => ref Projectile.ai[0];

    public override string Texture => "SpaceEventMod/Assets/Textures/EmptyPixel";

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

        var maxVelocity = Math.Clamp(Timer / 5, 0, 12);
        var lifetimeStarRadiusRatio = (float)Math.Clamp(maxVelocity / 12, 0, 1);

        for (var i = 0; i < 3; i++)
        {
            var mistPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(10 * lifetimeStarRadiusRatio, 120 * lifetimeStarRadiusRatio);
            var rotate = MathHelper.ToRadians(Main.rand.NextFloat(-3, 0));

            var mist = Dust.NewDustPerfect(mistPosition, ModContent.DustType<ManaInk>(), Vector2.Zero);
            mist.noGravity = true;
            mist.color = new Color(9, 17, 51);
            mist.fadeIn = 120;
            mist.scale = 0.3f;
            mist.customData = new ManaInkData(Main.rand.Next(3), InkType.Orbiting, 120, rotate, Projectile.position, Projectile.whoAmI);
        }

        var dustPosition = Projectile.Center + (new Vector2(Main.rand.NextFloat(10 * lifetimeStarRadiusRatio, 120 * lifetimeStarRadiusRatio), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));

        var sparkle = Dust.NewDustPerfect(dustPosition, ModContent.DustType<InkStar>(), Vector2.Zero);
        sparkle.noGravity = true;
        sparkle.color = new Color(89, 97, 255);
        sparkle.fadeIn = 20;
        sparkle.scale = 1f;
        sparkle.customData = new InkStarData(InkType.Orbiting, Projectile.position, Color.Lerp(Color.Yellow, Color.Purple, Main.rand.NextFloat()));
        sparkle.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);

        Timer++;
    }
}