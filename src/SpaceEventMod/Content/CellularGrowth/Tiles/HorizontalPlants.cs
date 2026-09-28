using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

// I FUCKING HATE TILEOBJECTDATA I FUCKING HATE TILEOBJECTDATA I FUCKING HATE TILEOBJECTDATA
internal class HorizontalPlants : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileCut[Type] = true;

        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.CoordinateWidth = 20;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.StyleMultiplier = 6;
        TileObjectData.newTile.RandomStyleRange = 6;
        TileObjectData.newTile.StyleWrapLimit = 6;

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Cosmoss>()];
        TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
        TileObjectData.addAlternate(6);

        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;

        AddMapEntry(Color.LightCoral);
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var tile = Main.tile[i, j];

        if (resetFrame)
        {
            tile.TileFrameY = (short)(Main.rand.Next(6) * 18);
        }

        if (ValidSurface(i + 1, j))
            tile.TileFrameX = 0;
        else if (ValidSurface(i - 1, j))
            tile.TileFrameX = 20;
        else
            WorldGen.KillTile(i, j);

        return false;

        bool ValidSurface(int i, int j)
        {
            var checkTile = Framing.GetTileSafely(i, j);

            return WorldGen.SolidTile(checkTile) && checkTile.TileType == ModContent.TileType<Cosmoss>();
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

        Vector2 position = new Vector2(i * 16 - (int)unscaledPosition.X + 8, j * 16 - (int)unscaledPosition.Y + 16);
        float windStrength = tileRenderer.GetWindCycle(i, j, tileRenderer._grassWindCounter);
        if (!WallID.Sets.AllowsWind[tile.wall])
            windStrength = 0f;

        if (!tileRenderer.InAPlaceWithWind(i, j, 1, 1))
            windStrength = 0f;

        tileRenderer.GetWindGridPush2Axis(i, j, 20, 0.35f, out var pushX, out var pushY);
        int multX = 0;
        int multY = 0;
        Vector2 origin = new Vector2(tileWidth / 2, 18 - halfBrickHeight - tileTop);

        Texture2D tileDrawTexture = TextureAssets.Tile[type].Value;

        pushX += windStrength;
        pushY += windStrength;

        switch (tileFrameX / 20)
        {
            case 1:
                multX = 0;
                multY = 1;
                origin = new Vector2(0f, 16 / 2);
                position.Y += 8f;
                position.Y += pushY;
                position.X += pushX;
                break;
            case 0:
                pushX *= -1f;
                multX = 0;
                multY = -1;
                origin = new Vector2(18f, 16 / 2);
                position.X += 16;
                position.Y += 8f;
                position.Y += -pushY;
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
}
