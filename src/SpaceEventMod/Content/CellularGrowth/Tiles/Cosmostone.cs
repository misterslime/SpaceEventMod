using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class Cosmostone : ModTile, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 100;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmoss>()][Type] = true;
        Main.tileMerge[ModContent.TileType<HerbCell>()][Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        DustType = DustID.Stone;
        HitSound = SoundID.Tink;

        AddMapEntry(Color.Gray);
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
    {
        WorldGen.TileMergeAttempt(-2, ModContent.TileType<HerbCell>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }
}
