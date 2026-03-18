using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class Cosmoss : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;
        Main.tileBlendAll[Type] = true;

        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<Cosmostone>();
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        MineResist = .5f;
        HitSound = SoundID.Tink;

        AddMapEntry(Color.LightCoral);
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Framing.GetTileSafely(i, j);
        if (!TileDrawing.IsVisible(tile))
            return;

        //Color drawColor = WorldGen.paintColor(Framing.GetTileSafely(i, j).TileColor);

        Color drawColor = Lighting.GetColor(i, j);

        Vector2 drawPosition = new Vector2(i * 16, j * 16) - Main.screenPosition;
        if (!Main.drawToScreen)
            drawPosition += new Vector2(Main.offScreenRange);

        Texture2D glowTexture = Assets.Assets.Textures.CellularGrowth.Tiles.Cosmoss_Glow.Value;

        var cosmossShader = Assets.Assets.Shaders.CellularGrowth.CosmossColors.Value;

        cosmossShader.Parameters["colorMap"].SetValue(Assets.Assets.Textures.CellularGrowth.Tiles.Cosmoss_Palette.Value);
        cosmossShader.Parameters["noiseTexture"].SetValue(Assets.Assets.Textures.Noise.Foam.Value);
        cosmossShader.Parameters["sineAmp"].SetValue(0.0075f);
        cosmossShader.Parameters["sineStrength"].SetValue(0.5f);
        cosmossShader.Parameters["verticalSineAmp"].SetValue(0.01f);
        cosmossShader.Parameters["verticalSineStrength"].SetValue(2f);
        cosmossShader.Parameters["noiseScale"].SetValue(5f);
        cosmossShader.Parameters["noiseStrength"].SetValue(1f);
        cosmossShader.Parameters["mixQuantization"].SetValue(3f);
        cosmossShader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 0.15f);
        cosmossShader.Parameters["resolution"].SetValue(glowTexture.Size());
        cosmossShader.Parameters["tilePos"].SetValue(new Vector2(i, j) * 16);
        cosmossShader.Parameters["sourceRect"].SetValue(new Vector4(tile.TileFrameX, tile.TileFrameY, 16, 16));

        SpriteBatchSnapshot snapshot;

        spriteBatch.End(out snapshot);
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, cosmossShader, Main.GameViewMatrix.NormalizedTransformationmatrix);
        spriteBatch.Draw(glowTexture, drawPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), drawColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
        spriteBatch.End();
        spriteBatch.Begin(snapshot);
    }
}
