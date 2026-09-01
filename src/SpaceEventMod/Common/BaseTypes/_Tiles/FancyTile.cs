using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using static Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace SpaceEventMod.Common.BaseTypes;

internal abstract class FancyTile : ModTile
{
    public List<Point> DrawPoints { get; } = [];
    public Dictionary<int, HashSet<int>> PaintCache { get; } = [];
    public static RenderTargetLease? TileTarget { get; set; }

    /// <summary>
    /// Apply a specified tile shader to the shader target.
    /// </summary>
    protected abstract Effect PrepareTileShader();

    /// <summary>
    /// Specify the texture asset that should be used for tile drawing.
    /// This should match the tile's framing.
    /// </summary>
    protected virtual Asset<Texture2D> GetTextureAsset() => TextureAssets.Tile[Type];

    /// <summary>
    /// Apply settings to the paint shader.
    /// </summary>
    protected virtual (Effect effect, int pass) PreparePaintShader(int paintColor)
    {
        return PaintBatch.PrepareShader(paintColor, TreePaintSystemData.GetTileSettings(-1, 0));
    }

    public sealed override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
    }

    public sealed override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (DrawPoints.Contains(new Point(i, j)))
            return;

        ref var paintIndexList = ref CollectionsMarshal.GetValueRefOrAddDefault(PaintCache, Main.tile[i, j].TileColor, out _);
        paintIndexList ??= new HashSet<int>();
        paintIndexList?.Add(DrawPoints.Count);

        DrawPoints.Add(new Point(i, j));

        if (paintIndexList is null)
            throw new System.Exception("Draw point hashset was null");

        return;
    }

    [ModSystemHooks.PostDrawTiles]
    internal static void RenderShaderedTilesWithPaint()
    {
        using var _ = Main.spriteBatch.Scope();

        TileTarget ??= ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);

        foreach (FancyTile tiles in ModContent.GetContent<FancyTile>())
        {
            // Draw tiles to target
            using (TileTarget.Scope(clearColor: Color.Transparent))
            {
                Main.spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    tiles.PrepareTileShader(),
                    Matrix.Identity);

                foreach ((int i, int j) in tiles.DrawPoints.ToArray())
                    DrawSingleTile(tiles.GetTextureAsset().Value, i, j, Main.spriteBatch);

                Main.spriteBatch.End();
            }

            // Draw tiles with paint masked on.
            // maybe i can make this able to use any kind of tile, not just single tiles?
            foreach (KeyValuePair<int, HashSet<int>> item in tiles.PaintCache)
            {
                if (item.Value.Count == 0)
                    continue;

                var paintShader = tiles.PreparePaintShader(item.Key);

                PaintBatch.instance.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        Main.DefaultSamplerState,
                        DepthStencilState.None,
                        Main.Rasterizer,
                        paintShader.effect,
                        Main.GameViewMatrix.TransformationMatrix,
                        paintShader.pass);

                foreach (var index in item.Value)
                {
                    Point point = tiles.DrawPoints[index];
                    Vector2 drawPosition = new Vector2(point.X * 16, point.Y * 16) - Main.screenPosition;

                    var sourceRect = new Rectangle(point.X * 16, point.Y * 16, 16, 16);
                    var rectangle = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, 16, 16);
                    PaintBatch.instance.Draw(TileTarget.Target, drawPosition, rectangle, Color.White);
                }

                PaintBatch.instance.End();

                item.Value.Clear();
            }

            tiles.DrawPoints.Clear();
        }
    }

    //Adapted and mutilated from TileDrawing.DrawSingleTile
    private static void DrawSingleTile(Texture2D texture, int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        var drawData = new TileDrawInfo
        {
            tileCache = tile
        };
        drawData.typeCache = drawData.tileCache.type;
        drawData.tileFrameX = drawData.tileCache.frameX;
        drawData.tileFrameY = drawData.tileCache.frameY;
        drawData.tileLight = Lighting.GetColor(i, j);
        Main.instance.TilesRenderer.GetTileDrawData(
            i,
            j,
            drawData.tileCache,
            drawData.typeCache,
            ref drawData.tileFrameX,
            ref drawData.tileFrameY,
            out drawData.tileWidth,
            out drawData.tileHeight,
            out drawData.tileTop,
            out drawData.halfBrickHeight,
            out drawData.addFrX,
            out drawData.addFrY,
            out drawData.tileSpriteEffect,
            out drawData.glowTexture,
            out drawData.glowSourceRect,
            out drawData.glowColor
        );
        drawData.drawTexture = texture;

        bool visible = false;
        if (drawData.tileLight.R >= 1 || drawData.tileLight.G >= 1 || drawData.tileLight.B >= 1)
            visible = true;

        if (drawData.tileCache.wall > 0 && (drawData.tileCache.wall == 318 || drawData.tileCache.fullbrightWall()))
            visible = true;

        visible &= TileDrawing.IsVisible(drawData.tileCache);

        if (!visible)
            return;

        Rectangle rectangle = new Rectangle(drawData.tileFrameX + drawData.addFrX, drawData.tileFrameY + drawData.addFrY, drawData.tileWidth, drawData.tileHeight - drawData.halfBrickHeight);
        Vector2 vector = new Vector2((float)(i * 16 - (int)Main.screenPosition.X) - ((float)drawData.tileWidth - 16f) / 2f, j * 16 - (int)Main.screenPosition.Y + drawData.tileTop + drawData.halfBrickHeight);
        drawData.colorTint = Color.White;
        drawData.tileLight = Main.instance.TilesRenderer.DrawTiles_GetLightOverride(i, j, drawData.tileCache, drawData.typeCache, drawData.tileFrameX, drawData.tileFrameY, drawData.tileLight);
        drawData.finalColor = TileDrawing.GetFinalLight(drawData.tileCache, drawData.typeCache, drawData.tileLight, drawData.colorTint);

        Main.instance.TilesRenderer.DrawBasicTile(Main.screenPosition, new(0, 0), i, j, drawData, rectangle, vector);
    }
}
