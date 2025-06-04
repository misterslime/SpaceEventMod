using SpaceEventMod.Core.Behavior.BehaviorTrees;

namespace SpaceEventMod.Common.Actions.Decorator;

public struct Inverter(INode child) : INode
{
    private INode Child = child;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var childState = Child.Update(parentTree, whoAmI);

        if (childState == NodeState.InProgress)
            return NodeState.InProgress;

        return childState == NodeState.Success ? NodeState.Failure : NodeState.Success;
    }
}
