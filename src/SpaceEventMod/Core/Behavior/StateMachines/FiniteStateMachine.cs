using System;
using System.Collections.Generic;

namespace SpaceEventMod.Core.Behavior.StateMachines;

public class FiniteStateMachine
{
    protected Dictionary<int, State> states;
    protected State currentState;

    public FiniteStateMachine()
    {
        states = [];
    }

    /// <summary>
    /// Add a new state to the state machine.
    /// </summary>
    /// <param name="key">Key of the state in the dictionary.</param>
    /// <param name="state">State to add.</param>
    public void Add(int key, State state) => states.Add(key, state);

    /// <summary>
    /// Update the current state in the machine.
    /// </summary>
    /// <param name="arguments">Random misc state specific arguments.</param>
    public void Update(float[] arguments = null) => currentState?.Update(this, arguments);

    /// <summary>
    /// Gets a state from the machine.
    /// </summary>
    /// <param name="key">Key to search for.</param>
    /// <returns>The state with the specified key. If the key is not found it will return <see langword="null"/>.</returns>
    public State GetState(int key) => states.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// Runs an <see cref="Action"/> over all the states in the machine.
    /// </summary>
    /// <param name="action">Action to run.</param>
    public void ForEach(Action<KeyValuePair<int, State>> action)
    {
        foreach (var state in states)
            action(state);
    }

    /// <summary>
    /// Changes the current state of the machine.
    /// </summary>
    /// <param name="state">State to change over to.</param>
    /// <param name="arguments">Arguments to pass into the exit and enter methods of each state.</param>
    public void SetCurrentState(State state, float[] arguments = null)
    {
        if (currentState == state)
            return;

        currentState?.Exit(this, arguments);
        currentState = state;
        currentState?.Enter(this, arguments);
    }

    /// <summary>
    /// hanges the current state of the machine.
    /// </summary>
    /// <param name="key">Key of the state to change over to.</param>
    /// <param name="arguments">Arguments to pass into the exit and enter methods of each state.</param>
    public void SetCurrentState(int key, float[] arguments = null)
    {
        if (states.TryGetValue(key, out var value))
            SetCurrentState(value, arguments);
    }
}