using Microsoft.Xna.Framework;
using System.Linq;

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
}

public class TransformationSystem : ComponentSystem<Transformation>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components.ToList())
        {
            component.Position += component.Velocity;
        }
    }
}
