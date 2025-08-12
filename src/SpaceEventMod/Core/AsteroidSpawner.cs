using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Events.FirmamentTide.Asteroids;
using SpaceEventMod.Content.Events.FirmamentTide.FirmamentSea;
using SpaceEventMod.Content.Events.FirmamentTide.Stars;
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
        if (!FirmamentSeaSystem.Sea.CanSpawnThings)
            return;

        var playerCenter = Main.player[Main.myPlayer].Center;
        var playerPositionSeaCoords = new Vector2(playerCenter.X, playerCenter.Y - (float)(Main.worldSurface * 0.35 * 16));
        var randomPosition = playerPositionSeaCoords + Main.rand.NextVector2Circular(30 * 16, 30 * 16);
        // 0.05f

        // only spawn 20 tiles above the sea surface
        if (randomPosition.Y > -320)
            return;

        var noiseSample = (float)(1 + noise.GetNoise(randomPosition.X * 0.3f, randomPosition.Y * 0.3f, 0));

        var density = MathHelper.Lerp(0.7f, 30f, EasingFunctions.CircEaseIn(noiseSample));

        var asteroids = AsteroidSystem.Asteroids;

        if (asteroids.Count > 0)
        {
            foreach (var asteroid in asteroids)
            {
                if ((asteroid.RestPosition - randomPosition).LengthSquared() <= Math.Pow(separationDistance * density, 2))
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

        var dimensions = GetDimensions(asteroidType);

        AsteroidSystem.Asteroids.Add(new Asteroid(randomPosition, playerPositionSeaCoords.Y - 60 * 16, asteroidType, dimensions.X, dimensions.Y));
    }
}
