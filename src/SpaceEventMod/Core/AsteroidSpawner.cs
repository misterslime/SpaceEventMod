using Microsoft.Xna.Framework;
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

        switch (asteroidType)
        {
            case 0:
                NewAsteroid(randomPosition, 48, 16, "SpaceEventMod/Assets/Textures/Props/Asteroid3Small");
                break;

            case 1:
                NewAsteroid(randomPosition, 48, 32, "SpaceEventMod/Assets/Textures/Props/Asteroid3Medium");
                break;

            case 2:
                NewAsteroid(randomPosition, 48, 48, "SpaceEventMod/Assets/Textures/Props/Asteroid3Large");
                break;

            case 3:
                NewAsteroid(randomPosition, 64, 24, "SpaceEventMod/Assets/Textures/Props/Asteroid4Small");
                break;

            case 4:
                NewAsteroid(randomPosition, 64, 32, "SpaceEventMod/Assets/Textures/Props/Asteroid4Medium");
                break;

            case 5:
                NewAsteroid(randomPosition, 64, 48, "SpaceEventMod/Assets/Textures/Props/Asteroid4Large");
                break;
        }
    }

    public void NewAsteroid(Vector2 spawnPosition, int width, int height, string spritePath)
    {
        var secondOrderSolver = new Vector2Dynamics(1f / 128f, 0.5f, 0.2f, spawnPosition);
        AsteroidSystem.Asteroids.Add(new Asteroid(secondOrderSolver, spawnPosition, spritePath, width, height));
    }
}
