using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Behavior.Automata;

public class FiniteStateMachine<T>(T context) : IAutomata<T>
{
    protected Dictionary<int, List<StateTransition<T>>> TransitionTable = [];
    protected Dictionary<int, IState<T>> States = [];
    protected IState<T> CurrentState => States[currentIndex];

    public T Context { get; set; } = context;

    private int currentIndex;

    /// <summary>
    /// Add a new state to the state machine.
    /// </summary>
    /// <param name="key">Key of the state in the dictionary.</param>
    /// <param name="state">State to add.</param>
    public FiniteStateMachine<T> Add(int key, IState<T> state)
    {
        States.Add(key, state);

        return this;
    }

    /// <summary>
    /// Update the current state in the machine.
    /// </summary>
    public void Update()
    {
        UpdateState();
        CurrentState?.Update(this);
    }

    /// <summary>
    /// hanges the current state of the machine.
    /// </summary>
    /// <param name="key">Key of the state to change over to.</param>
    public void SetState(int key)
    {
        if (States.TryGetValue(key, out var state))
        {
            if (CurrentState == state)
                return;

            CurrentState?.Exit(this);
            currentIndex = key;
            CurrentState?.Enter(this);
        }
    }

    private void UpdateState()
    {
        if (!TransitionTable.TryGetValue(currentIndex, out var value))
            return;

        var statesToTransitionTo = value?.Where(s => s.Condition()).ToList();

        if (!statesToTransitionTo.Any())
            return;

        SetState(statesToTransitionTo.First().Next);
    }

    /// <summary>
    /// Adds a transition rule to the machine's transition table.
    /// </summary>
    /// <param name="from">State to transition from</param>
    /// <param name="to">State index to transition to</param>
    /// <param name="condition">Condition for transitioning.</param>
    /// <returns>The machine, so you can easily add more transitions and states.</returns>
    public FiniteStateMachine<T> AddTransition(int from, int to, Func<bool> condition)
    {
        if (!TransitionTable.ContainsKey(from))
            TransitionTable[from] = [];

        TransitionTable[from].Add(new StateTransition<T>(to, condition));

        return this;
    }
}