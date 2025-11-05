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

        Vector2 InteractionForce(Vector2 input, float radius, float strength, int index)
        {
            Vector2 interactionForce = Vector2.Zero;
            Vector2 offset = input - s_positions[index];
            float squareDistance = Vector2.Dot(offset, offset);

            if (squareDistance < radius * radius)
            {
                float distance = MathF.Sqrt(squareDistance);
                Vector2 dirToInputPoint = distance <= float.Epsilon ? Vector2.Zero : offset / distance;

                float centreT = 1 - distance / radius;

                interactionForce += (dirToInputPoint * strength - s_velocities[index]) * centreT;
            }

            return interactionForce;
        }

        float strength = Main.mouseRight ? 25 : 0;
        strength += Main.mouseLeft ? -25 : 0;

        Parallel.For(0, s_numParticles, i =>
        {
            Vector2 middle = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            Vector2 mouseScreen = new Vector2(Main.MouseScreen.X, Main.MouseScreen.Y);

            s_velocities[i] += InteractionForce((mouseScreen - middle) / 40, 6f, strength, i) * deltaTime;
        });

        SimulationStep(deltaTime);

        Main.NewText(s_neighbours.Sum((p) => p.Count));
    }
    public static void Activate(Vector2 mouseWorld)
    {
        Active = !Active;

        if (!Active)
            return;

        const int numParticles = 5000;
        const float particleSize = 0.07f;
        const float particleSpacing = 0.07f;

        s_halfBoundsSize = new Vector2(16, 9);
        s_gravity = 10;

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
            s_positions[i] = new Vector2(x, y);
            s_velocities[i] = Vector2.Zero;
        }
    }
}
