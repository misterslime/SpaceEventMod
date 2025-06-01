using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Behavior.StateMachines;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Run a <see cref="FiniteStateMachine"/>.
/// </summary>
/// <param name="stateMachine">State machine to run.</param>
public class RunStateMachine(FiniteStateMachine stateMachine) : Node
{
    private FiniteStateMachine stateMachine = stateMachine;

    public override NodeState Update(int whoAmI)
    {
        stateMachine?.Update();

        return NodeState.Success;
    }
}