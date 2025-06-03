using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Behavior.StateMachines;

namespace SpaceEventMod.Common.Actions.Leaf;

/// <summary>
/// Run a <see cref="FiniteStateMachine"/>.
/// </summary>
/// <param name="stateMachine">State machine to run.</param>
public struct RunStateMachine(FiniteStateMachine stateMachine) : INode
{
    private FiniteStateMachine stateMachine = stateMachine;

    public NodeState Update(int whoAmI)
    {
        stateMachine?.Update();

        return NodeState.Success;
    }
}