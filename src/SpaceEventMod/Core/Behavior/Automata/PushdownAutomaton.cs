using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Behavior.Automata;

public class PushdownAutomaton<T>(T context) : IAutomata<T>
{
    protected Dictionary<int, List<StateTransition<T>>> TransitionTable = [];
    protected Dictionary<int, IState<T>> States = [];
    protected Stack<int> stateStack = [];

    protected IState<T> CurrentState => stateStack.Count > 0 ? States[stateStack.Peek()] : null;
    public T Context { get; set; } = context;

    /// <summary>
    /// Add a new state to the state machine.
    /// </summary>
    /// <param name="key">Key of the state in the dictionary.</param>
    /// <param name="state">State to add.</param>
    public PushdownAutomaton<T> Add(int key, IState<T> state)
    {
        States.Add(key, state);

        return this;
    }

    /// <summary>
    /// Pushes a state to the top of the stack.
    /// </summary>
    /// <param name="key">Key of the state to push.</param>
    public void PushState(int key)
    {
        if (!States.TryGetValue(key, out var state))
            return;

        if (CurrentState == state)
            return;

        stateStack.Push(key);
        state.Enter(this);
    }

    /// <summary>
    /// Update the current state in the machine.
    /// </summary>
    public void Update()
    {
        UpdateState();

        if (CurrentState.Update(this))
            PopState();
    }

    /// <summary>
    /// Pops the current state from the top of the stack.
    /// </summary>
    public void PopState()
    {
        if (stateStack.Count == 0)
            return;

        CurrentState.Exit(this);
        stateStack.Pop();
    }

    private void UpdateState()
    {
        if (stateStack.Count == 0 || !TransitionTable.TryGetValue(stateStack.Peek(), out var value))
            return;

        var statesToTransitionTo = value?.Where(s => s.Condition()).ToList();

        if (!statesToTransitionTo.Any())
            return;

        PushState(statesToTransitionTo.First().Next);
    }

    /// <summary>
    /// Adds a transition rule to the machine's transition table.
    /// </summary>
    /// <param name="from">State to transition from</param>
    /// <param name="to">State index to transition to</param>
    /// <param name="condition">Condition for transitioning.</param>
    /// <returns>The machine, so you can easily add more transitions and states.</returns>
    public PushdownAutomaton<T> AddTransition(int from, int to, Func<bool> condition)
    {
        if (!TransitionTable.ContainsKey(from))
            TransitionTable[from] = [];

        TransitionTable[from].Add(new StateTransition<T>(to, condition));

        return this;
    }
}
