using ComputeSharp;
using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

using Vector2 = System.Numerics.Vector2;

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

    private static ReadWriteBuffer<uint3> spatialLookupBuffer;
    private static ReadWriteBuffer<int> startIndicesBuffer;

    private int NextPowerOfTwo(int v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;

        return v;
    }

    private void Sort()
    {
        // Launch each step of the sorting algorithm (once the previous step is complete)
        // Number of steps = [log2(n) * (log2(n) + 1)] / 2
        // where n = nearest power of 2 that is greater or equal to the number of inputs
        int numStages = (int)Math.Log(NextPowerOfTwo(s_numParticles), 2);

        for (int stageIndex = 0; stageIndex < numStages; stageIndex++)
        {
            for (int stepIndex = 0; stepIndex < stageIndex + 1; stepIndex++)
            {
                // Calculate some pattern stuff
                int groupWidth = 1 << (stageIndex - stepIndex);
                int groupHeight = 2 * groupWidth - 1;

                GraphicsDevice.GetDefault().For(NextPowerOfTwo(s_numParticles) / 2, new BitonicMergeSort(spatialLookupBuffer, s_numParticles, (uint)groupWidth, (uint)groupHeight, (uint)stepIndex));
            }
        }

        GraphicsDevice.GetDefault().For(s_numParticles, new CalculateOffsets(spatialLookupBuffer, startIndicesBuffer, s_numParticles));

    }

    /*private Point PositionToCellCoord(Vector2 point, float radius)
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
                if (s_spatialLookup[i].Z != cellKey) break;

                int particleIndex = (int)s_spatialLookup[i].X;
                Vector2 offset = s_predictedPositions[particleIndex] - point;
                float squareDistance = Vector2.Dot(offset, offset);

                if (squareDistance <= squareRadius)
                {
                    action(offset, squareDistance, particleIndex);
                }
            }
        }
    }*/
}
