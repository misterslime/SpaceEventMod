using System;

namespace SpaceEventMod.Core.Behavior.Automata;

public interface IState<T>
{
    /// <summary>
    /// This gets run when the state is first switched to.
    /// </summary>
    /// <param name="context">The context this state is running with.</param>
    public void Enter(T context);

    /// <summary>
    /// This gets run when the state is being switched away from.
    /// </summary>
    /// <param name="context">The context this state is running with.</param>
    public void Exit(T context);

    /// <summary>
    /// This gets run every frame the state is active in.
    /// </summary>
    /// <param name="context">The context this state is running with.</param>
    /// <returns>Return true if this state is completed and can be removed from a stack.</returns>
    public bool Update(T context);
}