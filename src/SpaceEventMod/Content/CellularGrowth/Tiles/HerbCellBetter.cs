using Daybreak.Common.Rendering;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.Utilities;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

public class HerbCellTileEntity : ModTileEntity
{
    public float Rotation { get; set; }

    public override bool IsTileValidForEntity(int x, int y)
    {
        Main.NewText("uwu");
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<HerbCellBetter>();
    }

    // Tile Entities can store data. This data most likely needs to be synced to connected clients.
    public override void SaveData(TagCompound tag)
    {
        tag[nameof(Rotation)] = Rotation;
    }

    public override void LoadData(TagCompound tag)
    {
        Rotation = tag.GetFloat(nameof(Rotation));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Rotation);
    }

    public override void NetReceive(BinaryReader reader)
    {
        Rotation = reader.ReadSingle();
    }

    public override void Update()
    {
    }

    public void SyncTileEntity()
    {
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
    }
}

internal class HerbCellBetter : ModTile, ILoadItem
{
    private const int ONE_BY_TWO = 0;
    private const int TWO_BY_TWO = 34;
    private const int THREE_BY_THREE = 68;

    private static readonly Dictionary<int, Point16> s_widthsHeights = [];
    private static readonly Dictionary<int, int> s_variants = [];
    private static List<Point16> s_herbCellRootPositions = [];

    public void SetItemDefaults(ModItem modItem) => modItem.Item.value = 3000;

    public override void Load()
    {
        base.Load();

        LightingEngine.AfterTiles += DrawHerbCellRoots;

        s_widthsHeights.Add(ONE_BY_TWO, new Point16(16, 32));
        s_variants.Add(ONE_BY_TWO, 5);

        s_widthsHeights.Add(TWO_BY_TWO, new Point16(32, 32));
        s_variants.Add(TWO_BY_TWO, 1);

        s_widthsHeights.Add(THREE_BY_THREE, new Point16(48, 48));
        s_variants.Add(THREE_BY_THREE, 2);
    }

    public override void Unload()
    {
        base.Unload();

        LightingEngine.AfterTiles -= DrawHerbCellRoots;
    }

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        DustType = -1;

        TileID.Sets.DrawTileInSolidLayer[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<HerbCellTileEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(170, 91, 68));
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        return false;
    }

    private HerbCellTileEntity? Entity(int i, int j)
    {
        if (TileEntity.TryGet(i, j, out HerbCellTileEntity tileEntity))
        {
            return tileEntity;
        }

        return null;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly && !fail && Entity(i, j) is HerbCellTileEntity entity)
            entity.Kill(i, j);
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (Entity(i, j) is HerbCellTileEntity entity)
            entity.Kill(i, j);
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        WeightedRandom<int> randomSize = new WeightedRandom<int>();

        randomSize.Add(ONE_BY_TWO, 1);
        randomSize.Add(TWO_BY_TWO, 0.67);
        randomSize.Add(THREE_BY_THREE, 0.5);

        int result = randomSize.Get();

        Main.tile[i, j].TileFrameY = (short)result;
        Main.tile[i, j].TileFrameX = (short)((s_widthsHeights[result].X + 2) * Main.rand.Next(s_variants[result]));

        if (Entity(i, j) is HerbCellTileEntity entity)
        {
            float top = Main.tile[i, j - 1].HasTile && Main.tileSolid[Main.tile[i, j - 1].TileType] ? 1f : 0f;
            float bottom = Main.tile[i, j + 1].HasTile && Main.tileSolid[Main.tile[i, j + 1].TileType] ? 1f : 0f;
            float left = Main.tile[i - 1, j].HasTile && Main.tileSolid[Main.tile[i - 1, j].TileType] ? 1f : 0f;
            float right = Main.tile[i + 1, j].HasTile && Main.tileSolid[Main.tile[i + 1, j].TileType] ? 1f : 0f;

            Vector2 normal = new Vector2(2f * (right - left), 2f * (bottom - top)).SafeNormalize(Vector2.Zero);
            float rotation = normal.RotatedByRandom(MathHelper.PiOver4 * 0.5f).ToRotation();

            if (top == 0f && bottom == 0f && left == 0f && right == 0f)
                rotation = Main.rand.NextVector2CircularEdge(1, 1).ToRotation();




            Main.tile[i, j].slope(AngleToByte(rotation));
            entity.Rotation = rotation;
            entity.SyncTileEntity();
        }
    }

    private byte AngleToByte(float angle)
    {
        angle /= MathHelper.TwoPi;
        int angleInt = (int)MathF.Floor(angle * 255);
        return (byte)angleInt;
    }

    private float ByteToAngle(byte data)
    {
        float angle = data / 255f;
        return angle * MathHelper.TwoPi;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
        return false;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (Entity(i, j) is HerbCellTileEntity entity)
        {
            var tile = Main.tile[i, j];

            var texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);

            //var texture = TextureAssets.Tile[Type].Value;

            Vector2 position = new Vector2(i, j).ToWorldCoordinates() - Main.screenPosition;

            var dimensions = s_widthsHeights[tile.TileFrameY];

            Rectangle source = new Rectangle(tile.TileFrameX, tile.TileFrameY, dimensions.X, dimensions.Y);

            Vector2 origin = source.Size();
            origin.X *= 0.5f;
            origin.Y -= 16;

            float rotation = ByteToAngle(tile.slope());

            spriteBatch.Draw(texture, position, source, Color.White, entity.Rotation - MathHelper.PiOver2, origin, 1, default, 0);
            //spriteBatch.Draw(Assets.Textures.WhitePixel.Asset.Value, position, null, Color.White, entity.Rotation - MathHelper.PiOver2, Vector2.One * 0.5f, 20, default, 0);
        }

        s_herbCellRootPositions.Add(new Point16(i, j));
    }

    private static void DrawHerbCellRoots(object? sender, DrawEventArgs e)
    {
        if (s_herbCellRootPositions.Count == 0)
            return;

        using (var _ = e.SpriteBatch.Scope())
        {
            var transform = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);

            // there is SO a simpler way to do this
            var scale = (new Vector2(Main.screenWidth, Main.screenHeight).Transform(transform) - Vector2.Zero.Transform(transform)) / new Vector2(Main.screenWidth, Main.screenHeight);

            var shaderPass = Assets.Shaders.Fragment.TileStencil.CreatePass1();

            shaderPass.Parameters.TileTarget = Main.instance.tileTarget;
            shaderPass.Parameters.TileTargetSize = Main.instance.tileTarget.Size();
            shaderPass.Parameters.TileTargetPosition = Main.sceneTilePos;

            shaderPass.Parameters.TileTarget2 = Main.instance.tile2Target;
            shaderPass.Parameters.TileTarget2Size = Main.instance.tile2Target.Size();
            shaderPass.Parameters.TileTarget2Position = Main.sceneTile2Pos;

            shaderPass.Parameters.ScreenPosition = Main.screenPosition;
            shaderPass.Parameters.ScreenZoomRatio = Vector2.One;

            shaderPass.Apply();

            e.SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, shaderPass.Shader, Matrix.Identity);


            var rootsSource = new Rectangle(240, 0, 48, 48);
            var rootsOrigin = rootsSource.Size() * 0.5f;

            foreach (var position in s_herbCellRootPositions)
            {
                var texture = Main.instance.TilesRenderer.GetTileDrawTexture(Main.tile[position], position.X, position.Y);
                e.SpriteBatch.Draw(texture, position.ToWorldCoordinates() - Main.screenPosition, rootsSource, Color.White, 0f, rootsOrigin, 1.5f, 0f, 0f);

            }

            e.SpriteBatch.End();
            s_herbCellRootPositions.Clear();
        }
    }
}
