using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Behavior.StateMachines;

namespace SpaceEventMod.Common.Actions;

public class RunStateMachine : Node
{
    private FiniteStateMachine stateMachine;

    public RunStateMachine(FiniteStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override NodeState Update(int whoAmI)
    {
        stateMachine?.Update();

        return NodeState.Success;
    }
}