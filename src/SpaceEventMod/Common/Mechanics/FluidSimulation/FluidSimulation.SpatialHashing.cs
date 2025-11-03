using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation;

internal partial class FluidSimulation
{
    private const uint HASH_KEY_X = 15823;
    private const uint HASH_KEY_Y = 9737333;

    private static Point[] s_cellOffsets =
    {
        new Point(-1, 1),
        new Point(0, 1),
        new Point(1, 1),
        new Point(-1, 0),
        new Point(0, 0),
        new Point(1, 0),
        new Point(-1, -1),
        new Point(0, -1),
        new Point(1, -1)
    };

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    private struct Entry(int index, uint cellKey) : IComparable<Entry>
    {
        [FieldOffset(0)]
        public readonly int Index = index;

        [FieldOffset(4)]
        public readonly uint CellKey = cellKey;

        public int CompareTo(Entry other) => CellKey.CompareTo(other.CellKey);
    }

    private static Entry[] s_spatialLookup;
    private static int[] s_startIndices;

    private void UpdateSpatialLookup(float radius)
    {
        Parallel.For(0, s_predictedPositions.Length, i =>
        {
            Point cellCoords = PositionToCellCoord(s_predictedPositions[i], radius);
            uint cellKey = GetKeyFromHash(HashCell(cellCoords.X, cellCoords.Y));
            s_spatialLookup[i] = new Entry(i, cellKey);
            s_startIndices[i] = int.MaxValue;
        });

        Array.Sort(s_spatialLookup);

        Parallel.For(0, s_predictedPositions.Length, i =>
        {
            int key = (int)s_spatialLookup[i].CellKey;
            uint keyPrev = i == 0 ? uint.MaxValue : s_spatialLookup[i - 1].CellKey;

            if (keyPrev != key)
            {
                s_startIndices[key] = i;
            }
        });
    }

    private Point PositionToCellCoord(Vector2 point, float radius)
    {
        int cellX = (int)(point.X / radius);
        int cellY = (int)(point.Y / radius);
        return new Point(cellX, cellY);
    }

    private uint HashCell(int cellX, int cellY)
    {
        uint a = (uint)cellX * HASH_KEY_X;
        uint b = (uint)cellY * HASH_KEY_Y;
        return a + b;
    }

    private uint GetKeyFromHash(uint hash) => hash % (uint)s_spatialLookup.Length;


    private void ForeachPointInRadius(Vector2 point, Action<Vector2, float, int> action)
    {
        Point coords = PositionToCellCoord(point, SMOOTHING_RADIUS);
        float squareRadius = SMOOTHING_RADIUS * SMOOTHING_RADIUS;

        foreach (var cell in s_cellOffsets)
        {
            Point coord = coords + cell;

            int cellKey = (int)GetKeyFromHash(HashCell(coord.X, coord.Y));
            int cellStartIndex = s_startIndices[cellKey];

            for (int i = cellStartIndex; i < s_spatialLookup.Length; i++)
            {
                if (s_spatialLookup[i].CellKey != cellKey) break;

                int particleIndex = s_spatialLookup[i].Index;
                Vector2 offset = s_predictedPositions[particleIndex] - point;
                float squareDistance = Vector2.Dot(offset, offset);

                if (squareDistance <= squareRadius)
                {
                    action(offset, squareDistance, particleIndex);
                }
            }
        }
    }
}
