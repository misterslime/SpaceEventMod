using Microsoft.Xna.Framework;
using SteelSeries.GameSense;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Mechanics.SmoothParticleHydrodynamics;

internal partial class FluidSimulation
{
    private const float SMOOTHING_RADIUS = 0.35f;
    private const float TARGET_DENSITY = 1.2f;
    private const float PRESSURE_MULTIPLIER = 90f;
    private const float NEAR_PRESSURE_MULTIPLIER = 8f;
    private const float VISCOSITY = 0.075f;

    private static float s_gravity;
    private static int s_numParticles;
    private static Vector2[] s_positions;
    private static Vector2[] s_predictedPositions;
    private static Vector2[] s_velocities;
    private static float[] s_densities;
    private static float[] s_nearDensities;

    private void SimulationStep(float deltaTime)
    {
        Parallel.For(0, s_numParticles, i =>
        {
            s_velocities[i] += Vector2.UnitY * s_gravity * deltaTime;
            s_predictedPositions[i] = s_positions[i] + s_velocities[i] * deltaTime;

            UpdateSpatialHash(i, SMOOTHING_RADIUS);
        });

        SortAndCalculateOffsets();

        Parallel.For(0, s_numParticles, i =>
        {
            (float Density, float NearDensity) densitites = CalculateDensity(i);

            s_densities[i] = densitites.Density;
            s_nearDensities[i] = densitites.NearDensity;
        });

        Parallel.For(0, s_numParticles, i =>
        {
            Vector2 pressure = CalculatePressureGradient(i);
            Vector2 acceleration = pressure / s_densities[i];
            s_velocities[i] += acceleration * deltaTime;
        });

        Parallel.For(0, s_numParticles, i =>
        {
            Vector2 viscosity = CalculateViscosityForce(i);
            s_velocities[i] += viscosity * deltaTime;
        });

        Parallel.For(0, s_numParticles, i =>
        {
            s_positions[i] += s_velocities[i] * deltaTime;
            ResolveCollisions(ref s_positions[i], ref s_velocities[i]);
        });
    }

    private void ResolveCollisions(ref Vector2 position, ref Vector2 velocity)
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

    private (float Density, float NearDensity) CalculateDensity(int index)
    {
        Vector2 position = s_predictedPositions[index];

        float density = 0;
        float nearDensity = 0;

        for (int i = 0; i < s_neighbours[index].Count; i++)
        {
            float distance = MathF.Sqrt(s_neighbours[index][i].SquareDistance);
            density += SpikyKernelPow2(SMOOTHING_RADIUS, distance);
            nearDensity += SpikyKernelPow3(SMOOTHING_RADIUS, distance);
        }

        return (density, nearDensity);
    }

    private Vector2 CalculatePressureGradient(int index)
    {
        Vector2 gradient = Vector2.Zero;

        float density = s_densities[index];
        float densityNear = s_nearDensities[index];
        float pressure = ConvertDensityToPressure(density);
        float nearPressure = ConvertNearDensityToNearPressure(densityNear);

        for (int i = 0; i < s_neighbours[index].Count; i++)
        {
            Neighbour neighbour = s_neighbours[index][i];

            if (neighbour.Index == index) continue;

            float distance = MathF.Sqrt(neighbour.SquareDistance);

            Vector2 direction = distance == 0 ? Vector2.Zero : neighbour.Offset / distance;

            float neighbourDensity = s_densities[neighbour.Index];
            float neighbourNearDensity = s_nearDensities[neighbour.Index];
            float neighbourPressure = ConvertDensityToPressure(neighbourDensity);
            float neighbourNearPressure = ConvertNearDensityToNearPressure(neighbourNearDensity);

            float sharedPressure = (pressure + neighbourPressure) * 0.5f;
            float sharedNearPressure = (nearPressure + neighbourNearPressure) * 0.5f;

            float slope = DerivativeSpikyPow2(SMOOTHING_RADIUS, distance);
            float nearSlope = DerivativeSpikyPow3(SMOOTHING_RADIUS, distance);

            gradient += direction * sharedPressure * slope / neighbourDensity;
            gradient += direction * sharedNearPressure * slope / neighbourNearDensity;
        }

        return gradient;
    }


    private float ConvertNearDensityToNearPressure(float nearDensity)
    {
        float nearPressure = nearDensity * NEAR_PRESSURE_MULTIPLIER;
        return nearPressure;
    }

    private float ConvertDensityToPressure(float density)
    {
        float densityError = density - TARGET_DENSITY;
        float pressure = densityError * PRESSURE_MULTIPLIER;
        return pressure;
    }

    private Vector2 CalculateViscosityForce(int index)
    {
        Vector2 force = Vector2.Zero;
        Vector2 position = s_predictedPositions[index];

        for (int i = 0; i < s_neighbours[index].Count; i++)
        {
            Neighbour neighbour = s_neighbours[index][i];
            float influence = SmoothingKernelPoly6(SMOOTHING_RADIUS, MathF.Sqrt(neighbour.SquareDistance));
            force += (s_velocities[neighbour.Index] - s_velocities[index]) * influence;
        }

        return force * VISCOSITY;
    }
}
