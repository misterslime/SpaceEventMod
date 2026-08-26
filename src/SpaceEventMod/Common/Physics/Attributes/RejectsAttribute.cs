using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Attributes;

internal class RejectsAttribute(params Type[] values) : Attribute
{
    public HashSet<Type> Types { get; init; } = values.ToHashSet();
}
