using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components.Animation;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Passes.Integrators;

[Needs(typeof(SecondOrderData))]
internal class SecondOrderIntegration : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        var data = physicsObject.GetComponent<SecondOrderData>();

        var point = physicsObject.Center;
        var dynamics = data.SecondOrderDynamics;
        var input = data.InputPosition;
        var previousInput = data.PreviousInput;

        var k1 = dynamics.GetK1;
        var k2 = dynamics.GetK2;
        var k3 = dynamics.GetK3;

        var deltaInput = (k3 + 1) * input - k3 * previousInput;
        var deltaCurrent = k1 * point.PreviousPosition - (k1 + 1) * point.Position;
        var acceleration = (deltaInput + deltaCurrent) / k2;

        acceleration *= data.DeltaTime * data.DeltaTime;

        var newPoint = point;

        newPoint.Position = 2 * point.Position - point.PreviousPosition + acceleration;
        newPoint.PreviousPosition = point.Position;

        physicsObject.Center = newPoint;
    }
}
