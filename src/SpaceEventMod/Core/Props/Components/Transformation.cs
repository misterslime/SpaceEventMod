using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props.Systems;

namespace SpaceEventMod.Core.Props.Components;

public class Transformation : Component
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;

    public Transformation()
    {
        TransformationSystem.Register(this);
    }

    public override void Dispose()
    {
        TransformationSystem.Unregister(this);
    }

    public void Update()
    {
        this.Position += this.Velocity;
    }
}
