using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class Collider : Component
{
    public delegate Vector2? CollisionDelegate(Prop closest, Vector2 position, int width, int height);

    public CollisionDelegate OnTestCollisionVector { get; set; } = null;

    public Collider()
    {
        CollisionSystem.Register(this);
    }

    public override void Dispose()
    {
        CollisionSystem.Unregister(this);
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
        var collision = TestCollisionVector(position, width, height);
        if (collision != null)
        {
            return new Vector4(position.X + collision.Value.X, position.Y + collision.Value.Y, velocity.X, 0);
        }

        return orig(position, velocity, width, height, gravity, fall);
    }

    public static Vector2? TestCollisionVector(Vector2 position, int width, int height)
    {
        Collider closest = null;
        var distanceToClosest = float.MaxValue;

        foreach (Collider collider in components.ToList())
        {
            Vector2 propCenter = collider.prop.GetComponent<Hitbox>().GetCenter();
            var canHit = Collision.CanHit(position, 1, 1, propCenter, 1, 1);
            if (Vector2.DistanceSquared(position, propCenter) < distanceToClosest)
            {
                distanceToClosest = Vector2.DistanceSquared(position, propCenter);
                closest = collider;
            }
        }

        if (closest is not null)
            return closest.OnTestCollisionVector?.Invoke(closest.prop, position, width, height);

        return null;
    }

}
