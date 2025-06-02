using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Props;

namespace SpaceEventMod.Common.Components;

/// <summary>
/// Makes this prop ease its position through a second order solver.<br/>
/// Requires the <see cref="Transformation"/> component to function.
/// </summary>
/// <param name="frequency">How fast the easing will play out.</param>
/// <param name="dampening">How quickly the prop will come to a rest.</param>
/// <param name="anticipation">How the prop will anticipate its movement.</param>
/// <param name="initialInput">Initial position of the prop.</param>
public class DynamicMovement(float frequency, float dampening, float anticipation, Vector2 initialInput) : Component
{
    public Vector2Dynamics secondOrderSolver = new Vector2Dynamics(frequency, dampening, anticipation, initialInput);
    public Vector2 TargetPosition = initialInput;
}

public class DynamicMovementSystem : ComponentSystem<DynamicMovement>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            component.GetComponent<Transformation>().Position = component.secondOrderSolver.Update(1, component.TargetPosition);
        }
    }
}
