using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props;
using Terraria;

namespace SpaceEventMod.Common.Components;

/// <summary>
/// Makes this prop something that can be collided with like a platform.<br/>
/// Requires the <see cref="Transformation"/> and <see cref="Hitbox"/> components to function.
/// </summary>
/// <param name="stoodOn">Whether the collider is being stood on.</param>
public class Collider(bool stoodOn) : Component
{
    public bool StoodOn = stoodOn;
}

public class CollisionSystem : ComponentSystem<Collider>
{
    public override void Load()
    {
        On_Collision.SlopeCollision += CheckSlopeCollision;
    }

    public override void Unload()
    {
        On_Collision.SlopeCollision -= CheckSlopeCollision;
    }

    private Vector4 CheckSlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 position, Vector2 velocity, int width, int height, float gravity, bool fall)
    {
        var result = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        if (!fall)
            result = CheckCollision(position, velocity, width, height, gravity);

        return orig(result.XY(), result.ZW(), width, height, gravity, fall);
    }

    public Vector4 CheckCollision(Vector2 position, Vector2 velocity, int width, int height, float gravity)
    {
        var originalVector = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        // make the entity's hitbox only be its bottom half
        var entityHitbox = new Rectangle((int)position.X, (int)position.Y, width, height + 2);

        foreach (var collider in components)
        {
            var transformation = collider.GetComponent<Transformation>();

            var colliderBox = collider.GetComponent<Hitbox>().GetBoundingBox();

            var propCenter = collider.GetComponent<Hitbox>().GetCenter();
            var canHit = Collision.CanHit(position, 1, 1, propCenter, 1, 1);

            if (!entityHitbox.Intersects(colliderBox) || velocity.Y < 0 || !(position.X + width > colliderBox.Left && position.X < colliderBox.Right) || !canHit)
                continue;

            if (position.Y + height * 0.5f <= colliderBox.Y)
            {
                if (velocity.Y > 0)
                    collider.StoodOn = true;

                position.Y = MathHelper.Lerp(position.Y, colliderBox.Y - height + 2, 0.66f);
                position += transformation.Velocity;
                velocity.Y = 0;
            }

            Collision.up = true;
            Collision.stair = true;
        }

        return new Vector4(position.X, position.Y, velocity.X, velocity.Y);
    }
}
