using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Targeting;

public struct TargetNearestStarPosition : INode
{
    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (StarSystem.Stars.Count <= 0 || npc.ModNPC is not ITargetPosition)
            return NodeState.Failure;

        var distanceToStar = float.MaxValue;

        foreach (var star in StarSystem.Stars)
        {
            if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                ((ITargetPosition)npc.ModNPC).TargetPosition = star.GetCenter();
            }
        }

        return NodeState.Success;
    }
}
