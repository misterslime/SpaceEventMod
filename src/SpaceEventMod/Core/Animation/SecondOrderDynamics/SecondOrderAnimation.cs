using System;

namespace SpaceEventMod.Core.Animation.SecondOrderDynamics;

internal struct SecondOrderAnimation(float frequency, float dampening, float anticipation)
{
    public float GetK1 { get; } = dampening / (MathF.PI * frequency);
    public float GetK2 { get; } = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
    public float GetK3 { get; } = anticipation * dampening / (2 * MathF.PI * frequency);
}
