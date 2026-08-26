using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Interfaces;

internal enum IndexType : byte
{
    Point,
    PointAverage,
    ObjectPosition,
    ChildPosition
}

internal struct JointIndex(IndexType indexType, int index, int childIndex = -1)
{
    public IndexType IndexType { get; set; } = indexType;
    public int Index { get; set; } = index;
}

internal interface IJoint
{
    public JointIndex GetPointIndex(bool isFirst);
}
