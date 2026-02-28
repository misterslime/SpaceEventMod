using System;

namespace SpaceEventMod.Common.NPCs.Attributes;

internal class StateProcessAttribute<T>(T state) : Attribute
{
    public T State { get; set; } = state;
}
