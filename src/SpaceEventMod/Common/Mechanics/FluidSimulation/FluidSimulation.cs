using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Items;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Mechanics.FluidSimulation;

internal partial class FluidSimulation : ModSystem
{
    private static Vector2 s_halfBoundsSize;

    public static bool Active { get; set; }
    public override void PostUpdateNPCs()
    {
        if (!Active)
            return;

        float deltaTime = 1 / 60f;

        SimulationStep(deltaTime);
    }
    public static void Activate(Vector2 mouseWorld)
    {
        Active = !Active;

        if (!Active)
            return;

        const int numParticles = 3000;
        const float particleSize = 0.1f;
        const float particleSpacing = 0.1f;

        s_halfBoundsSize = new Vector2(16, 9);
        s_gravity = 10;

        s_numParticles = numParticles;
        s_densities = new float[numParticles];
        s_positions = new Vector2[numParticles];
        s_predictedPositions = new Vector2[numParticles];
        s_velocities = new Vector2[numParticles];
        s_spatialLookup = new Entry[numParticles];
        s_startIndices = new int[numParticles];

        int particlesPerRow = (int)Math.Sqrt(numParticles);
        int particlesPerCol = (numParticles - 1) / particlesPerRow + 1;
        float spacing = particleSize * 2f + particleSpacing;

        for (int i = 0; i < numParticles; i++)
        {
            float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            float y = (i / particlesPerCol - particlesPerCol / 2f + 0.5f) * spacing;
            s_positions[i] = new Vector2(x, y);
            s_velocities[i] = Vector2.Zero;
        }
    }
}
