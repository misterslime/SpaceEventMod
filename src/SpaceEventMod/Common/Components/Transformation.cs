using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props;

namespace SpaceEventMod.Common.Components;

/// <summary>
/// Gives the prop position, velocity, and rotation.
/// </summary>
/// <param name="position">Position of the prop.</param>
/// <param name="velocity">Velocity of the prop.</param>
public class Transformation(Vector2 position, Vector2 velocity) : Component
{
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
}

public class TransformationSystem : ComponentSystem<Transformation>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            component.Position += component.Velocity;
        }
    }
}
