using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Components.Animation;
using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Passes.Integrators;

[Needs(typeof(SecondOrderData))]
internal class SecondOrderIntegration : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        SecondOrderData data = physicsObject.GetComponent<SecondOrderData>();

        var point = physicsObject.Center;
        var dynamics = data.SecondOrderDynamics;
        var input = data.InputPosition;
        var previousInput = data.PreviousInput;

        float k1 = dynamics.GetK1;
        float k2 = dynamics.GetK2;
        float k3 = dynamics.GetK3;

        Vector2 deltaInput = (k3 + 1) * input - k3 * previousInput;
        Vector2 deltaCurrent = k1 * point.PreviousPosition - (k1 + 1) * point.Position;
        Vector2 acceleration = (deltaInput + deltaCurrent) / k2;

        acceleration *= data.DeltaTime * data.DeltaTime;

        var newPoint = point;

        newPoint.Position = 2 * point.Position - point.PreviousPosition + acceleration;
        newPoint.PreviousPosition = point.Position;

        physicsObject.Center = newPoint;
    }
}
