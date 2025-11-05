using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Mechanics.SmoothParticleHydrodynamics;

internal partial class FluidSimulation
{
    private readonly static float POLY_6 = 4 / (MathF.PI * MathF.Pow(SMOOTHING_RADIUS, 8));
    private readonly static float SPIKY_POW_3 = 10 / (MathF.PI * MathF.Pow(SMOOTHING_RADIUS, 5));
    private readonly static float SPIKY_POW_2 = 6 / (MathF.PI * MathF.Pow(SMOOTHING_RADIUS, 4));
    private readonly static float SPIKY_POW_3_DERIVATIVE = 30 / (MathF.PI * MathF.Pow(SMOOTHING_RADIUS, 5));
    private readonly static float SPIKY_POW_2_DERIVATIVE = 12 / (MathF.PI * MathF.Pow(SMOOTHING_RADIUS, 4));

    private float SmoothingKernelPoly6(float radius, float distance)
    {
        if (distance >= radius) return 0;
        float value = radius * radius - distance * distance;
        return value * value * value * POLY_6;
    }

    private float SpikyKernelPow3(float radius, float distance)
    {
        if (distance >= radius) return 0;
        float value = radius - distance;
        return value * value * value * SPIKY_POW_3;
    }

    private float SpikyKernelPow2(float radius, float distance)
    {
        if (distance >= radius) return 0;
        float value = radius - distance;
        return value * value * SPIKY_POW_2;
    }

    private float DerivativeSpikyPow3(float radius, float distance)
    {
        if (distance > radius) return 0;
        float value = radius - distance;
        return -value * value * SPIKY_POW_3_DERIVATIVE;
    }

    private float DerivativeSpikyPow2(float radius, float distance)
    {
        if (distance > radius) return 0;
        float value = radius - distance;
        return -value * SPIKY_POW_2_DERIVATIVE;
    }
}
