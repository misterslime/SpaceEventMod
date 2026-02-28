using SpaceEventMod.Core.Physics.Interfaces;
using System;

namespace SpaceEventMod.Core.Physics.Components;

internal struct PhysicsJoints(IJoint[] joints) : IComponent
{
    private IJoint[] _joints = joints;

    public ReadOnlySpan<IJoint> Joints { get => _joints; }

    public IJoint GetJoint(int index) => _joints[index];
}
