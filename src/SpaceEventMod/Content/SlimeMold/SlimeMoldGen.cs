using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Mathematics;
using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Daybreak.Common.Features.Hooks.ModifyItemDrawBasics;

namespace SpaceEventMod.Content.SlimeMold;

public class SlimeMoldGen : ModSystem
{
    public static Line[] _connectiveCells;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int islandsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tile Cleanup"));

        if (islandsIndex != -1)
        {
            tasks.Insert(islandsIndex - 1, new SlimeMoldPass("SlimeMold", 100f));
        }
    }
}

internal class SlimeMoldPass : GenPass
{
    public SlimeMoldPass(string name, float loadWeight) : base(name, loadWeight)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        int rockLayer = (int)(Main.maxTilesX * 0.4f);

        int tries = 0;
        for (int i = 0; i < (Main.maxTilesX / 2400) + 2; i++)
        {
            int x = WorldGen.genRand.Next(300, Main.maxTilesX - 300);
            int y = WorldGen.genRand.Next((int)Main.rockLayer + 100, Main.maxTilesY - 500);
            int size = (int)(WorldGen.genRand.Next(160, 200) * (Main.maxTilesX / 6400f));

            Rectangle rectangle = new Rectangle(x, y, (int)(size * 0.5f), size);
            if (!SpawnBiome(rectangle) && tries++ < 999)
            {
                i--;
            }
        }
    }

    private bool SpawnBiome(Rectangle r)
    {
        Vector2 top = new Vector2(0.5f, -0.2f);
        Vector2 middle = Vector2.One * 0.5f;

        FastNoiseLite noise = new FastNoiseLite(WorldGen.genRand.Next());
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
        noise.SetDomainWarpAmp(12f);

        float scale = 6;
        float deformScale = 1f;

        // Carve/Shape
        for (int i = r.Left - 60; i < r.Right + 60; i++)
        {
            for (int j = r.Top - 60; j < r.Bottom + 60; j++)
            {
                //WorldGen.World.Tiles[i, j] = TileTypes.Mud;

                Vector2 peeb = new Vector2(i - r.Left, j - r.Top);
                peeb /= new Vector2(r.Width, r.Height);

                var sdf = SdfShapes.Segment(peeb, middle, top).Distance;

                sdf += noise.GetNoise(i * scale, j * scale * 0.5f) * 0.2f;

                float deform = MathF.Abs(noise.GetNoise(i * deformScale, j * deformScale)) * 0.4f;

                float topDeform = noise.GetNoise(i * deformScale, r.Top * deformScale) * 20 + 20;

                if (j > r.Top - topDeform && sdf < 0.5 + deform)
                {
                    TryPlaceTile(i, j, TileID.Mud);
                    TryPlaceWall(i, j, WallID.MudUnsafe);
                }

                if (sdf <= 0.3f)
                    WorldGen.KillTile(i, j);
            }
        }

        // Grow slime mold grass
        for (int i = r.Left - 60; i < r.Right + 60; i++)
        {
            for (int j = r.Top - 60; j < r.Bottom + 60; j++)
            {
                //WorldGen.World.Tiles[i, j] = TileTypes.Mud;

                if (Main.tile[i, j].TileType != TileID.Mud || !Main.tile[i, j].HasTile)
                    continue;

                var air = 0;

                for (var mapX = i - 1; mapX <= i + 1; mapX++)
                {
                    for (var mapY = j - 1; mapY <= j + 1; mapY++)
                    {
                        float topDeform = noise.GetNoise(i * deformScale, r.Top * deformScale) * 20 + 20;


                        if (mapY > r.Bottom || mapY < r.Top - topDeform || mapX > r.Right || mapX < r.Left)
                            continue;

                        if (!Main.tile[mapX, mapY].HasTile)
                            air++;
                    }
                }

                if (air > 1)
                    TryPlaceTile(i, j, TileID.CrimsonJungleGrass); // slime mold placeholder
            }
        }

        return true;
    }

    private void TryPlaceWall(int i, int j, int type)
    {
        if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
            return;

        WorldGen.PlaceWall(i, j, type, mute: true);
    }

    private void TryPlaceTile(int i, int j, int type)
    {
        if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
            return;

        WorldGen.PlaceTile(i, j, type, mute: true, forced: true);
    }
}
