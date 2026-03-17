using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Utilities;

internal static class Boids
{
    public static Vector2 Cohesion(this NPC npc, NPC[] neighbors)
    {
        var centerOfMass = npc.Center;
        var count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != npc.whoAmI)
            {
                centerOfMass += neighbor.Center;
                count++;
            }
        }

        if (count > 0)
        {
            centerOfMass /= count;
            return (centerOfMass - npc.Center).SafeNormalize(Vector2.Zero);
        }

        return Vector2.Zero;
    }

    public static Vector2 Separation(this NPC npc, NPC[] neighbors, float separationRadius)
    {
        var moveAway = Vector2.Zero;
        var count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != npc.whoAmI && Vector2.Distance(npc.Center, neighbor.Center) < separationRadius)
            {
                var difference = npc.Center - neighbor.Center;
                moveAway += difference.SafeNormalize(Vector2.Zero) / difference.Length();
                count++;
            }
        }

        if (count > 0)
        {
            moveAway /= count;
        }

        return moveAway.SafeNormalize(Vector2.Zero);
    }

    public static Vector2 Alignment(this NPC npc, NPC[] neighbors)
    {
        var averageVelocity = Vector2.Zero;
        var count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != npc.whoAmI)
            {
                averageVelocity += neighbor.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            averageVelocity /= count;
            return averageVelocity.SafeNormalize(Vector2.Zero);
        }

        return Vector2.Zero;
    }

    public static Vector2 Surrounding(this NPC npc, NPC[] neighbors, float separationRadius)
    {
        var direction = Vector2.Zero;
        var distanceToNeighbor = float.MaxValue;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != npc.whoAmI && Vector2.DistanceSquared(neighbor.Center, npc.Center + npc.velocity) < distanceToNeighbor)
            {
                distanceToNeighbor = Vector2.DistanceSquared(neighbor.Center, npc.Center + npc.velocity);
                direction = npc.Center - Main.player[npc.target].Center;
                //direction = NPC.Center - neighbor.Center;
            }
        }

        direction = new Vector2(-direction.Y, direction.X);
        return direction.SafeNormalize(Vector2.Zero);
    }
}
