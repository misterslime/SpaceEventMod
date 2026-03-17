using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items.Amoeba;

internal class Cocoon : ModNPC
{
    public override void SetDefaults()
    {
        NPC.friendly = true;
        NPC.width = 286 / 4;
        NPC.height = 409 / 4;
        NPC.aiStyle = -1;
        NPC.damage = 10;
        NPC.defense = 0;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void AI()
    {
        NPC.ai[0] += 1f / 60f;

        var player = Main.player[NPC.target];

        if (player.dead || !player.active)
            NPC.StrikeNPC(new NPC.HitInfo() { Damage = int.MaxValue });

        if (NPC.collideX)
        {
            // squish on tile impact
        }

        if (NPC.collideY)
        {
            // squish on tile impact

            NPC.velocity.X *= 0.99f;
        }

        NPC.ai[1] = 1 + (float)Math.Sin(NPC.ai[0] * 8) * 0.05f;
        NPC.ai[2] = 1 + (float)Math.Sin(NPC.ai[0] * 8 + 1) * 0.05f;

        Lighting.AddLight(NPC.Center, Color.Red.ToVector3());

        var playerPos = NPC.getRect().Bottom();
        playerPos -= new Vector2(0, NPC.height * 0.5f * NPC.ai[2]);

        player.Center = playerPos;
        player.velocity = NPC.velocity;
        player.AddBuff(ModContent.BuffType<Cocooned>(), 20);
        player.GetModPlayer<CocoonPlayer>().MyCocoon = NPC.whoAmI;
    }

    public override void UpdateLifeRegen(ref int damage)
    {
        NPC.lifeRegen -= 30;
    }

    public override bool PreKill()
    {
        var player = Main.player[NPC.target];
        player.GetModPlayer<CocoonPlayer>().Cocooned = false;
        player.ClearBuff(ModContent.BuffType<Cocooned>());
        player.AddBuff(ModContent.BuffType<CocoonCooldown>(), 3600);

        var type = ModContent.ProjectileType<CocoonChunk>();

        var numChunks = Main.rand.Next(3, 4);
        for (var i = 0; i < numChunks; i++)
        {
            var blobVelocity = Main.rand.NextVector2CircularEdge(1f, 1f);
            blobVelocity.Y = -MathF.Abs(blobVelocity.Y);
            blobVelocity *= Main.rand.Next(400, 801) * 0.01f;


            Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, blobVelocity, type, 0, 0f, NPC.target, 0f);
        }

        return true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var effects = NPC.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        var texture = TextureAssets.Npc[Type].Value;

        

        var scale = new Vector2(NPC.ai[1], NPC.ai[2]) * (1f/4f);

        var origin = texture.Size();
        origin.X *= 0.5f;

        spriteBatch.Draw(
            texture,
            NPC.getRect().Bottom() - Main.screenPosition,
            texture.Bounds,
            drawColor * NPC.Opacity,
            0f,
            origin,
            scale,
            effects,
            0);

        return false;
    }
}
