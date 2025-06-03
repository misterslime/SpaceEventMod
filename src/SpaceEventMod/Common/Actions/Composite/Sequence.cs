using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System.Collections.Generic;

namespace SpaceEventMod.Common.Actions.Composite;

/// <summary>
/// A Sequence node. Runs until a child node fails.
/// </summary>
/// <param name="children">The child nodes of this leaf.</param>
public struct Sequence(params INode[] children) : INode
{
    private INode[] Children = children;

    public NodeState Update(int whoAmI)
    {
        foreach (var node in children)
        {
            switch (node.Update(whoAmI))
            {
                case NodeState.Failure:
                    return NodeState.Failure;
                case NodeState.Success:
                    continue;
                case NodeState.InProgress:
                    return NodeState.InProgress;
                default:
                    return NodeState.Success;
            }
        }

        return NodeState.Success;
    }
}