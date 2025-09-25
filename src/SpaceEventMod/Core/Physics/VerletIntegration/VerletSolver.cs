using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Physics.VerletIntegration;

internal class VerletSolver(Func<VerletPoint, VerletPoint> integrator) : PhysicsSolver<VerletPoint>(integrator)
{
    private readonly List<ILink> _links =  new List<ILink>();

    public VerletSolver AddLink(ILink link)
    {
        _links.Add(link);

        return this;
    }

    public VerletSolver AddLink<T, U>(T point1, U point2, float length)
    {
        _links.Add(new VerletLink<T, U>(point1, point2, length));

        return this;
    }

    public VerletSolver AddLinks(params ReadOnlySpan<ILink> values)
    {
        foreach (var value in values)
            _links.Add(value);

        return this;
    }

    public VerletSolver AddLinks<T, U>(params ReadOnlySpan<(T point1, U point2, float length)> values)
    {
        foreach (var value in values)
            _links.Add(new VerletLink<T, U>(value.point1, value.point2, value.length));

        return this;
    }

    public static VerletPoint VerletIntegration(VerletPoint point)
    {
        Vector2 previousPosition = point.PreviousPosition;

        point.PreviousPosition = point.Position;
        point.Position = 2 * point.Position - previousPosition + point.Acceleration;
        point.Acceleration = Vector2.Zero;

        return point;
    }

    protected override void PostUpdate(SimulationContext context)
    {
        foreach (var link in _links)
        {
            DistanceConstraint(in link);
        }
    }

    private void DistanceConstraint(in ILink link)
    {
        IPoint point1 = link.GetPoint(this, WhichPoint.First);
        IPoint point2 = link.GetPoint(this, WhichPoint.Second);

        if (point1 is ControlPoint controlPoint1 && point2 is VerletPoint verletPoint2)
        {
            Vector2 projection = (point2.Position - point1.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance;

            verletPoint2.Position = point1.Position + projection;

            link.SetPoint(this, verletPoint2, WhichPoint.Second);
        }
        else if (point2 is ControlPoint controlPoint2 && point1 is VerletPoint verletPoint1)
        {
            Vector2 projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance;

            verletPoint1.Position = point2.Position + projection;

            link.SetPoint(this, verletPoint1, WhichPoint.First);
        }
        else if (point1 is VerletPoint verletPoint1a && point2 is VerletPoint verletPoint2a)
        {
            Vector2 midPoint = (point1.Position + point2.Position) * 0.5f;
            Vector2 projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance * 0.5f;

            verletPoint1a.Position = midPoint + projection;
            verletPoint2a.Position = midPoint - projection;

            link.SetPoint(this, verletPoint1a, WhichPoint.First);
            link.SetPoint(this, verletPoint2a, WhichPoint.Second);
        }
    }
}
