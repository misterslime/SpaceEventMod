using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;

namespace SpaceEventMod.Core.Behavior.Automata;

public class PushdownAutomaton<T>() : IAutomata<T>
{
    public int CurrentStateIndex => stack.Count > 0 ? stack.Peek() : -1;

    protected Stack<int> stack = [];

    /// <summary>
    /// Update the current state in the machine.
    /// </summary>
    public void Update(T context, IState<T> state)
    {
        if (state.Update(context))
            PopState(context, state);
    }

    /// <summary>
    /// Proper state transition handling.
    /// </summary>
    public void TransitionTo(T context, Dictionary<int, IState<T>> states, int key)
    {
        if (CurrentStateIndex == key)
            return;

        PushState(key);
        states[key].Enter(context);
    }

    /// <summary>
    /// Pushes a state to the top of the stack.
    /// </summary>
    /// <param name="key">Key of the state to push.</param>
    public void PushState(int key)
    {
        stack.Push(key);
    }

    /// <summary>
    /// Pops the current state from the top of the stack.
    /// </summary>
    public void PopState(T context, IState<T> state)
    {
        if (stack.Count == 0)
            return;

        state.Exit(context);
        stack.Pop();
    }
}
