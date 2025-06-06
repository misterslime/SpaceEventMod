using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.GameObjects.Asteroids;
using SpaceEventMod.Core.GameObjects.Stars;
using SpaceEventMod.Core.Physics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core;

public class AsteroidSpawner : ModSystem
{
    public FastNoiseLite noise;
    public float minimumToSpawnAsteroid = 0.7f;
    public float separationDistance = 10 * 16;

    public override void Load()
    {
        noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
    }

    public override void Unload()
    {
        noise = null;
    }

    public override void PostUpdateNPCs()
    {
        var playerPosition = Main.player[Main.myPlayer].Center;
        var randomPosition = playerPosition + Main.rand.NextVector2Circular(30 * 16, 30 * 16);
        // 0.05f

        var noiseSample = (float)(1 + noise.GetNoise(randomPosition.X * 0.3f, randomPosition.Y * 0.3f, 0));

        var density = MathHelper.Lerp(0.7f, 30f, EasingFunctions.CircEaseIn(noiseSample));

        var asteroids = AsteroidSystem.Asteroids;

        if (asteroids.Count > 0)
        {
            foreach (var asteroid in asteroids)
            {
                if ((asteroid.GetCenter() - randomPosition).LengthSquared() <= Math.Pow(separationDistance * density, 2))
                    return;
            }
        }

        var stars = StarSystem.Stars;

        if (stars.Count > 0)
        {
            foreach (var star in stars)
            {
                if ((star.GetCenter() - randomPosition).LengthSquared() <= Math.Pow(separationDistance * density, 2))
                    return;
            }
        }

        var asteroidType = Main.rand.Next(6);

        Point GetDimensions(int variant)
        {
            Point[] dimensions = [
                new Point(48, 16),
                new Point(48, 32),
                new Point(48, 48),
                new Point(64, 24),
                new Point(64, 32),
                new Point(64, 48),
            ];

            return dimensions[variant];
        }

        Point dimensions = GetDimensions(asteroidType);

        NewAsteroid(randomPosition, dimensions.X, dimensions.Y, asteroidType);
    }

    public void NewAsteroid(Vector2 spawnPosition, int width, int height, int variant)
    {
        var secondOrderSolver = new Vector2Dynamics(1f / 128f, 0.5f, 0.2f, spawnPosition);
        AsteroidSystem.Asteroids.Add(new Asteroid(secondOrderSolver, spawnPosition, variant, width, height));
    }
}
