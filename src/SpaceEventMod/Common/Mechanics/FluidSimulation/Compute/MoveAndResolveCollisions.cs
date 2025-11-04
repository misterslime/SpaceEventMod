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
internal readonly partial struct MoveAndResolveCollisions(
    ReadWriteBuffer<float2> positionsBuffer,
    ReadWriteBuffer<float2> velocityBuffer,
    float2 bounds,
    float deltaTime) : IComputeShader
{
    public void Execute()
    {
        positionsBuffer[ThreadIds.X] += velocityBuffer[ThreadIds.X] * deltaTime;

        float collisionDamping = 0.95f;

        if (Hlsl.Abs(positionsBuffer[ThreadIds.X].X) > bounds.X)
        {
            positionsBuffer[ThreadIds.X].X = bounds.X * Hlsl.Sign(positionsBuffer[ThreadIds.X].X);
            velocityBuffer[ThreadIds.X].X *= -1 * collisionDamping;
        }

        if (Hlsl.Abs(positionsBuffer[ThreadIds.X].Y) > bounds.Y)
        {
            positionsBuffer[ThreadIds.X].Y = bounds.Y * Hlsl.Sign(positionsBuffer[ThreadIds.X].Y);
            velocityBuffer[ThreadIds.X].Y *= -1 * collisionDamping;
        }
    }
}
