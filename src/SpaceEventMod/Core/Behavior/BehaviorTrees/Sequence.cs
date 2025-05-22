using System.Collections.Generic;

namespace SpaceEventMod.Core.Behavior.BehaviorTrees;

public class Sequence(List<Node> children) : Node(children)
{
    public override NodeState Update(int whoAmI)
    {
        bool inProgress = false;

        foreach (Node node in children)
        {
            switch (node.Update(whoAmI))
            {
                case NodeState.Failure:
                    return NodeState.Failure;
                case NodeState.Success:
                    continue;
                case NodeState.InProgress:
                    inProgress = true;
                    continue;
                default:
                    return NodeState.Success;
            }
        }

        return inProgress ? NodeState.InProgress : NodeState.Success;
    }
}