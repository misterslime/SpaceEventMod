using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.SmoothParticleHydrodynamics;

internal partial class FluidSimulation
{
    private float Poly6 => 4 / (MathF.PI * MathF.Pow(_smoothingRadius, 8));
    private float SpikyPow3 => 10 / (MathF.PI * MathF.Pow(_smoothingRadius, 5));
    private float SpikyPow2 => 6 / (MathF.PI * MathF.Pow(_smoothingRadius, 4));
    private float SpikyPow3Derivative => 30 / (MathF.PI * MathF.Pow(_smoothingRadius, 5));
    private float SpikyPow2Derivative => 12 / (MathF.PI * MathF.Pow(_smoothingRadius, 4));

    private float SmoothingKernelPoly6(float radius, float distance)
    {
        if (distance >= radius) return 0;
        float value = radius * radius - distance * distance;
        return value * value * value * Poly6;
    }

    private float SpikyKernelPow3(float radius, float distance)
    {
        if (distance >= radius) return 0;
        float value = radius - distance;
        return value * value * value * SpikyPow3;
    }

    private float SpikyKernelPow2(float radius, float distance)
    {
        if (distance >= radius) return 0;
        float value = radius - distance;
        return value * value * SpikyPow2;
    }

    private float DerivativeSpikyPow3(float radius, float distance)
    {
        if (distance > radius) return 0;
        float value = radius - distance;
        return -value * value * SpikyPow3Derivative;
    }

    private float DerivativeSpikyPow2(float radius, float distance)
    {
        if (distance > radius) return 0;
        float value = radius - distance;
        return -value * SpikyPow2Derivative;
    }


    private Vector3 SignedDistanceGradientSegment(Vector2 p, Vector2 a, Vector2 b, float r)
    {
        Vector2 ba = b - a, pa = p - a;
        float h = MathHelper.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0.0f, 1.0f);
        Vector2 q = pa - h * ba;
        float d = q.Length();
        Vector2 g = q / d;
        return new Vector3(d - r, g.X, g.Y);
    }

    private Vector3 SmoothMinimum(Vector3 a, Vector3 b, float k)
    {
        k *= 4.0f;
        float h = MathF.Max(k - MathF.Abs(a.X - b.Y), 0.0f) / (2.0f * k);
        Vector2 gradient = Vector2.Lerp(new(a.Y, a.Z), new(b.Y, b.Z), (a.Y < b.X) ? h : 1.0f - h);
        return new Vector3(MathF.Min(a.X, b.Y) - h * h * k, gradient.X, gradient.Y);
    }
}
