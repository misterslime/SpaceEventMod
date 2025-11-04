using ComputeSharp;
using ComputeSharp.Resources;
using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;
using SpaceEventMod.Content.Items;
using SteelSeries.GameSense;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation;

internal partial class FluidSimulation
{
    private const float SMOOTHING_RADIUS = 2f;

    private static float s_gravity;
    private static int s_numParticles;
    private static ReadWriteBuffer<float2> positionsBuffer;
    private static ReadWriteBuffer<float2> predictedPositionsBuffer;
    private static ReadWriteBuffer<float2> velocityBuffer;
    private static ReadWriteBuffer<float> densitiesBuffer;
    private static ReadWriteBuffer<float> nearDensitiesBuffer;

    private void SimulationStep(float deltaTime)
    {
        int2[] offsets =
        {
            new int2(-1, 1),
            new int2(0, 1),
            new int2(1, 1),
            new int2(-1, 0),
            new int2(0, 0),
            new int2(1, 0),
            new int2(-1, -1),
            new int2(0, -1),
            new int2(1, -1)
        };

        using ReadWriteBuffer<int2> offsetsBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(offsets);

        /*Parallel.For(0, s_numParticles, i =>
        {
            s_velocities[i].Y += s_gravity * deltaTime;
            s_predictedPositions[i] = s_positions[i] + s_velocities[i] * (1 / 120f);
        });

        Parallel.For(0, s_predictedPositions.Length, i =>
        {
            Point cellCoords = PositionToCellCoord(s_predictedPositions[i], SMOOTHING_RADIUS);
            uint cellKey = GetKeyFromHash(HashCell(cellCoords.X, cellCoords.Y));
            s_spatialLookup[i] = new Entry(i, cellKey);
            s_startIndices[i] = int.MaxValue;
        });

        Array.Sort(s_spatialLookup);

        Parallel.For(0, s_predictedPositions.Length, i =>
        {
            uint key = s_spatialLookup[i].CellKey;
            uint keyPrev = i == 0 ? uint.MaxValue : s_spatialLookup[i - 1].CellKey;

            if (keyPrev != key)
            {
                s_startIndices[key] = i;
            }
        });
        
         Parallel.For(0, s_numParticles, i =>
        {
            float2 densities = CalculateDensity(s_predictedPositions[i]);
            s_densities[i] = densities.X;
            s_nearDensities[i] = densities.Y;
        });*/

        // Launch the shaders
        GraphicsDevice.GetDefault().For(s_numParticles, new ApplyGravity(velocityBuffer, positionsBuffer, predictedPositionsBuffer, s_gravity, deltaTime));
        GraphicsDevice.GetDefault().For(s_numParticles, new SpatialLookup(predictedPositionsBuffer, spatialLookupBuffer, startIndicesBuffer, SMOOTHING_RADIUS));

        //Sort();

        GraphicsDevice.GetDefault().For(s_numParticles, new CalculateDensities(SMOOTHING_RADIUS, s_numParticles, spatialLookupBuffer, startIndicesBuffer, predictedPositionsBuffer, densitiesBuffer, nearDensitiesBuffer, offsetsBuffer));

        /*Main.NewText(s_densities[94] + ", " + s_nearDensities[42]);

        Parallel.For(0, s_numParticles, i =>
        {
            float2 densities = CalculateDensity(s_predictedPositions[i]);
            s_densities[i] = densities.X;
            s_nearDensities[i] = densities.Y;
        });

        Main.NewText(s_densities[94] + ", " + s_nearDensities[42]);

        Parallel.For(0, s_numParticles, i =>
        {
            Vector4 forces = CalculatePressureGradient(i);

            Vector2 acceleration = new Vector2(forces.X, forces.Y) / s_densities[i];
            Vector2 nearAcceleration = new Vector2(forces.Z, forces.W) / s_nearDensities[i];
            s_velocities[i] += acceleration * deltaTime;
            //s_velocities[i] -= nearAcceleration * deltaTime;
        });*/

        /*Parallel.For(0, s_numParticles, i =>
        {
            s_positions[i] += s_velocities[i] * deltaTime;
            ResolveCollisions(ref s_positions[i], ref s_velocities[i]);
        });*/

        GraphicsDevice.GetDefault().For(s_numParticles, new MoveAndResolveCollisions(positionsBuffer, velocityBuffer, s_halfBoundsSize, deltaTime));
    }

    /*private void ResolveCollisions(ref Vector2 position, ref Vector2 velocity)
    {
        float collisionDamping = 0.95f;

        if (MathF.Abs(position.X) > s_halfBoundsSize.X)
        {
            position.X = s_halfBoundsSize.X * MathF.Sign(position.X);
            velocity.X *= -1 * collisionDamping;
        }

        if (MathF.Abs(position.Y) > s_halfBoundsSize.Y)
        {
            position.Y = s_halfBoundsSize.Y * MathF.Sign(position.Y);
            velocity.Y *= -1 * collisionDamping;
        }
    }
    private float ViscosityKernel(float radius, float distance)
    {
        if (distance > radius) return 0;
        float volume = MathF.PI * MathF.Pow(radius, 8) / 4f;
        float value = radius * radius - distance * distance;
        return value * value * value / volume;
    }

    private Vector2 CalculateViscosityForce(int index)
    {
        Vector2 viscosity = Vector2.Zero;
        Vector2 position = s_positions[index];

        ForeachPointInRadius(position, (Vector2 offset, float squareDistance, int i) =>
        {
            float distance = MathF.Sqrt(squareDistance);
            float influence = ViscosityKernel(SMOOTHING_RADIUS, distance);
            viscosity += (s_velocities[i] - s_velocities[index]) * influence;
        });

        return viscosity * 0.1f;
    }

    private float NearSmoothingKernel(float radius, float distance)
    {
        if (distance > radius) return 0;
        float volume = 10 / (MathF.PI * MathF.Pow(radius, 5));
        float value = (radius - distance);
        return value * value * value / volume;
    }

    private float NearSmoothingKernelDerivative(float radius, float distance)
    {
        if (distance > radius) return 0;
        float scale = 30 / (MathF.Pow(radius, 5) * MathF.PI);
        float value = (radius - distance);
        return value * value * scale;
    }

    private float SmoothingKernel(float radius, float distance)
    {
        if (distance > radius) return 0;
        float volume = (MathF.PI * MathF.Pow(radius, 4)) / 6;
        float value = (radius - distance);
        return value * value / volume;
    }

    private float SmoothingKernelDerivative(float radius, float distance)
    {
        if (distance > radius) return 0;
        float scale = 12 / (MathF.PI * MathF.Pow(radius, 4));
        float value = (radius - distance);
        return value * scale;
    }

    private Vector2 CalculateDensity(Vector2 position)
    {
        Vector2 density = Vector2.Zero;
        const float mass = 1;

        ForeachPointInRadius(position, (Vector2 offset, float squareDistance, int i) =>
        {
            float distance = MathF.Sqrt(squareDistance);
            float influence = SmoothingKernel(SMOOTHING_RADIUS, distance);
            float nearInfluence = NearSmoothingKernel(SMOOTHING_RADIUS, distance);
            density.X += mass * influence;
            density.Y += mass * nearInfluence;
        });

        return density;
    }

    private Vector4 CalculatePressureGradient(int index)
    {
        Vector2 gradient = Vector2.Zero;
        Vector2 nearGradient = Vector2.Zero;
        const float mass = 1;

        const float targetDensity = 1.2f;
        const float pressureMultiplier = 80;
        const float nearPressureMultiplier = 0;

        Vector2 ConvertDensityToPressure(float density, float nearDensity)
        {
            float densityError = density - targetDensity;
            float pressure = densityError * pressureMultiplier;
            float nearPressure = nearDensity * nearPressureMultiplier;
            return new Vector2(pressure, nearPressure);
        }

        Vector2 CalculateSharedPressure(float densityA, float nearDensityA, float densityB, float nearDensityB)
        {
            Vector2 pressureA = ConvertDensityToPressure(densityA, nearDensityA);
            Vector2 pressureB = ConvertDensityToPressure(densityB, nearDensityB);
            return (pressureA + pressureB) / 2f;
        }

        ForeachPointInRadius(s_predictedPositions[index], (Vector2 offset, float squareDistance, int i) => 
        {
            if (i == index) return;

            float distance = MathF.Sqrt(squareDistance);

            Vector2 direction = distance == 0 ? Vector2.Zero : offset / distance;

            float slope = SmoothingKernelDerivative(SMOOTHING_RADIUS, distance);
            float nearSlope = NearSmoothingKernelDerivative(SMOOTHING_RADIUS, distance);
            float density = s_densities[i];
            float nearDensity = s_nearDensities[i];
            Vector2 sharedPressure = -CalculateSharedPressure(density, nearDensity, s_densities[i], s_nearDensities[i]);

            Vector2 force = direction * slope * mass / density;

            gradient += sharedPressure.X * direction * slope * mass / density;
            nearGradient += sharedPressure.Y * direction * nearSlope * mass / density;
        });

        return new Vector4(gradient, nearGradient.X, nearGradient.Y);
    }*/
}
