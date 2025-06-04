using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

public struct HasTarget : INode
{
    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];
        return npc.target == -1 || !npc.HasValidTarget ? NodeState.Failure : NodeState.Success;
    }
}
