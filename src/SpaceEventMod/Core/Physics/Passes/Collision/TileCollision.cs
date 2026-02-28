using SpaceEventMod.Core.Physics.Collision;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Passes.Collision;

internal class TileCollision(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        physicsObject.Center = TileCollisionHelper.CheckPoint(physicsObject.Center, 6, 16);

        if (!physicsObject.HasComponent<PhysicsShape>())
            return;

        var shape = physicsObject.GetComponent<PhysicsShape>();

        for (var i = 0; i < shape.Points.Length; i++)
            physicsObject.GetComponent<PhysicsShape>().Points[i] = TileCollisionHelper.CheckPoint(shape.Points[i], 6, 16);
    }
}
