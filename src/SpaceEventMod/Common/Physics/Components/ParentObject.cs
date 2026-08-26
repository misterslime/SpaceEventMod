using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Components;

internal struct ParentObject(PhysicsObject parent) : IComponent
{
    public PhysicsObject Parent { get; init; } = parent;
}
