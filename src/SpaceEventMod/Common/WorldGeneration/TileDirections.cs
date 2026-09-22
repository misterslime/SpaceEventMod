using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.WorldGeneration;

internal static class TileDirections
{
    public static readonly Point[] WithCorners = [
        new Point(0, -1),
        new Point(0, 1),

        new Point(-1, 0),
        new Point(1, 0),

        new Point(-1, -1),
        new Point(1, -1),

        new Point(-1, 1),
        new Point(1, 1)
    ];

    public static readonly Point[] NoCorners = [
        new Point(0, -1),
        new Point(0, 1),

        new Point(-1, 0),
        new Point(1, 0)
    ];

    /// <summary>
    /// Return tiles adjacent to the connected tile.
    /// </summary>
    /// <param name="i">The X coordinate of the center tile.</param>
    /// <param name="j">The Y coordinate of the center tile.</param>
    /// <param name="searchShape">The general search space, represented by an array of relative tile positions.</param>
    /// <returns></returns>
    public static Point[] GetConnectedTiles(int i, int j, Point[] searchShape)
    {
        var ret = new Point[searchShape.Length];

        for (int x = 0; x < WithCorners.Length; x++)
            ret[x] = searchShape[x] + new Point(i, j);

        return ret;
    }

    public static (Point[] tilePositions, Point min, Point max) SearchFromTile(int i, int j, int depth, Point[] searchShape, Func<Tile, bool> condition)
    {
        Point max = new Point(i, j);
        Point min = new Point(i, j);

        HashSet<Point> tiles = new();
        List<(int depth, Point point)> found = new() { (depth, new Point(i, j)) };
        while (found.Count > 0)
        {
            for (int n = 0; n < found.Count;)
            {
                var point = found[found.Count - 1];
                found.RemoveAt(found.Count - 1);

                if (tiles.Contains(point.point)) continue;
                if (point.depth < 0) continue;
                if (point.point.X < 0 || point.point.X >= Main.maxTilesX ||
                    point.point.Y < 0 || point.point.Y >= Main.maxTilesY) continue;

                max.X = Math.Max(max.X, point.point.X);
                max.Y = Math.Max(max.Y, point.point.Y);

                min.X = Math.Min(min.X, point.point.X);
                min.Y = Math.Min(min.Y, point.point.Y);

                if (!condition(Main.tile[point.point])) continue;

                tiles.Add(point.point);

                foreach (var p in GetConnectedTiles(point.point.X, point.point.Y, searchShape))
                    found.Add((point.depth - 1, p));
            }
        }

        return (tiles.ToArray(), min, max);
    }

    public static Point GetNearestSolid(Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ)
    {
        var points = new List<Vector2>();

        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                float num = Math.Abs((float)i - compareSpot.X / 16f);
                float num2 = Math.Abs((float)j - compareSpot.Y / 16f);
                if (!(Math.Sqrt(num * num + num2 * num2) < (double)radius))
                    continue;

                Tile tile = Main.tile[i, j];
                if (tile != null && tile.active() && WorldGen.SolidOrSlopedTile(tile))
                    points.Add(new Point(i, j).ToWorldCoordinates());
            }
        }

        var nearest = points.OrderBy(x => Math.Abs((x - compareSpot).LengthSquared())).First().ToTileCoordinates();

        return nearest;
    }
}

