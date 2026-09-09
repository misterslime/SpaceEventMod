using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.BaseTypes;

/// <summary>
/// This class represents a ModTile with a shader applied to it.
/// Will also allow paint to be applied post-shader.
/// </summary>
internal abstract class MaskedTile : ModTile
{
    private Dictionary<int, List<Point>> PaintPointCache { get; } = [];
    private static HashSet<int> PaintIDCache { get; } = [];
    private static RenderTargetLease? PaintMask { get; set; }

    /// <summary>
    /// Apply a specified tile shader to the shader target.
    /// </summary>
    protected virtual Effect PrepareTileShader() => null;

    /// <summary>
    /// Specify the texture asset that should be used for tile drawing.
    /// This should match the tile's framing.
    /// </summary>
    protected virtual Asset<Texture2D> GetTextureAsset() => TextureAssets.Tile[Type];

    /// <summary>
	/// Allows you to draw things behind the masked tile, or to modify the way it is drawn. 
    /// Return false to stop the tile from drawing like a normal 1x1 tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
    protected virtual bool PreDrawMask(int i, int j, SpriteBatch spriteBatch) => true;

    /// <summary>
    /// Allows for additional rendering after the tile is drawn normally.
    /// </summary>
    /// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
    /// <param name="spriteBatch"></param>
    protected virtual void PostDrawMask(int i, int j, SpriteBatch spriteBatch)
    {

    }

    /// <summary>
    /// Allows you to determine whether the tile position should be added to the <c>MaskedTile.PaintPointCache</c> or not. Should be used for multitiles generally.
    /// </summary>
    /// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
    protected virtual bool ShouldDrawShaderedMask(int i, int j) => true;

    public sealed override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (ShouldDrawShaderedMask(i, j))
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
    }

    public sealed override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        int tileColor = Main.tile[i, j].TileColor;
        PaintIDCache.Add(tileColor);

        if (PaintPointCache.ContainsKey(tileColor) && 
            PaintPointCache[tileColor].Contains(new Point(i, j)))
            return;

        ref var paintIndexList = ref CollectionsMarshal.GetValueRefOrAddDefault(PaintPointCache, tileColor, out _);
        paintIndexList ??= new List<Point>();
        paintIndexList?.Add(new Point(i, j));

        if (paintIndexList is null)
            throw new System.Exception("Draw point hashset was null");
    }

    [ModSystemHooks.PostDrawTiles]
    internal static void RenderShaderedTilesWithPaint()
    {
        using var _ = Main.spriteBatch.Scope();

        PaintMask ??= ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);

        int drawCalls = 0;
        foreach (var paintColor in PaintIDCache)
        {
            // draw to paint target
            DrawToPaintTarget(paintColor, Main.spriteBatch, ref drawCalls);

            // draw paint target with paint shader
            var paintShader = PaintBatch.PrepareShader(paintColor, TreePaintSystemData.GetTileSettings(-1, 0));

            PaintBatch.instance.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                paintShader.effect,
                Main.GameViewMatrix.TransformationMatrix,
                paintShader.pass);

            PaintBatch.instance.Draw(PaintMask.Target, Vector2.Zero, null, Color.White);

            drawCalls++;
            PaintBatch.instance.End();
        }

        Main.NewText(String.Format("Drew painted tiles in {0} calls.\n({1} paints, {2} tile types)", drawCalls, PaintIDCache.Count, ModContent.GetContent<MaskedTile>().Count()));

        PaintIDCache.Clear();
    }

    /// <summary>
    /// Draws every cached tile point of a specific tile color to the tile target.
    /// </summary>
    /// <param name="paintColor">Paint/tile color to draw.</param>
    private static void DrawToPaintTarget(int paintColor, SpriteBatch spriteBatch, ref int drawCalls)
    {
        using (PaintMask.Scope(clearColor: Color.Transparent))
        {
            foreach (var tiles in ModContent.GetContent<MaskedTile>())
            {
                if (!tiles.PaintPointCache.TryGetValue(paintColor, out var value))
                    continue;

                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    tiles.PrepareTileShader(),
                    Matrix.Identity);

                foreach (var point in value)
                {
                    if (tiles.PreDrawMask(point.X, point.Y, spriteBatch))
                        DrawSingleTile(tiles.GetTextureAsset().Value, point.X, point.Y, spriteBatch);
                    tiles.PostDrawMask(point.X, point.Y, spriteBatch);
                }

                spriteBatch.End();

                drawCalls++;

                value.Clear();
            }
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
