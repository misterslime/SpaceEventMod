using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Events.Space;
using SteelSeries.GameSense;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Physics.SmoothParticleHydrodynamics;

internal partial class FluidSimulation
{
    private void SimulationStep(float deltaTime)
    {
        Parallel.For(0, _particles, i =>
        {
            _predictedPositions[i] = _positions[i] + _velocities[i] * deltaTime;

            UpdateSpatialHash(i, _smoothingRadius);
        });

        SortAndCalculateOffsets();

        Parallel.For(0, _particles, i =>
        {
            (float Density, float NearDensity) densitites = CalculateDensity(i);

            _densities[i] = densitites.Density;
            _nearDensities[i] = densitites.NearDensity;
        });

        Parallel.For(0, _particles, i =>
        {
            Vector2 pressure = CalculatePressureGradient(i);
            Vector2 acceleration = pressure / _densities[i];
            _velocities[i] += acceleration * deltaTime;
        }); 

        Parallel.For(0, _particles, i =>
        {
            Vector2 viscosity = CalculateViscosityForce(i);
            _velocities[i] += viscosity * deltaTime;
        });

        Parallel.For(0, _particles, i =>
        {
            _positions[i] += _velocities[i] * deltaTime;
            ResolveCollisions(ref _positions[i], ref _velocities[i], deltaTime);
        });
    }

    private void ResolveCollisions(ref Vector2 position, ref Vector2 velocity, float deltaTime)
    {
        Vector2 center = Position;
        Vector2 mouse = Main.MouseWorld / _scale;

        Vector2 toCenter = center - position;
        float distance = toCenter.Length();

        if (distance > 0)
        {
            Vector3 sdg = SmoothDistanceGradientSegment(position, center, mouse, 0f);

            Vector2 normal = new(sdg.Y, sdg.Z);
            velocity -= normal * deltaTime * _gravity;
        }

        velocity *= 0.995f;

        return;
    }

    private (float Density, float NearDensity) CalculateDensity(int index)
    {
        Vector2 position = _predictedPositions[index];

        float density = 0;
        float nearDensity = 0;

        for (int i = 0; i < s_neighbours[index].Count; i++)
        {
            float distance = MathF.Sqrt(s_neighbours[index][i].SquareDistance);
            density += SpikyKernelPow2(_smoothingRadius, distance);
            nearDensity += SpikyKernelPow3(_smoothingRadius, distance);
        }

        return (density, nearDensity);
    }

    private Vector2 CalculatePressureGradient(int index)
    {
        Vector2 gradient = Vector2.Zero;

        float density = _densities[index];
        float densityNear = _nearDensities[index];
        float pressure = ConvertDensityToPressure(density);
        float nearPressure = ConvertNearDensityToNearPressure(densityNear);

        for (int i = 0; i < s_neighbours[index].Count; i++)
        {
            Neighbour neighbour = s_neighbours[index][i];

            if (neighbour.Index == index) continue;

            float distance = MathF.Sqrt(neighbour.SquareDistance);

            Vector2 direction = distance == 0 ? Vector2.Zero : neighbour.Offset / distance;

            float neighbourDensity = _densities[neighbour.Index];
            float neighbourNearDensity = _nearDensities[neighbour.Index];
            float neighbourPressure = ConvertDensityToPressure(neighbourDensity);
            float neighbourNearPressure = ConvertNearDensityToNearPressure(neighbourNearDensity);

            float sharedPressure = (pressure + neighbourPressure) * 0.5f;
            float sharedNearPressure = (nearPressure + neighbourNearPressure) * 0.5f;

            float slope = DerivativeSpikyPow2(_smoothingRadius, distance);
            float nearSlope = DerivativeSpikyPow3(_smoothingRadius, distance);

            gradient += direction * sharedPressure * slope / neighbourDensity;
            gradient += direction * sharedNearPressure * slope / neighbourNearDensity;
        }

        return gradient;
    }


    private float ConvertNearDensityToNearPressure(float nearDensity)
    {
        float nearPressure = nearDensity * _nearPressureMultiplier;
        return nearPressure;
    }

    private float ConvertDensityToPressure(float density)
    {
        float densityError = density - _targetDensity;
        float pressure = densityError * _pressureMultiplier;
        return pressure;
    }

    private Vector2 CalculateViscosityForce(int index)
    {
        Vector2 force = Vector2.Zero;
        Vector2 position = _predictedPositions[index];

        for (int i = 0; i < s_neighbours[index].Count; i++)
        {
            Neighbour neighbour = s_neighbours[index][i];
            float influence = SmoothingKernelPoly6(_smoothingRadius, MathF.Sqrt(neighbour.SquareDistance));
            force += (_velocities[neighbour.Index] - _velocities[index]) * influence;
        }

        return force * _viscosity;
    }
}
