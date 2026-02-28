using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry;
using System;
using System.Collections.Generic;
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
            var densitites = CalculateDensity(i);

            _densities[i] = densitites.Density;
            _nearDensities[i] = densitites.NearDensity;
        });

        Parallel.For(0, _particles, i =>
        {
            var pressure = CalculatePressureGradient(i);
            var acceleration = pressure / _densities[i];
            _velocities[i] += acceleration * deltaTime;
        });

        Parallel.For(0, _particles, i =>
        {
            var viscosity = CalculateViscosityForce(i);
            _velocities[i] += viscosity * deltaTime;
        });

        Parallel.For(0, _particles, i =>
        {
            _positions[i] += _velocities[i] * deltaTime;
        });
    }

    public void AttractToSkeleton(List<Line> lines, float deltaTime, float smoothness)
    {
        Parallel.For(0, _particles, i =>
        {
            _positions[i] += _velocities[i] * deltaTime;

            var total = Vector2.Zero;

            for (var j = 0; j < lines.Count; j++)
            {
                var pointA = lines[j].Point1 / _scale;
                var pointB = lines[j].Point2 / _scale;

                var dist = SignedDistanceGradientSegment(_positions[i], pointA, pointB, 0f);

                total += new Vector2(dist.Y, dist.Z) / (dist.X + 0.1f);
            }

            total = total.SafeNormalize(Vector2.Zero);

            //Vector2 normal = new(total.Y, total.Z);
            _velocities[i] -= total * deltaTime * _gravity;
            _velocities[i] *= 0.995f;
        });

        return;
    }

    private (float Density, float NearDensity) CalculateDensity(int index)
    {
        var position = _predictedPositions[index];

        float density = 0;
        float nearDensity = 0;

        for (var i = 0; i < s_neighbours[index].Count; i++)
        {
            var distance = MathF.Sqrt(s_neighbours[index][i].SquareDistance);
            density += SpikyKernelPow2(_smoothingRadius, distance);
            nearDensity += SpikyKernelPow3(_smoothingRadius, distance);
        }

        return (density, nearDensity);
    }

    private Vector2 CalculatePressureGradient(int index)
    {
        var gradient = Vector2.Zero;

        var density = _densities[index];
        var densityNear = _nearDensities[index];
        var pressure = ConvertDensityToPressure(density);
        var nearPressure = ConvertNearDensityToNearPressure(densityNear);

        for (var i = 0; i < s_neighbours[index].Count; i++)
        {
            var neighbour = s_neighbours[index][i];

            if (neighbour.Index == index) continue;

            var distance = MathF.Sqrt(neighbour.SquareDistance);

            var direction = distance == 0 ? Vector2.Zero : neighbour.Offset / distance;

            var neighbourDensity = _densities[neighbour.Index];
            var neighbourNearDensity = _nearDensities[neighbour.Index];
            var neighbourPressure = ConvertDensityToPressure(neighbourDensity);
            var neighbourNearPressure = ConvertNearDensityToNearPressure(neighbourNearDensity);

            var sharedPressure = (pressure + neighbourPressure) * 0.5f;
            var sharedNearPressure = (nearPressure + neighbourNearPressure) * 0.5f;

            var slope = DerivativeSpikyPow2(_smoothingRadius, distance);
            var nearSlope = DerivativeSpikyPow3(_smoothingRadius, distance);

            gradient += direction * sharedPressure * slope / neighbourDensity;
            gradient += direction * sharedNearPressure * slope / neighbourNearDensity;
        }

        return gradient;
    }


    private float ConvertNearDensityToNearPressure(float nearDensity)
    {
        var nearPressure = nearDensity * _nearPressureMultiplier;
        return nearPressure;
    }

    private float ConvertDensityToPressure(float density)
    {
        var densityError = density - _targetDensity;
        var pressure = densityError * _pressureMultiplier;
        return pressure;
    }

    private Vector2 CalculateViscosityForce(int index)
    {
        var force = Vector2.Zero;
        var position = _predictedPositions[index];

        for (var i = 0; i < s_neighbours[index].Count; i++)
        {
            var neighbour = s_neighbours[index][i];
            var influence = SmoothingKernelPoly6(_smoothingRadius, MathF.Sqrt(neighbour.SquareDistance));
            force += (_velocities[neighbour.Index] - _velocities[index]) * influence;
        }

        return force * _viscosity;
    }
}
