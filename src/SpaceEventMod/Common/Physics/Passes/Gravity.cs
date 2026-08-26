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
using Terraria;

namespace SpaceEventMod.Common.Physics.Passes;

[Rejects(typeof(NoGravity))]
internal class Gravity(Vector2 gravity, int steps) : IPass
{
    private Vector2 _gravity = gravity;

    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        PhysicsPoint point = physicsObject.Center;
        point.Acceleration += _gravity;
        physicsObject.Center = point;

        if (!physicsObject.HasComponent<PhysicsShape>())
            return;

        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        for (int i = 0; i < shape.Points.Length; i++)
        {
            physicsObject.GetComponent<PhysicsShape>().Points[i].Acceleration += _gravity;
        }
    }
}
