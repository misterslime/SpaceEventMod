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

    /// <summary>
    /// Integrates the current position and velocity to target <paramref name="inputPosition"/>.
    /// </summary>
    /// <param name="deltaTime">Change in time.</param>
    /// <param name="inputPosition">Target position.</param>
    /// <param name="setVelocity">If you want to set the velocity to something set this to true.</param>
    /// <param name="velocity">Velocity to set current velocity to.</param>
    /// <returns>The output position after integration.</returns>
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

    /// <summary>
    /// Changes how the system will respond to inputs
    /// </summary>
    /// <param name="frequency">The frequency value to change to.</param>
    /// <param name="dampening">The dampening value to change to.</param>
    /// <param name="anticipation">The anticipation value to change to.</param>
    /// <param name="setVelocity">If you want to set the velocity to something set this to true.</param>
    /// <param name="velocity">Velocity to set current velocity to.</param>
    public virtual void ChangeAnimation(float frequency, float dampening, float anticipation, bool setVelocity = false, T velocity = default)
    {
        previousInputPosition = outputPosition;

        k1 = dampening / (MathF.PI * frequency);
        k2 = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
        k3 = anticipation * dampening / (2 * MathF.PI * frequency);

        if (setVelocity)
            Velocity = velocity;
    }

    public T GetVelocity()
    {
        return Velocity;
    }
}

public class FloatDynamics(float frequency, float dampening, float anticipation, float initialInput) : SecondOrderDynamics<float>(frequency, dampening, anticipation, initialInput)
{
}

public class Vector2Dynamics(float frequency, float dampening, float anticipation, Vector2 initialInput) : SecondOrderDynamics<Vector2>(frequency, dampening, anticipation, initialInput)
{
}