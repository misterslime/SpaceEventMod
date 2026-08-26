using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Components;
using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Physics.Passes;

[Needs(typeof(ChildObject))]
[Rejects(typeof(DoesntRepel))]
internal class PointsRepulsion(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        IEnumerable<ChildObject> childObjects = physicsObject.GetInstancedComponents<ChildObject>();

        var points = (from child in childObjects
                      from point in child.Child.GetComponent<PhysicsShape>().Points
                      select point).ToArray();

        foreach (ChildObject childObject in childObjects)
        {
            PhysicsShape shape = childObject.Child.GetComponent<PhysicsShape>();

            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < shape.Points.Length; j++)
                {
                    PhysicsPoint point = shape.Points[j];

                    Vector2 vector = point.Position - points[i].Position;

                    if (vector.LengthSquared() <= 256f)
                        childObject.Child.GetComponent<PhysicsShape>().Points[j].Acceleration += vector.SafeNormalize(Vector2.Zero) * 0.05f;
                }
            }
        }

    }
}
