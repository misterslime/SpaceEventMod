using ComputeSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;

[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct DrawToTexture(
    ReadWriteBuffer<float2> positionBuffer,
    ReadWriteBuffer<float2> velocityBuffer,
    IReadWriteNormalizedTexture2D<float4> texture,
    float4 restColor,
    float4 movingColor,
    float2 middle) : IComputeShader
{
    public void Execute()
    {
        float2 position = positionBuffer[ThreadIds.X];
        float2 velocity = velocityBuffer[ThreadIds.X];

        float speedT = Hlsl.Dot(velocity, velocity) / 5f;
        speedT = Hlsl.Clamp(speedT, 0, 1);

        float2 vectr = middle + position * 15f;

        float4 color = Hlsl.Lerp(restColor, movingColor, speedT);

        int2 coords = new Int2((int)vectr.X, (int)vectr.Y);

        texture[coords] = color;
        texture[coords].A = 1f;
    }
}
