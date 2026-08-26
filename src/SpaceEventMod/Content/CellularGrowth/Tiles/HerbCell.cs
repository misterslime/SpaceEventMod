using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class HerbCell : ModTile, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 100;

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
