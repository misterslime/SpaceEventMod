using System;

namespace SpaceEventMod.Core.Animation.SecondOrderDynamics;

internal struct SecondOrderAnimation(float frequency, float dampening, float anticipation)
{
    private float k1 = dampening / (MathF.PI * frequency);
    private float k2 = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
    private float k3 = anticipation * dampening / (2 * MathF.PI * frequency);

    public float GetK1 { get => k1; }
    public float GetK2 { get => k2; }
    public float GetK3 { get => k3; }
}
