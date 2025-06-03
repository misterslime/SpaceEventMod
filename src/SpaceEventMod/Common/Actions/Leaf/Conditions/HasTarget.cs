using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

public struct HasTarget : INode
{
    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];
        return npc.target != -1 ? NodeState.Success : NodeState.Failure;
    }
}
