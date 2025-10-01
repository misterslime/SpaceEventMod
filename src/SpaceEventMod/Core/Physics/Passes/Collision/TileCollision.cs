using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Collision;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Passes.Collision;

internal class TileCollision(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        physicsObject.Center = TileCollisionHelper.CheckPoint(physicsObject.Center, 6, 16);

        if (!physicsObject.HasComponent<PhysicsShape>())
            return;

        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        for (int i = 0; i < shape.Points.Length; i++)
            physicsObject.GetComponent<PhysicsShape>().Points[i] = TileCollisionHelper.CheckPoint(shape.Points[i], 6, 16);
    }
}
