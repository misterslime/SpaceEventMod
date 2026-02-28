using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.NPCs.Attributes;

internal class StateProcessAttribute<T>(T state) : Attribute
{
    public T State { get; set; } = state;
}
