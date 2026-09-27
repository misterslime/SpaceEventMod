using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Physics.Joints;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class CosmossPlants : ModTile
{
    private const int STYLE_RANGE = 6;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileCut[Type] = true;

        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(0, 0);

        TileObjectData.newTile.RandomStyleRange = STYLE_RANGE;
        TileObjectData.newTile.StyleWrapLimit = STYLE_RANGE;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Cosmoss>()];
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
        TileObjectData.addAlternate(STYLE_RANGE);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
        TileObjectData.addAlternate(STYLE_RANGE * 2);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
        TileObjectData.addAlternate(STYLE_RANGE * 3);

        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;

        AddMapEntry(Color.LightCoral);
    }

    public override void RandomUpdate(int i, int j)
    {
        // this is here bc it hates me
        WorldGen.Reframe(i, j);
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var tile = Main.tile[i, j];

        if (resetFrame)
        {
            tile.TileFrameX = (short)(Main.rand.Next(STYLE_RANGE) * 18);
            tile.TileFrameY = 0;
            return false;
        }

        if (ValidSurface(i, j + 1))
            tile.TileFrameY = 0 * 18;
        else if (ValidSurface(i, j - 1))
            tile.TileFrameY = 3 * 18;
        else if (ValidSurface(i + 1, j))
            tile.TileFrameY = 1 * 18;
        else if (ValidSurface(i - 1, j))
            tile.TileFrameY = 2 * 18;
        else
            WorldGen.KillTile(i, j);

        return false;

        bool ValidSurface(int i, int j)
        {
            var connectT = Framing.GetTileSafely(i, j);

            return WorldGen.SolidTile(connectT) && connectT.TileType == ModContent.TileType<Cosmoss>();
        }
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        return false;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];

        if (tile == null || !tile.active() || !TileDrawing.IsVisible(tile))
            return;

        var tileRenderer = Main.instance.TilesRenderer;

        Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
        ushort type = tile.type;
        short tileFrameX = tile.frameX;
        short tileFrameY = tile.frameY;
        tileRenderer.GetTileDrawData(i, j, tile, type, ref tileFrameX, ref tileFrameY, out var tileWidth, out var tileHeight, out var tileTop, out var halfBrickHeight, out var addFrX, out var addFrY, out var tileSpriteEffect, out var glowTexture, out var glowSourceRect, out var glowColor);
        Color tileLight = Lighting.GetColor(i, j);
        tileRenderer.DrawAnimatedTile_AdjustForVisionChangers(i, j, tile, type, tileFrameX, tileFrameY, ref tileLight, Main.rand.Next(4) == 0);
        tileLight = tileRenderer.DrawTiles_GetLightOverride(j, i, tile, type, tileFrameX, tileFrameY, tileLight);

        Vector2 position = new Vector2(i * 16 - (int)unscaledPosition.X + 8, j* 16 - (int)unscaledPosition.Y + 16);
        float windStrength = tileRenderer.GetWindCycle(i, j, tileRenderer._grassWindCounter);
        if (!WallID.Sets.AllowsWind[tile.wall])
            windStrength = 0f;

        if (!tileRenderer.InAPlaceWithWind(i, j, 1, 1))
            windStrength = 0f;

        tileRenderer.GetWindGridPush2Axis(i, j, 20, 0.35f, out var pushX, out var pushY);
        int multX = 0;
        int multY = 0;
        Vector2 origin = new Vector2(tileWidth / 2, 16 - halfBrickHeight - tileTop);

        Texture2D tileDrawTexture = TextureAssets.Tile[type].Value;

        pushX += windStrength;
        pushY += windStrength;

        switch (tileFrameY / 18)
        {
            case 0:
                multX = 1;
                multY = 0;
                origin = new Vector2(tileWidth / 2, 16 - halfBrickHeight - tileTop);
                position.X += 8f;
				position.Y += 18f;
                position.X += pushX;
                position.Y += Math.Abs(pushY);
                break;
            case 3:
                pushX *= -1f;
                multX = -1;
                multY = 0;
                origin = new Vector2(tileWidth / 2, -tileTop);
                position.Y -= 2f;
                position.X += 8f;
                position.X += -pushX;
                position.Y += 0f - Math.Abs(pushY);
                break;
            case 2:
                multX = 0;
                multY = 1;
                origin = new Vector2(0f, (16 - halfBrickHeight - tileTop) / 2);
                position.X -= 2f;
                position.Y += 8f;
                position.Y += pushY;
                position.X += pushX;
                break;
            case 1:
                pushY *= -1f;
                multX = 0;
                multY = -1;
                origin = new Vector2(16f, (16 - halfBrickHeight - tileTop) / 2);
                position.X += 18f;
                position.Y += 8f;
                position.Y += - pushY;
                position.X += pushX;
                break;
        }


        position.Y -= 16f;
        position.X -= 8f;

        windStrength = pushX * multX + pushY * multY;
        spriteBatch.Draw(tileDrawTexture, position, new Rectangle(tileFrameX + addFrX, tileFrameY + addFrY, tileWidth, tileHeight - halfBrickHeight), tileLight, windStrength * 0.1f, origin, 1f, tileSpriteEffect, 0f);
        /*if (glowTexture != null)
            tileRenderer.DrawNatureGlowmask(glowTexture, position, glowSourceRect, glowColor, num3 * 0.1f, new Vector2(tileWidth / 2, 16 - halfBrickHeight - tileTop), 1f, tileSpriteEffect, 0f);*/

    }

    private void GetWindPush(int i, int j, int pushAnimationTimeTotal, float pushForcePerFrame, out float pushX, out float pushY)
    {
        Main.instance.TilesRenderer.Wind.GetWindTime(i, j, pushAnimationTimeTotal, out var windTimeLeft, out var directionX, out var directionY);
        if (windTimeLeft >= pushAnimationTimeTotal / 2)
        {
            pushX = (pushAnimationTimeTotal - windTimeLeft) * pushForcePerFrame * (float)directionX;
            pushY = (pushAnimationTimeTotal - windTimeLeft) * pushForcePerFrame * (float)directionY;
            return;
        }

        pushX = (float)windTimeLeft * pushForcePerFrame * (float)directionX;
        pushY = (float)windTimeLeft * pushForcePerFrame * (float)directionY;

    }
}
