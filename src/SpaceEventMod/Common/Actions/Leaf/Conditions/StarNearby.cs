using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

public struct StarNearby(float range) : INode
{
    private float range = range;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (StarSystem.Stars.Count <= 0)
            return NodeState.Failure;

        Core.GameObjects.Stars.Star closestStar;
        var distanceToStar = float.MaxValue;

        foreach (var star in StarSystem.Stars)
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
