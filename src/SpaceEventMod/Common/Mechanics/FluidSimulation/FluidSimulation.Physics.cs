using Microsoft.Xna.Framework;
using SteelSeries.GameSense;
using System;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation;

internal partial class FluidSimulation
{
    private const float SMOOTHING_RADIUS = 1.2f;

    private static float s_gravity;
    private static int s_numParticles;
    private static Vector2[] s_positions;
    private static Vector2[] s_predictedPositions;
    private static Vector2[] s_velocities;
    private static float[] s_densities;

    private void SimulationStep(float deltaTime)
    {
        Parallel.For(0, s_numParticles, i =>
        {
            s_velocities[i] += Vector2.UnitY * s_gravity * deltaTime;
            s_predictedPositions[i] = s_positions[i] + s_velocities[i] * deltaTime;
        });

        UpdateSpatialLookup(SMOOTHING_RADIUS);

        Parallel.For(0, s_numParticles, i =>
        {
            s_densities[i] = CalculateDensity(s_predictedPositions[i]);
        });

        Parallel.For(0, s_numParticles, i =>
        {
            Vector2 force = CalculatePressureGradient(i);
            Vector2 acceleration = force / s_densities[i];
            s_velocities[i] += acceleration * deltaTime;
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

    private float SmoothingKernel(float radius, float distance)
    {
        if (distance > radius) return 0;
        float volume = MathF.PI * MathF.Pow(radius, 4) / 6f;
        return (radius - distance) * (radius - distance) / volume;
    }

    private float SmoothingKernelDerivative(float radius, float distance)
    {
        if (distance > radius) return 0;
        float scale = 12 / (MathF.PI * MathF.Pow(radius, 4));
        return -(distance - radius) * scale;
    }

    private float CalculateDensity(Vector2 position)
    {
        float density = 0;
        const float mass = 1;

        ForeachPointInRadius(position, (Vector2 offset, float squareDistance, int i) =>
        {
            float distance = MathF.Sqrt(squareDistance);
            float influence = SmoothingKernel(SMOOTHING_RADIUS, distance);
            density += mass * influence;
        });

        return density;
    }

    private Vector2 CalculatePressureGradient(int index)
    {
        Vector2 gradient = Vector2.Zero;
        const float mass = 1;

        const float targetDensity = 12.75f;
        const float pressureMultiplier = 500f;

        float ConvertDensityToPressure(float density)
        {
            float densityError = density - targetDensity;
            float pressure = densityError * pressureMultiplier;
            return pressure;
        }

        float CalculateSharedPressure(float densityA, float densityB)
        {
            float pressureA = ConvertDensityToPressure(densityA);
            float pressureB = ConvertDensityToPressure(densityB);
            return (pressureA + pressureB) / 2;
        }

        ForeachPointInRadius(s_predictedPositions[index], (Vector2 offset, float squareDistance, int i) => 
        {
            if (i == index) return;

            float distance = MathF.Sqrt(squareDistance);

            Vector2 direction = distance == 0 ? Vector2.Zero : offset / distance;

            float slope = SmoothingKernelDerivative(SMOOTHING_RADIUS, distance);
            float density = s_densities[i];
            float sharedPressure = -CalculateSharedPressure(density, s_densities[i]);
            gradient += direction * sharedPressure * slope * mass / density;
        });

        return gradient;
    }
}
