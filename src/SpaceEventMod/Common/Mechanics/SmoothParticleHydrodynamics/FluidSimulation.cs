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

internal partial class FluidSimulation : ModSystem
{
    private const float SCALE = 30f;

    private static Vector2 s_halfBoundsSize;

    public static bool Active { get; set; }
    public override void PostUpdateNPCs()
    {
        if (!Active)
            return;

        float deltaTime = 1 / 120f;

        SimulationStep(deltaTime);
    }
    public static void Activate(Vector2 mouseWorld)
    {
        Active = !Active;

        if (!Active)
            return;

        Vector2 circleCenter = Main.LocalPlayer.Center / SCALE;
        circleCenter.Y = 0;

        const int numParticles = 1200;
        const float particleSize = 0.07f;
        const float particleSpacing = 0.07f;

        s_halfBoundsSize = new Vector2(32, 9);
        s_gravity = 0;

        s_numParticles = numParticles;
        s_densities = new float[numParticles];
        s_nearDensities = new float[numParticles];
        s_positions = new Vector2[numParticles];
        s_predictedPositions = new Vector2[numParticles];
        s_velocities = new Vector2[numParticles];
        s_spatialLookup = new Entry[numParticles];
        s_startIndices = new int[numParticles];
        s_neighbours = new List<Neighbour>[s_numParticles];

        int particlesPerRow = (int)Math.Sqrt(numParticles);
        int particlesPerCol = (numParticles - 1) / particlesPerRow + 1;
        float spacing = particleSize * 2f + particleSpacing;

        for (int i = 0; i < numParticles; i++)
        {
            float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            float y = (i / particlesPerCol - particlesPerCol / 2f + 0.5f) * spacing;
            s_positions[i] = new Vector2(x, y) + circleCenter;
            s_velocities[i] = Vector2.Zero;
        }
    }
}
