using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class Collider : Component
{
    public bool StoodOn = false;

    public Collider()
    {
        StoodOn = false;
        CollisionSystem.Register(this);
    }
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

        Prop closest = FindClosestCollideableProp(position);

        if (closest is not null)
        {
            Transformation transformation = closest.GetComponent<Transformation>();

            Rectangle colliderBox = closest.GetComponent<Hitbox>().GetBoundingBox();

            if (!(position.X + width > colliderBox.Left && position.X < colliderBox.Right))
                return originalVector;

            if (!(velocity.Y >= 0 && entityHitbox.Intersects(colliderBox)))
                return originalVector;

            if (position.Y + height * 0.5f <= colliderBox.Y && velocity.Y >= 0)
            {
                closest.GetComponent<Collider>().StoodOn = true;

                position.Y = MathHelper.Lerp(position.Y, colliderBox.Y - height + 2, 0.66f);
                position += transformation.Velocity;
                velocity.Y = 0;
            }

            Collision.up = true;
            Collision.stair = true;

            return new Vector4(position.X, position.Y, velocity.X, velocity.Y);
        }

        return originalVector;
    }

    public Prop FindClosestCollideableProp(Vector2 position)
    {
        Prop closest = null;
        var distanceToClosest = float.MaxValue;

        foreach (Collider collider in components.ToList())
        {
            Rectangle propBoundingBox = collider.prop.GetComponent<Hitbox>().GetBoundingBox();

            Vector2 propCenter = collider.prop.GetComponent<Hitbox>().GetCenter();
            var canHit = Collision.CanHit(position, 1, 1, propCenter, 1, 1);
            if (Vector2.DistanceSquared(position, propCenter) < distanceToClosest)
            {
                distanceToClosest = Vector2.DistanceSquared(position, propCenter);
                closest = collider.prop;
            }
        }

        return closest;
    }
}
