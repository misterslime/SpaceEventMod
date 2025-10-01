using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Passes;

[Needs(typeof(PhysicsShape), typeof(AnchorObjectCentroid))]
internal class AnchorShape : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        AnchorObjectCentroid anchor = physicsObject.GetComponent<AnchorObjectCentroid>();
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        if (anchor.AnchorObject)
        {
            PhysicsPoint position = physicsObject.Center;

            position.Position = shape.GetCentroid();

            physicsObject.Center = position;
        }
        else
        {
            Vector2 objectPosition = physicsObject.Center.Position;
            Vector2 shapePosition = shape.GetCentroid();

            physicsObject.GetComponent<PhysicsShape>().MoveBy(objectPosition - shapePosition);
        }
    }
}
