using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Geometry;
using SpaceEventMod.Common.SDFs;
using SpaceEventMod.Common.WorldGeneration;
using SpaceEventMod.Content.CellularGrowth.Tiles;
using SpaceEventMod.Content.CellularGrowth.Walls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Daybreak.Common.Features.Hooks.ModifyItemDrawBasics;

namespace SpaceEventMod.Content.CellularGrowth;

public class CellularGrowthGen : ModSystem
{
    public static Line[] _connectiveCells;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int islandsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tile Cleanup"));

        if (islandsIndex != -1)
        {
            tasks.Insert(islandsIndex - 1, new CellularGrowthPass("Cellular Growth", 100f));
        }
    }
}

internal class CellularGrowthPass : GenPass
{
    private enum InteriorType : byte
    {
        Hollow,
        Caves,
        HollowCaves,
        None
    }

    public CellularGrowthPass(string name, float loadWeight) : base(name, loadWeight)
    {

    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        // Get cellular growth patch sizes and placements
        int patches = 2;
        int worldMargins = 40;
        float spaceBottom = Main.maxTilesY * 0.35f + worldMargins;

        int[] xPositions = new int[patches];

        Point[] seedDimensions = new Point[patches];
        Vector2[] seedPoints = new Vector2[patches];

        for (int i = 0; i < patches; i++)
        {
            int width = (int)(Main.maxTilesX / 4200f * 225); //Automatically scales based on world size
            int height = (int)(Main.maxTilesY / 1200f * 100);
            int x = WorldGen.genRand.Next(width + 80, Main.maxTilesX - (width + 80));

            int y = WorldGen.genRand.Next(36, 50);

            seedPoints[i] = new Vector2(x, y + worldMargins);
            seedDimensions[i] = new Point(width, height + worldMargins);
        }

        var asteroidPositions = GetAsteroidPositions(seedPoints, seedDimensions, worldMargins, spaceBottom);
        var asteroidBounds = GetAsteroidBounds(asteroidPositions, spaceBottom);

        // Paint asteroids
        FastNoiseLite noise = new FastNoiseLite(WorldGen.genRand.Next());

        int sizeThreshhold = (int)((100 * 0.35f) / MathF.Sqrt(2));
        int lowestAsteroidYValue = (int)spaceBottom;

        List<Point> asteroidPoints = new List<Point>();

        for (int i = 0; i < asteroidBounds.Count; i++)
        {
            bool large = false;
            var bound = asteroidBounds[i];

            if (bound.Width > sizeThreshhold || bound.Height > sizeThreshhold)
                large = true;

            GenNoisyPlanetoid(ref asteroidPoints, noise, bound);
            GenPlanetoidCaves(ref asteroidPoints, noise, bound, large);

            asteroidPoints.Clear();

            progress.Set(i / (asteroidBounds.Count - 1));
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
                position.X >= Main.maxTilesX || position.Y >= Main.maxTilesY)
                return true;

            float total = 99999f;

            for (int i = 0; i < seedPoints.Length; i++)
            {
                Ellipse ellipse = new Ellipse(seedPoints[i], seedDimensions[i].ToVector2());

                float dist = ellipse.GetSignedDistance(position).X;

                total = MathF.Min(total, dist);
            }

            float scale = (int)((Main.maxTilesX / 4200f) * 100);

            if (total >= 0)
                return false;

            return true;
        }

        float LargeAsteroids(float x, float y) =>
            WorldGen.genRand.NextFloat(90, 110);

        float NormalAsteroids(float x, float y) =>
            WorldGen.genRand.NextFloat(20, 110);

        VariablePoissonSampler2D poissonSampler = new VariablePoissonSampler2D(WorldGen.genRand, Main.maxTilesX, spaceBottom);

        poissonSampler.Initialize(20, 110);

        foreach (var pos in seedPoints)
            poissonSampler.AddSample(pos, WorldGen.genRand.NextFloat(35, 110));

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
            int right = Math.Min(Main.maxTilesX, (int)(sample.Position.X + rectangleLength));
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

    private static void GenNoisyPlanetoid(ref List<Point> tilePosSet, FastNoiseLite noise, Rectangle asteroid)
    {
        Vector2 center = asteroid.Center.ToVector2() - Vector2.One * 0.5f;
        Vector2 ab = asteroid.Size() * 0.5f;

        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetSeed(WorldGen.genRand.Next());

        float roughness = 2f;
        float baseRoughness = 0.5f;
        float strength = 0.3f;
        float persistence = 0.5f;
        int noiseLayers = 3;
        Vector2 noiseCenter = WorldGen.genRand.NextVector2Square(-1, 1) * 500;
        Vector2 margin = new Vector2(asteroid.Width * (strength + 1), asteroid.Height * (strength + 1));

        for (int i = (int)-margin.X; i < asteroid.Width + margin.X; i++)
        {
            for (int j = (int)-margin.Y; j < asteroid.Height + margin.Y; j++)
            {
                Vector2 position = new Vector2(i + asteroid.X, j + asteroid.Y);

                if (position.X < 0 || position.Y < 0 ||
                    position.X >= Main.maxTilesX || position.Y >= Main.maxTilesY)
                    continue;

                float elevation = SamplePlanetoidNoise(
                    noise, position - center, noiseCenter, roughness, baseRoughness, strength, persistence, noiseLayers);
                elevation += 1 - (strength * 0.5f);

                if (Ellipse.GetSignedDistance(position, center, ab * elevation).X <= 0)
                {
                    Point point = new Point(i + asteroid.X, j + asteroid.Y);

                    WorldGen.PlaceTile(point.X, point.Y, ModContent.TileType<Cosmostone>(), forced: true);
                    tilePosSet.Add(point);
                }
            }
        }

        // fill not exposed tiles with walls
        foreach (var position in tilePosSet)
        {
            int air = 0;

            foreach (var direction in TileDirections.WithCorners)
            {
                var point = position + direction;
                point = new Point(
                    Math.Clamp(point.X, 0, Main.maxTilesX),
                    Math.Clamp(point.Y, 0, Main.maxTilesY));

                if (!Main.tile[point].HasTile)
                    air++;
            }

            if (air == 0)
                WorldGen.PlaceWall(position.X, position.Y, ModContent.WallType<CosmostoneWall>());
        }
    }

    private static void GenPlanetoidCaves(ref List<Point> tilePosSet, FastNoiseLite noise, Rectangle asteroid, bool large)
    {
        SdfScene sdfScene = new SdfScene();

        Vector2 center = asteroid.Center.ToVector2() - Vector2.One * 0.5f;

        // Generate entrance sdfs
        if (WorldGen.genRand.NextBool() || large)
        {
            int entrances = WorldGen.genRand.Next(1, large ? 5 : 4);

            var angles = WorldGen.genRand.NextRandomAngles(entrances, 0.5f);

            List<Vector2> points = new List<Vector2>();
            float current = WorldGen.genRand.NextFloat(0, MathHelper.TwoPi);
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
            Vector2 ellipseDimensions = asteroid.Size() * 0.5f * WorldGen.genRand.NextFloat(0.4f, 0.6f);
            ellipseDimensions *= new Vector2(WorldGen.genRand.NextFloat(0.8f, 1.2f), WorldGen.genRand.NextFloat(0.8f, 1.2f));
            sdfScene.AddPrimitive(new Ellipse(center, ellipseDimensions));
        }

        float roughness = 3f;
        float baseRoughness = 1f;
        float strength = 3f;
        float persistence = 0.5f;
        int noiseLayers = 5;
        Vector2 noiseCenter = WorldGen.genRand.NextVector2Square(-1, 1) * 500;

        // carve out caves
        foreach (var position in tilePosSet)
        {
            var sample = sdfScene.Sample(position.ToVector2(), 1.5f, SmoothMinimum.CircularGeometrical);

            // displace radius based on position
            // displace radius based on sdf gradient

            // carve out cave
            if (sample.X <= 0)
                WorldGen.KillTile(position.X, position.Y);
        }
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
