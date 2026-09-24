using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Content.Space;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class RockObeliskLoader : ModSystem
{
    public static HashSet<int> RockObeliskTiles = new HashSet<int>
    {
        SpaceEventMod.Instance.Find<ModTile>("RockObelisk1x1").Type,
        SpaceEventMod.Instance.Find<ModTile>("RockObelisk1x2").Type,
        SpaceEventMod.Instance.Find<ModTile>("RockObelisk1x3").Type,
        SpaceEventMod.Instance.Find<ModTile>("RockObelisk2x1").Type,
        SpaceEventMod.Instance.Find<ModTile>("RockObelisk2x2").Type,
        SpaceEventMod.Instance.Find<ModTile>("RockObelisk2x3").Type
    };

    public override void Load()
    {
        Mod.AddContent(new RockObelisk("RockObelisk1x1", 1, 1));
        Mod.AddContent(new RockObelisk("RockObelisk1x2", 1, 2));
        Mod.AddContent(new RockObelisk("RockObelisk1x3", 1, 3));
        Mod.AddContent(new RockObelisk("RockObelisk2x1", 2, 1, false));
        Mod.AddContent(new RockObelisk("RockObelisk2x2", 2, 2));
        Mod.AddContent(new RockObelisk("RockObelisk2x3", 2, 3));
    }
}

[Autoload(false)]
internal class RockObelisk : ModTile
{
    private string _internalName;
    private string _tileTexture;

    private int _width;
    private int _height;

    private bool _hasGlow;

    private Asset<Texture2D> _glowTexture;


    public override string Name => _internalName;
    public override string Texture => _tileTexture;

    public RockObelisk(string internalName, int width, int height, bool hasGlow = true)
    {
        this._internalName = internalName;
        this._tileTexture = "SpaceEventMod/Assets/Textures/CellularGrowth/Tiles/" + internalName;
        this._width = width;
        this._height = height;
        this._hasGlow = hasGlow;
    }

    public override void SetStaticDefaults()
    {
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

        Main.tileLavaDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = false;
        Main.tileSolid[Type] = false;

        TileObjectData.newTile.Width = _width;
        TileObjectData.newTile.Height = _height;

        TileObjectData.newTile.CoordinateWidth = 16;

        List<int> coordinateHeights = new List<int>();

        for (int i = 0; i < _height - 1; i++)
        {
            coordinateHeights.Add(16);
        }

        coordinateHeights.Add(18);

        TileObjectData.newTile.CoordinateHeights = coordinateHeights.ToArray();
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(0, _height - 1);
        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.RandomStyleRange = 2; // Each style will occupy 2 placement styles
        TileObjectData.newTile.StyleMultiplier = 2; // 2 random varieties per placement style
        TileObjectData.newTile.StyleWrapLimit = 4; // Wrap to the next line in the texture after 4 placement styles, or 2 styles.

        TileObjectData.addTile(Type);

        HitSound = SoundID.Dig;
        RegisterItemDrop(ItemID.Wood);
        AddMapEntry(new Color(80, 55, 74));

        if (_hasGlow && !Main.dedServ)
            _glowTexture = ModContent.Request<Texture2D>(_tileTexture + "_Glow");
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (!_hasGlow)
            return;

        Tile tile = Framing.GetTileSafely(i, j);
        
        Texture2D glowmask = _glowTexture.Value;
        Vector2 drawPosition = new Vector2(i * 16, j * 16) - Main.screenPosition;

        if (!Main.drawToScreen)
            drawPosition += new Vector2(Main.offScreenRange);

        Color paintColor = WorldGen.paintColor(tile.TileColor);
        Color drawColor = Color.White;

        drawColor.R *= (byte)(paintColor.R / 255);
        drawColor.G *= (byte)(paintColor.G / 255);
        drawColor.B *= (byte)(paintColor.B / 255);
        drawColor.A = 0;

        Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 18);

        Main.spriteBatch.Draw(glowmask, drawPosition, frame, drawColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
    }

    public override void RandomUpdate(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        (int topX, int topY) = TileObjectData.TopLeft(i, j);
        TileObjectData data = TileObjectData.GetTileData(tile.type, 0);

        short styleXWidth = (short)data.CoordinateFullWidth;
        short styleYHeight = (short)data.CoordinateFullHeight;

        bool active = tile.TileFrameY >= styleYHeight;
        bool grown = tile.TileFrameX >= styleXWidth;

        short frameYAdjustment = 0;
        short frameXAdjustment = 0;

        if ((int)(SpaceEvent.Sea.SeaPos.Height.Position / 16f) <= j)
        {
            if (active)
                frameYAdjustment = (short)-styleYHeight;

            if (grown && Main.rand.NextBool(10))
                frameXAdjustment = (short)-styleXWidth;
        }
        else
        {
            if (!active)
                frameYAdjustment = styleYHeight;
        }

        for (int x = topX; x < topX + _width; x++)
        {
            for (int y = topY; y < topY + _height; y++)
            {
                Main.tile[x, y].TileFrameY += frameYAdjustment;
                Main.tile[x, y].TileFrameX += frameXAdjustment;
            }
        }

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, topX, topY, 2, 2);
    }

    public override bool RightClick(int i, int j)
    {
        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
        return true;
    }

    /// <summary>
    /// THIS THING DOESNT CHECK IF ITS THE RIGHT TILE TYPE PLEASE DONT USE THIS EVER PLEASE
    /// </summary>
    internal static void GrowPlants(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        TileObjectData data = TileObjectData.GetTileData(tile.type, 0);

        if (data is null)
            return;

        (int topX, int topY) = TileObjectData.TopLeft(i, j);

        short styleXWidth = (short)data.CoordinateFullWidth;
        bool grown = tile.TileFrameX >= styleXWidth;

        if (grown)
            return;

        short frameXAdjustment = styleXWidth;

        for (int x = topX; x < topX + data.Width; x++)
        {
            for (int y = topY; y < topY + data.Height; y++)
                Main.tile[x, y].TileFrameX += frameXAdjustment;
        }

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, topX, topY, 2, 2);
    }
}
