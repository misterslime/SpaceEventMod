using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;

[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct ApplyGravity(
    ReadWriteBuffer<float2> velocityBuffer,
    ReadWriteBuffer<float2> positionsBuffer,
    ReadWriteBuffer<float2> predictedPositionsBuffer,
    float gravity,
    float deltaTime) : IComputeShader
{
    public void Execute()
    {
        velocityBuffer[ThreadIds.X].Y += 1f * gravity * deltaTime;
        predictedPositionsBuffer[ThreadIds.X] = positionsBuffer[ThreadIds.X] + velocityBuffer[ThreadIds.X] * (1 / 120f);
    }
}
