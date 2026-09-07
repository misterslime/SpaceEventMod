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
}

