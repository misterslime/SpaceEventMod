using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Physics;
using Terraria;

namespace SpaceEventMod.Common.Interfaces;

/// <summary>
/// Indicates that this ModProjectile is a ball with verlet physics.
/// </summary>
internal interface IBall
{
    float Radius { get; }

    [ModSystemHooks.PreUpdateProjectiles]
    public static void CollideProjectiles()
    {
        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (projectile.ModProjectile is not IBall ball)
                continue;

            foreach (var otherProjectile in Main.ActiveProjectiles)
            {
                if (otherProjectile.ModProjectile is not IBall otherBall ||
                    otherProjectile.whoAmI == projectile.whoAmI)
                    continue;

                Vector2 collisionAxis = projectile.Center - otherProjectile.Center;
                float distance = collisionAxis.Length();
                float minDistance = ball.Radius + otherBall.Radius * 2;

                if (distance < minDistance)
                {
                    Vector2 normal = collisionAxis / distance;
                    float delta = minDistance - distance;
                    Vector2 displacement = delta * normal / 2f;

                    projectile.Center += VerletString.TileCollision(projectile.Center, displacement, true);
                    otherProjectile.Center += VerletString.TileCollision(otherProjectile.Center, -displacement, true);
                }
            }
        }
    }
}
