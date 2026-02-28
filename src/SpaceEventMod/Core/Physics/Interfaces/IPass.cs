namespace SpaceEventMod.Core.Physics.Interfaces;

internal interface IPass
{
    public int Steps { get; init; }

    public void Pass(PhysicsObject physicsObject);
}
