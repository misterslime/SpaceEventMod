using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

public struct LowHealth(float threshhold) : INode
{
    public float threshhold = threshhold;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];
        return npc.life <= threshhold * npc.lifeMax ? NodeState.Success : NodeState.Failure;
    }
}
