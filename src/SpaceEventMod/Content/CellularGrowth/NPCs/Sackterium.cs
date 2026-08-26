using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Miscellaneous.Projectiles;
using System;
using System.Collections;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Sackteriums;

internal partial class Sackterium : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 34;
        NPC.height = 50;
        NPC.damage = 0;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void AI()
    {
        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return;

        NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();

        if ((int)NPC.ai[0] < 0 || (int)NPC.ai[0] >= Main.maxProjectiles)
            return;

        if (Main.projectile[(int)NPC.ai[0]].type != ModContent.ProjectileType<WindGustBlow>() || !Main.projectile[(int)NPC.ai[0]].active)
            NPC.ai[0] = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.velocity, ModContent.ProjectileType<WindGustBlow>(), 0, 0, -1, NPC.whoAmI, 0, 200);

    }

    public override void OnKill()
    {
        if ((int)NPC.ai[0] < 0 || (int)NPC.ai[0] >= Main.maxProjectiles)
            return;

        if (Main.projectile[(int)NPC.ai[0]].type != ModContent.ProjectileType<WindGustBlow>() || !Main.projectile[(int)NPC.ai[0]].active)
            return;

        Main.projectile[(int)NPC.ai[0]].Kill();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;

        var origin = new Vector2(34, 50) * 0.5f;
        var rotation = NPC.rotation + MathHelper.PiOver2;

        spriteBatch.Draw(texture, NPC.Center - screenPos, null, drawColor, rotation, origin, NPC.scale, 0, 0);

        return false;
    }
}
