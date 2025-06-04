using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Actions.Decorator;

public struct Inverter(INode child) : INode
{
    private INode Child = child;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        NodeState childState = Child.Update(parentTree, whoAmI);

        if (childState == NodeState.InProgress)
            return NodeState.InProgress;

        return childState == NodeState.Success ? NodeState.Failure : NodeState.Success;
    }
}
