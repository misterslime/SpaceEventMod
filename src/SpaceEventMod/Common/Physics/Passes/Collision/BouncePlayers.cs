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

namespace SpaceEventMod.Common.Physics.Passes.Collision;

[Needs(typeof(PhysicsShape))]
internal class BouncePlayers : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        foreach (var player in Main.ActivePlayers)
        {
            for (int j = 0; j < shape.Points.Length; j++)
            {
                PhysicsPoint point = shape.Points[j];

                if (player.getRect().Contains(point.Position.ToPoint()))
                {
                    physicsObject.GetComponent<PhysicsShape>().Points[j].Acceleration += player.velocity * 3f;

                    if (shape.Closed)
                        player.velocity *= -2f;

                    return;
                }
            }

        }
    }
}
