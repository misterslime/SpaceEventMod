using ComputeSharp;
using SpaceEventMod.Core.Physics.Passes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;

[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct SpatialLookup(
    ReadWriteBuffer<float2> predictedPositionBuffer,
    ReadWriteBuffer<uint3> spatialLookupBuffer,
    ReadWriteBuffer<int> startIndicesBuffer,
    float radius) : IComputeShader
{
    public int2 GetCell2D(float2 position, float radius)
    {
        return (int2)Hlsl.Floor(position / radius);
    }

    public uint HashCell2D(int2 cell)
    {
        uint a = (uint)cell.X * 15823;
        uint b = (uint)cell.Y * 9737333;
        return (a + b);
    }

    public uint KeyFromHash(uint hash, uint tableSize)
    {
        return hash % tableSize;
    }

    public void Execute()
    {
        if (ThreadIds.X >= predictedPositionBuffer.Length) return;

        startIndicesBuffer[ThreadIds.X] = predictedPositionBuffer.Length;

        uint index = (uint)ThreadIds.X;
        int2 cell = GetCell2D(predictedPositionBuffer[(int)index], radius);
        uint hash = HashCell2D(cell);
        uint key = KeyFromHash(hash, (uint)predictedPositionBuffer.Length);
        spatialLookupBuffer[ThreadIds.X] = new uint3(index, hash, key);
    }
}
