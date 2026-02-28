using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes.Collision;

[Needs(typeof(PhysicsShape))]
internal class BouncePlayers : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        var shape = physicsObject.GetComponent<PhysicsShape>();

        foreach (var player in Main.ActivePlayers)
        {
            for (var j = 0; j < shape.Points.Length; j++)
            {
                var point = shape.Points[j];

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
