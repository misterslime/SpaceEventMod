using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Animation;

public struct Kinematics<T>
{
    public T PreviousPosition;
    public T Position;
    public T Velocity;

    public Kinematics(T initialInput, T initialVelocity = default)
    {
        PreviousPosition = initialInput;
        Position = initialInput;
        Velocity = initialVelocity;
    }

    public Kinematics(T initialInput, T previousPosition, T initialVelocity = default)
    {
        PreviousPosition = previousPosition;
        Position = initialInput;
        Velocity = initialVelocity;
    }
}

public class SecondOrderDynamicsOld(float frequency, float dampening, float anticipation)
{
    private float k1 = dampening / (MathF.PI * frequency);
    private float k2 = 1 / (2 * MathF.PI * frequency * (2 * MathF.PI * frequency));
    private float k3 = anticipation * dampening / (2 * MathF.PI * frequency);

    /// <summary>
    /// Integrates the current position and velocity to target <paramref name="inputPosition"/>.
    /// </summary>
    /// <param name="deltaTime">Change in time.</param>
    /// <param name="currentKinematics">Current kinematic data struct to work with.</param>
    /// <param name="inputPosition">Target position.</param>
    /// <param name="setVelocity">If you want to set the velocity to something set this to true.</param>
    /// <param name="velocity">Velocity to set current velocity to.</param>
    /// <returns>The output position after integration.</returns>
    public Kinematics<T> Update<T>(float deltaTime, Kinematics<T> currentKinematics, dynamic inputPosition, bool setVelocity = false, dynamic velocity = default)
    {
        var kinematics = currentKinematics;

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
}
