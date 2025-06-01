using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// Makes the prop fall dynamically if its being stood on.<br/>
/// Requires the <see cref="Collider"/> and <see cref="DynamicMovement"/> components to function.
/// </summary>
/// <param name="restingPosition">Position it'll return to when its not being stood on.</param>
/// <param name="fallPosition">Position it'll fall to when being stood on.</param>
public class FallWhenStoodOn(Vector2 restingPosition, Vector2 fallPosition) : Component
{
    public Vector2 RestingPosition = restingPosition;
    public Vector2 FallPosition = fallPosition;
}

public class FallWhenStoodOnSystem : ComponentSystem<FallWhenStoodOn>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            bool stoodOn = component.GetComponent<Collider>().StoodOn;
            component.GetComponent<DynamicMovement>().TargetPosition = stoodOn ? component.FallPosition : component.RestingPosition;
            component.GetComponent<Collider>().StoodOn = false;
        }
    }
}

