namespace SpaceEventMod.Core.Behavior.StateMachines;

public class EntityState<T>(T entity) : State()
{
    protected T Entity = entity;
}