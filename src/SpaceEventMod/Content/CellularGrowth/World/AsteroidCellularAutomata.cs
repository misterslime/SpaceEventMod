using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework;
using SpaceEventMod.Content.CellularGrowth.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.World;

public enum Output : byte
{
    Empty,
    Wall,
    Solid
}

public static class AsteroidCellularAutomata
{
    public enum CellState : byte
    {
        Alive,
        Dead,
        DefinitelyAlive,
        DefinitelyDead
    }

    public static Output[] Generate(AsteroidCaveType type, int width, int height, int iterations = 4, int percentAreWalls = 40)
    {
        Output[] map = new Output[width * height];

        CellState[] shapeCells = GenerateAsteroidShape(width, height, 6, 50);
        CellState[] caveCells = GenerateCaves(in shapeCells, width, height, 4, 35, type == AsteroidCaveType.Porous);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                bool shapeAlive = shapeCells[i + j * width] == CellState.Alive || shapeCells[i + j * width] == CellState.DefinitelyAlive;
                bool caveAlive = caveCells[i + j * width] == CellState.Alive || caveCells[i + j * width] == CellState.DefinitelyAlive;

                if (shapeAlive)
                    map[i + j * width] = Output.Solid;

                if (shapeAlive && caveAlive && type != AsteroidCaveType.None)
                    map[i + j * width] = Output.Wall;
            }
        }

        return map;
    }

    public static CellState[] GenerateAsteroidShape(int width, int height, int iterations = 4, int percentAreWalls = 40)
    {
        var map = new CellState[width * height];

        int minValue = Math.Min(4, width - 4);
        int maxValue = Math.Max(4, width - 4);

        var randomColumn = WorldGen.genRand.Next(minValue, maxValue);

        int halfWidth = (int)(width / 2f);
        int halfHeight = (int)(height / 2f);

        Vector2 ellipseAB = new Vector2(width, height) * 0.2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x != randomColumn && WorldGen.genRand.Next(100) < percentAreWalls)
                    map[x + y * width] = CellState.Dead;

                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    map[x + y * width] = CellState.DefinitelyDead;

                if (SignedDistanceFunctions.EllipseSDF(new Vector2(x - halfWidth, y - halfHeight), ellipseAB) < 0.0)
                    map[x + y * width] = CellState.DefinitelyAlive;

                if (SignedDistanceFunctions.EllipseSDF(new Vector2(x - halfWidth, y - halfHeight), ellipseAB * 2f) > 0.0)
                    map[x + y * width] = CellState.DefinitelyDead;
            }
        }

        for (var i = 0; i < iterations; i++)
            map = Step(map, width, height, false);

        return map;
    }

    private static bool IsEdgeTile(CellState[] mask, int i, int j, int width, int height)
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

        int adjacentTiles = 0;

        if (mask[i + j * width] == CellState.DefinitelyDead ||
            mask[i + j * width] == CellState.Dead)
            return false;

        foreach (var pos in positions)
        {
            if (pos.X < 0 || pos.Y < 0) continue;
            if (pos.X >= width || pos.Y >= height) continue;

            if (mask[pos.X + pos.Y * width] == CellState.DefinitelyAlive ||
                mask[pos.X + pos.Y * width] == CellState.Alive)
                adjacentTiles++;
        }

        return adjacentTiles != 0 && adjacentTiles != 8;
    }

    public static CellState[] GenerateCaves(in CellState[] mask, int width, int height, int iterations = 4, int percentAreWalls = 40, bool hollow = false)
    {
        var map = new CellState[width * height];

        int minValue = Math.Min(4, width - 4);
        int maxValue = Math.Max(4, width - 4);

        var randomColumn = WorldGen.genRand.Next(minValue, maxValue);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x != randomColumn && WorldGen.genRand.Next(100) < percentAreWalls)
                    map[x + y * width] = CellState.Dead;

                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    map[x + y * width] = CellState.DefinitelyDead;

                if (mask[x + y * width] == CellState.DefinitelyDead ||
                    mask[x + y * width] == CellState.Dead)
                    map[x + y * width] = CellState.DefinitelyDead;

                if (IsEdgeTile(mask, x, y, width, height))
                    map[x + y * width] = CellState.DefinitelyDead;
            }
        }

        // cave entrance
        List<Rectangle> openings = GetCaveEntrances(in mask, width, height);

        foreach (Rectangle opening in openings)
        {
            for (int j = opening.Y; j < opening.Y + opening.Height; j++)
            {
                for (int i = opening.X; i < opening.X + opening.Width; i++)
                {

                    map[i + j * width] = CellState.DefinitelyAlive;
                }
            }
        }

        for (var i = 0; i < iterations; i++)
            map = Step(map, width, height, hollow);

        return map;
    }

    private static List<Rectangle> GetCaveEntrances(in CellState[] mask, int width, int height)
    {
        List<Rectangle> openings = new List<Rectangle>();

        // left
        if (WorldGen.genRand.Next(2) == 0)
        {
            Rectangle opening = new Rectangle();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dir = 1;

                    if (!IsSolid(mask, x, y, width) || !IsSolid(mask, x, y + 1, width) || !IsSolid(mask, x, y + 2, width))
                        continue;

                    if (IsSolid(mask, x - dir, y, width) || IsSolid(mask, x - dir, y + 1, width) || IsSolid(mask, x - dir, y + 2, width))
                        continue;

                    if (Math.Abs(y - (height / 2)) < Math.Abs(opening.Y - (height / 2)))
                        opening = new Rectangle(Math.Min(x, x + 3 * dir + 1), y, 5, 3);
                }
            }

            openings.Add(opening);
        }

        // right
        if (WorldGen.genRand.Next(2) == 0)
        {
            Rectangle opening = new Rectangle();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dir = -1;

                    if (!IsSolid(mask, x, y, width) || !IsSolid(mask, x, y + 1, width) || !IsSolid(mask, x, y + 2, width))
                        continue;

                    if (IsSolid(mask, x - dir, y, width) || IsSolid(mask, x - dir, y + 1, width) || IsSolid(mask, x - dir, y + 2, width))
                        continue;

                    if (Math.Abs(y - (height / 2)) < Math.Abs(opening.Y - (height / 2)))
                        opening = new Rectangle(Math.Min(x, x + 3 * dir + 1), y, 5, 3);
                }
            }

            openings.Add(opening);
        }

        // down
        if (WorldGen.genRand.Next(2) == 0)
        {
            Rectangle opening = new Rectangle();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dir = -1;

                    if (!IsSolid(mask, x, y, width) || !IsSolid(mask, x + 1, y, width) || !IsSolid(mask, x + 2, y, width))
                        continue;

                    if (IsSolid(mask, x, y - dir, width) || IsSolid(mask, x + 1, y - dir, width) || IsSolid(mask, x + 2, y - dir, width))
                        continue;

                    if (Math.Abs(x - (width / 2)) < Math.Abs(opening.X - (width / 2)))
                        opening = new Rectangle(x, Math.Min(y, y + 3 * dir + 1), 3, 5);
                }
            }

            openings.Add(opening);
        }

        return openings;
    }

    private static bool IsSolid(CellState[] mask, int i, int j, int width)
    {
        return mask[i + j * width] == CellState.DefinitelyAlive ||
               mask[i + j * width] == CellState.Alive;
    }

    private static CellState[] Step(CellState[] map, int width, int height, bool hollow)
    {
        var newMap = new CellState[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (newMap[x + y * width] != CellState.DefinitelyAlive || newMap[x + y * width] != CellState.DefinitelyDead)
                    newMap[x + y * width] = PlaceDeadCellLogic(map, width, height, x, y, hollow);
            }
        }

        return newMap;
    }

    private static CellState PlaceDeadCellLogic(CellState[] map, int width, int height, int x, int y, bool hollow)
    {
        if (map[x + y * width] == CellState.DefinitelyAlive)
            return CellState.DefinitelyAlive;

        if (map[x + y * width] == CellState.DefinitelyDead)
            return CellState.DefinitelyDead;

        bool shouldHollow = hollow && CountNearbyDeadCells(map, width, height, x, y) <= 2;

        if (CountAdjacentDeadCells(map, width, height, x, y) >= 5 || shouldHollow)
            return CellState.Dead;

        return CellState.Alive;
    }

    private static int CountAdjacentDeadCells(CellState[] map, int width, int height, int x, int y)
    {
        var deadCells = 0;

        for (var mapX = x - 1; mapX <= x + 1; mapX++)
        {
            for (var mapY = y - 1; mapY <= y + 1; mapY++)
            {
                if (map[mapX + mapY * width] == CellState.Dead || map[mapX + mapY * width] == CellState.DefinitelyDead)
                    deadCells++;
            }
        }

        return deadCells;
    }

    private static int CountNearbyDeadCells(CellState[] map, int width, int height, int x, int y)
    {
        var deadCells = 0;

        for (var mapX = x - 2; mapX <= x + 2; mapX++)
        {
            for (var mapY = y - 2; mapY <= y + 2; mapY++)
            {
                if (Math.Abs(mapX - x) == 2 && Math.Abs(mapY - y) == 2)
                    continue;

                if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                    continue;

                if (map[mapX + mapY * width] == CellState.Dead || map[mapX + mapY * width] == CellState.DefinitelyDead)
                    deadCells++;
            }
        }

        return deadCells;
    }
}

