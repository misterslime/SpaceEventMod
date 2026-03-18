using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;

namespace SpaceEventMod.Content.CellularGrowth.NPCs;

internal class Starstriker : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 11;
    }

    public override void SetDefaults()
    {
        NPC.width = 64;
        NPC.height = 50;
        NPC.damage = 14;
        NPC.defense = 6;
        NPC.lifeMax = 200;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;
        NPC.value = 60f;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = NPCAIStyleID.Snail;

        AIType = NPCID.Snail;
    }

    public override void FindFrame(int frameHeight)
    {

        // frameHeight = 110
        NPC.scale = 1;
        NPC.frame.X = 0;
        NPC.spriteDirection = -NPC.direction;
        NPC.frameCounter += 1.0;
        NPC.frameCounter %= 1.0;
        if ((int)NPC.frameCounter == 0.0)
        {
            NPC.frame.Y = NPC.frame.Y + frameHeight;
            NPC.frame.Y %= 550;
        }

        NPC.frame.Width = 64;
        NPC.frame.Height = 50;
    }
}
