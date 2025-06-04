using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

public struct LowHealth(float threshhold) : INode
{
    public float threshhold = threshhold;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];
        return npc.life <= threshhold * npc.lifeMax ? NodeState.Success : NodeState.Failure;
    }
}
