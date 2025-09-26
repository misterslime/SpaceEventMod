using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

// help
internal interface ILink
{
    public float TargetDistance { get; }

    public dynamic GetPointIndex(bool isFirst);
}

/// <summary>
/// Link between 2 points in a <see cref="PhysicsSolver"/> simulation.
/// Stores the key or index of the point.
/// </summary>
/// <typeparam name="TFirst">Type of the first point.</typeparam>
/// <typeparam name="TSecond">Type of the second point.</typeparam>
internal struct PhysicsLink<TFirst, TSecond>(TFirst point1, TSecond point2, float targetDistance) : ILink
{
    private TFirst _point1Index = point1;
    private TSecond _point2Index = point2;

    public float TargetDistance { get; } = targetDistance;

    public dynamic GetPointIndex(bool isFirst)
    {
        return isFirst ? _point1Index : _point2Index;
    }
}
