using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

internal static class Integrators
{
    public static PhysicsPoint VerletIntegration(PhysicsPoint point, object integrationParameters)
    {
        var previousPosition = point.PreviousPosition;

        point.PreviousPosition = point.Position;
        point.Position = 2 * point.Position - previousPosition + point.Acceleration;
        point.Acceleration = Vector2.Zero;

        return point;
    }

    public static PhysicsPoint SemiImplicitEulerIntegration(PhysicsPoint point, object integrationParameters)
    {
        point.Velocity += point.Acceleration;
        point.Position += point.Velocity;
        point.Acceleration = Vector2.Zero;

        return point;
    }

    // based on https://www.youtube.com/watch?v=KPoeNZZ6H4s this video by t3ssel8r
    // computationally kinda expensive
    public static PhysicsPoint SecondOrderIntegration(PhysicsPoint point, object integrationParameters)
    {
        if (integrationParameters is not SecondOrderParameters secondOrderParameters)
            throw new InvalidOperationException("Tried to run second order integration without a valid integration parameter object.");

        var currentPoint = point;
        var secondOrderDynamics = secondOrderParameters.SecondOrderDynamics;

        dynamic currentInput = secondOrderParameters.InputPosition;
        dynamic inputVelocity = secondOrderParameters.Velocity;
        dynamic nextPosition = point.Position;
        dynamic previousVelocity = point.Velocity;

        if (secondOrderParameters.SetVelocity == false)
        {
            inputVelocity = (currentInput - point.PreviousPosition) / secondOrderParameters.DeltaTime;
            currentPoint.PreviousPosition = secondOrderParameters.InputPosition;
        }

        var k2Constrained = MathF.Max(secondOrderDynamics.GetK2, 1.1f * (secondOrderParameters.DeltaTime * secondOrderParameters.DeltaTime * 0.25f + secondOrderParameters.DeltaTime * secondOrderDynamics.GetK1 * 0.5f));
        nextPosition += secondOrderParameters.DeltaTime * previousVelocity; // integrate position with velocity
        previousVelocity += secondOrderParameters.DeltaTime * (secondOrderParameters.InputPosition + secondOrderDynamics.GetK3 * inputVelocity - nextPosition - secondOrderDynamics.GetK1 * previousVelocity) / k2Constrained; // integrate velocity with acceleration

        currentPoint.Position = nextPosition;
        currentPoint.Velocity = previousVelocity;

        return currentPoint;
    }
}
