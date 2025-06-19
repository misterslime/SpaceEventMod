using System;

namespace SpaceEventMod.Core.Behavior.Automata;

public record StateTransition<T>(int Next, Func<bool> Condition);

public interface IState<T>
{
    /// <summary>
    /// This gets run when the state is first switched to.
    /// </summary>
    /// <param name="stateMachine">State machine this state belongs to.</param>
    public void Enter(IAutomata<T> stateMachine);

    /// <summary>
    /// This gets run when the state is being switched away from.
    /// </summary>
    /// <param name="stateMachine">State machine this state belongs to.</param>
    public void Exit(IAutomata<T> stateMachine);

    /// <summary>
    /// This gets run every frame the state is active in.
    /// </summary>
    /// <param name="stateMachine">State machine this state belongs to.</param>
    /// <returns>Return true if this state is completed and can be removed from a stack.</returns>
    public bool Update(IAutomata<T> stateMachine);
}