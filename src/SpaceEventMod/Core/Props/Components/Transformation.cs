using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// Gives the prop position, velocity, and rotation.
/// </summary>
/// <param name="position">Position of the prop.</param>
/// <param name="velocity">Velocity of the prop.</param>
/// <param name="rotation">Rotation of the prop. Defaults to 0f.</param>
public class Transformation(Vector2 position, Vector2 velocity, float rotation = 0f) : Component
{
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
    public float Rotation = rotation;
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
