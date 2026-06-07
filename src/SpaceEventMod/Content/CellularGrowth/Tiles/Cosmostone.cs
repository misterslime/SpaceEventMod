using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class CosmostoneItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Cosmostone>());
        Item.width = 12;
        Item.height = 12;
    }
}

internal class Cosmostone : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMerge[TileID.Grass][Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmoss>()][Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        MineResist = .5f;
        HitSound = SoundID.Tink;

        AddMapEntry(Color.Gray);
    }
}
