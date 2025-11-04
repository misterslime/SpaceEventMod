using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;

[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct CalculateDensities(
    float smoothingRadius,
    int numParticles,
    ReadWriteBuffer<uint3> spatialLookupBuffer,
    ReadWriteBuffer<int> startIndicesBuffer,
    ReadWriteBuffer<float2> predictedPositionBuffer,
    ReadWriteBuffer<float> densitiesBuffer,
    ReadWriteBuffer<float> nearDensitiesBuffer,
    ReadWriteBuffer<int2> offsets) : IComputeShader
{
    public float SpikyKernelPow3(float dst, float radius)
    {
        if (dst < radius)
        {
            float v = radius - dst;
            return v * v * v * (10 / (3.14159265f * Hlsl.Pow(smoothingRadius, 5)));
        }
        return 0;
    }

    public float SpikyKernelPow2(float dst, float radius)
    {
        if (dst < radius)
        {
            float v = radius - dst;
            return v * v * (6 / (3.14159265f * Hlsl.Pow(smoothingRadius, 4)));
        }
        return 0;
    }

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
        if (ThreadIds.X >= numParticles) return;

        float2 position = predictedPositionBuffer[ThreadIds.X];

        float density = 0;
        float nearDensity = 0;

        int2 coords = GetCell2D(position, smoothingRadius);
        float squareRadius = smoothingRadius * smoothingRadius;

        for (int i = 0; i < 9; i++)
        {
            uint hash = HashCell2D(coords + offsets[i]);
            int key = (int)KeyFromHash(hash, (uint)numParticles);
            int currIndex = startIndicesBuffer[key];

            while (currIndex < numParticles)
            {
                uint3 indexData = spatialLookupBuffer[currIndex];
                currIndex++;

                if (indexData.Z != key) break;

                if (indexData.Y != hash) continue;

                int neighbourIndex = (int)indexData.X;
                float2 neighbourPos = predictedPositionBuffer[neighbourIndex];
                float2 offsetToNeighbour = neighbourPos - position;
                float sqrDstToNeighbour = Hlsl.Dot(offsetToNeighbour, offsetToNeighbour);

                if (sqrDstToNeighbour > squareRadius) continue;

                float dst = Hlsl.Sqrt(sqrDstToNeighbour);
                density += SpikyKernelPow2(smoothingRadius, dst);
                nearDensity += SpikyKernelPow3(smoothingRadius, dst);
            }
        }

        densitiesBuffer[ThreadIds.X] = density;
        nearDensitiesBuffer[ThreadIds.X] = nearDensity;
    }
}
