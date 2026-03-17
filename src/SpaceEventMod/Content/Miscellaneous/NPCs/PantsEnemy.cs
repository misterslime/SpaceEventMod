using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SpaceEventMod.Content.Miscellaneous.NPCs;

internal class PantsEnemy : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Velocity = 1f
        });
    }

    public override void SetDefaults()
    {
        NPC.width = 32;
        NPC.height = 52;
        NPC.damage = 12;
        NPC.defense = 6;
        NPC.lifeMax = 100;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;
        NPC.value = 60f;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = 3;

        AIType = NPCID.Zombie;
    }

    public override void AI()
    {
        NPC.rotation += (NPC.velocity.X + NPC.velocity.Y * NPC.direction) / MathF.Tau;
    }
}
