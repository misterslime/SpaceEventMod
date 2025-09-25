using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space.LevelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs;

internal class TitanSpawn : ModNPC
{
    private ref float Timer => ref NPC.ai[1];

    public override void SetDefaults()
    {
        NPC.width = 80;
        NPC.height = 92;
        NPC.damage = 50;
        NPC.defense = 16;
        NPC.lifeMax = 100;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;
        NPC.immortal = true;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void AI()
    {
        NPC.TargetClosest(false);

        float speed = 0.5f + MathF.Pow(MathF.Sin(Timer / 30f), 2) * 0.5f;
        speed *= 3f;

        Vector2 vector = Main.player[NPC.target].Center - NPC.Center;

        NPC.velocity = vector.SafeNormalize(Vector2.Zero) * speed;
        NPC.scale = ((float)NPC.life / (float)NPC.lifeMax);

        Vector3 lightColor = Lighting.GetSubLight(NPC.Center);

        if (lightColor.LengthSquared() > 0.10f && Timer % 20 == 0)
        {
            int damage = (int)(lightColor.LengthSquared() * 20);

            NPC.StrikeNPC(damage, 0, 0);
            NPC.life -= damage;
        }

        Timer++;
    }


    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        int frame = (int)MathF.Floor(Timer / 10) % 8;

        var texture = TextureAssets.Npc[Type].Value;
        var drawPosition = NPC.Center - screenPos;

        Rectangle rectangle = texture.Frame(1, 8, 0, frame);

        spriteBatch.Draw(texture, drawPosition, texture.Frame(1, 8, 0, frame), Color.White, 0f, rectangle.Size() * 0.5f, NPC.scale, 0, 0);

        return false;
    }
}
