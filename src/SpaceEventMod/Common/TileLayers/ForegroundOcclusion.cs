using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.CellularGrowth.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.ForegroundOcclusion;

// FIX FLICKERING
internal class ForegroundOcclusion : ModSystem
{
    private static HashSet<Point> s_wallPoints = new HashSet<Point>();
    private static int s_cosmostoneWallType = -1;

    [OnLoad]
    public static void Load() 
    { 
        On_Main.DrawBG += On_Main_DrawBG;
        On_TileLightScanner.ExportTo += On_TileLightScanner_ExportTo;
    }

    public override void PostSetupContent()
    {
        s_cosmostoneWallType = ModContent.WallType<CosmostoneWall>();
    }

    private static void On_Main_DrawBG(On_Main.orig_DrawBG orig, Main self)
    {
        orig(self);

        var player = Main.LocalPlayer;
        var point = player.Center.ToTileCoordinates();
        var tile = Main.tile[point];

        if (s_wallPoints.Contains(point))
        {
            return;
        }

        s_wallPoints.Clear();

        if (tile.WallType == ModContent.WallType<CosmostoneWall>())
        {
            var tiles = TileDirections.SearchFromTile(point.X, point.Y, 5000, TileDirections.WithCorners,
                x => x.WallType == ModContent.WallType<CosmostoneWall>());

            foreach (var t in tiles.tilePositions)
                s_wallPoints.Add(t);
        }
    }

    private static void On_TileLightScanner_ExportTo(On_TileLightScanner.orig_ExportTo orig, TileLightScanner self, Rectangle area, LightMap outputMap, TileLightScannerOptions options)
    {
        orig(self, area, outputMap, options);

        bool inside = s_wallPoints.Count != 0;

        FastParallel.For(area.Left, area.Right, delegate (int start, int end, object context) {
            for (int i = start; i < end; i++)
            {
                for (int j = area.Top; j <= area.Bottom; j++)
                {
                    Point point = new Point(i, j);
                    Vector2 center = new Vector2(i * 16 + 8, j * 16 + 8);

                    if (!s_wallPoints.Contains(point) && Main.tile[point].WallType == s_cosmostoneWallType)
                    {
                        //LightMaskMode tileMask = GetTileMask(Main.tile[i, j]);
                        outputMap.SetMaskAt(i - area.X, j - area.Y, LightMaskMode.Solid);
                        outputMap[i - area.X, j - area.Y] = Vector3.Zero;
                    }
                }
            }
        });
    }
}
