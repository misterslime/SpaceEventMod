using Microsoft.Xna.Framework;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class ActiveFunnyTile : ModTile, ILoadItem
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[Type][ModContent.TileType<Cosmostone>()] = true;
        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(255, 221, 0));

        DustType = DustID.Stone;
        HitSound = SoundID.Tink;
        MineResist = 2f;
        MinPick = 110;
    }

    public override void RandomUpdate(int i, int j)
    {
        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) <= j)
            return;

        var adjacentTiles = TileDirections.SearchFromTile(i, j, 7, TileDirections.WithCorners,
                x => x.HasTile && x.TileType == ModContent.TileType<ActiveFunnyTile>());

        foreach (var tile in adjacentTiles.tilePositions)
            TryActuate(tile.X, tile.Y);
    }

    public override void HitWire(int i, int j)
    {
        TryActuate(i, j);
    }

    private bool TryActuate(int i, int j)
    {
        HashSet<int> preventActuation = [21, 467, 26, 77, 88, 470, 475, 237, 597, 441, 468];

        // !TileID.Sets.PreventsActuationUnder[Main.tile[i, j - 1].type])
        if (Main.tile[i, j - 1] != null && (!Main.tile[i, j - 1].active() || !preventActuation.Contains(Main.tile[i, j - 1].type)) && WorldGen.CanKillTile(i, j))
        {
            Main.tile[i, j].type = (ushort)ModContent.TileType<InactiveFunnyTile>();
            WorldGen.SquareTileFrame(i, j);
            NetMessage.SendTileSquare(-1, i, j);

            return true;
        }

        return false;
    }
}

internal class InactiveFunnyTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[Type][ModContent.TileType<Cosmostone>()] = true;
        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<ActiveFunnyTile>()] = true;
        Main.tileMerge[ModContent.TileType<ActiveFunnyTile>()][Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(155, 116, 0));

        DustType = DustID.Stone;
        HitSound = SoundID.Tink;
        MineResist = 2f;
        MinPick = 110;
    }

    public override void HitWire(int i, int j)
    {
        TryActuate(i, j);
    }

    public override void RandomUpdate(int i, int j)
    {
        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) > j)
            return;

        var adjacentTiles = TileDirections.SearchFromTile(i, j, 7, TileDirections.WithCorners,
                x => x.HasTile && x.TileType == ModContent.TileType<InactiveFunnyTile>());

        foreach (var tile in adjacentTiles.tilePositions)
            TryActuate(tile.X, tile.Y);
    }

    private void TryActuate(int i, int j)
    {
        Main.tile[i, j].type = (ushort)ModContent.TileType<ActiveFunnyTile>();
        WorldGen.SquareTileFrame(i, j);
        NetMessage.SendTileSquare(-1, i, j);
    }
}
