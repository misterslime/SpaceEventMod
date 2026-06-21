using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using WorldGenSandbox.Utilities;

using static WorldGenSandbox.Game1;

namespace WorldGenSandbox.WorldGen;

internal class CellularGrowthGen
{
    public static void Generate()
    {
        // Get cellular growth patch sizes and placements
        int patches = 2;
        int worldMargins = 40;

        int[] xPositions = new int[patches];

        List<Point> seedDimensions = new List<Point>(patches);
        List<Vector2> seedPoints = new List<Vector2>(patches);

        for (int i = 0; i < patches; i++)
        {
            int width = (int)(Globals.World.MaxTilesX / 4200f * 225); //Automatically scales based on world size
            int height = (int)(Globals.World.MaxTilesY / 1200f * 100);
            int x = Globals.GenRand.Next(width + 80, Globals.World.MaxTilesX - (width + 80));

            int y = Globals.GenRand.Next(36, 50);

            seedPoints.Add(new Vector2(x, y + worldMargins));
            seedDimensions.Add(new Point(width, height + worldMargins));
        }

        // Get asteroid positions
        float spaceBottom = Globals.World.MaxTilesY * 0.35f + worldMargins;

        bool InBounds(float x, float y)
        {
            Vector2 position = new Vector2(x, y);

            if (position.X < 0 || position.Y < 0 ||
                position.X >= Globals.World.MaxTilesX || position.Y >= Globals.World.MaxTilesY)
                return true;

            float total = 99999f;

            for (int i = 0; i < seedPoints.Count; i++)
            {
                Vector2 relativePosition = position - seedPoints[i];

                float dist = SDFs.EllipseSDF(relativePosition, seedDimensions[i].ToVector2());

                total = MathF.Min(total, dist);
            }

            float scale = (int)((Globals.World.MaxTilesX / 4200f) * 100);

            if (total >= 0)
                return false;

            return true;
        }

        float LargeAsteroids(float x, float y) =>
            Globals.GenRand.NextFloat(90, 110);

        float NormalAsteroids(float x, float y) =>
            Globals.GenRand.NextFloat(20, 110);

        VariablePoissonSampler2D poissonSampler = new VariablePoissonSampler2D(Globals.GenRand, Globals.World.MaxTilesX, spaceBottom);

        poissonSampler.Initialize(20, 110);

        foreach (var pos in seedPoints)
            poissonSampler.AddSample(pos, Globals.GenRand.NextFloat(35, 110));

        poissonSampler.Generate(LargeAsteroids, InBounds);
        poissonSampler.Generate(NormalAsteroids, InBounds);

        List<Rectangle> bounds = new List<Rectangle>();

        foreach (var sample in poissonSampler.Samples)
        {
            // add a margin so smaller asteroids r bigger
            float radius = sample.Radius + 15;

            int rectangleLength = (int)((radius * 0.25f) / MathF.Sqrt(2));

            int left = Math.Max(0, (int)(sample.Position.X - rectangleLength));
            int right = Math.Min(Globals.World.MaxTilesX, (int)(sample.Position.X + rectangleLength));
            int top = Math.Max(0, (int)(sample.Position.Y - rectangleLength));
            int bottom = Math.Min((int)spaceBottom, (int)(sample.Position.Y + rectangleLength));

            bounds.Add(new Rectangle(left, top, right - left, bottom - top));
        }

        // Combine overlaping asteroids
        for (int i = 0; i < bounds.Count; i++)
        {
            Rectangle first = bounds[i];

            for (int j = 0; j < bounds.Count; j++)
            {
                if (i == j)
                    continue;

                Rectangle second = bounds[j];

                if (!first.Intersects(second))
                    continue;

                if (Vector2.Distance(first.Center.ToVector2(), second.Center.ToVector2()) >
                    MathF.Min(first.Size().Length() * 0.5f, second.Size().Length() * 0.5f))
                    continue;

                int x = Math.Min(first.Left, second.Left);
                int y = Math.Min(first.Top, second.Top);
                int width = Math.Max(first.Right, second.Right) - x;
                int height = Math.Max(first.Bottom, second.Bottom) - y;

                bounds.Add(new Rectangle(x, y, width, height));

                bounds.RemoveUnorderedAt(i);
                bounds.RemoveUnorderedAt(j);

                i = 0;
                break;
            }
        }

        // Delete any remaining overlapping asteroids, depending on which one is biggest
        for (int i = 0; i < bounds.Count; i++)
        {
            Rectangle first = bounds[i];

            for (int j = 0; j < bounds.Count; j++)
            {
                if (i == j)
                    continue;

                Rectangle second = bounds[j];

                if (!first.Intersects(second))
                    continue;

                int areaFirst = first.Width * first.Height;
                int areaSecond = second.Width * second.Height;

                if (areaFirst > areaSecond)
                    bounds.RemoveUnorderedAt(j);
                else
                    bounds.RemoveUnorderedAt(i);

                i = 0;
                break;
            }
        }

        // Paint asteroids
        FastNoiseLite noise = new FastNoiseLite(Globals.GenRand.Next());
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetSeed(Globals.GenRand.Next());

        int sizeThreshhold = (int)((100 * 0.25f) / MathF.Sqrt(2));


        foreach (var bound in bounds)
        {
            bool large = false;

            if (bound.Width > sizeThreshhold || bound.Height > sizeThreshhold)
                large = true;

            // 0 = hollow
            // 1 = caves
            // 2 = solid
            int caveType = Globals.GenRand.Next(0, large ? 2 : 3);

            GenNoisyPlanetoid(noise, bound, large ? TileTypes.HerbCell : TileTypes.Cosmostone);

            /*UniformPoissonSampler2D uniformSampler = new UniformPoissonSampler2D(WorldGen.genRand, 2, bound.Width, bound.Height, 20);

            uniformSampler.Generate();

            foreach (var sample in uniformSampler.SamplesList)
            {
                _tiles[(int)sample.X + bound.X, (int)sample.Y + bound.Y] = TileTypes.HerbCell;
            }*/
        }

        // connect asteroids (for connective cells)
        //ConnectAsteroids(poissonSampler, noise, radius);
    }


    private static void GenNoisyPlanetoid(FastNoiseLite noise, Rectangle asteroid, TileTypes type)
    {
        Vector2 center = asteroid.Center.ToVector2() - Vector2.One * 0.5f;
        Vector2 ab = asteroid.Size() * 0.5f;

        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetSeed(Globals.GenRand.Next());

        float roughness = 2f;
        float baseRoughness = 0.5f;
        float strength = 0.3f;
        float persistence = 0.5f;
        int noiseLayers = 3;
        Vector2 noiseCenter = Globals.GenRand.NextVector2Square(-1, 1) * 500;

        Vector2 margin = new Vector2(asteroid.Width * (strength + 1), asteroid.Height * (strength + 1));

        for (int i = (int)-margin.X; i < asteroid.Width + margin.X; i++)
        {
            for (int j = (int)-margin.Y; j < asteroid.Height + margin.Y; j++)
            {
                Vector2 position = new Vector2(i + asteroid.X, j + asteroid.Y);

                if (position.X < 0 || position.Y < 0 ||
                    position.X >= Globals.World.MaxTilesX || position.Y >= Globals.World.MaxTilesY)
                    continue;

                position -= center;


                float elevation = SamplePlanetoidNoise(
                    noise, position, roughness, baseRoughness, strength, persistence, noiseLayers, noiseCenter);
                elevation += 1 - (strength * 0.5f);


                if (SDFs.EllipseSDF(position, ab * elevation) <= 0)
                    Globals.World.Tiles[i + asteroid.X, j + asteroid.Y] = type;
            }
        }
    }

    private static float SamplePlanetoidNoise(
        FastNoiseLite noise,
        Vector2 position,
        float roughness,
        float baseRoughness,
        float strength,
        float persistence,
        int layers,
        Vector2 center)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < layers; i++)
        {
            Vector2 normalized = Vector2.Normalize(position);
            normalized *= 100 * frequency;
            normalized += center;

            noiseValue += (1 + noise.GetNoise(normalized.X, normalized.Y)) * 0.5f * amplitude;

            frequency *= roughness;
            amplitude *= persistence;
        }

        return noiseValue * strength;
    }
}
