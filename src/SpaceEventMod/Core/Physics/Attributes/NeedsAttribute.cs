using System;

namespace SpaceEventMod.Core.Physics.Attributes;

internal class NeedsAttribute(params Type[] values) : Attribute
{
    public Type[] Types { get; init; } = values;
}
