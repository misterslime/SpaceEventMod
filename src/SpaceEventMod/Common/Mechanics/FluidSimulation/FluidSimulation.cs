using ComputeSharp;
using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Mechanics.FluidSimulation.Compute;
using SpaceEventMod.Content.Items;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

using Vector2 = System.Numerics.Vector2;

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

        /*Vector2 InteractionForce(Vector2 input, float radius, float strength, int index)
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

            s_velocities[i] += InteractionForce((mouseScreen - middle) / 40, 12, strength, i) * deltaTime;
        });*/

        SimulationStep(deltaTime);
    }
    public static void Activate()
    {
        Active = !Active;

        if (!Active)
            return;

        const int numParticles = 40000;
        const float particleSize = 0.05f;
        const float particleSpacing = 0.05f;

        s_halfBoundsSize = new Vector2(20, 12);
        s_gravity = 10;

        s_numParticles = numParticles;
        float[] densities = new float[numParticles];
        float[] nearDensities = new float[numParticles];
        Vector2[] positions = new Vector2[numParticles];
        Vector2[] predictedPositions = new Vector2[numParticles];
        Vector2[] velocities = new Vector2[numParticles];
        uint3[] spatialLookup = new uint3[numParticles];
        int[] startIndices = new int[numParticles];

        int particlesPerRow = (int)Math.Sqrt(numParticles);
        int particlesPerCol = (numParticles - 1) / particlesPerRow + 1;
        float spacing = particleSize * 2f + particleSpacing;

        for (int i = 0; i < numParticles; i++)
        {
            float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            float y = (i / particlesPerCol - particlesPerCol / 2f + 0.5f) * spacing;
            positions[i] = new Vector2(x, y);
            velocities[i] = Vector2.Zero;
        }

        velocityBuffer?.Dispose();
        velocityBuffer = null;

        positionsBuffer?.Dispose();
        positionsBuffer = null;

        predictedPositionsBuffer?.Dispose();
        predictedPositionsBuffer = null;

        spatialLookupBuffer?.Dispose();
        spatialLookupBuffer = null;

        startIndicesBuffer?.Dispose();
        startIndicesBuffer = null;

        densitiesBuffer?.Dispose();
        densitiesBuffer = null;

        nearDensitiesBuffer?.Dispose();
        nearDensitiesBuffer = null;


        velocityBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(Array.ConvertAll(velocities, item => (float2)item));
        positionsBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(Array.ConvertAll(positions, item => (float2)item));
        predictedPositionsBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(Array.ConvertAll(predictedPositions, item => (float2)item));

        spatialLookupBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(spatialLookup);
        startIndicesBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(startIndices);

        densitiesBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(densities);
        nearDensitiesBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer(nearDensities);
    }
}
