using Microsoft.Xna.Framework;
using System;

namespace SpaceEventMod.Core.Physics;

// based on https://www.youtube.com/watch?v=KPoeNZZ6H4s this video by t3ssel8r
public abstract class SecondOrderDynamics<T>(float frequency, float dampening, float anticipation, T initialInput)
{
    private T previousInputPosition = initialInput;
    protected T outputPosition = initialInput;
    private T Velocity = default;

    private float k1 = dampening / (MathF.PI * frequency);
    private float k2 = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
    private float k3 = anticipation * dampening / (2 * MathF.PI * frequency);

    public virtual T Update(float deltaTime, T inputPosition, bool setVelocity = false, T velocity = default)
    {
        dynamic currentInput = inputPosition;
        dynamic inputVelocity = velocity;
        dynamic nextPosition = outputPosition;
        dynamic previousVelocity = Velocity;

        if (setVelocity == false)
        {
            inputVelocity = (currentInput - previousInputPosition) / deltaTime;
            previousInputPosition = inputPosition;
        }

        float k2Constrained = MathF.Max(k2, 1.1f * (deltaTime * deltaTime * 0.25f + deltaTime * k1 * 0.5f));
        nextPosition += deltaTime * previousVelocity; // integrate position with velocity
        previousVelocity += deltaTime * (inputPosition + k3 * inputVelocity - nextPosition - k1 * previousVelocity) / k2Constrained; // integrate velocity with acceleration

        outputPosition = nextPosition;
        Velocity = previousVelocity;

        return outputPosition;
    }
}

public class FloatDynamics(float frequency, float dampening, float anticipation, float initialInput) : SecondOrderDynamics<float>(frequency,  dampening, anticipation, initialInput)
{
}

public class Vector2Dynamics(float frequency, float dampening, float anticipation, Vector2 initialInput) : SecondOrderDynamics<Vector2>(frequency, dampening, anticipation, initialInput)
{
}