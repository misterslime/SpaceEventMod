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
}

/// <summary>
/// Link between 2 points in a <see cref="PhysicsSolver"/> simulation.
/// Stores the key or index of the point.
/// </summary>
internal struct PhysicsLink(int point1, int point2, float targetDistance) : ILink
{
    private int _point1Index = point1;
    private int _point2Index = point2;

    public float TargetDistance { get; } = targetDistance;

    public int GetPointIndex(bool isFirst)
    {
        return isFirst ? _point1Index : _point2Index;
    }
}

/// <summary>
/// Link between 2 points in a <see cref="PhysicsSolver"/> simulation.
/// Stores the key or index of the point.
/// </summary>
internal struct ControlledPhysicsLink(string controlPoint, int point, float targetDistance) : ILink
{
    private string _controlPointIndex = controlPoint;
    private int _pointIndex = point;

    public float TargetDistance { get; } = targetDistance;

    public dynamic GetPointIndex(bool isControl)
    {
        return isControl ? _controlPointIndex : _pointIndex;
    }
}
