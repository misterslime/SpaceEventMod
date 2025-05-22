namespace SpaceEventMod.Core.Behavior.StateMachines;

public class EntityState<T>(FiniteStateMachine stateMachine, T entity) : State(stateMachine)
{
    protected T Entity = entity;
}