using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.CellularGrowth.Walls;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.ForegroundOcclusion;

// step 1: make everything that doesnt have a cosmostone wall not draw if the player is in front of cosmostone walls
// step 2: make air that has cosmostone wall draw with cosmostone tile + framing if the player is NOT in front of cosmostone walls
// step 3: make actual cosmostone tiles merge with the cosmostone wall air unless player is in front of cosmostone wall
// step 4: lighting
internal class ForegroundOcclusion : ModSystem
{
    private static HashSet<Point> s_wallPoints = new HashSet<Point>();
    private static float s_colorLerpAmount = 0f;

    [OnLoad]
    public static void Load() 
    { 
        On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
        On_TileLightScanner.ExportTo += On_TileLightScanner_ExportTo;
    }

    public override void PostUpdatePlayers()
    {
        var player = Main.LocalPlayer;
        var point = player.Center.ToTileCoordinates();
        var tile = Main.tile[point];
        var lerpAmount = 0.995f;

        if (s_wallPoints.Contains(point))
        {
            s_colorLerpAmount = MathHelper.Lerp(s_colorLerpAmount, 1f, lerpAmount);
            return;
        }

        s_wallPoints.Clear();
        s_colorLerpAmount = MathHelper.Lerp(s_colorLerpAmount, 0f, lerpAmount);

        if (tile.WallType == ModContent.WallType<CosmostoneWall>())
        {
            var tiles = TileDirections.SearchFromTile(point.X, point.Y, 5000, TileDirections.WithCorners,
                x => x.WallType == ModContent.WallType<CosmostoneWall>());

            foreach (var t in tiles.tilePositions)
                s_wallPoints.Add(t);
        }
    }

    private static void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (var i = -2 + (int)Main.screenPosition.X / 16; i <= 2 + (int)(Main.screenPosition.X + Main.screenWidth) / 16; i++)
        {
            for (var j = -2 + (int)Main.screenPosition.Y / 16; j <= 2 + (int)(Main.screenPosition.Y + Main.screenHeight) / 16; j++)
            {
                Point point = new Point(i, j);

                if (!WorldGen.InWorld(i, j) || !s_wallPoints.Contains(point))
                    continue;

                var target = new Rectangle((int)(i * 16 - Main.screenPosition.X), (int)(j * 16 - Main.screenPosition.Y), 16, 16);
                var tex = Assets.Textures.WhitePixel.Asset.Value;

                //Main.spriteBatch.Draw(tex, target, null, Color.Gold * 0.25f);
            }
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }

    private static void On_TileLightScanner_ExportTo(On_TileLightScanner.orig_ExportTo orig, TileLightScanner self, Rectangle area, LightMap outputMap, TileLightScannerOptions options)
    {
        orig(self, area, outputMap, options);

        bool inside = s_wallPoints.Count != 0;

        FastParallel.For(area.Left, area.Right, (ParallelForAction)delegate (int start, int end, object context)
        {
            for (int i = start; i < end; i++)
            {
                for (int j = area.Top; j <= area.Bottom; j++)
                {
                    Point point = new Point(i, j);
                    Vector2 center = new Vector2(i * 16 + 8, j * 16 + 8);
                    Vector3 original = outputMap[i - area.X, j - area.Y];

                    if (!s_wallPoints.Contains(point) && Main.tile[point].WallType == ModContent.WallType<CosmostoneWall>())
                    {
                        outputMap.SetMaskAt(i - area.X, j - area.Y, LightMaskMode.Solid);
                        outputMap[i - area.X, j - area.Y] = Vector3.Zero;
                    }

                    if (false && inside && !s_wallPoints.Contains(point))
                    {
                        //LightMaskMode tileMask = self.GetTileMask(Main.tile[point]);
                        outputMap.SetMaskAt(i - area.X, j - area.Y, LightMaskMode.Solid);
                        //self.GetTileLight(point.X, point.Y, out var outputColor);
                        outputMap[i - area.X, j - area.Y] = Vector3.Zero;
                    }
                }
            }
        });
    }
}
