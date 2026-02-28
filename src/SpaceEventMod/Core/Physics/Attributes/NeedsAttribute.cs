using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Attributes;

internal class NeedsAttribute(params Type[] values) : Attribute
{
    public Type[] Types { get; init; } = values;
}
