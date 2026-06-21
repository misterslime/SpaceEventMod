using Microsoft.Xna.Framework;
using System;

namespace WorldGenSandbox.WorldGen;

internal class SlimeMoldGen
{
    private static float SdSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 pa = p - a, ba = b - a;
        float h = MathHelper.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0.0f, 1.0f);
        return (pa - ba * h).Length();
    }

    public static void Generate()
    {
        int rockLayer = (int)(Globals.World.MaxTilesY * 0.4f);

        int tries = 0;
        for (int i = 0; i < (Globals.World.MaxTilesX / 2400) + 2; i++)
        {
            int x = Globals.GenRand.Next(300, Globals.World.MaxTilesX - 300);
            int y = Globals.GenRand.Next((int)rockLayer + 100, Globals.World.MaxTilesY - 500);
            int size = (int)(Globals.GenRand.Next(160, 200) * (Globals.World.MaxTilesX / 6400f));

            Rectangle rectangle = new Rectangle(x, y, (int)(size * 0.5f), size);
            if (!SpawnBiome(rectangle) && tries++ < 999)
            {
                i--;
            }
        }
    }

    private static bool SpawnBiome(Rectangle r)
    {
        Vector2 top = new Vector2(0.5f, -0.2f);
        Vector2 middle = Vector2.One * 0.5f;

        FastNoiseLite noise = new FastNoiseLite(Globals.GenRand.Next());
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

                float sdf = SdSegment(peeb, middle, top);

                sdf += noise.GetNoise(i * scale, j * scale * 0.5f) * 0.2f;

                float deform = MathF.Abs(noise.GetNoise(i * deformScale, j * deformScale)) * 0.4f;

                float topDeform = noise.GetNoise(i * deformScale, r.Top * deformScale) * 20 + 20;

                if (j > r.Top - topDeform && sdf < 0.5 + deform)
                    Globals.World.Tiles[i, j] = TileTypes.Mud;

                if (sdf <= 0.3f)
                    Globals.World.Tiles[i, j] = TileTypes.Empty;
            }
        }

        // Grow slime mold grass
        for (int i = r.Left - 60; i < r.Right + 60; i++)
        {
            for (int j = r.Top - 60; j < r.Bottom + 60; j++)
            {
                //WorldGen.World.Tiles[i, j] = TileTypes.Mud;

                if (Globals.World.Tiles[i, j] != TileTypes.Mud)
                    continue;

                var air = 0;

                for (var mapX = i - 1; mapX <= i + 1; mapX++)
                {
                    for (var mapY = j - 1; mapY <= j + 1; mapY++)
                    {
                        float topDeform = noise.GetNoise(i * deformScale, r.Top * deformScale) * 20 + 20;


                        if (mapY > r.Bottom || mapY < r.Top - topDeform || mapX > r.Right || mapX < r.Left)
                            continue;

                        if (Globals.World.Tiles[mapX, mapY] == TileTypes.Empty)
                            air++;
                    }
                }

                if (air > 1)
                    Globals.World.Tiles[i, j] = TileTypes.SlimeMold;
            }
        }

        return true;
    }
}
