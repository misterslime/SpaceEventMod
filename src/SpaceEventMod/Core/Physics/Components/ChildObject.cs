using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components;

internal struct ChildObject(PhysicsObject child) : IComponent, IInstancedComponent
{
    public PhysicsObject Child { get; init; } = child;
}
