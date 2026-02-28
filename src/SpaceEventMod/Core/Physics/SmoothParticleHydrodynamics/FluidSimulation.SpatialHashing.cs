using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.SmoothParticleHydrodynamics;

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

    [StructLayout(LayoutKind.Explicit, Size = 12, Pack = 4)]
    private struct Entry(int index, uint cellKey, uint hash) : IComparable<Entry>
    {
        [FieldOffset(0)]
        public readonly int Index = index;

        [FieldOffset(4)]
        public readonly uint Key = cellKey;

        [FieldOffset(8)]
        public readonly uint Hash = hash;

        public int CompareTo(Entry other) => Key.CompareTo(other.Key);
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct Neighbour(int index, float squareDistance, Vector2 offset)
    {
        [FieldOffset(0)]
        public readonly int Index = index;

        [FieldOffset(4)]
        public readonly float SquareDistance = squareDistance;

        [FieldOffset(8)]
        public readonly Vector2 Offset = offset;
    }

    private List<Neighbour>[] s_neighbours;
    private Entry[] s_spatialLookup;
    private int[] s_startIndices;

    private void UpdateSpatialHash(int i, float size)
    {
        var cellCoords = PositionToCellCoord(_predictedPositions[i], size);
        var hash = HashCell(cellCoords.X, cellCoords.Y);
        var cellKey = GetKeyFromHash(hash);
        s_spatialLookup[i] = new Entry(i, cellKey, hash);
        s_startIndices[i] = int.MaxValue;
    }

    private void SortAndCalculateOffsets()
    {
        Array.Sort(s_spatialLookup);

        Parallel.For(0, _predictedPositions.Length, i =>
        {
            var key = (int)s_spatialLookup[i].Key;
            var keyPrev = i == 0 ? uint.MaxValue : s_spatialLookup[i - 1].Key;

            if (keyPrev != key)
            {
                s_startIndices[key] = i;
            }
        });

        Parallel.For(0, _predictedPositions.Length, GetNeighbours);
    }

    private void GetNeighbours(int i)
    {
        if (s_neighbours[i] is null)
            s_neighbours[i] = new List<Neighbour>(10);
        else
            s_neighbours[i].Clear();

        var point = _predictedPositions[i];
        var coords = PositionToCellCoord(point, _smoothingRadius);
        var squareRadius = _smoothingRadius * _smoothingRadius;

        foreach (var cell in s_cellOffsets)
        {
            var coord = coords + cell;

            var hash = HashCell(coord.X, coord.Y);
            var cellKey = (int)GetKeyFromHash(hash);
            var cellStartIndex = s_startIndices[cellKey];

            for (var j = cellStartIndex; j < s_spatialLookup.Length; j++)
            {
                if (s_spatialLookup[j].Key != cellKey) break;

                if (s_spatialLookup[j].Hash != hash) continue;

                var particleIndex = s_spatialLookup[j].Index;
                var offset = _predictedPositions[particleIndex] - point;
                var squareDistance = Vector2.Dot(offset, offset);

                if (squareDistance > squareRadius) continue;

                s_neighbours[i].Add(new Neighbour(particleIndex, squareDistance, offset));
            }
        }
    }

    private Point PositionToCellCoord(Vector2 point, float radius)
    {
        var cellX = (int)(point.X / radius);
        var cellY = (int)(point.Y / radius);
        return new Point(cellX, cellY);
    }

    private uint HashCell(int cellX, int cellY)
    {
        var a = (uint)cellX * HASH_KEY_X;
        var b = (uint)cellY * HASH_KEY_Y;
        return a + b;
    }

    private uint GetKeyFromHash(uint hash) => hash % (uint)s_spatialLookup.Length;
}
