namespace SpaceEventMod.Core.Behavior.Automata;

// whats next azzy r u gonna make the enemies use turing machines
public interface IAutomata<T>
{
    public T Context { get; set; }

    public void Update();
}
