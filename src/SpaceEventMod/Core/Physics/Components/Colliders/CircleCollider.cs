using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components.Colliders;

internal struct CircleCollider(float radius) : IComponent
{
    public float Radius { get; set; } = radius;
}
