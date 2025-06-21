using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Behavior.Automata;

public class StateMachine<T>
{
    public record StateTransition<T>(int Next, Func<T, bool> Condition);

    protected Dictionary<int, List<StateTransition<T>>> Transitions = [];
    protected Dictionary<int, IState<T>> States = [];

    /// <summary>
    /// Add a new state to the state machine.
    /// </summary>
    /// <param name="key">Key of the state in the dictionary.</param>
    /// <param name="state">State to add.</param>
    public StateMachine<T> Add(int key, IState<T> state)
    {
        States.Add(key, state);

        return this;
    }

    /// <summary>
    /// Adds a transition rule to the machine's transition table.
    /// </summary>
    /// <param name="from">State to transition from</param>
    /// <param name="to">State index to transition to</param>
    /// <param name="condition">Condition for transitioning.</param>
    /// <returns>The machine, so you can easily add more transitions and states.</returns>
    public StateMachine<T> AddTransition(int from, int to, Func<T, bool> condition)
    {
        if (!Transitions.ContainsKey(from))
            Transitions[from] = [];

        Transitions[from].Add(new StateTransition<T>(to, condition));

        return this;
    }

    /// <summary>
    /// Update an automata with the states and transitions in this container.
    /// </summary>
    public void UpdateMachine(IAutomata<T> automata, T context)
    {
        CheckTransitions(automata, context);

        if (States.TryGetValue(automata.CurrentStateIndex, out var state))
            automata.Update(context, state);
    }

    private void CheckTransitions(IAutomata<T> automata, T context)
    {
        if (!Transitions.TryGetValue(automata.CurrentStateIndex, out var state))
            return;

        var statesToTransitionTo = state?.Where(s => s.Condition(context)).ToList();

        if (!statesToTransitionTo.Any())
            return;

        automata.TransitionTo(context, States, statesToTransitionTo.First().Next);
    }
}

// this is solely here bc i think it looks nicer
public static class AutomataExtensionMethods
{
    public static void Update<T>(this IAutomata<T> automata, T context, StateMachine<T> statesAndTransitions)
    {
        statesAndTransitions.UpdateMachine(automata, context);
    }
}