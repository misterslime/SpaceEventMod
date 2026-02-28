using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Passes.Collision;

[Needs(typeof(PhysicsShape))]
internal class SoftBodyCollision(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        var shape = physicsObject.GetComponent<PhysicsShape>();

        for (var i = 0; i < SoftBodyManager.SoftBodies.Count; i++)
        {
            if (physicsObject.Equals(SoftBodyManager.SoftBodies[i]))
                continue;

            var shape2 = SoftBodyManager.SoftBodies[i].GetComponent<PhysicsShape>();

            for (var j = 0; j < shape.Points.Length; j++)
            {
                var point = shape.Points[j];

                for (var k = 0; k < shape2.Points.Length; k++)
                {
                    var point2 = shape2.Points[k];

                    var collisionAxis = point.Position - point2.Position;
                    var dist = collisionAxis.Length();

                    if (dist <= 30f)
                    {
                        collisionAxis /= dist;
                        var delta = 30f - dist;

                        physicsObject.GetComponent<PhysicsShape>().Points[j].Position += 0.5f * delta * collisionAxis;
                        SoftBodyManager.SoftBodies[i].GetComponent<PhysicsShape>().Points[k].Position -= 0.5f * delta * collisionAxis;
                    }
                }
            }
        }
    }
}
