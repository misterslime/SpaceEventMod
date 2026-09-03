using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Utilities;
using WorldGenSandbox.SDFs;
using WorldGenSandbox.Utilities;

using static WorldGenSandbox.Game1;

namespace WorldGenSandbox.WorldGen;

internal class CellularGrowthGen
{
    private enum InteriorType : byte
    {
        Hollow,
        Caves,
        HollowCaves,
        None
    }

    public static void Generate()
    {
        // Get cellular growth patch sizes and placements
        int patches = 2;
        int worldMargins = 40;
        float spaceBottom = Globals.World.MaxTilesY * 0.35f + worldMargins;

        int[] xPositions = new int[patches];

        Point[] seedDimensions = new Point[patches];
        Vector2[] seedPoints = new Vector2[patches];

        for (int i = 0; i < patches; i++)
        {
            int width = (int)(Globals.World.MaxTilesX / 4200f * 225); //Automatically scales based on world size
            int height = (int)(Globals.World.MaxTilesY / 1200f * 100);
            int x = Globals.GenRand.Next(width + 80, Globals.World.MaxTilesX - (width + 80));

            int y = Globals.GenRand.Next(36, 50);

            seedPoints[i] = new Vector2(x, y + worldMargins);
            seedDimensions[i] = new Point(width, height + worldMargins);
        }

        var asteroidPositions = GetAsteroidPositions(seedPoints, seedDimensions, worldMargins, spaceBottom);
        var asteroidBounds = GetAsteroidBounds(asteroidPositions, spaceBottom);

        // Paint asteroids
        FastNoiseLite noise = new FastNoiseLite(Globals.GenRand.Next());

        int sizeThreshhold = (int)((100 * 0.35f) / MathF.Sqrt(2));
        int lowestAsteroidYValue = (int)spaceBottom;

        List<Point> asteroidPoints = new List<Point>();

        foreach (var bound in asteroidBounds)
        {
            bool large = false;

            if (bound.Width > sizeThreshhold || bound.Height > sizeThreshhold)
                large = true;

            GenNoisyPlanetoid(ref asteroidPoints, noise, bound, large ? TileTypes.HerbCell : TileTypes.Cosmostone);
            GenPlanetoidCaves(ref asteroidPoints, noise, bound, large);

            asteroidPoints.Clear();
        }

        // connect asteroids (for connective cells)
        //ConnectAsteroids(poissonSampler, noise, radius);
    }

    private static List<Sample2D> GetAsteroidPositions(Vector2[] seedPoints, Point[] seedDimensions, int worldMargins, float spaceBottom)
    {
        // Get asteroid positions
        bool InBounds(float x, float y)
        {
            Vector2 position = new Vector2(x, y);

            if (position.X < 0 || position.Y < 0 ||
                position.X >= Globals.World.MaxTilesX || position.Y >= Globals.World.MaxTilesY)
                return true;

            float total = 99999f;

            for (int i = 0; i < seedPoints.Length; i++)
            {
                Ellipse ellipse = new Ellipse(seedPoints[i], seedDimensions[i].ToVector2());

                float dist = ellipse.GetSignedDistance(position).X;

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

        return poissonSampler.Samples;
    }

    private static List<Rectangle> GetAsteroidBounds(List<Sample2D> asteroidPositions, float spaceBottom)
    {
        List<Rectangle> bounds = new List<Rectangle>();

        foreach (var sample in asteroidPositions)
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

        return bounds;
    }

    private static void GenNoisyPlanetoid(ref List<Point> tilePosSet, FastNoiseLite noise, Rectangle asteroid, TileTypes type)
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

                float elevation = SamplePlanetoidNoise(
                    noise, position - center, noiseCenter, roughness, baseRoughness, strength, persistence, noiseLayers);
                elevation += 1 - (strength * 0.5f);

                if (Ellipse.GetSignedDistance(position, center, ab * elevation).X <= 0)
                {
                    Point point = new Point(i + asteroid.X, j + asteroid.Y);

                    Globals.World.Tiles[point.X, point.Y] = type;
                    tilePosSet.Add(point);
                }
            }
        }

        Console.WriteLine("meow + " + tilePosSet.Count);
    }

    private static void GenPlanetoidCaves(ref List<Point> tilePosSet, FastNoiseLite noise, Rectangle asteroid, bool large)
    {
        SdfScene sdfScene = new SdfScene();

        Vector2 center = asteroid.Center.ToVector2() - Vector2.One * 0.5f;

        // Generate entrance sdfs
        if (Globals.GenRand.NextBool() || large)
        {
            int entrances = Globals.GenRand.Next(1, large ? 5 : 4);

            var angles = Globals.GenRand.NextRandomAngles(entrances, 0.5f);

            List<Vector2> points = new List<Vector2>();
            float current = Globals.GenRand.NextFloat(0, MathHelper.TwoPi);
            float length = 500;
            float radius = large ? 3f : 2f;

            foreach (var angle in angles)
            {
                var end = center + length * new Vector2(MathF.Cos(current), MathF.Sin(current));
                sdfScene.AddPrimitive(new Segment(radius, center, end));
                current += angle;
            }
        }

        // make hollow
        if (large)
        {
            Vector2 ellipseDimensions = asteroid.Size() * 0.5f * Globals.GenRand.NextFloat(0.4f, 0.6f);
            ellipseDimensions *= new Vector2(Globals.GenRand.NextFloat(0.8f, 1.2f), Globals.GenRand.NextFloat(0.8f, 1.2f));
            sdfScene.AddPrimitive(new Ellipse(center, ellipseDimensions));
        }

        float roughness = 3f;
        float baseRoughness = 1f;
        float strength = 3f;
        float persistence = 0.5f;
        int noiseLayers = 5;
        Vector2 noiseCenter = Globals.GenRand.NextVector2Square(-1, 1) * 500;
        foreach (var position in tilePosSet)
        {
            var sample = sdfScene.Sample(position.ToVector2(), 1.5f, SmoothMinimum.CircularGeometrical);

            // displace radius based on position
            // displace radius based on sdf gradient

            if (sample.X <= 0)
                Globals.World.Tiles[position.X, position.Y] = TileTypes.CosmostoneWall;
        }

        /*for (int i = 0; i < entrances; i++)
        {
            
        }*/



        /*Vector2 center = asteroid.Center.ToVector2() - Vector2.One * 0.5f;
        Vector2 panotm = Globals.GenRand.NextVector2Circular(1, 1) * 10;

        digTunnel(center.X, center.Y, panotm.X, panotm.Y, 10, 4);*/
    }

    /// <summary>
    /// Samples a point of layered noise on a circle.
    /// </summary>
    /// <param name="noise">The FastNoiseLite state to use.</param>
    /// <param name="position">The position to be sampled.</param>
    /// <param name="roughness">How quickly layers increase in noise frequency.</param>
    /// <param name="baseRoughness">Noise frequency for the first layer.</param>
    /// <param name="strength">Value to multiply the sampled noise by.</param>
    /// <param name="persistence">How quickly layers decrease in amplitude.</param>
    /// <param name="layers">How many layers of noise to sample. Layers always increase in frequency and decrease in amplitude.</param>
    /// <param name="displacement">Noise displacement.</param>
    /// <returns>A number between 0 and 1.</returns>
    private static float SamplePlanetoidNoise(
        FastNoiseLite noise,
        Vector2 position,
        Vector2 displacement,
        float roughness,
        float baseRoughness,
        float strength,
        float persistence,
        int layers)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < layers; i++)
        {
            Vector2 normalized = Vector2.Normalize(position);
            normalized *= 100 * frequency;
            normalized += displacement;

            noiseValue += (1 + noise.GetNoise(normalized.X, normalized.Y)) * 0.5f * amplitude;

            frequency *= roughness;
            amplitude *= persistence;
        }
        
        return noiseValue * strength;
    }

}
