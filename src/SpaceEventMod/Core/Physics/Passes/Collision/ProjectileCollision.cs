using SpaceEventMod.Content.CellularGrowth.NPCs;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader.Core;
using static System.Net.Mime.MediaTypeNames;

namespace SpaceEventMod.Core.Physics.Passes.Collision;


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
