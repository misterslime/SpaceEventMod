using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Events.FirmamentTide.FirmamentSea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.FirmamentTide.Asteroids;

public class AsteroidCollision : ILoadable
{
    public void Load(Mod mod) => On_Collision.SlopeCollision += CheckSlopeCollision;

    public void Unload() => On_Collision.SlopeCollision -= CheckSlopeCollision;

    private Vector4 CheckSlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 position, Vector2 velocity, int width, int height, float gravity, bool fall)
    {
        var result = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        if (!fall && !FirmamentSeaSystem.Sea.Despawning)
            result = CheckCollision(position, velocity, width, height, gravity);

        return orig(result.XY(), result.ZW(), width, height, gravity, fall);
    }

    private Vector4 CheckCollision(Vector2 position, Vector2 velocity, int width, int height, float gravity)
    {
        var originalVector = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        // make the entity's hitbox only be its bottom half
        var entityHitbox = new Rectangle((int)position.X, (int)position.Y, width, height + 2);

        for (var i = 0; i < Asteroids.List.Count; i++)
        {
            var asteroid = Asteroids.List[i];

            var colliderBox = asteroid.GetBoundingBox();

            var propCenter = asteroid.GetCenter();
            var canHit = Collision.CanHit(position, 1, 1, propCenter, 1, 1);

            if (!entityHitbox.Intersects(colliderBox) || velocity.Y < 0 || !(position.X + width > colliderBox.Left && position.X < colliderBox.Right) || !canHit)
                continue;

            if (position.Y + height * 0.5f <= colliderBox.Y)
            {
                if (velocity.Y > 0)
                    asteroid.BeingStoodOn = true;

                position.Y = MathHelper.Lerp(position.Y, colliderBox.Y - height + 2, 0.66f);
                velocity.Y = 0;
            }

            Collision.up = true;
            Collision.stair = true;

            Asteroids.List[i] = asteroid;
        }

        return new Vector4(position.X, position.Y, velocity.X, velocity.Y);
    }
}
