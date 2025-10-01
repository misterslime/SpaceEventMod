using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Components;

internal struct PhysicsJoints(IJoint[] joints) : IComponent
{
    private IJoint[] _joints = joints;

    public ReadOnlySpan<IJoint> Joints { get => _joints; }

    public IJoint GetJoint(int index) => _joints[index];
}
