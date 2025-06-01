using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace SpaceEventMod.Core.Props.Components;

public class AsteroidNoiseSpawner(FastNoiseLite noise, float minimumToSpawnAsteroid, float separationDistance) : Component
{
    public FastNoiseLite noise = noise;
    public float minimumToSpawnAsteroid = minimumToSpawnAsteroid;
    public float separationDistance = separationDistance;
}

public class AsteroidNoiseSpawnerSystem : ComponentSystem<AsteroidNoiseSpawner>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            Vector2 playerPosition = Main.player[Main.myPlayer].Center;
            Vector2 randomPosition = playerPosition + Main.rand.NextVector2Circular(30 * 16, 30 * 16);
            // 0.05f

            float noiseSample = (float)(1 + component.noise.GetNoise(randomPosition.X * 0.3f, randomPosition.Y * 0.3f, 0));

            List<Mineable> asteroids = ComponentManager.GetComponents<Mineable>();

            float density = MathHelper.Lerp(0.7f, 30f, EasingFunctions.CircEaseIn(noiseSample));

            if (asteroids.Count > 0)
            {
                foreach (var asteroid in asteroids)
                {
                    if ((asteroid.GetComponent<Hitbox>().GetCenter() - randomPosition).LengthSquared() <= Math.Pow(component.separationDistance * density, 2))
                        return;
                }
            }

            int asteroidType = Main.rand.Next(6);

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
    }

    public void NewAsteroid(Vector2 spawnPosition, int width, int height, string spritePath)
    {
        /// actually create the prop in the world
        new Prop().AddComponent(new Transformation(spawnPosition, Vector2.Zero))
            .AddComponent(new Hitbox(width, height))
            .AddComponent(new Health(200, SoundID.Item70))
            .AddComponent(new Collider(false))
            .AddComponent(new Mineable())
            .AddComponent(new Grappleable())
            .AddComponent(new DynamicMovement(1f / 128f, 0.5f, 0.2f, spawnPosition))
            .AddComponent(new FallWhenStoodOn(spawnPosition, spawnPosition + Vector2.UnitY * 48f))
            .AddComponent(new DirectionalShake(2, Vector2.UnitX, 0, 20))
            .AddComponent(new LowHealthFlashing(Color.Red))
            .AddComponent(new Sprite(spritePath, 1f, 0f, Vector2.Zero, Color.White, Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally))
            .AddComponent(new DespawnWithDistance(60f * 16f))
            .Register();
    }
}

