using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Utilities.Extensions;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class Cosmoss : ModTile, ILoadItem
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

        Texture2D glowTexture = Assets.Textures.CellularGrowth.Tiles.Cosmoss_Glow.Asset.Value;

        var cosmossShader = Assets.Shaders.CellularGrowth.CosmossColors.Asset.Value;

        cosmossShader.Parameters["colorMap"].SetValue(Assets.Textures.CellularGrowth.Tiles.Cosmoss_Palette.Asset.Value);
        cosmossShader.Parameters["noiseTexture"].SetValue(Assets.Textures.Noise.Foam.Asset.Value);
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

        Matrix viewMatrix = Main.GameViewMatrix.TransformationMatrix * Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);

        SpriteBatchSnapshot snapshot;

        spriteBatch.End(out snapshot);
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, cosmossShader, viewMatrix);
        spriteBatch.Draw(glowTexture, drawPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), drawColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
        spriteBatch.End();
        spriteBatch.Begin(snapshot);
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
