using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Items;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Mechanics.SmoothParticleHydrodynamics;

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

    private float _scale = scale;
    private float _smoothingRadius = smoothingRadius;
    private float _targetDensity = targetDensity;
    private float _pressureMultiplier = pressureMultiplier;
    private float _nearPressureMultiplier = nearPressureMultiplier;
    private float _viscosity = viscosity;
    private float _gravity = gravity;

    private int _particles;
    private Vector2[] _positions;
    private Vector2[] _predictedPositions;
    private Vector2[] _velocities;
    private float[] _densities;
    private float[] _nearDensities;

    public Vector2 Position { get; set; }

    public void Update()
    {
        float deltaTime = 1 / 60f;

        SimulationStep(deltaTime);
    }
    public void Fill(Vector2 position, int particles, float particleSize, float particleSpacing)
    {
        Position = position / _scale;

        //const int numParticles = 400;
        //const float particleSize = 0.07f;
        //const float particleSpacing = 0.07f;

        //_gravity = 10;

        _particles = particles;
        _densities = new float[particles];
        _nearDensities = new float[particles];
        _positions = new Vector2[particles];
        _predictedPositions = new Vector2[particles];
        _velocities = new Vector2[particles];
        s_spatialLookup = new Entry[particles];
        s_startIndices = new int[particles];
        s_neighbours = new List<Neighbour>[particles];

        int particlesPerRow = (int)Math.Sqrt(particles);
        int particlesPerCol = (particles - 1) / particlesPerRow + 1;
        float spacing = particleSize * 2f + particleSpacing;

        for (int i = 0; i < particles; i++)
        {
            float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            float y = (i / particlesPerCol - particlesPerCol / 2f + 0.5f) * spacing;
            _positions[i] = new Vector2(x, y) + Position;
            _velocities[i] = Vector2.Zero;
        }
    }

    public void Draw(SpriteBatch spriteBatch, FluidDrawAction drawAction)
    {
        for (int i = 0; i < _positions.Length; i++)
            drawAction(in spriteBatch, _positions[i] * _scale, in _velocities[i], in _densities[i]);
    }
}
