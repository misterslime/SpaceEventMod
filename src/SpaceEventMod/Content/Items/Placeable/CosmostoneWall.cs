using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Placeable;

internal class CosmostoneWall : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<Walls.CosmostoneWall>());
    }
}
