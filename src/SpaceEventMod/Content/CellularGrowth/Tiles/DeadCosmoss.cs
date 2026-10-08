using Microsoft.Xna.Framework;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.Space;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class DeadCosmoss : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmoss>()][Type] = true;
        Main.tileBlendAll[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<Cosmostone>();
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        MineResist = 2f;
        HitSound = SoundID.Tink;

        AddMapEntry(Color.SlateGray);
    }

    public override void RandomUpdate(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        // set to cosmostone if below sea
        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) <= j && Main.rand.NextBool(250))
        {
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<Cosmostone>();
            WorldGen.SquareTileFrame(i, j);
            NetMessage.SendTileSquare(-1, i, j, 3);
        }

        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) > j)
        {
            WorldGen.SpreadGrass(i, j, ModContent.TileType<DeadCosmoss>(), ModContent.TileType<Cosmoss>(), repeat: false, tile.BlockColorAndCoating());
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            WorldGen.KillTile_MakeTileDust(i, j, Main.tile[i, j]);
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<Cosmostone>();
        }
    }

    public override bool CanExplode(int i, int j)
    {
        WorldGen.KillTile(i, j);

        return true;
    }
}
