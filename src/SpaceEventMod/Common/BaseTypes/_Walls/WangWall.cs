using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.BaseTypes;

internal abstract class WangWall : ModWall
{
    private static Dictionary<(bool TopLeft, bool TopRight, bool BottomRight, bool BottomLeft), Rectangle> s_frames = new Dictionary<(bool TopLeft, bool TopRight, bool BottomRight, bool BottomLeft), Rectangle>
    {
        { (true, false, false, false), new Rectangle(0, 0, 16, 16) },
        { (false, true, false, false), new Rectangle(18, 0, 16, 16) },
        { (false, false, true, false), new Rectangle(18 * 2, 0, 16, 16) },
        { (false, false, false, true), new Rectangle(18 * 3, 0, 16, 16) },
        
        { (true, false, false, true), new Rectangle(0, 18, 16, 16) },
        { (true, true, false, false), new Rectangle(18, 18, 16, 16) },
        { (false, true, true, false), new Rectangle(18 * 2, 18, 16, 16) },
        { (false, false, true, true), new Rectangle(18 * 3, 18, 16, 16) },
        
        { (false, true, true, true), new Rectangle(0, 18 * 2, 16, 16) },
        { (true, false, true, true), new Rectangle(18, 18 * 2, 16, 16) },
        { (true, true, false, true), new Rectangle(18 * 2, 18 * 2, 16, 16) },
        { (true, true, true, false), new Rectangle(18 * 3, 18 * 2, 16, 16) },
        
        { (true, false, true, false), new Rectangle(0, 18 * 3, 16, 16) },
        { (false, true, false, true), new Rectangle(18, 18 * 3, 16, 16) },
        
        { (true, true, true, true), new Rectangle(18 * 3, 18 * 3, 16, 16) },
        { (false, false, false, false), new Rectangle(18 * 2, 18 * 3, 16, 16) }
    };

    private static Dictionary<int, Asset<Texture2D>> s_textures = [];

    private static Dictionary<int, float> s_depth = [];

    private static Dictionary<int, int> s_variants = [];

    /// <summary>
    /// How many variants the wall has.
    /// </summary>
    protected virtual int Variants => 1;

    /// <summary>
    /// Should it explicitly always draw behind other walls? Use this to get free merge frames.
    /// </summary>
    protected virtual float Depth => 0f;

    public override void SetStaticDefaults()
    {
        s_depth.Add(Type, Depth);
        s_variants.Add(Type, Variants);

        SetWallDefaults();
    }

    public abstract void SetWallDefaults();

    public sealed override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

    [OnLoad]
    internal static void LoadHook()
    {
        LightingEngine.PreDrawWalls += DrawWangDualGridWalls;
    }

    private static void DrawWangDualGridWalls(object? sender, DrawEventArgs e)
    {
        Vector2 offset = Vector2.Zero;

        if (!Main.drawToScreen)
            offset = new Vector2(Main.offScreenRange);

        List<(Tile Wall, Point WallPosition, Rectangle Source, Rectangle Destination, float DepthAddition)> framesRectanglesThingsToDraw = new();

        int draws = 0;

        for (var i = -2 + (int)Main.screenPosition.X / 16; i <= 2 + (int)(Main.screenPosition.X + Main.screenWidth) / 16; i++)
        {
            for (var j = -2 + (int)Main.screenPosition.Y / 16; j <= 2 + (int)(Main.screenPosition.Y + Main.screenHeight) / 16; j++)
            {
                if (!WorldGen.InWorld(i, j))
                    continue;

                int variantNumber = new Point(i, j * 2).GetHashCode();

                Tile[] wallTypes = [Main.tile[i, j], Main.tile[i + 1, j], Main.tile[i, j + 1], Main.tile[i + 1, j + 1]];
                Point[] wallPositions = [new Point(i, j), new Point(i + 1, j), new Point(i, j + 1), new Point(i + 1, j + 1)];

                // this is genuinely bullshit ngl
                for (int k = 0; k < wallTypes.Length; k++)
                {
                    if (!s_depth.ContainsKey(wallTypes[k].WallType))
                        continue;

                    //rectangle variable
                    Rectangle rect = new Rectangle(k % 2, (int)Math.Floor(k / 2.0), 1, 1);

                    // canexpandto will check based on width and height of rectangle
                    if (k % 2 == 1 && CanExpandRectangleLeft(k, rect, ref wallTypes)) // attempt expand left
                        rect = new Rectangle(rect.X - 1, rect.Y, rect.Width + 1, rect.Height);

                    if (k % 2 == 0 && CanExpandRectangleRight(k, rect, ref wallTypes)) // attempt expand right
                        rect = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height);

                    if (k - 2 >= 0 && CanExpandRectangleUp(k, rect, ref wallTypes)) // attempt expand up
                        rect = new Rectangle(rect.X, rect.Y - 1, rect.Width, rect.Height + 1);

                    if (k + 2 <= 4 && CanExpandRectangleDown(k, rect, ref wallTypes)) // attempt expand down
                        rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + 1);

                    var topRight = Main.tile[i + 1, j];
                    var topLeft = Main.tile[i, j];
                    var botRight = Main.tile[i + 1, j + 1];
                    var botLeft = Main.tile[i, j + 1];

                    var sourceRect = s_frames[(
                        topLeft.WallType == wallTypes[k].WallType,
                        topRight.WallType == wallTypes[k].WallType,
                        botRight.WallType == wallTypes[k].WallType,
                        botLeft.WallType == wallTypes[k].WallType)];

                    var target = new Rectangle((int)(i * 16 - Main.screenPosition.X + offset.X + 8 + rect.X * 8), (int)(j * 16 - Main.screenPosition.Y + offset.Y + 8 + rect.Y * 8), rect.Width * 8, rect.Height * 8);

                    sourceRect = new Rectangle(sourceRect.X + rect.X * 8, sourceRect.Y + rect.Y * 8, rect.Width * 8, rect.Height * 8);

                    sourceRect.X += variantNumber % s_variants[wallTypes[k].WallType] * 72;

                    framesRectanglesThingsToDraw.Add((wallTypes[k], wallPositions[k], sourceRect, target, 0.01f * k));
                    draws++;
                }

                if (framesRectanglesThingsToDraw.Count == 0)
                    continue;

                foreach (var frames in framesRectanglesThingsToDraw)
                {
                    var texture = Main.instance.WallsRenderer.GetTileDrawTexture(frames.Wall, frames.WallPosition.X, frames.WallPosition.Y);

                    e.SpriteBatch.Draw(texture, frames.Destination, frames.Source, Color.White, 0f, Vector2.Zero, 0, s_depth[frames.Wall.WallType] + frames.DepthAddition);

                }
                framesRectanglesThingsToDraw.Clear();

            }
        }

        Main.NewText(draws);

        // this is genuinely bullshit ngl
        bool CanExpandRectangleLeft(int startIndex, Rectangle rectangle, ref Tile[] wallTypes)
        {
            if (startIndex % 2 != 1)
                return false;

            if (rectangle.Height == 2)
            {
                return CanExpandToType(wallTypes[startIndex], wallTypes[0]) && CanExpandToType(wallTypes[startIndex], wallTypes[2]);
            }
            else
            {
                return CanExpandToType(wallTypes[startIndex], startIndex == 1 ? wallTypes[0] : wallTypes[2]);
            }
        }

        bool CanExpandRectangleRight(int startIndex, Rectangle rectangle, ref Tile[] wallTypes)
        {
            if (startIndex % 2 != 0)
                return false;

            if (rectangle.Height == 2)
            {
                return CanExpandToType(wallTypes[startIndex], wallTypes[1]) && CanExpandToType(wallTypes[startIndex], wallTypes[3]);
            }
            else
            {
                return CanExpandToType(wallTypes[startIndex], startIndex == 0 ? wallTypes[1] : wallTypes[3]);
            }
        }

        bool CanExpandRectangleUp(int startIndex, Rectangle rectangle, ref Tile[] wallTypes)
        {
            if (startIndex - 2 < 0)
                return false;

            if (rectangle.Width == 2)
            {
                return CanExpandToType(wallTypes[startIndex], wallTypes[0]) && CanExpandToType(wallTypes[startIndex], wallTypes[1]);
            }
            else
            {
                return CanExpandToType(wallTypes[startIndex], startIndex == 2 ? wallTypes[0] : wallTypes[1]);
            }
        }

        bool CanExpandRectangleDown(int startIndex, Rectangle rectangle, ref Tile[] wallTypes)
        {
            if (startIndex + 2 >= 4)
                return false;

            if (rectangle.Width == 2)
            {
                return CanExpandToType(wallTypes[startIndex], wallTypes[2]) && CanExpandToType(wallTypes[startIndex], wallTypes[3]);
            }
            else
            {
                return CanExpandToType(wallTypes[startIndex], startIndex == 1 ? wallTypes[2] : wallTypes[3]);
            }
        }

        bool CanExpandToType(Tile thisType, Tile toType)
        {
            return thisType.WallType == toType.WallType || !s_textures.ContainsKey(toType.WallType) || s_depth[toType.WallType] != s_depth[thisType.WallType];
        }
    }
}
