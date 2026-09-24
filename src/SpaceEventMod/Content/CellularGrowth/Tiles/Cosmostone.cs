using Microsoft.Xna.Framework;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.Space;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;
using static Daybreak.Common.Features.Hooks.GlobalItemHooks;

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
        MineResist = 2f;
        MinPick = 110;

        AddMapEntry(Color.Gray);
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
    {
        WorldGen.TileMergeAttempt(-2, ModContent.TileType<HerbCell>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }

    public override void RandomUpdate(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) > j)
        {
            int air = 0;

            foreach (var connectedPosition in TileDirections.WithCorners)
            {
                var newPosition = new Point(i, j) + connectedPosition;

                if (newPosition.X < 0 || newPosition.X >= Main.maxTilesX ||
                    newPosition.Y < 0 || newPosition.Y >= Main.maxTilesY)
                    continue;

                if (!Main.tile[newPosition].HasTile)
                    air++;
            }

            if (air == 0)
                return;

            TileColorCache tileColor = tile.BlockColorAndCoating();

            WorldGen.SpreadGrass(i, j, ModContent.TileType<Cosmostone>(), ModContent.TileType<Cosmoss>(), repeat: false, tileColor);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, i, j, 3);
        }
    }
}
