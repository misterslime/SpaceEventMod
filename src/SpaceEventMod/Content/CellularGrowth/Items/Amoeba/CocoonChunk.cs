using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items.Amoeba;

internal class CocoonChunk : ModProjectile
{
    public static LocalizedText BuffText { get; private set; }

    public override void SetStaticDefaults()
    {
        BuffText = this.GetLocalization("BuffText");
    }

    public override void SetDefaults()
    {
        Projectile.width = 44;
        Projectile.height = 22;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        Projectile.damage = 0;

        Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());

        if (Projectile.velocity.Y == 0f)
        {
            Projectile.velocity.X = 0f;

            if (Main.player[Projectile.owner].WithinRange(Projectile.Center, 32))
            {
                SoundEngine.PlaySound(SoundID.NPCDeath21, Projectile.Center);
                Main.player[Projectile.owner].AddBuff(ModContent.BuffType<DigestiveEnzymes>(), 10 * 60);
                Projectile.Kill();
            }
        }

        Projectile.velocity.X *= 0.995f;

        if (Projectile.wet || Projectile.lavaWet)
        {
            Projectile.velocity.Y = 0f;
        }
        else
        {
            Projectile.velocity.Y += 0.1f;
            if (Projectile.velocity.Y > 6f)
                Projectile.velocity.Y = 6f;
        }

        if (Main.myPlayer == Projectile.owner && Projectile.Hitbox.Contains(Main.MouseWorld.ToPoint()))
        {
            Main.instance.MouseTextHackZoom(BuffText.Value);
            Main.LocalPlayer.cursorItemIconEnabled = false;
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;
}

