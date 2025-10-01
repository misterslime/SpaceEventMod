using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components.Animation;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Passes.Integrators;

[Needs(typeof(SecondOrderData))]
internal class SecondOrderIntegration : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        SecondOrderData data = physicsObject.GetComponent<SecondOrderData>();

        var currentPoint = physicsObject.Center;
        var secondOrderDynamics = data.SecondOrderDynamics;

        Vector2 currentInput = data.InputPosition;
        Vector2 inputVelocity = data.Velocity;
        Vector2 nextPosition = physicsObject.Center.Position;
        Vector2 previousVelocity = physicsObject.Center.Velocity;

        if (data.SetVelocity == false)
        {
            inputVelocity = (currentInput - physicsObject.Center.PreviousPosition) / data.DeltaTime;
            currentPoint.PreviousPosition = data.InputPosition;
        }

        var k2Constrained = MathF.Max(secondOrderDynamics.GetK2, 1.1f * (data.DeltaTime * data.DeltaTime * 0.25f + data.DeltaTime * secondOrderDynamics.GetK1 * 0.5f));
        nextPosition += data.DeltaTime * previousVelocity; // integrate position with velocity
        previousVelocity += data.DeltaTime * (data.InputPosition + secondOrderDynamics.GetK3 * inputVelocity - nextPosition - secondOrderDynamics.GetK1 * previousVelocity) / k2Constrained; // integrate velocity with acceleration

        currentPoint.Position = nextPosition;
        currentPoint.Velocity = previousVelocity;

        physicsObject.Center = currentPoint;
    }
}
