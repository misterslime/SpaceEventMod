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
    Func<PhysicsPoint, SimulationContext, PhysicsPoint> Action
);

internal record struct SimulationContext(
    int Index,
    ReadOnlyDictionary<string, ParameterValue> GlobalData,
    PhysicsData LocalData
);

internal sealed class PhysicsSolver(Func<PhysicsPoint, PhysicsPoint> integrator, int timesConstrained = 8)
{
    private readonly Dictionary<string, ParameterValue> _globalData = new Dictionary<string, ParameterValue>{ ["timesConstrained"] = timesConstrained };
    private readonly List<PhysicsPassData> _preIntegrationPhysicsPasses = new List<PhysicsPassData>();
    private readonly List<PhysicsPassData> _postIntegrationPhysicsPasses = new List<PhysicsPassData>();
    private readonly Func<PhysicsPoint, PhysicsPoint> _integrator = integrator;

    public PhysicsSolver AddGlobalData(string key, ParameterValue value)
    {
        _globalData.Add(key, value);
        return this;
    }

    public PhysicsSolver AddGlobalData(params ReadOnlySpan<(string name, ParameterValue value)> values)
    {
        foreach (var value in values)
            _globalData.Add(value.name, value.value);

        return this;
    }

    public PhysicsSolver AddPhysicsPass(string name, bool preIntegration, Func<PhysicsPoint, SimulationContext, PhysicsPoint> action, int steps = 1)
    {
        if (preIntegration)
            _preIntegrationPhysicsPasses.Add(new PhysicsPassData(name, steps, action));
        else 
            _postIntegrationPhysicsPasses.Add(new PhysicsPassData(name, steps, action));

        return this;
    }

    public PhysicsSolver AddPhysicsPass(params ReadOnlySpan<(string Name, bool PreIntegration, int Steps, Func<PhysicsPoint, SimulationContext, PhysicsPoint> Action)> values)
    {
        foreach (var value in values)
            AddPhysicsPass(value.Name, value.PreIntegration, value.Action, value.Steps);

        return this;
    }

    public void RunSimulation(in PhysicsData physicsData)
    {
        SimulationContext context = new SimulationContext()
        {
            GlobalData = _globalData.AsReadOnly(),
            LocalData = physicsData
        };

        // run pre-integration physics passes
        RunPhysicsPasses(in physicsData, in _preIntegrationPhysicsPasses, context);

        // integrate motion
        for (int j = 0; j < physicsData.PointCount; j++)
            physicsData.SetPoint(j, _integrator(context.LocalData.Points[j]));

        // run post-integration physics passes
        RunPhysicsPasses(in physicsData, in _postIntegrationPhysicsPasses, context);

        // constrain linked points
        for (var i = 0; i < context.GlobalData["timesConstrained"].Int; i++)
        {
            foreach (var link in context.LocalData.Links)
                DistanceConstraint(in physicsData, in link);
        }
    }

    private void RunPhysicsPasses(in PhysicsData physicsData, in List<PhysicsPassData> physicsPasses, SimulationContext context)
    {
        if (physicsPasses.Count <= 0)
            return;

        foreach (var physicsPass in physicsPasses)
        {
            for (var i = 0; i < physicsPass.Steps; i++)
            {
                for (int j = 0; j < context.LocalData.PointCount; j++)
                {
                    context.Index = j;
                    physicsData.SetPoint(j, physicsPass.Action(context.LocalData.Points[j], context));
                }
            }
        }
    }

    private void DistanceConstraint(in PhysicsData physicsData, in ILink link)
    {
        var point1 = GetLinkPoint(in physicsData, in link, true);
        var point2 = GetLinkPoint(in physicsData, in link, false);

        if (point1.IsControl && !point2.IsControl)
        {
            var projection = (point2.Position - point1.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance;

            point2.Position = point1.Position + projection;

            SetLinkPoint(in physicsData, in link, false, point2);
        }
        else if (!point1.IsControl && point2.IsControl)
        {
            var projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance;

            point1.Position = point2.Position + projection;

            SetLinkPoint(in physicsData, in link, true, point1);
        }
        else if (!point1.IsControl && !point2.IsControl)
        {
            var midPoint = (point1.Position + point2.Position) * 0.5f;
            var projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance * 0.5f;

            point1.Position = midPoint + projection;
            point2.Position = midPoint - projection;

            SetLinkPoint(in physicsData, in link, true, point1);
            SetLinkPoint(in physicsData, in link, false, point2);
        }
        else
            throw new InvalidOperationException("Tried to constrain 2 control points together.");
    }


    private PhysicsPoint GetLinkPoint(in PhysicsData physicsData, in ILink link, bool isFirst)
    {
        dynamic pointIndex = link.GetPointIndex(isFirst);
        return physicsData.GetPoint(pointIndex);
    }

    private void SetLinkPoint(in PhysicsData physicsData, in ILink link, bool isFirst, PhysicsPoint point)
    {
        dynamic pointIndex = link.GetPointIndex(isFirst);

        // it should never be setting a control point
        if (pointIndex is string)
            return;

        physicsData.SetPoint((int)pointIndex, point);
    }
}
