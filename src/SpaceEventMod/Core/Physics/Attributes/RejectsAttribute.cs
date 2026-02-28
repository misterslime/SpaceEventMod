using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Physics.Attributes;

internal class RejectsAttribute(params Type[] values) : Attribute
{
    public HashSet<Type> Types { get; init; } = values.ToHashSet();
}
