using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components;

internal struct ParentObject(PhysicsObject parent) : IComponent
{
    public PhysicsObject Parent { get; init; } = parent;
}
