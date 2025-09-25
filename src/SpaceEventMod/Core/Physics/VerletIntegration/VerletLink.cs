using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.VerletIntegration;

internal enum WhichPoint
{
    First,
    Second
}

// help
internal interface ILink
{
    public float TargetDistance { get; }

    public IPoint GetPoint(in PhysicsSolver<VerletPoint> simulation, WhichPoint which);

    public void SetPoint(in PhysicsSolver<VerletPoint> simulation, in VerletPoint point, WhichPoint which);
}

/// <summary>
/// Link between 2 points in a <see cref="PhysicsSolver"/> simulation.
/// Stores the key or index of the point.
/// </summary>
/// <typeparam name="T">Type of the first point.</typeparam>
/// <typeparam name="U">Type of the second point.</typeparam>
/// <exception cref="InvalidTypeParameterException">Thrown when T or U aren't either a string or an int.</exception>
internal class VerletLink<T, U>(T point1, U point2, float targetDistance) : ILink
{
    private T _point1Index = point1;
    private U _point2Index = point2;

    public float TargetDistance { get; } = targetDistance;

    public IPoint GetPoint(in PhysicsSolver<VerletPoint> simulation, WhichPoint which)
    {
        dynamic point1 = _point1Index;
        dynamic point2 = _point2Index;

        return which switch
        {
            WhichPoint.First => simulation.GetPoint(point1),
            WhichPoint.Second => simulation.GetPoint(point2),
            _ => throw new ArgumentOutOfRangeException(nameof(which), $"Not a valid expected value: {which}")
        };
    }

    public void SetPoint(in PhysicsSolver<VerletPoint> simulation, in VerletPoint point, WhichPoint which)
    {
        switch (which)
        {
            case WhichPoint.First:
                if (_point1Index is int p)
                    simulation.SetPoint(p, point);
                break;
            case WhichPoint.Second:
                if (_point2Index is int h)
                    simulation.SetPoint(h, point);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(which), $"Not an expected value: {which}");
        }
    }
}
