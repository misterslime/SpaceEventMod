using System.Collections.Generic;

namespace SpaceEventMod.Core.Behavior.BehaviorTrees;

public enum NodeState
{
    Success,
    Failure,
    InProgress
}

/// <summary>
/// A leaf node in a behaviour tree. This contains the basic behaviours of the NPC it is being run on.
/// </summary>
public abstract class Node
{
    private NodeState State { get; set; }
    public Node Parent;

    private List<Node> Children = new List<Node>();

    public virtual NodeState Update(int whoAmI) => NodeState.Failure;

    public Node()
    {

    }

    public Node(List<Node> children)
    {
        foreach (Node childNode in children)
        {
            childNode.Parent = this;
            Children.Add(childNode);
        }
    }
}