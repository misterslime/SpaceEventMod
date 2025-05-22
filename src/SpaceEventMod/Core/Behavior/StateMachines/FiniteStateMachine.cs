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

    public void Add(int key, State state) => states.Add(key, state);

    public void Update(float[] arguments = null) => currentState?.Update(arguments);

    public State GetState(int key) => states.TryGetValue(key, out State value) ? value : null;

    public void ForEach(Action<KeyValuePair<int, State>> action)
    {
        foreach (KeyValuePair<int, State> state in states)
            action(state);
    }

    public void SetCurrentState(State state, float[] arguments = null)
    {
        if (currentState == state)
            return;

        currentState?.Exit(arguments);
        currentState = state;
        currentState?.Enter(arguments);
    }

    public void SetCurrentState(int key, float[] arguments = null)
    {
        if (states.TryGetValue(key, out State value))
            SetCurrentState(value, arguments);
    }
}