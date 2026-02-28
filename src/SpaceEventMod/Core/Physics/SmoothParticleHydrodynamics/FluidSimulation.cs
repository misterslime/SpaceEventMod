using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SpaceEventMod.Core.Physics.SmoothParticleHydrodynamics;

internal partial class FluidSimulation(
    float scale,
    float smoothingRadius,
    float targetDensity,
    float pressureMultiplier,
    float nearPressureMultiplier,
    float viscosity,
    float gravity)
{
    public delegate void FluidDrawAction(in SpriteBatch spriteBatch, in Vector2 position, in Vector2 velocity, in float density);

    private float _smoothingRadius = smoothingRadius;
    private float _targetDensity = targetDensity;
    private float _pressureMultiplier = pressureMultiplier;
    private float _nearPressureMultiplier = nearPressureMultiplier;
    private float _viscosity = viscosity;
    private float _gravity = gravity;

    private int _particles;
    private Vector2[] _predictedPositions;
    private Vector2[] _velocities;
    private float[] _densities;
    private float[] _nearDensities;

    public Vector2 Position { get; set; }

    public Vector2[] Positions { get; private set; }

    public float Scale { get; } = scale;

    public void Update()
    {
        var deltaTime = 1 / 60f;

        SimulationStep(deltaTime);
    }
    public void Fill(Vector2 position, int particles, float particleSize, float particleSpacing)
    {
        Position = position / Scale;

        _particles = particles;
        _densities = new float[particles];
        _nearDensities = new float[particles];
        Positions = new Vector2[particles];
        _predictedPositions = new Vector2[particles];
        _velocities = new Vector2[particles];
        s_spatialLookup = new Entry[particles];
        s_startIndices = new int[particles];
        s_neighbours = new List<Neighbour>[particles];

        var particlesPerRow = (int)Math.Sqrt(particles);
        var particlesPerCol = (particles - 1) / particlesPerRow + 1;
        var spacing = particleSize * 2f + particleSpacing;

        for (var i = 0; i < particles; i++)
        {
            var x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            var y = (i / particlesPerCol - particlesPerCol / 2f + 0.5f) * spacing;
            Positions[i] = new Vector2(x, y) + Position;
            _velocities[i] = Vector2.Zero;
        }
    }
}
