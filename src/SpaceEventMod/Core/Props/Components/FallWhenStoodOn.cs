using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Props.Components;

public class FallWhenStoodOn : Component
{
    public Vector2 RestingPosition;
    public Vector2 FallPosition;
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

