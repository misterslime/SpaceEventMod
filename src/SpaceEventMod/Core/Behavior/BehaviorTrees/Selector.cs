using System.Collections.Generic;

namespace SpaceEventMod.Core.Behavior.BehaviorTrees;

/// <summary>
/// A Selector node. Runs until a child node is in progress or until a node returns success.
/// </summary>
/// <param name="children">The child nodes of this leaf.</param>
public class Selector(List<Node> children) : Node(children)
{
    public override NodeState Update(int whoAmI)
    {
        foreach (Node node in children)
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