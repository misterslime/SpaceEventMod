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

internal class HerbCell : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        //Main.tileBlockLight[Type] = true;

        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        DustType = -1;
        MineResist = 0.5f;

        AddMapEntry(new Color(30, 255, 241));
    }

}
