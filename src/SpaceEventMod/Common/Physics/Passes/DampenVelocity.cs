using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Components;
using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Passes;

[Needs(typeof(PhysicsShape))]
internal class DampenVelocity(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        for (int i = 0; i < shape.Points.Length; i++)
        {
            PhysicsPoint shapePoint = physicsObject.GetComponent<PhysicsShape>().Points[i];

            physicsObject.GetComponent<PhysicsShape>().Points[i].PreviousPosition = Vector2.Lerp(shapePoint.Position, shapePoint.PreviousPosition, 0.96f);
        }
    }
}
