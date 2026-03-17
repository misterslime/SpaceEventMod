using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Miscellaneous.NPCs;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Collision;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes.Collision;

[Needs(typeof(PhysicsShape))]
internal class SoftBodyCollision(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        for (int i = 0; i < SoftBodyManager.SoftBodies.Count; i++)
        {
            if (physicsObject.Equals(SoftBodyManager.SoftBodies[i]))
                continue;

            PhysicsShape shape2 = SoftBodyManager.SoftBodies[i].GetComponent<PhysicsShape>();

            for (int j = 0; j < shape.Points.Length; j++)
            {
                PhysicsPoint point = shape.Points[j];

                for (int k = 0; k < shape2.Points.Length; k++)
                {
                    PhysicsPoint point2 = shape2.Points[k];

                    Vector2 collisionAxis = point.Position - point2.Position;
                    float dist = collisionAxis.Length();

                    if (dist <= 30f)
                    {
                        collisionAxis /= dist;
                        float delta = 30f - dist;

                        physicsObject.GetComponent<PhysicsShape>().Points[j].Position += 0.5f * delta * collisionAxis;
                        SoftBodyManager.SoftBodies[i].GetComponent<PhysicsShape>().Points[k].Position -= 0.5f * delta * collisionAxis;
                    }
                }
            }
        }
    }
}
