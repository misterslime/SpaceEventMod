using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Conditions;

public struct CloseToTargetPosition(float range) : INode
{
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not ITargetPosition targetPosition)
            return NodeState.Failure;

        if (targetPosition.TargetPosition.WithinRange(npc.Center, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}
