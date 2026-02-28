using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes;

[Needs(typeof(ChildObject))]
[Rejects(typeof(DoesntRepel))]
internal class PointsRepulsion(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        var childObjects = physicsObject.GetInstancedComponents<ChildObject>();

        var points = (from child in childObjects
                      from point in child.Child.GetComponent<PhysicsShape>().Points
                      select point).ToArray();

        foreach (var childObject in childObjects)
        {
            var shape = childObject.Child.GetComponent<PhysicsShape>();

            for (var i = 0; i < points.Length; i++)
            {
                for (var j = 0; j < shape.Points.Length; j++)
                {
                    var point = shape.Points[j];

                    var vector = point.Position - points[i].Position;

                    if (vector.LengthSquared() <= 256f)
                        childObject.Child.GetComponent<PhysicsShape>().Points[j].Acceleration += vector.SafeNormalize(Vector2.Zero) * 0.05f;
                }
            }
        }

    }
}
