using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props.Systems;

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
