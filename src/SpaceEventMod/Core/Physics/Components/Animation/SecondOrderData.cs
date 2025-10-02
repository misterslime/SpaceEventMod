using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Animation.SecondOrderDynamics;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components.Animation;

internal struct SecondOrderData : IComponent
{
    public SecondOrderAnimation SecondOrderDynamics { get; }
    public float DeltaTime { get; }
    public Vector2 InputPosition { get; }
    public Vector2 PreviousInput { get; }

    public SecondOrderData(float deltaTime, SecondOrderAnimation secondOrderDynamics, Vector2 inputPosition)
    {
        DeltaTime = deltaTime;
        SecondOrderDynamics = secondOrderDynamics;
        InputPosition = inputPosition;
        PreviousInput = inputPosition;
    }

    public SecondOrderData(float deltaTime, SecondOrderAnimation secondOrderDynamics, Vector2 inputPosition, Vector2 previousInput)
    {
        DeltaTime = deltaTime;
        SecondOrderDynamics = secondOrderDynamics;
        InputPosition = inputPosition;
        PreviousInput = previousInput;
    }
}
