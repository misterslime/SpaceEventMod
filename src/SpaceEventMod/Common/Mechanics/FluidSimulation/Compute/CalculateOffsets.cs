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
internal readonly partial struct CalculateOffsets(
    ReadWriteBuffer<uint3> spatialLookupBuffer,
    ReadWriteBuffer<int> startIndicesBuffer,
    int numEntries) : IComputeShader
{
    public void Execute()
    {
        if (ThreadIds.X >= numEntries) { return; }

        uint key = spatialLookupBuffer[ThreadIds.X].Z;
        uint keyPrev = ThreadIds.X == 0 ? (uint)numEntries : spatialLookupBuffer[ThreadIds.X - 1].Z;

        if (key != keyPrev)
        {
            startIndicesBuffer[(int)key] = ThreadIds.X;
        }
    }
}
