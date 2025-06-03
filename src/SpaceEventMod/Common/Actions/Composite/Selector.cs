using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System.Collections.Generic;

namespace SpaceEventMod.Common.Actions.Composite;

/// <summary>
/// A Selector node. Runs until a child node is in progress or until a node returns success.
/// </summary>
/// <param name="children">The child nodes of this leaf.</param>
public struct Selector(params INode[] children) : INode
{
    private INode[] Children = children;

    public NodeState Update(int whoAmI)
    {
        foreach (var node in children)
        {
            switch (node.Update(whoAmI))
            {
                case NodeState.Failure:
                    continue;
                case NodeState.Success:
                    return NodeState.Success;
                case NodeState.InProgress:
                    return NodeState.InProgress;
                default:
                    continue;
            }
        }

        return NodeState.Failure;
    }
}