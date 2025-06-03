using SpaceEventMod.Core.Behavior.BehaviorTrees;

namespace SpaceEventMod.Common.Actions;

public struct AlwaysSucceeds : INode
{
    public NodeState Update(int whoAmI)
    {
        return NodeState.Success;
    }
}
