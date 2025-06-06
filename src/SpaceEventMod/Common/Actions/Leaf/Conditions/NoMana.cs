using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

public struct NoMana : INode
{
    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not ISquidInk squidInk)
            return NodeState.Failure;

        return squidInk.Mana == 0 ? NodeState.Success : NodeState.Failure;
    }
}
