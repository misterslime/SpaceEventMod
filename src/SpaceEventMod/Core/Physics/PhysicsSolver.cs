using MonoMod.Utils;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Physics.VerletIntegration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Physics;

internal record struct PhysicsPassData<T>(
    string Name,
    int Steps,
    Func<T, int, SimulationContext, PhysicsSolver<T>, T> Action
) where T : struct, IPoint;

internal record struct GlobalData(string Name, ParameterValue Value);

internal record struct SimulationContext(
    ReadOnlyDictionary<string, ParameterValue> Globals,
    ReadOnlyDictionary<string, ControlPoint> ControlPoints
);

internal abstract class PhysicsSolver<T>(Func<T, T> integrator) where T : struct, IPoint
{
    private readonly Dictionary<string, ParameterValue> _globalData = new Dictionary<string, ParameterValue>();
    private readonly Dictionary<string, ControlPoint> _controlPoints = new Dictionary<string, ControlPoint>();
    private readonly List<T> _points = new List<T>();
    private readonly List<PhysicsPassData<T>> _physicsPasses = new List<PhysicsPassData<T>>();
    private readonly Func<T, T> _integrator = integrator;

    public IPoint GetPoint(string key) => _controlPoints[key];
    public IPoint GetPoint(int index) => _points[index];

    public void SetPoint(string key, in ControlPoint point) => _controlPoints[key] = point;
    public void SetPoint(int index, in T point) => _points[index] = point;

    public int Count { get => _points.Count; }

    public PhysicsSolver<T> AddGlobalData(string key, ParameterValue value)
    {
        _globalData.Add(key, value);

        return this;
    }

    public PhysicsSolver<T> AddGlobalData(params ReadOnlySpan<(string, ParameterValue)> values)
    {
        foreach (var value in values)
            _globalData.Add(value.Item1, value.Item2);

        return this;
    }

    public PhysicsSolver<T> AddControlPoint(string key, ControlPoint point)
    {
        _controlPoints.Add(key, point);

        return this;
    }

    public PhysicsSolver<T> AddControlPoints(params ReadOnlySpan<(string, ControlPoint)> points)
    {
        foreach (var point in points)
            _controlPoints.Add(point.Item1, point.Item2);

        return this;
    }

    public PhysicsSolver<T> AddPoints(params ReadOnlySpan<T> points)
    {
        _points.AddRange(points);
        return this;
    }

    public PhysicsSolver<T> AddPhysicsPass(string name, Func<T, int, SimulationContext, PhysicsSolver<T>, T> action, int steps = 1)
    {
        _physicsPasses.Add(new PhysicsPassData<T>(name, steps, action));

        return this;
    }

    public void RunSimulation()
    {
        SimulationContext context = new SimulationContext()
        {
            Globals = _globalData.AsReadOnly(),
            ControlPoints = _controlPoints.AsReadOnly()
        };

        if (_physicsPasses.Count > 0 && PreUpdate(context))
        {
            foreach (var physicsPass in _physicsPasses)
            {
                for (var i = 0; i < physicsPass.Steps; i++)
                {
                    for (int j = 0; j < _points.Count(); j++)
                        _points[j] = physicsPass.Action(_points[j], j, context, this);
                }
            }

            // integrate motion
            for (int j = 0; j < _points.Count(); j++)
                _points[j] = _integrator(_points[j]);
        }

        PostUpdate(context);
    }

    protected virtual bool PreUpdate(SimulationContext context) => true;

    protected virtual void PostUpdate(SimulationContext context) { }
}
