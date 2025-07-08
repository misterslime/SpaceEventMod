using Microsoft.Xna.Framework;
using System;

namespace SpaceEventMod.Core.Physics;

public struct Kinematics<T>(T initialInput, T initialVelocity = default)
{
    public T PreviousPosition = initialInput;
    public T Position = initialInput;
    public T Velocity = initialVelocity;

    public Kinematics<T> SetPreviousPosition(T input)
    {
        this.PreviousPosition = input;
        return this;
    }
}

// based on https://www.youtube.com/watch?v=KPoeNZZ6H4s this video by t3ssel8r
public abstract class SecondOrderDynamics<T>(float frequency, float dampening, float anticipation)
{
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
    public virtual Kinematics<T> Update(float deltaTime, Kinematics<T> currentKinematics, T inputPosition, bool setVelocity = false, T velocity = default)
    {
        Kinematics<T> kinematics = currentKinematics;

        dynamic currentInput = inputPosition;
        dynamic inputVelocity = velocity;
        dynamic nextPosition = currentKinematics.Position;
        dynamic previousVelocity = currentKinematics.Velocity;

        if (setVelocity == false)
        {
            inputVelocity = (currentInput - currentKinematics.PreviousPosition) / deltaTime;
            kinematics.PreviousPosition = inputPosition;
        }

        var k2Constrained = MathF.Max(k2, 1.1f * (deltaTime * deltaTime * 0.25f + deltaTime * k1 * 0.5f));
        nextPosition += deltaTime * previousVelocity; // integrate position with velocity
        previousVelocity += deltaTime * (inputPosition + k3 * inputVelocity - nextPosition - k1 * previousVelocity) / k2Constrained; // integrate velocity with acceleration

        kinematics.Position = nextPosition;
        kinematics.Velocity = previousVelocity;

        return kinematics;
    }

    /// <summary>
    /// Changes how the system will respond to inputs
    /// </summary>
    /// <param name="frequency">The frequency value to change to.</param>
    /// <param name="dampening">The dampening value to change to.</param>
    /// <param name="anticipation">The anticipation value to change to.</param>
    public virtual void ChangeAnimation(float frequency, float dampening, float anticipation)
    {
        k1 = dampening / (MathF.PI * frequency);
        k2 = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
        k3 = anticipation * dampening / (2 * MathF.PI * frequency);
    }
}

public class FloatDynamics(float frequency, float dampening, float anticipation) : SecondOrderDynamics<float>(frequency, dampening, anticipation)
{
}

public class Vector2Dynamics(float frequency, float dampening, float anticipation) : SecondOrderDynamics<Vector2>(frequency, dampening, anticipation)
{
}