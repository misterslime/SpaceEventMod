using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Joints;

internal struct Anchor(JointIndex point1, JointIndex point2) : IJoint
{
    private JointIndex _point1Index = point1;
    private JointIndex _point2Index = point2;

    public JointIndex GetPointIndex(bool isFirst) => isFirst ? _point1Index : _point2Index;
}
