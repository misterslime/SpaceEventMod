using System.Collections.Generic;

namespace SpaceEventMod.Core.Behavior.Automata;

// whats next azzy r u gonna make the enemies use turing machines
public interface IAutomata<T>
{
    public int CurrentStateIndex { get; }

    public void Update(T context, IState<T> state);

    public void TransitionTo(T context, Dictionary<int, IState<T>> states, int key);
}
