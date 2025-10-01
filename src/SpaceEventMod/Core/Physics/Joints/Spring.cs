using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Joints;

internal struct Spring(JointIndex point1, JointIndex point2, float targetDistance, bool biased = false) : IJoint
{
    private JointIndex _point1Index = point1;
    private JointIndex _point2Index = point2;

    public bool Biased { get; init; } = biased;
    public float TargetDistance { get; init; } = targetDistance;

    public JointIndex GetPointIndex(bool isFirst) => isFirst ? _point1Index : _point2Index;
}
