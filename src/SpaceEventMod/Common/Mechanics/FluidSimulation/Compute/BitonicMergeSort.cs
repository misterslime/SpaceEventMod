using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;

[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct BitonicMergeSort(
    ReadWriteBuffer<uint3> lookupBuffer,
    int numEntries,
    uint groupWidth,
    uint groupHeight,
    uint stepIndex) : IComputeShader
{
    public void Execute()
    {
        uint i = (uint)ThreadIds.X;

        uint hIndex = i & (groupWidth - 1);
        uint indexLeft = hIndex + (groupHeight + 1) * (i / groupWidth);
        uint rightStepSize = stepIndex == 0 ? groupHeight - 2 * hIndex : (groupHeight + 1) / 2;
        uint indexRight = indexLeft + rightStepSize;

        if (indexRight >= numEntries) return;

        uint valueLeft = lookupBuffer[(int)indexLeft].Z;
        uint valueRight = lookupBuffer[(int)indexRight].Z;

        if (valueLeft > valueRight)
        {
            uint3 temp = lookupBuffer[(int)indexLeft];
            lookupBuffer[(int)indexLeft] = lookupBuffer[(int)indexRight];
            lookupBuffer[(int)indexRight] = temp;
        }
    }
}