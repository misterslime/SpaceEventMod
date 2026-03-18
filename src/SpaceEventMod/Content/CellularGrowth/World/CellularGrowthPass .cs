using Iced.Intel;
using Microsoft.Xna.Framework;
using SpaceEventMod.Content.CellularGrowth.Tiles;
using SpaceEventMod.Content.CellularGrowth.Walls;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Enums;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpaceEventMod.Content.CellularGrowth.World;

internal record struct FoliagePatch(
    Point Position,
    float CircleRadius,
    bool Interior); // true means it checks for tiles adjacent to wall tiles, false means it checks for air tile adjacency

internal class CellularGrowthPass : GenPass
{
    public CellularGrowthPass(string name, float loadWeight) : base(name, loadWeight)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        int clusters = 2;

        int[] xPositions = new int[clusters];

        List<Point> seedDimensions = new List<Point>(clusters);
        List<Vector2> seedPoints = new List<Vector2>(clusters);

        for (int i = 0; i < clusters; i++)
        {
            int width = (int)(Main.maxTilesX / 4200f * 150); //Automatically scales based on world size
            int height = (int)(Main.maxTilesY / 1200f * 90);
            int x = WorldGen.genRand.Next(width + 80, Main.maxTilesX - (width + 80));

            int y = WorldGen.genRand.Next(36, 50);

            seedPoints.Add(new Vector2(x, y));
            seedDimensions.Add(new Point(width, height));
        }

        FastNoiseLite noise = new FastNoiseLite(WorldGen.genRand.Next());
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        // get connective cells and asteroid positions
        int radius = 22;
        PoissonSampler2D poissonSampler = new PoissonSampler2D(radius, new Vector2(Main.maxTilesX, Main.maxTilesY), noise, seedDimensions, seedPoints);

        noise.SetSeed(WorldGen.genRand.Next());

        // connect asteroids (for connective cells)
        ConnectAsteroids(poissonSampler, noise, radius);

        //Tile tile = Main.tile[499350934, 3943492];

        //Tile tile2 = Framing.GetTileSafely(903294, 3245295);

        // generate asteroid shapes
        for (int i = 0; i < poissonSampler.Samples.Count; i++)
        {
            Vector2 sample = poissonSampler.Samples[i];

            List<Point> exteriorTiles;
            List<Point> interiorTiles;

            AsteroidGenValues asteroid = new AsteroidGenValues(sample, noise, Main.maxTilesX, Main.maxTilesY);
            ShapeAsteroid(asteroid, out exteriorTiles, out interiorTiles);
            GrowFoliage(asteroid, exteriorTiles, interiorTiles);
            //PlugCaveOpenings(asteroid);
        }
    }

    private void ConnectAsteroids(PoissonSampler2D poissonSampler, FastNoiseLite noise, int radius)
    {
        HashSet<Line> lines = new HashSet<Line>();
        HashSet<Point> connections = new HashSet<Point>();

        for (int i = 0; i < poissonSampler.Grid.GetLength(0); i++)
        {
            for (int j = 0; j < poissonSampler.Grid.GetLength(1); j++)
            {
                int myIndex = poissonSampler.Grid[i, j];

                if (myIndex < 0 || myIndex >= poissonSampler.Samples.Count)
                    continue;

                Point position = new Point(i, j);
                Vector2 asteroidPosition = poissonSampler.Samples[myIndex];
                float connectProbability = noise.GetNoise(asteroidPosition.X, asteroidPosition.Y) * 0.5f + 0.5f;

                foreach (var direction in TileDirections.WithCorners)
                {
                    Point newPoint = direction + position;

                    if (newPoint.X < 0 || newPoint.Y < 0) continue;
                    if (newPoint.X >= poissonSampler.Grid.GetLength(0) ||
                        newPoint.Y >= poissonSampler.Grid.GetLength(1))
                        continue;

                    int theirIndex = poissonSampler.Grid[newPoint.X, newPoint.Y];

                    if (theirIndex < 0 || theirIndex >= poissonSampler.Samples.Count)
                        continue;

                    Vector2 theirPosition = poissonSampler.Samples[theirIndex];
                    bool connect = WorldGen.genRand.NextFloat() <= connectProbability;

                    if (Vector2.Distance(asteroidPosition, theirPosition) >= radius * 1.5f || !connect)
                        continue;

                    Line line = new Line(asteroidPosition, theirPosition);
                    Point connection = new Point(myIndex, theirIndex);
                    Point connection2 = new Point(theirIndex, myIndex);

                    if (!lines.Contains(line) && !connections.Contains(connection) && !connections.Contains(connection2))
                    {
                        lines.Add(line);
                        connections.Add(connection);
                        connections.Add(connection2);
                    }
                }
            }
        }

        CellularGrowthGen._connectiveCells = lines.ToArray();
    }

    private void ShapeAsteroid(AsteroidGenValues asteroid, out List<Point> exteriorTiles, out List<Point> interiorTiles)
    {
        Output[] tiles = AsteroidCellularAutomata.Generate(asteroid.caveType, asteroid.width, asteroid.height, 16, 50);

        // asteroid + caves
        for (int i = 0; i < asteroid.width; i++)
        {
            for (int j = 0; j < asteroid.height; j++)
            {
                if (i + asteroid.start.X < 0 || i + asteroid.start.X >= Main.maxTilesX ||
                    j + asteroid.start.Y < 0 || j + asteroid.start.Y >= Main.maxTilesY)
                    continue;

                if (tiles[i + j * asteroid.width] == Output.Empty)
                    continue;
                else if (tiles[i + j * asteroid.width] == Output.Wall)
                {
                    TryPlaceWall(i + asteroid.start.X, j + asteroid.start.Y, ModContent.WallType<CosmostoneWall>());
                }
                else if (tiles[i + j * asteroid.width] == Output.Solid)
                {
                    TryPlaceTile(i + asteroid.start.X, j + asteroid.start.Y, ModContent.TileType<Cosmostone>());
                    TryPlaceWall(i + asteroid.start.X, j + asteroid.start.Y, ModContent.WallType<CosmostoneWall>());
                }


            }
        }

        exteriorTiles = new List<Point>(64);
        interiorTiles = new List<Point>(64);

        // get edge tiles
        for (int i = 0; i < asteroid.width; i++)
        {
            for (int j = 0; j < asteroid.height; j++)
            {
                if (i + asteroid.start.X < 1 || i + asteroid.start.X >= Main.maxTilesX - 1 ||
                    j + asteroid.start.Y < 1 || j + asteroid.start.Y >= Main.maxTilesY - 1)
                    continue;

                AdjacencyData<bool> interior = new AdjacencyData<bool>(i + asteroid.start.X, j + asteroid.start.Y,(tile) =>
                {
                    return !tile.HasTile && tile.WallType == 0;
                });

                AdjacencyData<bool> exterior = new AdjacencyData<bool>(i + asteroid.start.X, j + asteroid.start.Y, (tile) =>
                {
                    return !tile.HasTile && tile.WallType == ModContent.WallType<CosmostoneWall>();
                });

                if (interior.Left || interior.Right || 
                    interior.TopRight || interior.BottomRight || 
                    interior.Bottom || interior.Top || 
                    interior.BottomLeft || interior.TopLeft)
                    exteriorTiles.Add(new Point(i + asteroid.start.X, j + asteroid.start.Y));

                if (exterior.Left || exterior.Right ||
                    exterior.TopRight || exterior.BottomRight ||
                    exterior.Bottom || exterior.Top ||
                    exterior.BottomLeft || exterior.TopLeft)
                    interiorTiles.Add(new Point(i + asteroid.start.X, j + asteroid.start.Y));
            }
        }

        // clear walls outside asteroid
        foreach (Point point in exteriorTiles)
        {
            Main.tile[point.X, point.Y].WallType = 0;
        }

        GenVars.structures.AddProtectedStructure(new Rectangle(asteroid.start.X, asteroid.start.Y, asteroid.width, asteroid.height));
    }

    private void GrowFoliage(AsteroidGenValues asteroid, List<Point> exteriorTiles, List<Point> interiorTiles)
    {
        // decorations are done with sdfs, first create sdfs
        Dictionary<string, List<FoliagePatch>> decorations = new Dictionary<string, List<FoliagePatch>>();

        decorations["herbCell"] = new List<FoliagePatch>(64);
        decorations["cosmoss"] = new List<FoliagePatch>(64);

        // better idea: the amount of sdf patches per asteroid should be defined n then it should pick from the list mhm

        int cosmossPatches = (int)(5 * asteroid.noiseSample);
        int cosmossPatchesInner = 2 + (int)(5 * asteroid.noiseSample);

        int herbcellPatches = (int)(3 * asteroid.noiseSample);
        int herbcellPatchesInner = (int)(4 * asteroid.noiseSample);

        if (exteriorTiles.Count != 0)
        {
            AddPatch(decorations, exteriorTiles, cosmossPatches, "cosmoss", false, 12, 16, 3);
            AddPatch(decorations, exteriorTiles, herbcellPatches, "herbCell", false, 2, 6, 10);
        }

        if (interiorTiles.Count != 0)
        {
            AddPatch(decorations, interiorTiles, cosmossPatchesInner, "cosmoss", true, 10, 12, 5);
            AddPatch(decorations, interiorTiles, herbcellPatchesInner, "herbCell", true, 1, 4, 10);
        }

        // generate decorations
        float smoothing = 2;

        // cosmoss
        for (int i = 0; i < asteroid.width; i++)
        {
            for (int j = 0; j < asteroid.height; j++)
            {
                Point tile = new Point(i + asteroid.start.X, j + asteroid.start.Y);

                if (tile.X < 0 || tile.X >= Main.maxTilesX ||
                    tile.Y < 0 || tile.Y >= Main.maxTilesY)
                    continue;

                if (Framing.GetTileSafely(tile.X, tile.Y).TileType != ModContent.TileType<Cosmostone>())
                    continue;

                bool interior = interiorTiles.Contains(tile);
                bool exterior = exteriorTiles.Contains(tile);

                float total = 99999f;

                foreach (FoliagePatch decoration in decorations["cosmoss"])
                {
                    if (decoration.Interior && !interior)
                        continue;

                    if (!decoration.Interior && !exterior)
                        continue;

                    float dist = SignedDistanceFunctions.CircleSDF((tile - decoration.Position).ToVector2(), decoration.CircleRadius);

                    total = SignedDistanceFunctions.SmoothMinimum(total, dist, smoothing);
                }

                if (total < 0)
                    Main.tile[tile.X, tile.Y].TileType = (ushort)ModContent.TileType<Cosmoss>();
            }
        }

        // herbcell gen is already a thing
    }

    private void AddPatch(
        Dictionary<string, List<FoliagePatch>> foliage, List<Point> tilePool,
        int num, string type, bool interior, float minRadius, float maxRadius, int failureRate)
    {
        for (int i = 0; i < num; i++)
        {
            if (WorldGen.genRand.Next(failureRate) == 0)
                continue;

            FoliagePatch patch = new FoliagePatch();

            patch.Position = WorldGen.genRand.NextFromCollection(tilePool);
            patch.CircleRadius = WorldGen.genRand.NextFloat(minRadius, maxRadius);
            patch.Interior = interior;

            foliage[type].Add(patch);
        }
    }

    private void PlugCaveOpenings(AsteroidGenValues asteroid)
    {
        List<Point> positions = new List<Point>();

        for (int j = asteroid.start.Y; j < asteroid.start.Y + asteroid.height; j++)
        {
            for (int i = asteroid.start.X; i < asteroid.start.X + asteroid.width; i++)
            {
                if (PlugAutomata(i, j))
                    positions.Add(new Point(i, j));
            }
        }

        foreach (Point point in positions)
        {
            TryPlaceTile(point.X, point.Y, ModContent.TileType<Cosmostone>());
        }
    }

    private bool PlugAutomata(int i, int j)
    {
        Point[] positions = [
            new Point(i, j - 1),
            new Point(i, j + 1),

            new Point(i - 1, j),
            new Point(i + 1, j),

            new Point(i - 1, j - 1),
            new Point(i + 1, j - 1),

            new Point(i - 1, j + 1),
            new Point(i + 1, j + 1),
        ];

        Tile tile = Framing.GetTileSafely(i, j);

        if (!tile.HasTile && tile.WallType == 0)
            return false;

        int walls = 0;
        int empty = 0;

        foreach (Point position in  positions)
        {
            if (position.X < 0 || position.Y < 0 || position.X >= Main.maxTilesX || position.Y >= Main.maxTilesY)
                continue;

            Tile check = Framing.GetTileSafely(position.X, position.Y);

            if (!check.HasTile && check.WallType == 0)
                empty++;

            if (!check.HasTile && check.WallType == ModContent.WallType<CosmostoneWall>())
                walls++;
        }

        if (walls > 1)
            return true;

        return false;

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

        WorldGen.PlaceTile(i, j, type, mute: true);
    }

}
