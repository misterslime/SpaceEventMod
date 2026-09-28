using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using SpaceEventMod.Common.DataStructures;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.Space;
using SpaceEventMod.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class Cosmoss : FancyTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;
        Main.tileBlendAll[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<Cosmostone>();
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        MineResist = 2f;
        HitSound = SoundID.Tink;

        AddMapEntry(Color.LightCoral);
    }

    protected override Asset<Texture2D> GetTextureAsset() => Assets.Textures.CellularGrowth.Tiles.Cosmoss_Glow.Asset;

    protected override Effect PrepareTileShader()
    {
        var effect = Assets.Shaders.CellularGrowth.CosmossColors.CreatePass1();

        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight);
        var correctScreenTopLeft = screenCenter - worldViewDimensions / 2f;

        effect.Parameters.colorMap = Assets.Textures.CellularGrowth.Tiles.Cosmoss_Palette.Asset.Value;
        effect.Parameters.sineAmp = 0.005f;
        effect.Parameters.sineStrength = 1f;
        effect.Parameters.verticalSineAmp = 0.0125f;
        effect.Parameters.verticalSineStrength = 0.5f;
        effect.Parameters.uTime = Main.GlobalTimeWrappedHourly * 0.15f;
        effect.Parameters.screenPos = correctScreenTopLeft;
        effect.Parameters.worldViewDimensions = worldViewDimensions;

        effect.Apply();

        return effect.Shader;
    }

    public override void RandomUpdate(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) <= j)
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

            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<Cosmostone>();
            WorldGen.SquareTileFrame(i, j);
            NetMessage.SendTileSquare(-1, i, j, 3);

            return;
        }

        AttemptMossSpread(tile, i, j);
        AttemptGrowPlant(i, j);
    }

    private void AttemptMossSpread(Tile tile, int i, int j)
    {
        int grassType = tile.TileType;
        TileColorCache tileColor = tile.BlockColorAndCoating();
        bool grassHasSpread = false;
        for (int i2 = i - 1; i2 <= i + 1; i2++)
        {
            for (int j2 = j - 1; j2 <= j + 1; j2++)
            {
                Tile neighbor = Main.tile[i2, j2];
                if ((i == i2 && j == j2) || !neighbor.HasTile)
                    continue;

                int air = 0;

                foreach (var connectedPosition in TileDirections.WithCorners)
                {
                    var newPosition = new Point(i2, j2) + connectedPosition;

                    if (newPosition.X < 0 || newPosition.X >= Main.maxTilesX ||
                        newPosition.Y < 0 || newPosition.Y >= Main.maxTilesY)
                        continue;

                    if (!Main.tile[newPosition].HasTile)
                        air++;
                }

                if (air == 0)
                    continue;

                if (neighbor.TileType == ModContent.TileType<Cosmostone>())
                {
                    WorldGen.SpreadGrass(i2, j2, ModContent.TileType<Cosmostone>(), grassType, repeat: false, tileColor);

                    if (neighbor.TileType != grassType)
                        continue;

                    WorldGen.SquareTileFrame(i2, j2);
                    grassHasSpread = true;
                }
                else if (neighbor.TileType == ModContent.TileType<Cosmoss>())
                    WorldGen.SpreadGrass(i2, j2, ModContent.TileType<Cosmoss>(), grassType, repeat: false, tileColor);
                else if (RockObeliskLoader.RockObeliskTiles.Contains(neighbor.TileType))
                    RockObelisk.GrowPlants(i2, j2);
            }
        }

        if (Main.netMode == NetmodeID.Server && grassHasSpread)
        {
            NetMessage.SendTileSquare(-1, i, j, 3);
        }
    }

    private void AttemptGrowPlant(int i, int j)
    {
        if (Main.tile[i, j].BlockType != BlockType.Solid)
            return;

        List<Point> validPlacePositions = new();
        var current = new Point(i, j);

        for (int i2 = 0; i2 < TileDirections.NoCorners.Length; i2++)
        {
            Point point = current + TileDirections.NoCorners[i2];
            Tile tile = Framing.GetTileSafely(point);

            if (!tile.HasTile)
                validPlacePositions.Add(TileDirections.NoCorners[i2]);
        }

        if (validPlacePositions.Count == 0)
            return;

        Point newTileDirection = Main.rand.Next(validPlacePositions);
        Point newTilePosition = newTileDirection + current;

        List<(int Type, int RandomStyles)> types = [ (ModContent.TileType<CosmossPlants>(), 6) ];

        if (newTileDirection == TileDirections.NoCorners[2] || newTileDirection == TileDirections.NoCorners[3]) // left and right
            types.Add((ModContent.TileType<HorizontalPlants>(), 6));
        else if (newTileDirection == TileDirections.NoCorners[1])
            types.Add((ModContent.TileType<HangingPlants>(), 3));
        else if (newTileDirection == TileDirections.NoCorners[0])
            types.Add((ModContent.TileType<TallPlants>(), 16));



        var type = Main.rand.Next(types);

        var successful = WorldGen.PlaceObject(newTilePosition.X, newTilePosition.Y, type.Type, style: Main.rand.Next(type.RandomStyles), mute: true);
        WorldGen.Reframe(newTilePosition.X, newTilePosition.Y);

        if (Main.netMode == NetmodeID.Server && Main.tile[newTilePosition].TileType == type.Type)
            NetMessage.SendTileSquare(-1, newTilePosition.X, newTilePosition.Y, 3);
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
