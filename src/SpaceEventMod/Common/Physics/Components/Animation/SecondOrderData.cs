using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Animation;
using SpaceEventMod.Common.Physics.Interfaces;

namespace SpaceEventMod.Common.Physics.Components.Animation;

internal struct SecondOrderData : IComponent
{
    public AnimationParameters SecondOrderDynamics { get; }
    public float DeltaTime { get; }
    public Vector2 InputPosition { get; }
    public Vector2 PreviousInput { get; }

    public SecondOrderData(float deltaTime, AnimationParameters secondOrderDynamics, Vector2 inputPosition)
    {
        DeltaTime = deltaTime;
        SecondOrderDynamics = secondOrderDynamics;
        InputPosition = inputPosition;
        PreviousInput = inputPosition;
    }

    public SecondOrderData(float deltaTime, AnimationParameters secondOrderDynamics, Vector2 inputPosition, Vector2 previousInput)
    {
        DeltaTime = deltaTime;
        SecondOrderDynamics = secondOrderDynamics;
        InputPosition = inputPosition;
        PreviousInput = previousInput;
    }
}
