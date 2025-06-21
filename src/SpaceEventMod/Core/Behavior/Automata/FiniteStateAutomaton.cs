using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Behavior.Automata;

public class FiniteStateAutomaton<T>() : IAutomata<T>
{
    public int CurrentStateIndex => currentIndex;

    protected int currentIndex;

    /// <summary>
    /// Update the current state in the machine.
    /// </summary>
    public void Update(T context, IState<T> state)
    {
        state?.Update(context);
    }

    /// <summary>
    /// hanges the current state of the machine.
    /// </summary>
    /// <param name="key">Key of the state to change over to.</param>
    public void TransitionTo(T context, Dictionary<int, IState<T>> states, int key)
    {
        if (currentIndex == key)
            return;

        states[currentIndex].Exit(context);
        currentIndex = key;
        states[currentIndex].Enter(context);
    }
}