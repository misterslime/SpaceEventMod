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
public interface INode
{
    public NodeState Update(BehaviorTree parentTree, int whoAmI);
}