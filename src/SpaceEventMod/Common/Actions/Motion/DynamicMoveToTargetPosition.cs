using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Motion;

public struct DynamicMoveToTargetPosition(bool rotate) : INode
{
    private bool rotate = rotate;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not ITargetPosition || npc.ModNPC is not IDynamicMotion)
            return NodeState.Failure;

        npc.Center = ((IDynamicMotion)npc.ModNPC).SecondOrderSolver.Update(1, ((ITargetPosition)npc.ModNPC).TargetPosition);

        return NodeState.Success;
    }
}
