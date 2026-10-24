using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.CellularGrowth.Items;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using System;
using System.Collections;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static SpaceEventMod.Core.LocalizationReferences.Mods.SpaceEventMod;
using static Terraria.ModLoader.BackupIO;

namespace SpaceEventMod.Content.CellularGrowth.NPCs;
public class Flashbang : ModBuff
{
    public override void SetStaticDefaults()
    {
        BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.Shimmer);
    }

    public override void Update (NPC npc, ref int buffIndex)
    {
        npc.velocity = Vector2.Zero; 

    }


}
