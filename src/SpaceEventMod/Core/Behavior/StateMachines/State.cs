namespace SpaceEventMod.Core.Behavior.StateMachines;

public abstract class State
{
    public delegate void StateDelegate(FiniteStateMachine stateMachine, float[] arguments);

    public StateDelegate OnEnter { get; set; } = null;

    public StateDelegate OnExit { get; set; } = null;

    public StateDelegate OnUpdate { get; set; } = null;

    /// <summary>
    /// This gets run when the state is first switched to.
    /// </summary>
    /// <param name="stateMachine">State machine this state belongs to.</param>
    /// <param name="arguments">Misc arguments to pass in.</param>
    public virtual void Enter(FiniteStateMachine stateMachine, float[] arguments = null) => OnEnter?.Invoke(stateMachine, arguments);

    /// <summary>
    /// This gets run when the state is being switched away from.
    /// </summary>
    /// <param name="stateMachine">State machine this state belongs to.</param>
    /// <param name="arguments">Misc arguments to pass in.</param>
    public virtual void Exit(FiniteStateMachine stateMachine, float[] arguments = null) => OnExit?.Invoke(stateMachine, arguments);

    /// <summary>
    /// This gets run every frame the state is active in.
    /// </summary>
    /// <param name="stateMachine">State machine this state belongs to.</param>
    /// <param name="arguments">Misc arguments to pass in.</param>
    public virtual void Update(FiniteStateMachine stateMachine, float[] arguments = null) => OnUpdate?.Invoke(stateMachine, arguments);
}