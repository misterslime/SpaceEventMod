using Microsoft.Xna.Framework;
using MonoMod.Utils;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Physics;

internal record struct PhysicsPassData(
    string Name,
    int Steps,
    Func<PhysicsPoint, int, SimulationContext, PhysicsPoint> Action
);

internal record struct SimulationContext(
    ReadOnlyDictionary<string, ParameterValue> Globals,
    ReadOnlyCollection<PhysicsPoint> Points,
    ReadOnlyCollection<ILink> Links
);

internal sealed class PhysicsSolver(Func<PhysicsPoint, PhysicsPoint> integrator, PhysicsPointType expectedType)
{
    private readonly Dictionary<string, ParameterValue> _globalData = new Dictionary<string, ParameterValue>();
    private readonly List<PhysicsPoint> _points = new List<PhysicsPoint>();
    private readonly List<ILink> _links = new List<ILink>();
    private readonly List<PhysicsPassData> _physicsPasses = new List<PhysicsPassData>();
    private readonly Func<PhysicsPoint, PhysicsPoint> _integrator = integrator;
    private readonly PhysicsPointType _expectedType = expectedType;

    public PhysicsPoint GetPoint(string key) => new PhysicsPoint(_globalData[key].Vector2);
    public PhysicsPoint GetPoint(int index) => _points[index];

    public void SetPoint(string key, in Vector2 point) => _globalData[key] = point;
    public void SetPoint(int index, in PhysicsPoint point) => _points[index] = point;

    public int Count { get => _points.Count; }

    public PhysicsSolver AddGlobalData(string key, ParameterValue value)
    {
        _globalData.Add(key, value);

        return this;
    }

    public PhysicsSolver AddGlobalData(params ReadOnlySpan<(string, ParameterValue)> values)
    {
        foreach (var value in values)
            _globalData.Add(value.Item1, value.Item2);

        return this;
    }
    
    public PhysicsSolver AddPoint(PhysicsPoint point)
    {
        if (point.Type != _expectedType)
            throw new InvalidOperationException("Tried to add a point of the wrong type to the simulation.");

        _points.Add(point);
        return this;
    }

    public PhysicsSolver AddLink<T, U>(T point1, U point2, float length)
    {
        _links.Add(new PhysicsLink<T, U>(point1, point2, length));

        return this;
    }

    public PhysicsSolver AddPhysicsPass(string name, Func<PhysicsPoint, int, SimulationContext, PhysicsPoint> action, int steps = 1)
    {
        _physicsPasses.Add(new PhysicsPassData(name, steps, action));

        return this;
    }

    public void RunSimulation()
    {
        SimulationContext context = new SimulationContext()
        {
            Globals = _globalData.AsReadOnly(),
            Points = _points.AsReadOnly()
        };

        // run physics passes
        if (_physicsPasses.Count > 0)
        {
            foreach (var physicsPass in _physicsPasses)
            {
                for (var i = 0; i < physicsPass.Steps; i++)
                {
                    for (int j = 0; j < _points.Count(); j++)
                        _points[j] = physicsPass.Action(_points[j], j, context);
                }
            }
        }

        // integrate motion
        for (int j = 0; j < _points.Count(); j++)
            _points[j] = _integrator(_points[j]);

        // constrain linked points
        for (var i = 0; i < 8; i++)
        {
            foreach (var link in _links)
                DistanceConstraint(in link);
        }
    }

    private void DistanceConstraint(in ILink link)
    {
        var point1 = GetLinkPoint(link, true);
        var point2 = GetLinkPoint(link, false);

        if (point1.Type == PhysicsPointType.Control)
        {
            var projection = (point2.Position - point1.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance;

            point2.Position = point1.Position + projection;

            SetLinkPoint(link, point2, false);
        }
        else if (point2.Type == PhysicsPointType.Control)
        {
            var projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance;

            point1.Position = point2.Position + projection;

            SetLinkPoint(link, point1, true);
        }
        else
        {
            var midPoint = (point1.Position + point2.Position) * 0.5f;
            var projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance * 0.5f;

            point1.Position = midPoint + projection;
            point2.Position = midPoint - projection;

            SetLinkPoint(link, point1, true);
            SetLinkPoint(link, point2, false);
        }
    }


    private PhysicsPoint GetLinkPoint(ILink link, bool isFirst)
    {
        dynamic pointIndex = link.GetPointIndex(isFirst);
        return this.GetPoint(pointIndex);
    }

    // point is a verlet point bc the physics system should never be setting a control point
    private void SetLinkPoint(ILink link, PhysicsPoint point, bool isFirst)
    {
        dynamic pointIndex = link.GetPointIndex(isFirst);

        // it should never be setting a control point
        if (pointIndex is string)
            return;

        var verletPointIndex = (int)pointIndex;

        SetPoint(verletPointIndex, point);
    }
}
