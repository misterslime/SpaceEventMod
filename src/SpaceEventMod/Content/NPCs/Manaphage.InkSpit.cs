using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Dusts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs;

public class InkSpit : ModProjectile
{
    public ref float Timer => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.hostile = true;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 120;
    }

    public override void AI()
    {
        if (Timer > 120)
            Projectile.Kill();

        Projectile.velocity.Y += Projectile.ai[1];
        Projectile.rotation = Projectile.velocity.ToRotation();

        Timer++;
    }
}
