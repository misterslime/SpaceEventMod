using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Passes.Integrators;

internal class VerletIntegration : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        physicsObject.Center = Integrate(physicsObject.Center);

        if (!physicsObject.HasComponent<PhysicsShape>())
            return;

        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        for (int i = 0; i < physicsObject.GetComponent<PhysicsShape>().Points.Length; i++)
            physicsObject.GetComponent<PhysicsShape>().Points[i] = Integrate(shape.Points[i]);
    }

    private PhysicsPoint Integrate(PhysicsPoint point)
    {
        PhysicsPoint newPoint = point;

        newPoint.PreviousPosition = point.Position;
        newPoint.Position = 2 * point.Position - point.PreviousPosition + point.Acceleration;
        newPoint.Acceleration = Vector2.Zero;

        return newPoint;
    }
}
