using SpaceEventMod.Core.Physics.Interfaces;

namespace SpaceEventMod.Core.Physics.Components.SoftBodies;

internal struct GasFilledSoftBody(float desiredArea, float currentArea, float scaleFactor) : IComponent
{
    public float DesiredArea { get; init; } = desiredArea;
    public float ScaleFactor { get; init; } = scaleFactor;
}
