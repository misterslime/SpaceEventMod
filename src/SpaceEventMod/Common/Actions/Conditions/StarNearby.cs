using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.GameObjects.Asteroids;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Conditions;

public struct StarNearby(float range) : INode
{
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (StarSystem.Stars.Count <= 0)
            return NodeState.Failure;

        Core.GameObjects.Stars.Star closestStar;
        float distanceToStar = float.MaxValue;

        foreach (Core.GameObjects.Stars.Star star in StarSystem.Stars)
        {
            if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                closestStar = star;
            }
        }

        if (Math.Sqrt(distanceToStar) <= range)
            return NodeState.Success;

        return NodeState.Failure;
    }
}
