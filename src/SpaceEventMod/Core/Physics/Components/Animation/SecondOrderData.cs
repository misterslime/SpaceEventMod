using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Animation.SecondOrderDynamics;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components.Animation;

internal struct SecondOrderData(float deltaTime, SecondOrderAnimation secondOrderDynamics, Vector2 inputPosition, bool setVelocity = false, Vector2 velocity = default) : IComponent
{
    public SecondOrderAnimation SecondOrderDynamics { get; set; } = secondOrderDynamics;
    public float DeltaTime { get; set; } = deltaTime;
    public Vector2 InputPosition { get; set; } = inputPosition;
    public bool SetVelocity { get; set; } = setVelocity;
    public Vector2 Velocity { get; set; } = velocity;
}
