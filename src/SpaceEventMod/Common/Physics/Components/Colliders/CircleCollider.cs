using SpaceEventMod.Common.Physics.Interfaces;

namespace SpaceEventMod.Common.Physics.Components.Colliders;

internal struct CircleCollider(float radius) : IComponent
{
    public float Radius { get; set; } = radius;
}
