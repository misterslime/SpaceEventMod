using System;

namespace SpaceEventMod.Core.Physics.Animation;

internal struct SecondOrderParameters(float deltaTime, SecondOrderData secondOrderDynamics, dynamic inputPosition, bool setVelocity = false, dynamic velocity = default)
{
    public SecondOrderData SecondOrderDynamics { get; set; } = secondOrderDynamics;
    public float DeltaTime { get; set; } = deltaTime;
    public dynamic InputPosition { get; set; } = inputPosition;
    public bool SetVelocity { get; set; } = setVelocity;
    public dynamic Velocity { get; set; } = velocity;
}

// based on https://www.youtube.com/watch?v=KPoeNZZ6H4s this video by t3ssel8r
internal struct SecondOrderData(float frequency, float dampening, float anticipation)
{
    private float k1 = dampening / (MathF.PI * frequency);
    private float k2 = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
    private float k3 = anticipation * dampening / (2 * MathF.PI * frequency);

    public float GetK1 { get => k1; }
    public float GetK2 { get => k2; }
    public float GetK3 { get => k3; }
}
