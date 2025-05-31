using Microsoft.Xna.Framework;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class Collider : Component
{
    public bool StoodOn;
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
        Vector4 result = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        if (!fall)
            result = CheckCollision(position, velocity, width, height, gravity);

        return orig(result.XY(), result.ZW(), width, height, gravity, fall);
    }

    public Vector4 CheckCollision(Vector2 position, Vector2 velocity, int width, int height, float gravity)
    {
        Vector4 originalVector = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        // make the entity's hitbox only be its bottom half
        Rectangle entityHitbox = new Rectangle((int)position.X, (int)position.Y, width, height + 2);

        foreach (Collider collider in components)
        {
            Transformation transformation = collider.GetComponent<Transformation>();

            Rectangle colliderBox = collider.GetComponent<Hitbox>().GetBoundingBox();

            Vector2 propCenter = collider.GetComponent<Hitbox>().GetCenter();
            bool canHit = Collision.CanHit(position, 1, 1, propCenter, 1, 1);

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
