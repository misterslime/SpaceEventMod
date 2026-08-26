using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Components;
using SpaceEventMod.Common.Physics.Interfaces;
using SpaceEventMod.Content.CellularGrowth.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader.Core;
using static System.Net.Mime.MediaTypeNames;

namespace SpaceEventMod.Common.Physics.Passes.Collision;


[Needs(typeof(PhysicsShape))]
internal class ProjectileCollision(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        foreach (var projectile in Main.ActiveProjectiles)
        {
            for (int j = 0; j < shape.Points.Length; j++)
            {
                PhysicsPoint point = shape.Points[j];

                if (projectile.getRect().Contains(point.Position.ToPoint()))
                {
                    physicsObject.GetComponent<PhysicsShape>().Points[j].Acceleration += projectile.velocity;
                }
            }
        }
    }
}
