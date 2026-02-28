using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Components;

internal struct ParentObject(PhysicsObject parent) : IComponent
{
    public PhysicsObject Parent { get; init; } = parent;
}
