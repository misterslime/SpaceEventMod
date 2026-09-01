using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using SpaceEventMod.Common.DataStructures;
using SpaceEventMod.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class Cosmoss : FancyTile, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 100;

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

        MineResist = .5f;
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

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            WorldGen.KillTile_MakeTileDust(i, j, Main.tile[i, j]);
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<Cosmostone>();
        }
    }
}
